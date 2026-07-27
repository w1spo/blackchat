namespace BlackChat;

partial class RegisterForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegisterForm));
        usernameBox = new TextBox();
        passwordBox = new TextBox();
        confirmPasswordBox = new TextBox();
        errorLabel = new Label();
        registerBtn = new Button();
        label1 = new Label();
        label2 = new Label();
        label3 = new Label();
        label4 = new Label();
        pictureBox1 = new PictureBox();
        label5 = new Label();
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
        // confirmPasswordBox
        // 
        confirmPasswordBox.BackColor = SystemColors.WindowFrame;
        confirmPasswordBox.BorderStyle = BorderStyle.FixedSingle;
        confirmPasswordBox.ForeColor = SystemColors.Info;
        confirmPasswordBox.Location = new Point(126, 178);
        confirmPasswordBox.Name = "confirmPasswordBox";
        confirmPasswordBox.PasswordChar = '*';
        confirmPasswordBox.Size = new Size(204, 23);
        confirmPasswordBox.TabIndex = 5;
        // 
        // errorLabel
        // 
        errorLabel.ForeColor = Color.Red;
        errorLabel.Location = new Point(28, 211);
        errorLabel.Name = "errorLabel";
        errorLabel.Size = new Size(302, 25);
        errorLabel.TabIndex = 6;
        errorLabel.TextAlign = ContentAlignment.MiddleCenter;
        errorLabel.Visible = false;
        // 
        // registerBtn
        // 
        registerBtn.BackColor = Color.SlateBlue;
        registerBtn.FlatStyle = FlatStyle.Popup;
        registerBtn.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        registerBtn.ForeColor = Color.White;
        registerBtn.Location = new Point(28, 244);
        registerBtn.Name = "registerBtn";
        registerBtn.Size = new Size(302, 32);
        registerBtn.TabIndex = 7;
        registerBtn.Text = "Register";
        registerBtn.UseVisualStyleBackColor = false;
        registerBtn.Click += RegisterBtn_Click;
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
        label3.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        label3.ForeColor = Color.White;
        label3.Location = new Point(28, 178);
        label3.Name = "label3";
        label3.Size = new Size(62, 17);
        label3.TabIndex = 11;
        label3.Text = "Confirm:";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        label4.ForeColor = Color.White;
        label4.Location = new Point(96, 12);
        label4.Name = "label4";
        label4.Size = new Size(189, 32);
        label4.TabIndex = 12;
        label4.Text = "Create Account";
        // 
        // pictureBox1
        // 
        pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
        pictureBox1.Location = new Point(373, 12);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(251, 264);
        pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        pictureBox1.TabIndex = 13;
        pictureBox1.TabStop = false;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold);
        label5.ForeColor = Color.White;
        label5.Location = new Point(112, 53);
        label5.Name = "label5";
        label5.Size = new Size(160, 17);
        label5.TabIndex = 14;
        label5.Text = "To continue to Blackchat";
        // 
        // RegisterForm
        // 
        AllowDrop = true;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(35, 35, 40);
        ClientSize = new Size(669, 300);
        Controls.Add(label5);
        Controls.Add(pictureBox1);
        Controls.Add(label4);
        Controls.Add(label3);
        Controls.Add(label2);
        Controls.Add(label1);
        Controls.Add(registerBtn);
        Controls.Add(errorLabel);
        Controls.Add(confirmPasswordBox);
        Controls.Add(passwordBox);
        Controls.Add(usernameBox);
        FormBorderStyle = FormBorderStyle.None;
        MaximizeBox = false;
        Name = "RegisterForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "BlackChat - Register";
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
    private System.Windows.Forms.TextBox usernameBox;
    private System.Windows.Forms.TextBox passwordBox;
    private System.Windows.Forms.TextBox confirmPasswordBox;
    private System.Windows.Forms.Label errorLabel;
    private Button registerBtn;
    private Label label1;
    private Label label2;
    private Label label3;
    private Label label4;
    private PictureBox pictureBox1;
    private Label label5;
}