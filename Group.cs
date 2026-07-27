//Original by h1ghwaay
//Remarked By szaman251.

namespace BlackChat;

public class Group
{
    public string GroupCode { get; set; } = "";
    public string GroupName { get; set; } = "";
    public string CreatedBy { get; set; } = "";
    public List<string> Members { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> EncryptedGroupKeys { get; set; } = new(); // username -> zaszyfrowany klucz AES grupy (Base64)
}