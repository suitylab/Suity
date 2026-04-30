using System.Collections.Generic;
using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

namespace Suity.Editor.Views.TextEditing;

public class BraceFoldingStrategy
{
    /// <summary>
    /// Update folding ranges in the folding manager
    /// </summary>
    public void UpdateFoldings(FoldingManager manager, TextDocument document)
    {
        int firstErrorOffset;
        var newFoldings = CreateNewFoldings(document, out firstErrorOffset);
        manager.UpdateFoldings(newFoldings, firstErrorOffset);
    }

    /// <summary>
    /// Scan document to find all matching brace pairs
    /// </summary>
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
                if (c == '{')
                {
                    // Record absolute offset of left brace
                    startOffsets.Push(lineOffset + i);
                }
                else if (c == '}' && startOffsets.Count > 0)
                {
                    int start = startOffsets.Pop();
                    int end = lineOffset + i + 1;

                    // Optimization: only allow folding when start and end positions are not on the same line
                    // This avoids single-line code like "public int Property { get; set; }" from being folded
                    if (document.GetLineByOffset(start).LineNumber != line.LineNumber)
                    {
                        list.Add(new NewFolding(start, end));
                    }
                }
            }
        }

        // Must be sorted by start offset, otherwise rendering may have issues
        list.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
        return list;
    }
}