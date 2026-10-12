using TextInspector;
var engine = TextInspectorEngine.Create();
if(engine.CompiledRuleCount != 76) throw new Exception("Wrong TextInspector preset.");
var report = engine.CreateReport("Contact us at hello@example.com.");
if(!report.HasDetections || report.RequiresReview || report.RiskAssessment.TotalRiskPoints != 0) throw new Exception("Detection/risk contract failed.");
var threat = engine.CreateReport("I'm going to kill you.");
if(threat.Findings.Count == 0 || threat.Highlights.Count == 0) throw new Exception("Basic report failed.");
Console.WriteLine("TextInspector core smoke tests passed.");
