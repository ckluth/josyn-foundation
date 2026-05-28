using System.IO.Pipes;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <inheritdoc cref="IClientPipes"/>
public sealed class ClientPipes : IClientPipes
{
    /// <inheritdoc/>
    public required NamedPipeClientStream RequestPipe { get; init; }

    /// <inheritdoc/>
    public required NamedPipeClientStream ResponsePipe { get; init; }
    
    private int isBusy;

    /// <summary>Atomically claims the busy-slot. Returns true if the caller acquired it, false if already busy.</summary>
    internal bool TrySetBusy() => Interlocked.CompareExchange(ref isBusy, 1, 0) == 0;

    internal void ClearBusy() => Interlocked.Exchange(ref isBusy, 0);
}