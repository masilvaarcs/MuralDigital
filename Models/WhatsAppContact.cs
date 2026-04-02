namespace MuralDigital.Models;

public class WhatsAppContact
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsSelected { get; set; } = true;
}
