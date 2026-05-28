using System.Diagnostics;
using System.Text;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

internal static class ResultHelper
{
    internal static CallerInfo CreateCallerInfo(string methodName, string filePath, int lineNumber, string className = "")
    {
        return new CallerInfo
        {
            MethodName = methodName,
            ClassName = className,
            FilePath = filePath,
            LineNumber = lineNumber,
        };
    }
    
    internal static string CallStackToString(IReadOnlyList<CallerInfo> callers) =>
        callers.Count == 0
            ? "(kein Callstack)"
            : string.Join("\n", callers.Select(c => $"  at {c}"));

    // "Ausnahmefehler: " prefix is intentionally German — matches the project's runtime error message convention.
    internal static string FormatExceptionMessage(Exception exception) => $"Ausnahmefehler: {exception.Message}";

    // valueLabel: optional string representation of the result value, used by Result<TValue>.
    internal static string FormatResult(bool succeeded, string? errorMessage, IReadOnlyList<CallerInfo> callers, Exception? exception, string? valueLabel = null)
    {
        if (succeeded)
            return valueLabel is null ? "[Erfolgreich]" : $"[Erfolgreich] {valueLabel}";

        var sb = new StringBuilder();
        sb.Append("[Fehlgeschlagen] ").Append(errorMessage);
        if (callers.Count > 0)
        {
            sb.AppendLine();
            sb.Append(CallStackToString(callers));
        }
        if (exception is not null)
        {
            sb.AppendLine();
            sb.Append("Ausnahme: ").Append(exception);
        }
        return sb.ToString();
    }
}
