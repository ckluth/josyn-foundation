using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Contract definition for the JIP transport-layer server.
/// Sets up named pipes, waits for a client connection, and processes requests
/// through a configurable handler.
/// <para>
/// Process lifecycle is intentionally not part of this contract.
/// Callers are responsible for launching the client executable before calling
/// <see cref="RunAsync"/>, using <see cref="StartClientProcess"/>.
/// </para>
/// </summary>
public interface IPipesServer
{
    /// <summary>
    /// Launches the client executable with the JIP session key as a CLI argument
    /// and returns the process ID of the started process.
    /// <para>
    /// Call this before <see cref="RunAsync"/> so that process launch failures
    /// can be handled explicitly in the caller before the pipe server is started.
    /// </para>
    /// </summary>
    /// <param name="exePath">Full path to the client executable.</param>
    /// <param name="sessionKey">The session key used to derive the pipe names.</param>
    /// <returns>
    /// The process ID of the started process on success;
    /// failure if the executable is not found or the process could not be started.
    /// </returns>
    static abstract Result<int> StartClientProcess(string exePath, Guid sessionKey);

    /// <summary>
    /// Starts the server lifecycle: sets up request and response pipes,
    /// waits for a client connection, and processes requests sequentially
    /// until cancellation or error.
    /// <para>
    /// The client process must already be running before this is called.
    /// Use <see cref="StartClientProcess"/> to launch it.
    /// </para>
    /// </summary>
    /// <param name="args">Start configuration: session key, request handler, and timeout.</param>
    /// <param name="reConnect">
    /// If <see langword="true"/>, the server restarts automatically after a clean disconnect.
    /// </param>
    /// <param name="onReconnect">
    /// Optional callback invoked before each reconnect attempt —
    /// for example, for logging or status notifications.
    /// </param>
    /// <returns>
    /// Successful when the server shut down cleanly;
    /// failure on configuration problems or unrecoverable transport errors.
    /// </returns>
    static abstract Task<Result> RunAsync(IServerStartArguments args, bool reConnect = false, Action? onReconnect = null);
}