using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;

namespace Suity.Editor.AIGC;

/// <summary>
/// Specifies the readability mode for AIGC (AI Generated Content) processing.
/// </summary>
public enum AigcReadableTypes
{
    /// <summary>
    /// No readability mode is specified.
    /// </summary>
    None,
    /// <summary>
    /// Content is marked as readable by AIGC.
    /// </summary>
    Readable,
    /// <summary>
    /// Content is marked as not readable by AIGC.
    /// </summary>
    NotReadable,
}

/// <summary>
/// Attribute that specifies the AIGC readability settings for a type or member.
/// </summary>
[NativeType(CodeBase = "*AIGC", Name = "AigcRead", Description = "AIGC Read", Icon = "*CoreIcon|AI")]
public class AigcReadAttribute : DesignAttribute, IViewObject
{
    private AigcReadableTypes _readableType = AigcReadableTypes.None;

    /// <summary>
    /// Gets or sets the readability type for AIGC processing.
    /// </summary>
    public AigcReadableTypes ReadableType
    {
        get => _readableType;
        set => _readableType = value;
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _readableType = sync.Sync(nameof(ReadableType), _readableType, SyncFlag.None, AigcReadableTypes.None);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(_readableType, new ViewProperty(nameof(ReadableType), L("Read Mode")));
    }

    /// <summary>
    /// Returns a string representation of the AIGC readability setting.
    /// </summary>
    public override string ToString()
    {
        switch (_readableType)
        {
            case AigcReadableTypes.Readable:
                return L("AIGC: Read");

            case AigcReadableTypes.NotReadable:
                return L("AIGC: Not Read");

            case AigcReadableTypes.None:
            default:
                return "AIGC:---";
        }
    }
}
