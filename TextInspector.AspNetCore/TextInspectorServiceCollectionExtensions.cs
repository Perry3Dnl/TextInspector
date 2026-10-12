using Microsoft.Extensions.DependencyInjection;

namespace TextInspector.AspNetCore;

public static class TextInspectorServiceCollectionExtensions
{
    /// <summary>Registers one immutable Lite service; no database or editable rule repository.</summary>
    public static IServiceCollection AddTextInspector(this IServiceCollection services, Action<TextInspectorLiteOptions>? configure = null)
    {
        if(services is null) throw new ArgumentNullException(nameof(services));
        var options = new TextInspectorLiteOptions();
        configure?.Invoke(options);
        var service = new TextInspectorService(options);
        services.AddSingleton(service);
        services.AddSingleton<ITextInspectorService>(service);
        services.AddSingleton<ITextInspectorDetectionService>(service);
        services.AddSingleton<ITextInspectorReportService>(service);
        return services;
    }
}
