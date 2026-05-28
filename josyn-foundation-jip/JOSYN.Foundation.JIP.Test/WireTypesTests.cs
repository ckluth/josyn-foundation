using System.Text.Json;
using NUnit.Framework;

namespace JOSYN.Foundation.JIP.Test;

[TestFixture]
internal sealed class WireTypesTests
{
    // ── Request.ToString ─────────────────────────────────────────────────────

    [Test]
    public void Request_ToString_ProducesValidJsonWithWhatAndData()
    {
        var request = new Request { What = "GetStatus", Data = "payload" };

        var json = request.ToString();
        var doc = JsonDocument.Parse(json);

        Assert.That(doc.RootElement.GetProperty("What").GetString(), Is.EqualTo("GetStatus"));
        Assert.That(doc.RootElement.GetProperty("Data").GetString(), Is.EqualTo("payload"));
    }

    [Test]
    public void Request_ToString_WithNullData_ProducesValidJson()
    {
        var request = new Request { What = "Ping" };

        var json = request.ToString();

        Assert.DoesNotThrow(() => JsonDocument.Parse(json));
    }

    // ── Response.ToString ────────────────────────────────────────────────────

    [Test]
    public void Response_ToString_SuccessResponse_ContainsSucceededTrue()
    {
        var response = new Response { Succeeded = true, Data = "result" };

        var json = response.ToString();
        var doc = JsonDocument.Parse(json);

        Assert.That(doc.RootElement.GetProperty("Succeeded").GetBoolean(), Is.True);
    }

    [Test]
    public void Response_ToString_FailureResponse_ContainsSucceededFalse()
    {
        var response = new Response { Succeeded = false, Error = "Fehler" };

        var json = response.ToString();
        var doc = JsonDocument.Parse(json);

        Assert.That(doc.RootElement.GetProperty("Succeeded").GetBoolean(), Is.False);
    }

    // ── Round-trips ───────────────────────────────────────────────────────────

    [Test]
    public void Request_ToString_ThenParseRequest_RoundTripPreservesValues()
    {
        var original = new Request { What = "GetFoo", Data = "bar" };

        var parsed = JipProtocol.ParseRequest(original.ToString());

        Assert.That(parsed.Succeeded, Is.True);
        Assert.That(parsed.Value!.What, Is.EqualTo(original.What));
        Assert.That(parsed.Value.Data, Is.EqualTo(original.Data));
    }

    [Test]
    public void Response_ToString_ThenParseResponse_RoundTripPreservesValues()
    {
        var original = new Response { Succeeded = true, Data = "result-data" };

        var parsed = JipProtocol.ParseResponse(original.ToString());

        Assert.That(parsed.Succeeded, Is.True);
        Assert.That(parsed.Value!.Succeeded, Is.True);
        Assert.That(parsed.Value.Data, Is.EqualTo(original.Data));
    }

    [Test]
    public void Response_ToString_FailureRoundTrip_PreservesErrorMessage()
    {
        var original = new Response { Succeeded = false, Error = "Irgendein Fehler" };

        var parsed = JipProtocol.ParseResponse(original.ToString());

        Assert.That(parsed.Succeeded, Is.True);
        Assert.That(parsed.Value!.Succeeded, Is.False);
        Assert.That(parsed.Value.Error, Is.EqualTo(original.Error));
    }
}
