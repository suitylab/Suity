using static Suity.Helpers.GlobalLocalizer;
using ComputerBeacon.Json;
using MarkedNet;
using Suity.Editor.Design;
using Suity.Editor.Expressions;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Suity.Editor.CodeRender;
using Suity.Editor.CodeRender.Json;

namespace Suity.Editor.Services;

internal class JsonResourceService : IJsonResourceService
{
    public static JsonResourceService Instance { get; } = new();

    private readonly object _sync = new();

    public bool TryExtractJson(string s, out JsonObject obj)
    {
        s = s?.TrimStart();

        if (string.IsNullOrWhiteSpace(s))
        {
            obj = null;
            return false;
        }

        if (s.StartsWith("{"))
        {
            if (TryParseJson(s, out obj))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        var tokens = MarkedNet.Lexer.Lex(s, new Options());
        if (tokens is null || tokens.Tokens.Count == 0)
        {
            obj = null;

            return false;
        }

        foreach (var token in tokens.Tokens.Where(o => o.Type == "code"))
        {
            string c = token.Text?.TrimStart() ?? string.Empty;

            if (c.StartsWith("{") && TryParseJson(c, out obj))
            {
                return true;
            }
        }

        obj = null;

        return false;
    }

    private bool TryParseJson(string s, out JsonObject obj)
    {
        try
        {
            obj = ComputerBeacon.Json.Parser.Parse(s) as JsonObject;
            if (obj != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception)
        {
            obj = null;

            return false;
        }
    }

    public JsonObject GetJson(Guid id, string materialName = null, JsonResourceOptions options = null)
    {
        lock (_sync)
        {
            Asset asset = AssetManager.Instance.GetAsset(id, AssetFilters.Default);
            IRenderable renderable = asset as IRenderable;
            IMaterial material = AssetManager.Instance.GetAsset<IMaterial>(materialName, AssetFilters.Default);

            if (asset is IDataTableAsset dataAsset)
            {
                return MakeDataFamily(id, dataAsset, options);
            }

            if (asset is IDataAsset dataRowAsset)
            {
                return MakeDataRow(dataRowAsset, options);
            }
            else if (renderable != null)
            {
                material ??= renderable.DefaultMaterial;

                return GetJson(id, renderable, material, options);
            }
            else
            {
                if (options?.LogError ?? true)
                {
                    if (renderable is null)
                    {
                        Logs.LogError($"{id}资源不存在");
                    }
                    else if (material is null)
                    {
                        Logs.LogError($"{id}的材质不存在 : {materialName}");
                    }
                    else
                    {
                        Logs.LogError($"{id}无法渲染");
                    }
                }
                return null;
            }
        }
    }

    public JsonObject GetJson(Guid id, IRenderable renderable, IMaterial material, JsonResourceOptions options = null)
    {
        lock (_sync)
        {
            if (renderable is IDataTableAsset dataAsset)
            {
                return MakeDataFamily(id, dataAsset, options);
            }
            if (renderable is IDataAsset dataRowAsset)
            {
                return MakeDataRow(dataRowAsset, options);
            }

            if (renderable is null)
            {
                if (options?.LogError ?? true)
                {
                    Logs.LogError(L("渲染对象不存在") + ": " + id);
                }
                return null;
            }
            if (material is null)
            {
                if (renderable is IMaterial selfMaterial)
                {
                    material = selfMaterial;
                }
                else
                {
                    if (options?.LogError ?? true)
                    {
                        Logs.LogError(L($"渲染对象{renderable}的材质不存在: {id}"));
                    }
                    return null;
                }
            }

            if (!renderable.RenderEnabled)
            {
                if (options?.LogError ?? true)
                {
                    Logs.LogWarning(L($"渲染对象{renderable}已禁用: {id}"));
                }
                return null;
            }

            var config = options?.RenderConfig;

            var naming = config?.Naming ?? SystemNamingOption.Instance;

            var targets = renderable.GetRenderTargets(material, RenderFileName.Empty);
            List<KeyValuePair<RenderTarget, string>> results = [];

            foreach (var target in targets)
            {
                var exprContext = new ExpressionContext(naming)
                {
                    MaterialId = material.Id,
                    RenderTypeId = target.Item?.RenderType.Id ?? Guid.Empty,
                    RenderableId = CodeBinder.Instance.Id(target.Item?.Object),
                    Disabled = config?.Disabled ?? false,
                    Condition = options?.Condition ?? config?.Condition,
                };

                RenderResult result = target.Render(exprContext);
                if (result.Status == RenderStatus.ErrorContinue)
                {
                    if (options?.LogError ?? true)
                    {
                        Logs.LogError(L("渲染出现错误") + ": " + EditorUtility.GetBriefStringL(id));
                    }
                    continue;
                }
                else if (result.Status == RenderStatus.ErrorInterrupt)
                {
                    if (options?.LogError ?? true)
                    {
                        Logs.LogError(L("渲染出现错误被迫中止") + ": " + EditorUtility.GetBriefStringL(id));
                    }
                    break;
                }

                if (result.IsBinary)
                {
                    try
                    {
                        byte[] bytes = result.GetStream().StreamToBytes();
                        string text = Convert.ToBase64String(bytes);
                        results.Add(new KeyValuePair<RenderTarget, string>(target, text));
                    }
                    catch (Exception err)
                    {
                        err.LogError(L("转换Base64失败") + ": " + EditorUtility.GetBriefStringL(id));
                        continue;
                    }
                }
                else
                {
                    string text = result.GetText();

                    if (config != null)
                    {
                        text = Device.Current.GetService<ICodeRenderService>().ReplaceUserCode(config, target, text);
                    }

                    results.Add(new KeyValuePair<RenderTarget, string>(target, text));
                }
            }

            string tableId = CodeBinder.Instance.DataId(id);
            var rootObj = new JsonObject
            {
                ["Key"] = id.ToString(),
            };

            if (!string.IsNullOrEmpty(tableId))
            {
                rootObj["TableId"] = tableId;
            }

            var dataAry = new JsonArray();
            rootObj["Data"] = dataAry;

            foreach (var result in results)
            {
                var dataObj = new JsonObject
                {
                    ["Key"] = result.Key.FileName.PhysicFullPath,
                    ["RawData"] = result.Value,
                };

                string assetType = result.Key.Item?.Object?.GetType()?.ResolveAssetTypeName();
                if (!string.IsNullOrEmpty(assetType))
                {
                    dataObj["AssetType"] = assetType;
                }

                string materialType = result.Key.Material?.GetType()?.ResolveAssetTypeName();
                if (!string.IsNullOrEmpty(materialType))
                {
                    dataObj["MaterialType"] = materialType;
                }

                dataAry.Add(dataObj);
            }

            return rootObj;
        }
    }

    public JsonObject GetJson(IDataInputOwner dataInputOwner, JsonResourceOptions options = null)
    {
        lock (_sync)
        {
            var rootObj = new JsonObject();
            var dataCollectionAry = new JsonArray();
            rootObj["DataCollection"] = dataCollectionAry;

            foreach (var dataInput in dataInputOwner.GetDataInputs())
            {
                var dataCollectionObj = GetJson(dataInput, options);
                if (dataCollectionObj is null)
                {
                    return null;
                }

                dataCollectionAry.Add(dataCollectionObj);
            }

            return rootObj;
        }
    }

    public JsonObject GetJson(IDataInput dataInput, JsonResourceOptions options = null)
    {
        lock (_sync)
        {
            IRenderable renderable = EditorObjectManager.Instance.GetObject(dataInput.RenderableId) as IRenderable;
            IMaterial material = dataInput.Material ?? renderable?.DefaultMaterial;

            if (options?.RenderConfig is null)
            {
                options ??= new JsonResourceOptions();
                options.RenderConfig = dataInput.GetBuildConfig();
                options.Condition ??= options.RenderConfig?.Condition;
            }

            return GetJson(dataInput.RenderableId, renderable, material, options);
        }
    }

    public JsonObject GetJson(IDataItem row, JsonResourceOptions options = null)
    {
        var table = row.DataContainer ?? throw new NullReferenceException(nameof(row.DataContainer));

        string key;
        Guid guidKey = row.DataGuid;
        if (guidKey != Guid.Empty)
        {
            key = guidKey.ToString();
        }
        else
        {
            if (options?.LogError ?? true)
            {
                Logs.LogError(L("数据的Guid丢失") + $": {table.Name}-{row.Name}");
            }

            key = null;
        }

        string localId = row.DataLocalId;
        if (string.IsNullOrWhiteSpace(localId))
        {
            if (options?.LogError ?? true)
            {
                Logs.LogError(L("数据的LocalId丢失") + $": {table.Name}-{row.Name}");
            }
        }

        var dataObj = new JsonObject
        {
            ["Key"] = key,
            ["LocalId"] = localId,
        };

        var componentAry = GetJsonComponents(row, options);
        dataObj["Component"] = componentAry;

        return dataObj;
    }

    public JsonArray GetJsonComponents(IDataItem row, JsonResourceOptions options = null)
    {
        var componentAry = new JsonArray();

        var fieldFilter = options?.FieldFilter;

        // 这里不知道为什么会出现破坏枚举的问题，添加ToArray
        foreach (var comp in row.Components.ToArray())
        {
            var jobj = comp.ToJson(true, true, options?.Condition, propGetter: (o, field) =>
            {
                if (fieldFilter != null && !fieldFilter(field))
                {
                    return null;
                }

                return o.EnsureAutoFieldProperty(field, row, options?.Condition);
            });

            componentAry.Add(jobj);
        }

        return componentAry;
    }

    public object GetJson(SItem item, bool writeTypeName = true, JsonResourceOptions options = null)
    {
        if (item is null)
        {
            return null;
        }

        IDataItem row = item.RootContext as IDataItem;

        var condition = options?.Condition ?? options?.RenderConfig?.Condition;
        var fieldFilter = options?.FieldFilter;

        switch (item)
        {
            case SObject obj:
                {
                    var jobj = obj.ToJson(true, writeTypeName, condition, propGetter: (o, field) =>
                    {
                        if (fieldFilter != null && !fieldFilter(field))
                        {
                            return null;
                        }

                        return o.EnsureAutoFieldProperty(field, row, condition);
                    });

                    return jobj;
                }

            case SArray ary:
                {
                    var jary = ary.ToJson(true, writeTypeName, condition, propGetter: (o, field) =>
                    {
                        if (fieldFilter != null && !fieldFilter(field))
                        {
                            return null;
                        }

                        return o.EnsureAutoFieldProperty(field, row, condition);
                    });

                    return jary;
                }

            default:
                return null;
        }
    }

    public SItem FromJson(object obj, SItemResourceOptions options = null)
    {
        switch (obj)
        {
            case JsonObject jobj:
                return jobj.FromJson(options);

            case JsonArray jary:
                return jary.FromJson(options);

            case JsonDataReader reader:
                return FromJson(reader.ReadObject(), options);

            case string s:
                {
                    var objParse = ComputerBeacon.Json.Parser.Parse(s);
                    return FromJson(objParse, options);
                }

            default:
                return null;
        }
    }

    public Dictionary<string, object> FromJson(JsonObject obj, SimpleType type, SItemResourceOptions options = null)
    {
        return obj.FromJson(type, options);
    }




    private JsonObject MakeDataFamily(Guid id, IDataTableAsset dataAsset, JsonResourceOptions options = null)
    {
        if (dataAsset is null)
        {
            if (options?.LogError ?? true)
            {
                Logs.LogError(L("数据输入资产不存在") + ": " + id);
            }
            return null;
        }

        IDataContainer table;

        try
        {
            table = dataAsset.GetDataContainer(true);
        }
        catch (Exception err)
        {
            Logs.LogError(err);

            return null;
        }

        if (table is null)
        {
            if (options?.LogError ?? true)
            {
                Logs.LogError(L("无法在数据输入中找到数据源") + ": " + id);
            }
            return null;
        }

        var rootObj = new JsonObject
        {
            ["Key"] = table.Id.ToString(),
            ["TableId"] = table.TableId,
        };

        var dataAry = new JsonArray();
        rootObj["Data"] = dataAry;

        foreach (var row in table.Datas)
        {
            var dataObj = GetJson(row, options);
            dataAry.Add(dataObj);
        }

        return rootObj;
    }

    private JsonObject MakeDataRow(IDataAsset dataRowAsset, JsonResourceOptions options = null)
    {
        if (dataRowAsset is not Asset asset)
        {
            return null;
        }

        if (asset?.ParentAsset is not IDataTableAsset data)
        {
            return null;
        }

        var table = data.GetDataContainer(true);
        var row = table?.GetData(asset.LocalName);
        if (row is null)
        {
            return null;
        }

        return GetJson(row, options);
    }
}