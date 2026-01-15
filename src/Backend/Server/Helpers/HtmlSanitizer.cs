using System.Text.RegularExpressions;

namespace Server.Helpers;

public static class HtmlSanitizer
{
    /// <summary>
    /// Removes HTML tags from a string, optionally preserving line breaks
    /// </summary>
    public static string StripHtml(string? html, bool preserveLineBreaks = true)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        // Decode HTML entities first
        var decoded = System.Net.WebUtility.HtmlDecode(html);

        if (preserveLineBreaks)
        {
            // Replace <br>, <br/>, <p>, </p>, <div>, </div> with line breaks
            decoded = Regex.Replace(decoded, @"<(br|p|div)\s*/?>", "\n", RegexOptions.IgnoreCase);
            decoded = Regex.Replace(decoded, @"</(p|div)>", "\n", RegexOptions.IgnoreCase);
        }

        // Remove all HTML tags
        var stripped = Regex.Replace(decoded, @"<[^>]+>", string.Empty);

        // Clean up multiple whitespace and line breaks
        stripped = Regex.Replace(stripped, @"\s+", " ");
        stripped = Regex.Replace(stripped, @"\n\s*\n", "\n");

        return stripped.Trim();
    }

    /// <summary>
    /// Escapes HTML characters for safe display in HTML context
    /// </summary>
    public static string EscapeHtml(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        return System.Net.WebUtility.HtmlEncode(text);
    }
}
