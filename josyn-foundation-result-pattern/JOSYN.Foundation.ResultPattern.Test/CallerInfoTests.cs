using NUnit.Framework;

namespace JOSYN.Foundation.ResultPattern.Test;

[TestFixture]
public sealed class CallerInfoTests
{
    // ── ToString — with file and line ─────────────────────────────────────────

    [Test]
    public void ToString_WithFileAndLine_ReturnsClassDotMethodInFilenameColon()
    {
        var caller = new CallerInfo
        {
            MethodName = "DoSomething",
            ClassName  = "MyClass",
            FilePath   = @"C:\src\MyClass.cs",
            LineNumber = 42,
        };

        var text = caller.ToString();

        Assert.That(text, Is.EqualTo("MyClass.DoSomething() in MyClass.cs:42"));
    }

    // ── ToString — without usable file info ───────────────────────────────────

    [Test]
    public void ToString_WithEmptyFilePath_ReturnsClassDotMethodOnly()
    {
        var caller = new CallerInfo
        {
            MethodName = "DoSomething",
            ClassName  = "MyClass",
            FilePath   = "",
            LineNumber = 42,
        };

        var text = caller.ToString();

        Assert.That(text, Is.EqualTo("MyClass.DoSomething()"));
    }

    [Test]
    public void ToString_WithZeroLineNumber_ReturnsClassDotMethodOnly()
    {
        var caller = new CallerInfo
        {
            MethodName = "DoSomething",
            ClassName  = "MyClass",
            FilePath   = @"C:\src\MyClass.cs",
            LineNumber = 0,
        };

        var text = caller.ToString();

        Assert.That(text, Is.EqualTo("MyClass.DoSomething()"));
    }

    // ── ToString — default values ─────────────────────────────────────────────

    [Test]
    public void ToString_DefaultRecord_ReturnsDotsAndParens()
    {
        var caller = new CallerInfo();

        var text = caller.ToString();

        // Both FilePath and LineNumber are absent — falls back to "ClassName.MethodName()"
        Assert.That(text, Is.EqualTo(".()"));
    }
}
