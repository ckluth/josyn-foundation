using System.Diagnostics.CodeAnalysis;

namespace JOSYN.Foundation.JIP;

/// <summary>
/// Contract definition for an outgoing JIP response.
/// Describes only the data shape (wire format) — no serialization logic.
/// </summary>
public interface IResponse
{
    /// <summary>
    /// <see langword="true"/> on success. When <see langword="false"/>,
    /// <see cref="Error"/> is guaranteed to be set.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Error))]
    bool Succeeded { get; init; }

    /// <summary>
    /// Error message; set only when <see cref="Succeeded"/> is <see langword="false"/>.
    /// </summary>
    string? Error { get; init; }

    /// <summary>
    /// Optional payload string. The application layer is responsible for interpretation and
    /// deserialization.
    /// </summary>
    string? Data { get; init; }
}