namespace BlackChat;

public class Message
{
    public string Username { get; set; } = "";
    public string Text { get; set; } = "";
    public string IV { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}