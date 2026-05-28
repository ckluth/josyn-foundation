using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Contract definition for the JIP transport-layer server.
/// Sets up named pipes, waits for a client connection, and processes requests
/// through a configurable handler.
/// </summary>
public interface IPipesServer
{
    /// <summary>
    /// Starts the server lifecycle: sets up request and response pipes,
    /// waits for a client connection, and processes requests sequentially
    /// until cancellation or error.
    /// </summary>
    /// <param name="args">
    /// Start configuration: session key, request handler, timeout, and optional
    /// path to the client executable.
    /// </param>
    /// <param name="reConnect">
    /// If <see langword="true"/>, the server restarts automatically after a clean
    /// disconnect. Cannot be combined with
    /// <see cref="ServerStartArguments.ClientExePath"/>.
    /// </param>
    /// <param name="onReconnect">
    /// Optional callback invoked before each reconnect attempt —
    /// for example, for logging or status notifications.
    /// </param>
    /// <returns>
    /// Successful when the server shut down cleanly;
    /// failure on configuration problems or unrecoverable transport errors.
    /// </returns>
    static abstract Task<Result> RunAsync(ServerStartArguments args, bool reConnect = false, Action? onReconnect= null);
}