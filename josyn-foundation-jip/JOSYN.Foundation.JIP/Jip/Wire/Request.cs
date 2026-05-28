using System.Text.Json;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Incoming JIP request (wire format). Serialized via <see cref="JipProtocol"/>.
/// </summary>
public sealed record Request : IRequest
{
    /// <inheritdoc/>
    public required string What { get; init; }

    /// <inheritdoc/>
    public string? Data { get; init; }

    /// <inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(this);
}