using NUnit.Framework;

namespace JOSYN.Foundation.ResultPattern.Test;

[TestFixture]
public sealed class ResultGenericTests
{

    // ── Success via implicit conversion ──────────────────────────────────────

    [Test]
    public void ImplicitConversion_FromValue_Succeeded_IsTrue()
    {
        Result<int> result = 42;
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void ImplicitConversion_FromValue_Value_IsSet()
    {
        Result<int> result = 42;
        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void ImplicitConversion_FromValue_ErrorMessage_IsNull()
    {
        Result<int> result = 42;
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    public void ImplicitConversion_FromValue_Exception_IsNull()
    {
        Result<int> result = 42;
        Assert.That(result.Exception, Is.Null);
    }

    [Test]
    public void ImplicitConversion_FromReferenceType_Value_IsSet()
    {
        Result<string> result = "hello";
        Assert.That(result.Value, Is.EqualTo("hello"));
    }

    // ── Result<T>.Success(value) ─────────────────────────────────────────────

    [Test]
    public void Success_Succeeded_IsTrue()
    {
        var result = Result<string>.Success("ok");
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void Success_Value_IsSet()
    {
        var result = Result<string>.Success("ok");
        Assert.That(result.Value, Is.EqualTo("ok"));
    }

    // ── Result<T>.Fail(string) ───────────────────────────────────────────────

    [Test]
    public void Fail_String_Succeeded_IsFalse()
    {
        var result = Result<int>.Fail("bad");
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void Fail_String_ErrorMessage_IsSet()
    {
        var result = Result<int>.Fail("bad");
        Assert.That(result.ErrorMessage, Is.EqualTo("bad"));
    }

    [Test]
    public void Fail_String_Value_IsDefault()
    {
        var result = Result<int>.Fail("bad");
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public void Fail_String_Value_IsNull_ForReferenceType()
    {
        var result = Result<string>.Fail("bad");
        Assert.That(result.Value, Is.Null);
    }

    [Test]
    public void Fail_StringAndException_Exception_IsSet()
    {
        var ex = new InvalidOperationException("boom");
        var result = Result<int>.Fail("bad", ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── Result<T>.Fail(Exception) ────────────────────────────────────────────

    [Test]
    public void Fail_Exception_Succeeded_IsFalse()
    {
        var result = Result<int>.Fail(new Exception("bang"));
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void Fail_Exception_ErrorMessage_ContainsExceptionMessage()
    {
        var result = Result<int>.Fail(new Exception("bang"));
        Assert.That(result.ErrorMessage, Does.Contain("bang"));
    }

    [Test]
    public void Fail_Exception_Exception_IsSet()
    {
        var ex = new Exception("bang");
        var result = Result<int>.Fail(ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    [Test]
    public void Fail_Exception_ErrorMessage_ContainsCallerInfo()
    {
        var result = Result<int>.Fail(new Exception("x"));
        Assert.That(result.CallStackAsString, Does.Contain(nameof(ResultGenericTests)));
    }

    // ── Implicit conversion from Exception ───────────────────────────────────

    [Test]
    public void ImplicitConversion_FromException_Succeeded_IsFalse()
    {
        Result<int> result = new Exception("implicit");
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void ImplicitConversion_FromException_Value_IsDefault()
    {
        Result<int> result = new Exception("implicit");
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public void ImplicitConversion_FromException_ErrorMessage_ContainsExceptionMessage()
    {
        Result<int> result = new Exception("implicit");
        Assert.That(result.ErrorMessage, Does.Contain("implicit"));
    }

    [Test]
    public void ImplicitConversion_FromException_Exception_IsSet()
    {
        var ex = new Exception("implicit");
        Result<int> result = ex;
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    [Test]
    public void ImplicitConversion_FromException_ErrorMessage_ContainsCallerInfo()
    {
        Result<int> result = new Exception("x");
        Assert.That(result.CallStackAsString, Does.Contain(nameof(ResultGenericTests)));
    }

    // ── Implicit conversion from Error ─────────────────────────────────────

    [Test]
    public void ImplicitConversion_FromError_Succeeded_IsFalse()
    {
        Error error = Result.Error("err");
        Result<int> result = error;
        Assert.That(result.Succeeded, Is.False);
    }

    [Test]
    public void ImplicitConversion_FromError_ErrorMessage_IsPreserved()
    {
        Result<int> result = Result.Error("err");
        Assert.That(result.ErrorMessage, Is.EqualTo("err"));
    }

    [Test]
    public void ImplicitConversion_FromError_Value_IsDefault()
    {
        Result<int> result = Result.Error("err");
        Assert.That(result.Value, Is.EqualTo(0));
    }

    [Test]
    public void ImplicitConversion_FromError_WithException_ExceptionIsPreserved()
    {
        var ex = new Exception("ex");
        Result<int> result = Result.Error("err", ex);
        Assert.That(result.Exception, Is.SameAs(ex));
    }

    // ── Error struct ────────────────────────────────────────────────────────

    [Test]
    public void Error_Constructor_FromString_ErrorMessage_IsSet()
    {
        Error error = new Error("something went wrong");
        Assert.That(error.ErrorMessage, Is.EqualTo("something went wrong"));
    }

    [Test]
    public void Error_Constructor_FromString_Exception_IsNull()
    {
        Error error = new Error("something went wrong");
        Assert.That(error.Exception, Is.Null);
    }

    [Test]
    public void Error_ImplicitConversion_FromException_ErrorMessage_ContainsExceptionMessage()
    {
        var ex = new InvalidOperationException("oops");
        Error error = ex;
        Assert.That(error.ErrorMessage, Does.Contain("oops"));
    }

    [Test]
    public void Error_ImplicitConversion_FromException_Exception_IsSet()
    {
        var ex = new InvalidOperationException("oops");
        Error error = ex;
        Assert.That(error.Exception, Is.SameAs(ex));
    }

    // ── record equality ───────────────────────────────────────────────────────

    [Test]
    public void TwoSuccessResults_WithSameValue_AreEqual()
    {
        Result<int> a = 7;
        Result<int> b = 7;
        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void TwoFailResults_WithSameMessage_HaveSameErrorMessage()
    {
        var a = Result<int>.Fail("same");
        var b = Result<int>.Fail("same");
        Assert.That(a.ErrorMessage, Is.EqualTo(b.ErrorMessage));
    }

    [Test]
    public void TwoFailResults_AreNotRecordEqual()
    {
        // Callers differ (different line numbers per call site) → records are not equal
        var a = Result<int>.Fail("same");
        var b = Result<int>.Fail("same");
        Assert.That(a, Is.Not.EqualTo(b));
    }

    // ── Caller capture ────────────────────────────────────────────────────────

    [Test]
    public void Success_Callers_IsEmpty()
    {
        Assert.That(Result<int>.Success(42).Callers, Is.Empty);
    }

    [Test]
    public void Success_CallStackAsString_ReturnsNoCallstackMessage()
    {
        Assert.That(Result<int>.Success(42).CallStackAsString, Is.EqualTo("(kein Callstack)"));
    }

    [Test]
    public void Fail_String_Callers_HasOneEntry()
    {
        Assert.That(Result<int>.Fail("oops").Callers.Count, Is.EqualTo(1));
    }

    [Test]
    public void Fail_String_CallStackAsString_ContainsCallerInfo()
    {
        Assert.That(Result<int>.Fail("oops").CallStackAsString, Does.Contain(nameof(ResultGenericTests)));
    }

    [Test]
    public void Fail_CallStackAsString_StartsWithAt()
    {
        Assert.That(Result<int>.Fail("oops").CallStackAsString, Does.StartWith("  at "));
    }

    [Test]
    public void ImplicitConversion_FromException_Callers_HasOneEntry()
    {
        Result<int> result = new Exception("x");
        Assert.That(result.Callers.Count, Is.EqualTo(1));
    }

    [Test]
    public void ImplicitConversion_FromException_CallerInfo_HasFilePath()
    {
        Result<int> result = new Exception("x");
        Assert.That(result.Callers[0].FilePath, Is.Not.Empty);
    }

    [Test]
    public void ImplicitConversion_FromException_CallerInfo_HasLineNumber()
    {
        Result<int> result = new Exception("x");
        Assert.That(result.Callers[0].LineNumber, Is.GreaterThan(0));
    }

    [Test]
    public void ImplicitConversion_FromError_Callers_HasOneEntry()
    {
        Result<int> result = Result.Error("err");
        Assert.That(result.Callers.Count, Is.EqualTo(1));
    }

    [Test]
    public void ImplicitConversion_FromError_CallStackAsString_ContainsCallerInfo()
    {
        Result<int> result = Result.Error("err");
        Assert.That(result.CallStackAsString, Does.Contain(nameof(ResultGenericTests)));
    }

    [Test]
    public void ImplicitConversion_FromValue_Zero_Succeeded_IsTrue()
    {
        Result<int> result = 0;
        Assert.That(result.Succeeded, Is.True);
    }

    [Test]
    public void ImplicitConversion_FromValue_Zero_Value_IsZero()
    {
        Result<int> result = 0;
        Assert.That(result.Value, Is.EqualTo(0));
    }

    // ── ToResult / ToResult<T> ────────────────────────────────────────────────

    [Test]
    public void ToResult_OnFailed_Succeeded_IsFalse()
    {
        Assert.That(Result<int>.Fail("err").ToResult().Succeeded, Is.False);
    }

    [Test]
    public void ToResult_OnFailed_CarriesErrorMessage()
    {
        var failed = Result<int>.Fail("original error");
        Assert.That(failed.ToResult().ErrorMessage, Is.EqualTo("original error"));
    }

    [Test]
    public void ToResult_OnFailed_PreservesException()
    {
        var ex = new Exception("ex");
        Assert.That(Result<int>.Fail("error", ex).ToResult().Exception, Is.SameAs(ex));
    }

    [Test]
    public void ToResult_OnFailed_PreservesCallers()
    {
        var failed = Result<int>.Fail("error");
        Assert.That(failed.ToResult().Callers.Count, Is.EqualTo(failed.Callers.Count));
    }

    [Test]
    public void ToResult_OnSucceeded_ReturnsSucceeded()
    {
        Result<int> succeeded = 42;
        Assert.That(succeeded.ToResult().Succeeded, Is.True);
    }

    [Test]
    public void ToResultGeneric_OnFailed_Succeeded_IsFalse()
    {
        Assert.That(Result<int>.Fail("err").ToResult<string>().Succeeded, Is.False);
    }

    [Test]
    public void ToResultGeneric_OnFailed_CarriesErrorMessage()
    {
        var failed = Result<int>.Fail("original error");
        Assert.That(failed.ToResult<string>().ErrorMessage, Is.EqualTo("original error"));
    }

    [Test]
    public void ToResultGeneric_OnFailed_PreservesCallers()
    {
        var failed = Result<int>.Fail("error");
        Assert.That(failed.ToResult<string>().Callers.Count, Is.EqualTo(failed.Callers.Count));
    }

    [Test]
    public void SuccessResult_AndFailResult_AreNotEqual()
    {
        Result<int> success = 1;
        var fail = Result<int>.Fail("err");
        Assert.That(success, Is.Not.EqualTo(fail));
    }

    // ── Exception error message format ────────────────────────────────────────

    [Test]
    public void Fail_Exception_ErrorMessage_HasAusnahmefehlerPrefix()
    {
        var result = Result<int>.Fail(new Exception("boom"));
        Assert.That(result.ErrorMessage, Does.StartWith("Ausnahmefehler: "));
    }

    [Test]
    public void ImplicitConversion_FromException_ErrorMessage_HasAusnahmefehlerPrefix()
    {
        Result<int> result = new Exception("boom");
        Assert.That(result.ErrorMessage, Does.StartWith("Ausnahmefehler: "));
    }

    // ── CallerInfo.MethodName is captured ─────────────────────────────────────

    [Test]
    public void Fail_String_CallerInfo_HasMethodName()
    {
        var result = Result<int>.Fail("oops");
        Assert.That(result.Callers[0].MethodName, Is.Not.Empty);
    }

    // ── ToResult<TOther> on a succeeded Result<T> ─────────────────────────────

    [Test]
    public void ToResultGeneric_OnSucceeded_ReturnsFailed()
    {
        // ToResult<TOther>() is only valid on failed Results.
        // Calling it on a succeeded Result returns a failed Result<TOther>.
        Result<int> succeeded = 42;
        var result = succeeded.ToResult<string>();
        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("succeeded"));
    }

    // ── Cross-type error conversion pattern ──────────────────────────────────

    [Test]
    public void ErrorConversionPattern_ToOtherType_Succeeded_IsFalse()
    {
        Assert.That(ErrorConversionPattern_Outer().Succeeded, Is.False);
    }

    [Test]
    public void ErrorConversionPattern_ToOtherType_ErrorMessage_IsPreserved()
    {
        Assert.That(ErrorConversionPattern_Outer().ErrorMessage, Is.EqualTo("inner failed"));
    }

    // ── Value-returning computation pattern ───────────────────────────────────

    [Test]
    public void ComputationPattern_ValidInput_Succeeded_IsTrue()
    {
        Assert.That(ParseInt("42").Succeeded, Is.True);
    }

    [Test]
    public void ComputationPattern_ValidInput_Value_IsSet()
    {
        Assert.That(ParseInt("42").Value, Is.EqualTo(42));
    }

    [Test]
    public void ComputationPattern_InvalidInput_Succeeded_IsFalse()
    {
        Assert.That(ParseInt("abc").Succeeded, Is.False);
    }

    [Test]
    public void ComputationPattern_InvalidInput_ErrorMessage_ContainsInput()
    {
        Assert.That(ParseInt("abc").ErrorMessage, Does.Contain("abc"));
    }

    // ── Pattern helpers ───────────────────────────────────────────────────────

    private static Result<string> ErrorConversionPattern_Outer()
    {
        var r = Result<int>.Fail("inner failed");
        if (!r.Succeeded)
            return Result.Error(r.ErrorMessage, r.Exception);
        return r.Value.ToString();
    }

    private static Result<int> ParseInt(string s) =>
        int.TryParse(s, out var n) ? n : Result.Error($"Kein Integer: {s}");
}
