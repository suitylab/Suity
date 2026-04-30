using System;

namespace Suity.Views.Im.PropertyEditing;

/// <summary>
/// Numeric property field editor for struct-based numeric types (int, float, double, etc.).
/// Renders a property row with a numeric input control in the main column.
/// </summary>
/// <typeparam name="T">The numeric struct type to edit.</typeparam>
internal class NumericPropertyField<T> : ImGuiPropertyEditor<T> where T : struct
{
    /// <inheritdoc/>
    public override ImGuiNode RowFunction(ImGui gui, PropertyTarget target, PropertyRowAction? rowAction)
    {
        return gui.PropertyRow(target, (n, inner, column, pipeline) =>
        {
            if (pipeline.HasFlag(GuiPipeline.Main))
            {
                rowAction?.Invoke(n, column, GuiPipeline.PreAction);

                switch (column)
                {
                    case PropertyGridColumn.Prefix:
                        break;

                    case PropertyGridColumn.Name:
                        break;

                    case PropertyGridColumn.Main:
                        gui.NumericEditor<T>(inner, act => n.DoValueAction(act))
                        .SetPropertyTitleColor(target)
                        .SetFullWidth();
                        break;

                    case PropertyGridColumn.Option:
                        break;

                    default:
                        break;
                }

                rowAction?.Invoke(n, column, GuiPipeline.Main | GuiPipeline.PostAction);
            }
        });
    }

    /// <inheritdoc/>
    public override ImGuiNode? EditorFunction(ImGui gui, IValueTarget target, Action<IValueAction> handler)
    {
        return gui.NumericEditor<T>(target, handler)
            .SetFullWidth();
    }
}
