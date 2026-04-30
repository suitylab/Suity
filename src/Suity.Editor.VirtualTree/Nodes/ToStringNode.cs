using Suity.Views;
using System;
using System.Drawing;

namespace Suity.Editor.VirtualTree.Nodes;

/// <summary>
/// A virtual tree node that displays an object's string representation via <see cref="object.ToString"/>.
/// Supports text editing, preview text, and icon display through various view interfaces.
/// </summary>
public class ToStringNode : VirtualNode
{
    private object _value;

    /// <inheritdoc/>
    public override object DisplayedValue => _value;

    /// <inheritdoc/>
    protected override void OnGetValue()
    {
        _value = GetValue();
    }

    /// <inheritdoc/>
    protected override string GetText()
    {
        try
        {
            return (_value as ITextDisplay)?.DisplayText ?? base.GetText();
        }
        catch (Exception)
        {
            return base.GetText();
        }
    }

    /// <inheritdoc/>
    protected override void SetText(string value)
    {
        if (_value is ITextEdit edit && edit.CanEditText)
        {
            edit.SetText(value, null);
        }
    }

    /// <inheritdoc/>
    protected override TextStatus GetTextStatus()
    {
        return _value is ITextDisplay ext ? ext.DisplayStatus : base.GetTextStatus();
    }

    /// <inheritdoc/>
    protected override Color? GetColor()
    {
        return (_value as IViewColor)?.ViewColor ?? base.GetColor();
    }

    /// <inheritdoc/>
    protected override Image GetMainIcon()
    {
        var icon = _value is ITextDisplay display ? EditorUtility.GetIcon(display.DisplayIcon) : null;

        return icon ?? base.GetMainIcon();
    }

    /// <inheritdoc/>
    protected override string GetPreviewText()
    {
        string customText = GetCustomPreviewText();
        if (customText != null)
        {
            return customText;
        }

        try
        {
            string preview = DisplayedValue is IPreviewDisplay ext ? ext.PreviewText : base.GetPreviewText();
            return preview ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    /// <inheritdoc/>
    protected override void SetPreviewText(string value)
    {
        IPreviewEdit edit = _value as IPreviewEdit;
        if (edit?.CanEditPreviewText == true)
        {
            edit.SetPreviewText(value, null);
        }
    }

    /// <inheritdoc/>
    protected override Image GetPreviewIcon()
    {
        var icon = _value is IPreviewDisplay display ? EditorUtility.GetIcon(display.PreviewIcon) : null;

        return icon ?? base.GetPreviewIcon();
    }

    /// <inheritdoc/>
    protected override TextStatus GetPreviewTextStatus()
    {
        return TextStatus.Disabled;
    }

    /// <inheritdoc/>
    protected override void OnDisposing(bool manually)
    {
        base.OnDisposing(manually);

        _value = null;
    }
}
