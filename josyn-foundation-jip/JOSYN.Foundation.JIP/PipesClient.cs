using JOSYN.Foundation.ResultPattern;
using System.IO.Pipes;
using System.Text;

namespace JOSYN.Foundation.JIP;

/// <inheritdoc cref="IPipesClient"/>
public sealed class PipesClient : IPipesClient
{
    /// <inheritdoc /> 
    public static async Task<Result<ClientPipes>> ConnectAsync(Guid sessionKey)
    {
        var (requestPipeName, responsePipeName) = PipesProtocol.DerivePipeNamesFromSessionKey(sessionKey.ToString());
        return await ConnectAsync(requestPipeName, responsePipeName);
    }

    /// <inheritdoc /> 
    public static async Task<Result<string>> SendRequestAsync(string request, ClientPipes pipes)
    {
        var result = await SendRequestAsync(Encoding.UTF8.GetBytes(request), pipes);
        if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
        return Encoding.UTF8.GetString(result.Value);
    }

    /// <inheritdoc /> 
    public static async Task<Result<byte[]>> SendRequestAsync(byte[] requestBytes, ClientPipes pipes)
    {
        if (!pipes.TrySetBusy())
            return Result.Error(IPipesProtocol.MagicBusyToken);

        try
        {
            // Write request: explicit length prefix then raw bytes (no BinaryWriter.Write(byte[])
            // which would emit a second length prefix). Mirrors BinaryReader.ReadBytes() on the
            // server side.
            await using var writer = new BinaryWriter(pipes.RequestPipe, Encoding.UTF8, leaveOpen: true);
            writer.Write(requestBytes.Length);
            writer.Write(requestBytes, 0, requestBytes.Length);
            await pipes.RequestPipe.FlushAsync();

            // Read response: BinaryReader mirrors the explicit Write(int)/Write(byte[],0,n) the
            // server now uses, giving a single consistent framing contract on both sides.
            using var reader = new BinaryReader(pipes.ResponsePipe, Encoding.UTF8, leaveOpen: true);
            var responseLength = reader.ReadInt32();
            var responseBytes  = reader.ReadBytes(responseLength);
            
            var responseStr = Encoding.UTF8.GetString(responseBytes);
            if (responseStr.StartsWith(IPipesProtocol.MagicErrorToken))
                return Result.Error(responseStr[IPipesProtocol.MagicErrorToken.Length..]);

            return responseBytes;
        }
        catch (Exception ex) { return ex; }
        
        finally { pipes.ClearBusy(); }
    }

    /// <inheritdoc /> 
    public static async Task<Result> DisconnectAsync(ClientPipes pipes, bool sendShutdownRequest = false)
    {
        try
        {
            if (sendShutdownRequest)
            {
                var shutdownRequest = Encoding.UTF8.GetBytes(IPipesProtocol.MagicShutdownToken);
                await SendRequestAsync(shutdownRequest, pipes);
            }
            await pipes.RequestPipe.FlushAsync();
            await pipes.RequestPipe.DisposeAsync();
            await pipes.ResponsePipe.DisposeAsync();
            return Result.Success;
        }
        catch (Exception ex) { return ex; }
    }

    #region private

    private static async Task<Result<ClientPipes>> ConnectAsync(string requestPipeName, string responsePipeName)
    {
        try
        {
            const int maxAttempts = 5;
            var delay = TimeSpan.FromMilliseconds(300);

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    var reqPipe = new NamedPipeClientStream(".", requestPipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                    var resPipe = new NamedPipeClientStream(".", responsePipeName, PipeDirection.In, PipeOptions.Asynchronous);
                    await reqPipe.ConnectAsync(2_000);
                    await resPipe.ConnectAsync(2_000);
                    return new ClientPipes { RequestPipe = reqPipe, ResponsePipe = resPipe };
                }
                catch (Exception ex) when (ex is TimeoutException or IOException)
                {
                    if (attempt < maxAttempts)
                    {
                        await Task.Delay(delay);
                        delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 1.5, 2_000));
                    }
                    else
                        return Result.Error($"Failed to connect to pipes <{requestPipeName}> and/or <{responsePipeName}>", ex);
                }
            }
            return Result.Error($"Failed to connect to pipes <{requestPipeName}> and/or <{responsePipeName}>");
        }
        catch (Exception ex) { return ex; }
    }

    #endregion
}