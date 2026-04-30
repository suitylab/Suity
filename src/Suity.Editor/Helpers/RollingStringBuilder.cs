using System;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// A StringBuilder that maintains a maximum length by automatically removing
/// old content from the beginning when new content is appended.
/// Useful for scenarios like rolling logs or circular buffers where only
/// the most recent content should be retained.
/// </summary>
public class RollingStringBuilder
{
    private readonly int _maxLength;
    private readonly StringBuilder _sb;

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingStringBuilder"/> class
    /// with the specified maximum length.
    /// </summary>
    /// <param name="maxLength">The maximum number of characters to retain. Must be greater than zero.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="maxLength"/> is less than or equal to zero.</exception>
    public RollingStringBuilder(int maxLength)
    {
        if (maxLength <= 0)
            throw new ArgumentException("MaxLength must be greater than zero.", nameof(maxLength));

        _maxLength = maxLength;
        _sb = new StringBuilder();
    }

    /// <summary>
    /// Appends the specified string value to the builder. If the resulting length
    /// would exceed <see cref="MaxLength"/>, old characters are removed from the
    /// beginning to maintain the maximum length constraint.
    /// </summary>
    /// <param name="value">The string to append. Null or empty values are ignored.</param>
    public void Append(string value)
    {
        if (string.IsNullOrEmpty(value))
            return;

        // If appending would exceed max length, truncate the head first
        int totalLength = _sb.Length + value.Length;
        if (totalLength > _maxLength)
        {
            int excess = totalLength - _maxLength; // Number of characters to remove
            // Remove the first excess characters
            _sb.Remove(0, Math.Min(excess, _sb.Length));
        }

        _sb.Append(value);

        // Extra safety: in case value itself is longer than maxLength (e.g., appending an extremely long string at once)
        if (_sb.Length > _maxLength)
        {
            _sb.Remove(0, _sb.Length - _maxLength);
        }
    }

    /// <summary>
    /// Returns the current content of the builder as a string.
    /// </summary>
    /// <returns>The string representation of the current content.</returns>
    public override string ToString() => _sb.ToString();

    /// <summary>
    /// Gets the current number of characters in the builder.
    /// </summary>
    public int Length => _sb.Length;

    /// <summary>
    /// Gets the maximum number of characters that this builder will retain.
    /// </summary>
    public int MaxLength => _maxLength;

    /// <summary>
    /// Removes all characters from the builder, resetting it to an empty state.
    /// </summary>
    public void Clear() => _sb.Clear();
}