namespace BlackChat;

public class Group
{
    public string GroupCode { get; set; } = "";
    public string GroupName { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public List<string> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}