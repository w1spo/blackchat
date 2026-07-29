namespace BlackChat;

partial class LoginForm
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginForm));
        usernameBox = new TextBox();
        passwordBox = new TextBox();
        errorLabel = new Label();
        loginBtn = new Button();
        createAccountLink = new Button();
        label1 = new Label();
        label2 = new Label();
        label3 = new Label();
        pictureBox1 = new PictureBox();
        label4 = new Label();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
        SuspendLayout();
        // 
        // usernameBox
        // 
        usernameBox.BackColor = SystemColors.WindowFrame;
        usernameBox.BorderStyle = BorderStyle.FixedSingle;
        usernameBox.ForeColor = SystemColors.Info;
        usernameBox.Location = new Point(126, 98);
        usernameBox.Name = "usernameBox";
        usernameBox.Size = new Size(204, 23);
        usernameBox.TabIndex = 1;
        // 
        // passwordBox
        // 
        passwordBox.BackColor = SystemColors.WindowFrame;
        passwordBox.BorderStyle = BorderStyle.FixedSingle;
        passwordBox.ForeColor = SystemColors.Info;
        passwordBox.Location = new Point(126, 138);
        passwordBox.Name = "passwordBox";
        passwordBox.PasswordChar = '*';
        passwordBox.Size = new Size(204, 23);
        passwordBox.TabIndex = 3;
        // 
        // errorLabel
        // 
        errorLabel.ForeColor = Color.Red;
        errorLabel.Location = new Point(28, 171);
        errorLabel.Name = "errorLabel";
        errorLabel.Size = new Size(302, 25);
        errorLabel.TabIndex = 6;
        errorLabel.TextAlign = ContentAlignment.MiddleCenter;
        errorLabel.Visible = false;
        // 
        // loginBtn
        // 
        loginBtn.BackColor = Color.SlateBlue;
        loginBtn.FlatStyle = FlatStyle.Popup;
        loginBtn.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        loginBtn.ForeColor = Color.White;
        loginBtn.Location = new Point(28, 204);
        loginBtn.Name = "loginBtn";
        loginBtn.Size = new Size(302, 32);
        loginBtn.TabIndex = 7;
        loginBtn.Text = "Login";
        loginBtn.UseVisualStyleBackColor = false;
        loginBtn.Click += LoginBtn_Click;
        // 
        // createAccountLink
        // 
        createAccountLink.BackColor = Color.SlateBlue;
        createAccountLink.FlatStyle = FlatStyle.Popup;
        createAccountLink.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        createAccountLink.ForeColor = Color.White;
        createAccountLink.Location = new Point(28, 242);
        createAccountLink.Name = "createAccountLink";
        createAccountLink.Size = new Size(302, 32);
        createAccountLink.TabIndex = 8;
        createAccountLink.Text = "Create Account";
        createAccountLink.UseVisualStyleBackColor = false;
        createAccountLink.Click += createAccountLink_Click;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        label1.ForeColor = Color.White;
        label1.Location = new Point(28, 101);
        label1.Name = "label1";
        label1.Size = new Size(73, 17);
        label1.TabIndex = 9;
        label1.Text = "Username:";
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        label2.ForeColor = Color.White;
        label2.Location = new Point(28, 138);
        label2.Name = "label2";
        label2.Size = new Size(70, 17);
        label2.TabIndex = 10;
        label2.Text = "Password:";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
        label3.ForeColor = Color.White;
        label3.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Polite;
        label3.Location = new Point(86, 12);
        label3.Name = "label3";
        label3.Size = new Size(187, 32);
        label3.TabIndex = 11;
        label3.Text = "Welcome Back!";
        // 
        // pictureBox1
        // 
        pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
        pictureBox1.Location = new Point(373, 12);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(251, 262);
        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        pictureBox1.TabIndex = 12;
        pictureBox1.TabStop = false;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
        label4.ForeColor = Color.White;
        label4.LiveSetting = System.Windows.Forms.Automation.AutomationLiveSetting.Polite;
        label4.Location = new Point(126, 55);
        label4.Name = "label4";
        label4.Size = new Size(87, 17);
        label4.TabIndex = 13;
        label4.Text = "To BlackChat";
        // 
        // LoginForm
        // 
        AllowDrop = true;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(35, 35, 40);
        ClientSize = new Size(669, 300);
        Controls.Add(label4);
        Controls.Add(pictureBox1);
        Controls.Add(label3);
        Controls.Add(label2);
        Controls.Add(label1);
        Controls.Add(createAccountLink);
        Controls.Add(loginBtn);
        Controls.Add(errorLabel);
        Controls.Add(passwordBox);
        Controls.Add(usernameBox);
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        Name = "LoginForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "BlackChat - Login";
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private System.Windows.Forms.TextBox usernameBox;
    private System.Windows.Forms.TextBox passwordBox;
    private System.Windows.Forms.Label errorLabel;
    private Button loginBtn;
    private Button createAccountLink;
    private Label label1;
    private Label label2;
    private Label label3;
    private PictureBox pictureBox1;
    private Label label4;
}