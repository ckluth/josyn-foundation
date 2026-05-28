using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Foundation.JIP;


/// <inheritdoc cref="IPipesServer"/>
public sealed class PipesServer : IPipesServer
{
    /// <inheritdoc/>
    public static async Task<Result> RunAsync(ServerStartArguments args, bool reConnect = false, Action? onReconnect = null)
    {
        if (args.ClientExePath != null && reConnect)
            return Result.Fail("Reconnect wird nicht bei Client-Exe-Aufruf unterstützt.");
        
        
        Result<bool> res;
        while (true)
        {
            res = await PipesServer.RunAsyncInternal(args);
            if (res.Succeeded)
            {
                if (!reConnect)
                    break;

                var wasCancelled = res.Value;

                if (wasCancelled)
                    break;

                onReconnect?.Invoke();

            }
            else
                break;
        }
        return !res.Succeeded ? Result.Propagate(res.ToResult()) : Result.Success;
    }

    #region private

    private static async Task<Result<bool>> RunAsyncInternal(ServerStartArguments args)
    {
        if (!args.HasStringRequestHandler && args.HandleRawRequest == null)
            return Result<bool>.Fail("Kein Request-Handler konfiguriert. HandleStringRequest oder HandleRawRequest muss gesetzt sein.");

        if (args.ClientExePath != null)
        {
            var startClient = StartClientExe(args.ClientExePath, args.SessionKey.ToString());
            if (!startClient.Succeeded)
                return Result<bool>.Propagate(startClient.ToResult<bool>());
        }

        var cancellationHandle = CreatePollingCancellationToken(args.IsCancellationRequested);

        var rawRequestHandler = args.HasStringRequestHandler
            ? new RawRequestHandler { ProcessStrings = args.HandleStringRequest }.ProcessRawRequest
            : args.HandleRawRequest;

        var res = await RunAsyncInternal(
            rawRequestHandler,
            args.ConnectionTimeout,
            args.HandleErrorNotification,
            args.SessionKey.ToString(),
            cancellationHandle.Token);

        cancellationHandle.Dispose();

        if (res.Succeeded)
            return cancellationHandle.CancelledByCallback || res.Value;

        return Result<bool>.Propagate(res);
    }

    private static async Task<Result<bool>> RunAsyncInternal(
        Func<byte[], Task<byte[]>> processRequest,
        TimeSpan connectTimeout,
        Func<string, Exception, Task> onError,
        string sessionKey,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var getConnection = CreatePipes(sessionKey);

            if (!getConnection.Succeeded)
                return Result<bool>.Propagate(getConnection.ToResult<bool>());

            var conn = getConnection.Value;

            await using (conn.RequestPipe)
            await using (conn.ResponsePipe)
            {
                using var connectCts = new CancellationTokenSource(connectTimeout);
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    connectCts.Token, cancellationToken);

                try
                {
                    await Task.WhenAll(
                        conn.RequestPipe.WaitForConnectionAsync(linkedCts.Token),
                        conn.ResponsePipe.WaitForConnectionAsync(linkedCts.Token));
                }
                catch (OperationCanceledException)
                {
                    return Result.Error(cancellationToken.IsCancellationRequested
                        ? "Verbindung durch Aufrufer abgebrochen."
                        : "Timeout: kein Client verbunden.");
                }

                return await RequestLoopAsync(
                    conn.RequestPipe, conn.ResponsePipe, processRequest, onError, cancellationToken);
            }
        }
        catch (Exception ex) { return ex; }
    }

    private class RawRequestHandler
    {
        internal required Func<string, Task<string>> ProcessStrings { get; init; }

        internal async Task<byte[]> ProcessRawRequest(byte[] requestBytes)
        {
            var requestStr = Encoding.UTF8.GetString(requestBytes);
            var responseStr = await ProcessStrings(requestStr);
            return Encoding.UTF8.GetBytes(responseStr);
        }
    }

    private static CancellationHandle CreatePollingCancellationToken(
        Func<Task<bool>>? shouldCancel = null,
        int pollIntervalMs = 100)
    {
        var cts = new CancellationTokenSource();
        var handle = new CancellationHandle(cts);

        if (shouldCancel == null)
            return handle; // Token == CancellationToken.None equivalent; CancelledByCallback stays false

        // ReSharper disable once MethodSupportsCancellation
        _ = Task.Run(async () =>
        {
            while (!cts.IsCancellationRequested)
            {
                // Eine Exception in shouldCancel() würde hier diesen Task beenden, ohne dass cts.Cancel() aufgerufen wird.
                // Die Exception "verschwindet im Nirwana".
                // "Könnte man" auffangen - und propagieren, oder als CancelRequest behandeln - aber NOPE!
                // Explizite Design-Entscheidung hier:
                // Der IsCancellationRequested-Callback in der Anwendung soll lightweight und schlank implementiert sein
                // und keinen kritischen Code beinhalten! Bei Verstoß gegen dieses dokumentierte Konzept: Ein Fall von "Pech gehabt"!

                var isCancellationRequested = await shouldCancel();

                if (isCancellationRequested)
                {
                    // Flag BEFORE cancel — the awaiting code may resume immediately after
                    // CancelAsync() returns, so the flag must already be visible.
                    handle.CancelledByCallback = true;
                    await cts.CancelAsync();
                    break;
                }

                // ReSharper disable once MethodSupportsCancellation
                await Task.Delay(pollIntervalMs);
            }
        });

        return handle;
    }

    private static Result StartClientExe(string remoteExePath, string sessionKey)
    {
        if (!File.Exists(remoteExePath))
            return Result.Error($"Client-Exe not found: {remoteExePath}");

        try
        {
            var p = Process.Start(new ProcessStartInfo
            {
                FileName = remoteExePath,
                Arguments = PipesProtocol.CreateClientStartCLIArguments(sessionKey),
                UseShellExecute = true,
                CreateNoWindow = false
            });

            return (p == null)
                ? Result.Error($"Failed to start client process: {remoteExePath}")
                : Result.Success;
        }
        catch (Exception ex) { return ex; }
    }

    private static async Task<Result<bool>> RequestLoopAsync(
        NamedPipeServerStream reqPipe,
        NamedPipeServerStream resPipe,
        Func<byte[], Task<byte[]>> processRequest,
        Func<string, Exception, Task> onError,
        CancellationToken cancellationToken = default)
    {
        //-----------------------------------------------------------------------------
        // DISCLAIMER: Single-in-flight — kein Multiplexing im zentralen Request-Loop!
        //
        // Der Request-Loop verarbeitet Anfragen strikt sequenziell.
        // Schutz vor parallelen Requests liegt im Client (PipesClient.IsBusy).
        //-----------------------------------------------------------------------------

        try
        {
            // BinaryReader/BinaryWriter used consistently on both sides.
            // Note: BinaryWriter.Write(byte[]) emits a length prefix before the bytes, which
            // would create a double-prefix bug. The correct overload is Write(byte[], 0, n),
            // which writes raw bytes only — matching BinaryReader.ReadBytes(n) on the client.
            using var reader = new BinaryReader(reqPipe, Encoding.UTF8, leaveOpen: true);

            // ReSharper disable once UseAwaitUsing
            using var writer = new BinaryWriter(resPipe, Encoding.UTF8, leaveOpen: true);

            // When cancellation fires, close the pipe to unblock the synchronous ReadInt32() call below.
            // ReSharper disable once UseAwaitUsing
            using var _ = cancellationToken.Register(reqPipe.Close);

            while (reqPipe.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                byte[] requestBytes;
                try
                {
                    var messageLength = reader.ReadInt32();
                    requestBytes = reader.ReadBytes(messageLength);
                }
                catch (OperationCanceledException)
                {
                    // can't happen in the current design - but, paranoia...
                    return Result.Error("Request-Loop durch Aufrufer abgebrochen.");
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                catch (IOException) when (cancellationToken.IsCancellationRequested)
                {
                    // cancellationToken.Register() closed the pipe to unblock ReadInt32() — clean exit.
                    break;
                }

                if (Encoding.UTF8.GetString(requestBytes) == IPipesProtocol.MagicShutdownToken)
                {
                    var ack = Encoding.UTF8.GetBytes(IPipesProtocol.MagicShutdownToken);
                    writer.Write(ack.Length);
                    writer.Write(ack, 0, ack.Length);
                    writer.Flush();
                    return true; // shutdown requested by client — exit reconnect-loop
                }

                byte[] response;
                try
                {
                    response = await processRequest(requestBytes);
                }
                catch (Exception ex)
                {
                    await onError(Encoding.UTF8.GetString(requestBytes), ex);
                    var responseStr = $"{IPipesProtocol.MagicErrorToken}{ex}";
                    response = Encoding.UTF8.GetBytes(responseStr);
                }

                writer.Write(response.Length);
                writer.Write(response, 0, response.Length); // raw bytes only — no extra length prefix
                writer.Flush();
            }

            return false;
        }
        catch (Exception ex) { return ex; }
    }

    private static Result<ServerPipes> CreatePipes(string sessionKey)
    {
        var (requestPipeName, responsePipeName) = PipesProtocol.DerivePipeNamesFromSessionKey(sessionKey);
        return CreatePipes(requestPipeName, responsePipeName);
    }

    private static Result<ServerPipes> CreatePipes(string requestPipeName, string responsePipeName)
    {
        try
        {
            var reqPipe = new NamedPipeServerStream(
                pipeName: requestPipeName,
                direction: PipeDirection.In,
                maxNumberOfServerInstances: 1,
                transmissionMode: PipeTransmissionMode.Byte,
                options: PipeOptions.Asynchronous);

            var resPipe = new NamedPipeServerStream(
                pipeName: responsePipeName,
                direction: PipeDirection.Out,
                maxNumberOfServerInstances: 1,
                transmissionMode: PipeTransmissionMode.Byte,
                options: PipeOptions.Asynchronous);

            return new ServerPipes { RequestPipe = reqPipe, ResponsePipe = resPipe };
        }
        catch (Exception ex) { return ex; }
    }


    /// <summary>
    /// Carries the cancellation token and the result of the polling task back to the caller.
    /// </summary>
    private sealed class CancellationHandle
    {
        private readonly CancellationTokenSource cts;

        internal CancellationHandle(CancellationTokenSource cts)
        {
            this.cts = cts;
            Token = this.cts.Token;
        }

        /// <summary>
        /// True if cancellation was triggered by the <c>shouldCancel</c> callback
        /// (i.e. the caller requested it), false if the CTS was cancelled for any
        /// other reason or not cancelled at all.
        /// </summary>
        internal bool CancelledByCallback { get; set; }

        internal CancellationToken Token { get; }

        internal void Dispose()
        {
            // Force-cancel in case the caller disposes before the polling task fires,
            // then release the CTS.
            cts.Cancel();
            cts.Dispose();
        }
    }



    #endregion
}
