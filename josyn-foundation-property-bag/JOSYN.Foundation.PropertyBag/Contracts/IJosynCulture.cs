using System.Globalization;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Contract definition for the canonical JOSYN process culture.
/// Defines the culture used for all serializations (PropertyBag INI/JSON,
/// numbers, dates) throughout the JOSYN ecosystem.
/// </summary>
/// <remarks>
/// The culture is hard-wired at compile time — never changeable as a runtime configuration,
/// because a culture mismatch between writer and reader leads to silent data corruption.
/// </remarks>
public interface IJosynCulture
{
    /// <summary>
    /// The canonical culture for all JOSYN processes. Currently <c>de-DE</c>.
    /// </summary>
    static abstract CultureInfo Default { get; }
}
