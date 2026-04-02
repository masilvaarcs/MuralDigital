using System.Text.RegularExpressions;
using System.Web;

namespace MuralDigital.Services;

public interface IUrlShortenerService
{
    Task<string> ShortenAsync(string longUrl, string? friendlySlug = null);
    Task<bool> ValidateUrlAsync(string url);
}

public partial class TinyUrlService : IUrlShortenerService
{
    private readonly HttpClient _httpClient;

    public TinyUrlService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<string> ShortenAsync(string longUrl, string? friendlySlug = null)
    {
        if (string.IsNullOrWhiteSpace(longUrl))
            return string.Empty;

        var encoded = HttpUtility.UrlEncode(longUrl);

        // 1) TinyURL with custom alias (descriptive, like Campo-Abril-2026)
        if (!string.IsNullOrWhiteSpace(friendlySlug))
        {
            var alias = GenerateDescriptiveAlias(friendlySlug);
            var result = await TryTinyUrlWithAliasAsync(encoded, alias);
            if (result is not null) return result;

            // Retry with suffix if alias was taken
            var aliasWithSuffix = alias + "-" + Random.Shared.Next(10, 99);
            result = await TryTinyUrlWithAliasAsync(encoded, aliasWithSuffix);
            if (result is not null) return result;
        }

        // 2) TinyURL random (always works, direct redirect)
        var tinyResult = await TryTinyUrlRandomAsync(encoded);
        if (tinyResult is not null) return tinyResult;

        // 3) is.gd with custom slug
        if (!string.IsNullOrWhiteSpace(friendlySlug))
        {
            var slug = GenerateDescriptiveAlias(friendlySlug).ToLowerInvariant();
            var isgdResult = await TryIsGdAsync(encoded, slug);
            if (isgdResult is not null) return isgdResult;
        }

        // 4) is.gd random
        var isgdRandom = await TryIsGdAsync(encoded, null);
        if (isgdRandom is not null) return isgdRandom;

        // 5) Last resort: return original URL
        return longUrl;
    }

    private async Task<string?> TryTinyUrlWithAliasAsync(string encodedUrl, string alias)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(
                $"https://tinyurl.com/api-create.php?url={encodedUrl}&alias={alias}");
            var trimmed = response.Trim();
            // TinyURL returns "Error" if alias is taken
            if (!trimmed.StartsWith("Error", StringComparison.OrdinalIgnoreCase)
                && Uri.TryCreate(trimmed, UriKind.Absolute, out _))
                return trimmed;
        }
        catch { /* alias taken or service down */ }
        return null;
    }

    private async Task<string?> TryTinyUrlRandomAsync(string encodedUrl)
    {
        try
        {
            var response = await _httpClient.GetStringAsync(
                $"https://tinyurl.com/api-create.php?url={encodedUrl}");
            var trimmed = response.Trim();
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out _))
                return trimmed;
        }
        catch { /* service down */ }
        return null;
    }

    private async Task<string?> TryIsGdAsync(string encodedUrl, string? slug)
    {
        try
        {
            var url = $"https://is.gd/create.php?format=simple&url={encodedUrl}";
            if (slug is not null)
                url += $"&shorturl={slug}";
            var response = await _httpClient.GetStringAsync(url);
            var trimmed = response.Trim();
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out _))
                return trimmed;
        }
        catch { /* slug taken or service down */ }
        return null;
    }

    public async Task<bool> ValidateUrlAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            using var request = new HttpRequestMessage(HttpMethod.Head, url);
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            return (int)response.StatusCode < 400
                || response.StatusCode == System.Net.HttpStatusCode.Found
                || response.StatusCode == System.Net.HttpStatusCode.MovedPermanently;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generates a descriptive alias from item label.
    /// "Atual (Abril/2026)" → "Atual-Abril-2026"
    /// "Semana 6-12" → "Semana-6-12"
    /// Preserves mixed case for readability (like the user's old links).
    /// </summary>
    private static string GenerateDescriptiveAlias(string input)
    {
        // Remove accents but preserve case
        var cleaned = RemoveAccents(input);

        // Replace non-alphanumeric with hyphens
        var alias = NonAlphaNumRegex().Replace(cleaned, "-").Trim('-');

        // Collapse multiple hyphens
        alias = MultiHyphenRegex().Replace(alias, "-");

        // Limit to 30 chars (TinyURL alias limit ~30-40)
        if (alias.Length > 30)
            alias = alias[..30].TrimEnd('-');

        return alias;
    }

    private static string RemoveAccents(string input)
    {
        return input
            .Replace("ã", "a").Replace("Ã", "A")
            .Replace("á", "a").Replace("Á", "A")
            .Replace("â", "a").Replace("Â", "A")
            .Replace("à", "a").Replace("À", "A")
            .Replace("é", "e").Replace("É", "E")
            .Replace("ê", "e").Replace("Ê", "E")
            .Replace("í", "i").Replace("Í", "I")
            .Replace("ó", "o").Replace("Ó", "O")
            .Replace("ô", "o").Replace("Ô", "O")
            .Replace("õ", "o").Replace("Õ", "O")
            .Replace("ú", "u").Replace("Ú", "U")
            .Replace("ü", "u").Replace("Ü", "U")
            .Replace("ç", "c").Replace("Ç", "C");
    }

    [GeneratedRegex(@"[^a-zA-Z0-9]+")]
    private static partial Regex NonAlphaNumRegex();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultiHyphenRegex();
}
