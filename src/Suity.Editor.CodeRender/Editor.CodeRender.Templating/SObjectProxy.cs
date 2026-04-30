using Suity.Editor.Values;
using System;
using System.Dynamic;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// A proxy that provides dynamic access to an <see cref="SObject"/> properties for code rendering.
/// Wraps editor object values and exposes their properties through dynamic member access.
/// </summary>
internal class SObjectProxy : RenderProxy
{
    private readonly SObject _obj;

    /// <summary>
    /// Initializes a new instance with a base proxy, additional expression code, and an SObject.
    /// </summary>
    /// <param name="baseProxy">The base proxy to extend.</param>
    /// <param name="exCode">The additional expression code to append.</param>
    /// <param name="obj">The SObject to wrap. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is null.</exception>
    public SObjectProxy(RenderProxy baseProxy, string exCode, SObject obj)
        : base(baseProxy, exCode)
    {
        _obj = obj ?? throw new ArgumentNullException();
    }

    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        if (_obj.ContainsProperty(binder.Name))
        {
            object value = _obj[binder.Name];
            result = WrapEditorValue(this, "." + binder.Name, value);
            return true;
        }
        else
        {
            return base.TryGetMember(binder, out result);
        }
    }
}
