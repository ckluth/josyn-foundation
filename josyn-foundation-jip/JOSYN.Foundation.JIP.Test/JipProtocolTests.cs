using JOSYN.Foundation.ResultPattern;
using NUnit.Framework;

namespace JOSYN.Foundation.JIP.Test;

[TestFixture]
internal sealed class JipProtocolTests
{
    // ── ParseRequest ─────────────────────────────────────────────────────────

    [Test]
    public void ParseRequest_ValidJsonWithWhatAndData_ReturnsSuccessWithBothFields()
    {
        var raw = """{"What":"GetStatus","Data":"payload"}""";

        var result = JipProtocol.ParseRequest(raw);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.What, Is.EqualTo("GetStatus"));
        Assert.That(result.Value.Data, Is.EqualTo("payload"));
    }

    [Test]
    public void ParseRequest_ValidJsonWithoutData_ReturnsSuccessWithNullData()
    {
        var raw = """{"What":"Ping"}""";

        var result = JipProtocol.ParseRequest(raw);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.What, Is.EqualTo("Ping"));
        Assert.That(result.Value.Data, Is.Null);
    }

    [Test]
    public void ParseRequest_InvalidJson_ReturnsFailure()
    {
        var result = JipProtocol.ParseRequest("not-json");

        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void ParseRequest_EmptyString_ReturnsFailure()
    {
        var result = JipProtocol.ParseRequest(string.Empty);

        Assert.That(result.Succeeded, Is.False);
    }

    // ── ParseResponse ────────────────────────────────────────────────────────

    [Test]
    public void ParseResponse_ValidSuccessJson_ParseSucceedsWithSucceededTrue()
    {
        var raw = """{"Succeeded":true,"Data":"result"}""";

        var result = JipProtocol.ParseResponse(raw);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Succeeded, Is.True);
        Assert.That(result.Value.Data, Is.EqualTo("result"));
    }

    [Test]
    public void ParseResponse_ValidFailureJson_ParseSucceedsWithSucceededFalse()
    {
        var raw = """{"Succeeded":false,"Error":"something went wrong"}""";

        var result = JipProtocol.ParseResponse(raw);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Succeeded, Is.False);
        Assert.That(result.Value.Error, Is.EqualTo("something went wrong"));
    }

    [Test]
    public void ParseResponse_InvalidJson_ReturnsFailure()
    {
        var result = JipProtocol.ParseResponse("not-json");

        Assert.That(result.Succeeded, Is.False);
    }

    // ── ToResponse ───────────────────────────────────────────────────────────

    [Test]
    public void ToResponse_SucceededResultWithData_ReturnsTrueResponseWithData()
    {
        var result = Result<string?>.Success("hello");

        var response = JipProtocol.ToResponse(result);

        Assert.That(response.Succeeded, Is.True);
        Assert.That(response.Data, Is.EqualTo("hello"));
    }

    [Test]
    public void ToResponse_SucceededResultWithNullValue_ReturnsTrueResponseWithNullData()
    {
        var result = Result<string?>.Success(null);

        var response = JipProtocol.ToResponse(result);

        Assert.That(response.Succeeded, Is.True);
        Assert.That(response.Data, Is.Null);
    }

    [Test]
    public void ToResponse_FailedResult_ReturnsFalseResponseWithErrorMessage()
    {
        var result = Result<string?>.Fail("Fehler aufgetreten");

        var response = JipProtocol.ToResponse(result);

        Assert.That(response.Succeeded, Is.False);
        Assert.That(response.Error, Is.EqualTo("Fehler aufgetreten"));
    }

    // ── ToResult ─────────────────────────────────────────────────────────────

    [Test]
    public void ToResult_SucceededResponse_ReturnsSucceededResultWithData()
    {
        var response = new Response { Succeeded = true, Data = "value" };

        var result = JipProtocol.ToResult(response);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Is.EqualTo("value"));
    }

    [Test]
    public void ToResult_SucceededResponseWithNullData_ReturnsSucceededResultWithNullValue()
    {
        var response = new Response { Succeeded = true, Data = null };

        var result = JipProtocol.ToResult(response);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public void ToResult_FailedResponse_ReturnsFailedResultWithError()
    {
        var response = new Response { Succeeded = false, Error = "Fehler" };

        var result = JipProtocol.ToResult(response);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Fehler"));
    }
}
