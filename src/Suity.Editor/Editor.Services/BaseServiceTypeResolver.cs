using Suity.Collections;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Services;

/// <summary>
/// Service type resolver, used to resolve object types to service types
/// </summary>
/// <typeparam name="TService"></typeparam>
/// <typeparam name="TAttribute"></typeparam>
/// <typeparam name="TDefaultAttribute"></typeparam>
public abstract class BaseServiceTypeResolver<TService, TAttribute, TDefaultAttribute>
    where TAttribute : Attribute where TDefaultAttribute : Attribute
{
    private static readonly UniqueMultiDictionary<Type, Type> _serviceTypes = new();
    private static readonly UniqueMultiDictionary<Type, Type> _defaultServiceTypes = new();
    private static bool _init;

    private string _typeName;

    protected BaseServiceTypeResolver(string typeName)
    {
        _typeName = typeName;

        EditorRexes.EditorAwake.AddActionListener(Initialize);
    }

    public bool IsInitialized => _init;

    public void Initialize()
    {
        if (_init)
        {
            return;
        }

        EditorServices.SystemLog.AddLog($"{this.GetType()} Initializing...");
        EditorServices.SystemLog.PushIndent();

        foreach (Type serviceType in typeof(TService).GetDerivedTypes())
        {
            TAttribute[] attrs = serviceType.GetAttributesCached<TAttribute>().ToArray();

            foreach (var attr in attrs)
            {
                var objType = GetTargetType(attr);

                if (objType is null)
                {
                    continue;
                }

                if (serviceType.HasAttributeCached<TDefaultAttribute>())
                {
                    EditorServices.SystemLog.AddLog($"Add default {_typeName} type : {serviceType.Name} for {objType.Name}");
                    _defaultServiceTypes.Add(objType, serviceType);
                }
                else
                {
                    EditorServices.SystemLog.AddLog($"Add {_typeName} : {serviceType.Name} for {objType.Name}");
                    _serviceTypes.Add(objType, serviceType);
                }
            }
        }

        _init = true;

        EditorServices.SystemLog.PopIndent();
        EditorServices.SystemLog.AddLog($"{this.GetType()} Initialized.");
    }

    protected abstract Type GetTargetType(TAttribute attribute);

    public Type ResolveServiceType(Type objectType, bool resolveDefault = true)
    {
        if (!_init)
        {
            Initialize();
        }

        Type cType = objectType;
        while (cType != null)
        {
            Type viewType = ResolveMultipleType(_serviceTypes[cType]);
            if (viewType != null)
            {
                return viewType;
            }

            cType = cType.BaseType;
        }

        if (resolveDefault)
        {
            return ResolveDefaultServiceType(objectType);
        }

        return null;
    }

    public Type ResolveDefaultServiceType(Type objectType)
    {
        if (!_init)
        {
            Initialize();
        }

        Type cType = objectType;
        while (cType != null)
        {
            Type viewType = ResolveMultipleType(_defaultServiceTypes[cType]);
            if (viewType != null)
            {
                return viewType;
            }

            cType = cType.BaseType;
        }

        return null;
    }

    private Type ResolveMultipleType(IEnumerable<Type> types)
    {
        if (types.CountOne())
        {
            return types.First();
        }

        Type overrideType = types.FirstOrDefault(o => o.HasAttributeCached<RequestOverrideAttribute>());
        return overrideType ?? types.FirstOrDefault();
    }
}
