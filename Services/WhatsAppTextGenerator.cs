using System.Text;
using MuralDigital.Models;

namespace MuralDigital.Services;

public enum WhatsAppStyle
{
    Classico,
    Compacto,
    Destaque,
    Formal
}

public static class WhatsAppStyleInfo
{
    public static string GetName(WhatsAppStyle style) => style switch
    {
        WhatsAppStyle.Classico => "Clássico",
        WhatsAppStyle.Compacto => "Compacto",
        WhatsAppStyle.Destaque => "Destaque",
        WhatsAppStyle.Formal => "Formal",
        _ => style.ToString()
    };

    public static string GetDescription(WhatsAppStyle style) => style switch
    {
        WhatsAppStyle.Classico => "Emojis e negrito, espaçamento confortável",
        WhatsAppStyle.Compacto => "Menos espaço, direto ao ponto",
        WhatsAppStyle.Destaque => "Bordas decorativas, emojis chamativos",
        WhatsAppStyle.Formal => "Limpo e organizado, poucos emojis",
        _ => ""
    };
}

public interface IWhatsAppTextGenerator
{
    string Generate(MuralConfig config, WhatsAppStyle style = WhatsAppStyle.Classico);
    WhatsAppStyle[] GetAvailableStyles();
}

public class WhatsAppTextGenerator : IWhatsAppTextGenerator
{
    public WhatsAppStyle[] GetAvailableStyles() =>
        [WhatsAppStyle.Classico, WhatsAppStyle.Compacto, WhatsAppStyle.Destaque, WhatsAppStyle.Formal];

    public string Generate(MuralConfig config, WhatsAppStyle style = WhatsAppStyle.Classico) => style switch
    {
        WhatsAppStyle.Compacto => GenerateCompact(config),
        WhatsAppStyle.Destaque => GenerateHighlight(config),
        WhatsAppStyle.Formal => GenerateFormal(config),
        _ => GenerateClassic(config)
    };

    // ── CLÁSSICO ─────────────────────────────
    private static string GenerateClassic(MuralConfig config)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"{config.HeaderEmoji} *{config.HeaderText} - {config.Congregation}* {config.HeaderEmoji}");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(config.VisitNote))
        {
            sb.AppendLine($"  📌 *{config.VisitNote}*");
            sb.AppendLine();
        }

        foreach (var group in config.Groups.OrderBy(g => g.Order))
        {
            sb.AppendLine($"{group.Emoji} *{group.Title}*");
            if (!string.IsNullOrWhiteSpace(group.Subtitle))
                sb.AppendLine($"(_{group.Subtitle}_)");
            sb.AppendLine();

            foreach (var item in group.Items ?? [])
            {
                var url = GetUrl(item);
                if (url is null) continue;
                sb.AppendLine($"  📌 *{item.Label}*");
                sb.AppendLine($"  👉 {url}");
                sb.AppendLine();
            }
        }

        AppendFooter(sb, config.FooterSection, "─────────────────────────────");
        return sb.ToString().TrimEnd();
    }

    // ── COMPACTO ─────────────────────────────
    private static string GenerateCompact(MuralConfig config)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"*{config.HeaderEmoji} {config.HeaderText} — {config.Congregation}*");

        if (!string.IsNullOrWhiteSpace(config.VisitNote))
            sb.AppendLine($"📌 _{config.VisitNote}_");

        sb.AppendLine();

        foreach (var group in config.Groups.OrderBy(g => g.Order))
        {
            sb.Append($"*{group.Emoji} {group.Title}*");
            if (!string.IsNullOrWhiteSpace(group.Subtitle))
                sb.Append($" _{group.Subtitle}_");
            sb.AppendLine();

            foreach (var item in group.Items ?? [])
            {
                var url = GetUrl(item);
                if (url is null) continue;
                sb.AppendLine($"• *{item.Label}:* {url}");
            }
            sb.AppendLine();
        }

        AppendFooter(sb, config.FooterSection, "---");
        return sb.ToString().TrimEnd();
    }

    // ── DESTAQUE ─────────────────────────────
    private static string GenerateHighlight(MuralConfig config)
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════╗");
        sb.AppendLine($"  {config.HeaderEmoji} *{config.HeaderText}* {config.HeaderEmoji}");
        sb.AppendLine($"  *{config.Congregation}*");
        sb.AppendLine("╚══════════════════════════╝");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(config.VisitNote))
        {
            sb.AppendLine($"🔔 *{config.VisitNote}* 🔔");
            sb.AppendLine();
        }

        foreach (var group in config.Groups.OrderBy(g => g.Order))
        {
            sb.AppendLine($"▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬");
            sb.AppendLine($"✨ *{group.Emoji} {group.Title}* ✨");
            if (!string.IsNullOrWhiteSpace(group.Subtitle))
                sb.AppendLine($"      _{group.Subtitle}_");
            sb.AppendLine();

            foreach (var item in group.Items ?? [])
            {
                var url = GetUrl(item);
                if (url is null) continue;
                sb.AppendLine($"  🔹 *{item.Label}*");
                sb.AppendLine($"     🔗 {url}");
                sb.AppendLine();
            }
        }

        sb.AppendLine("▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬▬");
        AppendFooter(sb, config.FooterSection, "");
        return sb.ToString().TrimEnd();
    }

    // ── FORMAL ───────────────────────────────
    private static string GenerateFormal(MuralConfig config)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"*{config.HeaderText}*");
        sb.AppendLine($"_{config.Congregation}_");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(config.VisitNote))
        {
            sb.AppendLine($"Aviso: *{config.VisitNote}*");
            sb.AppendLine();
        }

        int groupNum = 1;
        foreach (var group in config.Groups.OrderBy(g => g.Order))
        {
            sb.AppendLine($"*{groupNum}. {group.Title}*");
            if (!string.IsNullOrWhiteSpace(group.Subtitle))
                sb.AppendLine($"   _{group.Subtitle}_");

            int itemNum = 1;
            foreach (var item in group.Items ?? [])
            {
                var url = GetUrl(item);
                if (url is null) continue;
                sb.AppendLine($"   {groupNum}.{itemNum}. {item.Label}");
                sb.AppendLine($"         {url}");
                itemNum++;
            }
            sb.AppendLine();
            groupNum++;
        }

        AppendFooter(sb, config.FooterSection, "---");
        return sb.ToString().TrimEnd();
    }

    // ── Helpers ──────────────────────────────
    private static string? GetUrl(MuralItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.ShortUrl))
            return item.ShortUrl;
        if (!string.IsNullOrWhiteSpace(item.OriginalUrl))
            return item.OriginalUrl;
        return null;
    }

    private static void AppendFooter(StringBuilder sb, string footerSection, string separator)
    {
        if (string.IsNullOrWhiteSpace(footerSection)) return;

        if (!string.IsNullOrWhiteSpace(separator))
            sb.AppendLine(separator);
        sb.AppendLine("*Últimas Atualizações:*");
        sb.AppendLine(footerSection);
    }
}
