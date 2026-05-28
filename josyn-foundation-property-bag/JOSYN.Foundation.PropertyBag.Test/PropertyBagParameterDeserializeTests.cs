using System.Reflection;
using NUnit.Framework;

namespace JOSYN.Foundation.PropertyBag.Test;

[TestFixture]
internal sealed class PropertyBagParameterDeserializeTests
{
    // Helper methods — their parameter metadata is the test input.
    private static void ThreeParamMethod(string name, int count, DateOnly date) { }
    private static void SingleStringMethod(string value) { }

    private static ParameterInfo[] GetParams(string methodName) =>
        typeof(PropertyBagParameterDeserializeTests)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static)!
            .GetParameters();

    [Test]
    public void Deserialize_Parameters_AllPresent_ReturnsFilledArray()
    {
        var date = new DateOnly(2024, 6, 15);
        string ini = $"name=Test\ncount=10\ndate={date:dd.MM.yyyy}";

        var result = PropertyBag.Deserialize(ini, GetParams(nameof(ThreeParamMethod)));

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value![0], Is.EqualTo("Test"));
        Assert.That(result.Value[1], Is.EqualTo(10));
        Assert.That(result.Value[2], Is.EqualTo(date));
    }

    [Test]
    public void Deserialize_Parameters_MissingArgument_ReturnsFailure()
    {
        const string ini = "name=Test";  // count and date are absent

        var result = PropertyBag.Deserialize(ini, GetParams(nameof(ThreeParamMethod)));

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Deserialize_Parameters_CaseInsensitiveFirstChar_UppercaseKeyMatchesLowercaseParam()
    {
        // Parameter is named "value" (lowercase); INI key is "Value" (uppercase first char).
        // TryGetArgumentCaseInsensitiveFirstChar toggles the first character as a fallback.
        const string ini = "Value=hello";

        var result = PropertyBag.Deserialize(ini, GetParams(nameof(SingleStringMethod)));

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.Value![0], Is.EqualTo("hello"));
    }

    [Test]
    public void Deserialize_Parameters_InvalidValueForType_ReturnsFailure()
    {
        const string ini = "name=Test\ncount=not-a-number\ndate=01.01.2024";

        var result = PropertyBag.Deserialize(ini, GetParams(nameof(ThreeParamMethod)));

        Assert.That(result.Succeeded, Is.False);
        Assert.That(result.ErrorMessage, Is.Not.Null.And.Not.Empty);
    }
}
