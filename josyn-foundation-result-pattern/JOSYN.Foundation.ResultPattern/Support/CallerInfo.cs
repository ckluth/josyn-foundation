#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Represents a single entry in the propagation chain of a result.
/// Populated automatically — manual construction is not intended.
/// </summary>
public sealed record CallerInfo
{
    /// <summary>
    /// Method name.
    /// </summary>
    public string MethodName { get; init; } = "";

    /// <summary>
    /// Name of the declaring type.
    /// </summary>
    public string ClassName { get; init; } = "";

    /// <summary>
    /// Path to the source file. Empty if no PDB file is available.
    /// </summary>
    public string FilePath { get; init; } = "";

    /// <summary>
    /// Line number. Zero if no PDB file is available.
    /// </summary>
    public int LineNumber { get; init; }

    /// <inheritdoc />
    public override string ToString()
    {
        if (string.IsNullOrEmpty(FilePath) || LineNumber == 0) return $"{ClassName}.{MethodName}()";
        return $"{ClassName}.{MethodName}() in {Path.GetFileName(FilePath)}:{LineNumber}";
    }
}
