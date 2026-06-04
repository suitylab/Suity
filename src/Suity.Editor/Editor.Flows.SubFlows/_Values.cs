using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.IO;

namespace Suity.Editor.Flows.SubFlows;

/// <summary>
/// Represents the types of events that can occur during an Sub-flow task lifecycle.
/// </summary>
public enum TaskEventTypes
{
    /// <summary>
    /// No event.
    /// </summary>
    [DisplayText("None")]
    None,

    /// <summary>
    /// Indicates that a task has begun.
    /// </summary>
    [DisplayText("Task Start")]
    TaskBegin,

    /// <summary>
    /// Indicates that a subtask has completed successfully.
    /// </summary>
    [DisplayText("Subtask Completed")]
    SubTaskFinished,

    /// <summary>
    /// Indicates that a subtask has failed.
    /// </summary>
    [DisplayText("Subtask Failed")]
    SubTaskFailed,
}

/// <summary>
/// Defines the possible statuses for a task commit, indicating the outcome of a task or subtask.
/// </summary>
public enum TaskCommitStatus
{
    /// <summary>
    /// No commit type specified.
    /// </summary>
    [DisplayText("None")]
    None,

    /// <summary>
    /// Indicates the task has finished successfully.
    /// </summary>
    [DisplayText("Task Finished")]
    TaskFinished,

    /// <summary>
    /// Indicates the task has failed.
    /// </summary>
    [DisplayText("Task Failed")]
    TaskFailed,

    /// <summary>
    /// Indicates the task has been disabled and will not execute.
    /// </summary>
    [DisplayText("Task Disabled")]
    TaskDisabled,
}

public record TaskCommitParameter(TaskCommitStatus EndType, object Value);

/// <summary>
/// Represents a wrapper for history text content with implicit conversion support.
/// </summary>
[NativeType(CodeBase = "SubFlow")]
[NativeAlias("*AIGC|ChatHistoryText")]
public record HistoryText
{
    /// <summary>
    /// Gets an empty <see cref="HistoryText"/> instance.
    /// </summary>
    public static HistoryText Empty { get; } = new(string.Empty);

    /// <summary>
    /// Gets the underlying text content.
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="HistoryText"/> with the specified text.
    /// </summary>
    public HistoryText(string text)
    {
        Text = text ?? string.Empty;
    }

    /// <summary>
    /// Returns the text content as a string representation.
    /// </summary>
    public override string ToString()
    {
        return Text;
    }

    /// <summary>
    /// Implicitly converts a <see cref="HistoryText"/> to a <see cref="string"/>.
    /// </summary>
    public static implicit operator string(HistoryText text)
    {
        return text?.Text;
    }

    /// <summary>
    /// Implicitly converts a <see cref="string"/> to a <see cref="HistoryText"/>. Returns <see cref="Empty"/> if the text is null or whitespace.
    /// </summary>
    public static implicit operator HistoryText(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            return new HistoryText(text);
        }
        else
        {
            return Empty;
        }
    }
}

public record HistoryTag
{
    public static HistoryTag Empty { get; } = new(string.Empty, null);

    public HistoryTag()
    {
    }

    public HistoryTag(HistoryText text)
    {
        Text = text;
    }

    public HistoryTag(HistoryText text, List<KeyValuePair<string, string>> attributes)
    {
        Text = text;
        Attributes = attributes;
    }


    public HistoryText Text { get; }

    public List<KeyValuePair<string, string>> Attributes { get; }


    public override string ToString() => Text;

    public static implicit operator string(HistoryTag text)
    {
        return text?.Text;
    }

    public static implicit operator HistoryText(HistoryTag text)
    {
        return text?.Text;
    }

    public static implicit operator HistoryTag(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            return new HistoryTag(new HistoryText(text));
        }
        else
        {
            return Empty;
        }
    }

    public static implicit operator HistoryTag(HistoryText text)
    {
        if (!string.IsNullOrWhiteSpace(text.Text))
        {
            return new HistoryTag(text);
        }
        else
        {
            return Empty;
        }
    }
}

public enum ResolveChatIntents
{
    Normal,
    Preview,
}

[NativeType(CodeBase = "SubFlow", Icon = "*CoreIcon|Scratch")]
[DisplayText("Scratch Pad Type")]
public enum ScratchPadTypes
{
    [DisplayText("Memory")]
    Memory,

    [DisplayText("File Full Content")]
    FileFullContent,

    [DisplayText("File Segment")]
    FileSegment,

    [DisplayText("File Edit")]
    FileEdit,

    [DisplayText("Article")]
    Article,

    [DisplayText("Removed")]
    Removed,

    [DisplayText("Not Found")]
    NotFound,

    [DisplayText("Clear")]
    Clear,

    [DisplayText("Encounter Error")]
    Error,
}

[NativeType(CodeBase = "SubFlow", Icon = "*CoreIcon|Scratch")]
[DisplayText("Scratch Pad")]
public class ScratchPad : DesignAttribute, ITextDisplay
{
    private readonly StringProperty _path = new(nameof(Path), "Path", null, "Path of the scratch pad item");
    private readonly ValueProperty<ScratchPadTypes> _type = new(nameof(Type), "Type", ScratchPadTypes.Memory, "Type of the scratch pad item");
    private readonly StringProperty _note = new(nameof(Note), "Note", null, "Note of the scratch pad item");
    private readonly TextBlockProperty _content = new(nameof(Content), "Content", null, "Content of the scratch pad item");

    public string Path { get => _path.Text; set => _path.Text = value; }
    public ScratchPadTypes Type { get => _type.Value; set => _type.Value = value; }
    public string Note { get => _note.Text; set => _note.Text = value; }
    public string Content { get => _content.Text; set => _content.Text = value; }

    #region ITextDisplay

    public string DisplayText => "Scratch Pad";

    public object DisplayIcon
    {
        get
        {
            switch (_type.Value)
            {
                case ScratchPadTypes.Memory:
                    return CoreIconCache.Memory;

                case ScratchPadTypes.FileFullContent:
                    return CoreIconCache.File;

                case ScratchPadTypes.FileSegment:
                    return CoreIconCache.Search;

                case ScratchPadTypes.FileEdit:
                    return CoreIconCache.Edit;

                case ScratchPadTypes.Article:
                    return CoreIconCache.Article;

                case ScratchPadTypes.Removed:
                    return CoreIconCache.Remove;

                case ScratchPadTypes.NotFound:
                    return CoreIconCache.Question;

                case ScratchPadTypes.Clear:
                    return CoreIconCache.Cleanup;

                case ScratchPadTypes.Error:
                default:
                    return CoreIconCache.Error;
            }
        }
    }

    public TextStatus DisplayStatus => TextStatus.Normal;

    #endregion
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _path.Sync(sync);
        _type.Sync(sync);
        _note.Sync(sync);
        _content.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _path.InspectorField(setup);
        _type.InspectorField(setup);
        _note.InspectorField(setup);
        _content.InspectorField(setup);
    }

    public override string ToString()
    {
        return $"[{Type.ToDisplayTextL()}] {Path}";
    }

    public string ToXmlTag(string basePath)
    {
        string path = Path?.Trim();

        string content = null;
        ScratchPadTypes type = Type;

        string note = Note;

        switch (type)
        {
            case ScratchPadTypes.Memory:
                content = Content;
                break;

            case ScratchPadTypes.FileFullContent:
                if (string.IsNullOrWhiteSpace(basePath))
                {
                    note = "Get file content failed due to Workspace missing.";
                    break;
                }

                try
                {
                    var fullPath = basePath.PathAppend(path);
                    if (File.Exists(fullPath))
                    {
                        content = File.ReadAllText(fullPath);
                    }
                    else
                    {
                        type = ScratchPadTypes.NotFound;
                    }
                }
                catch (Exception e)
                {
                    content = e.Message;
                    type = ScratchPadTypes.Error;
                }
                break;

            case ScratchPadTypes.FileSegment:
                content = Content;
                break;

            case ScratchPadTypes.FileEdit:
                content = Content;
                break;

            case ScratchPadTypes.Article:
                {
                    var article = AssetManager.Instance.GetAssetByResourceName<IArticleAsset>(path);
                    if (article != null)
                    {
                        content = article?.GetFullText();
                    }
                    else
                    {
                        type = ScratchPadTypes.NotFound;
                        content = null;
                    }
                }
                break;

            case ScratchPadTypes.Removed:
            default:
                content = null;
                break;
        }


        return $"<ScratchPad type='{type}' path='{path}' note='{note}'>\r\n{content}\r\n</ScratchPad>";
    }

}

[NativeType(CodeBase = "SubFlow", Icon = "*CoreIcon|Scratch")]
[DisplayText("Commit Scratch Pad")]
public class CommitScratchPad : DesignAttribute
{
}