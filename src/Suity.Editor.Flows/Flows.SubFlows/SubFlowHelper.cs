using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Views;
using System;
using System.Drawing;

namespace Suity.Editor.Flows.SubFlows;

/// <summary>
/// Provides helper methods for evaluating page-related values and types.
/// </summary>
public static class SubFlowHelper
{
    /// <summary>
    /// Determines whether the specified object represents an empty or null value.
    /// </summary>
    /// <param name="obj">The object to evaluate.</param>
    /// <returns><c>true</c> if the object is considered empty; otherwise, <c>false</c>.</returns>
    public static bool GetIsValueEmpty(object obj)
    {
        return obj switch
        {
            null => true,
            SKey skey => skey.TargetAsset is null,
            SAssetKey sAssetKey => sAssetKey.TargetAsset is null,
            SObject sobj => SObject.IsNullOrEmpty(sobj),
            SArray sary => sary.Count == 0,
            Array ary => ary.Length == 0,
            string str => string.IsNullOrWhiteSpace(str),
            bool b => !b,
            TextBlock textBlock => string.IsNullOrWhiteSpace(textBlock.Text),
            STextBlock sTextBlock => string.IsNullOrWhiteSpace(sTextBlock.TextValue),
            _ when IsNumericType(obj) => IsZero(obj),
            _ => false
        };
    }

    /// <summary>
    /// Determines whether the specified object is a numeric type.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns><c>true</c> if the object is a numeric type; otherwise, <c>false</c>.</returns>
    public static bool IsNumericType(object obj)
    {
        return obj switch
        {
            byte or sbyte or short or ushort or
            int or uint or long or ulong or
            float or double or decimal => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines whether the specified numeric object has a value of zero.
    /// </summary>
    /// <param name="obj">The numeric object to check.</param>
    /// <returns><c>true</c> if the value is zero; otherwise, <c>false</c>.</returns>
    public static bool IsZero(object obj)
    {
        return obj switch
        {
            null => false,
            byte b => b == 0,
            sbyte sb => sb == 0,
            short s => s == 0,
            ushort us => us == 0,
            int i => i == 0,
            uint ui => ui == 0,
            long l => l == 0,
            ulong ul => ul == 0,
            float f => f == 0f,
            double d => d == 0d,
            decimal m => m == 0m,
            _ => false
        };
    }


    /// <summary>
    /// Converts a page commit type to its corresponding display color.
    /// </summary>
    /// <param name="endType">The commit type.</param>
    /// <returns>The associated color, or null.</returns>
    public static Color? ToColor(this TaskCommitStatus endType)
    {
        switch (endType)
        {
            case TaskCommitStatus.None:
                return FlowColors.PageParameterColor;

            case TaskCommitStatus.TaskFinished:
                return FlowColors.WorkflowColor;

            case TaskCommitStatus.TaskFailed:
                return FlowColors.ErrorColor;

            default:
                return null;
        }
    }

    /// <summary>
    /// Converts an event type to its corresponding display color.
    /// </summary>
    /// <param name="eventType">The event type.</param>
    /// <returns>The associated color, or null.</returns>
    public static Color? ToColor(this TaskEventTypes eventType)
    {
        switch (eventType)
        {

            case TaskEventTypes.SubTaskFinished:
                return FlowColors.WorkflowColor;

            case TaskEventTypes.SubTaskFailed:
                return FlowColors.ErrorColor;

            case TaskEventTypes.TaskBegin:
            case TaskEventTypes.None:
            default:
                return FlowColors.PageParameterColor;
        }
    }
}
