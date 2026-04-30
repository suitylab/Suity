using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Editor.Documents;
using Suity.Editor.Expressions;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using System;
using System.Collections.Generic;
using System.Text;

namespace Suity.Editor;

/// <summary>
/// Core editor plugin that registers fundamental services such as clipboard, reference manager,
/// analysis service, and defines render types including struct, enum, function, data, component, and more.
/// </summary>
public class CorePlugin : EditorPlugin
{
    /// <summary>
    /// Gets the thread-safe pool of <see cref="StringBuilder"/> instances for efficient string building.
    /// </summary>
    internal static ConcurrentPool<StringBuilder> StringBuilderPool { get; } = new(() => new());

    private readonly Dictionary<Type, object> _services = [];

    /// <inheritdoc/>
    public override string Description => "Core";

    /// <inheritdoc/>
    internal protected override void Awake(PluginContext context)
    {
        base.Awake(context);

        _services[typeof(IRunDelayed)] = RunDelayed.Default;
        _services[typeof(IPluginService)] = PluginManager.Instance;
        _services[typeof(IAssemblyService)] = PluginManager.Instance;
        _services[typeof(NativeTypeReflector)] = NativeTypeReflector.Instance;
        _services[typeof(AssetActivatorManager)] = AssetActivatorManager.Instance;
        _services[typeof(DocumentManager)] = DocumentManagerBK.Instance;
        _services[typeof(LinkedAssetInitializer)] = LinkedAssetInitializer.Instance;

        _services[typeof(IClipboardService)] = ClipboardService.Instance;

        _services[typeof(SValueExternal)] = SValueExternalBK.Instance;
        _services[typeof(ReferenceManager)] = ReferenceManagerBK.Instance;
        _services[typeof(AnalysisService)] = AnalysisServiceBK.Instance;

        _services[typeof(IJsonSchemaService)] = JsonSchemaService.Instance;
        _services[typeof(ITypeConvertService)] = TypeConvertService.Instance;
    }

    /// <inheritdoc/>
    protected internal override void Start(PluginContext context)
    {
        base.Start(context);


        EditorCommands.Render.AddActionListener(() =>
        {
            EditorUtility.StartBuildTask(() => CodeRenderUtility.RenderAll(false));
        });

        EditorCommands.RenderIncremental.AddActionListener(() =>
        {
            EditorUtility.StartBuildTask(() => CodeRenderUtility.RenderAll(true));
        });
    }

    /// <inheritdoc/>
    protected internal override void AwakeProject()
    {
        base.AwakeProject();

        GroupAssetBuilder group = new GroupAssetBuilder()
            .WithLocalName("*RenderType")
            .WithAsset();

        RenderType.TypeFamily = new RenderType(RenderType.TypeFamilyName, "Type Family", CoreIconCache.Class).WithGroup(group);
        RenderType.TypeFormatter = new RenderType(RenderType.TypeFormatterName, "Type Formatter", CoreIconCache.Format).WithGroup(group);
        RenderType.Struct = new RenderType(RenderType.StructName, "Structure", CoreIconCache.Box).WithGroup(group);
        RenderType.Abstract = new RenderType(RenderType.AbstractName, "Abstract Structure", CoreIconCache.Abstract).WithGroup(group);
        RenderType.Enum = new RenderType(RenderType.EnumName, "Enumeration", CoreIconCache.Enum).WithGroup(group);
        RenderType.LogicModule = new RenderType(RenderType.LogicModuleName, "Logic Module", CoreIconCache.LogicModule).WithGroup(group);
        RenderType.FunctionFamily = new RenderType(RenderType.FunctionFamilyName, "Function Family", CoreIconCache.Function).WithGroup(group);
        RenderType.Function = new RenderType(RenderType.FunctionName, "Function", CoreIconCache.Function).WithGroup(group);
        RenderType.DataFamily = new RenderType(RenderType.DataFamilyName, "Data Table", CoreIconCache.Data).WithGroup(group);
        RenderType.Data = new RenderType(RenderType.DataName, "Data", CoreIconCache.Row).WithGroup(group);
        RenderType.TriggerController = new RenderType(RenderType.TriggerControllerName, "Trigger Controller", CoreIconCache.Controller).WithGroup(group);
        RenderType.Component = new RenderType(RenderType.ComponentName, "Component", CoreIconCache.Component).WithGroup(group);
        RenderType.RexTree = new RenderType(RenderType.RexTreeName, "Rex Tree", CoreIconCache.Rex).WithGroup(group);
        RenderType.Binary = new RenderType(RenderType.BinaryName, "Binary Data", CoreIconCache.Binary).WithGroup(group);
        RenderType.Text = new RenderType(RenderType.TextName, "Text Data", CoreIconCache.Text).WithGroup(group);

        RenderType.Group = group.ResolveAsset();
    }

    /// <inheritdoc/>
    public override object GetService(Type serviceType) => _services.GetValueSafe(serviceType);
}