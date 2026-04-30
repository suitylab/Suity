using Suity.Editor.ProjectGui.Commands;
using Suity.Editor.ProjectGui.Commands.FileBunchs;
using Suity.Editor.ProjectGui.Commands.WorkSpaces;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;

namespace Suity.Editor.ProjectGui;

/// <summary>
/// The root context menu command for the project view, aggregating all available menu actions.
/// </summary>
internal class ProjectRootCommand : RootMenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectRootCommand"/> class and populates it with menu commands.
    /// </summary>
    public ProjectRootCommand()
        : base(":Project")
    {
        // Code render related commands.
        /*AddCommand(new IncrementalRenderCommand());
        AddCommand(new RenderCommand());
        AddCommand(new RenderFileCommand());
        AddSeparator();*/

        AddCommand(new CreateFileGroupCommand());
        AddCommand(new CreateWorkSpaceGroupCommand());
        AddCommand(new ChangeWorkSpaceGroupCommand());
        AddCommand(new WorkSpaceConfigCommand());

        // Code render related commands.
        /*AddCommand(new AddWsRefRenderableCommnand());
        AddCommand(new AddWsRefRenderTargetLibraryCommnand());
        //AddCommand(new AddWsRefUserFileCommand());
        AddCommand(new AddWsRefFileBunchCommnand());
        AddCommand(new AddWsSystemReferenceCommand());
        AddCommand(new AddWsAssemblyRefCommand());
        AddCommand(new AddWsDisabledReferenceCommand());
        AddCommand(new WsBindRenderFileCommand());
        AddCommand(new WsUnbindRenderFileCommand());
        AddCommand(new WsCommitBunchCommand());
        AddCommand(new WsCommitAllBunchCommand());*/

        AddCommand(new OpenFileCommand());
        AddCommand(new RenameCommand());
        AddCommand(new CloneFileCommand());
        AddSeparator();

        AddCommand(new DeleteDirFileCommand());

        // Code render related commands.
        /*AddCommand(new WsUnbindBunchFileCommand());
        AddCommand(new WsRefDeleteCommand());
        AddCommand(new DeleteBunchFileCommand());
        AddCommand(new ShrinkFileBunchCommand());
        AddSeparator();

        AddCommand(new UserCodeGroupCommand());
        AddSeparator();

        AddCommand(new CompileSolutionCommand());
        AddCommand(new OpenSolutionCommand());
        AddCommand(new ViewWsUserCodeDbCommand());*/

        AddCommand(new GotoDefinitionCommand());

        // Code render related commands.
        /*AddCommand(new GotoModelDefinitionCommand());
        AddCommand(new GotoMaterialDefinitionCommand());*/

        AddCommand(new FindReferenceCommand());
        AddCommand(new ShowProblemCommand());

        // Code render related commands.
        /*AddCommand(new WsRefSelectAffectedFilesCommand());
        AddCommand(new OpenProjectFileCommand());*/

        AddCommand(new ExploreCommand());
        AddCommand(new ExportCommand());
        AddCommand(new PublishCommand());
        AddSeparator();
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        if (!Visible)
        {
            return;
        }

        Visible = Selection?.Length > 0;
    }

}