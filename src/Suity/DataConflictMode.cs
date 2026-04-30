namespace Suity;

/// <summary>
/// Specifies how data conflicts should be resolved.
/// </summary>
public enum DataConflictMode
{
    Default = 0,
    Override = 1,
    Ignore = 2,
    Throw = 3,
}