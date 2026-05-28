using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Contract definition for the JIP convention server helper.
/// Encapsulates request parsing, response serialization, and error wrapping,
/// so that handler code works exclusively with <see cref="Result{TValue}"/> —
/// without any knowledge of the wire format.
/// </summary>
public interface IJipServer
{
    /// <summary>
    /// Wraps a synchronous request handler into the
    /// <c>Func&lt;string, Task&lt;string&gt;&gt;</c> signature expected by <see cref="PipesServer"/>.
    /// Parse failures are returned as error responses.
    /// </summary>
    /// <param name="handler">
    /// Receives the parsed <see cref="Request"/> and returns a <see cref="Result{TValue}"/>
    /// with an optional string payload.
    /// </param>
    static abstract Func<string, Task<string>> WrapHandler(Func<Request, Result<string?>> handler);

    /// <summary>
    /// Wraps an asynchronous request handler into the
    /// <c>Func&lt;string, Task&lt;string&gt;&gt;</c> signature expected by <see cref="PipesServer"/>.
    /// Parse failures are returned as error responses.
    /// </summary>
    /// <param name="handler">
    /// Receives the parsed <see cref="Request"/> and asynchronously returns a <see cref="Result{TValue}"/>
    /// with an optional string payload.
    /// </param>
    static abstract Func<string, Task<string>> WrapHandler(Func<Request, Task<Result<string?>>> handler);
}
