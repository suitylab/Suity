namespace Suity.Editor;

/// <summary>
/// Output information
/// </summary>
public record OutputInfo
{
    public string OutputPath { get; init; }
    public string IntermediateOutputPath { get; init; }
    public string AssemblyName { get; init; }
    public string OutputType { get; init; }
}