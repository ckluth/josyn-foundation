using System.Globalization;
using System.Reflection;
using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <inheritdoc cref="IPropertyBag"/>
public sealed class PropertyBag : IPropertyBag
{
    private static readonly DictionaryToStringSerializer DefaultDictionaryToStringSerializer = IniDictionarySerializer.Serialize;

    #region Serializer

    /// <inheritdoc/>
    public static Result<string> Serialize<TRecord>(TRecord record) where TRecord : class
    {
        return Serialize(record, typeof(TRecord));
    }

    /// <inheritdoc/>
    public static Result<string> Serialize(object record, Type recordType)
    {
        return Serialize(record, recordType, DefaultDictionaryToStringSerializer);
    }

    /// <inheritdoc cref="IPropertyBag.Serialize{TRecord}(TRecord, DictionaryToStringSerializer)"/>
    public static Result<string> Serialize<TRecord>(TRecord record, DictionaryToStringSerializer serializeToString) where TRecord : class
    {
        var getDict = SerializeToDictionary(record, typeof(TRecord));
        return getDict.Succeeded ? serializeToString(getDict.Value) : Result.Error(getDict.ErrorMessage, getDict.Exception);
    }

    /// <inheritdoc cref="IPropertyBag.Serialize(object, Type, DictionaryToStringSerializer)"/>
    public static Result<string> Serialize(object record, Type recordType, DictionaryToStringSerializer serializeToString)
    {
        var getDict = SerializeToDictionary(record, recordType);
        return getDict.Succeeded ? serializeToString(getDict.Value) : Result.Error(getDict.ErrorMessage, getDict.Exception);
    }

    #endregion

    #region Deserializer

    /// <inheritdoc cref="IPropertyBag.Deserialize(string, Type)"/>
    public static Result<object> Deserialize(string raw, Type recordType)
    {
        var serializer = DetectRequiredDeserializer(raw);
        return Deserialize(raw, recordType, serializer);
    }

    /// <inheritdoc cref="IPropertyBag.Deserialize{TRecord}(string)"/>
    public static Result<TRecord> Deserialize<TRecord>(string raw) where TRecord : class
    {
        var serializer = DetectRequiredDeserializer(raw);
        return Deserialize<TRecord>(raw, serializer);
    }

    /// <inheritdoc cref="IPropertyBag.Deserialize(string, ParameterInfo[])"/>
    public static Result<object[]> Deserialize(string raw, ParameterInfo[] parameters)
    {
        var serializer = DetectRequiredDeserializer(raw);
        return DeserializeParameters(parameters, raw, serializer);

        static Result<object[]> DeserializeParameters(ParameterInfo[] parameters, string raw, StringToDictionarySerializer deserializeToDictionary)
        {
            var getDict = deserializeToDictionary(raw);
            return getDict.Succeeded ? CreateInvocationArguments(parameters, getDict.Value) : Result.Error(getDict.ErrorMessage, getDict.Exception);

            static Result<object[]> CreateInvocationArguments(ParameterInfo[] parameters, Dictionary<string, string> arguments)
            {
                try
                {
                    var getArguments = parameters
                        .Select(p =>
                        {
                            var (found, rawValue) = TryGetArgumentCaseInsensitiveFirstChar(arguments, p.Name! /*double-checked safe!*/);
                            if (!found)
                                return Result.Error($"CreateInvocationArguments: Missing argument in Dictionary: {p.Name}");
                            var targetType = Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType;
                            return ConvertFromString(rawValue! /*safe!*/ , targetType);
                        })
                        .ToArray();
                    var fail = getArguments.FirstOrDefault(r => !r.Succeeded);

                    if (fail != null)
                        return Result.Error(fail.ErrorMessage!, fail.Exception);

                    return getArguments.Select(r => r.Value! /*double-checked safe!*/).ToArray();
                }
                catch (Exception ex) { return ex; }


                static (bool found, string? rawValue) TryGetArgumentCaseInsensitiveFirstChar(Dictionary<string, string> arguments, string name)
                {
                    if (name == string.Empty)
                        return (false, null);

                    if (arguments.TryGetValue(name, out var value))
                        return (true, value);

                    var toggled = char.IsUpper(name[0])
                        ? char.ToLower(name[0]) + name[1..]
                        : char.ToUpper(name[0]) + name[1..];

                    return arguments.TryGetValue(toggled, out value)
                        ? (true, value)
                        : (false, null);
                }
            }
        }
    }

    #endregion

    #region private

    private static Result<Dictionary<string, string>> SerializeToDictionary(object record, Type recordType)
    {
        try
        {
            if (!IsRecord(recordType))
                return Result.Error($"{recordType.Name} must be a class record.");

            var properties = recordType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var unsupported = properties.Where(p => !SupportedPropertyTypes.IsMatch(p.PropertyType)).Select(p => $"{p.Name}: {p.PropertyType.Name}").ToList();

            if (unsupported.Count > 0)
                return Result.Error($"Unsupported property types in {recordType.Name}: {string.Join(", ", unsupported)}");

            var result = properties.ToDictionary(
                p => p.Name,
                p =>
                {
                    var value = p.GetValue(record);
                    return value switch
                    {
                        null => string.Empty,
                        _ => Convert.ToString(value, CultureInfo.CurrentCulture) ?? string.Empty
                    };
                });
            return result;
        }
        catch (Exception ex) { return ex; }
    }

    private static Result<object> DeserializeFromDictionary(Dictionary<string, string> raw, Type recordType)
    {
        if (!IsRecord(recordType))
            return Result.Error($"{recordType.Name} must be a class record.");
        try
        {
            var properties = recordType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var unsupported = properties.Where(p => !SupportedPropertyTypes.IsMatch(p.PropertyType)).Select(p => $"{p.Name}: {p.PropertyType.Name}").ToList();
            if (unsupported.Count > 0) return Result.Error($"Unsupported property types in {recordType.Name}: {string.Join(", ", unsupported)}");

            var paramlessCtor = recordType.GetConstructor(Type.EmptyTypes);

            if (paramlessCtor != null)
            {
                // init-property style: create instance then set each property
                var instance = Activator.CreateInstance(recordType)!;
                foreach (var prop in properties)
                {
                    var (targetType, isNullable) = GetNullableTypeInfo(prop);
                    object? converted = null;
                    if (!raw.TryGetValue(prop.Name, out var rawValue))
                    {
                        if (isNullable)
                            continue;
                        return Result.Error($"Non-Nullable Property ist nicht im Dictionary: {prop.Name}");
                    }
                    if (isNullable && rawValue == string.Empty)
                        converted = null;
                    else
                    {
                        var conversion = ConvertFromString(rawValue, targetType);
                        if (conversion.Succeeded)
                            converted = conversion.Value;
                        else
                            return Result<object>.Propagate(conversion.ToResult<object>());
                    }
                    prop.SetValue(instance, converted);
                }
                return instance;
            }

            // primary-constructor (positional) style: find matching ctor and invoke with args
            var ctors = recordType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var primaryCtor = ctors
                .OrderByDescending(c => c.GetParameters().Length)
                .FirstOrDefault(c => c.GetParameters().All(p =>
                {
                    if (p.Name == null) return false;
                    // nullable params may be absent from the dictionary
                    var isNullableParam = Nullable.GetUnderlyingType(p.ParameterType) != null
                        || (!p.ParameterType.IsValueType && new NullabilityInfoContext().Create(p).WriteState == NullabilityState.Nullable);
                    if (isNullableParam) return true;
                    return raw.Keys.Any(k => string.Equals(k, p.Name, StringComparison.OrdinalIgnoreCase));
                }));

            if (primaryCtor == null)
                return Result.Error($"Kein passender Konstruktor für {recordType.Name} gefunden.");

            var ctorParams = primaryCtor.GetParameters();
            var args = new object?[ctorParams.Length];
            for (var i = 0; i < ctorParams.Length; i++)
            {
                var param = ctorParams[i];
                var (targetType, isNullable) = GetNullableTypeInfoFromParam(param);

                var matchedKey = raw.Keys.FirstOrDefault(k =>
                    string.Equals(k, param.Name, StringComparison.OrdinalIgnoreCase));

                if (matchedKey == null)
                {
                    if (isNullable)
                    {
                        args[i] = null;
                        continue;
                    }
                    return Result.Error($"Non-Nullable Konstruktorparameter ist nicht im Dictionary: {param.Name}");
                }

                var rawValue = raw[matchedKey];
                if (isNullable && rawValue == string.Empty)
                {
                    args[i] = null;
                    continue;
                }

                var conversion = ConvertFromString(rawValue, targetType);
                if (!conversion.Succeeded)
                    return Result<object>.Propagate(conversion.ToResult<object>());
                args[i] = conversion.Value;
            }

            return primaryCtor.Invoke(args);

        }
        catch (Exception ex) { return ex; }


        static (Type targetType, bool isNullable) GetNullableTypeInfo(PropertyInfo property)
        {
            var type = property.PropertyType;
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null) return (underlying, true);
            if (type.IsValueType) return (type, false);
            var ctx = new NullabilityInfoContext();
            var info = ctx.Create(property);
            var isNullable = info.WriteState == NullabilityState.Nullable || info.ReadState == NullabilityState.Nullable;
            return (type, isNullable);
        }

        static (Type targetType, bool isNullable) GetNullableTypeInfoFromParam(ParameterInfo param)
        {
            var type = param.ParameterType;
            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null) return (underlying, true);
            if (type.IsValueType) return (type, false);
            var ctx = new NullabilityInfoContext();
            var info = ctx.Create(param);
            var isNullable = info.WriteState == NullabilityState.Nullable || info.ReadState == NullabilityState.Nullable;
            return (type, isNullable);
        }
    }

    private static Result<object> Deserialize(string raw, Type recordType, StringToDictionarySerializer deserializeToDictionary)
    {
        var getDict = deserializeToDictionary(raw);
        return getDict.Succeeded ? DeserializeFromDictionary(getDict.Value, recordType) : Result<object>.Propagate(getDict.ToResult<object>());
    }

    private static Result<TRecord> Deserialize<TRecord>(string raw, StringToDictionarySerializer deserializeToDictionary) where TRecord : class
    {
        var getDict = deserializeToDictionary(raw);
        if (!getDict.Succeeded) return Result<TRecord>.Propagate(getDict.ToResult<TRecord>());

        var getRecord = DeserializeFromDictionary(getDict.Value, typeof(TRecord));
        if (!getRecord.Succeeded) return Result<TRecord>.Propagate(getRecord.ToResult<TRecord>());

        return (getRecord.Value as TRecord)!;
    }

    // <Clone>$ is a compiler-generated method present on all C# record class types.
    // It is not part of the language specification, but has been stable across all
    // C# compiler versions since records were introduced. Technically fragile; practically safe.
    private static bool IsRecord(Type type) => type.GetMethod("<Clone>$") is not null;

    private static Result<object?> ConvertFromString(string rawValue, Type targetType)
    {
        try
        {
            if (targetType.IsEnum)
                return Enum.Parse(targetType, rawValue, ignoreCase: true);
            if (targetType == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(rawValue, CultureInfo.CurrentCulture);
            if (targetType == typeof(DateOnly))
                return DateOnly.Parse(rawValue);
            if (targetType == typeof(TimeOnly))
                return TimeOnly.Parse(rawValue);
            if (targetType == typeof(Guid))
                return Guid.Parse(rawValue);
            if (targetType == typeof(TimeSpan))
                return TimeSpan.Parse(rawValue);

            return Convert.ChangeType(rawValue, targetType);

        }
        catch (Exception ex) { return ex; }
    }

    private static StringToDictionarySerializer DetectRequiredDeserializer(string raw)
    {
        // a little bit lazy - but for now...

        if (raw.Trim().StartsWith('{'))
            return JsonDictionarySerializer.Deserialize;

        return IniDictionarySerializer.DeserializeSingleSection;
    }

    #endregion
}
