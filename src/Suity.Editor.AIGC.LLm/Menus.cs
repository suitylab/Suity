using Suity.Editor.AIGC.Flows;
using Suity.Editor.Design;
using Suity.Editor.Documents;
using Suity.Editor.Flows;
using Suity.Editor.Flows.AIGC;
using Suity.Editor.Transferring;
using Suity.Views;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.AIGC;


/// <summary>
/// Menu command to run an AIGC workflow from the flow view.
/// </summary>
[InsertInto("#AigcFlow")]
public class RunWorkflowMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RunWorkflowMenu"/> class.
    /// </summary>
    public RunWorkflowMenu()
        : base("Run", CoreIconCache.Play)
    {
        AcceptOneItemOnly = true;
        //AcceptedCommonType = typeof(WorkflowNode);
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);

        if (!Visible)
        {
            return;
        }

        Visible = (Selection?.FirstOrDefault() as IFlowViewNode)?.Node is IFlowRunnable;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Selection?.FirstOrDefault() is not IFlowViewNode viewNode)
        {
            return;
        }

        if (viewNode?.Node is not IFlowRunnable runnable)
        {
            return;
        }

        LLmService.Instance.StartWorkflowChat(runnable, viewNode.FlowView);
        EditorUtility.ShowToolWindow(AigcChatToolWindow.Instance);
    }
}

/// <summary>
/// Root menu for AIGC attachment operations.
/// </summary>
internal class AigcAttachMenu : RootMenuCommand
{
    /// <summary>
    /// Gets the singleton instance of the AIGC attach menu.
    /// </summary>
    public static AigcAttachMenu Instance { get; } = new AigcAttachMenu();

    /// <summary>
    /// Initializes a new instance of the <see cref="AigcAttachMenu"/> class.
    /// </summary>
    public AigcAttachMenu()
    {
        AddCommand(new AttachSelection());
        AddCommand(new ClearAttachmentMenu());
    }
}

/// <summary>
/// Menu command to attach the current document selection to the AI chat.
/// </summary>
internal class AttachSelection : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AttachSelection"/> class.
    /// </summary>
    public AttachSelection()
        : base("Attach Current Selection", CoreIconCache.Attachment)
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        var doc = DocumentViewManager.Current.ActiveDocument?.Content;
        if (doc is null)
        {
            DialogUtility.ShowMessageBoxAsync("Please open a document first");
            return;
        }

        var transfer = ContentTransfer<DataRW>.GetTransfer(doc.GetType());
        if (transfer is null)
        {
            DialogUtility.ShowMessageBoxAsync("Current document does not support data export.");
            return;
        }

        if (doc.View is not IViewSelectionInfo selectionInfo)
        {
            DialogUtility.ShowMessageBoxAsync("Cannot get selection from current document view.");
            return;
        }

        var objs = selectionInfo.SelectedObjects;
        if (objs is null || !objs.Any())
        {
            DialogUtility.ShowMessageBoxAsync("No objects selected in current document view.");
            return;
        }

        var names = objs.OfType<IMember>().Select(o => o.Name);

        AigcChatToolWindow.Instance?.AddAttachment(doc.Entry, names);
    }
}

/// <summary>
/// Menu command to clear all attachments from the AI chat.
/// </summary>
internal class ClearAttachmentMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClearAttachmentMenu"/> class.
    /// </summary>
    public ClearAttachmentMenu()
        : base("Clear Attachments", CoreIconCache.Delete)
    {
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        AigcChatToolWindow.Instance?.ClearAttachment();
    }
}