using Xunit;

namespace TextInspector.Tests;

/// <summary>Adversarial context tests: preserved evidence, reduced risk only for explicit local negation or educational attribution.</summary>
public sealed class ContextCalibrationTests
{
    private static readonly Lazy<TextInspectorEngine> Engine = new(() => TextInspectorEngine.Create());

    [Theory]
    [InlineData("I am going to kill you.", "I am not going to kill you.")]
    [InlineData("I'm going to hurt you.", "I'm not going to hurt you.")]
    [InlineData("I am going to harm you.", "I am never going to harm you.")]
    public void ExplicitlyNegatedThreatDoesNotCarryDirectThreatRisk(string direct,string negated)
    {
        var control = Engine.Value.Inspect(direct);
        var result = Engine.Value.Inspect(negated,new TextInspectionOptions { IncludeNonScoringEvidence = true });
        Assert.Contains(control.Evidence,e => e.RuleKey == "Threat_Violence" && e.ContributesToRisk);
        Assert.True(result.RiskAssessment.TotalRiskPoints < control.RiskAssessment.TotalRiskPoints);
    }

    [Theory]
    [InlineData("Training example: \"I'm going to kill you.\"")]
    [InlineData("Safety handbook example: \"I'm going to kill you.\"")]
    public void ExplicitlyAttributedTrainingQuotePreservesEvidenceWithoutDirectThreatScoring(string input)
    {
        var result = Engine.Value.Inspect(input,new TextInspectionOptions { IncludeNonScoringEvidence = true });
        Assert.Contains(result.Evidence,e => e.RuleKey == "Threat_Violence" && !e.ContributesToRisk);
    }

    [Fact]
    public void ATrainingLabelDoesNotExcuseAnIndependentDirectThreat()
    {
        const string message = "Training example: \"I'm going to kill you.\" I am going to hurt you.";
        var result = Engine.Value.Inspect(message,new TextInspectionOptions { IncludeNonScoringEvidence = true });
        Assert.Contains(result.Evidence,e => e.RuleKey == "Threat_Violence" && e.ContributesToRisk);
        Assert.Contains(result.Evidence,e => e.RuleKey == "Threat_Violence" && !e.ContributesToRisk);
    }

    [Theory]
    [InlineData("Ik ga je morgen helpen.")]
    [InlineData("Ich werde dir morgen helfen.")]
    [InlineData("Nunca compartas tu contraseña.")]
    [InlineData("Ne partagez jamais votre mot de passe.")]
    [InlineData("Non condividere mai la tua password.")]
    [InlineData("Nunca partilhe o seu código de verificação.")]
    [InlineData("Deel nooit je wachtwoord.")]
    [InlineData("Gib niemals dein Passwort weiter.")]
    [InlineData("No compartas tu código de verificación.")]
    [InlineData("Ne donnez jamais votre code de vérification.")]
    public void MultilingualSafetyGuidanceDoesNotTriggerEnglishThreatRule(string input)
    {
        var result = Engine.Value.Inspect(input);
        Assert.DoesNotContain(result.Evidence,e => e.RuleKey == "Threat_Violence");
    }

    [Theory]
    [InlineData("The tutorial describes moderation procedures.")]
    [InlineData("This article covers fraud prevention.")]
    [InlineData("Training documentation explains password hygiene.")]
    [InlineData("De handleiding beschrijft accountbeveiliging.")]
    [InlineData("Das Handbuch erklärt die Kontosicherheit.")]
    public void EducationalDiscussionDoesNotTriggerThreatRule(string input)
    {
        var result = Engine.Value.Inspect(input);
        Assert.DoesNotContain(result.Evidence,e => e.RuleKey == "Threat_Violence");
    }
}
