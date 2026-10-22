namespace TextInspector;

/// <summary>
/// Stable evidence keys emitted by deterministic URL intelligence. All built-in URL signals are
/// evidence-only; policies may explicitly consume them without introducing a second scoring model.
/// </summary>
public static class TextInspectorUrlSignalKeys
{
    public const string ExternalDomain = "Url_ExternalDomain";
    public const string FirstPartyLookalike = "Url_FirstPartyLookalike";
    public const string IpHost = "Url_IpHost";
    public const string PunycodeHost = "Url_PunycodeHost";
    public const string UserInfo = "Url_UserInfo";
    public const string UnusualPort = "Url_UnusualPort";
    public const string LoginOrVerificationPath = "Url_LoginOrVerificationPath";
    public const string KnownShortener = "Url_KnownShortener";

    public static IReadOnlyList<string> All { get; } = new[]
    {
        ExternalDomain,
        FirstPartyLookalike,
        IpHost,
        PunycodeHost,
        UserInfo,
        UnusualPort,
        LoginOrVerificationPath,
        KnownShortener
    };
}
