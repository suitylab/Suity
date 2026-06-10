using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Model.Core;
using Suity.Views.Gui;
using System;

namespace Suity.Editor.Controls;

public static class DockExtensions
{
    public static DockMode? ToDockMode(this DockHint dockHint)
    {
        switch (dockHint)
        {
            case DockHint.Document:
                return DockMode.Center;

            case DockHint.Float:
                return DockMode.Center;

            case DockHint.Top:
                return DockMode.Top;

            case DockHint.Left:
                return DockMode.Left;

            case DockHint.Bottom:
                return DockMode.Bottom;

            case DockHint.Right:
                return DockMode.Right;

            default:
                return null;
        }
    }

    public static void RemoveFromVisualTree(this Control control)
    {
        if (control == null) return;

        // 1. 检查逻辑树父级 (Logical Parent)
        if (control.Parent is Panel panelParent)
        {
            panelParent.Children.Remove(control);
        }
        else if (control.Parent is ContentControl contentParent)
        {
            contentParent.Content = null;
        }
        else if (control.Parent is Decorator decoratorParent) // 例如 Border
        {
            decoratorParent.Child = null;
        }
        // 2. 针对特殊情况，如果逻辑树没断，但视觉树有 Parent 的兜底处理
        else if (control.GetVisualParent() is Panel visualPanel)
        {
            visualPanel.Children.Remove(control);
        }
    }

    /// <summary>
    /// 将一个控件安全地加入到指定的父容器中
    /// </summary>
    /// <param name="child">要添加的子控件</param>
    /// <param name="parent">目标父容器</param>
    /// <exception cref="ArgumentNullException">当子控件或父控件为空时抛出</exception>
    /// <exception cref="NotSupportedException">当父容器类型不支持直接添加子控件时抛出</exception>
    public static void AddToVisualTree(this Control child, StyledElement parent)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (parent == null) throw new ArgumentNullException(nameof(parent));

        // 1. 核心安全检查：如果子控件已经有父级，必须先从旧父级断开
        if (child.Parent != null)
        {
            RemoveFromCurrentParent(child);
        }

        // 2. 根据父容器的类型，选择合适的挂载方式
        switch (parent)
        {
            // 情况 A: 父容器是 Panel (如 Grid, StackPanel, Canvas)
            case Panel panelParent:
                panelParent.Children.Add(child);
                break;

            // 情况 B: 父容器是 ContentControl (如 Button, UserControl, Window)
            case ContentControl contentParent:
                // 如果原本有内容，通常需要覆盖或抛出异常，这里选择覆盖
                contentParent.Content = child;
                break;

            // 情况 C: 父容器是 Decorator (如 Border, Viewbox)
            case Decorator decoratorParent:
                decoratorParent.Child = child;
                break;

            // 情况 D: 无法直接识别的容器类型
            default:
                throw new NotSupportedException($"暂时不支持将控件直接添加到 {parent.GetType().Name} 类型的容器中。");
        }
    }

    /// <summary>
    /// 辅助方法：将控件从它当前的父级中斩断（内部安全清理）
    /// </summary>
    private static void RemoveFromCurrentParent(Control child)
    {
        if (child.Parent is Panel panel)
        {
            panel.Children.Remove(child);
        }
        else if (child.Parent is ContentControl contentControl)
        {
            contentControl.Content = null;
        }
        else if (child.Parent is Decorator decorator)
        {
            decorator.Child = null;
        }
    }
}
