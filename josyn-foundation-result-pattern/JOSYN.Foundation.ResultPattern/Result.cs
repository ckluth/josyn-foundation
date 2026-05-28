using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130


/// <summary>
/// Represents the result of a void operation. Either successful or carrying an error message,
/// an optional exception, and a propagation chain.
/// </summary>
public sealed record Result : IResult<Result>
{
    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    public bool Succeeded => ErrorMessage == null;
    
    /// <inheritdoc />
    public string? ErrorMessage { get; init; }
    
    /// <inheritdoc /> 
    public Exception? Exception { get; init; }
    private Result(string? error = null, Exception? exception = null)
    {
        ErrorMessage = error;
        Exception = exception;
    }
    /// <inheritdoc />
    public static Result Success => default(ResultSuccess);

    /// <inheritdoc />
    public IReadOnlyList<CallerInfo> Callers { get; init; } = [];

    /// <inheritdoc />
    public static implicit operator Result(ResultSuccess _) => new();

    /// <inheritdoc />
    public static implicit operator Result(Exception exception)
    {
        var frame = new StackFrame(1, needFileInfo: true);
        var method = frame.GetMethod();
        var caller = new CallerInfo
        {
            MethodName = method?.Name ?? "",
            ClassName = method?.DeclaringType?.Name ?? "",
            FilePath = frame.GetFileName() ?? "",
            LineNumber = frame.GetFileLineNumber(),
        };
        return new Result(ResultHelper.FormatExceptionMessage(exception), exception) with { Callers = [caller] };
    }

    /// <inheritdoc />
    public static implicit operator Result(Error error)
    {
        var frame = new StackFrame(1, needFileInfo: true);
        var method = frame.GetMethod();
        var caller = new CallerInfo
        {
            MethodName = method?.Name ?? "",
            ClassName = method?.DeclaringType?.Name ?? "",
            FilePath = frame.GetFileName() ?? "",
            LineNumber = frame.GetFileLineNumber(),
        };
        return new Result(error.ErrorMessage, error.Exception) with { Callers = [caller] };
    }

    /// <inheritdoc />
    public static Result Fail(string error, Exception? exception = null, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber, new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "");
        return new Result(error, exception) with { Callers = [caller] };
    }

    /// <inheritdoc />
    public static Result Fail(Exception exception, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        var className = new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "";
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber , className);
        return new Result(ResultHelper.FormatExceptionMessage(exception), exception) with { Callers = [caller] };
    }

    /// <inheritdoc />
    public static Error Error(string error, Exception? exception = null) => new(error, exception);

    /// <inheritdoc />
    public static Result Propagate(Result result, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        Debug.Assert(!result.Succeeded, "Propagate() wurde auf einem succeeded Result aufgerufen.");
        if (result.Succeeded) return result;
        var className = new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "";
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber, className);
        return result with { Callers = [.. result.Callers, caller] };
    }

    /// <inheritdoc />
    public string CallStackAsString => ResultHelper.CallStackToString(Callers);

    /// <inheritdoc />
    public Result<TValue> ToResult<TValue>()
    {
        if (Succeeded) throw new InvalidOperationException("ToResult<T>() wurde auf einem succeeded Result aufgerufen.");
        return Result<TValue>.FailSilent(ErrorMessage!, Exception) with { Callers = Callers };
    }

    /// <summary>Creates a failed result without caller information. Used internally by the ToResult() overloads.</summary>
    internal static Result FailSilent(string error, Exception? exception = null) => new(error, exception);

    /// <summary>
    /// Returns a human-readable representation for debugging and logging.
    /// <list type="bullet">
    ///   <item><description>Success: <c>[Erfolgreich]</c></description></item>
    ///   <item><description>Failure: status line, propagation chain, and exception if present.</description></item>
    /// </list>
    /// </summary>
    public override string ToString() => ResultHelper.FormatResult(Succeeded, ErrorMessage, Callers, Exception);
}
