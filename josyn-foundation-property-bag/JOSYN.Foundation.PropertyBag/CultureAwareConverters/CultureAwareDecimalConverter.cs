using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// A <see cref="JsonConverter{T}"/> for <see cref="decimal"/> that formats and parses values
/// according to the current thread culture (default: <c>de-DE</c>), preserving the
/// culture-specific decimal separator (e.g. <c>,</c> instead of <c>.</c>).
/// </summary>
internal sealed class CultureAwareDecimalConverter : JsonConverter<decimal>
{
    /// <inheritdoc/>
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => decimal.Parse(reader.GetString()!, CultureInfo.CurrentCulture);

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(CultureInfo.CurrentCulture));
}
