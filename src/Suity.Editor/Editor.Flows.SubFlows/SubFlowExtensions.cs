using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;

namespace Suity.Editor.Flows.SubFlows;

/// <summary>
/// Provides extension methods for AIGC task pages and related types.
/// </summary>
public static class SubFlowExtensions
{
    /// <summary>
    /// Determines whether the specified <see cref="PageElementMode"/> represents a task or page mode.
    /// </summary>
    /// <param name="mode">The page element mode to check.</param>
    /// <returns><c>true</c> if the mode is <see cref="PageElementMode.Page"/> or <see cref="PageElementMode.Task"/>; otherwise, <c>false</c>.</returns>
    public static bool IsTaskOrPage(this PageElementMode mode)
    {
        switch (mode)
        {
            case PageElementMode.Page:
            case PageElementMode.Task:
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Converts a value to a chat history text representation using the type conversion service.
    /// </summary>
    /// <param name="type">The type definition of the value.</param>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted chat history text, or the original value's string representation.</returns>
    public static HistoryText ConvertChatHistoryText(this TypeDefinition type, object value, bool assetKeyMode)
    {
        if (assetKeyMode)
        {
            switch (value)
            {
                case Asset asset:
                    return asset.AssetKey;

                case SAssetKey assetKey:
                    return assetKey.SelectedKey;

                case SKey sKey:
                    return sKey.SelectedKey;

                default:
                    break;
            }
        }

        var historyText = TypeDefinition.FromNative<HistoryText>();
        var state = EditorServices.TypeConvertService.TryConvert(type, historyText, false, value, out var converted);
        var result = state == TypeConvertState.Unconvertible ? value : converted;

        return result?.ToString();
    }

    /// <summary>
    /// Returns true if the nullable boolean is null or has a value of true.
    /// Returns false only when the value is explicitly false.
    /// </summary>
    /// <param name="done">The nullable boolean value to evaluate.</param>
    /// <returns>True if the value is null or true; false otherwise.</returns>
    public static bool IsTrueOrEmpty(this bool? done)
    {
        if (done is { } v)
        {
            return v;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Returns true if the nullable boolean has a value of true; false otherwise.
    /// </summary>
    /// <param name="done">The nullable boolean value to evaluate.</param>
    /// <returns>True if the value is true; false if null or false.</returns>
    public static bool IsTrue(this bool? done) => done == true;


    /// <summary>
    /// Returns true if the nullable boolean has a value of false; false otherwise.
    /// </summary>
    /// <param name="done">The nullable boolean value to evaluate.</param>
    /// <returns>True if the value is false; false if null or true.</returns>
    public static bool IsFalse(this bool? done) => done == false;

    public static TextStatus ToCheckedStatus(this bool? done)
    {
        if (done is { } doneV)
        {
            return doneV ? TextStatus.Checked : TextStatus.Unchecked;
        }
        else
        {
            return TextStatus.Normal;
        }
    }
}
