using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor;
using Suity.Editor.Analyzing;
using Suity.Helpers;
using Suity.Reflecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Synchonizing.Preset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suity.Views.Named;

/// <summary>
/// Backend implementation of <see cref="NamedExternal"/> providing factory methods, text manipulation, and type resolution for named views.
/// </summary>
internal class NamedExternalBK : NamedExternal
{
    /// <summary>
    /// Gets the singleton instance of <see cref="NamedExternalBK"/>.
    /// </summary>
    public static readonly NamedExternalBK Instance = new();

    internal ISyncTypeResolver _globalResolver;

    /// <summary>
    /// Initializes the external backend by registering it as the active <see cref="NamedExternal"/> implementation.
    /// </summary>
    public void Initialize()
    {
        NamedExternal._external = this;
    }

    /// <inheritdoc/>
    public override RootCollectionExternal CreateRootCollectionEx(NamedRootCollection collection)
    {
        return new RootCollectionExternalBK(collection);
    }

    /// <inheritdoc/>
    public override INamedItemList CreateItemList(Predicate<object> canDropIn, Func<object, object> dropInConvert)
    {
        return new NamedItemList(canDropIn, dropInConvert);
    }

    /// <inheritdoc/>
    public override INamedSyncList<TValue> CreateNamedSyncList<TValue>(string nameField)
    {
        return new NamedSyncList<TValue>(nameField);
    }

    /// <inheritdoc/>
    public override INamedRenderTargetList CreateRenderTargetList(ISupportAnalysis analysis)
    {
        return new NamedRenderTargetList(analysis);
    }

    /// <inheritdoc/>
    public override INamedUsingList CreateUsingList(string fieldDescription, IEnumerable<StorageLocation> fileNames, object owner = null)
    {
        return new NamedStorageUsingList(fieldDescription, fileNames, owner);
    }

    /// <inheritdoc/>
    public override INamedUsingList CreateUsingList(string fieldDescription, IEnumerable<Guid> ids, object owner = null)
    {
        return new NamedObjectUsingList(fieldDescription, ids, owner);
    }

    /// <inheritdoc/>
    public override void SetText(NamedItem item, string text, ISyncContext setup, bool showNotice)
    {
        if (item.Name == text)
        {
            return;
        }

        if (!item.OnVerifyName(text))
        {
            if (showNotice)
            {
                QueuedAction.Do(() => DialogUtility.ShowMessageBoxAsyncL("Does not conform to naming rules"));
            }

            return;
        }

        if (item.Root != null)
        {
            if (!item.Root.ContainsItem(text, true))
            {
                if (setup != null)
                {
                    setup.DoServiceAction<IViewSetValue>(v => v.SetValue("Name", text));
                }
                else
                {
                    item.SetProperty("Name", text);
                }
            }
            else
            {
                if (showNotice)
                {
                    QueuedAction.Do(() => DialogUtility.ShowMessageBoxAsyncL("Name already exists"));
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void SetText(NamedField item, string text, ISyncContext setup, bool showNotice)
    {
        if (item.Name == text)
        {
            return;
        }

        if (!item.OnVerifyName(text))
        {
            if (showNotice)
            {
                QueuedAction.Do(() => DialogUtility.ShowMessageBoxAsyncL("Does not conform to naming rules"));
            }

            return;
        }

        if (item.List != null)
        {
            if (!item.List.Contains(text))
            {
                setup.DoServiceAction<IViewSetValue>(v => v.SetValue("Name", text));
            }
            else
            {
                if (showNotice)
                {
                    QueuedAction.Do(() => DialogUtility.ShowMessageBoxAsyncL("Name already exists"));
                }
            }
        }
    }

    /// <inheritdoc/>
    public override NamedItem CreateDefaultItem(NamedNode parentNode, NamedItemCreate itemCreate)
    {
        NamedItem item = parentNode?.CreateDefaultItem();
        item ??= itemCreate?.Invoke();

        if (item is null)
        {
            return null;
        }

        if (parentNode != null && item == parentNode)
        {
            // Invalid
            return null;
        }

        if (item.ParentNode != null)
        {
            // Invalid
            return null;
        }

        return item;
    }

    /// <inheritdoc/>
    public override async Task<NamedItem[]> GuiCreateItem(NamedNode parentNode, NamedItemGuiCreate itemCreate, NamedItemGuiConfig itemConfig)
    {
        NamedItem[] items = null;

        if (parentNode != null)
        {
            items = await parentNode.GuiCreateItems();
        }

        if (items is null && itemCreate != null)
        {
            items = await itemCreate();
        }

        if (items is null)
        {
            return null;
        }

        foreach (var item in items)
        {
            if (item is null)
            {
                // Invalid
                return null;
            }

            if (parentNode != null && item == parentNode)
            {
                // Invalid
                return null;
            }

            if (item.ParentNode != null)
            {
                // Invalid
                return null;
            }

            // Configure value
            bool config = false;
            if (parentNode != null)
            {
                config = await parentNode.GuiConfigItem(item);
            }

            if (!config && itemConfig != null)
            {
                config = await itemConfig(item);
            }

            if (!config)
            {
                return null;
            }
        }

        return items;
    }

    /// <inheritdoc/>
    public override string ResolveTypeName(Type baseItemType, Type type, object obj)
    {
        if (obj != null)
        {
            type = obj.GetType();
        }

        if (baseItemType == type)
        {
            return string.Empty;
        }
        else if (baseItemType?.IsAssignableFrom(type) == true)
        {
            return _globalResolver?.ResolveTypeName(type, obj) ?? type.GetTypeId();
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override Type ResolveType(Type baseItemType, string typeName, string parameter)
    {
        Type type = _globalResolver?.ResolveType(typeName, parameter) ?? InternalTypeResolve.ResolveType(typeName);
        if (type is null)
        {
            return baseItemType;
        }

        if (baseItemType?.IsAssignableFrom(type) == true)
        {
            return type;
        }
        else
        {
            return null;
        }
    }
}
