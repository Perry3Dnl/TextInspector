using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TextInspector;

// Shared, behavior-preserving engine component. Keep this file identical across Lite and Pro.
public sealed partial class TextInspectorEngine
{
    private string NormalizeForMatchOnly(string text)
        => NormalizeForMatchOnly(text,Options);

    private static string NormalizeForMatchOnly(string text,TextInspectorScoringOptions options)
    {
        if(text.Length == 0)
            return "";

        var sb = new StringBuilder(text.Length);
        bool pendingWhitespace = false;

        for(int i = 0;i < text.Length;i++)
        {
            char original = text[i];
            if(options.RemoveZeroWidthCharacters && IsIgnorableFormatCharacter(original))
                continue;

            char c = NormalizeCharacter(original,options);
            if(char.IsWhiteSpace(c))
            {
                if(sb.Length > 0)
                    pendingWhitespace = true;
                continue;
            }

            if(pendingWhitespace)
            {
                sb.Append(' ');
                pendingWhitespace = false;
            }

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }

    private NormalizedText NormalizeWithMap(string text)
    {
        if(text.Length == 0)
            return new NormalizedText("",Array.Empty<int>(),Array.Empty<int>());

        var sb = new StringBuilder(text.Length);
        var startMap = new List<int>(text.Length);
        var endMap = new List<int>(text.Length);

        int pendingWhitespaceStart = -1;
        int pendingWhitespaceEnd = -1;

        for(int i = 0;i < text.Length;i++)
        {
            char original = text[i];

            if(Options.RemoveZeroWidthCharacters && IsIgnorableFormatCharacter(original))
                continue;

            char c = NormalizeCharacter(original,Options);
            if(char.IsWhiteSpace(c))
            {
                if(pendingWhitespaceStart < 0)
                    pendingWhitespaceStart = i;
                pendingWhitespaceEnd = i + 1;
                continue;
            }

            if(pendingWhitespaceStart >= 0 && sb.Length > 0)
            {
                sb.Append(' ');
                startMap.Add(pendingWhitespaceStart);
                endMap.Add(pendingWhitespaceEnd);
            }

            pendingWhitespaceStart = -1;
            pendingWhitespaceEnd = -1;

            sb.Append(char.ToLowerInvariant(c));
            startMap.Add(i);
            endMap.Add(i + 1);
        }

        return new NormalizedText(sb.ToString(),startMap.ToArray(),endMap.ToArray());
    }

    private static char NormalizeCharacter(char c,TextInspectorScoringOptions options)
    {
        if(options.NormalizeFullWidthCharacters)
        {
            if(c == '\u3000')
                return ' ';
            if(c is >= '\uFF01' and <= '\uFF5E')
                c = (char)(c - 0xFEE0);
        }

        if(!options.NormalizeSmartPunctuation)
            return c;

        return c switch
        {
            '\u2018' or '\u2019' or '\u201A' or '\u201B' or '\u2032' => '\'',
            '\u201C' or '\u201D' or '\u201E' or '\u201F' or '\u2033' => '"',
            '\u2010' or '\u2011' or '\u2012' or '\u2013' or '\u2014' or '\u2015' or '\u2212' => '-',
            _ => c
        };
    }

    private static bool IsIgnorableFormatCharacter(char c)
        => c is '\u00AD' or '\u200B' or '\u200C' or '\u200D' or '\u2060' or '\uFEFF'
           || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.Format;

}
