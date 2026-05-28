using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Contract definition for the JIP wire protocol.
/// Contains protocol constants, CLI helper methods, and pipe name derivation.
/// </summary>
public interface IPipesProtocol
{
    /// <summary>
    /// Protocol identifier: prefix for CLI arguments and session key handoff.
    /// Value: <c>"JOSYN-IPC"</c>.
    /// </summary>
    public const string MagicToken = "JOSYN-IPC";

    /// <summary>
    /// Prefix for error responses from the server.
    /// Value: <c>"JOSYN-IPC-ERROR"</c>.
    /// </summary>
    public const string MagicErrorToken = $"{MagicToken}-ERROR";

    /// <summary>
    /// Response value when the client already has a pending request
    /// (single-in-flight protection via busy guard).
    /// Value: <c>"JOSYN-IPC-ERROR-BUSY"</c>.
    /// </summary>
    public const string MagicBusyToken = $"{MagicErrorToken}-BUSY";

    /// <summary>
    /// Request and acknowledgement token for an orderly server shutdown,
    /// triggered by <see cref="IPipesClient.DisconnectAsync"/>.
    /// Value: <c>"JOSYN-IPC-SHUTDOWN"</c>.
    /// </summary>
    public const string MagicShutdownToken = $"{MagicToken}-SHUTDOWN";

    /// <summary>
    /// Creates the CLI argument string that the server passes to the client process
    /// on startup to transmit the session key.
    /// </summary>
    /// <param name="sessionKey">Session key as a string.</param>
    /// <returns>CLI argument in the format <c>"JOSYN-IPC &lt;sessionKey&gt;"</c>.</returns>
    static abstract string CreateClientStartCLIArguments(string sessionKey);

    /// <summary>
    /// Parses the session key from the CLI arguments produced by
    /// <see cref="CreateClientStartCLIArguments"/>.
    /// </summary>
    /// <param name="args">Process CLI arguments (<c>args</c> from <c>Main</c>).</param>
    /// <returns>
    /// Parsed session key on success;
    /// <see cref="Guid.Empty"/> if the arguments do not match the expected format.
    /// </returns>
    static abstract Guid ParseSessionKeyCLIArguments(string[] args);

    /// <summary>
    /// Derives the request and response pipe names from the session key.
    /// </summary>
    /// <param name="sessionKey">Session key as a string.</param>
    /// <returns>
    /// Tuple containing the request pipe name (<c>"req-pipe-&lt;key&gt;"</c>)
    /// and the response pipe name (<c>"res-pipe-&lt;key&gt;"</c>).
    /// </returns>
    static abstract (string requestPipeName, string responsePipeName) DerivePipeNamesFromSessionKey(string sessionKey);
}