using Suity.Editor.CodeRender.Ast;
using Suity.Editor.Expressions;
using System;
using System.Dynamic;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// A proxy for a render target that provides dynamic configuration methods for code generation.
/// Allows setting rendering options such as full name usage, native array handling, and short name preferences.
/// </summary>
public class RenderTargetProxy : RenderModelProxy
{
    private readonly RenderTarget _target;

    /// <summary>
    /// Gets the underlying render target associated with this proxy.
    /// </summary>
    protected internal RenderTarget Target => _target;

    /// <summary>
    /// Initializes a new instance with an expression context and a render target.
    /// </summary>
    /// <param name="context">The expression context for code rendering. Must not be null.</param>
    /// <param name="target">The render target to wrap. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="target"/> is null.</exception>
    public RenderTargetProxy(ExpressionContext context, RenderTarget target)
        : base("Model", context, RenderLanguage.GetLanguage(target.Language), target.Item.Object as ICodeRenderElement)
    {
        if (context == null)
        {
            throw new ArgumentNullException();
        }

        _target = target ?? throw new ArgumentNullException();
    }

    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        return base.TryGetMember(binder, out result);
    }

    /// <inheritdoc/>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        if (!IsContentValid())
        {
            return base.TryInvokeMember(binder, args, out result);
        }

        result = null;

        switch (binder.Name)
        {
            case "FullName":
                // Enable full name mode for type resolution
                if (args.Length == 0)
                {
                    Context.UseFullName = true;
                    return true;
                }
                break;

            case "SetFullName":
                // Set whether to use full names for type resolution
                if (args.Length == 1 && args[0] is bool)
                {
                    Context.UseFullName = Convert.ToBoolean(args[0]);
                    return true;
                }
                break;

            case "SetNativeArray":
                // Set whether to use native array syntax
                if (args.Length == 1 && args[0] is bool)
                {
                    Context.UseNativeArray = Convert.ToBoolean(args[0]);
                    return true;
                }
                break;

            case "NativeArray":
                // Enable native array mode
                if (args.Length == 0)
                {
                    Context.UseNativeArray = true;
                    return true;
                }
                break;

            case "SetTryShortName":
                // Set whether to attempt using short names for types in the same namespace
                if (args.Length == 1 && args[0] is bool)
                {
                    Context.TryUseShortName = Convert.ToBoolean(args[0]);
                    return true;
                }
                break;

            case "TryShortName":
                // Enable short name mode for type resolution
                if (args.Length == 0)
                {
                    Context.TryUseShortName = true;
                    return true;
                }
                break;

            default:
                break;
        }

        return base.TryInvokeMember(binder, args, out result);
    }
}
