using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Represents the result of an operation that returns <typeparamref name="TValue"/> on success.
/// Either successful with a value, or carrying an error message, an optional exception, and a propagation chain.
/// </summary>
public sealed record Result<TValue> : IResult<Result<TValue>, TValue>
{
    /// <inheritdoc />
    [MemberNotNullWhen(true, nameof(Value))]
    public TValue? Value { get; init; }

    /// <inheritdoc />
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    [MemberNotNullWhen(true, nameof(Value))]
    public bool Succeeded => ErrorMessage == null;

    /// <inheritdoc />
    public string? ErrorMessage { get; init; }
    
    /// <inheritdoc />
    public Exception? Exception { get; init; }

    /// <inheritdoc />
    public IReadOnlyList<CallerInfo> Callers { get; init; } = [];

    private Result(TValue? value = default, string? error = null, Exception? exception = null)
    {
        Value = value;
        ErrorMessage = error;
        Exception = exception;
    }

    /// <inheritdoc />
    public static Result<TValue> Success(TValue value) => new(value);

    
    /// <inheritdoc />
    public static Result<TValue> Fail(string error, Exception? exception = null, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber, new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "");
        return new Result<TValue>(default, error, exception) with { Callers = [caller] };
    }

    /// <inheritdoc />
    public static Result<TValue> Fail(Exception exception, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        var className = new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "";
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber, className);
        return new Result<TValue>(default, ResultHelper.FormatExceptionMessage(exception), exception) with { Callers = [caller] };
    }

    /// <inheritdoc />
    public static implicit operator Result<TValue>(TValue value) => new(value);

    /// <inheritdoc />
    public static implicit operator Result<TValue>(Exception exception)
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
        return new Result<TValue>(default, ResultHelper.FormatExceptionMessage(exception), exception) with { Callers = [caller] };
    }

    /// <inheritdoc />
    public static implicit operator Result<TValue>(Error error)
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
        return new Result<TValue>(default, error.ErrorMessage, error.Exception) with { Callers = [caller] };
    }

    /// <inheritdoc />
    public static Result<TValue> Propagate(Result<TValue> result, [CallerMemberName] string internal_ignore_callermembername = "", [CallerFilePath] string internal_ignore_callerfilepath = "", [CallerLineNumber] int internal_ignore_callerlinenumber = 0)
    {
        Debug.Assert(!result.Succeeded, "Propagate() wurde auf einem succeeded Result aufgerufen.");
        if (result.Succeeded) return result;
        var className = new StackFrame(1).GetMethod()?.DeclaringType?.Name ?? "";
        var caller = ResultHelper.CreateCallerInfo(internal_ignore_callermembername, internal_ignore_callerfilepath, internal_ignore_callerlinenumber, className);
        return result with { Callers = [.. result.Callers, caller] };
    }

    /// <inheritdoc />
    public Result ToResult()
    {
        if (Succeeded) return Result.Success;
        return Result.FailSilent(ErrorMessage!, Exception) with { Callers = Callers };
    }

    /// <inheritdoc />
    public Result<TOther> ToResult<TOther>()
    {
        if (Succeeded) throw new InvalidOperationException("ToResult<T>() wurde auf einem succeeded Result aufgerufen.");
        return Result<TOther>.FailSilent(ErrorMessage!, Exception) with { Callers = Callers };
    }
    
    /// <inheritdoc />
    public string CallStackAsString => ResultHelper.CallStackToString(Callers);

    /// <summary>Creates a failed result without caller information. Used internally by the ToResult() overloads.</summary>
    internal static Result<TValue> FailSilent(string error, Exception? exception = null) => new(default, error, exception);

    /// <summary>
    /// Returns a human-readable representation for debugging and logging.
    /// <list type="bullet">
    ///   <item><description>Success: <c>[Erfolgreich] {Value}</c></description></item>
    ///   <item><description>Failure: status line, propagation chain, and exception if present.</description></item>
    /// </list>
    /// </summary>
    public override string ToString() => ResultHelper.FormatResult(Succeeded, ErrorMessage, Callers, Exception, Value?.ToString());
}
