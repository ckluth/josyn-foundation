using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Contract definition for the JIP convention client.
/// Encapsulates the complete send/receive pipeline: request construction, transport,
/// response parsing, and conversion to <see cref="Result{TValue}"/> with <c>string?</c>.
/// </summary>
public interface IJipClient
{
    /// <summary>
    /// Sends a request and returns the result as a <see cref="Result{TValue}"/> with an
    /// optional string payload.
    /// </summary>
    /// <param name="pipes">Connection to the server.</param>
    /// <param name="what">Identifier of the function to invoke.</param>
    /// <param name="data">Optional payload string.</param>
    /// <returns>
    /// Response payload on success; failure if the transport, parsing, or
    /// server-side processing fails.
    /// </returns>
    static abstract Task<Result<string?>> SendAsync(ClientPipes pipes, string what, string? data = null);
}
