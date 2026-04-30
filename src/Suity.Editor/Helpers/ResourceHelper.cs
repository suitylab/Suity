using ComputerBeacon.Json;
using MarkedNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suity.Editor.Helpers;


/// <summary>
/// Utility class for file operations, text manipulation, URL parsing, code block extraction, and JSON extraction from markdown text.
/// </summary>
public static class ResourceHelper
{
    /// <summary>
    /// Reads a specific line from a file located relative to the current working directory.
    /// </summary>
    /// <param name="filepath">The relative path to the file.</param>
    /// <param name="line_number">The 1-based line number to read. Defaults to 1.</param>
    /// <returns>The content of the specified line, or an empty string if an error occurs.</returns>
    public static string FileSystemEnv_read_from_file(string filepath, int line_number = 1)
    {
        string content = string.Empty;

        try
        {
            // Build the full path of the file using relative path
            string fullPath = Path.Combine(System.Environment.CurrentDirectory, filepath);

            // Read file content line by line
            using var reader = new StreamReader(fullPath);
            for (int i = 1; i < line_number; i++)
            {
                // Skip content before the specified number of lines
                reader.ReadLine();
            }

            // Read the content of the specified line
            content = reader.ReadLine();
        }
        catch (Exception ex)
        {
            // Handle exceptions when reading file
            Console.WriteLine("An error occurred while reading the file: " + ex.Message);
        }

        return content;
    }

    /// <summary>
    /// Writes content to a file with options for truncating, inserting, or overwriting at a specific line.
    /// </summary>
    /// <param name="filepath">The relative path to the file.</param>
    /// <param name="new_content">The content to write to the file.</param>
    /// <param name="truncating">If true, clears the file content before writing.</param>
    /// <param name="line_number">The 1-based line number to insert or overwrite at. If null, appends to the end.</param>
    /// <param name="overwrite">If true, overwrites the line at the specified position; otherwise inserts before it.</param>
    /// <returns>The updated file content as a single string, or an empty string if an error occurs.</returns>
    public static string FileSystemEnv_write_to_file(string filepath, string new_content, bool truncating = false, int? line_number = null, bool overwrite = false)
    {
        try
        {
            // Build the full path of the file using relative path
            string fullPath = Path.Combine(System.Environment.CurrentDirectory, filepath);

            if (truncating)
            {
                // Truncate file content
                File.WriteAllText(fullPath, string.Empty);
            }

            // Read file content
            string[] lines = File.ReadAllLines(fullPath);

            if (line_number.HasValue && line_number.Value >= 1 && line_number.Value <= lines.Length)
            {
                if (overwrite)
                {
                    // Overwrite content starting from the specified line
                    lines[line_number.Value - 1] = new_content;
                }
                else
                {
                    // Insert new content at the specified line
                    List<string> updatedLines = [.. lines];
                    updatedLines.Insert(line_number.Value - 1, new_content);
                    lines = [.. updatedLines];
                }
            }
            else
            {
                // Append new content to the end of the file
                lines = [.. lines, new_content];
            }

            // Write the modified content to the file
            File.WriteAllLines(fullPath, lines);
        }
        catch (Exception ex)
        {
            // Handle exceptions when writing to file
            Console.WriteLine("An error occurred while writing to the file: " + ex.Message);
        }

        // Return the updated file content
        return string.Join(System.Environment.NewLine, File.ReadAllLines(Path.Combine(System.Environment.CurrentDirectory, filepath)));
    }

    /// <summary>
    /// Reads text content starting from a specified line number.
    /// </summary>
    /// <param name="content">The full text content to process.</param>
    /// <param name="line_number">The 1-based line number to start reading from. If 1 or less, returns the entire content.</param>
    /// <returns>The text content from the specified line onward, or an empty string if the input is empty or the line number is out of range.</returns>
    public static string ReadText(string content, int line_number = 1)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }
        else if (line_number <= 1)
        {
            return content;
        }
        else
        {
            string[] lines = content.Split([System.Environment.NewLine], StringSplitOptions.None);

            if (line_number >= 1 && line_number <= lines.Length)
            {
                // Starting from the specified line, merge all following lines into a single string
                content = string.Join(System.Environment.NewLine, lines.Skip(line_number - 1));
            }
            else
            {
                content = string.Empty;
            }

            return content;
        }
    }

    /// <summary>
    /// Modifies text content by inserting, overwriting, or truncating at a specified line number.
    /// </summary>
    /// <param name="content">The original text content to modify.</param>
    /// <param name="new_content">The new content to insert or use for overwriting.</param>
    /// <param name="truncating">If true, clears the original content before applying changes.</param>
    /// <param name="line_number">The 1-based line number to modify at. If null, appends to the end.</param>
    /// <param name="overwrite">If true, replaces content starting at the specified line; otherwise inserts before it.</param>
    /// <returns>The modified text content.</returns>
    public static string ModifyText(string content, string new_content, bool truncating = false, int? line_number = null, bool overwrite = false)
    {
        try
        {
            if (truncating)
            {
                // If content needs to be truncated, clear the original text content
                content = string.Empty;
            }

            // Split text content into an array of lines
            string[] lines = content.Split([System.Environment.NewLine], StringSplitOptions.None);

            if (line_number.HasValue && line_number.Value >= 1 && line_number.Value <= lines.Length)
            {
                if (overwrite)
                {
                    // Overwrite content starting from the specified line
                    int startIndex = line_number.Value - 1;
                    Array.Clear(lines, startIndex, lines.Length - startIndex);
                    lines[startIndex] = new_content;
                }
                else
                {
                    // Insert new content before the specified line
                    List<string> updatedLines = [.. lines];
                    updatedLines.Insert(line_number.Value - 1, new_content);
                    lines = [.. updatedLines];
                }
            }
            else
            {
                // Append new content to the end of the text
                List<string> updatedLines = [.. lines];
                updatedLines.Add(new_content);
                lines = [.. updatedLines];
            }

            // Merge the modified content into new text
            content = string.Join(System.Environment.NewLine, lines);
        }
        catch (Exception ex)
        {
            // Handle exceptions when modifying text
            ex.LogError("An error occurred while modifying the text: " + ex.Message);

            throw;
        }

        // Return the updated text content
        return content;
    }



    /// <summary>
    /// Parses a URL into its scheme and address components by splitting on "://".
    /// </summary>
    /// <param name="url">The URL string to parse.</param>
    /// <returns>A tuple containing the scheme (e.g., "http") and the address (e.g., "example.com/path"). Returns empty strings if parsing fails.</returns>
    public static (string scheme, string address) ParseUrl(string url)
    {
        try
        {
            var (part1, part2) = SplitString(url, "://");

            return (part1, part2);
        }
        catch (UriFormatException ex)
        {
            Console.WriteLine("An error occurred while parsing the URL: " + ex.Message);
            return (string.Empty, string.Empty);
        }
    }

    /// <summary>
    /// Splits a string into two parts at the first occurrence of a specified delimiter.
    /// </summary>
    /// <param name="input">The string to split.</param>
    /// <param name="delimiter">The delimiter to split on.</param>
    /// <returns>A tuple containing the part before the delimiter and the part after it. If the delimiter is not found, returns the original string as the first part and an empty string as the second.</returns>
    public static (string part1, string part2) SplitString(string input, string delimiter)
    {
        int delimiterIndex = input.IndexOf(delimiter);

        if (delimiterIndex != -1)
        {
            string part1 = input[..delimiterIndex];
            string part2 = input[(delimiterIndex + delimiter.Length)..];

            return (part1, part2);
        }
        else
        {
            // If the split string is not found, return the original string as the first part and an empty string as the second part
            return (input, string.Empty);
        }
    }


    /// <summary>
    /// Extracts the first code block from a markdown-formatted string.
    /// </summary>
    /// <param name="s">The markdown text to parse.</param>
    /// <param name="code">When this method returns, contains the extracted code block text, or null if no code block was found.</param>
    /// <returns>True if a code block was found and extracted; otherwise, false.</returns>
    public static bool TryExtactCodeBlock(string s, out string code)
    {
        if (string.IsNullOrWhiteSpace(s))
        {
            code = null;
            return false;
        }

        var tokens = MarkedNet.Lexer.Lex(s, new Options());
        if (tokens is null || tokens.Tokens.Count == 0)
        {
            code = null;
            return false;
        }

        foreach (var token in tokens.Tokens.Where(o => o.Type == "code"))
        {
            code = token.Text?.TrimStart() ?? string.Empty;

            return true;
        }

        code = null;
        return false;
    }

    /// <summary>
    /// Extracts a JSON object from a string, supporting both raw JSON and JSON within markdown code blocks.
    /// </summary>
    /// <param name="s">The input string that may contain JSON.</param>
    /// <param name="obj">When this method returns, contains the parsed <see cref="JsonObject"/>, or null if no valid JSON was found.</param>
    /// <returns>True if a valid JSON object was found and parsed; otherwise, false.</returns>
    public static bool TryExtractJson(string s, out JsonObject obj)
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

    private static bool TryParseJson(string s, out JsonObject obj)
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

}