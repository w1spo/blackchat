using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace BlackChat;

public class FirebaseService
{
    private readonly FirebaseClient _firebase;
    private readonly string _firebaseUrl = "https://blackcrypt-d68fe-default-rtdb.asia-southeast1.firebasedatabase.app/";
    private KeyManager? _keyManager;
    private EncryptionService? _encryption;
    private readonly PasswordService _password = new();
    private readonly Dictionary<string, byte[]> _groupKeyCache = new();
    private string _username = string.Empty;

    public FirebaseService()
    {
        _firebase = new FirebaseClient(_firebaseUrl);
    }

    public void SetUserContext(string username)
    {
        _username = username;
        _keyManager = new KeyManager(username);
        _encryption = new EncryptionService(_keyManager);
    }

    
    private async Task<byte[]?> GetUserEcdhPublicKey(string username)
    {
        try
        {
            var userData = await _firebase
                .Child("users")
                .Child(username)
                .OnceSingleAsync<Dictionary<string, object>>();

            if (userData != null && userData.TryGetValue("PublicKeyECDH", out var pkObj))
            {
                var pkBase64 = pkObj?.ToString();
                if (!string.IsNullOrEmpty(pkBase64))
                    return Convert.FromBase64String(pkBase64);
            }
            return null;
        }
        catch { return null; }
    }

    
    public async Task<bool> CreateUser(string username, string password)
    {
        var exists = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (exists != null && exists.Count > 0) return false;

        var (hash, salt) = _password.HashPassword(password);
        var user = new User
        {
            Username = username,
            PasswordHash = hash,
            Salt = salt,
            Friends = new List<string>(),
            Groups = new List<string>(),
            PublicKeyECDH = ""
        };

        await _firebase
            .Child("users")
            .Child(username)
            .PutAsync(JsonConvert.SerializeObject(user));

        return true;
    }

    public async Task UpdateUserEcdhPublicKey(string username, string publicKeyBase64)
    {
        var userData = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userData != null)
        {
            userData["PublicKeyECDH"] = publicKeyBase64;
            await _firebase
                .Child("users")
                .Child(username)
                .PutAsync(JsonConvert.SerializeObject(userData));
        }
    }

    public async Task<bool> LoginUser(string username, string password)
    {
        var userData = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userData == null || userData.Count == 0) return false;

        var user = JsonConvert.DeserializeObject<User>(
            JsonConvert.SerializeObject(userData)
        );

        if (user == null) return false;

        return _password.VerifyPassword(password, user.PasswordHash, user.Salt);
    }

    
    private async Task SendMessageInternal(
        string username,
        string text,
        string channelType,
        string? chatId = null,
        byte[]? groupKey = null,
        string? targetUser = null)
    {
        if (_encryption == null || _keyManager == null)
            throw new InvalidOperationException("User context not set.");

        string encryptedText, iv, tag;

        if (channelType == "public")
        {
            (encryptedText, iv, tag) = _encryption.EncryptPublic(text);
        }
        else if (channelType == "private" && targetUser != null)
        {
            var otherPub = await GetUserEcdhPublicKey(targetUser);
            if (otherPub == null)
                throw new Exception($"User {targetUser} has no ECDH public key.");
            (encryptedText, iv, tag) = _encryption.EncryptPrivate(text, otherPub);
        }
        else if (channelType == "group" && groupKey != null)
        {
            (encryptedText, iv, tag) = _encryption.EncryptGroup(text, groupKey);
        }
        else
        {
            throw new Exception("Invalid channel type or missing parameters.");
        }

        var timestamp = DateTime.UtcNow;
        var dataToSign = Encoding.UTF8.GetBytes(
            $"{username}|{timestamp.Ticks}|{encryptedText}|{iv}|{tag}");
        var signature = _keyManager.SignData(dataToSign);

        var message = new Message
        {
            Username = username,
            Text = encryptedText,
            IV = iv,
            Tag = tag,
            Signature = signature,
            Timestamp = timestamp
        };

        var serialized = JsonConvert.SerializeObject(message);

        if (channelType == "public")
        {
            await _firebase.Child("messages").Child("public").PostAsync(serialized);
        }
        else if (channelType == "private" && chatId != null)
        {
            await _firebase
                .Child("messages")
                .Child("private")
                .Child(chatId)
                .PostAsync(serialized);
        }
        else if (channelType == "group" && chatId != null)
        {
            await _firebase
                .Child("messages")
                .Child("groups")
                .Child(chatId)
                .PostAsync(serialized);
        }
    }

    public async Task SendPublicMessage(string username, string text)
        => await SendMessageInternal(username, text, "public");

    public async Task SendPrivateMessage(string fromUser, string toUser, string text, string chatId)
        => await SendMessageInternal(fromUser, text, "private", chatId, targetUser: toUser);

    public async Task SendGroupMessage(string username, string groupId, string text)
    {
        var groupKey = await GetGroupKey(groupId);
        if (groupKey == null) throw new Exception("Group key not available.");
        await SendMessageInternal(username, text, "group", groupId, groupKey);
    }

    public async Task SendSystemMessage(string groupId, string messageText)
    {
        var msg = new Message
        {
            Username = "SYSTEM",
            Text = messageText,
            IV = "",
            Tag = "",
            Signature = "",
            Timestamp = DateTime.UtcNow
        };

        await _firebase
            .Child("messages")
            .Child("groups")
            .Child(groupId)
            .PostAsync(JsonConvert.SerializeObject(msg));
    }

    
    private async Task<List<Message>> GetMessagesInternal(
        string path,
        bool isPublic,
        string? chatId = null,
        DateTime? since = null,
        int limit = 50)
    {
        var result = new List<Message>();

        try
        {
            
            var snapshot = await _firebase.Child(path).OnceAsync<Dictionary<string, object>>();

            var allMessages = new List<Message>();

            foreach (var item in snapshot)
            {
                try
                {
                    var msg = JsonConvert.DeserializeObject<Message>(
                        JsonConvert.SerializeObject(item.Object)
                    );

                    if (msg == null || string.IsNullOrEmpty(msg.Text)) continue;

                    msg.Id = item.Key;

                    if (msg.Username == "SYSTEM")
                    {
                        allMessages.Add(msg);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(msg.IV) && !string.IsNullOrEmpty(msg.Tag))
                    {
                        if (isPublic)
                        {
                            msg.Text = _encryption?.DecryptPublic(msg.Text, msg.IV, msg.Tag) ?? msg.Text;
                        }
                        else if (path.StartsWith("messages/private"))
                        {
                            var parts = chatId?.Split('_');
                            if (parts != null && parts.Length == 3)
                            {
                                var other = parts[1] == _username ? parts[2] : parts[1];
                                var otherPub = await GetUserEcdhPublicKey(other);
                                if (otherPub != null)
                                {
                                    msg.Text = _encryption?.DecryptPrivate(msg.Text, msg.IV, msg.Tag, otherPub) ?? msg.Text;
                                }
                            }
                        }
                        else if (path.StartsWith("messages/groups"))
                        {
                            var groupKey = await GetGroupKey(chatId ?? "");
                            if (groupKey != null)
                            {
                                msg.Text = _encryption?.DecryptGroup(msg.Text, msg.IV, msg.Tag, groupKey) ?? msg.Text;
                            }
                        }
                    }

                    allMessages.Add(msg);
                }
                catch {  }
            }

            
            if (since.HasValue)
            {
                allMessages = allMessages.Where(m => m.Timestamp > since.Value).ToList();
            }

            
            result = allMessages
                .OrderByDescending(m => m.Timestamp)
                .Take(limit)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"GetMessages error: {ex.Message}");
        }

        return result;
    }

    
    public async Task<List<Message>> GetLatestPublicMessages(int limit = 50)
        => await GetMessagesInternal("messages/public", true, limit: limit);

    public async Task<List<Message>> GetPublicMessagesSince(DateTime since, int limit = 50)
        => await GetMessagesInternal("messages/public", true, since: since, limit: limit);

    public async Task<List<Message>> GetLatestPrivateMessages(string chatId, int limit = 50)
        => await GetMessagesInternal($"messages/private/{chatId}", false, chatId, limit: limit);

    public async Task<List<Message>> GetPrivateMessagesSince(string chatId, DateTime since, int limit = 50)
        => await GetMessagesInternal($"messages/private/{chatId}", false, chatId, since, limit);

    public async Task<List<Message>> GetLatestGroupMessages(string groupId, int limit = 50)
        => await GetMessagesInternal($"messages/groups/{groupId}", false, groupId, limit: limit);

    public async Task<List<Message>> GetGroupMessagesSince(string groupId, DateTime since, int limit = 50)
        => await GetMessagesInternal($"messages/groups/{groupId}", false, groupId, since, limit);

    
    public async Task<byte[]?> GetGroupKey(string groupCode)
    {
        if (_groupKeyCache.TryGetValue(groupCode, out var cached))
            return cached;

        var groupData = await _firebase
            .Child("groups")
            .Child(groupCode)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (groupData == null) return null;

        var group = JsonConvert.DeserializeObject<Group>(
            JsonConvert.SerializeObject(groupData)
        );

        if (group == null || !group.Members.Contains(_username) || group.EncryptedGroupKeys == null)
            return null;

        if (group.EncryptedGroupKeys.TryGetValue(_username, out var encryptedKey))
        {
            if (_keyManager != null)
            {
                var key = _keyManager.DecryptDataWithPrivateKey(encryptedKey);
                _groupKeyCache[groupCode] = key;
                return key;
            }
        }

        return null;
    }

    
    public async Task<bool> CreateGroup(string username, string groupName, string groupCode)
    {
        var exists = await _firebase
            .Child("groups")
            .Child(groupCode)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (exists != null && exists.Count > 0) return false;

        var groupAesKey = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(groupAesKey);

        var creatorPublic = await GetUserEcdhPublicKey(username);
        if (creatorPublic == null) return false;

        var encryptedForCreator = _keyManager?.EncryptDataWithPublicKey(groupAesKey, creatorPublic);
        if (string.IsNullOrEmpty(encryptedForCreator)) return false;

        var group = new Group
        {
            GroupCode = groupCode,
            GroupName = groupName,
            CreatedBy = username,
            Members = new List<string> { username },
            CreatedAt = DateTime.UtcNow,
            EncryptedGroupKeys = new Dictionary<string, string>
            {
                { username, encryptedForCreator }
            }
        };

        await _firebase
            .Child("groups")
            .Child(groupCode)
            .PutAsync(JsonConvert.SerializeObject(group));

        await AddUserToGroup(username, groupCode);
        return true;
    }

    public async Task<bool> JoinGroup(string username, string groupCode)
    {
        var groupData = await _firebase
            .Child("groups")
            .Child(groupCode)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (groupData == null) return false;

        var group = JsonConvert.DeserializeObject<Group>(
            JsonConvert.SerializeObject(groupData)
        );

        if (group == null || group.Members.Contains(username)) return false;

        var userPublic = await GetUserEcdhPublicKey(username);
        if (userPublic == null) return false;

        var groupKey = await GetGroupKey(groupCode);
        if (groupKey == null) return false;

        var encryptedForNew = _keyManager?.EncryptDataWithPublicKey(groupKey, userPublic);
        if (string.IsNullOrEmpty(encryptedForNew)) return false;

        group.Members.Add(username);
        if (group.EncryptedGroupKeys == null)
            group.EncryptedGroupKeys = new Dictionary<string, string>();

        group.EncryptedGroupKeys[username] = encryptedForNew;

        await _firebase
            .Child("groups")
            .Child(groupCode)
            .PutAsync(JsonConvert.SerializeObject(group));

        await AddUserToGroup(username, groupCode);
        await SendSystemMessage(groupCode, $"{username} JOINED THE GROUP");
        return true;
    }

    public async Task<bool> LeaveGroup(string username, string groupCode)
    {
        var groupData = await _firebase
            .Child("groups")
            .Child(groupCode)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (groupData == null) return false;

        var group = JsonConvert.DeserializeObject<Group>(
            JsonConvert.SerializeObject(groupData)
        );

        if (group == null || !group.Members.Contains(username)) return false;

        group.Members.Remove(username);
        if (group.EncryptedGroupKeys != null && group.EncryptedGroupKeys.ContainsKey(username))
        {
            group.EncryptedGroupKeys.Remove(username);
        }

        await _firebase
            .Child("groups")
            .Child(groupCode)
            .PutAsync(JsonConvert.SerializeObject(group));

        await RemoveUserFromGroup(username, groupCode);
        await SendSystemMessage(groupCode, $"{username} LEFT THE GROUP");
        return true;
    }

    public async Task<bool> DeleteGroup(string groupCode)
    {
        try
        {
            await _firebase.Child("groups").Child(groupCode).DeleteAsync();
            await _firebase.Child("messages").Child("groups").Child(groupCode).DeleteAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    
    public async Task<Dictionary<string, string>> GetFriends(string username)
    {
        var userData = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userData == null || !userData.ContainsKey("Friends"))
            return new Dictionary<string, string>();

        var friendsList = JsonConvert.DeserializeObject<List<string>>(
            JsonConvert.SerializeObject(userData["Friends"])
        ) ?? new List<string>();

        var friends = new Dictionary<string, string>();
        foreach (var friend in friendsList)
            friends[friend] = friend;

        return friends;
    }

    public async Task<Dictionary<string, string>> GetUserGroups(string username)
    {
        try
        {
            var userData = await _firebase
                .Child("users")
                .Child(username)
                .OnceSingleAsync<Dictionary<string, object>>();

            if (userData == null || !userData.ContainsKey("Groups"))
                return new Dictionary<string, string>();

            var groupsList = JsonConvert.DeserializeObject<List<string>>(
                JsonConvert.SerializeObject(userData["Groups"])
            ) ?? new List<string>();

            var groups = new Dictionary<string, string>();

            foreach (var groupCode in groupsList)
            {
                var groupData = await _firebase
                    .Child("groups")
                    .Child(groupCode)
                    .OnceSingleAsync<Dictionary<string, object>>();

                if (groupData != null && groupData.Count > 0)
                {
                    var group = JsonConvert.DeserializeObject<Group>(
                        JsonConvert.SerializeObject(groupData)
                    );

                    if (group != null)
                        groups[group.GroupName] = group.GroupCode;
                }
            }

            return groups;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading groups: {ex.Message}");
        }
    }

    public async Task<Group?> GetGroupInfo(string groupCode)
    {
        try
        {
            var groupData = await _firebase
                .Child("groups")
                .Child(groupCode)
                .OnceSingleAsync<Dictionary<string, object>>();

            if (groupData == null || groupData.Count == 0)
                return null;

            return JsonConvert.DeserializeObject<Group>(
                JsonConvert.SerializeObject(groupData)
            );
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> AddFriend(string username, string friendUsername)
    {
        var friendData = await _firebase
            .Child("users")
            .Child(friendUsername)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (friendData == null || friendData.Count == 0)
            return false;

        var userData = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userData == null)
            return false;

        var user = JsonConvert.DeserializeObject<User>(
            JsonConvert.SerializeObject(userData)
        );

        if (user == null) return false;

        if (user.Friends == null)
            user.Friends = new List<string>();

        if (user.Friends.Contains(friendUsername))
            return false;

        user.Friends.Add(friendUsername);

        await _firebase
            .Child("users")
            .Child(username)
            .PutAsync(JsonConvert.SerializeObject(user));

        return true;
    }

    
    private async Task AddUserToGroup(string username, string groupCode)
    {
        var userData = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userData == null) return;

        var user = JsonConvert.DeserializeObject<User>(
            JsonConvert.SerializeObject(userData)
        );

        if (user == null) return;

        if (user.Groups == null)
            user.Groups = new List<string>();

        if (!user.Groups.Contains(groupCode))
        {
            user.Groups.Add(groupCode);
            await _firebase
                .Child("users")
                .Child(username)
                .PutAsync(JsonConvert.SerializeObject(user));
        }
    }

    private async Task RemoveUserFromGroup(string username, string groupCode)
    {
        var userData = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userData == null) return;

        var user = JsonConvert.DeserializeObject<User>(
            JsonConvert.SerializeObject(userData)
        );

        if (user == null) return;

        if (user.Groups != null && user.Groups.Contains(groupCode))
        {
            user.Groups.Remove(groupCode);
            await _firebase
                .Child("users")
                .Child(username)
                .PutAsync(JsonConvert.SerializeObject(user));
        }
    }
}