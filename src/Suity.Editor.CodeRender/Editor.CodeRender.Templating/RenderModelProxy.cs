using Suity.Editor.CodeRender.Ast;
using Suity.Editor.Expressions;
using Suity.Editor.Values;
using System.Dynamic;
using System.Linq;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// A proxy that provides dynamic access to an <see cref="ICodeRenderElement"/> model for code rendering.
/// Exposes model properties such as name, type, children, attributes, and supports querying child nodes.
/// </summary>
public class RenderModelProxy : RenderProxy
{
    /// <summary>
    /// Gets or sets the code render element model associated with this proxy.
    /// </summary>
    protected internal ICodeRenderElement Model { get; set; }

    /// <summary>
    /// Initializes a new instance with a model name, expression context, render language, and code render element.
    /// </summary>
    /// <param name="model">The model name used as the base code expression.</param>
    /// <param name="context">The expression context for code rendering.</param>
    /// <param name="language">The render language configuration.</param>
    /// <param name="node">The code render element to wrap.</param>
    public RenderModelProxy(string model, ExpressionContext context, RenderLanguage language, ICodeRenderElement node)
        : base(model, context, language)
    {
        Model = node;
    }

    /// <summary>
    /// Initializes a new instance by extending a base proxy with additional expression code and a code render element.
    /// </summary>
    /// <param name="baseProxy">The base proxy to extend.</param>
    /// <param name="exCode">The additional expression code to append.</param>
    /// <param name="node">The code render element to wrap.</param>
    public RenderModelProxy(RenderProxy baseProxy, string exCode, ICodeRenderElement node)
        : base(baseProxy, exCode)
    {
        Model = node;
    }

    /// <inheritdoc/>
    protected override bool IsContentValid()
    {
        return Model != null && base.IsContentValid();
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
                result = Model.GetName();
                return true;

            case "FullName":
                result = Model.GetFullName();
                return true;

            case "ShortName":
                result = Model.GetShortName();
                return true;

            case "FullTypeName":
                result = Model.GetFullTypeName();
                return true;

            case "Id":
                if (Context?.Disabled == true)
                {
                    result = Model.GetFullTypeName();
                }
                else
                {
                    result = Model.GetId();
                }
                return true;

            case "Description":
                result = Model.GetDescription();
                return true;

            case "AssetName":
            case "AssetKey":
            case "AssetFullKey":
            case "FullAssetKey":
                result = CodeBinder.Instance.AssetKey(Model);
                return true;

            case "PathName":
            case "Path":
                result = Model.GetPathName();
                return true;

            case "ImportedId":
                result = Model.GetImportedId();
                return true;

            case "DataId":
                result = CodeBinder.Instance.DataId(Model);
                return true;

            case "Parent":
                ICodeRenderElement parent = CodeBinder.Instance.Parent(Model) as ICodeRenderElement;
                if (parent != null)
                {
                    result = new RenderModelProxy(this, ".Parent", parent);
                }
                else
                {
                    result = new ErrorProxy(this, ".Parent");
                }
                return true;

            case "Type":
            case "TypeInfo":
                result = new TypeDefinitionProxy(this, ".Type", Model.GetTypeInfo());
                return true;

            case "BaseType":
            case "BaseTypeInfo":
                result = new TypeDefinitionProxy(this, ".BaseType", Model.GetBaseTypeInfo());
                return true;

            case "BaseEnumType":
            case "BaseEnumTypeInfo":
                result = new TypeDefinitionProxy(this, ".BaseEnumType", Model.GetBaseEnumTypeInfo());
                return true;

            case "BaseValueType":
            case "BaseValueTypeInfo":
                result = new TypeDefinitionProxy(this, ".BaseValueType", Model.GetBaseValueTypeInfo());
                return true;

            case "ReturnType":
            case "ReturnTypeInfo":
                result = new TypeDefinitionProxy(this, ".ReturnType", Model.GetReturnTypeInfo());
                return true;

            case "RenderType":
            case "RenderTypeName":
                result = Model.RenderType?.LocalName ?? string.Empty;
                return true;

            case "Children":
            case "ChildNodes":
                result = Model.GetChildNodes()
                    .OfType<ICodeRenderElement>()
                    .Select(o => new RenderModelProxy(this, "." + o.GetName(), o)
                    );
                return true;

            case "ChildCount":
            case "Count":
                result = Model.GetChildNodes().OfType<ICodeRenderElement>().Count();
                return true;

            case "NameSpace":
                result = GetNameSpace(Model);
                return true;

            case "FileNameSpace":
            case "CustomNameSpace":
                // Use the context namespace for file/custom namespace
                result = Context.NameSpace;
                return true;

            case "Version":
                result = Model.GetVersion();
                return true;

            case "SupportedVersions":
                result = Model.GetSupportedVersions();
                return true;

            case "Components":
                result = Model.GetComponents()
                    .OfType<ICodeRenderElement>()
                    .Select(o => new RenderModelProxy(this, "." + o.GetName(), o)
                    );
                return true;

            case "ComponentCount":
                result = Model.GetComponents()
                    .OfType<ICodeRenderElement>().Count();
                return true;

            default:
                var obj = Model.GetProperty(CodeRenderProperty.GetProperty(binder.Name), null);
                if (obj != null)
                {
                    result = ProxyHelper.ResolveProxy(this, "." + binder.Name, obj);
                    return true;
                }
                else
                {
                    return base.TryGetMember(binder, out result);
                }
        }
    }

    /// <inheritdoc/>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        if (!IsContentValid())
        {
            return base.TryInvokeMember(binder, args, out result);
        }

        switch (binder.Name)
        {
            case "Child":
            case "GetChild":
            case "GetChildNode":
                if (args.Length == 1 && args[0] is string nameGetChildNode)
                {
                    if (Model.GetChildNode(nameGetChildNode) is ICodeRenderElement node)
                    {
                        result = new RenderModelProxy(this, "." + (string)args[0], node);
                        return true;
                    }
                }
                break;

            case "GetChildNodesWithAttribute":
                if (args.Length == 1 && args[0] is string nameGetChildNodesWithAttribute)
                {
                    result = Model.GetChildNodes()
                        .OfType<ICodeRenderElement>()
                        .Where(o => o.GetAttribute(nameGetChildNodesWithAttribute) != null)
                        .Select(o => new RenderModelProxy(this, "." + o.GetName(), o)
                        );
                    return true;
                }
                break;

            case "GetChildNodesWithRenderType":
                if (args.Length == 1 && args[0] is string nameGetChildNodesWithRenderType)
                {
                    result = Model.GetChildNodes()
                        .OfType<ICodeRenderElement>()
                        .Where(o => o.RenderType?.LocalName == nameGetChildNodesWithRenderType)
                        .Select(o => new RenderModelProxy(this, "." + o.GetName(), o)
                        );
                    return true;
                }
                break;

            case "Attribute":
            case "GetAttribute":
                if (args.Length == 1 && args[0] is string nameGetAttribute)
                {
                    var attr = Model.GetAttribute((string)args[0]);
                    if (attr != null)
                    {
                        result = WrapEditorValue(this, ".Attribute", attr);
                        return true;
                    }
                }
                break;

            case "Attributes":
            case "GetAttributes":
                if (args.Length == 1 && args[0] is string nameGetAttributes)
                {
                    SObject[] attrs = Model.GetAttributes(nameGetAttributes).ToArray();
                    result = WrapEditorValues($"{BaseCode}.Attributes", attrs);
                    return true;
                }
                break;

            case "UserCode":
                {
                    // Generate user code segment markers (begin and end combined)
                    if (args.Length == 1 && args[0] is string ext)
                    {
                        string key = GetUserCodeKey(ext);
                        var tag = Language.SegmentConfig;
                        result = $"{tag.PrefixBegin}{key}{tag.Suffix}{tag.PrefixEnd}{key}{tag.Suffix}";

                        return true;
                    }
                }
                break;

            case "UserCodeBegin":
                {
                    // Generate user code segment begin marker
                    if (args.Length == 1 && args[0] is string ext)
                    {
                        string key = GetUserCodeKey(ext);
                        var tag = Language.SegmentConfig;
                        result = $"{tag.PrefixBegin}{key}{tag.Suffix}\r\n";

                        return true;
                    }
                }
                break;

            case "UserCodeEnd":
                {
                    // Generate user code segment end marker
                    if (args.Length == 1 && args[0] is string ext)
                    {
                        string key = GetUserCodeKey(ext);
                        var tag = Language.SegmentConfig;
                        result = $"{tag.PrefixEnd}{key}{tag.Suffix}\r\n";

                        return true;
                    }
                }
                break;

            case "GenCode":
                {
                    // Generate generated code segment markers (begin and end combined)
                    if (args.Length == 1 && args[0] is string ext)
                    {
                        string key = GetGenCodeKey(ext);
                        var tag = Language.SegmentConfig;
                        result = $"{tag.PrefixBegin}{key}{tag.Suffix}{tag.PrefixEnd}{key}{tag.Suffix}";

                        return true;
                    }
                }
                break;

            case "GenCodeBegin":
                {
                    // Generate generated code segment begin marker
                    if (args.Length == 1 && args[0] is string ext)
                    {
                        string key = GetGenCodeKey(ext);
                        var tag = Language.SegmentConfig;
                        result = $"{tag.PrefixBegin}{key}{tag.Suffix}\r\n";

                        return true;
                    }
                }
                break;

            case "GenCodeEnd":
                {
                    // Generate generated code segment end marker
                    if (args.Length == 1 && args[0] is string ext)
                    {
                        string key = GetGenCodeKey(ext);
                        var tag = Language.SegmentConfig;
                        result = $"{tag.PrefixEnd}{key}{tag.Suffix}\r\n";

                        return true;
                    }
                }
                break;

            default:
                if (args.Length == 1)
                {
                    var obj = Model.GetProperty(CodeRenderProperty.GetProperty(binder.Name), args[0]);
                    if (obj != null)
                    {
                        result = ProxyHelper.ResolveProxy(this, "." + binder.Name, obj);
                        return true;
                    }
                }
                break;
        }

        return base.TryInvokeMember(binder, args, out result);
    }

    /// <summary>
    /// Generates a unique key for a user code segment.
    /// </summary>
    /// <param name="ext">The extension or identifier for the user code segment.</param>
    /// <returns>The generated user code segment key.</returns>
    private string GetUserCodeKey(string ext)
    {
        return Language.SegmentConfig.GetKey(CodeSegmentConfig.UserCode, Context.MaterialId, Context.RenderTypeId, CodeBinder.Instance.Id(Model), ext);
    }

    /// <summary>
    /// Generates a unique key for a generated code segment.
    /// </summary>
    /// <param name="ext">The extension or identifier for the generated code segment.</param>
    /// <returns>The generated code segment key.</returns>
    private string GetGenCodeKey(string ext)
    {
        return Language.SegmentConfig.GetKey(CodeSegmentConfig.GenCode, CodeBinder.Instance.Id(Model), ext);
    }

    /// <inheritdoc/>
    protected override System.Collections.IEnumerator OnGetEnumerator()
    {
        if (!IsContentValid())
        {
            return base.OnGetEnumerator();
        }

        var result = Model.GetChildNodes()
            .OfType<ICodeRenderElement>()
            .Select(o => new RenderModelProxy(this, "." + o.GetName(), o)
            );
        if (result != null)
        {
            return result.GetEnumerator();
        }
        else
        {
            return base.OnGetEnumerator();
        }
    }
}
