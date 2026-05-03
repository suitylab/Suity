using Suity.Collections;
using Suity.Drawing;
using Suity.Editor.CodeRender;
using System;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Types;

/// <summary>
/// Represents a logic module type in the editor.
/// </summary>
[AssetTypeBinding(AssetDefNames.LogicModule, "Logic Module")]
public class DLogicModule : DType
{
    /// <summary>
    /// Gets the default color for logic module types.
    /// </summary>
    public static Color LogicModuleColor { get; } = Color.FromArgb(255, 80, 224);

    /// <summary>
    /// Initializes a new instance of the DLogicModule class.
    /// </summary>
    public DLogicModule()
    { }

    /// <summary>
    /// Initializes a new instance of the DLogicModule class with a name.
    /// </summary>
    public DLogicModule(string name)
        : base(name)
    {
    }

    private Guid[] _components = [];

    /// <summary>
    /// Gets or sets the component IDs.
    /// </summary>
    public Guid[] Components
    {
        get => [.. _components];
        protected internal set
        {
            value ??= [];

            if (ArrayHelper.ArrayEquals(_components, value))
            {
                return;
            }

            _components = [.. value];

            NotifyPropertyUpdated();
        }
    }

    public override ImageDef DefaultIcon => CoreIconCache.LogicModule;
    public override Color? TypeColor => LogicModuleColor;
    public override RenderType RenderType => RenderType.LogicModule;

    public override object GetProperty(CodeRenderProperty property, object argument) => property.PropertyName switch
    {
        CodeRenderProperty.Components => _components.Select(AssetManager.Instance.GetAsset),
        _ => base.GetProperty(property, argument),
    };
}

public class DLogicModuleBuilder : DTypeBuilder<DLogicModule>
{
    public Func<Guid[]> ComponentGetter;
    private int _update;

    public DLogicModuleBuilder()
    {
        AddAutoUpdate(nameof(DLogicModule.Components), m =>
        {
            m.Components = ComponentGetter?.Invoke() ?? [];
        });
    }

    public void BeginUpdate()
    {
        _update++;
    }

    public void UpdateComponents()
    {
        if (_update == 0)
        {
            UpdateAuto(nameof(DLogicModule.Components));
        }
    }

    public void EndUpdate()
    {
        if (_update > 0)
        {
            _update--;
            if (_update == 0)
            {
                UpdateAuto(nameof(DLogicModule.Components));
            }
        }
    }
}