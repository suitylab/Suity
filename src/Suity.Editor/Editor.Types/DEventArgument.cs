using System.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Represents an event argument type in the editor.
/// </summary>
public class DEventArgument : DType
{
    /// <summary>
    /// The icon key for event argument types.
    /// </summary>
    public const string DEventArgumentIconKey = "*CoreIcon|EventArgument";

    private TypeDefinition _argumentType = TypeDefinition.Empty;

    /// <summary>
    /// Initializes a new instance of the DEventArgument class.
    /// </summary>
    public DEventArgument()
    { }

    /// <summary>
    /// Initializes a new instance of the DEventArgument class with a name.
    /// </summary>
    public DEventArgument(string name)
        : base(name)
    {
    }

    /// <inheritdoc />
    public override Image DefaultIcon => CoreIconCache.EventArgument;

    /// <summary>
    /// Gets or sets the argument type.
    /// </summary>
    public TypeDefinition ArgumentType
    {
        get => _argumentType;
        internal protected set
        {
            if (_argumentType == value)
            {
                return;
            }
            _argumentType = value;
            NotifyPropertyUpdated();
        }
    }
}

public class DEventArgumentBuilder : DTypeBuilder<DEventArgument>
{
    private TypeDefinition _argumentType;

    public DEventArgumentBuilder()
    {
        AddAutoUpdate(nameof(DEventArgument.ArgumentType), o => o.ArgumentType = _argumentType);
    }

    public void SetArgumentType(TypeDefinition type)
    {
        _argumentType = type;
        UpdateAuto(nameof(DEventArgument.ArgumentType));
    }
}