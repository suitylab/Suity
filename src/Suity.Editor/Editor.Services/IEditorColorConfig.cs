using Suity.Helpers;
using Suity.Views.Graphics;
using System.Drawing;

namespace Suity.Editor.Services;

/// <summary>
/// Color configuration interface for the editor.
/// </summary>
public interface IEditorColorConfig : IColorConfig
{
    /// <summary>
    /// Gets a color for a specific text status.
    /// </summary>
    /// <param name="status">The text status.</param>
    /// <returns>The color for the status.</returns>
    Color GetStatusColor(TextStatus status);
}

/// <summary>
/// Default color configuration for the editor.
/// </summary>
public class DefaultEditorColorConfig : DefaultColorConfig, IEditorColorConfig
{
    // Standard blue: 008EFF
    // Purple icon color: D9BAFF
    // Green icon color: 2FB688
    // Light blue: CBE4FF

    /// <summary>
    /// Gets the default instance of DefaultEditorColorConfig.
    /// </summary>
    public new static DefaultEditorColorConfig Default { get; } = new();

    /// <summary>
    /// Initializes a new instance of the DefaultEditorColorConfig class.
    /// </summary>
    public DefaultEditorColorConfig()
    {
    }

    private readonly Color _style_MdiBackground = ColorHelper.IntToColor(0x1B1B1B);
    private readonly Color _style_Background = ColorHelper.IntToColor(0x535353);
    private readonly Color _style_BackgroundInner = ColorHelper.IntToColor(0x424242);

    private readonly Color _style_Focus = ColorHelper.IntToColor(0x737373);
    private readonly Color _style_FocusInactive = ColorHelper.IntToColor(0x606060);

    private readonly Color _style_Button = ColorHelper.IntToColor(0x2693DD);

    private readonly Color _status_error = ColorHelper.IntToColor(0xFF6666);
    private readonly Color _status_warning = ColorHelper.IntToColor(0xFF9B00);
    private readonly Color _status_info = ColorHelper.IntToColor(0x008EFF/*0x789AFF*/);
    private readonly Color _status_comment = ColorHelper.IntToColor(0x27BB1E);
    private readonly Color _status_normal = ColorHelper.IntToColor(0xF0F0F0);
    private readonly Color _status_disabled = ColorHelper.IntToColor(0x808080);
    private readonly Color _status_anonlymouse = ColorHelper.IntToColor(0x008EFF);

    private readonly Color _status_import = ColorHelper.IntToColor(0x8DD0B2);
    private readonly Color _status_tag = ColorHelper.IntToColor(0x2C3671);
    private readonly Color _status_userCode = ColorHelper.IntToColor(0x7CB2D2);
    private readonly Color _status_resourceUse = ColorHelper.IntToColor(0x5D2E8C);

    private readonly Color _status_refA = ColorHelper.IntToColor(0x008EFF);
    private readonly Color _status_refB = ColorHelper.IntToColor(0xA784FF);
    private readonly Color _status_refC = ColorHelper.IntToColor(0x2FB688);

    //readonly Color _status_preview = Color.FromArgb(88, 210, 210);
    private readonly Color _status_preview = Color.Gray;

    /// <inheritdoc/>
    public virtual Color GetStatusColor(TextStatus status)
    {
        switch (status)
        {
            case TextStatus.Error:
            case TextStatus.Denied:
                return _status_error;

            case TextStatus.Warning:
                return _status_warning;

            case TextStatus.Info:
                return _status_info;

            case TextStatus.Comment:
                return _status_comment;

            case TextStatus.Anonymous:
                return _status_anonlymouse;

            case TextStatus.Disabled:
                return _status_disabled;

            case TextStatus.Import:
                return _status_import;

            case TextStatus.Tag:
                return _status_tag;

            case TextStatus.UserCode:
                return _status_userCode;

            case TextStatus.ResourceUse:
                return _status_resourceUse;

            case TextStatus.Reference:
                return _status_refA;

            case TextStatus.FileReference:
                return _status_refB;

            case TextStatus.EnumReference:
                return _status_refC;

            case TextStatus.Add:
                return _status_refA;

            case TextStatus.Remove:
                return _status_refA;

            case TextStatus.Modify:
                return _status_refA;

            case TextStatus.Preview:
                return _status_preview;

            case TextStatus.Normal:
            default:
                return _status_normal;
        }
    }
}