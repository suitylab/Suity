using HtmlAgilityPack;
using Suity.Editor.Flows;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Flows;

#region WebBrowseNode

/// <summary>
/// Node that browses a webpage via URL and retrieves its HTML or plain text content.
/// </summary>
[DisplayText("Browse Web", "*CoreIcon|Web")]
[ToolTipsText("Access a webpage via URL and get its HTML text")]
public class BrowseWeb : AigcFlowNode
{
    private FlowNodeConnector _in;
    private ConnectorStringProperty _url = new("Url", "Address");
    private ConnectorValueProperty<bool> _extractText = new("ExtractText", "Extract Text", true, "Extract only plain text, removing all other HTML tags.");

    private FlowNodeConnector _out;
    private FlowNodeConnector _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="BrowseWeb"/> class.
    /// </summary>
    public BrowseWeb()
    {
        _in = AddActionInputConnector("In", "Input");
        _url.AddConnector(this);
        _extractText.AddConnector(this);

        _out = AddActionOutputConnector("Out", "Output");
        _result = AddDataOutputConnector("Result", "string", "Result");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _url.Sync(sync);
        _extractText.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _url.InspectorField(setup, this);
        _extractText.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        string url = _url.GetValue(compute, this);

        if (string.IsNullOrWhiteSpace(url))
        {
            compute.SetValue(_result, string.Empty);
            return _out;
        }

        try
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();
                if (_extractText.GetValue(compute, this))
                {
                    content = ExtractText(content);
                }

                compute.SetValue(_result, content ?? string.Empty);
            }
        }
        catch (Exception err)
        {
            compute.AddLog(TextStatus.Error, err.Message);

            compute.SetValue(_result, string.Empty);
        }

        return _out;
    }

    /// <summary>
    /// Extracts plain text content from HTML string.
    /// </summary>
    /// <param name="content">The HTML content to extract text from.</param>
    /// <returns>The extracted plain text with extra whitespace removed.</returns>
    private static string ExtractText(string content)
    {
        HtmlDocument htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(content);

        StringBuilder builder = new StringBuilder();
        BuildInnerText(htmlDocument.DocumentNode, builder);
        string textContent = builder.ToString();

        textContent = RemoveExtraWhitespace(textContent);

        return textContent;
    }

    /// <summary>
    /// Removes extra whitespace from text, replacing consecutive whitespace with single spaces.
    /// </summary>
    /// <param name="text">The text to trim.</param>
    /// <returns>The trimmed text with normalized whitespace.</returns>
    private static string RemoveExtraWhitespace(string text)
    {
        // Use regex to replace consecutive whitespace with a single space
        string trimmedText = Regex.Replace(text, @"\s+", " ");

        // Trim leading and trailing spaces
        trimmedText = trimmedText.Trim();

        return trimmedText;
    }

    /// <summary>
    /// Recursively builds inner text from HTML node tree.
    /// </summary>
    /// <param name="node">The HTML node to process.</param>
    /// <param name="builder">The string builder to append text to.</param>
    /// <param name="separator">The separator to insert between nodes.</param>
    private static void BuildInnerText(HtmlNode node, StringBuilder builder, string separator = " ")
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            builder.Append(((HtmlTextNode)node).Text);
            return;
        }

        if (node.NodeType == HtmlNodeType.Comment)
        {
            builder.Append(((HtmlCommentNode)node).Comment);
            return;
        }

        // note: right now, this method is *slow*, because we recompute everything.
        // it could be optimized like innerhtml
        if (!node.HasChildNodes)
        {
            return;
        }

        foreach (HtmlNode childNode in node.ChildNodes)
        {
            BuildInnerText(childNode, builder, separator);
            builder.Append(separator);
        }
    }
}

#endregion
