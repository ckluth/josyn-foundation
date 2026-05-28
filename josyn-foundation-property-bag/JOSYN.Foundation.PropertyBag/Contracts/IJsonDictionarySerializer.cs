using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Serializes and deserializes JSON data to and from <c>Dictionary&lt;string, string&gt;</c> representations.
/// </summary>
/// <remarks>
/// Produces indented JSON with enum values as strings. Culture-aware
/// <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> instances are applied for
/// <see cref="System.DateTime"/>, <see cref="System.DateOnly"/>, <see cref="System.TimeOnly"/>, and
/// <see cref="decimal"/>, so values are formatted and parsed according to the current thread culture
/// (default: <c>de-DE</c>).
/// <para>
/// All operations return <see cref="Result"/> or <see cref="Result{T}"/> — exceptions are
/// not propagated.
/// </para>
/// </remarks>
public interface IJsonDictionarySerializer
{
    /// <summary>
    /// Serializes an arbitrary value to an indented JSON string using culture-aware converters.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="obj">The value to serialize.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the JSON string on success, or a failure if
    /// serialization fails.
    /// </returns>
    static abstract Result<string> Serialize<T>(T obj);

    /// <summary>
    /// Parses a JSON string into a flat <c>Dictionary&lt;string, string&gt;</c>.
    /// </summary>
    /// <remarks>
    /// The JSON must represent a flat object where every value is a JSON string, e.g.
    /// <c>{"Key": "Value"}</c>. Nested objects or non-string values are not supported
    /// and will produce a deserialization failure.
    /// </remarks>
    /// <param name="raw">The JSON string to parse.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the parsed dictionary on success, or a failure if the
    /// JSON is malformed or cannot be deserialized as a string-to-string dictionary.
    /// </returns>
    static abstract Result<Dictionary<string, string>> Deserialize(string raw);
}
