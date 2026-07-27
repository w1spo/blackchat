//Original by h1ghwaay
//Remarked By szaman251.

namespace BlackChat;

public partial class Form1 : Form
{
    private readonly FirebaseService _firebaseService;
    public readonly string _username;
    private string _currentChatId = "public";
    private string _selectedFriend = "";
    private string _selectedGroup = "";
    private Dictionary<string, string> _friends = new();
    private Dictionary<string, string> _groups = new();
    private bool _isClosing = false;
    private System.Windows.Forms.Timer refreshTimer = new();

    private enum ViewMode
    {
        Public,
        Friends,
        Groups
    }
    private ViewMode _currentMode = ViewMode.Public;

    public Form1(string username)
    {
        _username = username;
        _firebaseService = new FirebaseService();
        _firebaseService.SetUserContext(username);
        InitializeComponent();

        this.Text = $"BlackChat - {_username}";

        publicBtn.Click += PublicBtn_Click;
        chatsBtn.Click += ChatsBtn_Click;
        groupsBtn.Click += GroupsBtn_Click;
        addFriendBtn.Click += AddFriendBtn_Click;
        createGroupBtn.Click += CreateGroupBtn_Click;
        joinGroupBtn.Click += JoinGroupBtn_Click;
        sendBtn.Click += SendBtn_Click;
        contactsList.SelectedIndexChanged += ContactsList_SelectedIndexChanged;
        contactsList.MouseClick += ContactsList_MouseClick;
        messageField.KeyDown += MessageField_KeyDown;
        inviteCodeLabel.Visible = false;
        SetViewMode(ViewMode.Public);
        LoadFriends();
        LoadGroups();
        LoadPublicMessages();
        StartAutoRefresh();
        UpdateChatContext();
    }

    private void ContactsList_MouseClick(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right && _currentMode == ViewMode.Groups)
        {
            int index = contactsList.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                contactsList.SelectedIndex = index;
                ShowGroupContextMenu();
            }
        }
    }

    private async void ShowGroupContextMenu()
    {
        if (contactsList.SelectedItem == null) return;

        string groupName = contactsList.SelectedItem.ToString();
        if (!_groups.TryGetValue(groupName, out string? groupCode)) return;

        var group = await _firebaseService.GetGroupInfo(groupCode);
        if (group == null) return;

        bool isOwner = group.CreatedBy == _username;

        if (isOwner)
        {
            // Właściciel - pokaż opcję usunięcia
            DialogResult result = MessageBox.Show(
                $"Are you sure you want to delete group '{groupName}'?\n\nThis action is IRREVERSIBLE!\nAll messages will be permanently lost.",
                "Delete Group",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                string confirm = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Type REMOVE to delete group '{groupName}':",
                    "Confirm Deletion",
                    ""
                );

                if (confirm == "REMOVE")
                {
                    bool success = await _firebaseService.DeleteGroup(groupCode);
                    if (success)
                    {
                        MessageBox.Show("Group deleted successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadGroups();
                        SetViewMode(ViewMode.Public);
                    }
                    else
                    {
                        MessageBox.Show("Error deleting group!", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        else
        {
            // Członek - pokaż opcję opuszczenia
            DialogResult result = MessageBox.Show(
                $"Do you want to leave group '{groupName}'?",
                "Leave Group",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                bool success = await _firebaseService.LeaveGroup(_username, groupCode);
                if (success)
                {
                    MessageBox.Show($"You left group '{groupName}'!", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadGroups();
                    SetViewMode(ViewMode.Public);
                }
                else
                {
                    MessageBox.Show("Error leaving group!", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private void SetViewMode(ViewMode mode)
    {
        _currentMode = mode;
        contactsList.Items.Clear();

        switch (mode)
        {
            case ViewMode.Public:
                nameOfChannel.Text = "Public Chat";
                label1.Text = "Public Chat";
                _currentChatId = "public";
                _selectedFriend = "";
                _selectedGroup = "";
                LoadPublicMessages();
                break;

            case ViewMode.Friends:
                nameOfChannel.Text = "Chats";
                label1.Text = "Chats";
                contactsList.Items.Clear();
                chatBox.Clear();
                foreach (var friend in _friends.Values)
                {
                    contactsList.Items.Add(friend);
                }
                if (contactsList.Items.Count > 0)
                {
                    contactsList.SelectedIndex = 0;
                }
                break;

            case ViewMode.Groups:
                nameOfChannel.Text = "Groups";
                chatBox.Clear();
                label1.Text = "Groups";
                contactsList.Items.Clear();
                foreach (var groupName in _groups.Keys)
                {
                    contactsList.Items.Add(groupName);
                }
                if (contactsList.Items.Count > 0)
                {
                    contactsList.SelectedIndex = 0;
                }
                break;
        }
    }

    private void PublicBtn_Click(object? sender, EventArgs e)
    {
        SetViewMode(ViewMode.Public);
        UpdateChatContext();
        messageField.Clear();
        messageField.Focus();
        inviteCodeLabel.Visible = false;
    }

    private void ChatsBtn_Click(object? sender, EventArgs e)
    {
        SetViewMode(ViewMode.Friends);
        UpdateChatContext();
        messageField.Clear();
        messageField.Focus();
        inviteCodeLabel.Visible = false;
        chatBox.Clear();
    }

    private void GroupsBtn_Click(object? sender, EventArgs e)
    {
        SetViewMode(ViewMode.Groups);
        UpdateChatContext();
        messageField.Clear();
        messageField.Focus();
        inviteCodeLabel.Visible = false;
        chatBox.Clear();
    }

    private void ContactsList_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (contactsList.SelectedItem == null) return;

        string selected = contactsList.SelectedItem.ToString();

        if (_currentMode == ViewMode.Friends)
        {
            _selectedFriend = selected;
            _selectedGroup = "";
            string chatId = GetPrivateChatId(_username, _selectedFriend);
            nameOfChannel.Text = $"💬 {_selectedFriend}";
            LoadPrivateMessages(chatId);
            inviteCodeLabel.Visible = false;
            _currentChatId = chatId;
        }
        else if (_currentMode == ViewMode.Groups)
        {
            if (_groups.TryGetValue(selected, out string? groupCode))
            {
                _selectedGroup = groupCode;
                _selectedFriend = "";
                nameOfChannel.Text = $"📁 {selected}";
                inviteCodeLabel.Text = $"Invite Code: {groupCode}";
                inviteCodeLabel.Visible = true;
                LoadGroupMessages(groupCode);
                _currentChatId = groupCode;
            }
        }

        UpdateChatContext();
        messageField.Focus();
    }

    private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _isClosing = true;
        refreshTimer.Stop();
        refreshTimer.Dispose();
        Application.Exit();
    }

    private void UpdateChatContext()
    {
        if (_currentMode == ViewMode.Public)
        {
            _currentChatId = "public";
        }
        else if (_currentMode == ViewMode.Friends && !string.IsNullOrWhiteSpace(_selectedFriend))
        {
            _currentChatId = GetPrivateChatId(_username, _selectedFriend);
        }
        else if (_currentMode == ViewMode.Groups && !string.IsNullOrWhiteSpace(_selectedGroup))
        {
            _currentChatId = _selectedGroup;
        }
    }

    private void MessageField_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !e.Control)
        {
            e.Handled = true;
            e.SuppressKeyPress = true;
            SendMessageBasedOnMode();
        }
    }

    private void SendBtn_Click(object? sender, EventArgs e)
    {
        SendMessageBasedOnMode();
    }

    private async void SendMessageBasedOnMode()
    {
        string text = messageField.Text.Trim();
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (_currentMode == ViewMode.Public)
        {
            await SendPublicMessage(text);
        }
        else if (_currentMode == ViewMode.Friends)
        {
            if (!string.IsNullOrWhiteSpace(_selectedFriend))
            {
                await SendPrivateMessage(text);
            }
            else
            {
                MessageBox.Show("Please select a friend to chat with!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        else if (_currentMode == ViewMode.Groups)
        {
            if (!string.IsNullOrWhiteSpace(_selectedGroup))
            {
                await SendGroupMessage(text);
            }
            else
            {
                MessageBox.Show("Please select or join a group!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private async Task SendPublicMessage(string text)
    {
        messageField.Clear();
        sendBtn.Enabled = false;

        try
        {
            await _firebaseService.SendPublicMessage(_username, text);
            await LoadPublicMessages();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error sending message: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            sendBtn.Enabled = true;
            messageField.Focus();
        }
    }

    private async Task SendPrivateMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(_selectedFriend))
            return;

        messageField.Clear();
        sendBtn.Enabled = false;

        try
        {
            string chatId = GetPrivateChatId(_username, _selectedFriend);
            await _firebaseService.SendPrivateMessage(_username, _selectedFriend, text, chatId);
            await LoadPrivateMessages(chatId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error sending private message: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            sendBtn.Enabled = true;
            messageField.Focus();
        }
    }

    private async Task SendGroupMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(_selectedGroup))
            return;

        messageField.Clear();
        sendBtn.Enabled = false;

        try
        {
            await _firebaseService.SendGroupMessage(_username, _selectedGroup, text);
            await LoadGroupMessages(_selectedGroup);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error sending group message: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            sendBtn.Enabled = true;
            messageField.Focus();
        }
    }

    private string GetPrivateChatId(string user1, string user2)
    {
        var users = new List<string> { user1, user2 };
        users.Sort();
        return $"private_{users[0]}_{users[1]}";
    }

    private async Task LoadPublicMessages()
    {
        try
        {
            var messages = await _firebaseService.GetPublicMessages();
            chatBox.Clear();

            foreach (var msg in messages.OrderBy(m => m.Timestamp))
            {
                string displayText;
                if (msg.Username == "SYSTEM")
                {
                    displayText = $"🔔 {msg.Text}\n\n";
                }
                else
                {
                    displayText = $"[{msg.Timestamp:HH:mm}] {msg.Username}:\n{msg.Text}\n\n";
                }
                chatBox.AppendText(displayText);
                chatBox.SelectionStart = chatBox.Text.Length;
                chatBox.ScrollToCaret();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading messages: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadPrivateMessages(string chatId)
    {
        try
        {
            var messages = await _firebaseService.GetPrivateMessages(chatId);
            chatBox.Clear();

            foreach (var msg in messages.OrderBy(m => m.Timestamp))
            {
                string displayText = $"[{msg.Timestamp:HH:mm}] {msg.Username}:\n{msg.Text}\n\n";
                chatBox.AppendText(displayText);
                chatBox.SelectionStart = chatBox.Text.Length;
                chatBox.ScrollToCaret();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading private messages: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadGroupMessages(string groupId)
    {
        try
        {
            var messages = await _firebaseService.GetGroupMessages(groupId);
            chatBox.Clear();

            foreach (var msg in messages.OrderBy(m => m.Timestamp))
            {
                string displayText;
                if (msg.Username == "SYSTEM")
                {
                    displayText = $"🔔 {msg.Text}\n\n";
                }
                else
                {
                    displayText = $"[{msg.Timestamp:HH:mm}] {msg.Username}:\n{msg.Text}\n\n";
                }
                chatBox.AppendText(displayText);
                chatBox.SelectionStart = chatBox.Text.Length;
                chatBox.ScrollToCaret();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading group messages: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void LoadMessages()
    {
        await LoadPublicMessages();
    }

    private async void LoadFriends()
    {
        try
        {
            _friends = await _firebaseService.GetFriends(_username);
            if (_currentMode == ViewMode.Friends)
            {
                contactsList.Items.Clear();
                foreach (var friend in _friends.Values)
                {
                    contactsList.Items.Add(friend);
                }
                if (contactsList.Items.Count > 0)
                {
                    contactsList.SelectedIndex = 0;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading friends: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void LoadGroups()
    {
        try
        {
            _groups = await _firebaseService.GetUserGroups(_username);
            if (_currentMode == ViewMode.Groups)
            {
                contactsList.Items.Clear();
                foreach (var groupName in _groups.Keys)
                {
                    contactsList.Items.Add(groupName);
                }
                if (contactsList.Items.Count > 0)
                {
                    contactsList.SelectedIndex = 0;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading groups: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void AddFriendBtn_Click(object? sender, EventArgs e)
    {
        string friendUsername = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter friend's username:",
            "Add Friend",
            ""
        );

        if (string.IsNullOrWhiteSpace(friendUsername))
            return;

        if (friendUsername == _username)
        {
            MessageBox.Show("You cannot add yourself as a friend!", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            bool success = await _firebaseService.AddFriend(_username, friendUsername);
            if (success)
            {
                MessageBox.Show($"Friend '{friendUsername}' added successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadFriends();
            }
            else
            {
                MessageBox.Show("User not found or already your friend!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding friend: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void JoinGroupBtn_Click(object? sender, EventArgs e)
    {
        string groupCode = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter group code (format: G-XXX-XXXX):",
            "Join Group",
            ""
        );

        if (string.IsNullOrWhiteSpace(groupCode))
            return;

        groupCode = groupCode.ToUpper().Trim();

        if (!IsValidGroupCode(groupCode))
        {
            MessageBox.Show("Invalid group code format! Use: G-XXX-XXXX", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            bool success = await _firebaseService.JoinGroup(_username, groupCode);
            if (success)
            {
                MessageBox.Show($"Joined group '{groupCode}' successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGroups();
                _selectedGroup = groupCode;
                await LoadGroupMessages(groupCode);
                SetViewMode(ViewMode.Groups);
                UpdateChatContext();
            }
            else
            {
                MessageBox.Show("Group not found or you're already a member!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error joining group: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void CreateGroupBtn_Click(object? sender, EventArgs e)
    {
        string groupName = Microsoft.VisualBasic.Interaction.InputBox(
            "Enter group name:",
            "Create Group",
            ""
        );

        if (string.IsNullOrWhiteSpace(groupName))
            return;

        try
        {
            string groupCode = GenerateGroupCode();
            bool success = await _firebaseService.CreateGroup(_username, groupName, groupCode);
            if (success)
            {
                MessageBox.Show($"Group created successfully!\nGroup Code: {groupCode}\nShare this code with friends!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadGroups();
                _selectedGroup = groupCode;
                await LoadGroupMessages(groupCode);
                SetViewMode(ViewMode.Groups);
                UpdateChatContext();
            }
            else
            {
                MessageBox.Show("Error creating group!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating group: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool IsValidGroupCode(string code)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(code, @"^G-[A-Z0-9]{3}-[A-Z0-9]{4}$");
    }

    private string GenerateGroupCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var part1 = new string(Enumerable.Repeat(chars, 3).Select(s => s[random.Next(s.Length)]).ToArray());
        var part2 = new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
        return $"G-{part1}-{part2}";
    }

    private void StartAutoRefresh()
    {
        refreshTimer.Interval = 3000;
        refreshTimer.Tick += async (s, e) =>
        {
            if (_isClosing) return;

            try
            {
                if (_currentMode == ViewMode.Public)
                {
                    var messages = await _firebaseService.GetPublicMessages();
                    var lastMessage = messages.OrderByDescending(m => m.Timestamp).FirstOrDefault();
                    if (lastMessage != null && lastMessage.Timestamp > DateTime.UtcNow.AddSeconds(-5))
                    {
                        await LoadPublicMessages();
                    }
                }
                else if (_currentMode == ViewMode.Friends && !string.IsNullOrWhiteSpace(_selectedFriend))
                {
                    string chatId = GetPrivateChatId(_username, _selectedFriend);
                    var messages = await _firebaseService.GetPrivateMessages(chatId);
                    var lastMessage = messages.OrderByDescending(m => m.Timestamp).FirstOrDefault();
                    if (lastMessage != null && lastMessage.Timestamp > DateTime.UtcNow.AddSeconds(-5))
                    {
                        await LoadPrivateMessages(chatId);
                    }
                }
                else if (_currentMode == ViewMode.Groups && !string.IsNullOrWhiteSpace(_selectedGroup))
                {
                    var messages = await _firebaseService.GetGroupMessages(_selectedGroup);
                    var lastMessage = messages.OrderByDescending(m => m.Timestamp).FirstOrDefault();
                    if (lastMessage != null && lastMessage.Timestamp > DateTime.UtcNow.AddSeconds(-5))
                    {
                        await LoadGroupMessages(_selectedGroup);
                    }
                }
            }
            catch { }
        };
        refreshTimer.Start();


    }

    private void ContactsList_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Right && _currentMode == ViewMode.Groups)
        {
            int index = contactsList.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                contactsList.SelectedIndex = index;
                ShowGroupContextMenu();
            }
        }
    }

    private void InviteCodeLabel_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(inviteCodeLabel.Text))
        {
            string code = inviteCodeLabel.Text.Replace("Invite Code: ", "");
            Clipboard.SetText(code);
            MessageBox.Show($"Invite code '{code}' copied to clipboard!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

}