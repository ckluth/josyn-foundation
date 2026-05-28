using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <inheritdoc cref="IJsonDictionarySerializer"/>
public static class JsonDictionarySerializer
{
    /// <inheritdoc cref="IJsonDictionarySerializer.Serialize{T}(T)"/>
    public static Result<string> Serialize<T>(T obj)
    {
        try
        {
            return JsonSerializer.Serialize(obj, _cultureAwareOptions);
        }
        catch (Exception ex) { return ex; }
    }

    /// <inheritdoc cref="IJsonDictionarySerializer.Deserialize(string)"/>
    public static Result<Dictionary<string, string>> Deserialize(string raw)
    {
        try
        {
            var result = Deserialize<Dictionary<string, string>>(raw);
            if (!result.Succeeded)
                return Result<Dictionary<string, string>>.Propagate(result);
            
            return result.Value;
        }
        catch (Exception ex) { return ex; }
    }


    #region private

    // JsonSerializerOptions is expensive to construct — cache it.
    // The culture-aware converters read CultureInfo.CurrentCulture at call time, so caching is safe.
    private static readonly JsonSerializerOptions _cultureAwareOptions = CreateCultureAwareOptions();

    private static Result<T> Deserialize<T>(string json)
    {
        try
        {
            var result = JsonSerializer.Deserialize<T>(json, _cultureAwareOptions);
            return result == null ? Result<T>.Fail("JsonSerializer.Deserialize<T> returned null.") : Result<T>.Success(result);
        }
        catch (Exception ex) { return ex; }
    }
    
    private static JsonSerializerOptions CreateCultureAwareOptions()
    {
        var baseOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        };
        baseOptions.Converters.Add(new CultureAwareDateTimeConverter());
        baseOptions.Converters.Add(new CultureAwareDecimalConverter());
        baseOptions.Converters.Add(new CultureAwareDateOnlyConverter());
        baseOptions.Converters.Add(new CultureAwareTimeOnlyConverter());
        return baseOptions;
    }
    #endregion
}
