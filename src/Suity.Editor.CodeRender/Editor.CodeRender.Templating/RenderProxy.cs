using Suity.Editor.CodeRender.Ast;
using Suity.Editor.Expressions;
using Suity.Editor.Types;
using Suity.Editor.Values;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// Base proxy class for render operations that provides utility methods for wrapping editor values,
/// resolving type names, and managing namespace context during code generation.
/// </summary>
public class RenderProxy : DynamicProxy
{
    /// <summary>
    /// Gets or sets the expression context used for code rendering.
    /// </summary>
    protected internal ExpressionContext Context { get; set; }

    /// <summary>
    /// Gets or sets the render language configuration for code generation.
    /// </summary>
    protected internal RenderLanguage Language { get; set; }

    /// <summary>
    /// Initializes a new instance with a base code string, expression context, and render language.
    /// </summary>
    /// <param name="baseCode">The base code expression string.</param>
    /// <param name="context">The expression context for code rendering.</param>
    /// <param name="language">The render language configuration.</param>
    public RenderProxy(string baseCode, ExpressionContext context, RenderLanguage language)
        : base(baseCode)
    {
        Context = context;
        Language = language;
    }

    /// <summary>
    /// Initializes a new instance by extending a base proxy with additional expression code.
    /// Inherits the context and language from the base proxy.
    /// </summary>
    /// <param name="baseProxy">The base proxy to extend.</param>
    /// <param name="exCode">The additional expression code to append.</param>
    public RenderProxy(RenderProxy baseProxy, string exCode)
        : base(baseProxy, exCode)
    {
        Context = baseProxy.Context;
        Language = baseProxy.Language;
    }

    /// <summary>
    /// Checks whether the content (language and context) is valid for rendering operations.
    /// </summary>
    /// <returns>True if both Language and Context are non-null; otherwise, false.</returns>
    protected virtual bool IsContentValid()
    {
        return Language != null && Context != null;
    }

    /// <summary>
    /// Wraps an editor value into an appropriate proxy object for dynamic access.
    /// </summary>
    /// <param name="baseProxy">The base proxy to extend.</param>
    /// <param name="exCode">The additional expression code to append.</param>
    /// <param name="value">The value to wrap.</param>
    /// <returns>
    /// A proxy object appropriate for the value type: <see cref="SObjectProxy"/> for SObject,
    /// <see cref="SArrayProxy"/> for SArray, <see cref="AssetIdProxy"/> for SKey/SAssetKey,
    /// the raw value for SEnum, or a <see cref="DynamicProxy"/> for null/SDelegate.
    /// </returns>
    protected object WrapEditorValue(RenderProxy baseProxy, string exCode, object value)
    {
        if (value == null)
        {
            return new DynamicProxy(baseProxy, exCode);
        }

        switch (value)
        {
            case SObject sobj:
                return new SObjectProxy(baseProxy, exCode, sobj);

            case SArray sary:
                return new SArrayProxy(baseProxy, exCode, sary);

            case SEnum senm:
                return senm.Value;

            case SKey skey:
                return new AssetIdProxy(baseProxy, exCode, skey.TargetId);

            case SAssetKey sskey:
                return new AssetIdProxy(baseProxy, exCode, sskey.TargetId);

            case SDelegate sd:
                return new DynamicProxy(baseProxy, exCode);

            default:
                break;
        }

        return value;
    }

    /// <summary>
    /// Wraps an array of editor values into an <see cref="EditorValueArrayProxy"/> for dynamic access.
    /// </summary>
    /// <param name="code">The code expression string for the array proxy.</param>
    /// <param name="values">The array of values to wrap.</param>
    /// <returns>An <see cref="EditorValueArrayProxy"/> if values is non-null; otherwise, an <see cref="ErrorProxy"/>.</returns>
    protected object WrapEditorValues(string code, object[] values)
    {
        if (values != null)
        {
            return new EditorValueArrayProxy(code, Context, Language, values);
        }
        else
        {
            return new ErrorProxy(code);
        }
    }

    /// <summary>
    /// Resolves a type definition to its string representation, considering short name preferences.
    /// </summary>
    /// <param name="type">The type definition to resolve.</param>
    /// <returns>The resolved type name string, using short or full name based on context settings.</returns>
    protected string GetTypeString(TypeDefinition type)
    {
        if (Context.TryUseShortName && IsMyNameSpace(type))
        {
            return Language.ResolveTypeName(type, Context, true);
        }
        else
        {
            return Language.ResolveTypeName(type, Context, false);
        }
    }

    /// <summary>
    /// Gets the namespace of the specified value using the code binder.
    /// </summary>
    /// <param name="value">The value to get the namespace from.</param>
    /// <returns>The namespace string of the value.</returns>
    protected string GetNameSpace(object value)
    {
        return CodeBinder.Instance.NameSpace(value);
    }

    /// <summary>
    /// Determines whether the specified value belongs to the current context's namespace.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value's namespace matches the context namespace; otherwise, false.</returns>
    protected bool IsMyNameSpace(object value)
    {
        return GetNameSpace(value) == Context.NameSpace;
    }
}
