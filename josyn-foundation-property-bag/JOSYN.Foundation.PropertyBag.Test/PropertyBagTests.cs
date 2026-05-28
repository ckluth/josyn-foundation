using NUnit.Framework;

namespace JOSYN.Foundation.PropertyBag.Test;

#region Shared test types

public sealed record SimpleRecord
{
    public required string Name { get; init; }
    public int Count { get; init; }
}

// Primary-constructor (positional) style — no parameterless ctor
public sealed record PositionalRecord(string Title, int Value);
public sealed record PositionalNullableRecord(string Name, int? Count);

public sealed record NullablePropertiesRecord
{
    public required string RequiredName { get; init; }
    public string? OptionalName { get; init; }
    public int? OptionalCount { get; init; }
}

public sealed record UnsupportedTypeRecord
{
    public List<string> Items { get; init; } = [];
}

public enum Color { Red, Green, Blue }

public sealed record EnumRecord
{
    public Color Favorite { get; init; }
}

public sealed class PlainClass
{
    public string Name { get; set; } = "";
}

public sealed record DateTimeOffsetRecord
{
    public DateTimeOffset Timestamp { get; init; }
}

public sealed record AdditionalTypesRecord
{
    public bool Active { get; init; }
    public decimal Price { get; init; }
    public DateTime When { get; init; }
    public DateOnly Date { get; init; }
    public TimeOnly Time { get; init; }
    public Guid Id { get; init; }
    public TimeSpan Duration { get; init; }
}

#endregion

[TestFixture]
internal sealed class PropertyBagTests
{
    // ── Serialize<TRecord> ──────────────────────────────────────────────────

    [Test]
    public void Serialize_Record_WithIniSerializer_ProducesKeyValueLines()
    {
        var record = new SimpleRecord { Name = "JOSYN", Count = 42 };

        var result = PropertyBag.Serialize(record, IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Name=JOSYN"));
        Assert.That(result.Value, Does.Contain("Count=42"));
    }

    [Test]
    public void Serialize_Record_WithJsonSerializer_ProducesJsonObject()
    {
        var record = new SimpleRecord { Name = "JOSYN", Count = 42 };

        var result = PropertyBag.Serialize(record, JsonDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("\"Name\""));
        Assert.That(result.Value, Does.Contain("\"JOSYN\""));
    }

    [Test]
    public void Serialize_NonRecordClass_ReturnsFailureWithTypeName()
    {
        var plain = new PlainClass { Name = "test" };

        var result = PropertyBag.Serialize(plain, IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("PlainClass"));
    }

    [Test]
    public void Serialize_RecordWithUnsupportedPropertyType_ReturnsFailure()
    {
        var record = new UnsupportedTypeRecord { Items = ["a"] };

        var result = PropertyBag.Serialize(record, IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("UnsupportedTypeRecord"));
    }

    // ── Serialize(object, Type, ...) non-generic overload ──────────────────

    [Test]
    public void Serialize_NonGenericOverload_ProducesCorrectOutput()
    {
        var record = new SimpleRecord { Name = "Generic", Count = 7 };

        var result = PropertyBag.Serialize(record, typeof(SimpleRecord), IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Name=Generic"));
    }

    [Test]
    public void Serialize_NonGenericOverload_NonRecordType_ReturnsFailure()
    {
        var plain = new PlainClass { Name = "test" };

        var result = PropertyBag.Serialize(plain, typeof(PlainClass), IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.False);
    }

    // ── Deserialize<TRecord> with format auto-detection ─────────────────────

    [Test]
    public void Deserialize_IniInput_AutoDetectsFormatAndReturnsRecord()
    {
        const string ini = "Name=Hello\nCount=3";

        var result = PropertyBag.Deserialize<SimpleRecord>(ini);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Name, Is.EqualTo("Hello"));
        Assert.That(result.Value.Count, Is.EqualTo(3));
    }

    [Test]
    public void Deserialize_JsonInput_AutoDetectsFormatAndReturnsRecord()
    {
        const string json = """{"Name": "World", "Count": "5"}""";

        var result = PropertyBag.Deserialize<SimpleRecord>(json);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Name, Is.EqualTo("World"));
        Assert.That(result.Value.Count, Is.EqualTo(5));
    }

    [Test]
    public void Deserialize_NonRecordType_ReturnsFailureWithTypeName()
    {
        const string ini = "Name=test";

        var result = PropertyBag.Deserialize(ini, typeof(PlainClass));

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Does.Contain("PlainClass"));
    }

    [Test]
    public void Deserialize_MissingNonNullableProperty_ReturnsFailure()
    {
        const string ini = "Count=5";  // Name is required but absent

        var result = PropertyBag.Deserialize<SimpleRecord>(ini);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Deserialize_InvalidValueForPropertyType_FailureIsPropagatedWithMessage()
    {
        // Regression for bug fixed in session 0002: generic Deserialize<TRecord> was
        // returning Succeeded=true with a null Value when DeserializeFromDictionary failed.
        const string ini = "Name=Hello\nCount=not-a-number";

        var result = PropertyBag.Deserialize<SimpleRecord>(ini);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    // ── Enum support ────────────────────────────────────────────────────────

    [Test]
    public void Serialize_EnumProperty_SerializesAsName()
    {
        var record = new EnumRecord { Favorite = Color.Green };

        var result = PropertyBag.Serialize(record, IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Favorite=Green"));
    }

    [Test]
    public void Deserialize_EnumProperty_ParsedCaseInsensitively()
    {
        const string ini = "Favorite=green";

        var result = PropertyBag.Deserialize<EnumRecord>(ini);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Favorite, Is.EqualTo(Color.Green));
    }

    // ── Nullable property handling ──────────────────────────────────────────

    [Test]
    public void Deserialize_NullableProperty_MissingKeyIsAllowed()
    {
        const string ini = "RequiredName=Present";

        var result = PropertyBag.Deserialize<NullablePropertiesRecord>(ini);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.OptionalName, Is.Null);
        Assert.That(result.Value.OptionalCount, Is.Null);
    }

    [Test]
    public void Deserialize_NullableProperty_EmptyValueDeserializesAsNull()
    {
        const string ini = "RequiredName=Present\nOptionalName=\nOptionalCount=";

        var result = PropertyBag.Deserialize<NullablePropertiesRecord>(ini);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.OptionalName, Is.Null);
        Assert.That(result.Value.OptionalCount, Is.Null);
    }

    // ── Round-trips ─────────────────────────────────────────────────────────

    [Test]
    public void Serialize_ThenDeserialize_IniRoundTrip_PreservesValues()
    {
        var original = new SimpleRecord { Name = "RoundTrip", Count = 99 };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<SimpleRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Name, Is.EqualTo(original.Name));
        Assert.That(result.Value.Count, Is.EqualTo(original.Count));
    }

    [Test]
    public void Serialize_ThenDeserialize_JsonRoundTrip_PreservesValues()
    {
        var original = new SimpleRecord { Name = "RoundTrip", Count = 99 };

        var serialized = PropertyBag.Serialize(original, JsonDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<SimpleRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Name, Is.EqualTo(original.Name));
        Assert.That(result.Value.Count, Is.EqualTo(original.Count));
    }

    [Test]
    public void Serialize_ThenDeserialize_NullableNull_RoundTrip_PreservesNull()
    {
        var original = new NullablePropertiesRecord { RequiredName = "R", OptionalName = null, OptionalCount = null };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<NullablePropertiesRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.OptionalName, Is.Null);
        Assert.That(result.Value.OptionalCount, Is.Null);
    }

    // ── Primary-constructor (positional) records ────────────────────────────

    [Test]
    public void Serialize_PositionalRecord_ProducesKeyValueLines()
    {
        var record = new PositionalRecord("Copilot", 7);

        var result = PropertyBag.Serialize(record, IniDictionarySerializer.Serialize);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Title=Copilot"));
        Assert.That(result.Value, Does.Contain("Value=7"));
    }

    [Test]
    public void Deserialize_PositionalRecord_IniRoundTrip_PreservesValues()
    {
        var original = new PositionalRecord("RoundTrip", 42);

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<PositionalRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Title, Is.EqualTo("RoundTrip"));
        Assert.That(result.Value.Value, Is.EqualTo(42));
    }

    [Test]
    public void Deserialize_PositionalRecord_JsonRoundTrip_PreservesValues()
    {
        var original = new PositionalRecord("JSON", 99);

        var serialized = PropertyBag.Serialize(original, JsonDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<PositionalRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Title, Is.EqualTo("JSON"));
        Assert.That(result.Value.Value, Is.EqualTo(99));
    }

    [Test]
    public void Deserialize_PositionalNullableRecord_NullableParamMissingKey_IsAllowed()
    {
        const string ini = "Name=Present";

        var result = PropertyBag.Deserialize<PositionalNullableRecord>(ini);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Name, Is.EqualTo("Present"));
        Assert.That(result.Value.Count, Is.Null);
    }

    // ── DateTimeOffset support ──────────────────────────────────────────────

    [Test]
    public void Serialize_ThenDeserialize_DateTimeOffset_IniRoundTrip_PreservesValue()
    {
        var ts = new DateTimeOffset(2026, 5, 21, 20, 0, 0, TimeSpan.FromHours(2));
        var original = new DateTimeOffsetRecord { Timestamp = ts };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<DateTimeOffsetRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Timestamp, Is.EqualTo(ts));
    }

    // ── Additional supported types ──────────────────────────────────────────

    [Test]
    public void Serialize_ThenDeserialize_BoolProperty_IniRoundTrip_PreservesValue()
    {
        var original = new AdditionalTypesRecord { Active = true, Id = Guid.NewGuid() };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<AdditionalTypesRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Active, Is.True);
    }

    [Test]
    public void Serialize_ThenDeserialize_GuidProperty_IniRoundTrip_PreservesValue()
    {
        var id = Guid.NewGuid();
        var original = new AdditionalTypesRecord { Id = id };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<AdditionalTypesRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Id, Is.EqualTo(id));
    }

    [Test]
    public void Serialize_ThenDeserialize_TimeSpanProperty_IniRoundTrip_PreservesValue()
    {
        var duration = TimeSpan.FromHours(1.5);
        var original = new AdditionalTypesRecord { Duration = duration, Id = Guid.NewGuid() };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<AdditionalTypesRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Duration, Is.EqualTo(duration));
    }

    [Test]
    public void Serialize_ThenDeserialize_DecimalProperty_IniRoundTrip_PreservesValue()
    {
        var original = new AdditionalTypesRecord { Price = 1234.56m, Id = Guid.NewGuid() };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<AdditionalTypesRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Price, Is.EqualTo(1234.56m));
    }

    [Test]
    public void Serialize_Decimal_WithDeDECulture_UsesCommaAsDecimalSeparator()
    {
        var saved = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = JosynCulture.Default;
            var original = new AdditionalTypesRecord { Price = 1234.56m, Id = Guid.NewGuid() };

            var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);

            Assert.That(serialized.Succeeded, Is.True);
            Assert.That(serialized.Value, Does.Contain("1234,56"));
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = saved;
        }
    }

    [Test]
    public void Serialize_ThenDeserialize_DateTimeProperty_IniRoundTrip_PreservesValue()
    {
        var when = new DateTime(2026, 5, 25, 10, 30, 0);
        var original = new AdditionalTypesRecord { When = when, Id = Guid.NewGuid() };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<AdditionalTypesRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.When, Is.EqualTo(when));
    }

    [Test]
    public void Serialize_ThenDeserialize_DateOnlyProperty_IniRoundTrip_PreservesValue()
    {
        var date = new DateOnly(2026, 5, 25);
        var original = new AdditionalTypesRecord { Date = date, Id = Guid.NewGuid() };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<AdditionalTypesRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Date, Is.EqualTo(date));
    }

    [Test]
    public void Serialize_ThenDeserialize_TimeOnlyProperty_IniRoundTrip_PreservesValue()
    {
        var time = new TimeOnly(14, 30, 0);
        var original = new AdditionalTypesRecord { Time = time, Id = Guid.NewGuid() };

        var serialized = PropertyBag.Serialize(original, IniDictionarySerializer.Serialize);
        Assert.That(serialized.Succeeded, Is.True);

        var result = PropertyBag.Deserialize<AdditionalTypesRecord>(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Time, Is.EqualTo(time));
    }

    // ── Default-serializer overloads ────────────────────────────────────────

    [Test]
    public void Serialize_DefaultOverload_Generic_UsesIniFormat()
    {
        var record = new SimpleRecord { Name = "Default", Count = 1 };

        var result = PropertyBag.Serialize(record);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Name=Default"));
        Assert.That(result.Value, Does.Contain("Count=1"));
    }

    [Test]
    public void Serialize_DefaultOverload_ObjectAndType_UsesIniFormat()
    {
        var record = new SimpleRecord { Name = "ObjType", Count = 2 };

        var result = PropertyBag.Serialize(record, typeof(SimpleRecord));

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("Name=ObjType"));
        Assert.That(result.Value, Does.Contain("Count=2"));
    }
}
