using System.IO.Pipes;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Contract definition for the client-side named pipe handle.
/// Encapsulates the two pipes (request and response) of an active JIP client connection.
/// </summary>
public interface IClientPipes
{
    /// <summary>Named pipe through which the client sends requests to the server.</summary>
    NamedPipeClientStream RequestPipe { get; init; }

    /// <summary>Named pipe through which the client receives responses from the server.</summary>
    NamedPipeClientStream ResponsePipe { get; init; }
}
