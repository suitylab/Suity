using System;

namespace Suity.Editor;

public abstract class PluginFunctionality
{
    public string FunctionType { get; }
    public string Name { get; }

    public PluginFunctionality(string fType, string name)
    {
        FunctionType = fType;
        Name = name;
    }

    public abstract Type GetFunctionType();

    public abstract object GetFunctionObject();

    public override string ToString()
    {
        return FunctionType + " - " + Name;
    }
}

internal class PluginFunctionality<T> : PluginFunctionality
{
    public T Value { get; private set; }

    public PluginFunctionality(string fType, string name, T value)
        : base(fType, name)
    {
        Value = value;
    }

    public override Type GetFunctionType()
    {
        return typeof(T);
    }

    public override object GetFunctionObject()
    {
        return Value;
    }
}