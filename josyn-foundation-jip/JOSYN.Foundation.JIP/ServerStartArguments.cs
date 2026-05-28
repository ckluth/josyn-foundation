using System.Diagnostics.CodeAnalysis;

namespace JOSYN.Foundation.JIP;

/// <inheritdoc cref="IServerStartArguments"/>
public sealed record ServerStartArguments: IServerStartArguments
{
    /// <inheritdoc/>
    public Guid SessionKey { get; init; } = Guid.NewGuid();

    /// <inheritdoc/>
    public string? ClientExePath { get; init; }

    /// <inheritdoc/>
    public Func<string, Task<string>>? HandleStringRequest { get; init; }

    /// <inheritdoc/>
    public Func<byte[], Task<byte[]>>? HandleRawRequest { get; init; }

    /// <inheritdoc/>
    [MemberNotNullWhen(true, nameof(HandleStringRequest))]
    [MemberNotNullWhen(false, nameof(HandleRawRequest))]
    public bool HasStringRequestHandler => HandleStringRequest != null;

    /// <inheritdoc/>
    public TimeSpan ConnectionTimeout { get; init; } = new TimeSpan(0, 0, 10);

    /// <inheritdoc/>
    public required Func<string, Exception, Task> HandleErrorNotification { get; init; }

    /// <inheritdoc/>
    public Func<Task<bool>>? IsCancellationRequested { get; init; }      
}