using Suity.Collections;
using Suity.Helpers;
using System.Dynamic;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// A dynamic proxy object that builds template code expressions through dynamic member access.
/// Serves as the base class for all render proxies, capturing member access patterns as code strings.
/// </summary>
public class DynamicProxy : DynamicObject, System.Collections.IEnumerable
{
    /// <summary>
    /// An empty proxy instance with no associated code.
    /// </summary>
    public static readonly DynamicProxy Empty = new();

    private readonly string _code;

    /// <summary>
    /// Initializes a new instance with an empty code string.
    /// </summary>
    public DynamicProxy()
    {
        _code = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance with the specified code string.
    /// </summary>
    /// <param name="code">The template code expression to associate with this proxy.</param>
    public DynamicProxy(string code)
    {
        _code = code;
    }

    /// <summary>
    /// Initializes a new instance by extending a base proxy's code with additional expression code.
    /// </summary>
    /// <param name="baseProxy">The base proxy to extend.</param>
    /// <param name="exCode">The additional expression code to append. If null or whitespace, defaults to ".???".</param>
    public DynamicProxy(DynamicProxy baseProxy, string exCode)
    {
        if (string.IsNullOrWhiteSpace(exCode))
        {
            exCode = ".???";
        }

        _code = baseProxy._code + exCode;
    }

    /// <summary>
    /// Gets the base code expression string captured by this proxy.
    /// </summary>
    public string BaseCode => _code;

    /// <inheritdoc/>
    public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
    {
        result = new ErrorProxy(this, "-BinOp:" + binder.Operation.ToString());
        return true;
    }

    /// <inheritdoc/>
    public override bool TryConvert(ConvertBinder binder, out object result)
    {
        result = new ErrorProxy(this, "-Convert:" + binder.Type.Name);
        return true;
    }

    /// <inheritdoc/>
    public override bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
    {
        result = new ErrorProxy(this, "(New)");
        return true;
    }

    /// <inheritdoc/>
    public override bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
    {
        return true;
    }

    /// <inheritdoc/>
    public override bool TryDeleteMember(DeleteMemberBinder binder)
    {
        return true;
    }

    /// <inheritdoc/>
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        result = new ErrorProxy(this, $"[{indexes.ToString(",")}]");
        return true;
    }

    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        switch (binder.Name)
        {
            case "IsEmpty":
            case "IsNull":
            case "Empty":
                result = this.GetType() == typeof(DynamicProxy);
                return true;

            case "Exist":
            case "HasValue":
            case "NotNull":
                result = this.GetType() != typeof(DynamicProxy);
                return true;

            default:
                break;
        }

        result = new ErrorProxy(this, "." + binder.Name);
        return true;
    }

    /// <inheritdoc/>
    public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
    {
        result = new ErrorProxy(this, "()");
        return true;
    }

    /// <inheritdoc/>
    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        result = new ErrorProxy(this, $".{binder.Name}()");
        return true;
    }

    /// <inheritdoc/>
    public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
    {
        return true;
    }

    /// <inheritdoc/>
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        return true;
    }

    /// <inheritdoc/>
    public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
    {
        result = new ErrorProxy(this, "-UnaryOp:" + binder.Operation.ToString());
        return true;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return _code;
    }

    #region IEnumerable

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return OnGetEnumerator();
    }

    /// <summary>
    /// Provides the enumerator for iterating over this proxy's contents.
    /// Returns an empty enumerator by default.
    /// </summary>
    /// <returns>An <see cref="System.Collections.IEnumerator"/> for iteration.</returns>
    protected virtual System.Collections.IEnumerator OnGetEnumerator()
    {
        return EmptyEnumerator.Emtpy;
    }

    #endregion
}
