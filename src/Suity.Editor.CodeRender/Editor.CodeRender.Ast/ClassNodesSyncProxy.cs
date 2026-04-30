using Suity.Editor.Expressions;
using Suity.Parser.Ast;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;

namespace Suity.Editor.CodeRender.Ast;

/// <summary>
/// Synchronization proxy for <see cref="SourceNode"/> objects.
/// </summary>
[SyncProxyUsage(typeof(SourceNode))]
public class SourceProxy : SyncObjectProxy
{
    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        SourceNode source = TargetAs<SourceNode>();
        source.Body ??= [];
        source.Imports ??= [];

        source.NameSpace = sync.Sync("nameSpace", source.NameSpace, SyncFlag.AttributeMode);
        source.Language = sync.Sync("language", source.Language, SyncFlag.AttributeMode);
        source.Code = sync.Sync("Code", source.Code);

        sync.Sync("Body", source.Body, SyncFlag.GetOnly);
        sync.Sync("Imports", source.Imports, SyncFlag.GetOnly);
    }
}

/// <summary>
/// Synchronization proxy for <see cref="TypeNode"/> objects.
/// </summary>
[SyncProxyUsage(typeof(TypeNode))]
public class TypeNodeProxy : SyncObjectProxy
{
    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        TypeNode cls = TargetAs<TypeNode>();
        cls.Id ??= new() { Type = SyntaxNodes.Identifier };
        cls.Body ??= [];
        cls.Implements ??= [];

        cls.Kind = sync.SyncEnumAttribute<TypeKind>("mode", cls.Kind);
        cls.Id.Name = sync.Sync("name", cls.Id.Name, SyncFlag.AttributeMode);
        cls.AccessMode = sync.SyncEnumAttribute("access", cls.AccessMode);
        cls.IsStatic = sync.SyncBooleanAttribute("static", cls.IsStatic);
        cls.Language = sync.Sync("language", cls.Language, SyncFlag.AttributeMode);
        cls.Code = sync.Sync("Code", cls.Code);
        cls.Ast = sync.SyncBooleanAttribute("ast", cls.Ast);

        cls.Extends = sync.Sync("extends", cls.Extends, SyncFlag.AttributeMode);
        sync.Sync("Implements", cls.Implements, SyncFlag.GetOnly);

        sync.Sync("Body", cls.Body, SyncFlag.GetOnly);
    }
}

/// <summary>
/// Synchronization proxy for <see cref="ClassField"/> objects.
/// </summary>
[SyncProxyUsage(typeof(ClassField))]
public class ClassFieldProxy : SyncObjectProxy
{
    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        ClassField field = TargetAs<ClassField>();

        field.Id ??= new() { Type = SyntaxNodes.Identifier };

        field.Id.Name = sync.Sync("name", field.Id.Name, SyncFlag.AttributeMode);
        field.AccessMode = sync.SyncEnumAttribute("access", field.AccessMode);
        field.StaticMode = sync.SyncEnumAttribute("static", field.StaticMode);

        field.TypeName = sync.Sync("typeName", field.TypeName, SyncFlag.AttributeMode);
        field.Init = sync.Sync("Init", field.Init);
    }
}

/// <summary>
/// Synchronization proxy for <see cref="EnumField"/> objects.
/// </summary>
[SyncProxyUsage(typeof(EnumField))]
public class EnumFieldProxy : SyncObjectProxy
{
    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        EnumField field = TargetAs<EnumField>();

        field.Id ??= new() { Type = SyntaxNodes.Identifier };

        field.Id.Name = sync.Sync("name", field.Id.Name, SyncFlag.AttributeMode);
    }
}

/// <summary>
/// Synchronization proxy for <see cref="ClassMethod"/> objects.
/// </summary>
[SyncProxyUsage(typeof(ClassMethod))]
public class ClassMethodProxy : SyncObjectProxy
{
    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        ClassMethod func = TargetAs<ClassMethod>();

        func.Id ??= new() { Type = SyntaxNodes.Identifier };
        func.Parameters ??= [];
        func.Defaults ??= [];

        func.Id.Name = sync.Sync("name", func.Id.Name, SyncFlag.AttributeMode);
        func.AccessMode = sync.SyncEnumAttribute("access", func.AccessMode);
        func.StaticMode = sync.SyncEnumAttribute("static", func.StaticMode);
        func.IsConstructor = sync.SyncBooleanAttribute("constructor", func.IsConstructor);

        func.TypeName = sync.Sync("typeName", func.TypeName, SyncFlag.AttributeMode);

        sync.Sync("Parameters", func.Parameters, SyncFlag.GetOnly);
        sync.Sync("Defaults", func.Defaults, SyncFlag.GetOnly);

        func.Language = sync.Sync("language", func.Language, SyncFlag.AttributeMode);
        func.Code = sync.Sync("Code", func.Code);
        func.Element = sync.Sync("element", func.Element, SyncFlag.AttributeMode);
        func.Ast = sync.SyncBooleanAttribute("ast", func.Ast);
    }
}

/// <summary>
/// Synchronization proxy for <see cref="ClassNativeCode"/> objects.
/// </summary>
[SyncProxyUsage(typeof(ClassNativeCode))]
public class NativeCodeProxy : SyncObjectProxy
{
    /// <inheritdoc/>
    public override void Sync(IPropertySync sync, ISyncContext context)
    {
        ClassNativeCode n = TargetAs<ClassNativeCode>();

        n.Language = sync.Sync("language", n.Language, SyncFlag.AttributeMode);
        n.Code = sync.Sync("Code", n.Code);
    }
}