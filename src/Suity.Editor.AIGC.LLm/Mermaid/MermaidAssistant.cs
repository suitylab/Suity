using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Services;
using System;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Mermaid;

/// <summary>
/// Represents the options for generating a Mermaid diagram.
/// </summary>
public class MermaidOption
{
    /// <summary>
    /// Gets or sets the type of Mermaid graph to generate.
    /// </summary>
    public MermaidGraphType GraphType { get; set; }

    /// <summary>
    /// Gets or sets the source content to generate the diagram from.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the user-provided message or additional instructions for diagram generation.
    /// </summary>
    public string UserMessage { get; set; }
}

/// <summary>
/// An AI assistant specialized in generating Mermaid diagram code from content.
/// </summary>
[DisplayText("Chart Assistant")]
public class MermaidAssistant : AIAssistant
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MermaidAssistant"/> class.
    /// </summary>
    public MermaidAssistant()
    {
    }

    /// <inheritdoc/>
    public override async Task<AICallResult> HandleRequest(AIRequest request)
    {
        if ((request.Option as AIAssistantOption)?.Option is not MermaidOption option)
        {
            return AICallResult.Empty;
        }

        if (string.IsNullOrWhiteSpace(option.Content))
        {
            return AICallResult.Empty;
        }

        string result = await GenerateMermaid(request, option);

        if (string.IsNullOrWhiteSpace(result))
        {
            return AICallResult.Empty;
        }

        result = result.Trim();

        if (!result.StartsWith("```mermaid", StringComparison.OrdinalIgnoreCase))
        {
            result = "```mermaid\r\n" + result;
        }

        if (!result.EndsWith("```", StringComparison.OrdinalIgnoreCase))
        {
            result += "\r\n```";
        }

        return AICallResult.FromResult(result);
    }

    /// <summary>
    /// Generates Mermaid diagram code based on the specified request and options.
    /// </summary>
    /// <param name="request">The AI request containing conversation and context.</param>
    /// <param name="option">The Mermaid-specific options for diagram generation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the generated Mermaid code as a string.</returns>
    public static async Task<string> GenerateMermaid(AIRequest request, MermaidOption option)
    {
        if (string.IsNullOrWhiteSpace(option?.Content))
        {
            return string.Empty;
        }

        string promptId = $"Mermaid.{option.GraphType}";
        var builder = PromptBuilder.FromTemplate(promptId);
        if (builder is null)
        {
            return null;
        }

        request.FillPrompt(builder);
        builder.Replace(TAG.CONTENT, option.Content);
        builder.Replace(TAG.RULE, option.UserMessage);

        string prompt = builder.ToString();

        var call = request.CreateLLmCall(builder);
        var callReq = new LLmCallRequest(prompt)
        {
            Conversation = request.Conversation,
            Cancel = request.Cancellation,
            Title = "Mermaid",
        };

        string result = await call.Call(callReq);

        return result;
    }
}


#region Prompt
/// <summary>
/// Defines the AI prompt template for generating Mermaid flowchart diagrams.
/// </summary>
public class Mermaid_Flowchart : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Mermaid.Flowchart";

    /// <inheritdoc/>
    public override string Description => "Flowchart Generation";

    /// <inheritdoc/>
    public override string Prompt => @"
You are skilled in generating mermaid flowchart.
Your task is to create a detailed mermaid flowchart based on the given document.

# The document is as follow:
{{CONTENT}}

# Additional Instructions:
{{RULE}}

# Notice:
- Output the flowchart with official mermaid flowchart format.
- Output speech language is: {{SPEECH_LANGUAGE}}.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.DesignWriting;
}

/// <summary>
/// Defines the AI prompt template for generating Mermaid mind map diagrams.
/// </summary>
public class Mermaid_MindMap : AIPrompt
{
    /// <inheritdoc/>
    public override string PromptId => "Mermaid.Mindmap";

    /// <inheritdoc/>
    public override string Description => "Mind Map Generation";

    /// <inheritdoc/>
    public override string Prompt => @"
You are skilled in generating mermaid mind map.
Your task is to create a detailed mermaid mind map based on the given document.

# The document is as follow:
{{DOCUMENT}}

# Notice:
- Output the mind map with official mermaid mind map format.
- Output diagram content only and nothing else.
- Output speech language is: {{SPEECH_LANGUAGE}}.
";

    /// <inheritdoc/>
    public override LLmModelPreset ModelPreset => LLmModelPreset.DesignWriting;
}
#endregion