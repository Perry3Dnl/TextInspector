using Microsoft.Extensions.DependencyInjection;
using TextInspector;
using TextInspector.AspNetCore;

var services = new ServiceCollection();
services.AddTextInspector(options => options.EnableMasking = true);
using var provider = services.BuildServiceProvider();
var service = provider.GetRequiredService<TextInspectorService>();
if(!ReferenceEquals(service,provider.GetRequiredService<ITextInspectorService>()) ||
   !ReferenceEquals(service,provider.GetRequiredService<ITextInspectorDetectionService>()) ||
   !ReferenceEquals(service,provider.GetRequiredService<ITextInspectorReportService>())) throw new Exception("DI must reuse one service.");
var report = provider.GetRequiredService<ITextInspectorReportService>().CreateReport("Contact hello@example.com.");
if(report.MaskedText is null || report.MaskedText.Contains("hello@example.com") || !report.HasDetections || report.RequiresReview) throw new Exception("Lite DI report failed.");
if(typeof(TextInspectorServiceCollectionExtensions).GetMethods().Any(method => method.Name.Contains("EntityFramework") || method.Name.Contains("Files"))) throw new Exception("Pro registration leaked.");
Console.WriteLine("Lite ASP.NET Core smoke tests passed.");
