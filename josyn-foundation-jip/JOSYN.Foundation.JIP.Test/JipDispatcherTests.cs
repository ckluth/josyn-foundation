using JOSYN.Foundation.ResultPattern;
using NUnit.Framework;

namespace JOSYN.Foundation.JIP.Test;

[TestFixture]
internal sealed class JipDispatcherTests
{
    // ── Register (get-style: no input, string output) ────────────────────────

    [Test]
    public void Register_GetHandler_RegisteredKeyIsVisible()
    {
        var dispatcher = new JipDispatcher();

        dispatcher.Register("GetFoo", () => Task.FromResult(Result<string>.Success("value")));

        Assert.That(dispatcher.RegisteredKeys, Contains.Item("GetFoo"));
    }

    [Test]
    public async Task Register_GetHandler_OnDispatch_ReturnsSuccessResponseWithData()
    {
        var dispatcher = new JipDispatcher();
        dispatcher.Register("GetFoo", () => Task.FromResult(Result<string>.Success("the-value")));

        var responseJson = await dispatcher.Dispatch(new Request { What = "GetFoo" }.ToString());
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Succeeded, Is.True);
        Assert.That(response.Value!.Succeeded, Is.True);
        Assert.That(response.Value.Data, Is.EqualTo("the-value"));
    }

    [Test]
    public async Task Register_GetHandler_FailingHandler_OnDispatch_ReturnsFailureResponse()
    {
        var dispatcher = new JipDispatcher();
        dispatcher.Register("GetFoo", () => Task.FromResult(Result<string>.Fail("Handler-Fehler")));

        var responseJson = await dispatcher.Dispatch(new Request { What = "GetFoo" }.ToString());
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Value!.Succeeded, Is.False);
        Assert.That(response.Value.Error, Does.Contain("Handler-Fehler"));
    }

    // ── Register (put-style: string input, no output) ────────────────────────

    [Test]
    public async Task Register_PutHandler_WithData_OnDispatch_ReturnsSuccessResponse()
    {
        var dispatcher = new JipDispatcher();
        dispatcher.Register("PutFoo", (string _) => Task.FromResult(Result.Success));

        var responseJson = await dispatcher.Dispatch(new Request { What = "PutFoo", Data = "input" }.ToString());
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Value!.Succeeded, Is.True);
    }

    [Test]
    public async Task Register_PutHandler_NullData_OnDispatch_ReturnsFailureWithKeyInMessage()
    {
        var dispatcher = new JipDispatcher();
        dispatcher.Register("PutFoo", (string _) => Task.FromResult(Result.Success));

        var responseJson = await dispatcher.Dispatch(new Request { What = "PutFoo", Data = null }.ToString());
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Value!.Succeeded, Is.False);
        Assert.That(response.Value.Error, Does.Contain("PutFoo"));
    }

    // ── Register (nullable async: string? → Result<string?>) ─────────────────

    [Test]
    public async Task Register_NullableAsyncHandler_OnDispatch_ReturnsExpectedData()
    {
        var dispatcher = new JipDispatcher();
        dispatcher.Register("Echo", (string? data) => Task.FromResult(Result<string?>.Success(data)));

        var responseJson = await dispatcher.Dispatch(new Request { What = "Echo", Data = "hello" }.ToString());
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Value!.Data, Is.EqualTo("hello"));
    }

    // ── Register (sync: string? → Result<string?>) ───────────────────────────

    [Test]
    public async Task Register_SyncHandler_OnDispatch_ReturnsExpectedData()
    {
        var dispatcher = new JipDispatcher();
        dispatcher.Register("Ping", (string? _) => Result<string?>.Success("pong"));

        var responseJson = await dispatcher.Dispatch(new Request { What = "Ping" }.ToString());
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Value!.Data, Is.EqualTo("pong"));
    }

    // ── Register (constant result) ────────────────────────────────────────────

    [Test]
    public async Task Register_ConstantResult_OnDispatch_AlwaysReturnsConstantData()
    {
        var dispatcher = new JipDispatcher();
        dispatcher.Register("Version", Result<string?>.Success("1.0.0"));

        var responseJson = await dispatcher.Dispatch(new Request { What = "Version" }.ToString());
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Value!.Data, Is.EqualTo("1.0.0"));
    }

    [Test]
    public async Task Register_ConstantFailureResult_OnDispatch_ReturnsFailureResponse()
    {
        var dispatcher = new JipDispatcher();
        dispatcher.Register("AlwaysFails", Result<string?>.Fail("Immer ein Fehler"));

        var responseJson = await dispatcher.Dispatch(new Request { What = "AlwaysFails" }.ToString());
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Value!.Succeeded, Is.False);
        Assert.That(response.Value.Error, Does.Contain("Immer ein Fehler"));
    }

    // ── Fluent chaining ───────────────────────────────────────────────────────

    [Test]
    public void Register_ReturnsDispatcherInstance_ForFluentChaining()
    {
        var dispatcher = new JipDispatcher();

        var returned = dispatcher.Register("Ping", (string? _) => Result<string?>.Success("pong"));

        Assert.That(returned, Is.SameAs(dispatcher));
    }

    // ── Overwrite ─────────────────────────────────────────────────────────────

    [Test]
    public async Task Register_DuplicateKey_OverwritesPreviousHandler()
    {
        var dispatcher = new JipDispatcher();
        dispatcher.Register("GetFoo", (string? _) => Result<string?>.Success("old"));
        dispatcher.Register("GetFoo", (string? _) => Result<string?>.Success("new"));

        var responseJson = await dispatcher.Dispatch(new Request { What = "GetFoo" }.ToString());
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Value!.Data, Is.EqualTo("new"));
    }

    // ── Dispatch: unknown key ─────────────────────────────────────────────────

    [Test]
    public async Task Dispatch_UnknownKey_ReturnsFailureResponseContainingKey()
    {
        var dispatcher = new JipDispatcher();

        var responseJson = await dispatcher.Dispatch(new Request { What = "UnknownFunction" }.ToString());
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Value!.Succeeded, Is.False);
        Assert.That(response.Value.Error, Does.Contain("UnknownFunction"));
    }

    [Test]
    public async Task Dispatch_InvalidRequestJson_ReturnsFailureResponse()
    {
        var dispatcher = new JipDispatcher();

        var responseJson = await dispatcher.Dispatch("not-valid-json");
        var response = JipProtocol.ParseResponse(responseJson);

        Assert.That(response.Value!.Succeeded, Is.False);
    }

    // ── RegisterAll: edge cases ───────────────────────────────────────────────

    [Test]
    public void RegisterAll_EmptyProtocol_RegistersNoKeysAndReturnsSelf()
    {
        var dispatcher = new JipDispatcher();

        var returned = dispatcher.RegisterAll<IEmptyProtocol>(new FakeEmptyProtocol());

        Assert.That(dispatcher.RegisteredKeys, Is.Empty);
        Assert.That(returned, Is.SameAs(dispatcher));
    }

    [Test]
    public void RegisterAll_UnsupportedMethodSignature_ThrowsInvalidOperationException()
    {
        var dispatcher = new JipDispatcher();

        Assert.Throws<InvalidOperationException>(
            () => dispatcher.RegisterAll<IUnsupportedProtocol>(new FakeUnsupportedProtocol()));
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private interface IEmptyProtocol { }
    private sealed class FakeEmptyProtocol : IEmptyProtocol { }

    private interface IUnsupportedProtocol
    {
        string NotSupported(int value);
    }

    private sealed class FakeUnsupportedProtocol : IUnsupportedProtocol
    {
        public string NotSupported(int value) => value.ToString();
    }
}
