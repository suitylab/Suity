using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.CodeRender.Templating;

/// <summary>
/// Provides helper methods for resolving objects to appropriate proxy types during code rendering.
/// </summary>
public static class ProxyHelper
{
    /// <summary>
    /// Resolves an object to the appropriate render proxy based on its type.
    /// </summary>
    /// <param name="baseProxy">The base proxy to extend when creating a new proxy.</param>
    /// <param name="exCode">The additional expression code to append to the proxy.</param>
    /// <param name="obj">The object to resolve to a proxy.</param>
    /// <returns>
    /// A proxy appropriate for the object type: <see cref="RenderModelProxy"/> for ICodeRenderElement,
    /// <see cref="TypeDefinitionProxy"/> for TypeDefinition, <see cref="AssetIdProxy"/> for Guid,
    /// <see cref="SObjectProxy"/> for SObject, <see cref="SArrayProxy"/> for SArray,
    /// the raw value for value types or strings, or an <see cref="ErrorProxy"/> for unsupported types.
    /// </returns>
    public static object ResolveProxy(RenderProxy baseProxy, string exCode, object obj)
    {
        if (obj is null)
        {
            return new ErrorProxy(baseProxy, exCode);
        }

        if (obj.GetType().IsValueType || obj.GetType() == typeof(string))
        {
            return obj;
        }

        switch (obj)
        {
            case ICodeRenderElement model:
                return new RenderModelProxy(baseProxy, exCode, model);

            case IEnumerable<ICodeRenderElement> models:
                return models.OfType<ICodeRenderElement>().Select(o => new RenderModelProxy(baseProxy, exCode, o));

            case TypeDefinition type:
                return new TypeDefinitionProxy(baseProxy, exCode, type);

            case Guid id:
                return new AssetIdProxy(baseProxy, exCode, id);

            case SObject sobj:
                return new SObjectProxy(baseProxy, exCode, sobj);

            case SArray sary:
                return new SArrayProxy(baseProxy, exCode, sary);

            default:
                return new ErrorProxy(baseProxy, exCode);
        }
    }
}
