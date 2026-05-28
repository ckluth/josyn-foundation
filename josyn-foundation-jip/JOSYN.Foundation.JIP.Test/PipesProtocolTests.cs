using NUnit.Framework;

namespace JOSYN.Foundation.JIP.Test;

[TestFixture]
internal sealed class PipesProtocolTests
{
    // ── CreateClientStartCLIArguments ────────────────────────────────────────

    [Test]
    public void CreateClientStartCLIArguments_ValidKey_StartsWithMagicToken()
    {
        var key = Guid.NewGuid().ToString();

        var args = PipesProtocol.CreateClientStartCLIArguments(key);

        Assert.That(args, Does.StartWith(IPipesProtocol.MagicToken));
    }

    [Test]
    public void CreateClientStartCLIArguments_ValidKey_ProducesExpectedFormat()
    {
        var key = Guid.NewGuid().ToString();

        var args = PipesProtocol.CreateClientStartCLIArguments(key);

        Assert.That(args, Is.EqualTo($"{IPipesProtocol.MagicToken} {key}"));
    }

    [Test]
    public void CreateClientStartCLIArguments_EmptyKey_StillFormatsWithMagicToken()
    {
        var args = PipesProtocol.CreateClientStartCLIArguments(string.Empty);

        Assert.That(args, Is.EqualTo($"{IPipesProtocol.MagicToken} "));
    }

    // ── ParseSessionKeyCLIArguments ──────────────────────────────────────────

    [Test]
    public void ParseSessionKeyCLIArguments_WellFormedArgs_ReturnsGuid()
    {
        var expected = Guid.NewGuid();
        var args = new[] { IPipesProtocol.MagicToken, expected.ToString() };

        var result = PipesProtocol.ParseSessionKeyCLIArguments(args);

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ParseSessionKeyCLIArguments_EmptyArray_ReturnsGuidEmpty()
    {
        var result = PipesProtocol.ParseSessionKeyCLIArguments([]);

        Assert.That(result, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void ParseSessionKeyCLIArguments_WrongToken_ReturnsGuidEmpty()
    {
        var result = PipesProtocol.ParseSessionKeyCLIArguments(["WRONG-TOKEN", Guid.NewGuid().ToString()]);

        Assert.That(result, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void ParseSessionKeyCLIArguments_TooManyArgs_ReturnsGuidEmpty()
    {
        var result = PipesProtocol.ParseSessionKeyCLIArguments(
            [IPipesProtocol.MagicToken, Guid.NewGuid().ToString(), "extra"]);

        Assert.That(result, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void ParseSessionKeyCLIArguments_NonGuidValue_ReturnsGuidEmpty()
    {
        var result = PipesProtocol.ParseSessionKeyCLIArguments([IPipesProtocol.MagicToken, "not-a-guid"]);

        Assert.That(result, Is.EqualTo(Guid.Empty));
    }

    [Test]
    public void ParseSessionKeyCLIArguments_RoundTripWithCreate_ReturnsOriginalGuid()
    {
        var key = Guid.NewGuid();
        var cliString = PipesProtocol.CreateClientStartCLIArguments(key.ToString());
        var splitArgs = cliString.Split(' ', 2);

        var result = PipesProtocol.ParseSessionKeyCLIArguments(splitArgs);

        Assert.That(result, Is.EqualTo(key));
    }

    // ── DerivePipeNamesFromSessionKey ────────────────────────────────────────

    [Test]
    public void DerivePipeNamesFromSessionKey_RequestPipeName_HasReqPrefix()
    {
        var key = "my-session";

        var (requestPipeName, _) = PipesProtocol.DerivePipeNamesFromSessionKey(key);

        Assert.That(requestPipeName, Is.EqualTo($"req-pipe-{key}"));
    }

    [Test]
    public void DerivePipeNamesFromSessionKey_ResponsePipeName_HasResPrefix()
    {
        var key = "my-session";

        var (_, responsePipeName) = PipesProtocol.DerivePipeNamesFromSessionKey(key);

        Assert.That(responsePipeName, Is.EqualTo($"res-pipe-{key}"));
    }

    [Test]
    public void DerivePipeNamesFromSessionKey_BothNamesContainKey()
    {
        var key = Guid.NewGuid().ToString();

        var (requestPipeName, responsePipeName) = PipesProtocol.DerivePipeNamesFromSessionKey(key);

        Assert.That(requestPipeName, Does.Contain(key));
        Assert.That(responsePipeName, Does.Contain(key));
    }

    [Test]
    public void DerivePipeNamesFromSessionKey_RequestAndResponseNamesAreDifferent()
    {
        var key = Guid.NewGuid().ToString();

        var (requestPipeName, responsePipeName) = PipesProtocol.DerivePipeNamesFromSessionKey(key);

        Assert.That(requestPipeName, Is.Not.EqualTo(responsePipeName));
    }
}
