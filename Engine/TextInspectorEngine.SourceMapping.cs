using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TextInspector;

// Shared, behavior-preserving engine component. Keep this file identical across TextInspector and Pro.
public sealed partial class TextInspectorEngine
{
    private static int[] BuildLineStarts(string text)
    {
        var starts = new List<int> { 0 };
        for(int i = 0;i < text.Length;i++)
        {
            if(text[i] == '\n' && i + 1 <= text.Length)
                starts.Add(i + 1);
        }
        return starts.ToArray();
    }

    private static TextSourceRange CreateSourceRange(int[] lineStarts,int start,int length)
    {
        int endExclusive = start + length;
        (int startLine,int startColumn) = GetLineColumn(lineStarts,start);
        (int endLine,int endColumn) = GetLineColumn(lineStarts,endExclusive);
        return new TextSourceRange
        {
            StartLine = startLine,
            StartColumn = startColumn,
            EndLine = endLine,
            EndColumn = endColumn
        };
    }

    private static (int Line,int Column) GetLineColumn(int[] lineStarts,int position)
    {
        int index = Array.BinarySearch(lineStarts,position);
        if(index < 0)
            index = ~index - 1;
        index = Math.Max(0,index);
        return (index + 1,position - lineStarts[index] + 1);
    }

    private (string? Text,int? Start) CreateSnippet(string originalText,int start,int length)
    {
        int snippetStart = Math.Max(0,start - RuntimeOptions.EvidenceSnippetPad);
        int snippetEnd = Math.Min(originalText.Length,start + length + RuntimeOptions.EvidenceSnippetPad);

        if(snippetStart > 0 && snippetStart < originalText.Length && char.IsLowSurrogate(originalText[snippetStart]))
            snippetStart--;
        if(snippetEnd > 0 && snippetEnd < originalText.Length && char.IsHighSurrogate(originalText[snippetEnd - 1]))
            snippetEnd++;

        return (originalText.Substring(snippetStart,snippetEnd - snippetStart),snippetStart);
    }

}
