using JOSYN.Foundation.ResultPattern;

#pragma warning disable IDE0130
namespace JOSYN.Foundation.PropertyBag;
#pragma warning restore IDE0130

/// <inheritdoc cref="IIniDictionarySerializer"/>
public static class IniDictionarySerializer
{
    /// <inheritdoc cref="IIniDictionarySerializer.Serialize(Dictionary{string,Dictionary{string,string}})"/>
    public static Result<string> Serialize(Dictionary<string, Dictionary<string, string>> data)
    {
        try
        {
            using var writer = new StringWriter();
            if (IsSectionless(data))
            {
                foreach (var kvp in data[string.Empty])
                    writer.WriteLine($"{kvp.Key.Trim()}={kvp.Value}");
            }
            else
            {
                foreach (var section in data)
                {
                    writer.WriteLine($"[{section.Key.Trim()}]");
                    foreach (var kvp in section.Value)
                        writer.WriteLine($"{kvp.Key.Trim()}={kvp.Value}");
                    writer.WriteLine();
                }
            }
            return writer.ToString();
        }
        catch (Exception ex) { return ex; }
    }
    
    /// <inheritdoc cref="IIniDictionarySerializer.Serialize(Dictionary{string,string})"/>
    public static Result<string> Serialize(Dictionary<string, string> data)
    {
        var d = new Dictionary<string, Dictionary<string, string>> { {"", data} };
        var res = Serialize(d);
        return res;
    }
    
    /// <inheritdoc cref="IIniDictionarySerializer.DeserializeSingleSection(string)"/>
    public static Result<Dictionary<string, string>> DeserializeSingleSection(string raw)
    {
        var d = Deserialize(raw);
        
        if (!d.Succeeded)
            return Result<Dictionary<string, string>>.Propagate(d.ToResult<Dictionary<string, string>>());
        
        return d.Value.Count switch
        {
            0 => Result.Error("No sections found."),
            > 1 => Result.Error("Multiple sections found."),
            _ => d.Value.First().Value
        };
    }

    /// <inheritdoc cref="IIniDictionarySerializer.Deserialize(string)"/>
    public static Result<Dictionary<string, Dictionary<string, string>>> Deserialize(string raw)
    {
        try
        {
            var data = new Dictionary<string, Dictionary<string, string>>();
            string? currentSection = null;
            var lines = raw.Split(["\r\n", "\n"], StringSplitOptions.None);
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(';'))
                    continue;
                if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']'))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    data[currentSection] = new Dictionary<string, string>();
                }
                else
                {
                    if (currentSection == null)
                    {
                        currentSection = string.Empty;
                        data[currentSection] = new Dictionary<string, string>();
                    }
                    var keyValue = trimmedLine.Split(['='], 2);
                    if (keyValue.Length != 2) continue;
                    
                    
                    var key = keyValue[0].Trim();
                    var value = keyValue[1];

                    if (data[currentSection].ContainsKey(key))
                    {
                        return string.IsNullOrEmpty(currentSection) 
                            ? Result.Error($"Duplicate key '{key}'.") 
                            : Result.Error($"Duplicate key '{key}' in section '[{currentSection}]'.");
                    }

                    data[currentSection][key] = value;
                }
            }
            return data;
        }
        catch (Exception ex) { return ex; }
    }
    
    private static bool IsSectionless(Dictionary<string, Dictionary<string, string>> data) =>
        data.Count == 1 && data.ContainsKey(string.Empty);
}
