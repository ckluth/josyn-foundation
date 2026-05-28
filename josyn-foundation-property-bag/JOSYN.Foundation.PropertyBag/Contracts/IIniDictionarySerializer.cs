using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Serializes and deserializes INI-formatted data to and from <c>Dictionary</c> representations.
/// </summary>
/// <remarks>
/// Supports both sectioned INI (<c>[SectionName]</c> header followed by <c>Key=Value</c> lines)
/// and section-less INI (plain <c>Key=Value</c> lines without a section header).
/// <para>
/// Values are stored verbatim — no trimming is applied to the right-hand side of the <c>=</c>.
/// A manually crafted INI entry such as <c>Key= value</c> captures the leading space as part of the
/// value. The caller is responsible for the exact content on both sides of the <c>=</c>.
/// </para>
/// <para>
/// Lines starting with <c>;</c> and blank lines are treated as comments or whitespace
/// and are ignored during deserialization.
/// </para>
/// <para>
/// All operations return <see cref="Result"/> or <see cref="Result{T}"/> — exceptions are
/// not propagated.
/// </para>
/// </remarks>
public interface IIniDictionarySerializer
{
    /// <summary>
    /// Serializes a multi-section INI dictionary to a string.
    /// </summary>
    /// <remarks>
    /// If the dictionary contains exactly one entry with the key <see cref="string.Empty"/>, the
    /// output is produced without a section header (section-less INI). Otherwise each entry key
    /// becomes a <c>[SectionName]</c> header followed by its key-value pairs, with a blank line
    /// between sections.
    /// </remarks>
    /// <param name="data">
    /// A dictionary mapping section names to their key-value pairs. Use an empty string as the
    /// section key to produce a section-less INI document.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the INI-formatted string on success, or a failure if
    /// serialization fails.
    /// </returns>
    static abstract Result<string> Serialize(Dictionary<string, Dictionary<string, string>> data);

    /// <summary>
    /// Serializes a flat key-value dictionary to a section-less INI string.
    /// </summary>
    /// <remarks>
    /// Each entry in <paramref name="data"/> becomes a <c>Key=Value</c> line. No section header is
    /// written. To include section headers, use
    /// <see cref="IIniDictionarySerializer.Serialize(Dictionary{string,Dictionary{string,string}})"/>.
    /// </remarks>
    /// <param name="data">The key-value pairs to serialize.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the section-less INI-formatted string on success, or a
    /// failure if serialization fails.
    /// </returns>
    static abstract Result<string> Serialize(Dictionary<string, string> data);

    /// <summary>
    /// Parses a section-less INI string into a flat key-value dictionary.
    /// </summary>
    /// <remarks>
    /// Helper method wrapping <see cref="IIniDictionarySerializer.Deserialize(string)"/> that enforces
    /// a single-section constraint. Use when the input is a simple <c>Key=Value</c> document without
    /// section headers — for example when deserializing output produced by
    /// <see cref="IIniDictionarySerializer.Serialize(Dictionary{string,string})"/>.
    /// </remarks>
    /// <param name="raw">
    /// A section-less INI string. May contain comment lines (starting with <c>;</c>) and blank lines.
    /// </param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the parsed flat dictionary on success, or a failure if
    /// the input is empty, contains no parseable sections, contains more than one section, or parsing
    /// fails.
    /// </returns>
    static abstract Result<Dictionary<string, string>> DeserializeSingleSection(string raw);

    /// <summary>
    /// Parses an INI string into a nested dictionary indexed by section name.
    /// </summary>
    /// <remarks>
    /// Lines before the first section header are collected under an empty section key.
    /// Duplicate keys within the same section produce a failure. Keys in different sections
    /// may repeat.
    /// </remarks>
    /// <param name="raw">The INI-formatted string to parse.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing the parsed nested dictionary on success, or a
    /// failure if a duplicate key is found or parsing fails.
    /// </returns>
    static abstract Result<Dictionary<string, Dictionary<string, string>>> Deserialize(string raw);
}
