namespace BlackChat;

public partial class RegisterForm : Form
{
    private readonly FirebaseService _firebaseService;

    public RegisterForm()
    {
        _firebaseService = new FirebaseService();
        InitializeComponent();
    }

    private async void RegisterBtn_Click(object? sender, EventArgs e)
    {
        string username = usernameBox.Text.Trim();
        string password = passwordBox.Text.Trim();
        string confirmPassword = confirmPasswordBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Username and password are required");
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Passwords do not match");
            return;
        }

        if (password.Length < 6)
        {
            ShowError("Password must be at least 6 characters");
            return;
        }

        registerBtn.Enabled = false;
        errorLabel.Visible = false;

        try
        {
            bool success = await _firebaseService.CreateUser(username, password);
            if (success)
            {
                // Wygeneruj klucze dla nowego użytkownika i zapisz publiczny w Firebase
                var keyManager = new KeyManager(username);
                keyManager.GetOrCreateEcdsaKeys(); // tworzy i zapisuje na dysku
                var publicKey = keyManager.GetPublicKeyBase64();
                await _firebaseService.UpdateUserPublicKey(username, publicKey);

                MessageBox.Show("Account created successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                ShowError("Username already exists");
                registerBtn.Enabled = true;
            }
        }
        catch (Exception ex)
        {
            ShowError($"Registration error: {ex.Message}");
            registerBtn.Enabled = true;
        }
    }

    private void ShowError(string message)
    {     
        errorLabel.Text = message;
        errorLabel.Visible = true;
        registerBtn.Enabled = true;
    }

    private void RegisterForm_Load(object sender, EventArgs e)
    {

    }
}