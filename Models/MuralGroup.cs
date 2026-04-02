namespace MuralDigital.Models;

public class MuralGroup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Emoji { get; set; } = "⚙️";
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public List<MuralItem> Items { get; set; } = [];
    public int Order { get; set; }
}
