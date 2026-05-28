using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130


/// <summary>
/// Represents a method that parses a serialized string into a flat string-to-string dictionary.
/// </summary>
/// <remarks>
/// Implementations included in this library:
/// <list type="bullet">
///   <item><see cref="IniDictionarySerializer.DeserializeSingleSection(string)"/> — parses section-less INI.</item>
///   <item><see cref="JsonDictionarySerializer.Deserialize(string)"/> — parses flat JSON.</item>
/// </list>
/// Automatic format detection (between these two) is built into
/// <see cref="PropertyBag.Deserialize{TRecord}(string)"/> and its overloads, so
/// callers generally do not need to select a <see cref="StringToDictionarySerializer"/> manually.
/// </remarks>
/// <param name="str">The serialized string to parse.</param>
/// <returns>
/// A <see cref="Result{T}"/> containing the parsed flat dictionary on success, or a failure if parsing fails.
/// </returns>
public delegate Result<Dictionary<string, string>> StringToDictionarySerializer(string str);
