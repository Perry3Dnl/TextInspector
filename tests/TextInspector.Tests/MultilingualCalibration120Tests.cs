using Xunit;

namespace TextInspector.Tests;

/// <summary>
/// Curated multilingual control corpus: 80 non-threatening messages and 40
/// mixed-language messages containing an explicit English direct threat.
/// This validates cross-language collision resistance, not translation recall.
/// </summary>
public sealed class MultilingualCalibration120Tests
{
    private static readonly Lazy<TextInspectorEngine> Engine = new(() => TextInspectorEngine.Create());
    public static IEnumerable<object[]> Cases()
    {
        yield return new object[] { "M001", "nl", "De handleiding legt de beveiliging uit.", false };
        yield return new object[] { "M002", "nl", "De klantenservice is vandaag bereikbaar.", false };
        yield return new object[] { "M003", "nl", "Stuur nooit je wachtwoord naar iemand.", false };
        yield return new object[] { "M004", "nl", "De vergadering begint morgen.", false };
        yield return new object[] { "M005", "nl", "We bespreken een fictieve bedreiging.", false };
        yield return new object[] { "M006", "nl", "Dit is een educatieve waarschuwing.", false };
        yield return new object[] { "M007", "nl", "Het rapport bevat een geciteerd voorbeeld.", false };
        yield return new object[] { "M008", "nl", "Ik ga je morgen helpen.", false };
        yield return new object[] { "M009", "nl", "De cursus behandelt online veiligheid.", false };
        yield return new object[] { "M010", "nl", "Het team heeft de documentatie bijgewerkt.", false };
        yield return new object[] { "M011", "de", "Das Handbuch erklärt die Sicherheit.", false };
        yield return new object[] { "M012", "de", "Der Kundendienst ist heute erreichbar.", false };
        yield return new object[] { "M013", "de", "Teile niemals dein Passwort.", false };
        yield return new object[] { "M014", "de", "Die Besprechung beginnt morgen.", false };
        yield return new object[] { "M015", "de", "Wir besprechen eine fiktive Drohung.", false };
        yield return new object[] { "M016", "de", "Dies ist eine pädagogische Warnung.", false };
        yield return new object[] { "M017", "de", "Der Bericht enthält ein zitiertes Beispiel.", false };
        yield return new object[] { "M018", "de", "Ich werde dir morgen helfen.", false };
        yield return new object[] { "M019", "de", "Der Kurs behandelt Online-Sicherheit.", false };
        yield return new object[] { "M020", "de", "Das Team hat die Dokumentation aktualisiert.", false };
        yield return new object[] { "M021", "es", "El manual explica la seguridad.", false };
        yield return new object[] { "M022", "es", "El soporte está disponible hoy.", false };
        yield return new object[] { "M023", "es", "Nunca compartas tu contraseña.", false };
        yield return new object[] { "M024", "es", "La reunión comienza mañana.", false };
        yield return new object[] { "M025", "es", "Comentamos una amenaza ficticia.", false };
        yield return new object[] { "M026", "es", "Esta es una advertencia educativa.", false };
        yield return new object[] { "M027", "es", "El informe contiene una cita.", false };
        yield return new object[] { "M028", "es", "Te ayudaré mañana.", false };
        yield return new object[] { "M029", "es", "El curso trata sobre seguridad digital.", false };
        yield return new object[] { "M030", "es", "El equipo actualizó la documentación.", false };
        yield return new object[] { "M031", "fr", "Le manuel explique la sécurité.", false };
        yield return new object[] { "M032", "fr", "Le support est disponible aujourd'hui.", false };
        yield return new object[] { "M033", "fr", "Ne partagez jamais votre mot de passe.", false };
        yield return new object[] { "M034", "fr", "La réunion commence demain.", false };
        yield return new object[] { "M035", "fr", "Nous parlons d'une menace fictive.", false };
        yield return new object[] { "M036", "fr", "Ceci est un avertissement pédagogique.", false };
        yield return new object[] { "M037", "fr", "Le rapport contient une citation.", false };
        yield return new object[] { "M038", "fr", "Je vais vous aider demain.", false };
        yield return new object[] { "M039", "fr", "Le cours traite de la sécurité numérique.", false };
        yield return new object[] { "M040", "fr", "L'équipe a actualisé la documentation.", false };
        yield return new object[] { "M041", "it", "Il manuale spiega la sicurezza.", false };
        yield return new object[] { "M042", "it", "L'assistenza è disponibile oggi.", false };
        yield return new object[] { "M043", "it", "Non condividere mai la tua password.", false };
        yield return new object[] { "M044", "it", "La riunione inizia domani.", false };
        yield return new object[] { "M045", "it", "Parliamo di una minaccia fittizia.", false };
        yield return new object[] { "M046", "it", "Questo è un avviso educativo.", false };
        yield return new object[] { "M047", "it", "Il rapporto contiene una citazione.", false };
        yield return new object[] { "M048", "it", "Ti aiuterò domani.", false };
        yield return new object[] { "M049", "it", "Il corso riguarda la sicurezza online.", false };
        yield return new object[] { "M050", "it", "Il team ha aggiornato la documentazione.", false };
        yield return new object[] { "M051", "pt", "O manual explica a segurança.", false };
        yield return new object[] { "M052", "pt", "O suporte está disponível hoje.", false };
        yield return new object[] { "M053", "pt", "Nunca partilhe a sua palavra-passe.", false };
        yield return new object[] { "M054", "pt", "A reunião começa amanhã.", false };
        yield return new object[] { "M055", "pt", "Discutimos uma ameaça fictícia.", false };
        yield return new object[] { "M056", "pt", "Este é um aviso educativo.", false };
        yield return new object[] { "M057", "pt", "O relatório contém uma citação.", false };
        yield return new object[] { "M058", "pt", "Vou ajudar-te amanhã.", false };
        yield return new object[] { "M059", "pt", "O curso trata da segurança digital.", false };
        yield return new object[] { "M060", "pt", "A equipa atualizou a documentação.", false };
        yield return new object[] { "M061", "sv", "Handboken förklarar säkerheten.", false };
        yield return new object[] { "M062", "sv", "Supporten är tillgänglig idag.", false };
        yield return new object[] { "M063", "sv", "Dela aldrig ditt lösenord.", false };
        yield return new object[] { "M064", "sv", "Mötet börjar imorgon.", false };
        yield return new object[] { "M065", "sv", "Vi diskuterar ett fiktivt hot.", false };
        yield return new object[] { "M066", "sv", "Det här är en pedagogisk varning.", false };
        yield return new object[] { "M067", "sv", "Rapporten innehåller ett citat.", false };
        yield return new object[] { "M068", "sv", "Jag ska hjälpa dig imorgon.", false };
        yield return new object[] { "M069", "sv", "Kursen handlar om digital säkerhet.", false };
        yield return new object[] { "M070", "sv", "Teamet har uppdaterat dokumentationen.", false };
        yield return new object[] { "M071", "pl", "Instrukcja wyjaśnia zasady bezpieczeństwa.", false };
        yield return new object[] { "M072", "pl", "Pomoc techniczna jest dziś dostępna.", false };
        yield return new object[] { "M073", "pl", "Nigdy nie udostępniaj hasła.", false };
        yield return new object[] { "M074", "pl", "Spotkanie zaczyna się jutro.", false };
        yield return new object[] { "M075", "pl", "Omawiamy fikcyjną groźbę.", false };
        yield return new object[] { "M076", "pl", "To ostrzeżenie edukacyjne.", false };
        yield return new object[] { "M077", "pl", "Raport zawiera cytat.", false };
        yield return new object[] { "M078", "pl", "Pomogę ci jutro.", false };
        yield return new object[] { "M079", "pl", "Kurs dotyczy bezpieczeństwa w sieci.", false };
        yield return new object[] { "M080", "pl", "Zespół zaktualizował dokumentację.", false };
        yield return new object[] { "M081", "nl", "De handleiding legt de beveiliging uit. I'm going to kill you.", true };
        yield return new object[] { "M082", "nl", "De klantenservice is vandaag bereikbaar. I'm going to kill you.", true };
        yield return new object[] { "M083", "nl", "Stuur nooit je wachtwoord naar iemand. I'm going to kill you.", true };
        yield return new object[] { "M084", "nl", "De vergadering begint morgen. I'm going to kill you.", true };
        yield return new object[] { "M085", "nl", "We bespreken een fictieve bedreiging. I'm going to kill you.", true };
        yield return new object[] { "M086", "de", "Das Handbuch erklärt die Sicherheit. I'm going to kill you.", true };
        yield return new object[] { "M087", "de", "Der Kundendienst ist heute erreichbar. I'm going to kill you.", true };
        yield return new object[] { "M088", "de", "Teile niemals dein Passwort. I'm going to kill you.", true };
        yield return new object[] { "M089", "de", "Die Besprechung beginnt morgen. I'm going to kill you.", true };
        yield return new object[] { "M090", "de", "Wir besprechen eine fiktive Drohung. I'm going to kill you.", true };
        yield return new object[] { "M091", "es", "El manual explica la seguridad. I'm going to kill you.", true };
        yield return new object[] { "M092", "es", "El soporte está disponible hoy. I'm going to kill you.", true };
        yield return new object[] { "M093", "es", "Nunca compartas tu contraseña. I'm going to kill you.", true };
        yield return new object[] { "M094", "es", "La reunión comienza mañana. I'm going to kill you.", true };
        yield return new object[] { "M095", "es", "Comentamos una amenaza ficticia. I'm going to kill you.", true };
        yield return new object[] { "M096", "fr", "Le manuel explique la sécurité. I'm going to kill you.", true };
        yield return new object[] { "M097", "fr", "Le support est disponible aujourd'hui. I'm going to kill you.", true };
        yield return new object[] { "M098", "fr", "Ne partagez jamais votre mot de passe. I'm going to kill you.", true };
        yield return new object[] { "M099", "fr", "La réunion commence demain. I'm going to kill you.", true };
        yield return new object[] { "M100", "fr", "Nous parlons d'une menace fictive. I'm going to kill you.", true };
        yield return new object[] { "M101", "it", "Il manuale spiega la sicurezza. I'm going to kill you.", true };
        yield return new object[] { "M102", "it", "L'assistenza è disponibile oggi. I'm going to kill you.", true };
        yield return new object[] { "M103", "it", "Non condividere mai la tua password. I'm going to kill you.", true };
        yield return new object[] { "M104", "it", "La riunione inizia domani. I'm going to kill you.", true };
        yield return new object[] { "M105", "it", "Parliamo di una minaccia fittizia. I'm going to kill you.", true };
        yield return new object[] { "M106", "pt", "O manual explica a segurança. I'm going to kill you.", true };
        yield return new object[] { "M107", "pt", "O suporte está disponível hoje. I'm going to kill you.", true };
        yield return new object[] { "M108", "pt", "Nunca partilhe a sua palavra-passe. I'm going to kill you.", true };
        yield return new object[] { "M109", "pt", "A reunião começa amanhã. I'm going to kill you.", true };
        yield return new object[] { "M110", "pt", "Discutimos uma ameaça fictícia. I'm going to kill you.", true };
        yield return new object[] { "M111", "sv", "Handboken förklarar säkerheten. I'm going to kill you.", true };
        yield return new object[] { "M112", "sv", "Supporten är tillgänglig idag. I'm going to kill you.", true };
        yield return new object[] { "M113", "sv", "Dela aldrig ditt lösenord. I'm going to kill you.", true };
        yield return new object[] { "M114", "sv", "Mötet börjar imorgon. I'm going to kill you.", true };
        yield return new object[] { "M115", "sv", "Vi diskuterar ett fiktivt hot. I'm going to kill you.", true };
        yield return new object[] { "M116", "pl", "Instrukcja wyjaśnia zasady bezpieczeństwa. I'm going to kill you.", true };
        yield return new object[] { "M117", "pl", "Pomoc techniczna jest dziś dostępna. I'm going to kill you.", true };
        yield return new object[] { "M118", "pl", "Nigdy nie udostępniaj hasła. I'm going to kill you.", true };
        yield return new object[] { "M119", "pl", "Spotkanie zaczyna się jutro. I'm going to kill you.", true };
        yield return new object[] { "M120", "pl", "Omawiamy fikcyjną groźbę. I'm going to kill you.", true };
    }
    [Theory]
    [MemberData(nameof(Cases))]
    public void ThreatPatternDoesNotCollideWithBenignForeignText(
        string id,string language,string message,bool expectedThreat)
    {
        var result = Engine.Value.Inspect(message,new TextInspectionOptions { IncludeNonScoringEvidence = true });
        bool found = result.Evidence.Any(e => e.RuleKey == "Threat_Violence");
        Assert.True(found == expectedThreat,
            $"{id} ({language}): expected Threat_Violence={expectedThreat}, actual={found}: {message}");
    }
}
