using System.IO.Pipes;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Contract definition for the server-side named pipe handle.
/// Encapsulates the two pipes (request and response) of an active JIP server connection.
/// </summary>
public interface IServerPipes
{
    /// <summary>Named pipe through which the server receives requests from the client.</summary>
    NamedPipeServerStream RequestPipe { get; init; }

    /// <summary>Named pipe through which the server sends responses to the client.</summary>
    NamedPipeServerStream ResponsePipe { get; init; }
}
