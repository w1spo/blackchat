namespace BlackChat;

public class Message
{
    public string Id { get; set; } = "";          
    public string Username { get; set; } = "";
    public string Text { get; set; } = "";
    public string IV { get; set; } = "";
    public string Tag { get; set; } = "";
    public string Signature { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}