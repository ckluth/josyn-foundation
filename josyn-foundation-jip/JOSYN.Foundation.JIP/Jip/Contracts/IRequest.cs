namespace JOSYN.Foundation.JIP;

/// <summary>
/// Contract definition for an incoming JIP request.
/// Describes only the data shape (wire format) — no serialization logic.
/// </summary>
public interface IRequest
{
    /// <summary>
    /// Identifier of the function to invoke (e.g. "GET-CONFIG").
    /// </summary>
    string What { get; init; }

    /// <summary>
    /// Optional payload string. The application layer is responsible for interpretation and
    /// serialization.
    /// </summary>
    string? Data { get; init; }
}