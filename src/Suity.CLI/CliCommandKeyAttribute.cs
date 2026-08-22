namespace Suity.Editor;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CliCommandKeyAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}
