using Suity.Rex.Mapping;
using Suity.Rex.VirtualDom;
using System;

namespace Suity.Editor;

/// <summary>
/// Provides static Rex actions for editor commands.
/// </summary>
public static class EditorCommands
{
    public static readonly RexTree Tree = new();
    public static readonly RexMapper Mapper = new(false);

    public static readonly RexAction SaveDocument = new(Tree, $"File.SaveDocument");
    public static readonly RexAction SaveAllDocuments = new(Tree, "File.SaveAllDocuments");
    public static readonly RexAction Undo = new(Tree, "Edit.Undo");
    public static readonly RexAction Redo = new(Tree, "Edit.Redo");
    public static readonly RexAction NaviUndo = new(Tree, "Edit.NaviUndo");
    public static readonly RexAction NaviRedo = new(Tree, "Edit.NaviRedo");
    public static readonly RexAction Copy = new(Tree, "Edit.Copy");
    public static readonly RexAction Cut = new(Tree, "Edit.Cut");
    public static readonly RexAction Paste = new(Tree, "Edit.Paste");
    public static readonly RexAction Comment = new(Tree, "Edit.Comment");
    public static readonly RexAction Uncomment = new(Tree, "Edit.Uncomment");
    public static readonly RexAction FindReference = new(Tree, "Edit.FindReference");
    public static readonly RexAction FindImplement = new(Tree, "Edit.FindImplement");
    public static readonly RexAction Account = new(Tree, "Edit.Account");

    public static readonly RexAction Render = new(Tree, "Build.Render");
    public static readonly RexAction RenderIncremental = new(Tree, "Build.RenderIncremental");

    public static readonly RexAction CompileSolution = new(Tree, "Build.CompileSolution");

    public static readonly RexAction AnalyzeUserCode = new(Tree, "Build.AnalyzeUserCode");
    public static readonly RexAction ValidateActiveDocument = new(Tree, "Build.ValidateActiveDocument");

    public static readonly RexAction ShowLogView = new(Tree, "View.ShowLogView");
    public static readonly RexAction ClearLog = new(Tree, "View.ClearLog");
    public static readonly RexAction ShowPublishView = new(Tree, "View.ShowPublishView");
    public static readonly RexAction ShowChatView = new(Tree, "View.ShowChatView");


    #region Command

    public static void DoCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        EditorCommands.Tree.DoAction(command);
    }

    public static IDisposable RegisterCommand(string command, Action action)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            throw new ArgumentException("Command is empty.", nameof(command));
        }

        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return EditorCommands.Tree.AddActionListener(command, action);
    }

    #endregion
}