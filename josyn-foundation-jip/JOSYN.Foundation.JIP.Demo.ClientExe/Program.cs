using JOSYN.Foundation.JIP;
using JOSYN.Foundation.ResultPattern;
using System.Text;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP.Demo;
#pragma warning restore IDE0130

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;
        
        var sessionKey = PipesProtocol.ParseSessionKeyCLIArguments(args);

        if (sessionKey == Guid.Empty)
            return LogError("Es wurde kein Pipes-SessionKey übergeben", 1);

        Console.WriteLine("SessionKey: " + sessionKey);
        Console.WriteLine("[PRESS KEY TO CONNECT]");
        Console.ReadKey(true);

        var getPipes = await PipesClient.ConnectAsync(sessionKey);
        if (!getPipes.Succeeded)
            return LogErrorResult(getPipes.ToResult(), 1);

        Console.WriteLine("Connected.\n");
        var pipes = getPipes.Value;

        // --- PING (void) ---
        var ping = await JipClient.SendAsync(pipes, "PING");
        PrintResult("PING", ping.ToResult());
        if (!ping.Succeeded) return 1;

        // --- GET-CONFIG (string-Payload) ---
        var config = await JipClient.SendAsync(pipes, "GET-CONFIG");
        PrintResult("GET-CONFIG", config.ToResult(), config.Value);
        if (!config.Succeeded) return 1;

        // --- ECHO (string-Payload round-trip) ---
        var echo = await JipClient.SendAsync(pipes, "ECHO", "Hallo JOSYN");
        PrintResult("ECHO", echo.ToResult(), echo.Value);
        if (!echo.Succeeded) return 1;

        // --- DO-MAGIC (erwarteter Fehler) ---
        var magic = await JipClient.SendAsync(pipes, "DO-MAGIC");
        PrintResult("DO-MAGIC", magic.ToResult());

        Console.WriteLine("\n[PRESS KEY TO DISCONNECT]");
        Console.ReadKey(true);

        await PipesClient.DisconnectAsync(pipes, sendShutdownRequest: false);
        Console.WriteLine("Disconnected.\n[PRESS KEY TO EXIT]");
        Console.ReadKey(true);
        return 0;
    }

    private static void PrintResult(string label, Result result, string? data = null)
    {
        if (result.Succeeded)
        {
            Console.Write($"CLI|{label}> OK");
            if (data != null) Console.Write($" → {data}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"CLI|{label}> FEHLER → {result.ErrorMessage}");
            Console.ResetColor();
        }
        Console.WriteLine("\n");
    }

    #region FakeLog

    private static int LogErrorResult(Result result, int exitCode)
    {
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

    private static int LogError(string msg, int exitCode)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(msg);
        Console.ResetColor();
        Console.WriteLine("\n[PRESS ANY KEY]");
        Console.ReadKey();
        return exitCode;
    }

    #endregion
}