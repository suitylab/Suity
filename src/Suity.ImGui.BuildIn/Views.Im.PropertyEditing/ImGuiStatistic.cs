
namespace Suity.Views.Im;

/// <summary>
/// Statistics tracking for ImGui rendering and input processing performance metrics.
/// Records various counters for frames, events, and function calls.
/// </summary>
public class ImGuiStatistic
{
    /// <summary>
    /// Gets or sets the number of input frames processed.
    /// </summary>
    public ulong InputFrame;

    /// <summary>
    /// Gets or sets the number of output frames rendered.
    /// </summary>
    public ulong OutputFrame;

    /// <summary>
    /// Gets or sets the total number of nodes created.
    /// </summary>
    public ulong NodeCreated;

    /// <summary>
    /// Gets or sets the number of timer event calls.
    /// </summary>
    public ulong TimerEventCall;

    /// <summary>
    /// Gets or sets the number of refresh event calls.
    /// </summary>
    public ulong RefreshEventCall;

    /// <summary>
    /// Gets or sets the number of base function set calls.
    /// </summary>
    public ulong SetFunctionBase;

    /// <summary>
    /// Gets or sets the number of input function calls.
    /// </summary>
    public ulong InputFunctionCall;

    /// <summary>
    /// Gets or sets the number of layout function calls.
    /// </summary>
    public ulong LayoutFunctionCall;

    /// <summary>
    /// Gets or sets the number of fit function calls.
    /// </summary>
    public ulong FitFunctionCall;

    /// <summary>
    /// Gets or sets the number of render function calls.
    /// </summary>
    public ulong RenderFunctionCall;

    /// <summary>
    /// Gets or sets the number of set value calls.
    /// </summary>
    public ulong SetValueCall;

    /// <summary>
    /// Gets or sets the number of get value calls.
    /// </summary>
    public ulong GetValueCall;

    /// <summary>
    /// Gets or sets the number of get style calls.
    /// </summary>
    public ulong GetStyleCall;

    /// <summary>
    /// Gets or sets the number of update values from style calls.
    /// </summary>
    public ulong UpdateValuesFromStyleCall;

    /// <summary>
    /// Gets or sets the number of style changes.
    /// </summary>
    public ulong StyleChanged;

    /// <summary>
    /// Gets or sets the number of pseudo state changes.
    /// </summary>
    public ulong PseudoChanged;

    /// <summary>
    /// Gets or sets the number of offset position deep calls.
    /// </summary>
    public ulong OffsetPositionDeepCall;

    /// <summary>
    /// Gets or sets the number of set children pseudo deep calls.
    /// </summary>
    public ulong SetChildrenPseudoDeepCall;

    /// <summary>
    /// Gets or sets the number of update input version calls.
    /// </summary>
    public ulong UpdateInputVersionCall;

    /// <summary>
    /// Gets or sets the number of set mouse state calls.
    /// </summary>
    public ulong SetMouseStateCall;

    /// <summary>
    /// Gets or sets the number of layout out of range occurrences.
    /// </summary>
    public ulong LayoutOutOfRange;
}
