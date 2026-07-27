//Original by h1ghwaay
//Remarked By szaman251.

namespace BlackChat;

public class User
{
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Salt { get; set; } = "";
    public List<string> Friends { get; set; } = new();
    public List<string> Groups { get; set; } = new();
    public string PublicKeyECDH { get; set; } = ""; // Base64
}