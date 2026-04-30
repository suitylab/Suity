using System;
using System.Text.RegularExpressions;

namespace Suity.Helpers;

/// <summary>
/// Represents the result of string validation operations.
/// Each value corresponds to a specific validation failure or success condition.
/// </summary>
public enum StringValidateResult
{
    /// <summary>
    /// The string passed all validation checks.
    /// </summary>
    OK,

    /// <summary>
    /// The field is required but was empty or whitespace.
    /// </summary>
    [DisplayText("This field is required")]
    MustFill,

    /// <summary>
    /// The string length does not match the expected length.
    /// </summary>
    [DisplayText("Length does not match")]
    InvalidLength,

    /// <summary>
    /// The string is shorter than the minimum required length.
    /// </summary>
    [DisplayText("String too short")]
    TooShort,

    /// <summary>
    /// The string exceeds the maximum allowed length.
    /// </summary>
    [DisplayText("String too long")]
    TooLong,

    /// <summary>
    /// The string must be at least 8 characters long.
    /// </summary>
    [DisplayText("Minimum 8 characters required")]
    MinLength8,

    /// <summary>
    /// The string must be at least 6 characters long.
    /// </summary>
    [DisplayText("Minimum 6 characters required")]
    MinLength6,

    /// <summary>
    /// The string must be exactly 6 characters long.
    /// </summary>
    [DisplayText("6 characters required")]
    Length6,

    /// <summary>
    /// The string must contain only numeric characters.
    /// </summary>
    [DisplayText("Numbers only")]
    Numeric,

    /// <summary>
    /// The string format is invalid.
    /// </summary>
    [DisplayText("Invalid format")]
    InvalidFormat,

    /// <summary>
    /// The string must contain at least one lowercase letter.
    /// </summary>
    [DisplayText("At least 1 lowercase letter required")]
    AtLeastOneLowerChar,

    /// <summary>
    /// The string must contain at least one uppercase letter.
    /// </summary>
    [DisplayText("At least 1 uppercase letter required")]
    AtLeastOneUpperChar,

    /// <summary>
    /// The string must contain at least one special character.
    /// </summary>
    [DisplayText("At least 1 special character required")]
    AtLeastOneSpecialChar,

    /// <summary>
    /// The string must not contain spaces.
    /// </summary>
    [DisplayText("Spaces not allowed")]
    CannotContainsSpace,

    /// <summary>
    /// The password and its confirmation do not match.
    /// </summary>
    [DisplayText("Passwords do not match")]
    PasswordRepeatNotMatch,
}

/// <summary>
/// Provides static methods for validating strings against common patterns such as passwords, usernames, emails, and activation codes.
/// </summary>
public static class StringValidator
{
    /// <summary>
    /// Validates a string against password requirements.
    /// Passwords must be 8-50 characters, contain at least one lowercase letter, one uppercase letter, one special character, and no spaces.
    /// </summary>
    /// <param name="str">The string to validate as a password.</param>
    /// <returns>A <see cref="StringValidateResult"/> indicating the validation outcome.</returns>
    public static StringValidateResult MatchPassword(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return StringValidateResult.MustFill;
        }

        if (str.Length < 8)
        {
            return StringValidateResult.MinLength8;
        }

        if (str.Length > 50)
        {
            return StringValidateResult.TooLong;
        }

        if (!Regex.IsMatch(str, @"[a-z]+"))
        {
            return StringValidateResult.AtLeastOneLowerChar;
        }

        if (!Regex.IsMatch(str, @"[A-Z]+"))
        {
            return StringValidateResult.AtLeastOneUpperChar;
        }

        if (!Regex.IsMatch(str, @"[`~\!@#\$%\^\&\*\(\)\-_\=\+\[\{\}\]\\\|;:'"",<.>\/\?€£¥₹]+"))
        {
            return StringValidateResult.AtLeastOneSpecialChar;
        }

        if (Regex.IsMatch(str, @"[\s]+"))
        {
            return StringValidateResult.CannotContainsSpace;
        }

        return StringValidateResult.OK;
    }

    /// <summary>
    /// Validates a string against username requirements.
    /// Usernames must be 6-30 characters long.
    /// </summary>
    /// <param name="str">The string to validate as a username.</param>
    /// <returns>A <see cref="StringValidateResult"/> indicating the validation outcome.</returns>
    public static StringValidateResult MatchUserName(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return StringValidateResult.MustFill;
        }

        if (str.Length < 6)
        {
            return StringValidateResult.MinLength6;
        }

        if (str.Length > 30)
        {
            return StringValidateResult.TooLong;
        }

        return StringValidateResult.OK;
    }

    /// <summary>
    /// Validates a string against email format requirements.
    /// </summary>
    /// <param name="str">The string to validate as an email address.</param>
    /// <returns>A <see cref="StringValidateResult"/> indicating the validation outcome.</returns>
    public static StringValidateResult MatchEmail(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return StringValidateResult.MustFill;
        }

        if (!Regex.IsMatch(str, @"^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$"))
        {
            return StringValidateResult.InvalidFormat;
        }

        return StringValidateResult.OK;
    }

    /// <summary>
    /// Validates a string against activation code requirements.
    /// Activation codes must be exactly 6 numeric digits.
    /// </summary>
    /// <param name="str">The string to validate as an activation code.</param>
    /// <returns>A <see cref="StringValidateResult"/> indicating the validation outcome.</returns>
    public static StringValidateResult MatchActivationCode(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return StringValidateResult.MustFill;
        }

        if (str.Length != 6)
        {
            return StringValidateResult.Length6;
        }

        if (!Regex.IsMatch(str, @"^\d{6}$"))
        {
            return StringValidateResult.Numeric;
        }

        return StringValidateResult.OK;
    }
}

/// <summary>
/// Represents a validated string value with a custom validator function and validation result tracking.
/// </summary>
public class StringValidateValue
{
    /// <summary>
    /// Gets or sets the current string value to be validated.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets the validator function used to validate the <see cref="Value"/>.
    /// </summary>
    public Func<string, StringValidateResult> Validator { get; }

    /// <summary>
    /// Gets or sets the result of the last validation. Null if not yet validated.
    /// </summary>
    public StringValidateResult? Result { get; set; }

    /// <summary>
    /// Gets or sets an external result message to display instead of the default validation message.
    /// </summary>
    public string ExResultMessage { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StringValidateValue"/> class with the specified validator.
    /// </summary>
    /// <param name="validator">The validation function. Cannot be null.</param>
    public StringValidateValue(Func<string, StringValidateResult> validator)
    {
        Validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Validates the current <see cref="Value"/> using the configured <see cref="Validator"/> and stores the result.
    /// </summary>
    public void Validate()
    {
        Result = Validator(Value);
    }

    /// <summary>
    /// Resets the validation result and external result message to their default states.
    /// </summary>
    public void ResetResult()
    {
        Result = null;
        ExResultMessage = null;
    }

    /// <summary>
    /// Clears the current value and resets the validation result.
    /// </summary>
    public void Clear()
    {
        Value = string.Empty;
        Result = null;
    }

    /// <summary>
    /// Gets a value indicating whether the last validation result was successful.
    /// </summary>
    public bool IsValid => Result == StringValidateResult.OK;

    /// <summary>
    /// Gets the display message for the current validation result.
    /// Returns <see cref="ExResultMessage"/> if set, otherwise returns the display text of the <see cref="Result"/>.
    /// </summary>
    public string Message
    {
        get
        {
            if (Result is { } result && result != StringValidateResult.OK)
            {
                return result.ToDisplayText();
            }

            return ExResultMessage ?? string.Empty;
        }
    }
}
