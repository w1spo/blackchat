namespace BlackChat;

public class User
{
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Salt { get; set; } = "";
    public List<string> Friends { get; set; } = new();
    public List<string> Groups { get; set; } = new();
}