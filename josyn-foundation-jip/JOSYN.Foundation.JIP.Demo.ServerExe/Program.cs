using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using System.Diagnostics;
using System.Text;

namespace JOSYN.Foundation.JIP.Demo.ServerExe;

// 
//  TODO: mit Server sharen
//
public interface IJosynApplicationProtocol
{
    Task<Result<string>> GetRawArguments();

    Task<Result> PutRawResult(string result);

}

public sealed class JAPServer: IJosynApplicationProtocol
{
    public async Task<Result<string>> GetRawArguments()
    {
        return await Task.FromResult(FakeReadArgumentsFromFile());
    }

    public async Task<Result> PutRawResult(string result)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("[PROCESSING]");
        Console.WriteLine(result);
        Console.WriteLine();
        Console.ResetColor();
        
        return await Task.FromResult(Result.Success);
    }
    
    internal static string FakeReadArgumentsFromFile()
    {
        const string inicontent = """
                                  Msg=Hello JOSYN
                                  Count=9
                                  MaybeCount=
                                  IsSpecial=True
                                  Expired=21.09.1988 00:00:00
                                  OnlyDate=04.11.1966
                                  MaybeDate=
                                  EnumValue=Value2
                                  MyTimeSpan=09:10:59
                                  Price=1.200,30
                                  """;
        return inicontent;
    }
}

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            Console.WriteLine("ARGS: " + string.Join(" | ", args));
            var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);
            if (sessionKey == Guid.Empty)
                return LogError("Keine IPC-Session-UID angegeben.", 1);
            
            return await RunServer(sessionKey);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return 1;
        }
    }

    private static async Task<int> RunServer(Guid sessionKey, int? timeout = null)
    {
        Console.WriteLine("Starting Server...");
        var sw = Stopwatch.StartNew();

        var serverStartArguments = new ServerStartArguments
        {
            ConnectionTimeout = TimeSpan.FromDays(timeout ?? 1),
            HandleStringRequest = HandleRequest,
            SessionKey = sessionKey,
            HandleErrorNotification = HandleHandlerError,
            IsCancellationRequested = WasEscapePressed,
        };
        
        var res = await PipesServer.RunAsync(serverStartArguments, true, () =>
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nResestablishing Connection\n");
            Console.ResetColor();
        } );

        Console.WriteLine($"Finished after {sw.Elapsed}");
        return !res.Succeeded ? LogErrorResult(res, 1) : TerminateWithSuccess(); ;
    }

    private static Task<bool> WasEscapePressed()
    {
        if (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Escape)
            return Task.FromResult(false);
        Console.WriteLine("ESC gedrückt. Abbruch...");
        
        return Task.FromResult(true);
    }
    
    private static readonly JAPServer japServer = new();

    private static readonly IJipDispatcher jipDispatcher = new JipDispatcher()
        .RegisterAll<IJosynApplicationProtocol>(japServer)
        .Register("PING",       Result<string?>.Success(null))
        .Register("GET-CONFIG", Result<string?>.Success("{ \"version\": \"1.0\", \"mode\": \"demo\" }"))
        .Register("ECHO",       (string? data) => Result<string?>.Success("ECHO " + data));

    private static async Task<string> HandleRequest(string requestStr)
    {
        Console.WriteLine($"SRV|RECEIVED> {requestStr}");
        var responseStr = await jipDispatcher.Dispatch(requestStr);
        Console.WriteLine($"SRV|SENDING>  {responseStr}");
        return responseStr;
    }

    #region FakeLog
    private static async Task HandleHandlerError(string request, Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine($"Error handling request: {request}");
        Console.WriteLine($"Exception: {ex}");
        Console.WriteLine();
        Console.ResetColor();
        await Task.CompletedTask;
    }

    private static int TerminateWithSuccess()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Terminated...");
        Console.ResetColor();
        Console.WriteLine("\n[PRESS ANY KEY]");
        Console.ReadKey();
        return 0;
    }

    private static int LogErrorResult(Result result, int exitCode, string? msg = null)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(msg);
            Console.ResetColor();
            Console.WriteLine();
        }

        var sb = new StringBuilder();

        sb.AppendLine("[Errormessage]");
        sb.AppendLine(result.ErrorMessage);
        sb.AppendLine();
        sb.AppendLine("[Callstack]");
        sb.AppendLine(result.CallStackAsString);
        if (result.Exception != null)
        {
            sb.AppendLine();
            sb.AppendLine("[Exception]");
            sb.AppendLine(result.Exception.ToString());
        }

        return LogError(sb.ToString(), exitCode);
    }

    private static int LogError(string msg, int exitCode, bool waitForKeyPress = true)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ResetColor();

        if (!waitForKeyPress) return 0;
        
        Console.WriteLine("\n[PRESS ANY KEY]");
        Console.ReadKey();
        return exitCode;

    }

    #endregion

}

