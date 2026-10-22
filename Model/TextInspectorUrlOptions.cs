namespace TextInspector;

/// <summary>
/// Local deterministic URL intelligence configuration. Domain classification never performs DNS
/// or network I/O and does not define a separate score.
/// </summary>
public sealed class TextInspectorUrlOptions
{
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Application-owned domains. Exact hosts and subdomains are first-party; suffix text elsewhere
    /// in a hostname is not (for example google.nl.attacker.com is not a subdomain of google.nl).
    /// </summary>
    public string[] FirstPartyDomains { get; set; } = Array.Empty<string>();

    /// <summary>External domains explicitly trusted by the application.</summary>
    public string[] TrustedDomains { get; set; } = Array.Empty<string>();

    /// <summary>Known redirect/shortener hosts surfaced as supporting evidence.</summary>
    public string[] ShortenerDomains { get; set; } =
    {
        "bit.ly",
        "tinyurl.com",
        "t.co",
        "goo.gl",
        "rebrand.ly",
        "shorturl.at",
        "cutt.ly"
    };

    /// <summary>Path/query terms surfaced as URL intent evidence.</summary>
    public string[] SuspiciousPathTerms { get; set; } =
    {
        "login",
        "signin",
        "sign-in",
        "verify",
        "verification",
        "account",
        "secure",
        "password",
        "wallet",
        "payment"
    };
}
