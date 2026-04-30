using Suity.Editor.Values;
using Suity.Synchonizing;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Gui.InspectorGui;

/// <summary>
/// Test enumeration for editor property testing.
/// </summary>
public enum TestEnum
{
    /// <summary>
    /// First test value.
    /// </summary>
    ValueA,
    /// <summary>
    /// Second test value.
    /// </summary>
    ValueB,
    /// <summary>
    /// Third test value.
    /// </summary>
    ValueC,
}

/// <summary>
/// Test data structure for editor property editing and synchronization testing.
/// </summary>
public class TestEditorStruct
{
    /// <summary>
    /// A boolean test value.
    /// </summary>
    public bool BoolValue;
    /// <summary>
    /// A byte test value.
    /// </summary>
    public byte ByteValue;
    /// <summary>
    /// A float test value.
    /// </summary>
    public float FloatValue;
    /// <summary>
    /// A string test value.
    /// </summary>
    public string StringValue;
    /// <summary>
    /// An object test value.
    /// </summary>
    public object ObjectValue;
    /// <summary>
    /// A test view object for synchronization testing.
    /// </summary>
    public TestViewObject TestSyncObject = new();
    /// <summary>
    /// An enum test value.
    /// </summary>
    public TestEnum EnumValue;
    /// <summary>
    /// A min/max range test value.
    /// </summary>
    public MinMaxValue MinMaxStruct;
    /// <summary>
    /// A list of integers for testing.
    /// </summary>
    public List<int> ListValue = [];
    /// <summary>
    /// A test view list for synchronization testing.
    /// </summary>
    public TestViewList<int> ViewList = new();
    /// <summary>
    /// An SObject for synchronization testing.
    /// </summary>
    public SObject SObj;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestEditorStruct"/> class.
    /// </summary>
    public TestEditorStruct()
    {
    }
}

/// <summary>
/// Property editor for <see cref="TestEditorStruct"/> that displays all fields in a grouped property grid.
/// </summary>
public class TestEditorStructEditor : ImGuiGroupedPropertyEditor<TestEditorStruct>
{
    private readonly ImGuiPropertyField _boolValue;
    private readonly ImGuiPropertyField _byteValue;
    private readonly ImGuiPropertyField _floatValue;
    private readonly ImGuiPropertyField _stringValue;
    private readonly ImGuiPropertyField _objectValue;
    private readonly ImGuiPropertyField _viewObjValue;
    private readonly ImGuiPropertyField _enumValue;
    private readonly ImGuiPropertyField _minMax;
    private readonly ImGuiPropertyField _listValue;
    private readonly ImGuiPropertyField _viewList;
    private readonly ImGuiPropertyField _sobj;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestEditorStructEditor"/> class.
    /// </summary>
    public TestEditorStructEditor()
    {
        _boolValue = CreateField<bool>(
            nameof(TestEditorStruct.BoolValue), o => o.BoolValue, (o, v, ctx) => o.BoolValue = v);

        _byteValue = CreateField<byte>(
            nameof(TestEditorStruct.ByteValue), o => o.ByteValue, (o, v, ctx) => o.ByteValue = v);

        _floatValue = CreateField<float>(
            nameof(TestEditorStruct.FloatValue), o => o.FloatValue, (o, v, ctx) => o.FloatValue = v);

        _stringValue = CreateField<string>(
            nameof(TestEditorStruct.StringValue), o => o.StringValue, (o, v, ctx) => o.StringValue = v);

        _objectValue = CreateField<object>(
            nameof(TestEditorStruct.ObjectValue), o => o.ObjectValue, (o, v, ctx) => o.ObjectValue = v);

        _viewObjValue = CreateField<TestViewObject>(
            nameof(TestEditorStruct.TestSyncObject), o => o.TestSyncObject, (o, v, ctx) => o.TestSyncObject = v);

        _enumValue = CreateField<TestEnum>(
            nameof(TestEditorStruct.EnumValue), o => o.EnumValue, (o, v, ctx) => o.EnumValue = v);

        _minMax = CreateField<MinMaxValue>(
            nameof(TestEditorStruct.MinMaxStruct), o => o.MinMaxStruct, (o, v, ctx) => o.MinMaxStruct = v);

        _listValue = CreateField<List<int>>(
            nameof(TestEditorStruct.ListValue), o => o.ListValue);

        _viewList = CreateField<TestViewList<int>>(
            nameof(TestEditorStruct.ViewList), o => o.ViewList);

        _sobj = CreateField<SObject>(
            nameof(TestEditorStruct.SObj), o => o.SObj, (o, v, ctx) => o.SObj = v);
    }

    /// <inheritdoc/>
    protected override void RowFunctionInner(ImGui gui, PropertyTarget target, PropertyRowAction rowAction)
    {
        _boolValue.OnGui(gui, target);
        _byteValue.OnGui(gui, target);
        _floatValue.OnGui(gui, target);
        _stringValue.OnGui(gui, target);
        _objectValue.OnGui(gui, target);
        _viewObjValue.OnGui(gui, target);
        _enumValue.OnGui(gui, target);
        _minMax.OnGui(gui, target);
        _listValue.OnGui(gui, target);
        _viewList.OnGui(gui, target);
        _sobj.OnGui(gui, target);
    }
}

/// <summary>
/// Test view object implementing property synchronization and view setup.
/// </summary>
public class TestViewObject : IViewObject
{
    /// <summary>
    /// A boolean test value.
    /// </summary>
    public bool BoolValue;
    /// <summary>
    /// A byte test value.
    /// </summary>
    public byte ByteValue;
    /// <summary>
    /// A float test value.
    /// </summary>
    public float FloatValue;
    /// <summary>
    /// A string test value.
    /// </summary>
    public string StringValue;
    /// <summary>
    /// An enum test value.
    /// </summary>
    public TestEnum EnumValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestViewObject"/> class.
    /// </summary>
    public TestViewObject()
    {
    }

    /// <inheritdoc/>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        BoolValue = sync.Sync(nameof(BoolValue), BoolValue);
        ByteValue = sync.Sync(nameof(ByteValue), ByteValue);
        FloatValue = sync.Sync(nameof(FloatValue), FloatValue);
        StringValue = sync.Sync(nameof(StringValue), StringValue);
        EnumValue = sync.Sync(nameof(EnumValue), EnumValue);
    }

    /// <inheritdoc/>
    public void SetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(BoolValue, new ViewProperty(nameof(BoolValue)));
        setup.InspectorField(ByteValue, new ViewProperty(nameof(ByteValue)));
        setup.InspectorField(FloatValue, new ViewProperty(nameof(FloatValue)));
        setup.InspectorField(StringValue, new ViewProperty(nameof(StringValue)));
        setup.InspectorField(EnumValue, new ViewProperty(nameof(EnumValue)) { Status = TextStatus.Reference });
    }
}

/// <summary>
/// Generic test view list implementing index synchronization.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public class TestViewList<T> : IViewList
{
    private readonly List<T> _list = [];

    /// <inheritdoc/>
    public int ListViewId => ViewIds.Inspector;

    /// <inheritdoc/>
    public int Count => _list.Count;

    /// <inheritdoc/>
    public bool DropInCheck(object value)
    {
        return value is T;
    }

    /// <inheritdoc/>
    public object DropInConvert(object value)
    {
        if (value is T t)
        {
            return t;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public void Sync(IIndexSync sync, ISyncContext context)
    {
        sync.SyncGenericIList(
            _list,
            createNew: () => Activator.CreateInstance(typeof(T))
        );
    }
}

/// <summary>
/// A struct representing a minimum and maximum integer range.
/// </summary>
public struct MinMaxValue
{
    /// <summary>
    /// The minimum value of the range.
    /// </summary>
    public int Min;
    /// <summary>
    /// The maximum value of the range.
    /// </summary>
    public int Max;
}

/// <summary>
/// Custom property editor for <see cref="MinMaxValue"/> that displays Min and Max side by side.
/// </summary>
public class MinMaxValueEditor : ImGuiPropertyEditor<MinMaxValue>
{
    /// <inheritdoc/>
    public override ImGuiNode RowFunction(ImGui gui, PropertyTarget target, PropertyRowAction rowAction)
    {
        if (!typeof(MinMaxValue).IsAssignableFrom(target.EditedType))
        {
            return base.RowFunction(gui, target, rowAction);
        }

        //var node = gui.PropertyGroup(target).OnPropertyGroupExpand(() =>
        //{
        //    gui.PropertyField(minValue);
        //    gui.PropertyField(maxValue);
        //});

        var node = gui.PropertyRow(target, (n, inner, column, pipeline) =>
        {
            if (pipeline.HasFlag(GuiPipeline.Main))
            {
                switch (column)
                {
                    case PropertyGridColumn.Prefix:
                        break;

                    case PropertyGridColumn.Name:
                        break;

                    case PropertyGridColumn.Main:
                        {
                            var minValue = inner.GetOrCreateStructField<MinMaxValue, int>(
                                nameof(MinMaxValue.Min), o => o.Min, (o, v, ctx) => { o.Min = v; return o; });

                            var maxValue = inner.GetOrCreateStructField<MinMaxValue, int>(
                                nameof(MinMaxValue.Max), o => o.Max, (o, v, ctx) => { o.Max = v; return o; });

                            gui.NumericEditor<int>(minValue, act => n.DoValueAction(act))
                                .SetWidthPercentage(50);

                            gui.NumericEditor<int>(maxValue, act => n.DoValueAction(act))
                                .SetWidthPercentage(50);
                        }
                        break;

                    case PropertyGridColumn.Option:
                        break;

                    default:
                        break;
                }
            }
        });

        return node;
    }
}