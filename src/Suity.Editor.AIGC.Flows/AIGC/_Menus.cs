using Suity.Views;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.AIGC;

/// <summary>
/// Menu command that navigates to the diagram view of a selected AIGC task page.
/// </summary>
[InsertInto("#AigcTaskPage")]
public class GotoDiagramMenu : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GotoDiagramMenu"/> class.
    /// </summary>
    public GotoDiagramMenu()
        : base("Goto Diagram", CoreIconCache.Aigc)
    {
        AcceptOneItemOnly = true;
        AcceptedCommonType = typeof(AigcTaskPage);
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        base.OnPopUp(selectionCount, types, commonNodeType);
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Selection?.FirstOrDefault() is not AigcTaskPage task)
        {
            return;
        }

        var view = task.GetDocument()?.View as AigcTaskPageDocumentView;
        view?.HandleGotoWorkflow(task);
    }

}
