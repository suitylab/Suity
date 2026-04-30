using Suity.Helpers;
using Suity.Views;
using Suity.Views.Menu;
using System.Linq;

namespace Suity.Editor.Documents.TypeEdit.Commands;

/// <summary>
/// Menu command to find implementations of an abstract type.
/// </summary>
[InsertInto("#TypeDesign")]
public class FindImplementCommand : MenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FindImplementCommand"/> class.
    /// </summary>
    public FindImplementCommand()
        : base("Find Implementation", CoreIconCache.Search.ToIconSmall())
    {
        AcceptType<AbstractType>(false);
        AcceptOneItemOnly = true;
    }

    /// <inheritdoc/>
    public override void DoCommand()
    {
        if (Selection?.FirstOrDefault() is not AbstractType type)
        {
            return;
        }

        EditorUtility.FindImplement(type);
    }
}
