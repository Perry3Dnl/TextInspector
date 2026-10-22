using System.Globalization;
using System.Text.RegularExpressions;

namespace TextInspector;

// Shared by Free and Pro. Custom rule configuration remains a Pro capability.
public sealed partial class TextInspectorEngine
{
    private static bool TryCompileNumericConditions(
        TextInspectorRulePattern rule,
        Regex regex,
        List<TextInspectorDiagnostic> diagnostics,
        out RuntimeNumericCondition[] conditions)
    {
        conditions = Array.Empty<RuntimeNumericCondition>();
        if(rule.NumericConditions is null)
        {
            diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.InvalidNumericCondition,rule,
                "NumericConditions cannot be null; use an empty array for no numeric conditions."));
            return false;
        }

        if(rule.NumericConditions.Length == 0)
            return true;

        var compiled = new List<RuntimeNumericCondition>(rule.NumericConditions.Length);
        foreach(var condition in rule.NumericConditions)
        {
            if(condition is null || string.IsNullOrWhiteSpace(condition.CaptureGroup)
                || !regex.GetGroupNames().Contains(condition.CaptureGroup.Trim(),StringComparer.Ordinal)
                || !Enum.IsDefined(typeof(TextInspectorNumericComparison),condition.Comparison)
                || (condition.DecimalSeparator != "." && condition.DecimalSeparator != ",")
                || (condition.UnitCaptureGroup is null) != (condition.UnitFactors is null)
                || (condition.UnitCaptureGroup is not null && (!regex.GetGroupNames().Contains(condition.UnitCaptureGroup.Trim(),StringComparer.Ordinal)
                    || condition.UnitFactors!.Count == 0 || condition.UnitFactors.Any(pair => string.IsNullOrWhiteSpace(pair.Key) || pair.Value <= 0))))
            {
                diagnostics.Add(Diagnostic(TextInspectorDiagnosticKind.InvalidNumericCondition,rule,
                    "Each numeric condition requires an existing capture group, a supported comparison, and a '.' or ',' decimal separator."));
                return false;
            }

            var format = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            format.NumberDecimalSeparator = condition.DecimalSeparator;
            compiled.Add(new RuntimeNumericCondition(
                condition.CaptureGroup.Trim(),condition.Comparison,condition.Value,NumberFormatInfo.ReadOnly(format),
                condition.UnitCaptureGroup?.Trim(),condition.UnitFactors is null ? null
                    : new Dictionary<string,decimal>(condition.UnitFactors,StringComparer.OrdinalIgnoreCase)));
        }

        conditions = compiled.ToArray();
        return true;
    }

    private static bool PassesNumericConditions(
        RuntimeRule rule,
        Match match,
        List<TextInspectorDiagnostic>? diagnostics = null)
    {
        bool passes = true;
        foreach(var condition in rule.NumericConditions)
        {
            var group = match.Groups[condition.CaptureGroup];
            if(!group.Success || group.Captures.Count != 1
                || !TryParseExactNumber(group.Value,condition.Format,out decimal value))
            {
                diagnostics?.Add(new TextInspectorDiagnostic
                {
                    Kind = TextInspectorDiagnosticKind.InvalidNumericCapture,
                    RuleId = rule.RuleId,
                    RuleKey = rule.Key,
                    Message = $"Capture '{condition.CaptureGroup}' must contain exactly one decimal value representable without rounding, using '{condition.Format.NumberDecimalSeparator}' as its decimal separator. This match was not evaluated."
                });
                return false;
            }

            if(condition.UnitCaptureGroup is not null)
            {
                var unit = match.Groups[condition.UnitCaptureGroup];
                if(!unit.Success || unit.Captures.Count != 1
                    || condition.UnitFactors is null || !condition.UnitFactors.TryGetValue(unit.Value,out var factor))
                {
                    diagnostics?.Add(new TextInspectorDiagnostic
                    {
                        Kind = TextInspectorDiagnosticKind.InvalidNumericCapture,
                        RuleId = rule.RuleId, RuleKey = rule.Key,
                        Message = "Numeric unit capture is missing, repeated or unknown; match was not evaluated."
                    });
                    return false;
                }
                try { value = checked(value * factor); }
                catch(OverflowException)
                {
                    diagnostics?.Add(new TextInspectorDiagnostic
                    {
                        Kind = TextInspectorDiagnosticKind.InvalidNumericCapture,
                        RuleId = rule.RuleId, RuleKey = rule.Key,
                        Message = "Numeric unit conversion overflowed; match was not evaluated."
                    });
                    return false;
                }
            }

            // Evaluate every capture even if an earlier comparison was false, so malformed
            // values in another condition cannot silently disappear through short-circuiting.
            passes &= condition.Comparison switch
            {
                TextInspectorNumericComparison.Equal => value == condition.Value,
                TextInspectorNumericComparison.NotEqual => value != condition.Value,
                TextInspectorNumericComparison.GreaterThan => value > condition.Value,
                TextInspectorNumericComparison.GreaterThanOrEqual => value >= condition.Value,
                TextInspectorNumericComparison.LessThan => value < condition.Value,
                TextInspectorNumericComparison.LessThanOrEqual => value <= condition.Value,
                _ => false // Unknown values are rejected at compilation.
            };
        }
        return passes;
    }

    private static bool TryParseExactNumber(string text,NumberFormatInfo format,out decimal value)
    {
        value = 0;
        if(text.Length == 0)
            return false;

        int start = text[0] is '+' or '-' ? 1 : 0;
        int separator = -1;
        for(int index = start;index < text.Length;index++)
        {
            char current = text[index];
            if(current >= '0' && current <= '9')
                continue;
            if(current == format.NumberDecimalSeparator[0] && separator < 0)
                separator = index;
            else
                return false;
        }

        // Require digits on both sides of an optional decimal separator.
        if(start == text.Length || separator == start || separator == text.Length - 1)
            return false;
        if(!decimal.TryParse(text,NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,format,out value))
            return false;

        int integerEnd = separator < 0 ? text.Length : separator;
        string integer = text.Substring(start,integerEnd - start).TrimStart('0');
        if(integer.Length == 0)
            integer = "0";
        string fraction = separator < 0 ? "" : text.Substring(separator + 1).TrimEnd('0');
        string canonical = integer + (fraction.Length == 0 ? "" : "." + fraction);
        if(text[0] == '-' && canonical != "0")
            canonical = "-" + canonical;

        // Decimal.TryParse may round excessive fractional precision. Refuse that loss
        // instead of turning a value slightly above a limit into an apparent equality.
        string exact = value == 0 ? "0" : value.ToString("0.############################",CultureInfo.InvariantCulture);
        return string.Equals(canonical,exact,StringComparison.Ordinal);
    }

    private sealed record RuntimeNumericCondition(
        string CaptureGroup,
        TextInspectorNumericComparison Comparison,
        decimal Value,
        NumberFormatInfo Format,
        string? UnitCaptureGroup,
        IReadOnlyDictionary<string,decimal>? UnitFactors);
}
