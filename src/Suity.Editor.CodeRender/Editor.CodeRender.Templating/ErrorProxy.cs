using System.Dynamic;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// A proxy that represents an error or invalid state in the template expression chain.
/// Always returns true for Empty/IsNull and false for Exist/HasValue/NotNull checks.
/// Used as a fallback when a member or operation cannot be resolved.
/// </summary>
public class ErrorProxy : DynamicProxy
{
    /// <summary>
    /// Initializes a new instance with the specified error code.
    /// </summary>
    /// <param name="code">The error code expression string.</param>
    public ErrorProxy(string code)
        : base(code)
    {
    }

    /// <summary>
    /// Initializes a new instance by concatenating a base error code with additional error code.
    /// </summary>
    /// <param name="baseErrorCode">The base error code string.</param>
    /// <param name="code">The additional error code to append.</param>
    public ErrorProxy(string baseErrorCode, string code)
        : base(baseErrorCode + code)
    {
    }

    /// <summary>
    /// Initializes a new instance by extending a base proxy with additional error expression code.
    /// </summary>
    /// <param name="baseProxy">The base proxy to extend.</param>
    /// <param name="exCode">The additional error expression code to append.</param>
    public ErrorProxy(DynamicProxy baseProxy, string exCode)
        : base(baseProxy, exCode)
    {
    }

    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        switch (binder.Name)
        {
            case "IsEmpty":
            case "IsNull":
            case "Empty":
                result = true;
                return true;

            case "Exist":
            case "HasValue":
            case "NotNull":
                result = false;
                return true;

            default:
                break;
        }

        return base.TryGetMember(binder, out result);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[?{BaseCode}?]";
    }
}
