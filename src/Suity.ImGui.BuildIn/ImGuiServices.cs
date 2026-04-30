using Suity.Views.Graphics;
using Suity.Views.Im;
using Suity.Views.Im.Flows;
using Suity.Views.Im.PropertyEditing;
using Suity.Views.Im.PropertyEditing.Targets;
using Suity.Views.Im.PropertyEditing.ViewObjects;
using Suity.Views.Im.TreeEditing;

namespace Suity;

/// <summary>
/// Provides initialization and factory methods for ImGui services.
/// </summary>
public static class ImGuiServices
{
    private static bool _init;

    /// <summary>
    /// Static constructor that ensures services are initialized.
    /// </summary>
    static ImGuiServices()
    {
        Initialize();
    }

    /// <summary>
    /// Initializes all ImGui-related external backends and extension points.
    /// </summary>
    public static void Initialize()
    {
        if (_init)
        {
            return;
        }

        _init = true;

        ImGuiExternalBK.Instance.Initialize();

        PropertyGridExtensions._external = PropertyGridExternalBK.Instance;
        PropertyFieldExtensions._external = PropertyFieldExternalBK.Instance;
        PropertyTargetUtility._external = PropertyTargetExternalBK.Instance;
        ActionSetterExtensions._external = ActionSetterExternalBK.Instance;
        EditorTemplates._external = EditorTemplateExternalBK.Instance;
        SValueEditorTemplates._external = SValueEditorExternalBK.Instance;
        ImGuiPathTreeExternal._external = ImGuiPathTreeExternalBK.Instance;
        NodeGraphExtensions._external = NodeGraphExternalBK.Instance;
    }

    /// <summary>
    /// Creates a new <see cref="ImGui"/> instance with the specified context and configuration.
    /// </summary>
    /// <param name="context">The graphic context for rendering.</param>
    /// <param name="config">The configuration for the ImGui instance.</param>
    /// <returns>A new <see cref="ImGui"/> instance.</returns>
    public static ImGui CreateImGui(IGraphicContext context, ImGuiConfig config)
    {
        if (!_init)
        {
            Initialize();
        }

        return new ImGuiBK(context, config);
    }
}