namespace MuralDigital.Models;

public class MuralConfig
{
    public string Congregation { get; set; } = "Cong. Auxiliadora";
    public string HeaderEmoji { get; set; } = "🚨";
    public string HeaderText { get; set; } = "MURAL ON-LINE";
    public string VisitNote { get; set; } = string.Empty;
    public string FooterSection { get; set; } = string.Empty;
    public List<MuralGroup> Groups { get; set; } = [];
    public List<WhatsAppContact> Contacts { get; set; } = [];
}
