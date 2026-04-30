using Suity.Editor.CodeRender.Ast;
using Suity.Editor.Expressions;
using System.Dynamic;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// A proxy that wraps an array of editor values, providing indexed access to individual wrapped proxies.
/// Used when multiple values need to be accessed as a collection in template expressions.
/// </summary>
public class EditorValueArrayProxy : RenderProxy
{
    private readonly object[] _values;

    /// <summary>
    /// Initializes a new instance with a base code string, expression context, render language, and an array of objects.
    /// Each object is wrapped into an appropriate proxy using <see cref="RenderProxy.WrapEditorValue"/>.
    /// </summary>
    /// <param name="baseCode">The base code expression string.</param>
    /// <param name="context">The expression context for code rendering.</param>
    /// <param name="language">The render language configuration.</param>
    /// <param name="objs">The array of objects to wrap.</param>
    public EditorValueArrayProxy(string baseCode, ExpressionContext context, RenderLanguage language, object[] objs)
        : base(baseCode, context, language)
    {
        _values = new RenderProxy[objs.Length];
        for (int i = 0; i < _values.Length; i++)
        {
            _values[i] = WrapEditorValue(this, $"[{i}]", objs[i]);
        }
    }

    /// <inheritdoc/>
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
        if (indexes.Length == 1 && indexes[0] is int)
        {
            int index = (int)indexes[0];
            if (index >= 0 && index < _values.Length)
            {
                result = _values[index];
                return true;
            }
        }

        return base.TryGetIndex(binder, indexes, out result);
    }
}
