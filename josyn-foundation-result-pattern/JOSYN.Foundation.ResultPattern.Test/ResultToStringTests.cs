using NUnit.Framework;

namespace JOSYN.Foundation.ResultPattern.Test;

[TestFixture]
public sealed class ResultToStringTests
{
    // ── Result.ToString() — Success ───────────────────────────────────────────

    [Test]
    public void Result_Success_ToString_ReturnsErfolgreich()
    {
        Assert.That(Result.Success.ToString(), Is.EqualTo("[Erfolgreich]"));
    }

    // ── Result.ToString() — Failure ───────────────────────────────────────────

    [Test]
    public void Result_Fail_ToString_StartsWithFehlgeschlagen()
    {
        Assert.That(Result.Fail("etwas ist schiefgelaufen").ToString(), Does.StartWith("[Fehlgeschlagen]"));
    }

    [Test]
    public void Result_Fail_ToString_ContainsErrorMessage()
    {
        Assert.That(Result.Fail("etwas ist schiefgelaufen").ToString(), Does.Contain("etwas ist schiefgelaufen"));
    }

    [Test]
    public void Result_Fail_ToString_ContainsCallStack()
    {
        Assert.That(Result.Fail("fehler").ToString(), Does.Contain("  at "));
    }

    [Test]
    public void Result_Fail_WithException_ToString_ContainsAusnahme()
    {
        var ex = new InvalidOperationException("ungültige Operation");
        Assert.That(Result.Fail("fehler", ex).ToString(), Does.Contain("Ausnahme:"));
    }

    [Test]
    public void Result_Fail_WithException_ToString_ContainsExceptionMessage()
    {
        var ex = new InvalidOperationException("ungültige Operation");
        Assert.That(Result.Fail("fehler", ex).ToString(), Does.Contain("ungültige Operation"));
    }

    [Test]
    public void Result_Fail_WithoutException_ToString_DoesNotContainAusnahme()
    {
        Assert.That(Result.Fail("fehler").ToString(), Does.Not.Contain("Ausnahme:"));
    }

    // ── Result<T>.ToString() — Success ────────────────────────────────────────

    [Test]
    public void ResultGeneric_Success_ToString_ContainsErfolgreich()
    {
        Result<int> result = 42;
        Assert.That(result.ToString(), Does.StartWith("[Erfolgreich]"));
    }

    [Test]
    public void ResultGeneric_Success_ToString_ContainsValue()
    {
        Result<int> result = 42;
        Assert.That(result.ToString(), Does.Contain("42"));
    }

    [Test]
    public void ResultGeneric_Success_String_ToString_ContainsValue()
    {
        Result<string> result = "hallo";
        Assert.That(result.ToString(), Does.Contain("hallo"));
    }

    [Test]
    public void ResultGeneric_Success_NullValue_ToString_ReturnsErfolgreich()
    {
        Result<string?> result = Result<string?>.Success(null);
        Assert.That(result.ToString(), Is.EqualTo("[Erfolgreich]"));
    }

    // ── Result<T>.ToString() — Failure ────────────────────────────────────────

    [Test]
    public void ResultGeneric_Fail_ToString_StartsWithFehlgeschlagen()
    {
        Assert.That(Result<int>.Fail("etwas ist schiefgelaufen").ToString(), Does.StartWith("[Fehlgeschlagen]"));
    }

    [Test]
    public void ResultGeneric_Fail_ToString_ContainsErrorMessage()
    {
        Assert.That(Result<int>.Fail("etwas ist schiefgelaufen").ToString(), Does.Contain("etwas ist schiefgelaufen"));
    }

    [Test]
    public void ResultGeneric_Fail_ToString_ContainsCallStack()
    {
        Assert.That(Result<int>.Fail("fehler").ToString(), Does.Contain("  at "));
    }

    [Test]
    public void ResultGeneric_Fail_WithException_ToString_ContainsAusnahme()
    {
        var ex = new InvalidOperationException("ungültige Operation");
        Assert.That(Result<int>.Fail("fehler", ex).ToString(), Does.Contain("Ausnahme:"));
    }

    [Test]
    public void ResultGeneric_Fail_WithoutException_ToString_DoesNotContainAusnahme()
    {
        Assert.That(Result<int>.Fail("fehler").ToString(), Does.Not.Contain("Ausnahme:"));
    }
}
