using Suity.Editor.Types;
using System.Dynamic;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// A proxy that provides dynamic access to type definition information for code rendering.
/// Exposes type properties such as name, namespace, and type characteristics (array, struct, enum, etc.).
/// </summary>
public class TypeDefinitionProxy : RenderProxy
{
    /// <summary>
    /// Gets or sets the type definition associated with this proxy.
    /// </summary>
    protected internal TypeDefinition TypeCode { get; set; }

    /// <summary>
    /// Initializes a new instance with a base proxy, additional expression code, and a type definition.
    /// </summary>
    /// <param name="baseProxy">The base proxy to extend.</param>
    /// <param name="exCode">The additional expression code to append.</param>
    /// <param name="type">The type definition to wrap. If null, defaults to <see cref="TypeDefinition.Empty"/>.</param>
    public TypeDefinitionProxy(RenderProxy baseProxy, string exCode, TypeDefinition type)
        : base(baseProxy, exCode)
    {
        TypeCode = type ?? TypeDefinition.Empty;
    }

    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        if (!IsContentValid())
        {
            return base.TryGetMember(binder, out result);
        }

        switch (binder.Name)
        {
            case "Name":
            case "FullName":
            case "FullTypeName":
                result = TypeCode.GetFullTypeName();
                return true;

            case "ShortName":
                result = TypeCode.GetShortTypeName();
                return true;

            case "ElementType":
            case "InnerType":
            case "Inner":
                result = new TypeDefinitionProxy(this, ".Inner", TypeCode.ElementType);
                return true;

            case "IsController":
                result = TypeCode.Target is DController;
                return true;

            case "IsArray":
                result = TypeCode.IsArray;
                return true;

            case "IsValue":
            case "IsValueOrString":
                result = TypeCode.IsValue;
                return true;

            case "IsNative":
                result = TypeCode.IsNative;
                return true;

            case "IsNumeric":
                result = TypeCode.IsNumeric;
                return true;

            case "IsNormalStruct":
            case "IsStruct":
                result = TypeCode.IsStruct;
                return true;

            case "IsString":
                result = TypeCode == NativeTypes.StringType;
                return true;

            case "IsAbstract":
                result = TypeCode.IsAbstract;
                return true;

            case "IsAbstractStruct":
                result = TypeCode.IsAbstractStruct;
                return true;

            case "IsAbstractFunction":
                result = TypeCode.IsAbstractFunction;
                return true;

            case "IsAbstractArray":
                result = TypeCode.IsAbstractArray;
                return true;

            case "IsFunction":
                result = TypeCode.IsFunction;
                return true;

            case "IsObject":
                result = TypeCode = NativeTypes.ObjectType;
                return true;

            case "IsKeyLink":
                result = TypeCode.IsDataLink;
                return true;

            case "IsAssetLink":
                result = TypeCode.IsAssetLink;
                return true;

            case "IsAnyLink":
            case "IsLink":
                result = TypeCode.IsLink;
                return true;

            case "IsEnum":
                result = TypeCode.IsEnum;
                return true;

            case "Info":
            case "TypeInfo":
            case "Target":
                var type = TypeCode.Target;
                if (type != null)
                {
                    result = new RenderModelProxy(this, ".Target", type);
                }
                else
                {
                    result = new ErrorProxy(this, ".Target");
                }
                return true;
        }

        return base.TryGetMember(binder, out result);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return GetTypeString(TypeCode);
    }
}
