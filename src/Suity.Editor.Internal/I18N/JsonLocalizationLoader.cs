/// @file
/// @copyright  Copyright (c) 2023-2024 SafeTwice S.L. All rights reserved.
/// @license    See LICENSE.txt

using System;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace I18N.DotNet
{
    /// <summary>
    /// Loads JSON localization files into a flat table of dot-separated keys.
    /// </summary>
    /// <remarks>
    /// One file per language. The file contents are nested JSON objects whose leaf values are the
    /// localized strings, e.g. <c>{ "nav": { "home": "Home" } }</c> produces the entry <c>nav.home</c>.
    /// Arrays are flattened using their index, e.g. <c>a[0]</c>.
    /// </remarks>
    internal static class JsonLocalizationLoader
    {
        public static void Load( Stream stream, Action<string, string> addEntry )
        {
            if( stream == null )
            {
                throw new ArgumentNullException( nameof( stream ) );
            }

            if( addEntry == null )
            {
                throw new ArgumentNullException( nameof( addEntry ) );
            }

            JsonDocument document;

            try
            {
                document = JsonDocument.Parse( stream );
            }
            catch( JsonException err )
            {
                throw new ILoadableLocalizer.ParseException( $"Invalid JSON localization: {err.Message}" );
            }

            using( document )
            {
                var root = document.RootElement;

                if( root.ValueKind != JsonValueKind.Object )
                {
                    throw new ILoadableLocalizer.ParseException( "Invalid JSON localization: the root element must be an object" );
                }

                FlattenObject( root, string.Empty, addEntry );
            }
        }

        private static void FlattenObject( JsonElement element, string prefix, Action<string, string> addEntry )
        {
            foreach( var property in element.EnumerateObject() )
            {
                string key = string.IsNullOrEmpty( prefix ) ? property.Name : prefix + "." + property.Name;
                FlattenValue( property.Value, key, addEntry );
            }
        }

        private static void FlattenValue( JsonElement value, string key, Action<string, string> addEntry )
        {
            switch( value.ValueKind )
            {
                case JsonValueKind.Object:
                    FlattenObject( value, key, addEntry );
                    break;

                case JsonValueKind.Array:
                    int index = 0;
                    foreach( var item in value.EnumerateArray() )
                    {
                        FlattenValue( item, key + "[" + index.ToString( CultureInfo.InvariantCulture ) + "]", addEntry );
                        index++;
                    }
                    break;

                case JsonValueKind.String:
                    addEntry( key, value.GetString() ?? string.Empty );
                    break;

                case JsonValueKind.Number:
                    addEntry( key, value.GetRawText() );
                    break;

                case JsonValueKind.True:
                    addEntry( key, "true" );
                    break;

                case JsonValueKind.False:
                    addEntry( key, "false" );
                    break;
            }
        }
    }
}
