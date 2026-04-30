using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Collections.Generic;

namespace Suity.Editor.AIGC.Tools;

/// <summary>
/// Provides extension methods for validating, fixing, and converting data model types.
/// </summary>
public static class DataModelExtensions
{
    #region Varify

    /// <summary>
    /// Validates all structures within a <see cref="DataModelSegmentation"/> and collects any problems.
    /// </summary>
    /// <param name="segs">The data model segmentation to validate.</param>
    /// <param name="problems">The list to append validation problems to.</param>
    public static void Varify(this DataModelSegmentation segs, ref List<string> problems)
    {
        segs.Structures ??= [];

        foreach (var dataStructure in segs.Structures)
        {
            dataStructure.Varify(ref problems);
        }
    }

    /// <summary>
    /// Validates a <see cref="StructureSegment"/> and collects any problems.
    /// </summary>
    /// <param name="seg">The structure segment to validate.</param>
    /// <param name="problems">The list to append validation problems to.</param>
    public static void Varify(this StructureSegment seg, ref List<string> problems)
    {
        if (seg is null)
        {
            (problems ??= []).Add($"Structure is empty");
            return;
        }

        if (string.IsNullOrWhiteSpace(seg.Name))
        {
            (problems ??= []).Add($"Structure name is missing");
        }
        else if (!NamingVerifier.VerifyIdentifier(seg.Name))
        {
            (problems ??= []).Add($"Structure name is invalid: {seg.Name}, it should be a valid English identifier in PascalCase");
        }
    }

    /// <summary>
    /// Validates all structures within a <see cref="DataModelSpecification"/> and collects any problems.
    /// </summary>
    /// <param name="specs">The data model specification to validate.</param>
    /// <param name="problems">The list to append validation problems to.</param>
    public static void Varify(this DataModelSpecification specs, ref List<string> problems)
    {
        if (specs is null)
        {
            (problems ??= []).Add($"Data model is empty");
            return;
        }

        specs.Structures ??= [];

        foreach (var structure in specs.Structures)
        {
            structure.Varify(ref problems);
        }
    }

    /// <summary>
    /// Validates a <see cref="StructureSpecification"/> and collects any problems.
    /// </summary>
    /// <param name="spec">The structure specification to validate.</param>
    /// <param name="problems">The list to append validation problems to.</param>
    public static void Varify(this StructureSpecification spec, ref List<string> problems)
    {
        if (spec is null)
        {
            (problems ??= []).Add($"Structure is empty");
            return;
        }

        if (string.IsNullOrWhiteSpace(spec.Name))
        {
            (problems ??= []).Add($"Structure name is missing");
        }
        else if (!NamingVerifier.VerifyIdentifier(spec.Name))
        {
            (problems ??= []).Add($"Structure name is invalid: {spec.Name}, it should be a valid English identifier in PascalCase");
        }

        spec.Items ??= [];

        foreach (var field in spec.Items)
        {
            field.Varify(spec, ref problems);
        }
    }

    /// <summary>
    /// Validates a <see cref="FieldSpecification"/> within its parent structure and collects any problems.
    /// </summary>
    /// <param name="field">The field specification to validate.</param>
    /// <param name="spec">The parent structure specification.</param>
    /// <param name="problems">The list to append validation problems to.</param>
    public static void Varify(this FieldSpecification field, StructureSpecification spec, ref List<string> problems)
    {
        if (spec is null)
        {
            (problems ??= []).Add($"Field parent is empty");
            return;
        }

        if (field is null)
        {
            (problems ??= []).Add($"Structure contains empty field: '{spec.Name}'");
            return;
        }

        if (string.IsNullOrWhiteSpace(spec.Name))
        {
            (problems ??= []).Add($"Field name is missing in structure: '{spec.Name}'");
        }
        else if (!NamingVerifier.VerifyIdentifier(field.Name))
        {
            (problems ??= []).Add($"Field name is invalid: '{spec.Name}.{field.Name}', it should be a valid English identifier in PascalCase");
        }

        field.FieldType = field.FieldType?.Trim() ?? string.Empty;

        if (spec.Type.IsStructOrAbstract())
        {
            if (string.IsNullOrWhiteSpace(field.FieldType))
            {
                (problems ??= []).Add($"Field type is missing: '{spec.Name}.{field.Name}'");
            }
            else if (field.FieldType.Contains("List<") || field.FieldType.Contains("Dictionary<"))
            {
                (problems ??= []).Add($"The type of the field is invalid: '{field.FieldType}', it should not be List or Dictionary");
            }
        }
        else
        {
            field.FieldType = string.Empty;
        }
    }

    #endregion

    #region Fix

    /// <summary>
    /// Applies default fixes to a <see cref="DataModelSegmentation"/>.
    /// </summary>
    /// <param name="segs">The data model segmentation to fix.</param>
    public static void Fix(this DataModelSegmentation segs)
    {
    }

    /// <summary>
    /// Applies default fixes to a <see cref="StructureSegment"/>, trimming and setting fallback values.
    /// </summary>
    /// <param name="seg">The structure segment to fix.</param>
    public static void Fix(this StructureSegment seg)
    {
        seg.Name = seg.Name?.Trim() ?? "???";
        seg.Brief = seg.Brief?.Trim() ?? string.Empty;
        seg.DerivedFrom = seg.DerivedFrom?.Trim() ?? string.Empty;
        seg.ItemDefinition ??= string.Empty;
        seg.Attributes ??= [];
    }

    /// <summary>
    /// Fixes a <see cref="DataModelSpecification"/> by comparing it against an original specification and applying corrections.
    /// </summary>
    /// <param name="specs">The data model specification to fix.</param>
    /// <param name="originSpecs">The original specification to compare against.</param>
    /// <param name="fixture">The list to record all applied fixes.</param>
    /// <param name="conversation">Optional conversation handler for reporting warnings.</param>
    public static void Fix(this DataModelSpecification specs, DataModelSpecification originSpecs, List<string> fixture, IConversationHandler conversation)
    {
        if (specs is null)
        {
            throw new ArgumentNullException(nameof(specs));
        }

        if (originSpecs is null)
        {
            throw new ArgumentNullException(nameof(originSpecs));
        }

        if (fixture is null)
        {
            throw new ArgumentNullException(nameof(fixture));
        }

        Dictionary<string, StructureSpecification> specDic = [];
        foreach (var seg in originSpecs.Structures)
        {
            if (!specDic.ContainsKey(seg.Name))
            {
                specDic.Add(seg.Name, seg);
            }
            else
            {
                conversation?.AddWarningMessage("The data structure name is duplicated : " + seg.Name);
            }
        }

        specs.Fix(specDic, fixture);
    }

    /// <summary>
    /// Fixes a <see cref="DataModelSpecification"/> by aligning it with a <see cref="DataModelSegmentation"/> and applying corrections.
    /// </summary>
    /// <param name="specs">The data model specification to fix.</param>
    /// <param name="segs">The data model segmentation to align with.</param>
    /// <param name="fixture">The list to record all applied fixes.</param>
    /// <param name="conversation">Optional conversation handler for reporting warnings.</param>
    public static void Fix(this DataModelSpecification specs, DataModelSegmentation segs, List<string> fixture, IConversationHandler conversation)
    {
        if (specs is null)
        {
            throw new ArgumentNullException(nameof(specs));
        }

        if (segs is null)
        {
            throw new ArgumentNullException(nameof(segs));
        }

        if (fixture is null)
        {
            throw new ArgumentNullException(nameof(fixture));
        }

        segs.Fix();

        Dictionary<string, StructureSegment> segDic = [];
        foreach (var seg in segs.Structures)
        {
            if (!segDic.ContainsKey(seg.Name))
            {
                segDic.Add(seg.Name, seg);
            }
            else
            {
                conversation?.AddWarningMessage("The data structure name is duplicated : " + seg.Name);
            }
        }

        specs.Structures ??= [];
        specs.Fix(segDic, fixture);

        specs.Fix(specs, fixture, conversation);
    }

    /// <summary>
    /// Fixes all fields in a <see cref="DataModelSpecification"/> using a dictionary of existing specifications.
    /// </summary>
    /// <param name="specs">The data model specification to fix.</param>
    /// <param name="specDic">Dictionary of structure specifications for reference.</param>
    /// <param name="fixture">The list to record all applied fixes.</param>
    public static void Fix(this DataModelSpecification specs, Dictionary<string, StructureSpecification> specDic, List<string> fixture)
    {
        specs.Structures ??= [];
        foreach (var spec in specs.Structures)
        {
            foreach (var field in spec.Items)
            {
                field.Fix(spec, specDic, fixture);
            }
        }
    }

    /// <summary>
    /// Fixes all fields in a <see cref="DataModelSpecification"/> using a dictionary of structure segments.
    /// </summary>
    /// <param name="specs">The data model specification to fix.</param>
    /// <param name="segDic">Dictionary of structure segments for reference.</param>
    /// <param name="fixture">The list to record all applied fixes.</param>
    public static void Fix(this DataModelSpecification specs, Dictionary<string, StructureSegment> segDic, List<string> fixture)
    {
        specs.Structures ??= [];
        foreach (var spec in specs.Structures)
        {
            foreach (var field in spec.Items)
            {
                field.Fix(spec, segDic, fixture);
            }
        }
    }

    /// <summary>
    /// Fixes a <see cref="StructureSpecification"/> by comparing it with another specification and recording differences.
    /// </summary>
    /// <param name="spec">The structure specification to fix.</param>
    /// <param name="specDic">Dictionary of structure specifications for reference.</param>
    /// <param name="fixture">The list to record all applied fixes.</param>
    public static void Fix(this StructureSpecification spec, Dictionary<string, StructureSpecification> specDic, List<string> fixture)
    {
        spec.Fix();

        if (specDic.TryGetValue(spec.Name, out var specOther))
        {
            specOther.Fix();

            if (specOther.Type != spec.Type)
            {
                fixture.Add($"{spec.Name} Type : {spec.Type} -> {specOther.Type}");
                spec.Type = specOther.Type;
            }

            if (specOther.Usage != spec.Usage)
            {
                fixture.Add($"{spec.Name} Usage : {spec.Usage} -> {specOther.Usage}");
                spec.Usage = specOther.Usage;
            }

            if (specOther.DrivenMode != spec.DrivenMode)
            {
                fixture.Add($"{spec.Name} DrivenMode : {spec.DrivenMode} -> {specOther.DrivenMode}");
                spec.DrivenMode = specOther.DrivenMode;
            }

            if (specOther.DerivedFrom != spec.DerivedFrom)
            {
                fixture.Add($"{spec.Name} DerivedFrom : {spec.DerivedFrom} -> {specOther.DerivedFrom}");
                spec.DerivedFrom = specOther.DerivedFrom;
            }

            if (specOther.Brief != spec.Brief && !string.IsNullOrWhiteSpace(specOther.Brief))
            {
                fixture.Add($"{spec.Name} -> Description : {spec.Brief} -> {specOther.Brief}");
                spec.Brief = specOther.Brief;
            }
        }

        foreach (var field in spec.Items)
        {
            field.Fix(spec, specDic, fixture);
        }
    }

    /// <summary>
    /// Fixes a <see cref="StructureSpecification"/> by comparing it with a structure segment and recording differences.
    /// </summary>
    /// <param name="spec">The structure specification to fix.</param>
    /// <param name="segDic">Dictionary of structure segments for reference.</param>
    /// <param name="fixture">The list to record all applied fixes.</param>
    public static void Fix(this StructureSpecification spec, Dictionary<string, StructureSegment> segDic, List<string> fixture)
    {
        spec.Fix();

        if (segDic.TryGetValue(spec.Name, out var seg))
        {
            seg.Fix();

            if (seg.Type != spec.Type)
            {
                fixture.Add($"{spec.Name} Type : {spec.Type} -> {seg.Type}");
                spec.Type = seg.Type;
            }

            if (seg.Usage != spec.Usage)
            {
                fixture.Add($"{spec.Name} Usage :  {spec.Usage} -> {seg.Usage}");
                spec.Usage = seg.Usage;
            }

            if (seg.DrivenMode != spec.DrivenMode)
            {
                fixture.Add($"{spec.Name} DrivenMode : {spec.DrivenMode} -> {seg.DrivenMode}");
                spec.DrivenMode = seg.DrivenMode;
            }

            if (seg.DerivedFrom != spec.DerivedFrom)
            {
                fixture.Add($"{spec.Name} DerivedFrom : {spec.DerivedFrom} -> {seg.DerivedFrom}");
                spec.DerivedFrom = seg.DerivedFrom;
            }

            if (seg.Brief != spec.Brief && !string.IsNullOrWhiteSpace(seg.Brief))
            {
                fixture.Add($"{spec.Name} -> Description : {spec.Brief} -> {seg.Brief}");
                spec.Brief = seg.Brief;
            }

            spec.Attributes.Clear();
            spec.Attributes.AddRange(seg.Attributes);
        }

        foreach (var field in spec.Items)
        {
            field.Fix(spec, segDic, fixture);
        }
    }

    /// <summary>
    /// Applies default fixes to a <see cref="StructureSpecification"/>, trimming and setting fallback values.
    /// </summary>
    /// <param name="spec">The structure specification to fix.</param>
    public static void Fix(this StructureSpecification spec)
    {
        spec.Name = spec.Name?.Trim() ?? "???";
        spec.Brief = spec.Brief?.Trim() ?? string.Empty;
        spec.DerivedFrom = spec.DerivedFrom?.Trim() ?? string.Empty;
        spec.Items ??= [];
        spec.Attributes ??= [];
    }


    /// <summary>
    /// Fixes a <see cref="FieldSpecification"/> by comparing it with a structure specification.
    /// </summary>
    /// <param name="field">The field specification to fix.</param>
    /// <param name="parent">The parent structure specification.</param>
    /// <param name="specDic">Dictionary of structure specifications for reference.</param>
    /// <param name="fixture">The list to record all applied fixes.</param>
    public static void Fix(this FieldSpecification field, StructureSpecification parent, Dictionary<string, StructureSpecification> specDic, List<string> fixture)
    {
        field.Fix(parent, fixture);

        if (specDic.TryGetValue(field.FieldType, out var spec))
        {
            field.Fix(parent, spec.Type, spec.Type.GetIsLinked(spec.Usage), fixture);
        }
    }

    /// <summary>
    /// Fixes a <see cref="FieldSpecification"/> by comparing it with a structure segment.
    /// </summary>
    /// <param name="field">The field specification to fix.</param>
    /// <param name="parent">The parent structure specification.</param>
    /// <param name="secDic">Dictionary of structure segments for reference.</param>
    /// <param name="fixture">The list to record all applied fixes.</param>
    public static void Fix(this FieldSpecification field, StructureSpecification parent, Dictionary<string, StructureSegment> secDic, List<string> fixture)
    {
        field.Fix(parent, fixture);

        if (secDic.TryGetValue(field.FieldType, out var seg))
        {
            field.Fix(parent, seg.Type, seg.Type.GetIsLinked(seg.Usage), fixture);
        }
    }

    private static void Fix(this FieldSpecification field, StructureSpecification parent, List<string> fixture)
    {
        field.Name = field.Name?.Trim() ?? "???";
        field.Description = field.Description?.Trim() ?? string.Empty;

        string fieldType = field.FieldType.Trim('<', '>', '(', ')', '[', ']', '@', ' ');
        if (fieldType != field.FieldType)
        {
            fixture.Add($"{parent.Name}.{field.Name} FieldType : {field.FieldType} -> {fieldType}");
            field.FieldType = fieldType;
        }
    }

    private static void Fix(this FieldSpecification field, StructureSpecification parent, DataStructureType type, bool isLinked, List<string> fixture)
    {
    }

    #endregion

    /// <summary>
    /// Converts a <see cref="DCompond"/> to a <see cref="StructureSpecification"/>.
    /// </summary>
    /// <param name="dCompond">The compound type to convert.</param>
    /// <returns>A new <see cref="StructureSpecification"/> instance.</returns>
    public static StructureSpecification ToSpecification(this DCompond dCompond) 
        => StructureSpecification.FromDCompond(dCompond);

    /// <summary>
    /// Converts a <see cref="DEnum"/> to a <see cref="StructureSpecification"/>.
    /// </summary>
    /// <param name="dEnum">The enum type to convert.</param>
    /// <returns>A new <see cref="StructureSpecification"/> instance.</returns>
    public static StructureSpecification ToSpecification(this DEnum dEnum) 
        => StructureSpecification.FromDEnum(dEnum);

    /// <summary>
    /// Converts a <see cref="DCompond"/> to a tag string representation.
    /// </summary>
    /// <param name="dCompond">The compound type to convert.</param>
    /// <param name="fullName">Whether to include the full namespace in the tag.</param>
    /// <returns>A tag string representing the compound type.</returns>
    public static string ToTag(this DCompond dCompond, bool fullName = false)
        => StructureSpecification.FromDCompond(dCompond)?.ToTag(fullName ? dCompond.NameSpace : null) ?? string.Empty;

    /// <summary>
    /// Converts a <see cref="DEnum"/> to a tag string representation.
    /// </summary>
    /// <param name="dEnum">The enum type to convert.</param>
    /// <param name="fullName">Whether to include the full namespace in the tag.</param>
    /// <returns>A tag string representing the enum type.</returns>
    public static string ToTag(this DEnum dEnum, bool fullName = false)
        => StructureSpecification.FromDEnum(dEnum)?.ToTag(fullName ? dEnum.NameSpace : null) ?? string.Empty;

    /// <summary>
    /// Determines whether the specified <see cref="DataStructureType"/> is a struct or abstract type.
    /// </summary>
    /// <param name="type">The data structure type to check.</param>
    /// <returns><c>true</c> if the type is <see cref="DataStructureType.Struct"/> or <see cref="DataStructureType.Abstract"/>; otherwise, <c>false</c>.</returns>
    public static bool IsStructOrAbstract(this DataStructureType type)
    {
        return type == DataStructureType.Struct || type == DataStructureType.Abstract;
    }
}
