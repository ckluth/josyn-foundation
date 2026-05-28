using NUnit.Framework;

namespace JOSYN.Foundation.PropertyBag.Test;

[TestFixture]
internal sealed class JsonDictionarySerializerTests
{
    [Test]
    public void Serialize_Dictionary_ProducesValidJsonWithKeyValuePairs()
    {
        var data = new Dictionary<string, string> { ["Name"] = "Test", ["Count"] = "7" };

        var result = JsonDictionarySerializer.Serialize(data);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Does.Contain("\"Name\""));
        Assert.That(result.Value, Does.Contain("\"Test\""));
        Assert.That(result.Value, Does.Contain("\"Count\""));
        Assert.That(result.Value, Does.Contain("\"7\""));
    }

    [Test]
    public void Deserialize_ValidJson_ReturnsDictionary()
    {
        const string json = """{"Name": "World", "Count": "5"}""";

        var result = JsonDictionarySerializer.Deserialize(json);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!["Name"], Is.EqualTo("World"));
        Assert.That(result.Value["Count"], Is.EqualTo("5"));
    }

    [Test]
    public void Deserialize_InvalidJson_ReturnsFailureWithMessage()
    {
        const string malformed = "not json at all";

        var result = JsonDictionarySerializer.Deserialize(malformed);

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Serialize_ThenDeserialize_RoundTrip_PreservesValues()
    {
        var original = new Dictionary<string, string> { ["Foo"] = "bar", ["Num"] = "42" };

        var serialized = JsonDictionarySerializer.Serialize(original);
        Assert.That(serialized.Succeeded, Is.True);

        var result = JsonDictionarySerializer.Deserialize(serialized.Value!);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!["Foo"], Is.EqualTo("bar"));
        Assert.That(result.Value["Num"], Is.EqualTo("42"));
    }

    [Test]
    public void Serialize_EmptyDictionary_ProducesEmptyJsonObject()
    {
        var result = JsonDictionarySerializer.Serialize(new Dictionary<string, string>());

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value!.Trim(), Does.StartWith("{").And.EndWith("}"));
    }

    [Test]
    public void Deserialize_EmptyJsonObject_ReturnsEmptyDictionary()
    {
        var result = JsonDictionarySerializer.Deserialize("{}");

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value, Is.Empty);
    }
}
