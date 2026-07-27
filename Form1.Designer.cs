namespace BlackChat
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            panel1 = new Panel();
            createGroupBtn = new PictureBox();
            joinGroupBtn = new PictureBox();
            addFriendBtn = new PictureBox();
            publicBtn = new PictureBox();
            groupsBtn = new PictureBox();
            chatsBtn = new PictureBox();
            logoPic = new PictureBox();
            panel2 = new Panel();
            contactsList = new ListBox();
            label1 = new Label();
            panel3 = new Panel();
            chatBox = new RichTextBox();
            panel5 = new Panel();
            messageField = new RichTextBox();
            sendBtn = new PictureBox();
            panel4 = new Panel();
            inviteCodeLabel = new Label();
            nameOfChannel = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)createGroupBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)joinGroupBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)addFriendBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)publicBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupsBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chatsBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)logoPic).BeginInit();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sendBtn).BeginInit();
            panel4.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Controls.Add(createGroupBtn);
            panel1.Controls.Add(joinGroupBtn);
            panel1.Controls.Add(addFriendBtn);
            panel1.Controls.Add(publicBtn);
            panel1.Controls.Add(groupsBtn);
            panel1.Controls.Add(chatsBtn);
            panel1.Controls.Add(logoPic);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(65, 700);
            panel1.TabIndex = 0;
            // 
            // createGroupBtn
            // 
            createGroupBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            createGroupBtn.Image = (Image)resources.GetObject("createGroupBtn.Image");
            createGroupBtn.Location = new Point(12, 594);
            createGroupBtn.Margin = new Padding(10);
            createGroupBtn.Name = "createGroupBtn";
            createGroupBtn.Size = new Size(44, 44);
            createGroupBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            createGroupBtn.TabIndex = 6;
            createGroupBtn.TabStop = false;
            // 
            // joinGroupBtn
            // 
            joinGroupBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            joinGroupBtn.Image = (Image)resources.GetObject("joinGroupBtn.Image");
            joinGroupBtn.Location = new Point(12, 644);
            joinGroupBtn.Margin = new Padding(10);
            joinGroupBtn.Name = "joinGroupBtn";
            joinGroupBtn.Size = new Size(44, 44);
            joinGroupBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            joinGroupBtn.TabIndex = 5;
            joinGroupBtn.TabStop = false;
            // 
            // addFriendBtn
            // 
            addFriendBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            addFriendBtn.Image = (Image)resources.GetObject("addFriendBtn.Image");
            addFriendBtn.Location = new Point(12, 544);
            addFriendBtn.Margin = new Padding(10);
            addFriendBtn.Name = "addFriendBtn";
            addFriendBtn.Size = new Size(44, 44);
            addFriendBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            addFriendBtn.TabIndex = 4;
            addFriendBtn.TabStop = false;
            // 
            // publicBtn
            // 
            publicBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            publicBtn.Image = (Image)resources.GetObject("publicBtn.Image");
            publicBtn.Location = new Point(12, 197);
            publicBtn.Margin = new Padding(10);
            publicBtn.Name = "publicBtn";
            publicBtn.Size = new Size(44, 44);
            publicBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            publicBtn.TabIndex = 3;
            publicBtn.TabStop = false;
            // 
            // groupsBtn
            // 
            groupsBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            groupsBtn.Image = (Image)resources.GetObject("groupsBtn.Image");
            groupsBtn.Location = new Point(12, 147);
            groupsBtn.Margin = new Padding(10);
            groupsBtn.Name = "groupsBtn";
            groupsBtn.Size = new Size(44, 44);
            groupsBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            groupsBtn.TabIndex = 2;
            groupsBtn.TabStop = false;
            // 
            // chatsBtn
            // 
            chatsBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chatsBtn.Image = (Image)resources.GetObject("chatsBtn.Image");
            chatsBtn.Location = new Point(12, 97);
            chatsBtn.Margin = new Padding(10);
            chatsBtn.Name = "chatsBtn";
            chatsBtn.Size = new Size(44, 44);
            chatsBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            chatsBtn.TabIndex = 1;
            chatsBtn.TabStop = false;
            // 
            // logoPic
            // 
            logoPic.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            logoPic.Image = (Image)resources.GetObject("logoPic.Image");
            logoPic.Location = new Point(12, 12);
            logoPic.Margin = new Padding(10);
            logoPic.Name = "logoPic";
            logoPic.Size = new Size(44, 44);
            logoPic.SizeMode = PictureBoxSizeMode.AutoSize;
            logoPic.TabIndex = 0;
            logoPic.TabStop = false;
            // 
            // panel2
            // 
            panel2.BackColor = Color.FromArgb(35, 35, 45);
            panel2.Controls.Add(contactsList);
            panel2.Controls.Add(label1);
            panel2.Dock = DockStyle.Left;
            panel2.Location = new Point(65, 0);
            panel2.Margin = new Padding(0);
            panel2.Name = "panel2";
            panel2.Size = new Size(245, 700);
            panel2.TabIndex = 1;
            // 
            // contactsList
            // 
            contactsList.BackColor = Color.FromArgb(35, 35, 45);
            contactsList.BorderStyle = BorderStyle.None;
            contactsList.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            contactsList.ForeColor = Color.White;
            contactsList.FormattingEnabled = true;
            contactsList.Location = new Point(16, 68);
            contactsList.Name = "contactsList";
            contactsList.Size = new Size(216, 609);
            contactsList.TabIndex = 7;
            contactsList.MouseClick += ContactsList_MouseClick;
            contactsList.MouseDown += ContactsList_MouseDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.White;
            label1.Location = new Point(11, 9);
            label1.Name = "label1";
            label1.Size = new Size(73, 30);
            label1.TabIndex = 0;
            label1.Text = "Chats:";
            // 
            // panel3
            // 
            panel3.Controls.Add(chatBox);
            panel3.Controls.Add(panel5);
            panel3.Controls.Add(panel4);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(310, 0);
            panel3.Margin = new Padding(0);
            panel3.Name = "panel3";
            panel3.Size = new Size(690, 700);
            panel3.TabIndex = 2;
            // 
            // chatBox
            // 
            chatBox.BackColor = Color.FromArgb(35, 35, 40);
            chatBox.BorderStyle = BorderStyle.None;
            chatBox.Dock = DockStyle.Fill;
            chatBox.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            chatBox.ForeColor = Color.White;
            chatBox.Location = new Point(0, 56);
            chatBox.Name = "chatBox";
            chatBox.ReadOnly = true;
            chatBox.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
            chatBox.ShortcutsEnabled = false;
            chatBox.Size = new Size(690, 550);
            chatBox.TabIndex = 2;
            chatBox.Text = "";
            // 
            // panel5
            // 
            panel5.BackColor = Color.FromArgb(35, 35, 45);
            panel5.Controls.Add(messageField);
            panel5.Controls.Add(sendBtn);
            panel5.Dock = DockStyle.Bottom;
            panel5.Location = new Point(0, 606);
            panel5.Name = "panel5";
            panel5.Size = new Size(690, 94);
            panel5.TabIndex = 1;
            // 
            // messageField
            // 
            messageField.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            messageField.BackColor = Color.FromArgb(55, 55, 64);
            messageField.BorderStyle = BorderStyle.None;
            messageField.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            messageField.ForeColor = Color.White;
            messageField.Location = new Point(17, 27);
            messageField.Name = "messageField";
            messageField.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
            messageField.Size = new Size(589, 44);
            messageField.TabIndex = 3;
            messageField.Text = "";
            // 
            // sendBtn
            // 
            sendBtn.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            sendBtn.Image = (Image)resources.GetObject("sendBtn.Image");
            sendBtn.Location = new Point(625, 27);
            sendBtn.Name = "sendBtn";
            sendBtn.Size = new Size(44, 44);
            sendBtn.TabIndex = 7;
            sendBtn.TabStop = false;
            // 
            // panel4
            // 
            panel4.BackColor = Color.FromArgb(35, 35, 45);
            panel4.Controls.Add(inviteCodeLabel);
            panel4.Controls.Add(nameOfChannel);
            panel4.Dock = DockStyle.Top;
            panel4.Location = new Point(0, 0);
            panel4.Name = "panel4";
            panel4.Size = new Size(690, 56);
            panel4.TabIndex = 0;
            // 
            // inviteCodeLabel
            // 
            inviteCodeLabel.AutoSize = true;
            inviteCodeLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            inviteCodeLabel.ForeColor = Color.White;
            inviteCodeLabel.Location = new Point(17, 33);
            inviteCodeLabel.Name = "inviteCodeLabel";
            inviteCodeLabel.Size = new Size(166, 17);
            inviteCodeLabel.TabIndex = 1;
            inviteCodeLabel.Text = "Invite code: G-XXX-XXXX";
            inviteCodeLabel.Click += InviteCodeLabel_Click;
            // 
            // nameOfChannel
            // 
            nameOfChannel.AutoSize = true;
            nameOfChannel.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            nameOfChannel.ForeColor = Color.White;
            nameOfChannel.Location = new Point(17, 6);
            nameOfChannel.Name = "nameOfChannel";
            nameOfChannel.Size = new Size(121, 21);
            nameOfChannel.TabIndex = 0;
            nameOfChannel.Text = "<groupname>";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(35, 35, 40);
            ClientSize = new Size(1000, 700);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1000, 700);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "BlackChat";
            FormClosing += Form1_FormClosing;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)createGroupBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)joinGroupBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)addFriendBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)publicBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupsBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)chatsBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)logoPic).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)sendBtn).EndInit();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel2;
        private PictureBox createGroupBtn;
        private PictureBox joinGroupBtn;
        private PictureBox addFriendBtn;
        private PictureBox publicBtn;
        private PictureBox groupsBtn;
        private PictureBox chatsBtn;
        private PictureBox logoPic;
        private Label label1;
        private Panel panel3;
        private PictureBox sendBtn;
        private ListBox contactsList;
        private RichTextBox chatBox;
        private Panel panel5;
        private Panel panel4;
        private Label nameOfChannel;
        private RichTextBox messageField;
        private Label inviteCodeLabel;
    }
}