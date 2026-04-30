using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Views;
using System.Drawing;

namespace Suity.Editor.Documents.TypeEdit;

/// <summary>
/// Represents a label item in an enum item list, used for visual grouping.
/// </summary>
[NativeAlias]
[DisplayText("Label", "*CoreIcon|Label")]
public class EnumItemLabel : EnumItemBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumItemLabel"/> class.
    /// </summary>
    public EnumItemLabel()
    {
        Description = "Label";
    }

    /// <inheritdoc/>
    protected override string OnGetSuggestedPrefix() => "###Label";

    /// <inheritdoc/>
    protected override bool OnVerifyName(string name) => true;

    /// <inheritdoc/>
    protected override Image OnGetIcon() => CoreIconCache.Label;

    /// <inheritdoc/>
    protected override string OnGetDisplayText() => Description;

    /// <inheritdoc/>
    protected override TextStatus OnGetTextStatus() => TextStatus.Comment;

    /// <inheritdoc/>
    protected override bool OnCanEditText() => true;

    /// <inheritdoc/>
    protected override void OnSetText(string text, ISyncContext setup, bool showNotice)
    {
        if (Description == text)
        {
            return;
        }

        setup.DoServiceAction<IViewSetValue>(v => v.SetValue("Description", text));
    }

    /// <inheritdoc/>
    public override string ToString() => Description ?? base.ToString();
}
