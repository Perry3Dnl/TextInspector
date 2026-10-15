using Xunit;

namespace TextInspector.Tests;

/// <summary>Adversarial named-rule corpus. Labels are fixed before running detector CI.</summary>
public sealed class AdversarialLabeledCorpus150Tests
{
    private static readonly Lazy<TextInspectorEngine> Engine =
        new(() => TextInspectorEngine.Create());

    public static IEnumerable<object[]> Cases()
    {
        yield return new object[] { "A001", "negation", "I am not going to kill you", "Threat_Violence", false };
        yield return new object[] { "A002", "negation", "Contextual example: I am not going to kill you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A003", "negation", "I am never going to hurt you", "Threat_Violence", false };
        yield return new object[] { "A004", "negation", "Contextual example: I am never going to hurt you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A005", "negation", "I'm not going to harm you", "Threat_Violence", false };
        yield return new object[] { "A006", "negation", "Contextual example: I'm not going to harm you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A007", "negation", "I won't kill you", "Threat_Violence", false };
        yield return new object[] { "A008", "negation", "Contextual example: I won't kill you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A009", "negation", "Do not threaten anyone", "Threat_Violence", false };
        yield return new object[] { "A010", "negation", "Contextual example: Do not threaten anyone — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A011", "negation", "Never say you will hurt someone", "Threat_Violence", false };
        yield return new object[] { "A012", "negation", "Contextual example: Never say you will hurt someone — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A013", "negation", "I am going to help you", "Threat_Violence", false };
        yield return new object[] { "A014", "negation", "Contextual example: I am going to help you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A015", "negation", "I am going to call you", "Threat_Violence", false };
        yield return new object[] { "A016", "negation", "Contextual example: I am going to call you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A017", "negation", "I am not gonna murder you", "Threat_Violence", false };
        yield return new object[] { "A018", "negation", "Contextual example: I am not gonna murder you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A019", "negation", "I'm unlikely to hurt you", "Threat_Violence", false };
        yield return new object[] { "A020", "negation", "Contextual example: I'm unlikely to hurt you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A021", "negation", "I would not harm you", "Threat_Violence", false };
        yield return new object[] { "A022", "negation", "Contextual example: I would not harm you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A023", "negation", "Please never threaten me", "Threat_Violence", false };
        yield return new object[] { "A024", "negation", "Contextual example: Please never threaten me — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A025", "negation", "That was not a threat", "Threat_Violence", false };
        yield return new object[] { "A026", "negation", "Contextual example: That was not a threat — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A027", "negation", "I am going to thank you", "Threat_Violence", false };
        yield return new object[] { "A028", "negation", "Contextual example: I am going to thank you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A029", "negation", "I am going to support you", "Threat_Violence", false };
        yield return new object[] { "A030", "negation", "Contextual example: I am going to support you — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A031", "quotation", "\"I am going to kill you\"", "Threat_Violence", true };
        yield return new object[] { "A032", "quotation", "Contextual example: \"I am going to kill you\" — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A033", "quotation", "'I'm gonna hurt you'", "Threat_Violence", true };
        yield return new object[] { "A034", "quotation", "Contextual example: 'I'm gonna hurt you' — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A035", "quotation", "The quoted line was: I am going to harm you", "Threat_Violence", true };
        yield return new object[] { "A036", "quotation", "Contextual example: The quoted line was: I am going to harm you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A037", "quotation", "He wrote: I'm going to murder you", "Threat_Violence", true };
        yield return new object[] { "A038", "quotation", "Contextual example: He wrote: I'm going to murder you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A039", "quotation", "Transcript: I am going to hurt you", "Threat_Violence", true };
        yield return new object[] { "A040", "quotation", "Contextual example: Transcript: I am going to hurt you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A041", "quotation", "A character says: I am going to kill you", "Threat_Violence", true };
        yield return new object[] { "A042", "quotation", "Contextual example: A character says: I am going to kill you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A043", "quotation", "The script contains I'm gonna hurt you", "Threat_Violence", true };
        yield return new object[] { "A044", "quotation", "Contextual example: The script contains I'm gonna hurt you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A045", "quotation", "The novel says I'm going to harm you", "Threat_Violence", true };
        yield return new object[] { "A046", "quotation", "Contextual example: The novel says I'm going to harm you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A047", "quotation", "Evidence: I am going to murder you", "Threat_Violence", true };
        yield return new object[] { "A048", "quotation", "Contextual example: Evidence: I am going to murder you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A049", "quotation", "A warning quoted I am going to hurt you", "Threat_Violence", true };
        yield return new object[] { "A050", "quotation", "Contextual example: A warning quoted I am going to hurt you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A051", "quotation", "Report of message: kill them all", "Threat_Violence", true };
        yield return new object[] { "A052", "quotation", "Contextual example: Report of message: kill them all — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A053", "quotation", "Screenshot text: killing them all", "Threat_Violence", true };
        yield return new object[] { "A054", "quotation", "Contextual example: Screenshot text: killing them all — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A055", "quotation", "Moderator quote: I am going to kill you", "Threat_Violence", true };
        yield return new object[] { "A056", "quotation", "Contextual example: Moderator quote: I am going to kill you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A057", "quotation", "Archived chat: I'm gonna hurt you", "Threat_Violence", true };
        yield return new object[] { "A058", "quotation", "Contextual example: Archived chat: I'm gonna hurt you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A059", "quotation", "For study: I'm going to harm you", "Threat_Violence", true };
        yield return new object[] { "A060", "quotation", "Contextual example: For study: I'm going to harm you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A061", "multilingual", "Ik ga je morgen bellen", "Threat_Violence", false };
        yield return new object[] { "A062", "multilingual", "Contextual example: Ik ga je morgen bellen — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A063", "multilingual", "Ich werde dir morgen helfen", "Threat_Violence", false };
        yield return new object[] { "A064", "multilingual", "Contextual example: Ich werde dir morgen helfen — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A065", "multilingual", "Je vais te téléphoner", "Threat_Violence", false };
        yield return new object[] { "A066", "multilingual", "Contextual example: Je vais te téléphoner — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A067", "multilingual", "Voy a ayudarte mañana", "Threat_Violence", false };
        yield return new object[] { "A068", "multilingual", "Contextual example: Voy a ayudarte mañana — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A069", "multilingual", "Ik maak een verslag", "Threat_Violence", false };
        yield return new object[] { "A070", "multilingual", "Contextual example: Ik maak een verslag — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A071", "multilingual", "Wir sprechen heute darüber", "Threat_Violence", false };
        yield return new object[] { "A072", "multilingual", "Contextual example: Wir sprechen heute darüber — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A073", "multilingual", "Dat is een goed idee", "Threat_Violence", false };
        yield return new object[] { "A074", "multilingual", "Contextual example: Dat is een goed idee — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A075", "multilingual", "Die Anleitung ist fertig", "Threat_Violence", false };
        yield return new object[] { "A076", "multilingual", "Contextual example: Die Anleitung ist fertig — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A077", "multilingual", "Laten we de documenten lezen", "Threat_Violence", false };
        yield return new object[] { "A078", "multilingual", "Contextual example: Laten we de documenten lezen — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A079", "multilingual", "Wir gehen zum Museum", "Threat_Violence", false };
        yield return new object[] { "A080", "multilingual", "Contextual example: Wir gehen zum Museum — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A081", "multilingual", "Bonjour et bienvenue", "Threat_Violence", false };
        yield return new object[] { "A082", "multilingual", "Contextual example: Bonjour et bienvenue — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A083", "multilingual", "Goedemorgen allemaal", "Threat_Violence", false };
        yield return new object[] { "A084", "multilingual", "Contextual example: Goedemorgen allemaal — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A085", "multilingual", "Vielen Dank für die Hilfe", "Threat_Violence", false };
        yield return new object[] { "A086", "multilingual", "Contextual example: Vielen Dank für die Hilfe — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A087", "multilingual", "La réunion commence demain", "Threat_Violence", false };
        yield return new object[] { "A088", "multilingual", "Contextual example: La réunion commence demain — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A089", "multilingual", "Das Buch ist interessant", "Threat_Violence", false };
        yield return new object[] { "A090", "multilingual", "Contextual example: Das Buch ist interessant — archived for review.", "Threat_Violence", false };
        yield return new object[] { "A091", "unicode", "Ｉ ａｍ ｇｏｉｎｇ ｔｏ ｋｉｌｌ ｙｏｕ", "Threat_Violence", true };
        yield return new object[] { "A092", "unicode", "Contextual example: Ｉ ａｍ ｇｏｉｎｇ ｔｏ ｋｉｌｌ ｙｏｕ — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A093", "unicode", "I'm going to kill you", "Threat_Violence", true };
        yield return new object[] { "A094", "unicode", "Contextual example: I'm going to kill you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A095", "unicode", "I am going to hurt you", "Threat_Violence", true };
        yield return new object[] { "A096", "unicode", "Contextual example: I am going to hurt you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A097", "unicode", "I'm gonna harm you", "Threat_Violence", true };
        yield return new object[] { "A098", "unicode", "Contextual example: I'm gonna harm you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A099", "unicode", "I'm going to murder you", "Threat_Violence", true };
        yield return new object[] { "A100", "unicode", "Contextual example: I'm going to murder you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A101", "unicode", "I am going to kill you", "Threat_Violence", true };
        yield return new object[] { "A102", "unicode", "Contextual example: I am going to kill you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A103", "unicode", "I am going to hurt you", "Threat_Violence", true };
        yield return new object[] { "A104", "unicode", "Contextual example: I am going to hurt you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A105", "unicode", "I'm gonna hurt you", "Threat_Violence", true };
        yield return new object[] { "A106", "unicode", "Contextual example: I'm gonna hurt you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A107", "unicode", "I'm going to harm you", "Threat_Violence", true };
        yield return new object[] { "A108", "unicode", "Contextual example: I'm going to harm you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A109", "unicode", "I am going to murder you", "Threat_Violence", true };
        yield return new object[] { "A110", "unicode", "Contextual example: I am going to murder you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A111", "unicode", "I'm gonna kill you", "Threat_Violence", true };
        yield return new object[] { "A112", "unicode", "Contextual example: I'm gonna kill you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A113", "unicode", "I am going to kill you", "Threat_Violence", true };
        yield return new object[] { "A114", "unicode", "Contextual example: I am going to kill you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A115", "unicode", "I'm going to hurt you", "Threat_Violence", true };
        yield return new object[] { "A116", "unicode", "Contextual example: I'm going to hurt you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A117", "unicode", "I am going to harm you", "Threat_Violence", true };
        yield return new object[] { "A118", "unicode", "Contextual example: I am going to harm you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A119", "unicode", "I'm gonna murder you", "Threat_Violence", true };
        yield return new object[] { "A120", "unicode", "Contextual example: I'm gonna murder you — archived for review.", "Threat_Violence", true };
        yield return new object[] { "A121", "ambiguous", "The design team updated its guide", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A122", "ambiguous", "Contextual example: The design team updated its guide — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A123", "ambiguous", "The service team answered the question", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A124", "ambiguous", "Contextual example: The service team answered the question — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A125", "ambiguous", "We published the moderator handbook", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A126", "ambiguous", "Contextual example: We published the moderator handbook — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A127", "ambiguous", "Our public support article explains changes", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A128", "ambiguous", "Contextual example: Our public support article explains changes — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A129", "ambiguous", "The administration building is open", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A130", "ambiguous", "Contextual example: The administration building is open — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A131", "ambiguous", "This is a project status update", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A132", "ambiguous", "Contextual example: This is a project status update — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A133", "ambiguous", "Developers updated the authentication docs", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A134", "ambiguous", "Contextual example: Developers updated the authentication docs — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A135", "ambiguous", "Staff training begins tomorrow", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A136", "ambiguous", "Contextual example: Staff training begins tomorrow — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A137", "ambiguous", "The application shows a notification", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A138", "ambiguous", "Contextual example: The application shows a notification — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A139", "ambiguous", "The official website has FAQs", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A140", "ambiguous", "Contextual example: The official website has FAQs — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A141", "ambiguous", "Please read the documentation", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A142", "ambiguous", "Contextual example: Please read the documentation — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A143", "ambiguous", "We discussed login form accessibility", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A144", "ambiguous", "Contextual example: We discussed login form accessibility — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A145", "ambiguous", "A professor described support operations", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A146", "ambiguous", "Contextual example: A professor described support operations — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A147", "ambiguous", "The message was forwarded internally", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A148", "ambiguous", "Contextual example: The message was forwarded internally — archived for review.", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A149", "ambiguous", "The team reviewed the incident report", "Fraud_Staff_Impersonation", false };
        yield return new object[] { "A150", "ambiguous", "Contextual example: The team reviewed the incident report — archived for review.", "Fraud_Staff_Impersonation", false };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void PresetRuleDecisionAndEvidenceSpan(string id,string category,string input,string expectedRule,bool expectedMatch)
    {
        var result = Engine.Value.Inspect(input,new TextInspectionOptions
        {
            IncludeNonScoringEvidence = true,
            IncludeHighlightSpans = true
        });
        var hits = result.Evidence.Where(e => e.RuleKey == expectedRule).ToArray();
        Assert.True((hits.Length > 0) == expectedMatch,
            $"{id} ({category}): expected {expectedRule} match={expectedMatch}, observed {hits.Length}, input={input}");
        foreach(var hit in hits)
        {
            Assert.InRange(hit.Start,0,input.Length - 1);
            Assert.InRange(hit.Length,1,input.Length-hit.Start);
            Assert.Equal(input.Substring(hit.Start,hit.Length),hit.MatchedText);
        }
    }
}
