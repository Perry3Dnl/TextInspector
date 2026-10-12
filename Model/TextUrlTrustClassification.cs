namespace TextInspector;

/// <summary>Relationship between an observed URL host and the configured application domains.</summary>
public enum TextUrlTrustClassification
{
    /// <summary>No first-party/trusted domain baseline was configured.</summary>
    Unknown = 0,

    /// <summary>The host is the configured first-party domain or one of its subdomains.</summary>
    FirstParty = 1,

    /// <summary>The host is explicitly trusted but is not first-party.</summary>
    Trusted = 2,

    /// <summary>The host is outside all configured first-party and trusted domains.</summary>
    External = 3
}
