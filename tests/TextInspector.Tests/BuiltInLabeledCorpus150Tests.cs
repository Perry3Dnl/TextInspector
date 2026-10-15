using Xunit;

namespace TextInspector.Tests;

/// <summary>
/// 150 labeled, reviewable examples locked before detector calibration.
/// Each assertion addresses a named built-in rule; unrelated rules are permitted.
/// </summary>
public sealed class BuiltInLabeledCorpus150Tests
{
    private static readonly Lazy<TextInspectorEngine> Engine =
        new(() => TextInspectorEngine.Create());

    public static IEnumerable<object[]> Cases()
    {
        yield return new object[] { "C001", "I am going to kill you", "Threat_Violence", true };
        yield return new object[] { "C002", "Context: I am going to kill you. End of message.", "Threat_Violence", true };
        yield return new object[] { "C003", "I'm gonna hurt you", "Threat_Violence", true };
        yield return new object[] { "C004", "Context: I'm gonna hurt you. End of message.", "Threat_Violence", true };
        yield return new object[] { "C005", "I'm going to harm you", "Threat_Violence", true };
        yield return new object[] { "C006", "Context: I'm going to harm you. End of message.", "Threat_Violence", true };
        yield return new object[] { "C007", "I'm going to murder you", "Threat_Violence", true };
        yield return new object[] { "C008", "Context: I'm going to murder you. End of message.", "Threat_Violence", true };
        yield return new object[] { "C009", "I am going to hurt you", "Threat_Violence", true };
        yield return new object[] { "C010", "Context: I am going to hurt you. End of message.", "Threat_Violence", true };
        yield return new object[] { "C011", "I am an administrator", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C012", "Context: I am an administrator. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C013", "I'm a moderator", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C014", "Context: I'm a moderator. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C015", "official support team", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C016", "Context: official support team. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C017", "support team here", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C018", "Context: support team here. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C019", "verify your password", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C020", "Context: verify your password. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C021", "The murder mystery is a bestseller", "Threat_Violence", false };
        yield return new object[] { "C022", "Context: The murder mystery is a bestseller. End of message.", "Threat_Violence", false };
        yield return new object[] { "C023", "This video game has fictional battles", "Threat_Violence", false };
        yield return new object[] { "C024", "Context: This video game has fictional battles. End of message.", "Threat_Violence", false };
        yield return new object[] { "C025", "The biography discusses an old conflict", "Threat_Violence", false };
        yield return new object[] { "C026", "Context: The biography discusses an old conflict. End of message.", "Threat_Violence", false };
        yield return new object[] { "C027", "We are discussing literary villains", "Threat_Violence", false };
        yield return new object[] { "C028", "Context: We are discussing literary villains. End of message.", "Threat_Violence", false };
        yield return new object[] { "C029", "The crime novel was fascinating", "Threat_Violence", false };
        yield return new object[] { "C030", "Context: The crime novel was fascinating. End of message.", "Threat_Violence", false };
        yield return new object[] { "C031", "Our design team updated the website", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C032", "Context: Our design team updated the website. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C033", "A support article is available", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C034", "Context: A support article is available. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C035", "The administration office is closed", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C036", "Context: The administration office is closed. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C037", "The manual describes moderator duties", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C038", "Context: The manual describes moderator duties. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C039", "The developer documentation is public", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C040", "Context: The developer documentation is public. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C041", "docs@example.com", "PII_Email_Address", true };
        yield return new object[] { "C042", "Context: docs@example.com. End of message.", "PII_Email_Address", true };
        yield return new object[] { "C043", "help@example.org", "PII_Email_Address", true };
        yield return new object[] { "C044", "Context: help@example.org. End of message.", "PII_Email_Address", true };
        yield return new object[] { "C045", "test@example.net", "PII_Email_Address", true };
        yield return new object[] { "C046", "Context: test@example.net. End of message.", "PII_Email_Address", true };
        yield return new object[] { "C047", "dev@example.com", "PII_Email_Address", true };
        yield return new object[] { "C048", "Context: dev@example.com. End of message.", "PII_Email_Address", true };
        yield return new object[] { "C049", "admin@example.org", "PII_Email_Address", true };
        yield return new object[] { "C050", "Context: admin@example.org. End of message.", "PII_Email_Address", true };
        yield return new object[] { "C051", "example.com", "Platform_Domain_Link", true };
        yield return new object[] { "C052", "Context: example.com. End of message.", "Platform_Domain_Link", true };
        yield return new object[] { "C053", "example.org", "Platform_Domain_Link", true };
        yield return new object[] { "C054", "Context: example.org. End of message.", "Platform_Domain_Link", true };
        yield return new object[] { "C055", "docs.example.net", "Platform_Domain_Link", true };
        yield return new object[] { "C056", "Context: docs.example.net. End of message.", "Platform_Domain_Link", true };
        yield return new object[] { "C057", "developer.example.com", "Platform_Domain_Link", true };
        yield return new object[] { "C058", "Context: developer.example.com. End of message.", "Platform_Domain_Link", true };
        yield return new object[] { "C059", "releases.example.org", "Platform_Domain_Link", true };
        yield return new object[] { "C060", "Context: releases.example.org. End of message.", "Platform_Domain_Link", true };
        yield return new object[] { "C061", "I am going to kill you tonight", "Threat_Violence", true };
        yield return new object[] { "C062", "Context: I am going to kill you tonight. End of message.", "Threat_Violence", true };
        yield return new object[] { "C063", "I'm going to hurt you tomorrow", "Threat_Violence", true };
        yield return new object[] { "C064", "Context: I'm going to hurt you tomorrow. End of message.", "Threat_Violence", true };
        yield return new object[] { "C065", "I'm gonna harm you right now", "Threat_Violence", true };
        yield return new object[] { "C066", "Context: I'm gonna harm you right now. End of message.", "Threat_Violence", true };
        yield return new object[] { "C067", "I'm going to murder you later", "Threat_Violence", true };
        yield return new object[] { "C068", "Context: I'm going to murder you later. End of message.", "Threat_Violence", true };
        yield return new object[] { "C069", "I am going to kill you again", "Threat_Violence", true };
        yield return new object[] { "C070", "Context: I am going to kill you again. End of message.", "Threat_Violence", true };
        yield return new object[] { "C071", "Official support team speaking", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C072", "Context: Official support team speaking. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C073", "I am staff", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C074", "Context: I am staff. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C075", "I'm an admin", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C076", "Context: I'm an admin. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C077", "system message", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C078", "Context: system message. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C079", "login details", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C080", "Context: login details. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C081", "We're going to review the report", "Threat_Violence", false };
        yield return new object[] { "C082", "Context: We're going to review the report. End of message.", "Threat_Violence", false };
        yield return new object[] { "C083", "I am going to help you today", "Threat_Violence", false };
        yield return new object[] { "C084", "Context: I am going to help you today. End of message.", "Threat_Violence", false };
        yield return new object[] { "C085", "The team is going to launch the product", "Threat_Violence", false };
        yield return new object[] { "C086", "Context: The team is going to launch the product. End of message.", "Threat_Violence", false };
        yield return new object[] { "C087", "I am going to call you tomorrow", "Threat_Violence", false };
        yield return new object[] { "C088", "Context: I am going to call you tomorrow. End of message.", "Threat_Violence", false };
        yield return new object[] { "C089", "We're going to visit the museum", "Threat_Violence", false };
        yield return new object[] { "C090", "Context: We're going to visit the museum. End of message.", "Threat_Violence", false };
        yield return new object[] { "C091", "The system shows a normal message", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C092", "Context: The system shows a normal message. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C093", "The staff meeting ended early", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C094", "Context: The staff meeting ended early. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C095", "Customer support hours changed", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C096", "Context: Customer support hours changed. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C097", "A login tutorial is available", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C098", "Context: A login tutorial is available. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C099", "The official manual is available", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C100", "Context: The official manual is available. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C101", "kill them all", "Threat_Violence", true };
        yield return new object[] { "C102", "Context: kill them all. End of message.", "Threat_Violence", true };
        yield return new object[] { "C103", "killing them all", "Threat_Violence", true };
        yield return new object[] { "C104", "Context: killing them all. End of message.", "Threat_Violence", true };
        yield return new object[] { "C105", "kill him all", "Threat_Violence", true };
        yield return new object[] { "C106", "Context: kill him all. End of message.", "Threat_Violence", true };
        yield return new object[] { "C107", "kill her all", "Threat_Violence", true };
        yield return new object[] { "C108", "Context: kill her all. End of message.", "Threat_Violence", true };
        yield return new object[] { "C109", "I am going to harm you", "Threat_Violence", true };
        yield return new object[] { "C110", "Context: I am going to harm you. End of message.", "Threat_Violence", true };
        yield return new object[] { "C111", "account restricted", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C112", "Context: account restricted. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C113", "account is restricted", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C114", "Context: account is restricted. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C115", "logout now", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C116", "Context: logout now. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C117", "official help", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C118", "Context: official help. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C119", "dev team speaking", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C120", "Context: dev team speaking. End of message.", "Fraud_Staff_Impersonation", true };
        yield return new object[] { "C121", "That movie has a villain", "Threat_Violence", false };
        yield return new object[] { "C122", "Context: That movie has a villain. End of message.", "Threat_Violence", false };
        yield return new object[] { "C123", "The history book covers medieval battles", "Threat_Violence", false };
        yield return new object[] { "C124", "Context: The history book covers medieval battles. End of message.", "Threat_Violence", false };
        yield return new object[] { "C125", "A fictional character was defeated", "Threat_Violence", false };
        yield return new object[] { "C126", "Context: A fictional character was defeated. End of message.", "Threat_Violence", false };
        yield return new object[] { "C127", "We watched a detective series", "Threat_Violence", false };
        yield return new object[] { "C128", "Context: We watched a detective series. End of message.", "Threat_Violence", false };
        yield return new object[] { "C129", "This novel is about peace", "Threat_Violence", false };
        yield return new object[] { "C130", "Context: This novel is about peace. End of message.", "Threat_Violence", false };
        yield return new object[] { "C131", "The help center has documentation", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C132", "Context: The help center has documentation. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C133", "I wrote a public safety guide", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C134", "Context: I wrote a public safety guide. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C135", "We completed a team retrospective", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C136", "Context: We completed a team retrospective. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C137", "Please review our staff handbook", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C138", "Context: Please review our staff handbook. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C139", "The site includes an account guide", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C140", "Context: The site includes an account guide. End of message.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "C141", "I am going to kill you", "Threat_Violence", true };
        yield return new object[] { "C142", "Context: I am going to kill you. End of message.", "Threat_Violence", true };
        yield return new object[] { "C143", "I'm gonna hurt you", "Threat_Violence", true };
        yield return new object[] { "C144", "Context: I'm gonna hurt you. End of message.", "Threat_Violence", true };
        yield return new object[] { "C145", "I'm going to harm you", "Threat_Violence", true };
        yield return new object[] { "C146", "Context: I'm going to harm you. End of message.", "Threat_Violence", true };
        yield return new object[] { "C147", "I'm going to murder you", "Threat_Violence", true };
        yield return new object[] { "C148", "Context: I'm going to murder you. End of message.", "Threat_Violence", true };
        yield return new object[] { "C149", "I am going to hurt you", "Threat_Violence", true };
        yield return new object[] { "C150", "Context: I am going to hurt you. End of message.", "Threat_Violence", true };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void NamedRuleMatchesExpectedLabel(string id,string text,string ruleKey,bool shouldMatch)
    {
        Assert.Matches(@"^C\d{3}$",id);
        var result = Engine.Value.Inspect(text,new TextInspectionOptions
        {
            IncludeNonScoringEvidence = true
        });
        var found = result.Evidence.Any(e => string.Equals(
            e.RuleKey,ruleKey,StringComparison.OrdinalIgnoreCase));
        Assert.True(found == shouldMatch,
            $"Corpus {id}: rule {ruleKey}, expected={shouldMatch}, actual={found}, text={text}");
    }
}
