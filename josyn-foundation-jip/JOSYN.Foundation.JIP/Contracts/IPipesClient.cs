using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Contract definition for the JIP transport-layer client.
/// Connects to a running server and sends requests
/// over the length-prefix protocol (<c>int32</c> + bytes, little-endian).
/// </summary>
public interface IPipesClient
{
    /// <summary>
    /// Connects to the running JIP server via two named pipes
    /// derived from the <paramref name="sessionKey"/>
    /// (request pipe and response pipe). Retries the connection with
    /// exponential backoff up to five times.
    /// </summary>
    /// <param name="sessionKey">
    /// Unique session key; must match the one used by the server.
    /// </param>
    /// <returns>
    /// <see cref="ClientPipes"/> handle on success;
    /// failure if the server was not reachable within all retry attempts.
    /// </returns>
    static abstract Task<Result<ClientPipes>> ConnectAsync(Guid sessionKey);

    /// <summary>
    /// Sends a binary request to the server and returns the raw binary response.
    /// Guards against concurrent calls via a busy guard (single-in-flight protocol).
    /// </summary>
    /// <param name="requestBytes">Raw request bytes.</param>
    /// <param name="pipes">Connection handle from <see cref="ConnectAsync"/>.</param>
    /// <returns>
    /// Binary response bytes on success;
    /// failure if the server returns an error magic token, the client is already
    /// busy, or the transport fails.
    /// </returns>
    static abstract Task<Result<byte[]>> SendRequestAsync(byte[] requestBytes, ClientPipes pipes);

    /// <summary>
    /// Sends a UTF-8-encoded string request to the server and returns the
    /// response as a string.
    /// </summary>
    /// <param name="request">Request string (UTF-8).</param>
    /// <param name="pipes">Connection handle from <see cref="ConnectAsync"/>.</param>
    /// <returns>
    /// Response string on success;
    /// failure if the server returns an error magic token or the transport fails.
    /// </returns>
    static abstract Task<Result<string>> SendRequestAsync(string request, ClientPipes pipes);

    /// <summary>
    /// Disconnects from the server and releases the pipe resources.
    /// Optionally sends a shutdown magic token before closing the pipes.
    /// </summary>
    /// <param name="pipes">Connection handle to disconnect.</param>
    /// <param name="sendShutdownRequest">
    /// If <see langword="true"/>, sends the <see cref="IPipesProtocol.MagicShutdownToken"/>
    /// before disconnecting — signals an orderly shutdown to the server.
    /// </param>
    /// <returns>Successful when the connection was cleanly terminated.</returns>
    static abstract Task<Result> DisconnectAsync(ClientPipes pipes, bool sendShutdownRequest = false);
}