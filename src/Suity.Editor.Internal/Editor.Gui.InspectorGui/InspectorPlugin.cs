using Suity.Editor.Services;
using Suity.Views.Im;
using System.Linq;

namespace Suity.Editor.Gui.InspectorGui;

/// <summary>
/// Editor plugin that manages the property inspector view lifecycle and state persistence.
/// </summary>
public class InspectorPlugin : EditorPlugin
{
    /// <summary>
    /// Shared expand/collapse state for the inspector tree view.
    /// </summary>
    internal static GuiExpandBackupState s_expandState = new();

    /// <inheritdoc/>
    public override string Description => "Property View";

    /// <inheritdoc/>
    internal protected override void AwakeProject()
    {
        base.AwakeProject();

        if (GetProjectState() is string[] states)
        {
            s_expandState.SetExpandedPaths(states);
        }
    }

    /// <inheritdoc/>
    internal protected override void StopProject()
    {
        base.StopProject();

        // push
        (EditorUtility.GetToolWindow<InspectorImGui>() as IInspector)?.InspectObject(null);
        // save
        string[] states = s_expandState.GetExpandedPaths().Select(o => o.ToString()).ToArray();

        SetProjectState(states);
    }
}