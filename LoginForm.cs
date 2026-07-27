using System.Drawing.Drawing2D;

namespace BlackChat;

public partial class LoginForm : Form
{
    private readonly FirebaseService _firebaseService;

    public LoginForm()
    {
        _firebaseService = new FirebaseService();
        InitializeComponent();
        SetRoundedCorners(10);
    }


private void SetRoundedCorners(int radius)
{
    GraphicsPath path = new GraphicsPath();

    path.AddArc(0, 0, radius, radius, 180, 90);
    path.AddArc(Width - radius, 0, radius, radius, 270, 90);
    path.AddArc(Width - radius, Height - radius, radius, radius, 0, 90);
    path.AddArc(0, Height - radius, radius, radius, 90, 90);

    path.CloseFigure();

    Region = new Region(path);
}
private async void LoginBtn_Click(object? sender, EventArgs e)
    {
        string username = usernameBox.Text.Trim();
        string password = passwordBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Please enter username and password");
            return;
        }

        loginBtn.Enabled = false;
        errorLabel.Visible = false;

        try
        {
            bool isValid = await _firebaseService.LoginUser(username, password);
            if (isValid)
            {
                Form1 chatForm = new(username);
                chatForm.Show();
                this.Hide();
            }
            else
            {
                ShowError("Invalid username or password");
                loginBtn.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            ShowError($"Login error: {ex.Message}");
            loginBtn.Enabled = true;
        }
    }

    private void createAccountLink_Click(object? sender, EventArgs e)
    {
        RegisterForm registerForm = new();
        registerForm.ShowDialog();
        loginBtn.Enabled = true;
        errorLabel.Visible = false;
    }

    private void ShowError(string message)
    {
        errorLabel.Text = message;
        errorLabel.Visible = true;
        loginBtn.Enabled = true;
    }

    private void acrylicTitle1_Click(object sender, EventArgs e)
    {

    }

    private void windowPanel1_Paint(object sender, PaintEventArgs e)
    {

    }
}