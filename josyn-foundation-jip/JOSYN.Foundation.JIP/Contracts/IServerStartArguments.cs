namespace JOSYN.Foundation.JIP;

/// <summary>
/// Start configuration for <see cref="PipesServer"/>.
/// Encapsulates handler, session key, timeout, and optional client path
/// as an immutable record.
/// </summary>
public interface IServerStartArguments
{
    /// <summary>
    /// Unique session key from which the pipe names are derived.
    /// Default: an automatically generated <see cref="Guid"/>.
    /// </summary>
    Guid SessionKey { get; init; }

    /// <summary>
    /// Optional path to the client executable that the server starts on launch.
    /// If <see langword="null"/>, the client must have been started independently.
    /// </summary>
    string? ClientExePath { get; init; }

    /// <summary>
    /// String request handler (UTF-8). Exactly one of
    /// <see cref="HandleStringRequest"/> or <see cref="HandleRawRequest"/> must be set.
    /// </summary>
    Func<string, Task<string>>? HandleStringRequest { get; init; }

    /// <summary>
    /// Binary request handler. Exactly one of
    /// <see cref="HandleRawRequest"/> or <see cref="HandleStringRequest"/> must be set.
    /// </summary>
    Func<byte[], Task<byte[]>>? HandleRawRequest { get; init; }
    
    /// <summary>
    /// <see langword="true"/> if <see cref="HandleStringRequest"/> is set;
    /// <see langword="false"/> if <see cref="HandleRawRequest"/> is used.
    /// </summary>
    bool HasStringRequestHandler { get; }

    /// <summary>
    /// Maximum wait time for an incoming client connection.
    /// Default: 10 seconds.
    /// </summary>
    TimeSpan ConnectionTimeout { get; init; }

    /// <summary>
    /// Callback for non-critical errors in the request loop.
    /// Receives the request string and the thrown exception for error logging.
    /// </summary>
    Func<string, Exception, Task> HandleErrorNotification { get; init; }

    /// <summary>
    /// Optional asynchronous cancellation callback. Polled at the polling interval (100 ms).
    /// Returns <see langword="true"/> when the server should stop.
    /// If <see langword="null"/>, the server runs until the connection ends.
    /// </summary>
    Func<Task<bool>>? IsCancellationRequested { get; init; }

}