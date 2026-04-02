using System.Globalization;
using System.Text.RegularExpressions;

namespace MuralDigital.Converters;

public class InvertBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value;
}

public class BoolToShortenTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "⏳..." : "🔗 Encurtar";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

public class IsNotEmptyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrWhiteSpace(s);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// Converts WhatsApp-formatted text (*bold*, _italic_) into a FormattedString
/// with proper Spans so the preview resembles actual WhatsApp rendering.
/// </summary>
public partial class WhatsAppFormattedTextConverter : IValueConverter
{
    // Matches *bold* and _italic_ segments (non-greedy, single-line segments)
    [GeneratedRegex(@"(\*(.+?)\*)|(_(.+?)_)")]
    private static partial Regex WhatsAppMarkupRegex();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
            return new FormattedString();

        var formatted = new FormattedString();
        var textColor = Application.Current?.RequestedTheme == AppTheme.Dark
            ? Colors.White
            : Colors.Black;
        var linkColor = Application.Current?.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#80CBC4")
            : Color.FromArgb("#1565C0");

        // Process line by line to handle links
        var lines = text.Split('\n');
        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
        {
            var line = lines[lineIdx];

            // Check if line is a URL
            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("http://") || trimmed.StartsWith("https://"))
            {
                var indent = line[..^trimmed.Length];
                if (indent.Length > 0)
                    formatted.Spans.Add(new Span { Text = indent, TextColor = textColor, FontSize = 13 });

                formatted.Spans.Add(new Span
                {
                    Text = trimmed,
                    TextColor = linkColor,
                    TextDecorations = TextDecorations.Underline,
                    FontSize = 13
                });
            }
            else
            {
                // Parse WhatsApp formatting within the line
                ParseFormattedLine(line, formatted, textColor);
            }

            // Add newline between lines (not after last)
            if (lineIdx < lines.Length - 1)
                formatted.Spans.Add(new Span { Text = "\n", FontSize = 13 });
        }

        return formatted;
    }

    private static void ParseFormattedLine(string line, FormattedString formatted, Color textColor)
    {
        var regex = WhatsAppMarkupRegex();
        int lastIndex = 0;

        foreach (Match match in regex.Matches(line))
        {
            // Add text before this match
            if (match.Index > lastIndex)
            {
                formatted.Spans.Add(new Span
                {
                    Text = line[lastIndex..match.Index],
                    TextColor = textColor,
                    FontSize = 13
                });
            }

            if (match.Groups[2].Success) // *bold*
            {
                formatted.Spans.Add(new Span
                {
                    Text = match.Groups[2].Value,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = textColor,
                    FontSize = 13
                });
            }
            else if (match.Groups[4].Success) // _italic_
            {
                formatted.Spans.Add(new Span
                {
                    Text = match.Groups[4].Value,
                    FontAttributes = FontAttributes.Italic,
                    TextColor = textColor,
                    FontSize = 13
                });
            }

            lastIndex = match.Index + match.Length;
        }

        // Add remaining text after last match
        if (lastIndex < line.Length)
        {
            formatted.Spans.Add(new Span
            {
                Text = line[lastIndex..],
                TextColor = textColor,
                FontSize = 13
            });
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
