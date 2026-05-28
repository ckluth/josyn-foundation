using System.Reflection;
using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Serializes and deserializes C# <c>record class</c> types to and from string-based formats.
/// </summary>
/// <remarks>
/// Supported formats are section-less INI (<c>Key=Value</c> lines) and JSON. During
/// deserialization the format is detected automatically: if the first non-whitespace character
/// is <c>{</c>, JSON is assumed; otherwise INI.
/// <para>
/// Only <c>record class</c> types are accepted (detected at runtime via the compiler-generated
/// <c>&lt;Clone&gt;$</c> method). Property types must belong to the set of supported primitive
/// and well-known BCL types.
/// </para>
/// <para>
/// All operations return <see cref="Result"/> or <see cref="Result{T}"/> — exceptions are
/// not propagated.
/// </para>
/// </remarks>
public interface IPropertyBag
{
    /// <summary>
    /// Serializes a <c>record class</c> instance to a section-less INI string using the default format.
    /// </summary>
    /// <remarks>
    /// Overload using <see cref="IniDictionarySerializer"/> as the serializer.
    /// To specify a particular output format, use
    /// <see cref="IPropertyBag.Serialize{TRecord}(TRecord, DictionaryToStringSerializer)"/> instead.
    /// </remarks>
    /// <typeparam name="TRecord">The record type to serialize. Must be a <c>record class</c>.</typeparam>
    /// <param name="record">The record instance to serialize.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the serialized INI string on success, or a failure if the
    /// type is not a <c>record class</c>, contains unsupported property types, or serialization fails.
    /// </returns>
    static abstract Result<string> Serialize<TRecord>(TRecord record)
        where TRecord : class;


    /// <summary>
    /// Serializes a <c>record class</c> instance passed as <see cref="object"/> to a
    /// section-less INI string using the default format.
    /// </summary>
    /// <remarks>
    /// Overload using <see cref="IniDictionarySerializer"/> as the serializer.
    /// Use this overload when the concrete record type is only known at runtime. For
    /// compile-time-known types, prefer <see cref="IPropertyBag.Serialize{TRecord}(TRecord)"/>.
    /// </remarks>
    /// <param name="record">The record instance to serialize.</param>
    /// <param name="recordType">The exact <see cref="Type"/> of the record. Must be a <c>record class</c>.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the serialized INI string on success, or a failure if
    /// <paramref name="recordType"/> is not a <c>record class</c>, contains unsupported property types,
    /// or serialization fails.
    /// </returns>
    static abstract Result<string> Serialize(object record, Type recordType);


    /// <summary>
    /// Serializes a <c>record class</c> instance to a string using the specified format serializer.
    /// </summary>
    /// <typeparam name="TRecord">
    /// The record type to serialize. Must be a <c>record class</c>.
    /// </typeparam>
    /// <param name="record">The record instance to serialize.</param>
    /// <param name="serializeToString">
    /// A <see cref="DictionaryToStringSerializer"/> delegate that converts the intermediate flat
    /// <c>Dictionary&lt;string, string&gt;</c> into the final string.
    /// Use <see cref="IniDictionarySerializer.Serialize(Dictionary{string,string})"/> for INI,
    /// or <see cref="JsonDictionarySerializer.Serialize{T}(T)"/> for JSON.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the serialized string on success, or a failure if the
    /// type is not a <c>record class</c>, contains unsupported property types, or serialization fails.
    /// </returns>
    static abstract Result<string> Serialize<TRecord>(TRecord record, DictionaryToStringSerializer serializeToString)
        where TRecord : class;

    /// <summary>
    /// Serializes a <c>record class</c> instance passed as <see cref="object"/> to a string using
    /// the specified format serializer.
    /// </summary>
    /// <remarks>
    /// Use this overload when the concrete record type is only known at runtime. For
    /// compile-time-known types, prefer <see cref="IPropertyBag.Serialize{TRecord}(TRecord, DictionaryToStringSerializer)"/>.
    /// </remarks>
    /// <param name="record">The record instance to serialize.</param>
    /// <param name="recordType">
    /// The exact <see cref="Type"/> of the record. Must be a <c>record class</c>.
    /// </param>
    /// <param name="serializeToString">
    /// A <see cref="DictionaryToStringSerializer"/> delegate that converts the intermediate flat
    /// <c>Dictionary&lt;string, string&gt;</c> into the final string.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the serialized string on success, or a failure if
    /// <paramref name="recordType"/> is not a <c>record class</c>, contains unsupported property types,
    /// or serialization fails.
    /// </returns>
    static abstract Result<string> Serialize(object record, Type recordType, DictionaryToStringSerializer serializeToString);

    /// <summary>
    /// Auto-detects the string format (INI or JSON) and deserializes the input into
    /// an instance of <paramref name="recordType"/>, returned as <see cref="object"/>.
    /// </summary>
    /// <remarks>
    /// Use this overload when the target type is only known at runtime. For
    /// compile-time-known types, prefer <see cref="IPropertyBag.Deserialize{TRecord}(string)"/>.
    /// </remarks>
    /// <param name="raw">
    /// The serialized string to parse. Must be in section-less INI or JSON format.
    /// </param>
    /// <param name="recordType">The target <c>record class</c> type to deserialize into.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the deserialized record as <see cref="object"/> on success,
    /// or a failure if format detection, parsing, type validation, or property conversion fails.
    /// </returns>
    static abstract Result<object> Deserialize(string raw, Type recordType);

    /// <summary>
    /// Auto-detects the string format (INI or JSON) and deserializes the input into
    /// a typed <typeparamref name="TRecord"/> instance.
    /// </summary>
    /// <typeparam name="TRecord">
    /// The target <c>record class</c> type. Must have a parameterless constructor (all
    /// compiler-generated record classes satisfy this requirement).
    /// </typeparam>
    /// <param name="raw">
    /// The serialized string to parse. Must be in section-less INI or JSON format.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the deserialized <typeparamref name="TRecord"/> on success,
    /// or a failure if format detection, parsing, type validation, or property conversion fails.
    /// </returns>
    static abstract Result<TRecord> Deserialize<TRecord>(string raw)
        where TRecord : class;

    /// <summary>
    /// Auto-detects the string format (INI or JSON) and deserializes the input into
    /// an array of converted method-invocation arguments.
    /// </summary>
    /// <remarks>
    /// Keys in the serialized data are matched against parameter names case-insensitively on the
    /// first character. Nullable parameters absent from the data are silently set to
    /// <see langword="null"/>. Non-nullable parameters that are missing produce a failure.
    /// <para>
    /// The returned array is positionally aligned with <paramref name="parameters"/> and can be
    /// passed directly to <see cref="System.Reflection.MethodBase.Invoke(object?, object?[])"/>.
    /// </para>
    /// </remarks>
    /// <param name="raw">
    /// The serialized string to parse. Must be in section-less INI or JSON format.
    /// </param>
    /// <param name="parameters">
    /// The method parameter descriptors against which the parsed key-value pairs are matched.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing an <see cref="object"/>[] of converted argument values
    /// on success, or a failure if a required argument is missing or a type conversion fails.
    /// </returns>
    static abstract Result<object[]> Deserialize(string raw, ParameterInfo[] parameters);
}
