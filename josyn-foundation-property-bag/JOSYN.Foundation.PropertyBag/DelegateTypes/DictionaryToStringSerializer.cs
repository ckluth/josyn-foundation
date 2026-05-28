using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Represents a method that converts a flat string-to-string dictionary into a serialized string.
/// </summary>
/// <remarks>
/// Implementations included in this library:
/// <list type="bullet">
///   <item><see cref="IniDictionarySerializer.Serialize(Dictionary{string,string})"/> — produces section-less INI.</item>
///   <item><see cref="JsonDictionarySerializer.Serialize{T}(T)"/> — produces indented JSON.</item>
/// </list>
/// Pass an implementation to <see cref="PropertyBag.Serialize{TRecord}(TRecord, DictionaryToStringSerializer)"/>
/// or <see cref="PropertyBag.Serialize(object, Type, DictionaryToStringSerializer)"/> to choose the output format.
/// </remarks>
/// <param name="data">The flat key-value dictionary to serialize.</param>
/// <returns>
/// A <see cref="Result{T}"/> containing the serialized string on success, or a failure if serialization fails.
/// </returns>
public delegate Result<string> DictionaryToStringSerializer(Dictionary<string, string> data);
