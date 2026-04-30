using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.ProjectGui.Nodes;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Views.Menu;

namespace Suity.Editor.ProjectGui.Commands.WorkSpaces;

/// <summary>
/// Command to add an assembly reference to a workspace.
/// </summary>
internal class AddWsAssemblyRefCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddWsAssemblyRefCommand"/> class.
    /// </summary>
    public AddWsAssemblyRefCommand()
        : base("Add Assembly Reference", CoreIconCache.Assembly.ToIconSmall())
    {
        AcceptType<WorkSpaceAssemblyGroupNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = (WorkSpaceAssemblyGroupNode)view.SelectedNode;
        var asmRef = await DialogUtility.ShowAssetSelectionGUIAsync<IAssemblyReference>(L("Add Assembly Reference"));
        if (asmRef != null)
        {
            node.WorkSpace.AddAssemblyReference(asmRef.Id);
        }
        node.PopulateUpdate();
    }
}

/// <summary>
/// Command to add a system assembly reference to a workspace.
/// </summary>
internal class AddWsSystemReferenceCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddWsSystemReferenceCommand"/> class.
    /// </summary>
    public AddWsSystemReferenceCommand()
        : base("Add System Reference", CoreIconCache.System.ToIconSmall())
    {
        AcceptType<WorkSpaceAssemblyGroupNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = (WorkSpaceAssemblyGroupNode)view.SelectedNode;

        string asmName = await DialogUtility.ShowSingleLineTextDialogAsyncL("Add System Reference", "System", s =>
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                // DialogUtility.ShowMessageBoxAsyncL("Name cannot be empty");
                return false;
            }

            return true;
        });

        if (string.IsNullOrEmpty(asmName))
        {
            return;
        }

        asmName = asmName.Trim();

        node.WorkSpace.AddSystemAssemblyReference(asmName);
        node.PopulateUpdate();
    }
}

/// <summary>
/// Command to add a disabled reference to a workspace.
/// </summary>
internal class AddWsDisabledReferenceCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddWsDisabledReferenceCommand"/> class.
    /// </summary>
    public AddWsDisabledReferenceCommand()
        : base("Add Disabled Reference", CoreIconCache.Disable.ToIconSmall())
    {
        AcceptType<WorkSpaceAssemblyGroupNode>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override async void DoCommand()
    {
        if (Sender is not IProjectGui view)
        {
            return;
        }

        var node = (WorkSpaceAssemblyGroupNode)view.SelectedNode;

        string asmName = await DialogUtility.ShowSingleLineTextDialogAsyncL("Disabled File Name", "", s =>
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                //DialogUtility.ShowMessageBoxAsync(L("Name cannot be empty"));
                return false;
            }

            return true;
        });

        if (string.IsNullOrEmpty(asmName))
        {
            return;
        }

        asmName = asmName.Trim();

        node.WorkSpace.AddDisabledAssemblyReference(asmName);
        node.PopulateUpdate();
    }
}
