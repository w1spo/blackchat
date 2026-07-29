



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

    
    private readonly Dictionary<string, List<Message>> _messageCache = new();
    
    private readonly Dictionary<string, DateTime> _lastMessageTime = new();
    
    private readonly SemaphoreSlim _loadSemaphore = new SemaphoreSlim(1, 1);

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
        inviteCodeLabel.Click += InviteCodeLabel_Click;

        SetViewMode(ViewMode.Public);
        LoadFriends();
        LoadGroups();
        StartAutoRefresh();
        UpdateChatContext();
        InitializeNotifyIcon();
    }

    private void InitializeNotifyIcon()
    {
        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Items.Add("Open BlackChat", null, (s, e) =>
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.Activate();
        });
        menu.Items.Add("Exit", null, (s, e) => Application.Exit());
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
                nameOfChannel.Text = "# PUBLIC_CHAT";
                label1.Text = "> PUBLIC";
                _currentChatId = "public";
                _selectedFriend = "";
                _selectedGroup = "";
                chatBox.Clear();
                
                _ = LoadMessagesAsync("public", force: true);
                break;

            case ViewMode.Friends:
                nameOfChannel.Text = "# PRIVATE";
                label1.Text = "> CONTACTS";
                chatBox.Clear();
                contactsList.Items.Clear();
                foreach (var friend in _friends.Values)
                    contactsList.Items.Add(friend);
                if (contactsList.Items.Count > 0)
                    contactsList.SelectedIndex = 0;
                break;

            case ViewMode.Groups:
                nameOfChannel.Text = "# COLLECTIVE";
                label1.Text = "> GROUPS";
                chatBox.Clear();
                contactsList.Items.Clear();
                foreach (var groupName in _groups.Keys)
                    contactsList.Items.Add(groupName);
                if (contactsList.Items.Count > 0)
                    contactsList.SelectedIndex = 0;
                break;
        }

        UpdateChatContext();
        messageField.Clear();
        messageField.Focus();
        inviteCodeLabel.Visible = false;
    }

    private void PublicBtn_Click(object? sender, EventArgs e) => SetViewMode(ViewMode.Public);
    private void ChatsBtn_Click(object? sender, EventArgs e) => SetViewMode(ViewMode.Friends);
    private void GroupsBtn_Click(object? sender, EventArgs e) => SetViewMode(ViewMode.Groups);

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
            _currentChatId = chatId;
            inviteCodeLabel.Visible = false;
            chatBox.Clear();
            _ = LoadMessagesAsync(chatId, force: true);
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
                _currentChatId = groupCode;
                chatBox.Clear();
                _ = LoadMessagesAsync(groupCode, force: true);
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
            _currentChatId = "public";
        else if (_currentMode == ViewMode.Friends && !string.IsNullOrWhiteSpace(_selectedFriend))
            _currentChatId = GetPrivateChatId(_username, _selectedFriend);
        else if (_currentMode == ViewMode.Groups && !string.IsNullOrWhiteSpace(_selectedGroup))
            _currentChatId = _selectedGroup;
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

    private void SendBtn_Click(object? sender, EventArgs e) => SendMessageBasedOnMode();

    private async void SendMessageBasedOnMode()
    {
        string text = messageField.Text.Trim();
        if (string.IsNullOrWhiteSpace(text)) return;

        if (_currentMode == ViewMode.Public)
            await SendPublicMessage(text);
        else if (_currentMode == ViewMode.Friends)
        {
            if (!string.IsNullOrWhiteSpace(_selectedFriend))
                await SendPrivateMessage(text);
            else
                MessageBox.Show("Please select a friend to chat with!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else if (_currentMode == ViewMode.Groups)
        {
            if (!string.IsNullOrWhiteSpace(_selectedGroup))
                await SendGroupMessage(text);
            else
                MessageBox.Show("Please select or join a group!", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private async Task SendPublicMessage(string text)
    {
        messageField.Clear();
        sendBtn.Enabled = false;
        try
        {
            await _firebaseService.SendPublicMessage(_username, text);
            
            if (_currentMode == ViewMode.Public)
                await LoadMessagesAsync("public", force: true);
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
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(_selectedFriend)) return;

        messageField.Clear();
        sendBtn.Enabled = false;
        try
        {
            string chatId = GetPrivateChatId(_username, _selectedFriend);
            await _firebaseService.SendPrivateMessage(_username, _selectedFriend, text, chatId);
            if (_currentMode == ViewMode.Friends && _selectedFriend == contactsList.SelectedItem?.ToString())
                await LoadMessagesAsync(chatId, force: true);
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
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(_selectedGroup)) return;

        messageField.Clear();
        sendBtn.Enabled = false;
        try
        {
            await _firebaseService.SendGroupMessage(_username, _selectedGroup, text);
            if (_currentMode == ViewMode.Groups && _selectedGroup == _currentChatId)
                await LoadMessagesAsync(_selectedGroup, force: true);
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

    
    
    
    private async Task LoadMessagesAsync(string channelId, bool force = false)
    {
        if (_isClosing) return;

        
        if (!await _loadSemaphore.WaitAsync(0)) return;

        try
        {
            
            if (!IsCurrentChannel(channelId)) return;

            List<Message> newMessages = new();
            DateTime lastTime = DateTime.MinValue;

            
            lock (_messageCache)
            {
                if (_lastMessageTime.TryGetValue(channelId, out var cachedTime))
                    lastTime = cachedTime;
            }

            if (force || lastTime == DateTime.MinValue)
            {
                
                if (channelId == "public")
                    newMessages = await _firebaseService.GetLatestPublicMessages(50);
                else if (channelId.StartsWith("private_"))
                    newMessages = await _firebaseService.GetLatestPrivateMessages(channelId, 50);
                else
                    newMessages = await _firebaseService.GetLatestGroupMessages(channelId, 50);

                
                lock (_messageCache)
                {
                    _messageCache[channelId] = newMessages.OrderBy(m => m.Timestamp).ToList();
                    if (newMessages.Any())
                        _lastMessageTime[channelId] = newMessages.Max(m => m.Timestamp);
                }

                
                if (IsCurrentChannel(channelId))
                    DisplayAllMessages(channelId);
            }
            else
            {
                
                if (channelId == "public")
                    newMessages = await _firebaseService.GetPublicMessagesSince(lastTime, 50);
                else if (channelId.StartsWith("private_"))
                    newMessages = await _firebaseService.GetPrivateMessagesSince(channelId, lastTime, 50);
                else
                    newMessages = await _firebaseService.GetGroupMessagesSince(channelId, lastTime, 50);

                
                lock (_messageCache)
                {
                    if (_messageCache.TryGetValue(channelId, out var cached))
                    {
                        var existingIds = new HashSet<string>(cached.Select(m => m.Id));
                        var uniqueNew = newMessages.Where(m => !existingIds.Contains(m.Id)).ToList();

                        if (uniqueNew.Any())
                        {
                            cached.AddRange(uniqueNew);
                            cached = cached.OrderBy(m => m.Timestamp).ToList();
                            _messageCache[channelId] = cached;
                            _lastMessageTime[channelId] = cached.Max(m => m.Timestamp);

                            
                            if (IsCurrentChannel(channelId))
                                AppendMessages(uniqueNew);
                        }
                    }
                    else
                    {
                        
                        _messageCache[channelId] = newMessages.OrderBy(m => m.Timestamp).ToList();
                        if (newMessages.Any())
                            _lastMessageTime[channelId] = newMessages.Max(m => m.Timestamp);
                        if (IsCurrentChannel(channelId))
                            DisplayAllMessages(channelId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"LoadMessages error: {ex.Message}");
        }
        finally
        {
            _loadSemaphore.Release();
        }
    }

    
    private void DisplayAllMessages(string channelId)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(() => DisplayAllMessages(channelId)));
            return;
        }

        if (!IsCurrentChannel(channelId)) return;

        List<Message> messages;
        lock (_messageCache)
        {
            if (!_messageCache.TryGetValue(channelId, out messages)) return;
        }

        chatBox.SuspendLayout();
        chatBox.Clear();

        foreach (var msg in messages.OrderBy(m => m.Timestamp))
        {
            string displayText = FormatMessage(msg);
            chatBox.AppendText(displayText);
        }

        chatBox.ResumeLayout();
        chatBox.SelectionStart = chatBox.Text.Length;
        chatBox.ScrollToCaret();
    }

    
    private void AppendMessages(List<Message> newMessages)
    {
        if (this.InvokeRequired)
        {
            this.Invoke(new Action(() => AppendMessages(newMessages)));
            return;
        }

        if (!IsCurrentChannel(_currentChatId)) return;

        chatBox.SuspendLayout();

        foreach (var msg in newMessages.OrderBy(m => m.Timestamp))
        {
            string displayText = FormatMessage(msg);
            chatBox.AppendText(displayText);
        }

        chatBox.ResumeLayout();
        chatBox.SelectionStart = chatBox.Text.Length;
        chatBox.ScrollToCaret();
    }

    private string FormatMessage(Message msg)
    {
        if (msg.Username == "SYSTEM")
            return $"🔔 {msg.Text}\n\n";
        else
            return $"[{msg.Timestamp:HH:mm}] {msg.Username}:\n{msg.Text}\n\n";
    }

    private bool IsCurrentChannel(string channelId)
    {
        if (channelId == "public" && _currentMode == ViewMode.Public)
            return true;

        if (_currentMode == ViewMode.Friends && !string.IsNullOrWhiteSpace(_selectedFriend))
        {
            string currentPrivateId = GetPrivateChatId(_username, _selectedFriend);
            if (channelId == currentPrivateId) return true;
        }

        if (_currentMode == ViewMode.Groups && !string.IsNullOrWhiteSpace(_selectedGroup))
        {
            if (channelId == _selectedGroup) return true;
        }

        return false;
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
                    contactsList.Items.Add(friend);
                if (contactsList.Items.Count > 0 && contactsList.SelectedIndex == -1)
                    contactsList.SelectedIndex = 0;
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
                    contactsList.Items.Add(groupName);
                if (contactsList.Items.Count > 0 && contactsList.SelectedIndex == -1)
                    contactsList.SelectedIndex = 0;
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

        if (string.IsNullOrWhiteSpace(friendUsername)) return;
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

        if (string.IsNullOrWhiteSpace(groupCode)) return;

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
                _ = LoadMessagesAsync(groupCode, force: true);
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

        if (string.IsNullOrWhiteSpace(groupName)) return;

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
                _ = LoadMessagesAsync(groupCode, force: true);
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
        => System.Text.RegularExpressions.Regex.IsMatch(code, @"^G-[A-Z0-9]{3}-[A-Z0-9]{4}$");

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
        refreshTimer.Interval = 2000;
        refreshTimer.Tick += async (s, e) =>
        {
            if (_isClosing) return;

            try
            {
                if (_currentMode == ViewMode.Public)
                    await LoadMessagesAsync("public");
                else if (_currentMode == ViewMode.Friends && !string.IsNullOrWhiteSpace(_selectedFriend))
                {
                    string chatId = GetPrivateChatId(_username, _selectedFriend);
                    await LoadMessagesAsync(chatId);
                }
                else if (_currentMode == ViewMode.Groups && !string.IsNullOrWhiteSpace(_selectedGroup))
                {
                    await LoadMessagesAsync(_selectedGroup);
                }
            }
            catch {  }
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

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        this.Show();
        this.WindowState = FormWindowState.Normal;
        this.Activate();
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

    private void Form1_Load(object sender, EventArgs e) { }
}