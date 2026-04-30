using Suity.Synchonizing.Core;
using System;

namespace Suity.Editor.CodeRender.Ast;

/// <summary>
/// Resolves types for AST synchronization operations.
/// </summary>
public class AstTypeResolver : ISyncTypeResolver
{
    /// <summary>
    /// The singleton instance of the AST type resolver.
    /// </summary>
    public static readonly AstTypeResolver Instance = new AstTypeResolver();

    /// <inheritdoc/>
    public string ResolveTypeName(Type type, object obj)
    {
        return null;
    }

    /// <inheritdoc/>
    public Type ResolveType(string typeName, string parameter)
    {
        if (typeName == null)
        {
            return null;
        }

        switch (typeName.ToLowerInvariant())
        {
            case "typenode":
                return typeof(TypeNode);

            case "field":
                return typeof(ClassField);

            case "enumfield":
                return typeof(EnumField);

            case "method":
                return typeof(ClassMethod);

            case "native":
                return typeof(ClassNativeCode);

            default:
                return null;
        }
    }

    /// <inheritdoc/>
    public object ResolveObject(string typeName, string parameter)
    {
        return null;
    }

    /// <inheritdoc/>
    public string ResolveObjectValue(object obj)
    {
        return null;
    }

    /// <inheritdoc/>
    public object CreateProxy(object obj)
    {
        return null;
    }
}