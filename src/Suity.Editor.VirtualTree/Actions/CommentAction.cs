using static Suity.Helpers.GlobalLocalizer;
using Suity.Views;

namespace Suity.Editor.VirtualTree.Actions;

/// <summary>
/// Represents an undoable action that sets or clears the comment state on one or more view objects.
/// </summary>
public class CommentAction : VirtualNodeSetterAction
{
    private readonly IViewComment[] _objs;
    private readonly bool[] _oldIsComments;
    private readonly bool _isComment;

    /// <inheritdoc/>
    public override string Name => L("Set Comment");

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentAction"/> class.
    /// </summary>
    /// <param name="model">The virtual tree model this action operates on.</param>
    /// <param name="objs">The array of view comment objects to modify.</param>
    /// <param name="isComment">The new comment state to apply to all objects.</param>
    public CommentAction(VirtualTreeModel model, IViewComment[] objs, bool isComment)
        : base(model)
    {
        _objs = objs;
        _oldIsComments = new bool[objs.Length];

        for (int i = 0; i < objs.Length; i++)
        {
            _oldIsComments[i] = objs[i].IsComment;
        }
        _isComment = isComment;
    }

    /// <inheritdoc/>
    public override void Do()
    {
        for (int i = 0; i < _objs.Length; i++)
        {
            _objs[i].IsComment = _isComment;
        }

        base.Do();
    }

    /// <inheritdoc/>
    public override void Undo()
    {
        for (int i = 0; i < _objs.Length; i++)
        {
            _objs[i].IsComment = _oldIsComments[i];
        }

        base.Undo();
    }
}
