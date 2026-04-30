using Suity.Synchonizing;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Suity.Views;

public static class IViewObjectSetupExtensions
{
    #region Inspector

    public static bool SupportInspector(this IViewObjectSetup setup)
    {
        return setup.IsViewIdSupported(ViewIds.Inspector);
    }

    public static void InspectorFieldOf<T>(this IViewObjectSetup setup, ViewProperty property)
    {
        property.ViewId = ViewIds.Inspector;
        setup.AddField(typeof(T), property);
    }

    public static void InspectorField<T>(this IViewObjectSetup setup, T value, ViewProperty property)
    {
        property.ViewId = ViewIds.Inspector;
        var type = value?.GetType() ?? typeof(T);
        setup.AddField(type, property);
    }


    [Obsolete("This method may cause problems when in the method coverage process and should be abandoned.")]
    public static void AllInspectorField(this IViewObjectSetup setup, ISyncObject obj, bool expand = false, bool readOnly = false)
    {
        var sync = new ViewObjectSetupAllFieldSync(setup, null, expand, readOnly, ViewIds.Inspector);
        obj.Sync(sync, EmptySyncContext.Empty);
    }

    [Obsolete("This method may cause problems when in the method coverage process and should be abandoned.")]
    public static void AllInspectorField(this IViewObjectSetup setup, ISyncObject obj, Predicate<string> predicate, bool expand = false, bool readOnly = false)
    {
        var sync = new ViewObjectSetupAllFieldSync(setup, predicate, expand, readOnly, ViewIds.Inspector);
        obj.Sync(sync, EmptySyncContext.Empty);
    }

    #endregion

    #region TreeView

    public static bool SupportTreeView(this IViewObjectSetup setup)
    {
        return setup.IsViewIdSupported(ViewIds.TreeView);
    }

    public static void TreeViewFieldOf<T>(this IViewObjectSetup setup, ViewProperty property)
    {
        property.ViewId = ViewIds.TreeView;
        setup.AddField(typeof(T), property);
    }

    public static void TreeViewField<T>(this IViewObjectSetup setup, T value, ViewProperty property)
    {
        property.ViewId = ViewIds.TreeView;
        Type type = value != null ? value.GetType() : typeof(T);
        setup.AddField(type, property);
    }

    public static void AllTreeViewField(this IViewObjectSetup setup, ISyncObject obj, bool expand = false, bool readOnly = false)
    {
        var sync = new ViewObjectSetupAllFieldSync(setup, null, expand, readOnly, ViewIds.TreeView);
        obj.Sync(sync, EmptySyncContext.Empty);
    }

    public static void AllTreeViewField(this IViewObjectSetup setup, ISyncObject obj, Predicate<string> predicate, bool expand = false, bool readOnly = false)
    {
        var sync = new ViewObjectSetupAllFieldSync(setup, predicate, expand, readOnly, ViewIds.TreeView);
        obj.Sync(sync, EmptySyncContext.Empty);
    }

    #endregion

    #region MainTreeView

    public static bool SupportMainTreeView(this IViewObjectSetup setup)
    {
        return setup.IsViewIdSupported(ViewIds.MainTreeView);
    }

    public static void MainTreeViewFieldOf<T>(this IViewObjectSetup setup, ViewProperty property)
    {
        property.ViewId = ViewIds.MainTreeView;
        setup.AddField(typeof(T), property);
    }

    public static void MainTreeViewField<T>(this IViewObjectSetup setup, T value, ViewProperty property)
    {
        property.ViewId = ViewIds.MainTreeView;
        Type type = value != null ? value.GetType() : typeof(T);
        setup.AddField(type, property);
    }

    public static void AllMainTreeViewField(this IViewObjectSetup setup, ISyncObject obj, bool expand = false, bool readOnly = false)
    {
        var sync = new ViewObjectSetupAllFieldSync(setup, null, expand, readOnly, ViewIds.MainTreeView);
        obj.Sync(sync, EmptySyncContext.Empty);
    }

    public static void AllMainTreeViewField(this IViewObjectSetup setup, ISyncObject obj, Predicate<string> predicate, bool expand = false, bool readOnly = false)
    {
        var sync = new ViewObjectSetupAllFieldSync(setup, predicate, expand, readOnly, ViewIds.MainTreeView);
        obj.Sync(sync, EmptySyncContext.Empty);
    }

    #endregion

    #region DetailTreeView

    public static bool SupportDetailTreeView(this IViewObjectSetup setup)
    {
        return setup.IsViewIdSupported(ViewIds.DetailTreeView);
    }

    public static void DetailTreeViewFieldOf<T>(this IViewObjectSetup setup, ViewProperty property)
    {
        property.ViewId = ViewIds.DetailTreeView;
        setup.AddField(typeof(T), property);
    }

    public static void DetailTreeViewField<T>(this IViewObjectSetup setup, T value, ViewProperty property)
    {
        property.ViewId = ViewIds.DetailTreeView;
        Type type = value != null ? value.GetType() : typeof(T);
        setup.AddField(type, property);
    }

    public static void AllDetailTreeViewField(this IViewObjectSetup setup, ISyncObject obj, bool expand = false, bool readOnly = false)
    {
        var sync = new ViewObjectSetupAllFieldSync(setup, null, expand, readOnly, ViewIds.DetailTreeView);
        obj.Sync(sync, EmptySyncContext.Empty);
    }

    public static void AllDetailTreeViewField(this IViewObjectSetup setup, ISyncObject obj, Predicate<string> predicate, bool expand = false, bool readOnly = false)
    {
        var sync = new ViewObjectSetupAllFieldSync(setup, predicate, expand, readOnly, ViewIds.DetailTreeView);
        obj.Sync(sync, EmptySyncContext.Empty);
    }

    #endregion

    #region Extended

    public static bool SupportExtended(this IViewObjectSetup setup)
    {
        return setup.IsViewIdSupported(ViewIds.Extended);
    }

    public static void ExtendedFieldOf<T>(this IViewObjectSetup setup, ViewProperty property)
    {
        property.ViewId = ViewIds.Extended;
        setup.AddField(typeof(T), property);
    }

    public static void ExtendedField<T>(this IViewObjectSetup setup, T value, ViewProperty property)
    {
        property.ViewId = ViewIds.Extended;
        Type type = value != null ? value.GetType() : typeof(T);
        setup.AddField(type, property);
    }

    #endregion

    #region Empty

    public static IViewObjectSetup Empty(this IViewObjectSetup setup, ViewProperty property)
    {
        property.ViewId = ViewIds.Inspector;
        setup.AddField(typeof(EmptyValue), property);

        return setup;
    }

    public static IViewObjectSetup Empty(this IViewObjectSetup setup, string name, string text, object icon = null)
    {
        var property = new ViewProperty(name, text, icon);

        return setup.Empty(property);
    }

    #endregion

    #region Button


    public static IViewObjectSetup Button(this IViewObjectSetup setup, ViewProperty property)
    {
        property.ViewId = ViewIds.Inspector;
        setup.AddField(typeof(ButtonValue), property);

        return setup;
    }

    public static IViewObjectSetup Button(this IViewObjectSetup setup, string name, string text, object icon = null)
    {
        var property = new ViewProperty(name, text, icon);

        return setup.Button(property);
    }

    #endregion

    #region Tooltips

    public static void Tooltips(this IViewObjectSetup setup, string name, string text, Action<ViewProperty> config = null)
    {
        var prop = new ViewProperty(name, text);
        config?.Invoke(prop);
        setup.InspectorField(TooltipsValue.Empty, prop);
    }

    public static void Tooltips(this IViewObjectSetup setup, string name, TextStatus status, string text)
    {
        var prop = new ViewProperty(name, text).WithStatus(status);
        
        setup.InspectorField(TooltipsValue.Empty, prop);
    }

    public static void Tooltips(this IViewObjectSetup setup, string name, object icon, string text)
    {
        var prop = new ViewProperty(name, text, icon);
        setup.InspectorField(TooltipsValue.Empty, prop);
    }

    public static void Verbose(this IViewObjectSetup setup, string text, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => setup.Tooltips($"tooltips_{member}{line}", TextStatus.Normal, text);

    public static void Info(this IViewObjectSetup setup, string text, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => setup.Tooltips($"tooltips_{member}{line}", TextStatus.Info, text);

    public static void Warning(this IViewObjectSetup setup, string text, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => setup.Tooltips($"tooltips_{member}{line}", TextStatus.Warning, text);

    public static void Error(this IViewObjectSetup setup, string text, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null)
        => setup.Tooltips($"tooltips_{member}{line}", TextStatus.Error, text);

    #endregion

    #region Label

    public static void Label(this IViewObjectSetup setup, string text, Action<ViewProperty> config = null)
    {
        Label(setup, text, text, config);
    }

    public static void Label(this IViewObjectSetup setup, string name, string text, Action<ViewProperty> config = null)
    {
        var prop = new ViewProperty(name, text);
        config?.Invoke(prop);
        setup.InspectorField(LabelValue.Empty, prop);
    }

    public static void Label(this IViewObjectSetup setup, ViewProperty property)
    {
        setup.InspectorField(LabelValue.Empty, property);
    }

    public static void LabelWithIcon(this IViewObjectSetup setup, string text, object icon, Action<ViewProperty> config = null)
    {
        var prop = new ViewProperty(text, text) { Icon = icon };
        config?.Invoke(prop);
        setup.InspectorField(LabelValue.Empty, prop);
    }

    public static void LabelWithIcon(this IViewObjectSetup setup, string name, string text, object icon, Action<ViewProperty> config = null)
    {
        var prop = new ViewProperty(name, text) { Icon = icon };
        config?.Invoke(prop);
        setup.InspectorField(LabelValue.Empty, prop);
    }

    #endregion

    #region ViewObjectSetupAllFieldSync

    private class ViewObjectSetupAllFieldSync : IPropertySync
    {
        private readonly IViewObjectSetup _setup;
        private Predicate<string> _predicate;
        private readonly bool _expand;
        private readonly bool _readOnly;
        private readonly int _viewId;

        public ViewObjectSetupAllFieldSync(IViewObjectSetup setup, Predicate<string> predicate, bool expand, bool readOnly, int viewId)
        {
            _setup = setup;
            _predicate = predicate;
            _expand = expand;
            _readOnly = readOnly;
            _viewId = viewId;
        }

        public SyncMode Mode => SyncMode.GetAll;

        public SyncIntent Intent => SyncIntent.View;

        public string Name => null;

        public IEnumerable<string> Names => [];

        public object Value => null;

        public T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null)
        {
            if (_predicate == null || _predicate(name))
            {
                _setup.AddField(typeof(T), new ViewProperty(name)
                {
                    Description = description,
                    Expand = _expand,
                    ReadOnly = _readOnly,
                    ViewId = _viewId
                });
            }
            return obj;
        }
    }

    #endregion
}