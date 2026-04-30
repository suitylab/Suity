using System.Drawing;

namespace Suity.Editor.VirtualTree.Nodes;

/// <summary>
/// A virtual tree node that displays and edits string values.
/// The preview text is directly editable and serves as the displayed value.
/// </summary>
public class StringNode : VirtualNode
{
    private string _value = string.Empty;
    private readonly Image _icon;

    /// <inheritdoc/>
    public override object DisplayedValue => _value;

    /// <inheritdoc/>
    protected override void OnGetValue()
    {
        object obj = GetValue();
        _value = obj != null ? obj.ToString() : string.Empty;
    }

    /// <inheritdoc/>
    protected override Image GetMainIcon() => _icon;

    /// <inheritdoc/>
    protected override string GetPreviewText() => _value;

    /// <inheritdoc/>
    protected override void SetPreviewText(string value)
    {
        _value = value;

        PerformSetValue();
        PerformGetValue();
    }

    /// <inheritdoc/>
    protected override bool GetCanEditPreviewText() => true;
}
