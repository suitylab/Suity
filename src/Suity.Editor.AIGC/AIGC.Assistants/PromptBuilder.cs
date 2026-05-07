using Suity.Collections;
using Suity.Editor.Services;
using Suity.Editor.Types;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Builds prompt messages from templates with placeholder replacement support.
/// </summary>
[NativeType(CodeBase = "AIGC", Description = "Prompt Builder", Icon = "*CoreIcon|Prompt")]
public class PromptBuilder
{
    private readonly string _template;

    private readonly Func<AIPromptRecord> _templateProvider;
    private readonly string _templateName;

    private readonly List<KeyValuePair<string, string>> _replacements = [];
    private readonly HashSet<string> _hashCheck = [];

    private List<string> _appendLine;

    private string _missingContent = "---";
    private string _buildResult;
    private bool _containsMissingKeywords;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptBuilder"/> class with a template string.
    /// </summary>
    /// <param name="template">The prompt template string containing placeholders.</param>
    public PromptBuilder(string template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            throw new ArgumentException(L($"\"{nameof(template)}\" cannot be null or whitespace."), nameof(template));
        }

        _template = template;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PromptBuilder"/> class with a template provider function.
    /// </summary>
    /// <param name="templateProvider">A function that provides the prompt template record.</param>
    /// <param name="templateName">Optional name of the template for identification.</param>
    public PromptBuilder(Func<AIPromptRecord> templateProvider, string templateName = null)
    {
        _templateProvider = templateProvider ?? throw new ArgumentNullException(nameof(templateProvider));
        _templateName = templateName;
    }

    /// <summary>
    /// Gets the name of the template.
    /// </summary>
    public string TemplateName => _templateName;

    /// <summary>
    /// Gets or sets the default replacement for missing content. If there are unmatched placeholders in the prompt, this content will be used for replacement.
    /// </summary>
    public string MissingContent
    {
        get => _missingContent;
        set
        {
            _missingContent = value;
            _buildResult = null;
        }
    }

    /// <summary>
    /// Gets whether the built prompt contains unmatched placeholders (i.e., content in the form {{...}}).
    /// </summary>
    public bool ContainsMissingKeywords => _containsMissingKeywords;

    /// <summary>
    /// Replaces a placeholder key with the specified text value.
    /// </summary>
    /// <param name="key">The placeholder key to replace.</param>
    /// <param name="text">The replacement text.</param>
    /// <param name="emptyNotice">The text to use when the replacement text is null or whitespace.</param>
    /// <returns>The current <see cref="PromptBuilder"/> instance for chaining.</returns>
    public PromptBuilder Replace(string key, string text, string emptyNotice = "---") 
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            text = emptyNotice;
        }

        _buildResult = null;
        return Replace(key, (object)text);
    }

    /// <summary>
    /// Replaces a placeholder key with the specified object value converted to string.
    /// </summary>
    /// <param name="key">The placeholder key to replace.</param>
    /// <param name="value">The replacement value.</param>
    /// <returns>The current <see cref="PromptBuilder"/> instance for chaining.</returns>
    public PromptBuilder Replace(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (_hashCheck.Contains(key))
        {
            throw new InvalidOperationException(L("Duplicate replacement key: ") + key);
        }

        // Trim before checking
        string trimmed = key.Trim();

        bool startsWithBraces = trimmed.StartsWith("{{");
        bool endsWithBraces = trimmed.EndsWith("}}");

        if (!startsWithBraces)
        {
            trimmed = "{{" + trimmed;
        }

        if (!endsWithBraces)
        {
            trimmed = trimmed + "}}";
        }

        string strValue = value?.ToString() ?? string.Empty;
        _replacements.Add(new KeyValuePair<string, string>(trimmed, strValue));
        _hashCheck.Add(key);
        _buildResult = null;

        return this;
    }

    /// <summary>
    /// Appends a line to the end of the prompt template.
    /// </summary>
    /// <param name="line">The line to append.</param>
    /// <returns>The current <see cref="PromptBuilder"/> instance for chaining.</returns>
    public PromptBuilder AppendLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            line = string.Empty;
        }

        (_appendLine ??= []).Add(line);
        _buildResult = null;

        return this;
    }

    /// <summary>
    /// Gets the prompt record from the template provider, if available.
    /// </summary>
    /// <returns>An <see cref="AIPromptRecord"/> from the provider, or null if no provider is set.</returns>
    public AIPromptRecord GetPromptRecord() => _templateProvider?.Invoke();

    /// <summary>
    /// Creates a clone of this prompt builder with all current replacements and settings.
    /// </summary>
    /// <returns>A new <see cref="PromptBuilder"/> instance with the same configuration.</returns>
    public PromptBuilder Clone()
    {
        PromptBuilder other;

        if (!string.IsNullOrWhiteSpace(_template))
        {
            other = new(_template);
        }
        else
        {
            other = new(_templateProvider, _templateName);
        }

        other._replacements.AddRange(_replacements);
        other._hashCheck.AddRange(_hashCheck);

        if (_appendLine != null)
        {
            other._appendLine = [.._appendLine];
        }

        return other;
    }

    /// <summary>
    /// Builds and returns the final prompt string with all replacements applied.
    /// </summary>
    /// <returns>The fully built prompt string.</returns>
    public override string ToString()
    {
        _buildResult ??= InternalBuild();
        return _buildResult;
    }
    
    private string InternalBuild()
    {
        _containsMissingKeywords = false;

        string template = _template ?? _templateProvider?.Invoke()?.Prompt;
        if (string.IsNullOrWhiteSpace(template))
        {
            throw new AigcException(L("Failed to get prompt template."));
        }

        var builder = new StringBuilder(template);
        if (_appendLine != null)
        {
            foreach (var line in _appendLine)
            {
                builder.AppendLine(line);
            }
        }

        foreach (var item in _replacements)
        {
            builder.Replace(item.Key, item.Value);
        }

        string result = builder.ToString();

        var missingKeywords = ExtractKeywords(result);
        if (missingKeywords.Length == 0)
        {
            return result;
        }

        if (string.IsNullOrWhiteSpace(_missingContent))
        {
            throw new InvalidOperationException($"Prompt contains unmatched double brackets: {string.Join(", ", missingKeywords)}. Template name: {_templateName}.");
        }

        foreach (string keyword in missingKeywords)
        {
            builder.Replace("{{" + keyword + "}}", _missingContent);
        }

        _containsMissingKeywords = true;
        result = builder.ToString();
        return result;
    }

    /// <summary>
    /// Extracts all placeholder keywords from the input string that are enclosed in double braces {{...}}.
    /// </summary>
    /// <param name="input">The input string to search for placeholders.</param>
    /// <returns>An array of unique placeholder keywords found in the input.</returns>
    public static string[] ExtractKeywords(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return [];
        }

        // Regex to match content in {{...}}, non-greedy match
        var matches = Regex.Matches(input, @"\{\{(.*?)\}\}");

        var resultSet = new HashSet<string>(); // Used for deduplication

        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                resultSet.Add(match.Groups[1].Value);
            }
        }

        return [..resultSet];
    }

    /// <summary>
    /// Creates a new <see cref="PromptBuilder"/> from a template identified by the given prompt ID.
    /// </summary>
    /// <param name="promptId">The ID of the prompt template to load.</param>
    /// <returns>A new <see cref="PromptBuilder"/> instance configured with the specified template.</returns>
    public static PromptBuilder FromTemplate(string promptId)
    {
        AIPromptRecord getTemplate()
        {
            return AIAssistantService.Instance.GetPromptRecordOrThrow(promptId);
        }


        return new PromptBuilder(getTemplate, promptId);
    }
}

/// <summary>
/// Converts a <see cref="PromptBuilder"/> to an <see cref="LLmMessage"/>.
/// </summary>
public class PromptBuilderToLLmMessageConverter : TypeConverter<PromptBuilder, LLmMessage>
{
    /// <summary>
    /// Converts the prompt builder to an LLM message with the user role.
    /// </summary>
    /// <param name="objFrom">The prompt builder to convert.</param>
    /// <returns>An <see cref="LLmMessage"/> containing the built prompt text.</returns>
    public override LLmMessage Convert(PromptBuilder objFrom)
    {
        string content = objFrom.ToString();

        return new LLmMessage
        {
            Role = LLmMessageRole.User,
            Message = content,
        };
    }
}

/// <summary>
/// Converts a <see cref="PromptBuilder"/> to a plain text string.
/// </summary>
public class PromptBuilderToTextConverter : TypeToTextConverter<PromptBuilder>
{
    /// <summary>
    /// Converts the prompt builder to its string representation.
    /// </summary>
    /// <param name="objFrom">The prompt builder to convert.</param>
    /// <returns>The built prompt string.</returns>
    public override string Convert(PromptBuilder objFrom)
    {
        return objFrom.ToString();
    }
}

/// <summary>
/// Converts a <see cref="PromptAsset"/> to a <see cref="PromptBuilder"/>.
/// </summary>
public class PromptAssetToPromptBuilderConverter : ITypeDefinitionConverter
{
    /// <summary>
    /// Gets the source type definitions that this converter can handle.
    /// </summary>
    public TypeDefinition[] TypesFrom => [TypeDefinition.FromAssetLink<PromptAsset>()];

    /// <summary>
    /// Gets the target type definitions that this converter produces.
    /// </summary>
    public TypeDefinition[] TypesTo => [TypeDefinition.FromNative<PromptBuilder>()];

    /// <summary>
    /// Converts a prompt asset to a prompt builder.
    /// </summary>
    /// <param name="objFrom">The source object to convert.</param>
    /// <param name="typeTo">The target type definition.</param>
    /// <returns>A <see cref="PromptBuilder"/> created from the prompt asset, or null if conversion fails.</returns>
    public object ConvertType(object objFrom, TypeDefinition typeTo)
    {
        if (objFrom is PromptAsset asset)
        {
            return asset.CreatePromptBuilder();
        }

        return null;
    }
}
