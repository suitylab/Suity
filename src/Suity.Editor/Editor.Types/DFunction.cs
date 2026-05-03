using Suity.Drawing;
using Suity.Editor.CodeRender;
using System;
using System.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a function type in the editor.
/// </summary>
[AssetTypeBinding(AssetDefNames.Function, "Function")]
public class DFunction : DCompond
{
    /// <summary>
    /// The icon key for function types.
    /// </summary>
    public const string DFunctionIconKey = "*CoreIcon|Function";

    /// <summary>
    /// Gets the default color for function types.
    /// </summary>
    public static Color FunctionTypeColor { get; } = Color.FromArgb(255, 144, 191);

    private TypeDefinition _returnType;
    private DReturnTypeBinding _returnTypeBinding;

    private IRegistryHandle<DFunction> _returnTypeEntry;

    private bool _actionMode;

    /// <summary>
    /// Initializes a new instance of the DFunction class.
    /// </summary>
    public DFunction()
    { }

    /// <inheritdoc />
    public override ImageDef DefaultIcon => CoreIconCache.Function;

    /// <inheritdoc />
    public override Color? TypeColor => FunctionTypeColor;

    /// <summary>
    /// Gets or sets the return type.
    /// </summary>
    public TypeDefinition ReturnType
    {
        get => _returnType;
        protected internal set
        {
            if (_returnType == value)
            {
                return;
            }
            _returnType = value;
            _returnTypeEntry?.Dispose();
            _returnTypeEntry = DTypeManager.Instance.AddToReturnType(this);
            NotifyPropertyUpdated();
        }
    }

    /// <summary>
    /// Gets or sets whether this is an action (void return type).
    /// </summary>
    public bool ActionMode
    {
        get => _actionMode;
        protected internal set
        {
            if (_actionMode == value)
            {
                return;
            }
            _actionMode = value;
            NotifyPropertyUpdated();
        }
    }

    /// <inheritdoc />
    public override RenderType RenderType => RenderType.Function;

    /// <inheritdoc />
    public override TypeDefinition BaseTypeDefinition => _returnType.MakeAbstractFunctionType();

    /// <summary>
    /// Gets or sets the return type binding.
    /// </summary>
    public DReturnTypeBinding ReturnTypeBinding
    {
        get => _returnTypeBinding;
        protected internal set
        {
            if (_returnTypeBinding == value)
            {
                return;
            }

            _returnTypeBinding = value;
            _returnTypeEntry?.Dispose();
            _returnTypeEntry = DTypeManager.Instance.AddToReturnType(this);
            NotifyPropertyUpdated();
        }
    }

    internal override void InternalOnAssetActivate(string assetKey)
    {
        _returnTypeEntry?.Dispose();
        _returnTypeEntry = DTypeManager.Instance.AddToReturnType(this);

        base.InternalOnAssetActivate(assetKey);
    }

    internal override void InternalOnEntryDetached(Guid id)
    {
        _returnTypeEntry?.Dispose();
        _returnTypeEntry = null;

        base.InternalOnEntryDetached(id);
    }

    protected override void OnIsPrimaryUpdated()
    {
        base.OnIsPrimaryUpdated();

        _returnTypeEntry?.Update();
    }
}

public sealed class DFunctionBuilder : DBaseStructBuilder<DFunction>
{
    private TypeDefinition _returnType;
    private DReturnTypeBinding _returnTypeBinding;
    private bool _actionMode;

    public DFunctionBuilder()
    {
        AddAutoUpdate(nameof(DFunction.ReturnType), o => o.ReturnType = _returnType);
        AddAutoUpdate(nameof(DFunction.ReturnTypeBinding), o => o.ReturnTypeBinding = _returnTypeBinding);
        AddAutoUpdate(nameof(DFunction.ActionMode), o => o.ActionMode = _actionMode);
    }

    public void SetReturnType(TypeDefinition type)
    {
        _returnType = type;
        UpdateAuto(nameof(DFunction.ReturnType));
    }

    public void SetReturnTypeBinding(DReturnTypeBinding binding)
    {
        _returnTypeBinding = binding;
        UpdateAuto(nameof(DFunction.ReturnTypeBinding));
    }

    public void SetActionMode(bool actionMode)
    {
        _actionMode = actionMode;
        UpdateAuto(nameof(DFunction.ActionMode));
    }
}