namespace Suity.Editor;

public interface ICliArguments
{
    string CommandKey { get; }
    string[] RawArgs { get; }
    IReadOnlyDictionary<string, string> Options { get; }
    int Count { get; }

    string? GetOption(string key, string? defaultValue = null);
    bool HasFlag(string flag);
    string? this[int index] { get; }
}
