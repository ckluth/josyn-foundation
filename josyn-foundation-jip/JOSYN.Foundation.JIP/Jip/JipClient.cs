using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <inheritdoc cref="IJipClient"/>
public static class JipClient
{
    /// <inheritdoc cref="IJipClient.SendAsync"/>
    public static async Task<Result<string?>> SendAsync(ClientPipes pipes, string what, string? data = null)
    {
        var request = new Request { What = what, Data = data };
        var getRaw  = await PipesClient.SendRequestAsync(request.ToString(), pipes);
        if (!getRaw.Succeeded) return Result<string?>.Propagate(getRaw.ToResult<string?>());
        var parseResponse = JipProtocol.ParseResponse(getRaw.Value);
        if (!parseResponse.Succeeded) return Result<string?>.Propagate(parseResponse.ToResult<string?>());
        return JipProtocol.ToResult(parseResponse.Value);
    }
}

