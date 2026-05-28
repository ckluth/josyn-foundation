using NUnit.Framework;

namespace JOSYN.Foundation.PropertyBag.Test;

[TestFixture]
internal sealed class IniDictionarySerializerTests
{
    // ── Serialize(Dictionary<string, string>) ───────────────────────────────

    [Test]
    public void Serialize_Sectionless_ProducesKeyEqualsValueLines()
    {
        var data = new Dictionary<string, string> { ["Name"] = "Hello", ["Count"] = "3" };

        var result = IniDictionarySerializer.Serialize(data);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Name=Hello"));
        Assert.That(result.Value, Does.Contain("Count=3"));
    }

    [Test]
    public void Serialize_KeyWithWhitespace_KeyIsTrimmedInOutput()
    {
        var data = new Dictionary<string, string> { ["  TrimMe  "] = "value" };

        var result = IniDictionarySerializer.Serialize(data);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("TrimMe=value"));
        Assert.That(result.Value, Does.Not.Contain("  TrimMe"));
    }

    [Test]
    public void Serialize_ValueWithLeadingSpace_ValuePreservedExact()
    {
        // INI values are whitespace-exact — no trimming applied (invariant from session 0002 fix).
        var data = new Dictionary<string, string> { ["Key"] = " SpacedValue" };

        var result = IniDictionarySerializer.Serialize(data);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Key= SpacedValue"));
    }

    // ── Serialize(Dictionary<string, Dictionary<string, string>>) ───────────

    [Test]
    public void Serialize_SectionedDictionary_ProducesSectionHeaders()
    {
        var data = new Dictionary<string, Dictionary<string, string>>
        {
            ["Section1"] = new() { ["Key"] = "Val" }
        };

        var result = IniDictionarySerializer.Serialize(data);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("[Section1]"));
        Assert.That(result.Value, Does.Contain("Key=Val"));
    }

    // ── Deserialize ─────────────────────────────────────────────────────────

    [Test]
    public void Deserialize_BasicKeyValuePairs_ParsedCorrectly()
    {
        const string raw = "Alpha=one\nBeta=two";

        var result = IniDictionarySerializer.Deserialize(raw);

        Assert.That(result.Succeeded, Is.True);
        var section = result.Value![string.Empty];
        Assert.That(section["Alpha"], Is.EqualTo("one"));
        Assert.That(section["Beta"], Is.EqualTo("two"));
    }

    [Test]
    public void Deserialize_CommentLines_AreIgnored()
    {
        const string raw = ";this is a comment\nKey=value";

        var result = IniDictionarySerializer.Deserialize(raw);

        Assert.That(result.Succeeded, Is.True);
        var section = result.Value![string.Empty];
        Assert.That(section.ContainsKey(";this is a comment"), Is.False);
        Assert.That(section["Key"], Is.EqualTo("value"));
    }

    [Test]
    public void Deserialize_BlankLines_AreIgnored()
    {
        const string raw = "Key1=a\n\nKey2=b\n\n";

        var result = IniDictionarySerializer.Deserialize(raw);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value![string.Empty].Count, Is.EqualTo(2));
    }

    [Test]
    public void Deserialize_SectionedContent_ParsedIntoSeparateSections()
    {
        const string raw = "[SectionA]\nKey=A\n[SectionB]\nKey=B";

        var result = IniDictionarySerializer.Deserialize(raw);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!["SectionA"]["Key"], Is.EqualTo("A"));
        Assert.That(result.Value["SectionB"]["Key"], Is.EqualTo("B"));
    }

    [Test]
    public void Deserialize_DuplicateKeyInSectionlessContent_ReturnsFailure()
    {
        const string raw = "Key=first\nKey=second";

        var result = IniDictionarySerializer.Deserialize(raw);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Key"));
    }

    [Test]
    public void Deserialize_DuplicateKeyInSection_ReturnsFailure()
    {
        const string raw = "[S]\nKey=first\nKey=second";

        var result = IniDictionarySerializer.Deserialize(raw);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Key"));
    }

    [Test]
    public void Deserialize_ValueContainsEquals_ValuePreservedCorrectly()
    {
        // Split is limited to 2 parts, so a value containing '=' is kept intact.
        const string raw = "Url=http://x.com?a=1&b=2";

        var result = IniDictionarySerializer.Deserialize(raw);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value![string.Empty]["Url"], Is.EqualTo("http://x.com?a=1&b=2"));
    }

    [Test]
    public void Deserialize_ValueWithLeadingSpace_SpacePreservedExact()
    {
        // Deserialization does not trim values (invariant from session 0002 fix).
        const string raw = "Key= leading space";

        var result = IniDictionarySerializer.DeserializeSingleSection(raw);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!["Key"], Is.EqualTo(" leading space"));
    }

    // ── DeserializeSingleSection ────────────────────────────────────────────

    [Test]
    public void DeserializeSingleSection_SingleSection_ReturnsFlatDictionary()
    {
        const string raw = "Name=Test\nValue=123";

        var result = IniDictionarySerializer.DeserializeSingleSection(raw);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!["Name"], Is.EqualTo("Test"));
        Assert.That(result.Value["Value"], Is.EqualTo("123"));
    }

    [Test]
    public void DeserializeSingleSection_MultipleSections_ReturnsFailure()
    {
        const string raw = "[A]\nX=1\n[B]\nY=2";

        var result = IniDictionarySerializer.DeserializeSingleSection(raw);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("Multiple"));
    }

    [Test]
    public void DeserializeSingleSection_EmptyInput_ReturnsFailure()
    {
        var result = IniDictionarySerializer.DeserializeSingleSection(string.Empty);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("No sections"));
    }

    // ── Round-trip ──────────────────────────────────────────────────────────

    [Test]
    public void Serialize_ThenDeserializeSingleSection_RoundTrip_PreservesValues()
    {
        var data = new Dictionary<string, string> { ["X"] = "42", ["Y"] = "hello" };

        var serialized = IniDictionarySerializer.Serialize(data);
        Assert.That(serialized.Succeeded, Is.True);

        var result = IniDictionarySerializer.DeserializeSingleSection(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!["X"], Is.EqualTo("42"));
        Assert.That(result.Value["Y"], Is.EqualTo("hello"));
    }

    // ── Edge cases ──────────────────────────────────────────────────────────

    [Test]
    public void Deserialize_CrLfLineEndings_ParsedCorrectly()
    {
        const string raw = "Alpha=one\r\nBeta=two";

        var result = IniDictionarySerializer.Deserialize(raw);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value![string.Empty]["Alpha"], Is.EqualTo("one"));
        Assert.That(result.Value[string.Empty]["Beta"], Is.EqualTo("two"));
    }

    [Test]
    public void Deserialize_LineWithoutEqualsSign_IsIgnored()
    {
        const string raw = "ValidKey=value\nthis line has no equals sign\nOther=ok";

        var result = IniDictionarySerializer.Deserialize(raw);

        Assert.That(result.Succeeded, Is.True);
        var section = result.Value![string.Empty];
        Assert.That(section.ContainsKey("ValidKey"), Is.True);
        Assert.That(section.ContainsKey("Other"), Is.True);
        Assert.That(section.Count, Is.EqualTo(2));
    }
}
