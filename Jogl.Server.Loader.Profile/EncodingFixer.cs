using System.Text;
using System.Text.RegularExpressions;

public static class EncodingFixer
{
    // Dictionary of common corruption patterns and their fixes
    private static readonly Dictionary<string, string> CorruptionMap = new Dictionary<string, string>
    {
        // Diacritical mark corruptions
        {"╠ü", "ü"},
        {"╠ê", "ü"},
        {"╠ä", "ä"},
        {"╠ö", "ö"},
        {"╠é", "é"},
        {"╠è", "è"},
        {"╠à", "à"},
        {"╠á", "á"},
        {"╠í", "í"},
        {"╠ó", "ó"},
        {"╠ú", "ú"},
        {"╠ñ", "ñ"},
        {"╠ç", "ç"},
        
        // Punctuation corruptions
        {"ΓÇÉ", "-"},
        {"ΓÇô", "–"}, // en dash
        {"ΓÇö", "—"}, // em dash
        {"ΓÇ£", "\""},
        {"ΓÇ¥", "\""},
        {"ΓÇÖ", "'"},
        {"ΓÇÖ", "'"},
        {"ΓÇ¿", "…"},
        
        // Other common corruptions
        {"Ã¡", "á"},
        {"Ã©", "é"},
        {"Ã­", "í"},
        {"Ã³", "ó"},
        {"Ãº", "ú"},
        {"Ã±", "ñ"},
        {"Ã¼", "ü"},
        {"Ã¶", "ö"},
        {"Ã¤", "ä"},
    };

    /// <summary>
    /// Fixes common text encoding corruption issues
    /// </summary>
    /// <param name="corruptedText">The text with encoding issues</param>
    /// <returns>The fixed text</returns>
    public static string FixCorruption(string corruptedText)
    {
        if (string.IsNullOrEmpty(corruptedText))
            return corruptedText;

        string fixedText = corruptedText;

        // Apply all known corruption fixes
        foreach (var corruption in CorruptionMap)
        {
            fixedText = fixedText.Replace(corruption.Key, corruption.Value);
        }

        // Additional regex-based fixes for patterns
        fixedText = FixRegexPatterns(fixedText);

        return fixedText;
    }

    /// <summary>
    /// Fixes corruption patterns using regex
    /// </summary>
    private static string FixRegexPatterns(string text)
    {
        // Fix combining diacritical marks that appear separately
        text = Regex.Replace(text, @"([aeiouAEIOU])╠[üêäöéèàáíóúñç]",
            match => FixCombiningMark(match.Value));

        // Fix multiple corruption patterns
        text = Regex.Replace(text, @"ΓÇ[ÉôöÖñ£¥¿]", FixPunctuation);

        return text;
    }

    /// <summary>
    /// Helper method to fix combining diacritical marks
    /// </summary>
    private static string FixCombiningMark(string match)
    {
        char baseChar = match[0];
        string corruption = match.Substring(1);

        return CorruptionMap.ContainsKey(corruption)
            ? baseChar + CorruptionMap[corruption].Substring(1)  // Keep base char, add proper diacritic
            : match;
    }

    /// <summary>
    /// Helper method to fix punctuation corruption
    /// </summary>
    private static string FixPunctuation(Match match)
    {
        return CorruptionMap.ContainsKey(match.Value)
            ? CorruptionMap[match.Value]
            : match.Value;
    }

    /// <summary>
    /// Attempts to detect and fix charset encoding issues
    /// </summary>
    public static string FixCharsetIssues(string text)
    {
        try
        {
            // Try to detect if text was incorrectly decoded as Windows-1252 instead of UTF-8
            byte[] bytes = Encoding.GetEncoding("Windows-1252").GetBytes(text);
            string utf8Text = Encoding.UTF8.GetString(bytes);

            // If the UTF-8 version looks more "normal", use it
            if (HasFewerCorruptionMarkers(utf8Text, text))
                return utf8Text;
        }
        catch (Exception)
        {
            // If conversion fails, continue with original text
        }

        return text;
    }

    /// <summary>
    /// Checks if one text has fewer corruption markers than another
    /// </summary>
    private static bool HasFewerCorruptionMarkers(string text1, string text2)
    {
        int count1 = CountCorruptionMarkers(text1);
        int count2 = CountCorruptionMarkers(text2);
        return count1 < count2;
    }

    /// <summary>
    /// Counts common corruption markers in text
    /// </summary>
    private static int CountCorruptionMarkers(string text)
    {
        int count = 0;
        foreach (string marker in CorruptionMap.Keys)
        {
            count += Regex.Matches(text, Regex.Escape(marker)).Count;
        }
        return count;
    }
}