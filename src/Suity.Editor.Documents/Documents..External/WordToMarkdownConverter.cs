using NPOI.XWPF.UserModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Suity.Editor.Documents.External;

/// <summary>
/// Provides methods to convert Word (.docx) documents to Markdown format.
/// </summary>
public static class WordToMarkdownConverter
{
    /// <summary>
    /// Converts a Word document to Markdown format from a storage location.
    /// </summary>
    /// <param name="fileName">The storage location of the Word document.</param>
    /// <returns>The Markdown representation of the document.</returns>
    public static string ConvertDocxToMarkdown(StorageLocation fileName)
    {
        StringBuilder sb = new StringBuilder();

        using (var stroage = fileName.GetStorageItem())
        using (var fs = stroage.GetInputStream())
        {
            XWPFDocument doc = new XWPFDocument(fs);

            // Iterate through all body elements in the document (paragraphs and tables)
            foreach (var element in doc.BodyElements)
            {
                if (element is XWPFParagraph paragraph)
                {
                    ProcessParagraph(paragraph, sb);
                }
                else if (element is XWPFTable table)
                {
                    ProcessTable(table, sb);
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Converts a Word document to Markdown format from a file path.
    /// </summary>
    /// <param name="filePath">The file path to the Word document.</param>
    /// <returns>The Markdown representation of the document.</returns>
    public static string ConvertDocxToMarkdown(string filePath)
    {
        StringBuilder sb = new StringBuilder();

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            XWPFDocument doc = new XWPFDocument(fs);

            // Iterate through all body elements in the document (paragraphs and tables)
            foreach (var element in doc.BodyElements)
            {
                if (element is XWPFParagraph paragraph)
                {
                    ProcessParagraph(paragraph, sb);
                }
                else if (element is XWPFTable table)
                {
                    ProcessTable(table, sb);
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Processes a paragraph and appends its Markdown representation to the string builder.
    /// </summary>
    /// <param name="para">The paragraph to process.</param>
    /// <param name="sb">The string builder to append to.</param>
    public static void ProcessParagraph(XWPFParagraph para, StringBuilder sb)
    {
        string text = para.ParagraphText;
        if (string.IsNullOrWhiteSpace(text)) return;

        // Process headings (check StyleID, typically Heading 1 corresponds to #)
        string style = para.Style;
        if (!string.IsNullOrEmpty(style))
        {
            if (style.Contains("1")) sb.Append("# ");
            else if (style.Contains("2")) sb.Append("## ");
            else if (style.Contains("3")) sb.Append("### ");
            else if (style.Contains("4")) sb.Append("#### ");
        }

        // Process bold/italic (simplified: iterate through Run)
        // Note: For complex formatting, recommend extracting Text directly, this example simply appends
        sb.AppendLine(text);
        sb.AppendLine(); // Markdown paragraphs need blank lines between them
    }

    /// <summary>
    /// Processes a table and appends its Markdown representation to the string builder.
    /// </summary>
    /// <param name="table">The table to process.</param>
    /// <param name="sb">The string builder to append to.</param>
    public static void ProcessTable(XWPFTable table, StringBuilder sb)
    {
        for (int i = 0; i < table.Rows.Count; i++)
        {
            var row = table.Rows[i];
            var cells = row.GetTableCells().Select(c => c.GetText().Replace("\n", " "));

            sb.AppendLine("| " + string.Join(" | ", cells) + " |");

            // If first row, add Markdown table separator
            if (i == 0)
            {
                var separators = cells.Select(_ => "---");
                sb.AppendLine("| " + string.Join(" | ", separators) + " |");
            }
        }
        sb.AppendLine();
    }
}