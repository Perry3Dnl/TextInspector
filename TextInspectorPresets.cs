using System.Text.Json;
using System.Text.Json.Serialization;

namespace TextInspector;

/// <summary>
/// Built-in scoring options and rules embedded in the assembly.
/// </summary>
public static class TextInspectorPresets
{
    public const string OptionsFileName = "options.json";
    public const string RulesFileName = "rules.json";
    public const string PoliciesFileName = "policies.json";

    internal static readonly JsonSerializerOptions Json = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly IReadOnlyDictionary<string,string> ResourceNames = DiscoverResources();

    public static TextInspectorScoringOptions LoadOptions()
        => Read<TextInspectorScoringOptions>(OptionsFileName);

    public static IReadOnlyList<TextInspectorRulePattern> LoadRules()
        => Read<List<TextInspectorRulePattern>>(RulesFileName);

    /// <summary>Composite policies require TextInspector Pro.</summary>
    public static IReadOnlyList<TextInspectorPolicy> LoadPolicies()
        => throw new NotSupportedException("Composite policies require TextInspector Pro.");

    /// <summary>Writing a configurable preset directory requires TextInspector Pro.</summary>
    public static void WriteMissing(string directory)
        => throw new NotSupportedException("Preset file persistence requires TextInspector Pro.");

    private static T Read<T>(string fileName)
    {
        using var stream = Open(fileName);
        return JsonSerializer.Deserialize<T>(stream,Json)
            ?? throw new InvalidOperationException($"Preset '{fileName}' was empty.");
    }

    private static Stream Open(string fileName)
    {
        if(!ResourceNames.TryGetValue(fileName,out var resourceName))
            throw new InvalidOperationException($"Preset resource for '{fileName}' was not found.");

        return typeof(TextInspectorPresets).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Preset resource '{resourceName}' was not found.");
    }

    private static IReadOnlyDictionary<string,string> DiscoverResources()
    {
        var assembly = typeof(TextInspectorPresets).Assembly;
        var names = assembly.GetManifestResourceNames();
        var output = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);

        foreach(var fileName in new[] { OptionsFileName, RulesFileName })
        {
            string suffix = ".Presets." + fileName;
            var matches = names
                .Where(name => name.EndsWith(suffix,StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if(matches.Length == 1)
            {
                output[fileName] = matches[0];
                continue;
            }

            if(matches.Length == 0)
                throw new InvalidOperationException($"Embedded preset '{fileName}' was not found (expected resource suffix '{suffix}').");

            throw new InvalidOperationException($"Multiple embedded resources match preset '{fileName}': {string.Join(", ",matches)}");
        }

        return output;
    }
}
