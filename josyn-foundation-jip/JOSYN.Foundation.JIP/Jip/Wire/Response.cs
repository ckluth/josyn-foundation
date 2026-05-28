using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Outgoing JIP response (wire format). Serialized via <see cref="JipProtocol"/>.
/// </summary>
public sealed record Response : IResponse
{
    /// <inheritdoc/>
    [MemberNotNullWhen(false, nameof(Error))]
    public bool Succeeded { get; init; }

    /// <inheritdoc/>
    public string? Error { get; init; }

    /// <inheritdoc/>
    public string? Data { get; init; }

    /// <inheritdoc/>
    public override string ToString() => JsonSerializer.Serialize(this);
}