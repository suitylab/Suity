using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using System.Drawing;

namespace Suity.Views.Named;

/// <summary>
/// Represents a comment item in the named item hierarchy, used for adding annotations.
/// </summary>
[NativeAlias("Comment", UseForSaving = true)]
[DisplayText("Comment", "*CoreIcon|Comment")]
public class NamedComment : NamedItem
{
    /// <summary>
    /// Gets or sets the comment text.
    /// </summary>
    public string Comment { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedComment"/> class with a default localized comment text.
    /// </summary>
    public NamedComment()
    {
        Comment = L("Comment");
    }

    /// <summary>
    /// Gets the suggested prefix for naming new comments.
    /// </summary>
    protected internal override string OnGetSuggestedPrefix() => "###Comment";

    /// <summary>
    /// Verifies if the specified name is valid. Comments always return true.
    /// </summary>
    /// <param name="name">The name to verify.</param>
    /// <returns>Always true.</returns>
    protected internal override bool OnVerifyName(string name) => true;

    /// <summary>
    /// Called during synchronization to sync the comment text.
    /// </summary>
    /// <param name="sync">The property synchronizer.</param>
    /// <param name="context">The sync context.</param>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        Comment = sync.Sync("Comment", Comment);
    }

    /// <summary>
    /// Gets the icon to display for this comment (always null).
    /// </summary>
    protected override Image OnGetIcon() => null;

    /// <summary>
    /// Gets the text status indicating this is a comment.
    /// </summary>
    protected override TextStatus OnGetTextStatus() => TextStatus.Comment;

    /// <summary>
    /// Gets the display text, which is the comment text.
    /// </summary>
    protected override string OnGetDisplayText() => Comment;

    /// <summary>
    /// Sets the comment text through the view service.
    /// </summary>
    /// <param name="text">The new comment text.</param>
    /// <param name="setup">The sync context.</param>
    /// <param name="showNotice">Whether to show a notice.</param>
    protected override void OnSetText(string text, ISyncContext setup, bool showNotice)
    {
        if (Comment == text)
        {
            return;
        }

        setup.DoServiceAction<IViewSetValue>(v => v.SetValue("Comment", text));
    }

    /// <summary>
    /// Returns a string representation of this comment.
    /// </summary>
    public override string ToString() => Comment;
}
