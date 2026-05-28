using System.Text.Json;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Foundation.JIP;

/// <summary>
/// Implementation of the JIP convention layer.
/// Mediates between the wire format (<see cref="Request"/>, <see cref="Response"/>)
/// and the implementation layer (<see cref="Result{TValue}"/> with <c>string?</c>).
/// </summary>
public sealed class JipProtocol : IJipProtocol
{
    // -------------------------------------------------------------------------
    // Parsing
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public static Result<Request> ParseRequest(string raw)
    {
        try
        {
            var request = JsonSerializer.Deserialize<Request>(raw);
            if (request is null)
                return Result<Request>.Fail("Anfrage konnte nicht deserialisiert werden.");

            return request;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <inheritdoc/>
    public static Result<Response> ParseResponse(string raw)
    {
        try
        {
            var response = JsonSerializer.Deserialize<Response>(raw);
            if (response is null)
                return Result<Response>.Fail("Antwort konnte nicht deserialisiert werden.");

            return response;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    // -------------------------------------------------------------------------
    // Server-Seite: Result<string?> → Response
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public static Response ToResponse(Result<string?> result)
    {
        if (result.Succeeded)
            return new Response { Succeeded = true, Data = result.Value };

        return new Response
        {
            Succeeded = false,
            Error     = result.ErrorMessage,
        };
    }

    // -------------------------------------------------------------------------
    // Client-Seite: Response → Result<string?>
    // -------------------------------------------------------------------------

    /// <inheritdoc/>
    public static Result<string?> ToResult(Response response) =>
        response.Succeeded
            ? Result<string?>.Success(response.Data)
            : Result<string?>.Fail(response.Error);
}

