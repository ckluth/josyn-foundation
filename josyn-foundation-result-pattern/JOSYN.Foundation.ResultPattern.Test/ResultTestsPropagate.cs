using NUnit.Framework;

namespace JOSYN.Foundation.ResultPattern.Test;

[TestFixture]
public sealed class ResultTestsPropagate
{
    // ── void Result, propagated across mixed generic types ────────────────────

    [Test]
    public void Propagate_AcrossTypes_FourLevelChain_Succeeded_IsFalse() =>
        Assert.That(LoadUserDisplayName().Succeeded, Is.False);

    [Test]
    public void Propagate_AcrossTypes_FourLevelChain_Callers_HasFourEntries() =>
        Assert.That(LoadUserDisplayName().Callers.Count, Is.EqualTo(4));

    [Test]
    public void Propagate_AcrossTypes_FourLevelChain_ErrorMessage_IsPreserved() =>
        Assert.That(LoadUserDisplayName().ErrorMessage, Does.Contain("Datenbankverbindung fehlgeschlagen."));

    [Test]
    public void Propagate_AcrossTypes_FourLevelChain_Exception_IsPreserved() =>
        Assert.That(LoadUserDisplayName().Exception, Is.Not.Null);

    [Test]
    public void Propagate_AcrossTypes_FourLevelChain_CallStackAsString_AllEntriesStartWithAt() =>
        Assert.That(LoadUserDisplayName().CallStackAsString.Split('\n'), Has.All.StartWith("  at "));

    // ── void Result, same type across all levels ──────────────────────────────

    [Test]
    public void Propagate_SameType_FourLevelChain_Succeeded_IsFalse() =>
        Assert.That(DoA().Succeeded, Is.False);

    [Test]
    public void Propagate_SameType_FourLevelChain_Callers_HasFourEntries() =>
        Assert.That(DoA().Callers.Count, Is.EqualTo(4));

    [Test]
    public void Propagate_SameType_FourLevelChain_ErrorMessage_IsPreserved() =>
        Assert.That(DoA().ErrorMessage, Does.Contain("Etwas ist schiefgelaufen."));

    [Test]
    public void Propagate_SameType_FourLevelChain_Exception_IsPreserved() =>
        Assert.That(DoA().Exception, Is.Not.Null);

    [Test]
    public void Propagate_SameType_FourLevelChain_CallStackAsString_AllEntriesStartWithAt() =>
        Assert.That(DoA().CallStackAsString.Split('\n'), Has.All.StartWith("  at "));

    // ── generic Result<T>, propagated across levels ───────────────────────────

    [Test]
    public void Propagate_GenericResult_FourLevelChain_Succeeded_IsFalse() =>
        Assert.That(LoadUserDisplayName2().Succeeded, Is.False);

    [Test]
    public void Propagate_GenericResult_FourLevelChain_Callers_HasFourEntries() =>
        Assert.That(LoadUserDisplayName2().Callers.Count, Is.EqualTo(4));

    [Test]
    public void Propagate_GenericResult_FourLevelChain_ErrorMessage_IsPreserved() =>
        Assert.That(LoadUserDisplayName2().ErrorMessage, Does.Contain("Datenbankverbindung fehlgeschlagen."));

    [Test]
    public void Propagate_GenericResult_FourLevelChain_Exception_IsPreserved() =>
        Assert.That(LoadUserDisplayName2().Exception, Is.Not.Null);

    [Test]
    public void Propagate_GenericResult_FourLevelChain_CallStackAsString_AllEntriesStartWithAt() =>
        Assert.That(LoadUserDisplayName2().CallStackAsString.Split('\n'), Has.All.StartWith("  at "));

    // ── single-level propagation (2-frame chain) ──────────────────────────────

    [Test]
    public void Propagate_SingleLevel_Succeeded_IsFalse() =>
        Assert.That(SingleLevel_Outer().Succeeded, Is.False);

    [Test]
    public void Propagate_SingleLevel_Callers_HasTwoEntries() =>
        Assert.That(SingleLevel_Outer().Callers.Count, Is.EqualTo(2));

    [Test]
    public void Propagate_SingleLevel_ErrorMessage_IsPreserved() =>
        Assert.That(SingleLevel_Outer().ErrorMessage, Is.EqualTo("inner failed"));

    // ── scenario helpers ──────────────────────────────────────────────────────

    private Result LoadUserDisplayName()
    {
        var result = ParseUserAge();
        if (!result.Succeeded) return Result.Propagate(result.ToResult());
        return Result.Success;
    }

    private Result<int> ParseUserAge()
    {
        var result = ReadUserName();
        if (!result.Succeeded) return Result<int>.Propagate(result.ToResult<int>());
        return int.Parse(result.Value);
    }

    private Result<string> ReadUserName()
    {
        var result = LoadUserRecord();
        if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
        return result.Value.ToString();
    }

    private Result<Guid> LoadUserRecord()
    {
        try { throw new InvalidOperationException("Datenbankverbindung fehlgeschlagen."); }
        catch (Exception ex) { return ex; }
    }

    private Result DoA()
    {
        var result = DoB();
        if (!result.Succeeded) return Result.Propagate(result);
        return Result.Success;
    }

    private Result DoB()
    {
        var result = DoC();
        if (!result.Succeeded) return Result.Propagate(result);
        return Result.Success;
    }

    private Result DoC()
    {
        var result = DoD();
        if (!result.Succeeded) return Result.Propagate(result);
        return Result.Success;
    }

    private Result DoD()
    {
        try { throw new InvalidOperationException("Etwas ist schiefgelaufen."); }
        catch (Exception ex) { return ex; }
    }

    private Result<string> LoadUserDisplayName2()
    {
        var result = ParseUserAge2();
        if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
        return result.Value.ToString();
    }

    private Result<int> ParseUserAge2()
    {
        var result = ReadUserName2();
        if (!result.Succeeded) return Result<int>.Propagate(result.ToResult<int>());
        return int.Parse(result.Value);
    }

    private Result<string> ReadUserName2()
    {
        var result = LoadUserRecord2();
        if (!result.Succeeded) return Result<string>.Propagate(result.ToResult<string>());
        return result.Value.ToString();
    }

    private Result<Guid> LoadUserRecord2()
    {
        try { throw new InvalidOperationException("Datenbankverbindung fehlgeschlagen."); }
        catch (Exception ex) { return ex; }
    }

    private static Result SingleLevel_Inner() => Result.Fail("inner failed");

    private static Result SingleLevel_Outer()
    {
        var r = SingleLevel_Inner();
        if (!r.Succeeded) return Result.Propagate(r);
        return Result.Success;
    }
}
