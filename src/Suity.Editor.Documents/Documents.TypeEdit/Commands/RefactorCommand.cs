using Suity.Views;
using Suity.Views.Menu;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Suity.Editor.Documents.TypeEdit.Commands;

/// <summary>
/// Menu command that provides access to refactoring operations for struct fields.
/// </summary>
[InsertInto(":InspectorTree")]
public class RefactorCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RefactorCommand"/> class.
    /// </summary>
    public RefactorCommand()
        : base("Refactor", CoreIconCache.Structure)
    {
        AddCommand(new ExtractStructCommand());
        AddCommand(new CollapseStructCommand());
        AddSeparator();
        AddCommand(new ExtractArrayCommand());
        AddCommand(new CollapseArrayCommand());
    }

    /// <inheritdoc/>
    protected override void OnPopUp(int selectionCount, ICollection<Type> types, Type commonNodeType)
    {
        var objs = EditorUtility.Inspector.DetailTreeSelection;

        Visible = objs.Any() && objs.All(o => o is StructField);
    }
}
