using Suity.Editor;
using Suity.Editor.Analyzing;
using Suity.Helpers;
using Suity.NodeQuery;
using Suity.Synchonizing.Core;
using Suity.Views;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Represents a node in a virtual tree structure with display capabilities.
/// </summary>
public partial class VirtualNode : ISupportStyle, IViewColor
{
    /// <summary>
    /// Gets or sets the style reader for this node.
    /// </summary>
    public INodeReader Styles { get; set; }

    /// <summary>
    /// Gets or sets a custom tag object associated with this node.
    /// </summary>
    public object Tag { get; set; }

    #region Displaying

    private string _propertyDescription;
    private TextStatus _propertyStatus;

    private string _text;
    private string _previewText;
    private Image _mainIcon;
    private Image _customIcon;
    private Image _previewIcon;
    private Image _statusIcon;
    private Color? _color;

    private string _fieldDisplayName;
    private bool _canEditText;
    private bool _canEditPreviewText;
    private TextStatus _textStatus;
    private TextStatus _previewTextStatus;

    /// <summary>
    /// Gets or sets the property description text for this node.
    /// </summary>
    public string PropertyDescription
    {
        get => _propertyDescription;
        set
        {
            if (_propertyDescription != value)
            {
                _propertyDescription = value;
                NotifyModel();
            }
        }
    }

    /// <summary>
    /// Gets or sets the status of the property text.
    /// </summary>
    public TextStatus PropertyStatus
    {
        get => _propertyStatus;
        set
        {
            if (_propertyStatus != value)
            {
                _propertyStatus = value;
                NotifyModel();
            }
        }
    }

    /// <summary>
    /// Gets or sets the description text for this node.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (_description != value)
            {
                _description = value;
                NotifyModel();
            }
        }
    }

    /// <summary>
    /// Gets or sets the icon for this node.
    /// </summary>
    public Image Icon
    {
        get => _icon ?? _mainIcon;
        set
        {
            if (_icon != value)
            {
                _icon = value?.ToIconSmall();
                NotifyModel();
            }
        }
    }

    /// <summary>
    /// Gets or sets the primary text displayed for this node.
    /// </summary>
    public string Text
    {
        get
        {
            if (!string.IsNullOrEmpty(_propertyDescription))
            {
                return _propertyDescription;
            }
            else
            {
                return _text ?? string.Empty;
            }
        }
        set
        {
            if (string.IsNullOrEmpty(_propertyDescription))
            {
                SetText(value);
                _text = GetText();
            }
        }
    }

    /// <summary>
    /// Gets or sets the preview text shown alongside the main text.
    /// </summary>
    public string PreviewText
    {
        get => _previewText;
        set
        {
            SetPreviewText(value);
            _previewText = GetPreviewText();
        }
    }

    /// <summary>
    /// Gets the main icon of this node.
    /// </summary>
    public Image MainIcon => _mainIcon;

    /// <summary>
    /// Gets the custom icon of this node.
    /// </summary>
    public Image CustomIcon => _customIcon;

    /// <summary>
    /// Gets the preview icon of this node.
    /// </summary>
    public Image PreviewIcon => _previewIcon;

    /// <summary>
    /// Gets the status icon of this node.
    /// </summary>
    public Image StatusIcon => _statusIcon;

    /// <inheritdoc/>
    public Color? ViewColor => _color;

    /// <summary>
    /// Gets the display name for the field.
    /// </summary>
    public string FieldDisplayName => _fieldDisplayName;

    /// <summary>
    /// Gets a value indicating whether the text can be edited.
    /// </summary>
    public bool CanEditText => _canEditText;

    /// <summary>
    /// Gets a value indicating whether the preview text can be edited.
    /// </summary>
    public bool CanEditPreviewText => _canEditPreviewText;

    /// <summary>
    /// Gets the status of the text.
    /// </summary>
    public TextStatus TextStatus => _textStatus;

    /// <summary>
    /// Gets the status of the preview text.
    /// </summary>
    public TextStatus PreviewTextStatus => _previewTextStatus;

    /// <summary>
    /// Gets the text that is actually displayed, preferring description over text.
    /// </summary>
    public string DisplayText
    {
        get
        {
            if (!string.IsNullOrEmpty(_description))
            {
                return _description;
            }
            else
            {
                return _text ?? string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets the description for this node. Override to provide custom description.
    /// </summary>
    protected virtual string GetDescription() => string.Empty;

    /// <summary>
    /// Gets the primary text for this node. Override to provide custom text.
    /// </summary>
    protected virtual string GetText()
    {
        //return !string.IsNullOrEmpty(_description) ? _description : _propertyName;
        return _propertyName;
    }

    /// <summary>
    /// Sets the primary text for this node. Override to support text editing.
    /// </summary>
    /// <param name="value">The new text value.</param>
    protected virtual void SetText(string value)
    {
    }

    /// <summary>
    /// Gets the preview text for this node. Override to provide custom preview text.
    /// </summary>
    protected virtual string GetPreviewText() => string.Empty;

    /// <summary>
    /// Sets the preview text for this node. Override to support preview text editing.
    /// </summary>
    /// <param name="value">The new preview text value.</param>
    protected virtual void SetPreviewText(string value)
    { }

    /// <summary>
    /// Gets the status of the preview text. Override to provide custom status.
    /// </summary>
    protected virtual TextStatus GetPreviewTextStatus() => TextStatus.Disabled;

    /// <summary>
    /// Gets the main icon for this node. Override to provide a custom icon.
    /// </summary>
    protected virtual Image GetMainIcon() => null;

    /// <summary>
    /// Gets the custom icon for this node. Override to provide a custom icon.
    /// </summary>
    protected virtual Image GetCustomIcon() => null;

    /// <summary>
    /// Gets the preview icon for this node. Override to provide a custom icon.
    /// </summary>
    protected virtual Image GetPreviewIcon() => null;

    /// <summary>
    /// Gets the display name for the field. Override to provide a custom name.
    /// </summary>
    protected virtual string GetFieldDisplayName() => string.Empty;

    /// <summary>
    /// Gets a value indicating whether the text can be edited. Override to enable text editing.
    /// </summary>
    protected virtual bool GetCanEditText() => false;

    /// <summary>
    /// Gets a value indicating whether the preview text can be edited. Override to enable preview text editing.
    /// </summary>
    protected virtual bool GetCanEditPreviewText() => false;

    /// <summary>
    /// Gets the status of the text. Override to provide custom status.
    /// </summary>
    protected virtual TextStatus GetTextStatus() => TextStatus.Normal;

    /// <summary>
    /// Gets the status icon based on the current text status.
    /// </summary>
    protected virtual Image GetStatusIcon() => _textStatus.ToStatusIcon();

    /// <summary>
    /// Gets the color for this node. Override to provide a custom color.
    /// </summary>
    protected virtual Color? GetColor() => null;

    private void UpdateDisplay()
    {
        var analysis = (DisplayedValue as ISupportAnalysis)?.Analysis;

        _description = GetDescription();
        _text = GetText();

        {
            _previewText = null;
            string customPreviewText = GetCustomPreviewText();
            if (customPreviewText != null)
            {
                _previewText = customPreviewText;
            }
            else if (!string.IsNullOrEmpty(analysis?.AnalysisText))
            {
                _previewText = analysis.AnalysisText;
                string p = GetPreviewText();
                if (!string.IsNullOrEmpty(p))
                {
                    _previewText = $"{_previewText} | {p}";
                }
            }
            if (string.IsNullOrWhiteSpace(_previewText))
            {
                _previewText = GetPreviewText();
            }
        }

        _fieldDisplayName = GetFieldDisplayName();
        _canEditText = GetCanEditText();
        _canEditPreviewText = GetCanEditPreviewText();

        if (DisplayedValue is IViewComment { IsComment: true } && _textStatus < TextStatus.Comment)
        {
            _textStatus = TextStatus.Comment;
        }
        else if (analysis != null && analysis.Status != TextStatus.Normal)
        {
            _textStatus = analysis.Status;
        }
        else if (analysis?.ResourceUseCount > 0)
        {
            _textStatus = TextStatus.ResourceUse;
        }
        //else if (analysis?.UserCodeCount > 0)
        //{
        //    _textStatus = TextStatus.UserCode;
        //}
        else
        {
            _textStatus = _propertyStatus != TextStatus.Normal ? _propertyStatus : GetTextStatus();
        }

        if (analysis?.ReferenceCount > 0)
        {
            _previewTextStatus = TextStatus.Anonymous;
        }
        else
        {
            _previewTextStatus = GetPreviewTextStatus();
        }

        _mainIcon = GetMainIcon()?.ToIconSmall();
        _customIcon = GetCustomIcon()?.ToIconSmall();
        _previewIcon = GetPreviewIcon()?.ToIconSmall();
        _statusIcon = GetStatusIcon()?.ToIconSmall();
        _color = GetColor();
    }

    #endregion

    #region Preview

    /// <summary>
    /// Gets the custom preview text by combining values from all preview paths.
    /// </summary>
    /// <returns>The combined preview text, or null if no preview is available.</returns>
    public string GetCustomPreviewText()
    {
        var model = FindModel();
        if (model is null || !model.CustomPreviewText)
        {
            return null;
        }

        var paths = model.PreviewPaths;
        if (paths is null || paths.Count == 0)
        {
            return null;
        }

        object[] values = paths.Select(path => GetCustomPreviewValue(path.Path)).ToArray();
        if (values.All(s => s is null))
        {
            return null;
        }

        string[] texts = new string[paths.Count];

        for (int i = 0; i < paths.Count; i++)
        {
            texts[i] = GetCustomPreviewText(paths[i], values[i]);
        }

        return string.Join(" | ", texts);
    }

    /// <summary>
    /// Gets the value at the specified sync path from the displayed value.
    /// </summary>
    /// <param name="path">The sync path to retrieve the value from.</param>
    /// <returns>The value at the specified path, or null if not found.</returns>
    public object GetCustomPreviewValue(SyncPath path)
    {
        if (Visitor.TryGetValueDeep(DisplayedValue, path, out object result))
        {
            return result;
        }
        else
        {
            return null;
        }
    }

    private string GetCustomPreviewText(PreviewPath previewPath, object value)
    {
        string propName;
        string text;

        if (EditorUtility.ShowAsDescription.Value)
        {
            propName = previewPath.DisplayName ?? previewPath.Name;
        }
        else
        {
            propName = previewPath.Name;
        }

        if (value is IPreviewDisplay previewDisplay)
        {
            text = previewDisplay.PreviewText ?? string.Empty;
        }
        else if (value != null)
        {
            text = value.ToString() ?? string.Empty;
        }
        else
        {
            text = null;
        }

        if (text is null)
        {
            return null;
        }
        else if (!string.IsNullOrEmpty(propName))
        {
            return $"{propName}:{text}";
        }
        else
        {
            return text;
        }
    }

    public Image GetCustomPreviewIcon()
    {
        return null;

        //PreviewPath previewPath = _model?.PreviewPath;
        //if (previewPath != null)
        //{
        //    return CoreIcon.Preview;
        //}
        //else
        //{
        //    return null;
        //}

        //object obj = GetCustomPreviewValue(previewPath.Path);
        //if (obj is null)
        //{
        //    return null;
        //}

        //if (obj is IPreviewDisplay previewDisplay)
        //{
        //    return EditorUtility.GetIcon(previewDisplay.PreviewIcon);
        //}
        //else
        //{
        //    return CoreIcon.Preview;
        //}
    }

    //public bool TryGetCustomPreviewValue(out object result)
    //{
    //    SyncPath path = _model?.PreviewPath;
    //    if (path is null)
    //    {
    //        result = null;
    //        return false;
    //    }

    //    object v = null;
    //    Visitor.TryGetValueDeep(DisplayedValue, path, out v);

    //    result = v ?? string.Empty;
    //    return true;
    //}

    #endregion
}