using System.Globalization;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <inheritdoc cref="IJosynCulture"/>
public static class JosynCulture
{
    /// <inheritdoc cref="IJosynCulture.Default"/>
    public static readonly CultureInfo Default = CultureInfo.GetCultureInfo("de-DE");
}
