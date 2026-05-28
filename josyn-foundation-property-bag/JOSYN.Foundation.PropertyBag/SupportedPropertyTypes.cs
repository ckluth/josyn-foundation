#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <summary>
/// Defines the set of property types that <see cref="PropertyBag"/> can serialize and deserialize.
/// </summary>
/// <remarks>
/// Nullable wrappers (<c>T?</c>) of every supported type are also accepted.
/// All <see langword="enum"/> types are supported regardless of their underlying type.
/// </remarks>
internal static class SupportedPropertyTypes
{
    /// <summary>
    /// Determines whether <paramref name="type"/> is a supported property type, including
    /// nullable wrappers and all <see langword="enum"/> types.
    /// </summary>
    /// <param name="type">The property type to check.</param>
    /// <returns>
    /// <see langword="true"/> if the type can be processed by <see cref="PropertyBag"/>;
    /// otherwise <see langword="false"/>.
    /// </returns>
    internal static bool IsMatch(Type type)
    {
        var targetType = Nullable.GetUnderlyingType(type) ?? type;
        return targetType.IsEnum || SupportedPropertyTypes.Types.Contains(targetType);
    }

    private static readonly HashSet<Type> Types =
    [
        typeof(string),
        typeof(bool),
        typeof(char),
        typeof(byte),
        typeof(sbyte),
        typeof(short),
        typeof(ushort),
        typeof(int),
        typeof(uint),
        typeof(long),
        typeof(ulong),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(Guid),
        
        // more when needed...
    ];
}
