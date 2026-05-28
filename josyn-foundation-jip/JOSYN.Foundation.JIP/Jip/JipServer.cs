#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

using JOSYN.Foundation.ResultPattern;

/// <inheritdoc cref="IJipServer"/>
public static class JipServer
{
    /// <inheritdoc cref="IJipServer.WrapHandler(Func{Request, Result{string?}})"/>
    public static Func<string, Task<string>> WrapHandler(Func<Request, Result<string?>> handler)
    {
        return requestStr =>
        {
            var parseResult = JipProtocol.ParseRequest(requestStr);
            var response = parseResult.Succeeded
                ? JipProtocol.ToResponse(handler(parseResult.Value))
                : JipProtocol.ToResponse(Result<string?>.Fail($"Ungültige JIP-Anfrage: {parseResult.ErrorMessage}"));
            return Task.FromResult(response.ToString());
        };
    }

    /// <inheritdoc cref="IJipServer.WrapHandler(Func{Request, Task{Result{string?}}})"/>
    public static Func<string, Task<string>> WrapHandler(Func<Request, Task<Result<string?>>> handler)
    {
        return async requestStr =>
        {
            var parseResult = JipProtocol.ParseRequest(requestStr);
            var response = parseResult.Succeeded
                ? JipProtocol.ToResponse(await handler(parseResult.Value))
                : JipProtocol.ToResponse(Result<string?>.Fail($"Ungültige JIP-Anfrage: {parseResult.ErrorMessage}"));
            return response.ToString();
        };
    }
}

