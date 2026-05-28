using NUnit.Framework;

namespace JOSYN.Foundation.ResultPattern.Test;

[TestFixture]
public sealed class ResultTests
{

    //// ── Success ──────────────────────────────────────────────────────────────

    [Test]
    public void Success_Succeeded_IsTrue()
    {
        var result = Result.Success;
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void Success_ErrorMessage_IsNull()
    {
        var result = Result.Success;
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public void Success_Exception_IsNull()
    {
        var result = Result.Success;
        Assert.That(result.Exception, Is.Null);
    }

    // ── Result.Fail(string) ──────────────────────────────────────────────────

    [Test]
    public void Fail_String_Succeeded_IsFalse()
    {
        var result = Result.Fail("oops");
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void Fail_String_ErrorMessage_IsSet()
    {
        var result = Result.Fail("oops");
        Assert.That(result.ErrorMessage, Is.EqualTo("oops"));
    }

    [Test]
    public void Fail_String_Exception_IsNull_WhenNotProvided()
    {
        var result = Result.Fail("oops");
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void Fail_StringAndException_Exception_IsSet()
    {
        var ex = new InvalidOperationException("boom");
        var result = Result.Fail("oops", ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── Result.Fail(Exception) ───────────────────────────────────────────────

    [Test]
    public void Fail_Exception_Succeeded_IsFalse()
    {
        var result = Result.Fail(new Exception("bang"));
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void Fail_Exception_ErrorMessage_ContainsExceptionMessage()
    {
        var result = Result.Fail(new Exception("bang"));
        Assert.That(result.ErrorMessage, Does.Contain("bang"));
    }

    [Test]
    public void Fail_Exception_Exception_IsSet()
    {
        var ex = new Exception("bang");
        var result = Result.Fail(ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── Implicit conversion from Exception ───────────────────────────────────

    [Test]
    public void ImplicitConversion_FromException_Succeeded_IsFalse()
    {
        Result result = new Exception("implicit");
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void ImplicitConversion_FromException_ErrorMessage_ContainsExceptionMessage()
    {
        Result result = new Exception("implicit");
        Assert.That(result.ErrorMessage, Does.Contain("implicit"));
    }

    [Test]
    public void ImplicitConversion_FromException_Exception_IsSet()
    {
        var ex = new Exception("implicit");
        Result result = ex;
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── GetErrorInfo — caller info in error message ───────────────────────────

    [Test]
    public void Fail_Exception_ErrorMessage_ContainsCallerInfo()
    {
        var result = Result.Fail(new Exception("x"));
        // The call stack should contain the class/method name of the caller
        Assert.That(result.CallStackAsString, Does.Contain(nameof(ResultTests)));
    }

    // ── Result.Error(string) → Error struct → implicit to Result ─────────

    [Test]
    public void Error_Factory_ReturnsErrorStruct()
    {
        var Error = Result.Error("err");
        Assert.That(Error.ErrorMessage, Is.EqualTo("err"));
    }

    [Test]
    public void Error_ImplicitToResult_Succeeded_IsFalse()
    {
        Result result = Result.Error("err");
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void Error_ImplicitToResult_ErrorMessage_IsPreserved()
    {
        Result result = Result.Error("err");
        Assert.That(result.ErrorMessage, Is.EqualTo("err"));
    }

    [Test]
    public void Error_WithException_ExceptionIsPreserved()
    {
        var ex = new Exception("ex");
        Result result = Result.Error("err", ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── record equality ───────────────────────────────────────────────────────

    [Test]
    public void TwoSuccessResults_AreEqual()
    {
        var a = Result.Success;
        var b = Result.Success;
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void TwoFailResults_WithSameMessage_HaveSameErrorMessage()
    {
        var a = Result.Fail("same");
        var b = Result.Fail("same");
        Assert.That(a.ErrorMessage, Is.EqualTo(b.ErrorMessage));
    }

    [Test]
    public void TwoFailResults_AreNotRecordEqual()
    {
        // Callers differ (different line numbers per call site) → records are not equal
        var a = Result.Fail("same");
        var b = Result.Fail("same");
        Assert.That(a, Is.Not.EqualTo(b));
    }

    // ── Caller capture ────────────────────────────────────────────────────────

    [Test]
    public void Success_Callers_IsEmpty()
    {
        Assert.That(Result.Success.Callers, Is.Empty);
    }

    [Test]
    public void Success_CallStackAsString_ReturnsNoCallstackMessage()
    {
        Assert.That(Result.Success.CallStackAsString, Is.EqualTo("(kein Callstack)"));
    }

    [Test]
    public void Fail_String_Callers_HasOneEntry()
    {
        Assert.That(Result.Fail("oops").Callers.Count, Is.EqualTo(1));
    }

    [Test]
    public void Fail_String_CallStackAsString_ContainsCallerInfo()
    {
        Assert.That(Result.Fail("oops").CallStackAsString, Does.Contain(nameof(ResultTests)));
    }

    [Test]
    public void Fail_Exception_Callers_HasOneEntry()
    {
        Assert.That(Result.Fail(new Exception("bang")).Callers.Count, Is.EqualTo(1));
    }

    [Test]
    public void Fail_CallStackAsString_StartsWithAt()
    {
        Assert.That(Result.Fail("oops").CallStackAsString, Does.StartWith("  at "));
    }

    [Test]
    public void ImplicitConversion_FromException_Callers_HasOneEntry()
    {
        Result result = new Exception("x");
        Assert.That(result.Callers.Count, Is.EqualTo(1));
    }

    [Test]
    public void ImplicitConversion_FromException_CallerInfo_HasFilePath()
    {
        Result result = new Exception("x");
        Assert.That(result.Callers[0].FilePath, Is.Not.Empty);
    }

    [Test]
    public void ImplicitConversion_FromException_CallerInfo_HasLineNumber()
    {
        Result result = new Exception("x");
        Assert.That(result.Callers[0].LineNumber, Is.GreaterThan(0));
    }

    [Test]
    public void ImplicitConversion_FromError_Callers_HasOneEntry()
    {
        Result result = Result.Error("err");
        Assert.That(result.Callers.Count, Is.EqualTo(1));
    }

    [Test]
    public void ImplicitConversion_FromError_CallStackAsString_ContainsCallerInfo()
    {
        Result result = Result.Error("err");
        Assert.That(result.CallStackAsString, Does.Contain(nameof(ResultTests)));
    }

    // ── ToResult<T> ───────────────────────────────────────────────────────────

    [Test]
    public void ToResult_Generic_OnFailed_Succeeded_IsFalse()
    {
        Assert.That(Result.Fail("err").ToResult<int>().Succeeded, Is.False);
    }

    [Test]
    public void ToResult_Generic_OnFailed_CarriesErrorMessage()
    {
        var failed = Result.Fail("original error");
        Assert.That(failed.ToResult<int>().ErrorMessage, Is.EqualTo("original error"));
    }

    [Test]
    public void ToResult_Generic_OnFailed_PreservesException()
    {
        var ex = new Exception("ex");
        Assert.That(Result.Fail("error", ex).ToResult<int>().Exception, Is.SameAs(ex));
    }

    [Test]
    public void ToResult_Generic_OnFailed_PreservesCallers()
    {
        var failed = Result.Fail("error");
        Assert.That(failed.ToResult<int>().Callers.Count, Is.EqualTo(failed.Callers.Count));
    }

    // ── Exception error message format ────────────────────────────────────────

    [Test]
    public void Fail_Exception_ErrorMessage_HasAusnahmefehlerPrefix()
    {
        var result = Result.Fail(new Exception("boom"));
        Assert.That(result.ErrorMessage, Does.StartWith("Ausnahmefehler: "));
    }

    [Test]
    public void ImplicitConversion_FromException_ErrorMessage_HasAusnahmefehlerPrefix()
    {
        Result result = new Exception("boom");
        Assert.That(result.ErrorMessage, Does.StartWith("Ausnahmefehler: "));
    }

    // ── CallerInfo.MethodName is captured ─────────────────────────────────────

    [Test]
    public void Fail_String_CallerInfo_HasMethodName()
    {
        var result = Result.Fail("oops");
        Assert.That(result.Callers[0].MethodName, Is.Not.Empty);
    }

    // ── ToResult<T> on a succeeded Result ─────────────────────────────────────

    [Test]
    public void ToResult_Generic_OnSucceeded_Throws()
    {
        // ToResult<T>() is only valid on failed Results.
        // Calling it on a succeeded Result throws InvalidOperationException.
        var succeeded = Result.Success;
        Assert.That(() => succeeded.ToResult<int>(), Throws.InstanceOf<InvalidOperationException>());
    }
}
