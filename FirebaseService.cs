using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using System.Text;

namespace BlackChat;

public class FirebaseService
{
    private readonly FirebaseClient _firebase;
    private readonly string _firebaseUrl = "https://NullControl-d75f4-default-rtdb.europe-west1.firebasedatabase.app/";
    private KeyManager? _keyManager;
    private EncryptionService? _encryption;
    private readonly PasswordService _password = new();
    private readonly Dictionary<string, string> _publicKeyCache = new();

    public FirebaseService()
    {
        _firebase = new FirebaseClient(_firebaseUrl);
    }

    public void SetUserContext(string username)
    {
        _keyManager = new KeyManager(username);
        var aesKey = _keyManager.GetOrCreateAesKey();
        _encryption = new EncryptionService(aesKey);
    }

    private async Task<string?> GetUserPublicKey(string username)
    {
        if (_publicKeyCache.TryGetValue(username, out var cached))
            return cached;

        try
        {
            var userData = await _firebase
                .Child("users")
                .Child(username)
                .OnceSingleAsync<Dictionary<string, object>>();

            if (userData != null && userData.TryGetValue("PublicKey", out var pkObj))
            {
                var pk = pkObj?.ToString();
                if (!string.IsNullOrEmpty(pk))
                {
                    _publicKeyCache[username] = pk;
                    return pk;
                }
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> CreateUser(string username, string password)
    {
        var userExists = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userExists != null && userExists.Count > 0)
            return false;

        var (hash, salt) = _password.HashPassword(password);

        var user = new User
        {
            Username = username,
            PasswordHash = hash,
            Salt = salt,
            Friends = new List<string>(),
            Groups = new List<string>(),
            PublicKey = "" // zostanie ustawione później
        };

        await _firebase
            .Child("users")
            .Child(username)
            .PutAsync(JsonConvert.SerializeObject(user));

        return true;
    }

    public async Task UpdateUserPublicKey(string username, string publicKeyBase64)
    {
        var userData = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userData != null)
        {
            userData["PublicKey"] = publicKeyBase64;
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

        if (userData == null || userData.Count == 0)
            return false;

        var user = JsonConvert.DeserializeObject<User>(
            JsonConvert.SerializeObject(userData)
        );

        if (user == null)
            return false;

        return _password.VerifyPassword(password, user.PasswordHash, user.Salt);
    }

    private async Task SendMessageInternal(string username, string text, string channelType, string? chatId = null)
    {
        if (_encryption == null || _keyManager == null)
            throw new InvalidOperationException("User context not set.");

        var (encryptedText, iv, tag) = _encryption.Encrypt(text);

        // Dane do podpisu: username|timestamp|encryptedText|iv|tag
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
            await _firebase.Child("messages").Child("private").Child(chatId).PostAsync(serialized);
        }
        else if (channelType == "group" && chatId != null)
        {
            await _firebase.Child("messages").Child("groups").Child(chatId).PostAsync(serialized);
        }
    }

    public async Task SendPublicMessage(string username, string text)
    {
        await SendMessageInternal(username, text, "public");
    }

    public async Task SendPrivateMessage(string fromUser, string toUser, string text, string chatId)
    {
        await SendMessageInternal(fromUser, text, "private", chatId);
    }

    public async Task SendGroupMessage(string username, string groupId, string text)
    {
        await SendMessageInternal(username, text, "group", groupId);
    }

    public async Task SendSystemMessage(string groupId, string messageText)
    {
        var message = new Message
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
            .PostAsync(JsonConvert.SerializeObject(message));
    }

    private async Task<List<Message>> GetMessagesInternal(string path, bool verifySignature)
    {
        var result = new List<Message>();
        var snapshot = await _firebase.Child(path).OnceAsync<Dictionary<string, object>>();

        foreach (var msgObj in snapshot)
        {
            try
            {
                var messageData = JsonConvert.DeserializeObject<Message>(
                    JsonConvert.SerializeObject(msgObj.Object)
                );

                if (messageData == null || string.IsNullOrEmpty(messageData.Text))
                    continue;

                if (messageData.Username == "SYSTEM")
                {
                    result.Add(messageData);
                    continue;
                }

                if (verifySignature && !string.IsNullOrEmpty(messageData.Signature))
                {
                    var publicKey = await GetUserPublicKey(messageData.Username);
                    if (string.IsNullOrEmpty(publicKey))
                        continue; // brak klucza – pomiń

                    var dataToVerify = Encoding.UTF8.GetBytes(
                        $"{messageData.Username}|{messageData.Timestamp.Ticks}|{messageData.Text}|{messageData.IV}|{messageData.Tag}");
                    var isValid = KeyManager.VerifySignature(
                        dataToVerify,
                        messageData.Signature,
                        publicKey);

                    if (!isValid)
                        continue; // pomiń nieautoryzowane
                }

                // Odszyfruj
                if (!string.IsNullOrEmpty(messageData.IV) && !string.IsNullOrEmpty(messageData.Tag))
                {
                    if (_encryption != null)
                    {
                        messageData.Text = _encryption.Decrypt(
                            messageData.Text,
                            messageData.IV,
                            messageData.Tag);
                    }
                }

                result.Add(messageData);
            }
            catch
            {
                // Pomiń uszkodzone wiadomości
            }
        }

        return result;
    }

    public async Task<List<Message>> GetPublicMessages()
    {
        return await GetMessagesInternal("messages/public", true);
    }

    public async Task<List<Message>> GetPrivateMessages(string chatId)
    {
        return await GetMessagesInternal($"messages/private/{chatId}", true);
    }

    public async Task<List<Message>> GetGroupMessages(string groupId)
    {
        return await GetMessagesInternal($"messages/groups/{groupId}", true);
    }

    // Pozostałe metody (GetFriends, GetUserGroups, GetGroupInfo, AddFriend, CreateGroup, JoinGroup, LeaveGroup, DeleteGroup, AddUserToGroup, RemoveUserFromGroup) pozostają bez zmian
    // ale wymagają dostosowania – w tym miejscu pomijam dla zwięzłości, ale w rzeczywistym kodzie muszą być przepisane tak samo jak w oryginale, jedynie bez zmian w logice.
    // Poniżej skrótowo zamieszczam je, aby zachować kompletność.

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

        if (user == null)
            return false;

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

    public async Task<bool> CreateGroup(string username, string groupName, string groupCode)
    {
        var groupExists = await _firebase
            .Child("groups")
            .Child(groupCode)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (groupExists != null && groupExists.Count > 0)
            return false;

        var group = new Group
        {
            GroupCode = groupCode,
            GroupName = groupName,
            CreatedBy = username,
            Members = new List<string> { username },
            CreatedAt = DateTime.UtcNow
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

        if (groupData == null || groupData.Count == 0)
            return false;

        var group = JsonConvert.DeserializeObject<Group>(
            JsonConvert.SerializeObject(groupData)
        );

        if (group == null)
            return false;

        if (group.Members.Contains(username))
            return false;

        group.Members.Add(username);

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

        if (groupData == null || groupData.Count == 0)
            return false;

        var group = JsonConvert.DeserializeObject<Group>(
            JsonConvert.SerializeObject(groupData)
        );

        if (group == null)
            return false;

        if (!group.Members.Contains(username))
            return false;

        group.Members.Remove(username);

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

    private async Task AddUserToGroup(string username, string groupCode)
    {
        var userData = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userData == null)
            return;

        var user = JsonConvert.DeserializeObject<User>(
            JsonConvert.SerializeObject(userData)
        );

        if (user == null)
            return;

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

        if (userData == null)
            return;

        var user = JsonConvert.DeserializeObject<User>(
            JsonConvert.SerializeObject(userData)
        );

        if (user == null)
            return;

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