namespace Suity.CLI;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ConsoleCommandKeyAttribute(string key) : Attribute
{
    public string Key { get; } = key;
}
