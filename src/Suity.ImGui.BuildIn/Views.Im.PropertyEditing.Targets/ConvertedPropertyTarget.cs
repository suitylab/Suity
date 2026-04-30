using System;
using System.Collections.Generic;

namespace Suity.Views.Im.PropertyEditing.Targets;

/// <summary>
/// A property target that applies conversion functions to get and set values, transforming the underlying data representation.
/// </summary>
internal class ConvertedPropertyTarget : PropertyTargetBK
{
    private readonly PropertyTarget _origin;
    private readonly TargetConversion _convert;
    private readonly TargetRevertConversion _convertRevert;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConvertedPropertyTarget"/> class.
    /// </summary>
    /// <param name="origin">The original property target to wrap.</param>
    /// <param name="convert">The conversion function applied when getting values.</param>
    /// <param name="convertRevert">The revert conversion function applied when setting values.</param>
    public ConvertedPropertyTarget(PropertyTarget origin, TargetConversion convert, TargetRevertConversion convertRevert)
        : base(origin.PropertyName, origin.PresetType)
    {
        _origin = origin ?? throw new ArgumentNullException(nameof(origin));
        _convert = convert ?? throw new ArgumentNullException(nameof(convert));
        _convertRevert = convertRevert ?? throw new ArgumentNullException(nameof(convertRevert));

        Parent = origin.Parent;
        Index = origin.Index;
        Description = origin.Description;
        Status = origin.Status;
        Disabled = origin.Disabled;
        ReadOnly = origin.ReadOnly;
        Optional = origin.Optional;
        Styles = origin.Styles;
        WriteBack = origin.WriteBack;
        CacheValues = origin.CacheValues;
        InitExpanded = origin.InitExpanded;
        Color = origin.Color;
        SupportMultipleColumn = false;
        Attributes = origin.Attributes;
        ServiceProvider = origin.ServiceProvider;
        CachedTheme = origin.CachedTheme;

        if (origin.ArrayTarget is { })
        {
            SetupArray(origin.ArrayTarget.Handler);
        }
    }

    /// <inheritdoc/>
    public override bool IsRoot => _origin.IsRoot;

    /// <inheritdoc/>
    public override bool ValueMultiple => _origin.ValueMultiple;

    /// <inheritdoc/>
    public override IEnumerable<object?> GetParentObjects() => _origin.GetParentObjects();

    /// <inheritdoc/>
    protected override IEnumerable<object?> OnGetValues() => _convert(_origin);

    /// <inheritdoc/>
    protected override void OnSetValues(IEnumerable<object?> objects, ISetterContext? context) => _origin.SetValues(_convertRevert(_origin, objects), context);
}