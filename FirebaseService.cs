using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;
using BlackChat;

namespace BlackChat;

public class FirebaseService
{
    private readonly FirebaseClient _firebase;
    private readonly EncryptionService _encryption;
    private readonly PasswordService _password;
    private readonly string _firebaseUrl = "https://NullControl-d75f4-default-rtdb.europe-west1.firebasedatabase.app/";

    public FirebaseService()
    {
        _firebase = new FirebaseClient(_firebaseUrl);
        _encryption = new EncryptionService();
        _password = new PasswordService();
    }

    public async Task<bool> CreateUser(string username, string password)
    {
        var userExists = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userExists != null && userExists.Count > 0)
        {
            return false;
        }

        var (hash, salt) = _password.HashPassword(password);

        var user = new User
        {
            Username = username,
            PasswordHash = hash,
            Salt = salt,
            Friends = new List<string>(),
            Groups = new List<string>()
        };

        await _firebase
            .Child("users")
            .Child(username)
            .PutAsync(JsonConvert.SerializeObject(user));

        return true;
    }

    public async Task<bool> LoginUser(string username, string password)
    {
        var userData = await _firebase
            .Child("users")
            .Child(username)
            .OnceSingleAsync<Dictionary<string, object>>();

        if (userData == null || userData.Count == 0)
        {
            return false;
        }

        var user = JsonConvert.DeserializeObject<User>(
            JsonConvert.SerializeObject(userData)
        );

        if (user == null)
        {
            return false;
        }

        return _password.VerifyPassword(password, user.PasswordHash, user.Salt);
    }

    public async Task SendPublicMessage(string username, string text)
    {
        var (encryptedText, iv) = _encryption.Encrypt(text);

        var message = new Message
        {
            Username = username,
            Text = encryptedText,
            IV = Convert.ToBase64String(iv),
            Timestamp = DateTime.UtcNow
        };

        await _firebase
            .Child("messages")
            .Child("public")
            .PostAsync(JsonConvert.SerializeObject(message));
    }

    public async Task SendPrivateMessage(string fromUser, string toUser, string text, string chatId)
    {
        var (encryptedText, iv) = _encryption.Encrypt(text);

        var message = new Message
        {
            Username = fromUser,
            Text = encryptedText,
            IV = Convert.ToBase64String(iv),
            Timestamp = DateTime.UtcNow
        };

        await _firebase
            .Child("messages")
            .Child("private")
            .Child(chatId)
            .PostAsync(JsonConvert.SerializeObject(message));
    }

    public async Task SendGroupMessage(string username, string groupId, string text)
    {
        var (encryptedText, iv) = _encryption.Encrypt(text);

        var message = new Message
        {
            Username = username,
            Text = encryptedText,
            IV = Convert.ToBase64String(iv),
            Timestamp = DateTime.UtcNow
        };

        await _firebase
            .Child("messages")
            .Child("groups")
            .Child(groupId)
            .PostAsync(JsonConvert.SerializeObject(message));
    }

    public async Task SendSystemMessage(string groupId, string messageText)
    {
        var message = new Message
        {
            Username = "SYSTEM",
            Text = messageText,
            IV = "",
            Timestamp = DateTime.UtcNow
        };

        await _firebase
            .Child("messages")
            .Child("groups")
            .Child(groupId)
            .PostAsync(JsonConvert.SerializeObject(message));
    }

    public async Task<List<Message>> GetPublicMessages()
    {
        var messages = await _firebase
            .Child("messages")
            .Child("public")
            .OnceAsync<Dictionary<string, object>>();

        var result = new List<Message>();

        foreach (var msg in messages)
        {
            try
            {
                var messageData = JsonConvert.DeserializeObject<Message>(
                    JsonConvert.SerializeObject(msg.Object)
                );

                if (messageData != null && !string.IsNullOrEmpty(messageData.Text))
                {
                    if (messageData.Username == "SYSTEM")
                    {
                        result.Add(messageData);
                    }
                    else
                    {
                        var ivBytes = Convert.FromBase64String(messageData.IV);
                        messageData.Text = _encryption.Decrypt(messageData.Text, ivBytes);
                        result.Add(messageData);
                    }
                }
            }
            catch { }
        }

        return result;
    }

    public async Task<List<Message>> GetPrivateMessages(string chatId)
    {
        var messages = await _firebase
            .Child("messages")
            .Child("private")
            .Child(chatId)
            .OnceAsync<Dictionary<string, object>>();

        var result = new List<Message>();

        foreach (var msg in messages)
        {
            try
            {
                var messageData = JsonConvert.DeserializeObject<Message>(
                    JsonConvert.SerializeObject(msg.Object)
                );

                if (messageData != null && !string.IsNullOrEmpty(messageData.Text))
                {
                    var ivBytes = Convert.FromBase64String(messageData.IV);
                    messageData.Text = _encryption.Decrypt(messageData.Text, ivBytes);
                    result.Add(messageData);
                }
            }
            catch { }
        }

        return result;
    }

    public async Task<List<Message>> GetGroupMessages(string groupId)
    {
        var messages = await _firebase
            .Child("messages")
            .Child("groups")
            .Child(groupId)
            .OnceAsync<Dictionary<string, object>>();

        var result = new List<Message>();

        foreach (var msg in messages)
        {
            try
            {
                var messageData = JsonConvert.DeserializeObject<Message>(
                    JsonConvert.SerializeObject(msg.Object)
                );

                if (messageData != null && !string.IsNullOrEmpty(messageData.Text))
                {
                    if (messageData.Username == "SYSTEM")
                    {
                        result.Add(messageData);
                    }
                    else
                    {
                        var ivBytes = Convert.FromBase64String(messageData.IV);
                        messageData.Text = _encryption.Decrypt(messageData.Text, ivBytes);
                        result.Add(messageData);
                    }
                }
            }
            catch { }
        }

        return result;
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
        {
            friends[friend] = friend;
        }

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
                    {
                        groups[group.GroupName] = group.GroupCode;
                    }
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

        // Wyślij wiadomość systemową
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

        // Wyślij wiadomość systemową
        await SendSystemMessage(groupCode, $"{username} LEFT THE GROUP");

        return true;
    }

    public async Task<bool> DeleteGroup(string groupCode)
    {
        try
        {
            // Usuń grupę
            await _firebase
                .Child("groups")
                .Child(groupCode)
                .DeleteAsync();

            // Usuń wiadomości grupy
            await _firebase
                .Child("messages")
                .Child("groups")
                .Child(groupCode)
                .DeleteAsync();

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