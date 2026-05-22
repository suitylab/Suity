using System;
using System.Collections.Generic;

namespace Suity.Helpers;

public abstract class GenericArrayMaker
{
    static readonly Dictionary<Type, GenericArrayMaker> _makers = [];

    public abstract object MakeArrayOrList(Type type, List<object> objs);
    public abstract object MakeArray(List<object> objs);
    public abstract object MakeList(List<object> objs);

    public static GenericArrayMaker GetMaker(Type type)
    {
        if (!_makers.TryGetValue(type, out var maker))
        {
            var makerType = typeof(GenericArrayMaker<>).MakeGenericType(type);
            maker = (GenericArrayMaker)Activator.CreateInstance(makerType);
            _makers[type] = maker;
        }
        return maker;
    }
}

public class GenericArrayMaker<T> : GenericArrayMaker
{
    public override object MakeArrayOrList(Type type, List<object> objs)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) && type.GetGenericArguments()[0] == typeof(T))
        {
            return MakeList(objs);
        }
        else if (type.IsArray && type.GetElementType() == typeof(T))
        {
            return MakeArray(objs);
        }
        else
        {
            throw new ArgumentException();
        }
    }


    public override object MakeArray(List<object> objs)
    {
        T[] array = new T[objs.Count];
        for (int i = 0; i < objs.Count; i++)
        {
            array[i] = (T)objs[i];
        }
        return array;
    }

    public override object MakeList(List<object> objs)
    {
        List<T> list = new(objs.Count);
        for (int i = 0; i < objs.Count; i++)
        {
            list.Add((T)objs[i]);
        }
        return list;
    }
}