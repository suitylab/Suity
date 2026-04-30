using Suity.Collections;
using Suity.Editor.Documents;
using Suity.Editor.Design;
using Suity.Editor.Services;
using Suity.Editor.Transferring;
using Suity.Json;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.AIGC;

/// <summary>
/// Defines the scope of data to be read when exporting content for AIGC processing.
/// </summary>
public enum ReadScopes
{
    /// <summary>
    /// Includes all information without any filtering.
    /// </summary>
    [DisplayText("All Information")]
    All,

    /// <summary>
    /// Includes only fields explicitly marked as readable by AIGC.
    /// </summary>
    [DisplayText("Include AIGC Readable")]
    AigcReadable,

    /// <summary>
    /// Excludes fields explicitly marked as not readable by AIGC, includes everything else.
    /// </summary>
    [DisplayText("Exclude AIGC Not Readable")]
    ExcludeAigcNotReadable,

    /// <summary>
    /// Includes only basic information, excluding detailed field data.
    /// </summary>
    [DisplayText("Basic Information")]
    Basic,
}

/// <summary>
/// Specifies the type of content to export for AIGC operations.
/// </summary>
public enum AigcExportContentTypes
{
    /// <summary>
    /// No content type specified.
    /// </summary>
    None,
    /// <summary>
    /// Export content for type editing operations.
    /// </summary>
    TypeEdit,
    /// <summary>
    /// Export content for data editing operations.
    /// </summary>
    DataEdit,
}

/// <summary>
/// Represents a set of attachments associated with a document for AIGC processing.
/// Manages the selection scope and provides functionality to export attachment data as JSON.
/// </summary>
public class AttachmentSet
{
    private static ReadScopes _lastScopeSelection = ReadScopes.All;

    /// <summary>
    /// Gets the document entry associated with this attachment set.
    /// </summary>
    public DocumentEntry Document { get; }

    private readonly HashSet<string> _attachments = [];
    private ReadScopes _readScope = ReadScopes.All;
    private readonly GuiDropDownValue _scopeSelect;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentSet"/> class with the specified document.
    /// Uses the last scope selection as the default read scope.
    /// </summary>
    /// <param name="document">The document entry to associate with this attachment set.</param>
    public AttachmentSet(DocumentEntry document)
        : this(document, _lastScopeSelection)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentSet"/> class with the specified document and read scope.
    /// </summary>
    /// <param name="document">The document entry to associate with this attachment set.</param>
    /// <param name="readScope">The scope of data to read when exporting content.</param>
    public AttachmentSet(DocumentEntry document, ReadScopes readScope)
    {
        Document = document ?? throw new ArgumentNullException(nameof(document));

        _readScope = readScope;

        _scopeSelect = new GuiDropDownValue();
        _scopeSelect.SetupEnumType(typeof(ReadScopes));
        _scopeSelect.SelectedValue = _readScope;
    }

    /// <summary>
    /// Adds a collection of attachment paths to this attachment set.
    /// </summary>
    /// <param name="attachments">The collection of attachment paths to add. Null values are ignored.</param>
    public void AddAttachments(IEnumerable<string> attachments)
    {
        if (attachments is null)
        {
            return;
        }

        _attachments.AddRange(attachments);
    }

    /// <summary>
    /// Gets or sets the scope of data to be read when exporting content for AIGC processing.
    /// </summary>
    public ReadScopes ReadScope
    {
        get => _readScope;
        set => _readScope = value;
    }

    /// <summary>
    /// Gets the full file path of the associated document.
    /// </summary>
    public string FullPath => Document.FileName.FullPath;

    /// <summary>
    /// Gets the number of attachments in this set.
    /// </summary>
    public int Count => _attachments.Count;

    /// <summary>
    /// Renders the GUI representation of this attachment set.
    /// </summary>
    /// <param name="gui">The ImGui instance used for rendering.</param>
    /// <param name="deleteAction">Optional action to invoke when the delete button is clicked.</param>
    public void OnGui(ImGui gui, Action<AttachmentSet> deleteAction = null)
    {
        gui.HorizontalLayout($"#attachment_{FullPath}")
        .InitFullWidth()
        .OnContent(() =>
        {
            gui.Image(Document.Content?.Icon ?? CoreIconCache.Attachment)
            .InitClass("icon");

            gui.Text("#name", Document.ToString())
            .InitClass("propInput");

            gui.Text("#count", $"({_attachments.Count} selected)")
            .InitClass("propInput");

            var modelSelect = gui.DropDownButton("scope_select")
            .SetDropDownValue(_scopeSelect)
            .InitWidth(200)
            .InitClass("propInput")
            .OnEdited(n =>
            {
                _lastScopeSelection = _readScope = (ReadScopes)_scopeSelect.SelectedValue;
            });

            if (deleteAction != null)
            {
                gui.Button("#delete", CoreIconCache.Delete)
                .InitClass("toolBtnTrans")
                .OnClick(() => deleteAction(this));
            }
        });
    }

    /// <summary>
    /// Generates a JSON representation of the attachment data based on the current read scope.
    /// </summary>
    /// <returns>A JSON string containing the exported attachment data, or null if export fails.</returns>
    public string GetJson()
    {
        if (Document.Content is not IMemberContainer container)
        {
            return null;
        }

        var transfer = ContentTransfer<DataRW>.GetTransfer(container.GetType());
        if (transfer is null)
        {
            return null;
        }

        try
        {
            var option = new JsonResourceOptions
            {
                FieldFilter = field =>
                {
                    if (field.Attributes.GetIsDisabled())
                    {
                        return false;
                    }

                    switch (_readScope)
                    {
                        case ReadScopes.Basic:
                            return false;

                        case ReadScopes.AigcReadable:
                            {
                                var aiReadable = field.GetAttribute<AigcReadAttribute>();
                                return aiReadable != null && aiReadable.ReadableType == AigcReadableTypes.Readable;
                            }

                        case ReadScopes.ExcludeAigcNotReadable:
                            {
                                var aiReadable = field.GetAttribute<AigcReadAttribute>();
                                return aiReadable is null || aiReadable.ReadableType != AigcReadableTypes.NotReadable;
                            }

                        case ReadScopes.All:
                        default:
                            return true;
                    }
                },
            };

            var attachments = _attachments.Select(n => container.GetMember(n)).SkipNull().ToArray();

            var writer = new JsonDataWriter();
            writer.Node("@format").WriteString("SuityJson");
            transfer.Output(Document.Content, new DataRW { Writer = writer, Options = option }, attachments);

            return writer.ToString(true);
        }
        catch (Exception err)
        {
            err.LogError("Failed to get code.");

            return null;
        }
    }

    /// <summary>
    /// Creates a shallow copy of this attachment set with the same document, read scope, and attachments.
    /// </summary>
    /// <returns>A new <see cref="AttachmentSet"/> instance with identical state.</returns>
    public AttachmentSet Clone()
    {
        var clone = new AttachmentSet(Document, _readScope);
        clone._attachments.AddRange(_attachments);

        return clone;
    }

    /// <summary>
    /// Returns a string representation of this attachment set, which is the document's string representation.
    /// </summary>
    /// <returns>A string representing the associated document.</returns>
    public override string ToString()
    {
        return Document.ToString();
    }
}