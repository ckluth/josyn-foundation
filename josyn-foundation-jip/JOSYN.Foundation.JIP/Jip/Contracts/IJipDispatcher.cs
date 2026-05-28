using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

/// <summary>
/// Contract definition for the JIP request dispatcher.
/// Routes incoming JIP requests by <see cref="Request.What"/> to registered handlers,
/// eliminating manual <c>switch</c> expressions on the server side.
/// </summary>
public interface IJipDispatcher
{
    /// <summary>
    /// The set of keys registered so far.
    /// Intended for test assertions that verify protocol completeness.
    /// </summary>
    IReadOnlySet<string> RegisteredKeys { get; }

    /// <summary>
    /// Registers an asynchronous handler with no input data and a string return value.
    /// Suitable for methods of the form <c>Task&lt;Result&lt;string&gt;&gt; GetXxx()</c>.
    /// </summary>
    IJipDispatcher Register(string key, Func<Task<Result<string>>> handler);

    /// <summary>
    /// Registers an asynchronous handler with a required string argument and no return value.
    /// Returns a failure if the request contains no data.
    /// Suitable for methods of the form <c>Task&lt;Result&gt; PutXxx(string value)</c>.
    /// </summary>
    IJipDispatcher Register(string key, Func<string, Task<Result>> handler);

    /// <summary>
    /// Registers an asynchronous handler with an optional input and output string.
    /// Suitable for the most general async handler form.
    /// </summary>
    IJipDispatcher Register(string key, Func<string?, Task<Result<string?>>> handler);

    /// <summary>
    /// Registers a synchronous handler with an optional input and output string.
    /// Suitable for inline functions of the form <c>(data) => Result&lt;string?&gt;.Success(...)</c>.
    /// </summary>
    IJipDispatcher Register(string key, Func<string?, Result<string?>> handler);

    /// <summary>
    /// Registers a constant result — useful for fixed responses such as PING or
    /// hard-coded configuration values.
    /// </summary>
    IJipDispatcher Register(string key, Result<string?> constantResult);

    /// <summary>
    /// Registers all methods of <typeparamref name="TProtocol"/> by convention:
    /// the method name becomes the <c>What</c> key, and the signature determines the handler form.
    /// </summary>
    /// <remarks>
    /// Supported signatures:
    /// <list type="bullet">
    /// <item><c>Task&lt;Result&lt;string&gt;&gt; Method()</c></item>
    /// <item><c>Task&lt;Result&gt; Method(string data)</c></item>
    /// </list>
    /// Throws <see cref="InvalidOperationException"/> during registration if an unsupported signature is encountered.
    /// </remarks>
    /// <typeparam name="TProtocol">
    /// The protocol interface type. Always specify the interface explicitly, not the concrete class,
    /// to avoid registering non-protocol members.
    /// </typeparam>
    IJipDispatcher RegisterAll<TProtocol>(TProtocol impl) where TProtocol : class;

    /// <summary>
    /// Dispatches a raw JIP request string. Unknown <c>What</c> values are answered with
    /// an error response.
    /// Pass this method as <see cref="ServerStartArguments.HandleStringRequest"/>.
    /// </summary>
    Task<string> Dispatch(string requestStr);
}
