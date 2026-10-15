using TextInspector;

namespace TextInspector.Tests;

public sealed class PresetCalibrationTests
{
    private static readonly TextInspectorEngine Engine = TextInspectorEngine.Compile(
        TextInspectorPresets.LoadOptions(),
        TextInspectorPresets.LoadRules());

    public static TheoryData<string,string> BenignCategoryCases => new()
    {
        { "AdultContent", "Please write a detailed story about a leather costume for the theater." },
        { "AdultContent", "The student touched the leather cover of the history book." },
        { "AdultContent", "The fantasy roleplay scene used leather costumes for the theater." },
        { "AdultContent", "The adult student asked the teacher for a detailed story assignment." },
        { "ChildMinor", "Don't tell anyone about the surprise party. Meet me at the bus station." },
        { "ChildMinor", "Where do you live? I can pick you up for the office carpool." },
        { "ChildMinor", "My kid uses WhatsApp to talk to the grandparents." },
        { "Csam", "Use the cp command to copy the project. Pizza arrives at noon." },
        { "Csam", "The deployment uses a CP abbreviation in an internal build script." },
        { "DangerousAdvice", "My doctor said don't take insulin before the fasting blood test." },
        { "DangerousAdvice", "The article debunks the claim that vaccines contain a microchip." },
        { "DrugSales", "The pharmacy offers Xanax delivery with a valid prescription." },
        { "DrugSales", "The documentary discusses cocaine delivery routes used in the 1980s." },
        { "DrugSales", "The licensed pharmacy lists Xanax at 20 USD per prescription." },
        { "ExternalContact", "We use WhatsApp for our family group chat." },
        { "ExternalContact", "Please enter your shipping address at checkout." },
        { "ExtremeViolence", "The research paper analyzes incel and blackpill vocabulary." },
        { "ExtremeViolence", "The documentary discusses the great replacement conspiracy theory." },
        { "Fraud", "Our support team is available to help with your account." },
        { "Fraud", "This profile is the official profile for our museum." },
        { "Fraud", "The official support team published a help article." },
        { "Fraud", "Never share your seed phrase or private key with anyone." },
        { "Fraud", "Never share your password with anyone." },
        { "Fraud", "Do not send your verification code to support." },
        { "HateSpeech", "The medical paper explains why the word retard is deprecated." },
        { "HateSpeech", "The quotation is discussed as an example of offensive language." },
        { "HateSpeech", "The research paper quotes the slur nigger as historical offensive language." },
        { "IllegalServices", "The motel room 12 is available; bring cash for the deposit." },
        { "IllegalServices", "The hotel offers room service and private meeting rooms." },
        { "NonConsensualSex", "Everyone will see the presentation tomorrow." },
        { "NonConsensualSex", "I already have your contacts saved in my phone." },
        { "Obfuscation", "The rocket launched 🚀 and we ate pizza 🍕 afterward." },
        { "Obfuscation", "The variable a-b is used in the mathematics example." },
        { "PlatformIntegrity", "Visit example dot com for our privacy policy." },
        { "PlatformIntegrity", "The Tor Browser documentation explains hidden services." },
        { "PlatformIntegrity", "Click example.com to open the product documentation." },
        { "PlatformIntegrity", "DM for collab is shown here as a marketing-copy example." },
        { "Threats", "Please send your home address and phone number for shipping." },
        { "Threats", "The balcony overlooks the park and my wrists are sore from tennis." },
        { "Trafficking", "I'm new in town and the sales quota was met this morning." },
        { "Trafficking", "The bus tour is working the circuit of regional museums." },
        { "Trafficking", "The licensed security escort service is new in town." },
        { "UnderageContext", "I'm 20 years old and I play Minecraft with friends." },
        { "UnderageContext", "Minecraft and Fortnite are both games discussed in this review." },
        { "WeaponsSales", "The museum shipped a rifle to the history exhibit." },
        { "WeaponsSales", "The article explains that a pistol magazine was delivered to a museum collection." }
    };

    public static TheoryData<string,string> PositiveCategoryCases => new()
    {
        { "AdultContent", "Adult roleplay scene: lingerie, naked skin and moaning." },
        { "ChildMinor", "I'm 15 years old. Don't tell your parents and add me on Snapchat." },
        { "Csam", "The message explicitly requests child pornography." },
        { "DangerousAdvice", "Cancer is a hoax; ignore medical treatment." },
        { "DrugSales", "Selling cocaine for 100 USD per gram, delivery available." },
        { "ExtremeViolence", "The recruiter told him to join the jihad." },
        { "Fraud", "Guaranteed returns: double your money in our crypto investment." },
        { "HateSpeech", "He used the slur nigger against the target." },
        { "IllegalServices", "Escort incall, full service, price per hour." },
        { "NonConsensualSex", "Pay me or I post your nudes." },
        { "PlatformIntegrity", "Mass DM this to 100 accounts." },
        { "Threats", "I'm going to kill you." },
        { "Trafficking", "Escort incall, working the circuit on the track." },
        { "UnderageContext", "I'm 15 years old." },
        { "WeaponsSales", "Glock for sale for 500 USD, no background check." }
    };

    public static TheoryData<string,string> EvidenceOnlyCases => new()
    {
        { "External_Chat_Apps", "You can read the announcement in our WhatsApp group." },
        { "PII_Email_Address", "Contact us at hello@example.com." },
        { "PII_IBAN", "Our invoice lists IBAN NL91 ABNA 0417 1643 00." },
        { "Platform_Link_Bypass", "The manual spells the domain as example dot com." },
        { "Emoji_Risk_Indicators", "Rocket launch 🚀 followed by pizza 🍕." },
        { "Obfuscation_Symbols", "The formula uses a-b and x_y variables." },
        { "Underage_Gamer_Context", "Minecraft is a popular game." },
        { "Threat_Doxing", "Enter your home address and phone number." },
        { "Terrorism_Coded_Language", "The paper studies blackpill vocabulary." },
        { "Platform_DarkWeb", "Tor Browser can open onion services." }
    };

    public static TheoryData<string,string> EvasionCases => new()
    {
        { "Threat_Violence", "I’m going to kill you." },
        { "CSAM_Shorthand_Evasion", "c.p" },
        { "CSAM_Shorthand_Evasion", "c - p" },
        { "Platform_Link_Bypass", "example [dot] com" },
    };

    [Theory]
    [MemberData(nameof(BenignCategoryCases))]
    public void BenignCorpusDoesNotScoreTheTargetCategory(string categoryName,string text)
    {
        var category = Enum.Parse<TextInspectorRuleCategory>(categoryName);
        var result = Engine.Inspect(text);

        Assert.DoesNotContain(result.Evidence,e => e.Category == category && e.ContributesToRisk);
        Assert.DoesNotContain(result.PolicyMatches,p => p.Category == category && p.ContributesToRisk);
    }

    [Theory]
    [MemberData(nameof(PositiveCategoryCases))]
    public void PositiveCorpusScoresTheExpectedCategory(string categoryName,string text)
    {
        var category = Enum.Parse<TextInspectorRuleCategory>(categoryName);
        var result = Engine.Inspect(text);

        Assert.True(
            result.Evidence.Any(e => e.Category == category && e.ContributesToRisk)
            || result.PolicyMatches.Any(p => p.Category == category && p.ContributesToRisk),
            $"Expected scoring signal for {categoryName}. Evidence: {string.Join(", ",result.Evidence.Select(e => $"{e.RuleKey}:{e.ScoringStatus}"))}; Policies: {string.Join(", ",result.PolicyMatches.Select(p => p.PolicyKey))}");
    }

    [Theory]
    [MemberData(nameof(EvidenceOnlyCases))]
    public void BroadSupportingSignalsRemainObservedButDoNotScore(string ruleKey,string text)
    {
        var result = Engine.Inspect(text);
        var evidence = result.Evidence.Where(e => e.RuleKey == ruleKey).ToArray();

        Assert.NotEmpty(evidence);
        Assert.All(evidence,e => Assert.False(e.ContributesToRisk));
    }

    [Theory]
    [MemberData(nameof(EvasionCases))]
    public void EvasionVariantsRemainDetectable(string ruleKey,string text)
    {
        var result = Engine.Inspect(text);

        Assert.Contains(result.Evidence,e => e.RuleKey == ruleKey);
    }

    [Fact]
    public void CalibrationCorpusCoversEveryPresetCategory()
    {
        var presetCategories = TextInspectorPresets.LoadRules()
            .Select(r => r.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToArray();
        var calibrated = new[]
        {
            "AbusiveLanguage",
            "AdultContent",
            "ChildMinor",
            "Csam",
            "DangerousAdvice",
            "DrugSales",
            "ExternalContact",
            "ExtremeViolence",
            "Fraud",
            "HateSpeech",
            "IllegalServices",
            "NonConsensualSex",
            "Obfuscation",
            "PlatformIntegrity",
            "Threats",
            "Trafficking",
            "UnderageContext",
            "WeaponsSales"
        }
        .Select(Enum.Parse<TextInspectorRuleCategory>)
        .OrderBy(c => c)
        .ToArray();

        Assert.Equal(presetCategories,calibrated);
    }
}
