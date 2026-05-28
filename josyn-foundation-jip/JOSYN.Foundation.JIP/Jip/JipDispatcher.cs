
#pragma warning disable IDE0130
namespace JOSYN.Foundation.JIP;
#pragma warning restore IDE0130

using JOSYN.Foundation.ResultPattern;

/// <inheritdoc cref="IJipDispatcher"/>
public sealed class JipDispatcher : IJipDispatcher
{
    private readonly Dictionary<string, Func<string?, Task<Result<string?>>>> _handlers = new();
    private readonly Func<string, Task<string>> _builtDispatch;

    /// <inheritdoc/>
    public IReadOnlySet<string> RegisteredKeys => _handlers.Keys.ToHashSet();

    /// <summary>Initializes a new <see cref="JipDispatcher"/> with no registered handlers.</summary>
    public JipDispatcher()
    {
        // Closure captures the _handlers reference — entries added via Register() are visible at dispatch time.
        _builtDispatch = JipServer.WrapHandler(async req =>
        {
            if (!_handlers.TryGetValue(req.What, out var handler))
                return Result<string?>.Fail($"Unbekannte Funktion: '{req.What}'");
            return await handler(req.Data);
        });
    }

    /// <inheritdoc/>
    public IJipDispatcher Register(string key, Func<Task<Result<string>>> handler)
        => RegisterCore(key, async _ =>
        {
            var r = await handler();
            return r.Succeeded ? Result<string?>.Success(r.Value) : r.ToResult<string?>();
        });

    /// <inheritdoc/>
    public IJipDispatcher Register(string key, Func<string, Task<Result>> handler)
        => RegisterCore(key, async data =>
        {
            if (data is null)
                return Result<string?>.Fail($"Funktion '{key}' erwartet Eingabedaten, erhielt aber null.");
            var r = await handler(data);
            return r.Succeeded ? Result<string?>.Success(null) : r.ToResult<string?>();
        });

    /// <inheritdoc/>
    public IJipDispatcher Register(string key, Func<string?, Task<Result<string?>>> handler)
        => RegisterCore(key, handler);

    /// <inheritdoc/>
    public IJipDispatcher Register(string key, Func<string?, Result<string?>> handler)
        => RegisterCore(key, data => Task.FromResult(handler(data)));

    /// <inheritdoc/>
    public IJipDispatcher Register(string key, Result<string?> constantResult)
        => RegisterCore(key, _ => Task.FromResult(constantResult));

    /// <inheritdoc/>
    public IJipDispatcher RegisterAll<TProtocol>(TProtocol impl) where TProtocol : class
    {
        foreach (var m in typeof(TProtocol).GetMethods())
        {
            var key = m.Name;
            var parameters = m.GetParameters();

            if (parameters.Length == 0 && m.ReturnType == typeof(Task<Result<string>>))
            {
                RegisterCore(key, async _ =>
                {
                    var r = await (Task<Result<string>>)m.Invoke(impl, null)!;
                    return r.Succeeded ? Result<string?>.Success(r.Value) : r.ToResult<string?>();
                });
            }
            else if (parameters.Length == 1
                     && parameters[0].ParameterType == typeof(string)
                     && m.ReturnType == typeof(Task<Result>))
            {
                RegisterCore(key, async data =>
                {
                    if (data is null)
                        return Result<string?>.Fail($"Funktion '{key}' erwartet Eingabedaten, erhielt aber null.");
                    var r = await (Task<Result>)m.Invoke(impl, [data])!;
                    return r.Succeeded ? Result<string?>.Success(null) : r.ToResult<string?>();
                });
            }
            else
            {
                throw new InvalidOperationException(
                    $"Methode '{typeof(TProtocol).Name}.{key}' hat eine nicht unterstützte Signatur für RegisterAll. " +
                    $"Unterstützt: Task<Result<string>> Method() oder Task<Result> Method(string).");
            }
        }
        return this;
    }

    /// <inheritdoc/>
    public Task<string> Dispatch(string requestStr) => _builtDispatch(requestStr);

    private JipDispatcher RegisterCore(string key, Func<string?, Task<Result<string?>>> handler)
    {
        _handlers[key] = handler;
        return this;
    }
}
