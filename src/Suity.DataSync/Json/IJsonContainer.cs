namespace ComputerBeacon.Json;

internal interface IJsonContainer
{
    bool IsArray { get; }

    void InternalAdd(string key, object value);
}