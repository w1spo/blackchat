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
            logoPic = new PictureBox();
            chatsBtn = new PictureBox();
            groupsBtn = new PictureBox();
            publicBtn = new PictureBox();
            addFriendBtn = new PictureBox();
            createGroupBtn = new PictureBox();
            joinGroupBtn = new PictureBox();
            panel2 = new Panel();
            label1 = new Label();
            contactsList = new ListBox();
            panel3 = new Panel();
            chatBox = new RichTextBox();
            panel5 = new Panel();
            messageField = new RichTextBox();
            sendBtn = new PictureBox();
            panel4 = new Panel();
            nameOfChannel = new Label();
            inviteCodeLabel = new Label();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)logoPic).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chatsBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)groupsBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)publicBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)addFriendBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)createGroupBtn).BeginInit();
            ((System.ComponentModel.ISupportInitialize)joinGroupBtn).BeginInit();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)sendBtn).BeginInit();
            panel4.SuspendLayout();
            SuspendLayout();
            
            
            
            panel1.BackColor = Color.FromArgb(8, 0, 12);
            panel1.Controls.Add(logoPic);
            panel1.Controls.Add(chatsBtn);
            panel1.Controls.Add(groupsBtn);
            panel1.Controls.Add(publicBtn);
            panel1.Controls.Add(addFriendBtn);
            panel1.Controls.Add(createGroupBtn);
            panel1.Controls.Add(joinGroupBtn);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(0);
            panel1.Name = "panel1";
            panel1.Size = new Size(70, 700);
            panel1.TabIndex = 2;
            
            
            
            logoPic.BackColor = Color.Transparent;
            logoPic.Image = (Image)resources.GetObject("logoPic.Image");
            logoPic.Location = new Point(13, 12);
            logoPic.Name = "logoPic";
            logoPic.Size = new Size(44, 44);
            logoPic.SizeMode = PictureBoxSizeMode.AutoSize;
            logoPic.TabIndex = 0;
            logoPic.TabStop = false;
            
            
            
            chatsBtn.BackColor = Color.Transparent;
            chatsBtn.Image = (Image)resources.GetObject("chatsBtn.Image");
            chatsBtn.Location = new Point(13, 80);
            chatsBtn.Name = "chatsBtn";
            chatsBtn.Size = new Size(44, 44);
            chatsBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            chatsBtn.TabIndex = 1;
            chatsBtn.TabStop = false;
            chatsBtn.Click += ChatsBtn_Click;
            
            
            
            groupsBtn.BackColor = Color.Transparent;
            groupsBtn.Image = (Image)resources.GetObject("groupsBtn.Image");
            groupsBtn.Location = new Point(13, 140);
            groupsBtn.Name = "groupsBtn";
            groupsBtn.Size = new Size(44, 44);
            groupsBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            groupsBtn.TabIndex = 2;
            groupsBtn.TabStop = false;
            groupsBtn.Click += GroupsBtn_Click;
            
            
            
            publicBtn.BackColor = Color.Transparent;
            publicBtn.Image = (Image)resources.GetObject("publicBtn.Image");
            publicBtn.Location = new Point(13, 200);
            publicBtn.Name = "publicBtn";
            publicBtn.Size = new Size(44, 44);
            publicBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            publicBtn.TabIndex = 3;
            publicBtn.TabStop = false;
            publicBtn.Click += PublicBtn_Click;
            
            
            
            addFriendBtn.BackColor = Color.Transparent;
            addFriendBtn.Image = (Image)resources.GetObject("addFriendBtn.Image");
            addFriendBtn.Location = new Point(13, 540);
            addFriendBtn.Name = "addFriendBtn";
            addFriendBtn.Size = new Size(44, 44);
            addFriendBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            addFriendBtn.TabIndex = 4;
            addFriendBtn.TabStop = false;
            addFriendBtn.Click += AddFriendBtn_Click;
            
            
            
            createGroupBtn.BackColor = Color.Transparent;
            createGroupBtn.Image = (Image)resources.GetObject("createGroupBtn.Image");
            createGroupBtn.Location = new Point(13, 590);
            createGroupBtn.Name = "createGroupBtn";
            createGroupBtn.Size = new Size(44, 44);
            createGroupBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            createGroupBtn.TabIndex = 5;
            createGroupBtn.TabStop = false;
            createGroupBtn.Click += CreateGroupBtn_Click;
            
            
            
            joinGroupBtn.BackColor = Color.Transparent;
            joinGroupBtn.Image = (Image)resources.GetObject("joinGroupBtn.Image");
            joinGroupBtn.Location = new Point(13, 640);
            joinGroupBtn.Name = "joinGroupBtn";
            joinGroupBtn.Size = new Size(44, 44);
            joinGroupBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            joinGroupBtn.TabIndex = 6;
            joinGroupBtn.TabStop = false;
            joinGroupBtn.Click += JoinGroupBtn_Click;
            
            
            
            panel2.BackColor = Color.FromArgb(14, 0, 20);
            panel2.Controls.Add(label1);
            panel2.Controls.Add(contactsList);
            panel2.Dock = DockStyle.Left;
            panel2.Location = new Point(70, 0);
            panel2.Name = "panel2";
            panel2.Padding = new Padding(10);
            panel2.Size = new Size(240, 700);
            panel2.TabIndex = 1;
            
            
            
            label1.Font = new Font("Consolas", 14F, FontStyle.Bold);
            label1.ForeColor = Color.FromArgb(255, 0, 136);
            label1.Location = new Point(10, 8);
            label1.Name = "label1";
            label1.Size = new Size(200, 30);
            label1.TabIndex = 0;
            label1.Text = "> CONTACTS";
            label1.TextAlign = ContentAlignment.MiddleLeft;
            
            
            
            contactsList.BackColor = Color.FromArgb(18, 0, 28);
            contactsList.BorderStyle = BorderStyle.None;
            contactsList.Font = new Font("Consolas", 11F);
            contactsList.ForeColor = Color.FromArgb(220, 180, 210);
            contactsList.Location = new Point(10, 45);
            contactsList.Name = "contactsList";
            contactsList.Size = new Size(215, 594);
            contactsList.TabIndex = 1;
            contactsList.MouseClick += ContactsList_MouseClick;
            contactsList.SelectedIndexChanged += ContactsList_SelectedIndexChanged;
            contactsList.MouseDown += ContactsList_MouseDown;
            
            
            
            panel3.BackColor = Color.FromArgb(6, 0, 10);
            panel3.Controls.Add(chatBox);
            panel3.Controls.Add(panel5);
            panel3.Controls.Add(panel4);
            panel3.Dock = DockStyle.Fill;
            panel3.Location = new Point(310, 0);
            panel3.Name = "panel3";
            panel3.Size = new Size(690, 700);
            panel3.TabIndex = 0;
            
            
            
            chatBox.BackColor = Color.FromArgb(6, 0, 10);
            chatBox.BorderStyle = BorderStyle.None;
            chatBox.Dock = DockStyle.Fill;
            chatBox.Font = new Font("Consolas", 10F);
            chatBox.ForeColor = Color.FromArgb(210, 190, 220);
            chatBox.Location = new Point(0, 56);
            chatBox.Name = "chatBox";
            chatBox.ReadOnly = true;
            chatBox.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
            chatBox.ShortcutsEnabled = false;
            chatBox.Size = new Size(690, 564);
            chatBox.TabIndex = 0;
            chatBox.Text = "";
            
            
            
            panel5.BackColor = Color.FromArgb(10, 0, 16);
            panel5.Controls.Add(messageField);
            panel5.Controls.Add(sendBtn);
            panel5.Dock = DockStyle.Bottom;
            panel5.Location = new Point(0, 620);
            panel5.Name = "panel5";
            panel5.Padding = new Padding(12);
            panel5.Size = new Size(690, 80);
            panel5.TabIndex = 1;
            
            
            
            messageField.BackColor = Color.FromArgb(18, 0, 30);
            messageField.BorderStyle = BorderStyle.None;
            messageField.Font = new Font("Consolas", 11F);
            messageField.ForeColor = Color.FromArgb(230, 210, 240);
            messageField.Location = new Point(12, 12);
            messageField.Name = "messageField";
            messageField.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
            messageField.Size = new Size(550, 56);
            messageField.TabIndex = 0;
            messageField.Text = "";
            messageField.KeyDown += MessageField_KeyDown;
            
            
            
            sendBtn.BackColor = Color.Transparent;
            sendBtn.Image = (Image)resources.GetObject("sendBtn.Image");
            sendBtn.Location = new Point(575, 18);
            sendBtn.Name = "sendBtn";
            sendBtn.Size = new Size(44, 44);
            sendBtn.SizeMode = PictureBoxSizeMode.AutoSize;
            sendBtn.TabIndex = 1;
            sendBtn.TabStop = false;
            sendBtn.Click += SendBtn_Click;
            
            
            
            panel4.BackColor = Color.FromArgb(10, 0, 16);
            panel4.Controls.Add(nameOfChannel);
            panel4.Controls.Add(inviteCodeLabel);
            panel4.Dock = DockStyle.Top;
            panel4.Location = new Point(0, 0);
            panel4.Name = "panel4";
            panel4.Padding = new Padding(12, 6, 12, 6);
            panel4.Size = new Size(690, 56);
            panel4.TabIndex = 2;
            
            
            
            nameOfChannel.Font = new Font("Consolas", 14F, FontStyle.Bold);
            nameOfChannel.ForeColor = Color.FromArgb(255, 0, 136);
            nameOfChannel.Location = new Point(12, 6);
            nameOfChannel.Name = "nameOfChannel";
            nameOfChannel.Size = new Size(300, 30);
            nameOfChannel.TabIndex = 0;
            nameOfChannel.Text = "# PUBLIC";
            nameOfChannel.TextAlign = ContentAlignment.MiddleLeft;
            
            
            
            inviteCodeLabel.Font = new Font("Consolas", 9F);
            inviteCodeLabel.ForeColor = Color.FromArgb(180, 80, 150);
            inviteCodeLabel.Location = new Point(12, 32);
            inviteCodeLabel.Name = "inviteCodeLabel";
            inviteCodeLabel.Size = new Size(300, 20);
            inviteCodeLabel.TabIndex = 1;
            inviteCodeLabel.Text = "INVITE: G-XXX-XXXX";
            inviteCodeLabel.TextAlign = ContentAlignment.MiddleLeft;
            inviteCodeLabel.Visible = false;
            inviteCodeLabel.Click += InviteCodeLabel_Click;
            
            
            
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(6, 0, 10);
            ClientSize = new Size(1000, 700);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1000, 700);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "> BLACKCHAT :: NULLSEC :: ROSE";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            Paint += Form1_Paint;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)logoPic).EndInit();
            ((System.ComponentModel.ISupportInitialize)chatsBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)groupsBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)publicBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)addFriendBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)createGroupBtn).EndInit();
            ((System.ComponentModel.ISupportInitialize)joinGroupBtn).EndInit();
            panel2.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)sendBtn).EndInit();
            panel4.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private Panel panel4;
        private Panel panel5;
        private PictureBox logoPic;
        private PictureBox chatsBtn;
        private PictureBox groupsBtn;
        private PictureBox publicBtn;
        private PictureBox addFriendBtn;
        private PictureBox createGroupBtn;
        private PictureBox joinGroupBtn;
        private PictureBox sendBtn;
        private Label label1;
        private Label nameOfChannel;
        private Label inviteCodeLabel;
        private ListBox contactsList;
        private RichTextBox chatBox;
        private RichTextBox messageField;

        
        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            
            using var pen = new Pen(Color.FromArgb(255, 0, 136), 3);
            pen.Alignment = System.Drawing.Drawing2D.PenAlignment.Inset;
            e.Graphics.DrawRectangle(pen, 1, 1, this.ClientSize.Width - 3, this.ClientSize.Height - 3);

            
            using var pen2 = new Pen(Color.FromArgb(200, 0, 100), 1);
            e.Graphics.DrawRectangle(pen2, 4, 4, this.ClientSize.Width - 9, this.ClientSize.Height - 9);

            
            using var pen3 = new Pen(Color.FromArgb(255, 0, 136, 80), 1); 
            e.Graphics.DrawLine(pen3, 70, 0, 70, this.ClientSize.Height);
            e.Graphics.DrawLine(pen3, 310, 0, 310, this.ClientSize.Height);

            
            using var pen4 = new Pen(Color.FromArgb(255, 0, 136, 150), 1);
            e.Graphics.DrawLine(pen4, 0, 56, this.ClientSize.Width, 56);
        }
    }
}