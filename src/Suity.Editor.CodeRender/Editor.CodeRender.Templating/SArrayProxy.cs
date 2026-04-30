using Suity.Editor.Values;
using System;
using System.Collections;
using System.Dynamic;
using System.Linq;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// A proxy that provides dynamic access to an <see cref="SArray"/> for code rendering.
/// Supports indexed access, count property, and enumeration over array items.
/// </summary>
internal class SArrayProxy : RenderProxy
{
    private readonly SArray _ary;

    /// <summary>
    /// Initializes a new instance with a base proxy, additional expression code, and an SArray.
    /// </summary>
    /// <param name="baseProxy">The base proxy to extend.</param>
    /// <param name="exCode">The additional expression code to append.</param>
    /// <param name="ary">The SArray to wrap. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ary"/> is null.</exception>
    public SArrayProxy(RenderProxy baseProxy, string exCode, SArray ary)
        : base(baseProxy, exCode)
    {
        _ary = ary ?? throw new ArgumentNullException();
    }

    /// <inheritdoc/>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        if (binder.Name == "Count")
        {
            result = _ary.Count;
            return true;
        }
        else
        {
            return base.TryGetMember(binder, out result);
        }
    }

    /// <inheritdoc/>
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        if (indexes.Length == 1 && indexes[0] is int v)
        {
            int index = v;
            object value = null;
            if (index >= 0 && index < _ary.Count)
            {
                value = _ary[index];
            }
            result = WrapEditorValue(this, $"[{index}]", value);
            return true;
        }

        return base.TryGetIndex(binder, indexes, out result);
    }

    /// <inheritdoc/>
    protected override IEnumerator OnGetEnumerator()
    {
        return _ary.Items.Select(o => WrapEditorValue(this, $".Item", o)).GetEnumerator();
    }
}
