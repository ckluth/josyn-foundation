using System.IO.Pipes;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <inheritdoc cref="IServerPipes"/>
public sealed class ServerPipes : IServerPipes
{
    /// <inheritdoc/>
    public required NamedPipeServerStream RequestPipe { get; init; }

    /// <inheritdoc/>
    public required NamedPipeServerStream ResponsePipe { get; init; }
}