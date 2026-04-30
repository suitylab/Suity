using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;
using System.Collections.Generic;

namespace Suity.Editor.Views.TextEditing;

public class JsonFoldingStrategy
{
    public void UpdateFoldings(FoldingManager manager, TextDocument document)
    {
        int firstErrorOffset;
        var newFoldings = CreateNewFoldings(document, out firstErrorOffset);
        manager.UpdateFoldings(newFoldings, firstErrorOffset);
    }

    public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
    {
        firstErrorOffset = -1;
        var list = new List<NewFolding>();
        var startOffsets = new Stack<int>();

        if (document == null) return list;

        foreach (var line in document.Lines)
        {
            int lineOffset = line.Offset;
            string text = document.GetText(line.Offset, line.Length);

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                // Support both braces and brackets
                if (c == '{' || c == '[')
                {
                    startOffsets.Push(lineOffset + i);
                }
                else if ((c == '}' || c == ']') && startOffsets.Count > 0)
                {
                    int start = startOffsets.Pop();
                    int end = lineOffset + i + 1;

                    // Ensure cross-line folding
                    if (document.GetLineByOffset(start).LineNumber != line.LineNumber)
                    {
                        list.Add(new NewFolding(start, end));
                    }
                }
            }
        }

        list.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
        return list;
    }
}