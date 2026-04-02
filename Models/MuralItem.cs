namespace MuralDigital.Models;

public class MuralItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Label { get; set; } = string.Empty;
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
}
