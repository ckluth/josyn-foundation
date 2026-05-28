using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Foundation.JIP;

/// <summary>
/// Contract definition for the JIP convention layer.
/// Separates the wire format (<see cref="Request"/>, <see cref="Response"/>) from the
/// implementation layer (<see cref="Result{TValue}"/> with <c>string?</c>).
/// </summary>
public interface IJipProtocol
{
    // -------------------------------------------------------------------------
    // Parsing (Exceptions → Result)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Deserializes a raw JSON string into a <see cref="Request"/> object.
    /// </summary>
    static abstract Result<Request> ParseRequest(string raw);

    /// <summary>
    /// Deserializes a raw JSON string into a <see cref="Response"/> object.
    /// </summary>
    static abstract Result<Response> ParseResponse(string raw);

    // -------------------------------------------------------------------------
    // Server-Seite: Result<string?> → Response
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a <see cref="Response"/> from a <see cref="Result{TValue}"/> with an
    /// optional string payload. The application layer is responsible for serializing the
    /// value into this string.
    /// </summary>
    static abstract Response ToResponse(Result<string?> result);

    // -------------------------------------------------------------------------
    // Client-Seite: Response → Result<string?>
    // -------------------------------------------------------------------------

    /// <summary>
    /// Converts a <see cref="Response"/> to a <see cref="Result{TValue}"/> with an
    /// optional string payload. The application layer is responsible for deserializing
    /// the value.
    /// </summary>
    static abstract Result<string?> ToResult(Response response);
}
