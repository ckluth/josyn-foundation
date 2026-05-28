using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.ResultPattern;
#pragma warning restore IDE0130

/// <summary>
/// Contract for a typed result. Implemented by <see cref="Result{TValue}"/>.
/// </summary>
public interface IResult<TSelf, TValue> where TSelf : IResult<TSelf, TValue>
{
    /// <summary>
    /// <see langword="true"/> if the operation succeeded and <see cref="Value"/> is set.
    /// When <see langword="false"/>, <see cref="ErrorMessage"/> is guaranteed to be non-null.
    /// </summary>
    [MemberNotNullWhen(false, nameof(ErrorMessage))]
    [MemberNotNullWhen(true, nameof(Value))]
    bool Succeeded { get; }

    /// <summary>
    /// The result value. <see langword="null"/> or the default value when <see cref="Succeeded"/> is <see langword="false"/>.
    /// Always check <see cref="Succeeded"/> before accessing.
    /// </summary>
    TValue? Value { get; }

    /// <summary>
    /// The error message. Set only when <see cref="Succeeded"/> is <see langword="false"/>.
    /// </summary>
    string? ErrorMessage { get; }

    /// <summary>
    /// The originating exception, if any.
    /// </summary>
    Exception? Exception { get; }

    /// <summary>
    /// The call chain built up by <see cref="Propagate"/>. Empty on a freshly created failure.
    /// </summary>
    IReadOnlyList<CallerInfo> Callers { get; }

    /// <summary>
    /// Human-readable representation of <see cref="Callers"/> for logging or display.
    /// </summary>
    string CallStackAsString { get; }

    /// <summary>
    /// Creates a successful result containing <paramref name="value"/>.
    /// Prefer the implicit conversion from <typeparamref name="TValue"/> in return statements.
    /// </summary>
    static abstract TSelf Success(TValue value);

    /// <summary>
    /// Strips the value and returns a plain <see cref="Result"/>. The error and call chain are preserved.
    /// </summary>
    Result ToResult();

    /// <summary>
    /// Reinterprets this failure as <see cref="Result{TOther}"/>.
    /// Call only on a failed result — calling on a successful result produces a silent failure.
    /// </summary>
    Result<TOther> ToResult<TOther>();

    /// <summary>
    /// Creates a failed result with an error message.
    /// Optionally attaches an <paramref name="exception"/>.
    /// </summary>
    static abstract TSelf Fail(
        string error,
        Exception? exception = null,
        [CallerMemberName] string internal_ignore_methodName = "",
        [CallerFilePath] string internal_ignore_filePath = "",
        [CallerLineNumber] int internal_ignore_lineNumber = 0);

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    static abstract TSelf Fail(
        Exception exception,
        [CallerMemberName] string internal_ignore_methodName = "",
        [CallerFilePath] string internal_ignore_filePath = "",
        [CallerLineNumber] int internal_ignore_lineNumber = 0);

    /// <summary>
    /// Appends the current caller to the propagation chain and returns the failure unchanged.
    /// Always call immediately after <c>if (!result.Succeeded)</c>.
    /// </summary>
    static abstract TSelf Propagate(
        TSelf result,
        [CallerMemberName] string internal_ignore_callermembername = "",
        [CallerFilePath] string internal_ignore_callerfilepath = "",
        [CallerLineNumber] int internal_ignore_callerlinenumber = 0);

    /// <summary>
    /// Enables <c>return myValue;</c> in methods that return <see cref="Result{TValue}"/>.
    /// </summary>
    static abstract implicit operator TSelf(TValue value);

    /// <summary>
    /// Use in catch blocks: <c>catch (Exception ex) { return ex; }</c>
    /// </summary>
    static abstract implicit operator TSelf(Exception exception);

    /// <summary>
    /// Enables returning an <see cref="Error"/> value directly from a method.
    /// </summary>
    static abstract implicit operator TSelf(Error error);
}
