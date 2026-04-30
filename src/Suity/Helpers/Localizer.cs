using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Suity.Helpers;

/// <summary>
/// Represents just a string. This class is used to allow interpolated strings to preferably be passed as FormattableString 
/// instead of string to methods that overload both types.
/// </summary>
public class PlainString
{
    /// <value>
    /// Value of the string.
    /// </value>
    public string Value { get; }

    /// <summary>
    /// Default constructor.
    /// </summary>
    public PlainString(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Converts a string value to a PlainString.
    /// </summary>
    /// <param name="value">Value.</param>
    public static implicit operator PlainString(string value)
    {
        return new PlainString(value);
    }

    /// <summary>
    /// Converts a FormattableString value to a PlainString.
    /// </summary>
    /// <remarks>
    /// This implicit operator is needed to avoid FormattableString values to be automatically
    /// converted to string and then to PlainString when resolving parameter overloads.
    /// </remarks>
    /// <param name="value">Value.</param>
    /// <exception cref="InvalidOperationException">Always thrown.</exception>
    [ExcludeFromCodeCoverage]
    public static implicit operator PlainString(FormattableString value)
    {
        throw new InvalidOperationException();
    }

    public override string ToString() => Value;
}

/// <summary>
/// Converter of strings from a base-language value to its corresponding language-specific localization.
/// </summary>
public interface ILocalizer
{
    //===========================================================================
    //                                PROPERTIES
    //===========================================================================

    /// <summary>
    /// Target language of the localizer.
    /// </summary>
    public string TargetLanguage { get; }

    /// <summary>
    /// Target culture of the localizer.
    /// </summary>
    public CultureInfo TargetCulture { get; }

    //===========================================================================
    //                                  METHODS
    //===========================================================================

    /// <summary>
    /// Localizes a string.
    /// </summary>
    /// <remarks>
    /// Converts the base-language string <paramref name="text"/> to its corresponding language-specific localized value.
    /// </remarks>
    /// <param name="text">Base-language string.</param>
    /// <returns>Language-specific localized string if found, or <paramref name="text"/> otherwise.</returns>
    string Localize(PlainString text);

    /// <summary>
    /// Localizes an interpolated string.
    /// </summary>
    /// <remarks>
    /// Converts the composite format string of the base-language formattable string <paramref name="frmtText"/> (e.g. an interpolated string) 
    /// to its corresponding language-specific localized composite format value, and then generates the result by formatting the 
    /// localized composite format value along with the <paramref name="frmtText"/> arguments by using the formatting conventions of the localizer culture.
    /// </remarks>
    /// <param name="frmtText">Base-language formattable string.</param>
    /// <returns>Formatted string generated from the language-specific localized format string if found,
    ///          or generated from <paramref name="frmtText"/> otherwise.</returns>
    /// <exception cref="FormatException">Thrown when the localized format value of <paramref name="frmtText"/> is invalid.</exception>
    string Localize(FormattableString frmtText);

    /// <summary>
    /// Localizes and then formats a string.
    /// </summary>
    /// <remarks>
    /// Converts the base-language format string <paramref name="format"/> to its corresponding language-specific localized format value,
    /// and then generates the result by formatting the localized format value along with the <paramref name= "args" /> arguments by using the formatting
    /// conventions of the localizer culture.
    /// </remarks>
    /// <param name="format">Base-language format string.</param>
    /// <param name="args">Arguments for the format string.</param>
    /// <returns>Formatted string generated from the language-specific localized format string if found,
    ///          or generated from <paramref name="format"/> otherwise.</returns>
    /// <exception cref="FormatException">Thrown when <paramref name="format"/> or its localized format value is invalid.</exception>
    string LocalizeFormat(string format, params object[] args);

    /// <summary>
    /// Localizes multiple strings.
    /// </summary>
    /// <remarks>
    /// Converts the base-language strings in <paramref name="texts"/> to their corresponding language-specific localized values.
    /// </remarks>
    /// <param name="texts">Base-language strings.</param>
    /// <returns>Language-specific localized strings if found, or the base-language string otherwise.</returns>
    IEnumerable<string> Localize(IEnumerable<string> texts);

    /// <summary>
    /// Gets the localizer for a context in the current localizer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Contexts are used to disambiguate the conversion of the same base-language string to different
    /// language-specific strings depending on the context where the conversion is performed.
    /// </para>
    /// <para>
    /// Contexts can be nested. The context identifier can identify a chain of nested contexts by separating
    /// their identifiers with the '.' character (left = outermost / right = innermost).
    /// </para>
    /// </remarks>
    /// <param name="contextId">Identifier of the context.</param>
    /// <returns>Localizer for the given context.</returns>
    ILocalizer Context(string contextId);

    /// <summary>
    /// Gets the localizer for a context in the current localizer.
    /// </summary>
    /// <remarks>
    /// Contexts are used to disambiguate the conversion of the same base-language string to different
    /// language-specific strings depending on the context where the conversion is performed.
    /// </remarks>
    /// <param name="splitContextIds">Chain of context identifiers in split form.</param>
    /// <returns>Localizer for the given context.</returns>
    ILocalizer Context(IEnumerable<string> splitContextIds);
}

public class EmptyLocalizer : ILocalizer
{
    public static EmptyLocalizer Instance { get; } = new();

    public string TargetLanguage => null;

    public CultureInfo TargetCulture => null;

    private EmptyLocalizer() { }

    public string Localize(PlainString text) => text?.ToString() ?? string.Empty;

    public string Localize(FormattableString frmtText) => frmtText?.ToString() ?? string.Empty;

    public string LocalizeFormat(string format, params object[] args) => string.Format(format, args);

    public IEnumerable<string> Localize(IEnumerable<string> texts) => texts;

    public ILocalizer Context(string contextId) => this;

    public ILocalizer Context(IEnumerable<string> splitContextIds) => this;
}

public static class GlobalLocalizer
{
    //===========================================================================
    //                           PUBLIC PROPERTIES
    //===========================================================================

    /// <summary>
    /// Global localizer.
    /// </summary>
    public static ILocalizer Localizer { get; set; } = EmptyLocalizer.Instance;

    //===========================================================================
    //                            PUBLIC METHODS
    //===========================================================================

    /// <summary>
    /// Localizes a string using the global localizer.
    /// </summary>
    /// <seealso cref="ILocalizer.Localize(PlainString)"/>
    /// <param name="text">Base-language string.</param>
    /// <returns>Language-specific localized string if found, or <paramref name="text"/> otherwise.</returns>
    public static string Localize(PlainString text)
    {
        if (text?.Value is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(text.Value))
        {
            return text.Value;
        }

        return Localizer.Localize(text);
    }

    public static string L(PlainString text)
    {
        if (text?.Value is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(text.Value))
        {
            return text.Value;
        }

        return Localizer.Localize(text);
    }

    /// <summary>
    /// Localizes an interpolated string using the global localizer.
    /// </summary>
    /// <seealso cref="ILocalizer.Localize(FormattableString)"/>
    /// <param name="frmtText">Base-language formattable string.</param>
    /// <returns>Formatted string generated from the language-specific localized format string if found, 
    ///          or generated from <paramref name="frmtText"/> otherwise.</returns>
    public static string Localize(FormattableString frmtText)
    {
        return Localizer.Localize(frmtText);
    }

    public static string L(FormattableString text)
    {
        return Localizer.Localize(text);
    }

    /// <summary>
    /// Localizes multiple strings using the global localizer.
    /// </summary>
    /// <seealso cref="ILocalizer.Localize(IEnumerable{string})"/>
    /// <param name="texts">Base-language strings.</param>
    /// <returns>Language-specific localized strings if found, or the base-language string otherwise.</returns>
    public static IEnumerable<string> Localize(IEnumerable<string> texts)
    {
        return Localizer.Localize(texts);
    }

    /// <summary>
    /// Localizes and then formats a string using the global localizer.
    /// </summary>
    /// <seealso cref="ILocalizer.LocalizeFormat(string, object[])"/>
    /// <param name="format">Base-language format string.</param>
    /// <param name="args">Arguments for the format string.</param>
    /// <returns>Formatted string generated from the language-specific localized format string if found,
    ///          or generated from <paramref name="format"/> otherwise.</returns>
    public static string LocalizeFormat(string format, params object[] args)
    {
        return Localizer.LocalizeFormat(format, args);
    }

    /// <summary>
    /// Gets a context in the global localizer.
    /// </summary>
    /// <seealso cref="ILocalizer.Context(string)"/>
    /// <param name="contextId">Identifier of the context.</param>
    /// <returns>Localizer for the given context.</returns>
    public static ILocalizer Context(string contextId)
    {
        return Localizer.Context(contextId);
    }

    /// <summary>
    /// Gets a context in the global localizer.
    /// </summary>
    /// <seealso cref="ILocalizer.Context(IEnumerable{string})"/>
    /// <param name="splitContextIds">Chain of context identifiers in split form.</param>
    /// <returns>Localizer for the given context.</returns>
    public static ILocalizer Context(IEnumerable<string> splitContextIds)
    {
        return Localizer.Context(splitContextIds);
    }
}