using Suity.Collections;
using Suity.Editor.Design;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Json;
using System.Collections.Generic;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Utility class for generating JSON schema overviews and briefs from DCompond types.
/// Provides extension methods to convert compound types into JSON schema representations,
/// markdown documentation, and brief descriptions.
/// </summary>
public static class JsonSchemaUtility
{
    /// <summary>
    /// Generates a JSON schema overview for the properties of a DCompond type.
    /// </summary>
    /// <param name="type">The compound type to generate the schema for.</param>
    /// <param name="ctx">Optional function context for schema generation.</param>
    /// <param name="depth">Maximum depth to traverse nested types. Defaults to 10.</param>
    /// <param name="typeGuiding">Whether to include type guiding information in the schema. Defaults to true.</param>
    /// <returns>A JSON string representing the schema property overview, or an empty string if the type is null.</returns>
    public static string ToSchemaPropertyOverview(this DCompond type, FunctionContext ctx = null, int depth = 10, bool typeGuiding = true)
    {
        if (type is null)
        {
            return string.Empty;
        }

        string desc = type.GetAttribute<ToolTipsAttribute>()?.ToolTips;
        if (string.IsNullOrWhiteSpace(desc))
        {
            desc = null;
        }

        var schema = EditorServices.JsonSchemaService;

        var option = new SchemaGenerateOptions
        {
            Context = ctx,
            Description = desc,
            Depth = depth,
            TypeGuiding = typeGuiding,
        };

        var prop = schema.CreateSchemaProperty(type, option);

        return prop.ToString();
    }


    /// <summary>
    /// Generates a JSON schema overview from an object, dispatching to the appropriate handler
    /// based on the object's type (string, TypeDefinition, or DCompond).
    /// </summary>
    /// <param name="type">The object to generate a schema overview for.</param>
    /// <param name="ctx">Optional function context for schema generation.</param>
    /// <param name="depth">Maximum depth to traverse nested types. Defaults to 10.</param>
    /// <param name="typeGuiding">Whether to include type guiding information. Defaults to true.</param>
    /// <returns>A JSON schema string, the original string if input is a string, or null if the type is unsupported.</returns>
    public static string ToSchemaOverview(this object type, FunctionContext ctx = null, int depth = 10, bool typeGuiding = true)
    {
        if (type is string str)
        {
            return str;
        }
        else if (type is TypeDefinition typeDef)
        {
            return typeDef.ToSchemaOverview(ctx, depth, typeGuiding);
        }
        else if (type is DCompond dcompond)
        {
            return dcompond.ToSchemaOverview(ctx, depth, typeGuiding);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Generates a JSON schema overview for a TypeDefinition by delegating to its target DCompond.
    /// </summary>
    /// <param name="type">The type definition to generate a schema overview for.</param>
    /// <param name="ctx">Optional function context for schema generation.</param>
    /// <param name="depth">Maximum depth to traverse nested types. Defaults to 10.</param>
    /// <param name="typeGuiding">Whether to include type guiding information. Defaults to true.</param>
    /// <returns>A JSON schema string if the target is a DCompond, or null otherwise.</returns>
    public static string ToSchemaOverview(this TypeDefinition type, FunctionContext ctx = null, int depth = 10, bool typeGuiding = true)
    {
        if (type.Target is DCompond dcompond)
        {
            return dcompond.ToSchemaOverview(ctx, depth);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Generates a full JSON schema overview for a DCompond type using the JSON schema service.
    /// </summary>
    /// <param name="type">The compound type to generate the schema for.</param>
    /// <param name="ctx">Optional function context for schema generation.</param>
    /// <param name="depth">Maximum depth to traverse nested types. Defaults to 10.</param>
    /// <param name="typeGuiding">Whether to include type guiding information in the schema. Defaults to true.</param>
    /// <returns>A JSON string representing the full schema, or an empty string if the type is null.</returns>
    public static string ToSchemaOverview(this DCompond type, FunctionContext ctx = null, int depth = 10, bool typeGuiding = true)
    {
        if (type is null)
        {
            return string.Empty;
        }

        var schema = EditorServices.JsonSchemaService;

        var option = new SchemaGenerateOptions
        {
            Context = ctx,
            Depth = depth,
            TypeGuiding = typeGuiding,
        };

        var prop = schema.CreateSchema(type, option);

        var writer = new JsonDataWriter(); //new IndentDataWriter();
        prop.WriteData(writer);

        string str = writer.ToString();

        return str;
    }

    /// <summary>
    /// Generates a markdown-formatted schema overview for a DCompond type.
    /// </summary>
    /// <param name="type">The compound type to generate the markdown schema for.</param>
    /// <param name="ctx">Optional function context for schema generation.</param>
    /// <param name="depth">Maximum depth to traverse nested types. Defaults to 10.</param>
    /// <param name="typeGuiding">Whether to include type guiding information. Defaults to true.</param>
    /// <returns>A markdown-formatted string representing the schema, or an empty string if the type is null.</returns>
    public static string ToSchemaOverviewMarkdown(this DCompond type, FunctionContext ctx = null, int depth = 10, bool typeGuiding = true)
    {
        if (type is null)
        {
            return string.Empty;
        }

        var schema = EditorServices.JsonSchemaService;

        var option = new SchemaGenerateOptions
        {
            Context = ctx,
            Depth = depth,
            TypeGuiding = typeGuiding,
        };

        var prop = schema.CreateSchema(type, option);

        var writer = new MarkdownDataWriter();
        prop.WriteData(writer);

        string str = writer.ToString();

        return str;
    }

    /// <summary>
    /// Generates a brief text summary for a collection of DCompond types, including their names and descriptions.
    /// </summary>
    /// <param name="types">The collection of compound types to summarize.</param>
    /// <param name="prefix">A prefix string to prepend to each type name in the output.</param>
    /// <returns>A formatted string containing the name and description of each type.</returns>
    public static string ToSchemaBrief(this IEnumerable<DCompond> types, string prefix)
    {
        var builder = new StringBuilder();

        foreach (var type in types.SkipNull())
        {
            builder.AppendLine($"[{prefix} Name] {type.Name}");

            string desc = type.GetAttribute<ToolTipsAttribute>()?.ToolTips;
            if (!string.IsNullOrWhiteSpace(desc))
            {
                builder.Append($"[Description] ");
                builder.Append(desc);
            }

            builder.AppendLine();
            builder.AppendLine();
        }

        string s = builder.ToString();

        return s;
    }

    /// <summary>
    /// Generates a combined JSON schema overview for a collection of DCompond types.
    /// Each type's schema is appended to the result with a trailing newline.
    /// </summary>
    /// <param name="types">The collection of compound types to generate schemas for.</param>
    /// <param name="ctx">Optional function context for schema generation.</param>
    /// <param name="depth">Maximum depth to traverse nested types. Defaults to 10.</param>
    /// <param name="typeGuiding">Whether to include type guiding information. Defaults to true.</param>
    /// <returns>A concatenated string of all schema overviews.</returns>
    public static string ToSchemaOverview(this IEnumerable<DCompond> types, FunctionContext ctx = null, int depth = 10, bool typeGuiding = true)
    {
        var builder = new StringBuilder();

        foreach (var type in types.SkipNull())
        {
            string overview = type.ToSchemaOverview(ctx, depth, typeGuiding);
            builder.AppendLine(overview);
            builder.AppendLine();
        }

        return builder.ToString();
    }
}