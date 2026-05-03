using Suity.Drawing;
using System.Drawing;

namespace Suity.Editor.Types;

/// <summary>
/// Represents an abstract function type in the editor.
/// </summary>
[AssetTypeBinding(AssetDefNames.AbstractFunction, "Abstract Function")]
public class DSystemAbstractFunction : DType
{
    private TypeDefinition _returnType = TypeDefinition.Empty;

    /// <summary>
    /// Initializes a new instance of the DSystemAbstractFunction class.
    /// </summary>
    public DSystemAbstractFunction()
    { }

    /// <summary>
    /// Initializes a new instance of the DSystemAbstractFunction class.
    /// </summary>
    internal DSystemAbstractFunction(string assetKey, TypeDefinition returnType, string iconKey)
    {
        LocalName = assetKey;
        if (!TypeDefinition.IsNullOrEmpty(returnType))
        {
            _returnType = returnType.MakeAbstractFunctionType();
        }
        else
        {
            _returnType = TypeDefinition.Empty;
        }
        IconKey = iconKey;
    }

    /// <summary>
    /// Gets or sets the return type.
    /// </summary>
    public TypeDefinition ReturnType
    {
        get => _returnType;
        protected internal set
        {
            if (value == null)
            {
                value = TypeDefinition.Empty;
            }

            //if (!TypeDefinition.IsNullOrEmpty(value))
            //{
            //    value = value.MakeAbstractFunctionType();
            //}

            if (value == _returnType)
            {
                return;
            }

            _returnType = value;
            NotifyPropertyUpdated();
        }
    }

    /// <inheritdoc />
    public override ImageDef DefaultIcon => CoreIconCache.Function;

    /// <inheritdoc />
    public override TypeRelationships Relationship => TypeRelationships.AbstractFunction;

    /// <inheritdoc />
    public override TypeDefinition ElementTypeDefinition => _returnType;

    /// <inheritdoc />
    public override TypeDefinition PrimaryTypeDefinition => DTypeManager.Instance.GetFunctionsByReturnType(Definition)?.PrimaryAsset?.Definition;

    /// <inheritdoc />
    public override bool IsAssignableFrom(TypeDefinition implementType)
    {
        if (TypeDefinition.IsNullOrEmpty(implementType))
        {
            return false;
        }
        if (!implementType.IsFunction)
        {
            return false;
        }

        if (TypeDefinition.IsNullOrEmpty(_returnType))
        {
            return true;
        }

        if (_returnType == implementType.BaseAbstractType?.ElementType)
        {
            return true;
        }

        if (_returnType.IsAssignableFrom(implementType.BaseAbstractType?.ElementType))
        {
            return true;
        }

        if (implementType.Target is DFunction dfunc)
        {
            // Dynamic type
            if (ElementTypeDefinition.IsArray || ElementTypeDefinition.IsAbstractArray)
            {
                if ((dfunc.ReturnTypeBinding & DReturnTypeBinding.Array) != DReturnTypeBinding.None)
                {
                    return true;
                }
            }
            else
            {
                if ((dfunc.ReturnTypeBinding & DReturnTypeBinding.Object) != DReturnTypeBinding.None)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

/// <summary>
/// Builder for creating DSystemAbstractFunction instances.
/// </summary>
public class DSystemAbstractFunctionBuilder : DTypeBuilder<DSystemAbstractFunction>
{
    private TypeDefinition _returnType = TypeDefinition.Empty;

    /// <summary>
    /// Initializes a new instance of the DSystemAbstractFunctionBuilder class.
    /// </summary>
    public DSystemAbstractFunctionBuilder()
    {
        AddAutoUpdate(nameof(DSystemAbstractFunction.ReturnType), o => o.ReturnType = _returnType);
    }

    /// <summary>
    /// Initializes a new instance of the DSystemAbstractFunctionBuilder class with parameters.
    /// </summary>
    public DSystemAbstractFunctionBuilder(string name, TypeDefinition returnType, string iconKey)
        : this()
    {
        SetLocalName(name);
        SetReturnType(returnType ?? TypeDefinition.Empty);
        SetIconKey(iconKey);
    }

    /// <summary>
    /// Sets the return type.
    /// </summary>
    public void SetReturnType(TypeDefinition type)
    {
        _returnType = type;
        UpdateAuto(nameof(DSystemAbstractFunction.ReturnType));
    }
}