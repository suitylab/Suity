using Suity.Collections;
using Suity.Editor.Services;
using Suity.Editor.Values;
using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Suity.Editor.Types;

/// <summary>
/// Native type reflector
/// </summary>
internal sealed class NativeTypeReflector
{
    /// <summary>
    /// Gets the singleton instance of <see cref="NativeTypeReflector"/>.
    /// </summary>
    public static readonly NativeTypeReflector Instance = new();
    private bool _init;

    private readonly Dictionary<string, NativeTypeLibrary> _libraries = [];
    private readonly Dictionary<Type, DType> _nativeTypes = [];

    // Interfaces need special handling because they can be used to get Assets
    private readonly HashSet<Type> _interfaces = [];

    internal NativeTypeReflector()
    {
        // Need to ensure this runs after NativeTypeBackend initialization
        // InternalRexes.EditorAwake.AddActionListener(Initialize);
    }

    /// <summary>
    /// Gets the <see cref="DType"/> associated with the specified native type.
    /// </summary>
    /// <param name="type">The native .NET type.</param>
    /// <returns>The associated <see cref="DType"/>, or null if not found.</returns>
    public DType GetDType(Type type)
    {
        return _nativeTypes.GetValueSafe(type);
    }

    /// <summary>
    /// Gets all interface types that have been registered.
    /// </summary>
    public IEnumerable<Type> InterfaceTypes => _interfaces;

    /// <summary>
    /// Initializes the reflector using registered assemblies from the editor services.
    /// </summary>
    public void Initialize() => Initialize(EditorServices.AssemblyService.RegisteredAssemblies);

    /// <summary>
    /// Initializes the reflector by scanning the specified assemblies for native types.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    public void Initialize(IEnumerable<Assembly> assemblies)
    {
        if (_init)
        {
            return;
        }
        _init = true;

        EditorServices.SystemLog.AddLog("AssemblyTypeReflector Initializing...");
        EditorServices.SystemLog.PushIndent();

        AddAssemblyTypesToLibrary(assemblies);

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog("AssemblyTypeReflector Initialized.");
    }

    private void AddAssemblyTypesToLibrary(IEnumerable<Assembly> assemblies)
    {
        if (assemblies is null)
        {
            return;
        }

        //Dictionary<string, NativeTypeLibrary> assets = [];

        foreach (Assembly assembly in assemblies)
        {
            EditorServices.SystemLog.AddLog($"Scanning {assembly.GetShortAssemblyName()}...");
            EditorServices.SystemLog.PushIndent();

            try
            {
                ReflectTypes(assembly, _libraries);
                foreach (var asset in _libraries.Values)
                {
                    asset.ResolveId();
                }
            }
            catch (Exception err)
            {
                err.LogError($"Reflect type failed : {assembly.GetShortAssemblyName()}");
                continue;
            }

            EditorServices.SystemLog.PopIndent();
        }
    }

    /// <summary>
    /// Reflect an assembly
    /// </summary>
    /// <param name="assembly">Assembly</param>
    /// <returns>Returns the reflected type library</returns>
    private NativeTypeLibrary[] ReflectTypes(Assembly assembly, Dictionary<string, NativeTypeLibrary> assets)
    {
        // GetType means private types are also visible
        // If using GetExportedTypes, only public types will be searched
        Type[] types = null;

        try
        {
            types = assembly.GetExportedTypes();
        }
        catch (Exception err)
        {
            err.LogError($"Get assembly types failed : {assembly.GetShortAssemblyName()}");
        }

        if (types is null)
        {
            return [];
        }

        foreach (Type nativeType in types)
        {
            //if (!nativeType.IsPublic) continue;

            try
            {
                ReflectType(assembly, nativeType, assets);
            }
            catch (Exception err)
            {
                err.LogError($"Reflect type failed : {nativeType.Name}");
            }
        }

        return [.. assets.Values];
    }

    private void ReflectType(Assembly assembly, Type type, Dictionary<string, NativeTypeLibrary> assets)
    {
        NativeTypeAttribute attr = type.GetAttributeCached<NativeTypeAttribute>();
        if (attr is null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(attr.Name) && !NamingVerifier.VerifyIdentifier(attr.Name))
        {
            Logs.LogError($"Invalid native type name : {attr.Name} for type {type.FullName}, it must be a valid identifier. Please check your 'NativeType' attribute.");
        }

        if (string.IsNullOrWhiteSpace(attr.CodeBase))
        {
            Logs.LogWarning($"CodeBase is not specified for type {type.FullName}, using '*Suity' as default codebase. Please check your 'NativeType' attribute.");
        }

        string codeBase = attr.CodeBase?.Trim();
        if (string.IsNullOrWhiteSpace(codeBase))
        {
            codeBase = "Suity";
        }

        if (!codeBase.StartsWith("*"))
        {
            codeBase = "*" + codeBase;
        }

        if (type.IsClass || type.IsInterface)
        {
            NativeTypeLibrary library = EnsureTypeLibrary(codeBase, assets);

            if (typeof(SObjectController).IsAssignableFrom(type))
            {
                //EditorUtility.LogCore?.LogDebug($"Failed to add dtype for {nativeType.GetTypeCSCodeName()} (must derived from SObjectController)");
                var dStruct = MakeNativeStruct(type, attr);
                library.AddType(type, dStruct);
                _nativeTypes.Add(type, dStruct);

                EditorServices.SystemLog.AddLog($"Add struct {dStruct.GetType().Name} {dStruct.Name} for {type.GetTypeCSCodeName()}");
            }
            else
            {
                var ntype = new DNativeType(type, attr);
                library.AddType(type, ntype);
                _nativeTypes.Add(type, ntype);

                if (type.IsInterface)
                {
                    _interfaces.Add(type);
                }

                EditorServices.SystemLog.AddLog($"Add type {ntype.GetType().Name} {ntype.Name} for {type.GetTypeCSCodeName()}");
            }
        }
        else if (type.IsEnum)
        {
            NativeTypeLibrary library = EnsureTypeLibrary(codeBase, assets);

            DEnum dEnum = MakeNativeEnum(type, attr);
            library.AddType(type, dEnum);

            _nativeTypes.Add(type, dEnum);

            EditorServices.SystemLog.AddLog($"Add enum {dEnum.GetType().Name} {dEnum.Name} for {type.GetTypeCSCodeName()}");
        }
        else
        {
            EditorServices.SystemLog.AddLog($"Failed to add dtype for {type.GetTypeCSCodeName()}");
        }
    }

    private DCompond MakeNativeStruct(Type type, NativeTypeAttribute structAttr)
    {
        if (!string.IsNullOrWhiteSpace(structAttr.ReturnType) || structAttr.ReturnTypeBinding != DReturnTypeBinding.None)
        {
            // Function
            var nativeFunction = new DNativeFunction(
                !string.IsNullOrWhiteSpace(structAttr.Name) ? structAttr.Name : type.Name,
                type)
            {
                Description = structAttr.Description,
                Detail = structAttr.Detail,
                Brief = structAttr.Brief,
                //Category = structAttr.Category,
                ReturnTypeBinding = structAttr.ReturnTypeBinding,
                IconKey = !string.IsNullOrWhiteSpace(structAttr.Icon) ? structAttr.Icon : "*CoreIcon|System",
                IsPrimary = structAttr.IsPrimaryType,
                BindingInfo = type
            };

            NativeReturnTypeAttribute returnTypeAttr = type.GetAttributeCached<NativeReturnTypeAttribute>();
            if (returnTypeAttr != null)
            {
                nativeFunction.ReturnType = TypeDefinition.Resolve(returnTypeAttr.ReturnType);
            }
            else
            {
                nativeFunction.ReturnType = TypeDefinition.Resolve(structAttr.ReturnType);
            }

            if (type.HasAttributeCached<NativePrimaryAttribute>())
            {
                nativeFunction.IsPrimary = true;
            }

            return nativeFunction;
        }
        else
        {
            // Structure
            var nativeClass = new DNativeStruct(
                !string.IsNullOrWhiteSpace(structAttr.Name) ? structAttr.Name : type.Name,
                type)
            {
                Description = structAttr.Description,
                Detail = structAttr.Detail,
                Brief = structAttr.Brief,
                //Category = structAttr.Category,
                IconKey = !string.IsNullOrWhiteSpace(structAttr.Icon) ? structAttr.Icon : "*CoreIcon|System",
                IsPrimary = structAttr.IsPrimaryType,
                BindingInfo = type
            };

            // Configure abstract struct implementation
            NativeAbstractAttribute abstractAttr = type.GetAttributeCached<NativeAbstractAttribute>();
            if (abstractAttr != null)
            {
                if (abstractAttr.AbstractTypes.Length > 0)
                {
                    nativeClass.SetBaseType(abstractAttr.AbstractTypes[0]);
                }
            }
            else if (!string.IsNullOrWhiteSpace(structAttr.BaseType))
            {
                nativeClass.SetBaseType(structAttr.BaseType);
            }

            if (type.HasAttributeCached<NativePrimaryAttribute>())
            {
                nativeClass.IsPrimary = true;
            }

            return nativeClass;
        }
    }

    /// <summary>
    /// Create enum information
    /// </summary>
    /// <param name="nativeEnumType">Native enum type</param>
    /// <param name="enumAttr">Enum attribute</param>
    /// <returns>Returns the created enum type</returns>
    private DNativeEnum MakeNativeEnum(Type nativeEnumType, NativeTypeAttribute enumAttr)
    {
        string enumName = !string.IsNullOrWhiteSpace(enumAttr.Name) ? enumAttr.Name : nativeEnumType.Name;

        var pxEnum = new DNativeEnum(enumName, nativeEnumType)
        {
            Description = enumAttr.Description,
            BindingInfo = nativeEnumType
        };

        foreach (FieldInfo nativeField in nativeEnumType.GetFields())
        {
            if (nativeField.FieldType != nativeEnumType)
            {
                continue;
            }

            int index = Convert.ToInt32(nativeField.GetValue(null));

            NativeFieldAttribute fieldAttr = nativeField.GetAttributeCached<NativeFieldAttribute>();

            int id = 0;
            if (fieldAttr != null && !int.TryParse(fieldAttr.DefaultValue, out id))
            {
                id = 0;
            }

            if (fieldAttr != null)
            {
                string displayName = !string.IsNullOrWhiteSpace(fieldAttr.Description) ? fieldAttr.Description : fieldAttr.Name;
                if (string.IsNullOrWhiteSpace(displayName)) displayName = nativeField.Name;

                pxEnum.AddOrUpdateField(nativeField.Name, id, displayName, fieldAttr, null);
            }
            else
            {
                pxEnum.AddOrUpdateField(nativeField.Name, id, null, fieldAttr, null);
            }

            pxEnum.UpdateField(nativeField.Name, index);
        }

        return pxEnum;
    }

    internal NativeTypeLibrary EnsureTypeLibrary(string codeBase) => EnsureTypeLibrary(codeBase, this._libraries);

    private NativeTypeLibrary EnsureTypeLibrary(string codeBase, Dictionary<string, NativeTypeLibrary> assets)
    {
        NativeTypeLibrary library = assets.GetValueSafe(codeBase);
        if (library != null)
        {
            return library;
        }

        library = AssetManager.Instance.GetAsset<NativeTypeLibrary>(codeBase);
        if (library != null)
        {
            assets.Add(codeBase, library);

            return library;
        }

        library = new NativeTypeLibrary(codeBase);
        library.ResolveId();

        assets.Add(codeBase, library);

        return library;
    }
}