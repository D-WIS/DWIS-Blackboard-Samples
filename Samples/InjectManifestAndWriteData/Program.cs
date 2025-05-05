using DWIS.API.DTO;
using DWIS.Client.ReferenceImplementation;
using DWIS.Client.ReferenceImplementation.OPCFoundation;
using DWIS.Vocabulary.Schemas;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;


var builder = Host.CreateDefaultBuilder();
builder.ConfigureServices(services => 
services.AddLogging(config => config.AddConsole().SetMinimumLevel(LogLevel.Information)));

var host = builder.Build();
var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger<Program>();



var configuration = DefaultDWISClientConfiguration.LoadDefault();
if (args != null && args.Length == 1)
{
    configuration.ServerAddress = args[0];
}
IOPCUADWISClient client = new DWISClientOPCF(configuration, loggerFactory.CreateLogger<DWISClientOPCF>());
var manifest = GetManifest();


logger.LogInformation("Inject manifest");
var res = client.Inject(manifest);
logger.LogInformation("Manifest injected");


Random random = new Random();
int prefix = random.Next();
PeriodicTimer periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(.5));
while (await periodicTimer.WaitForNextTickAsync()) 
{
    DateTime now = DateTime.Now;
    var data = manifest.ProvidedVariables.Select(pv => (pv.VariableID,(object)(prefix + random.NextDouble()), now)).ToList();

    foreach (var d in data)
    {
        logger.LogInformation($"Write data: {d.VariableID}, {d.Item2}");
    }

    client.UpdateProvidedVariables(data);
}

Console.ReadLine();

ManifestFile GetManifest()
{
    ManifestFile manifest = new ManifestFile()
    {
        InjectedNodes = new List<InjectedNode>(),
        InjectedReferences = new List<InjectedReference>(),
        InjectedVariables = new List<InjectedVariable>(),
        InjectionInformation = new InjectionInformation(),
        ProvidedVariables = new List<ProvidedVariable>(),
        ManifestName = "Sample-manifest",
        Provider = new InjectionProvider() { Company = "NORCE", Name = "SamplesApp" }
    };
    List<(string name, string prototype, string unit, string quantity)> injectionData = new List<(string name, string prototype, string unit, string quantity)>();


    injectionData.Add((Nouns.HookLoad, Nouns.HookLoad, Units.Kilogram, Quantities.HookLoadDrillingQuantity));
    injectionData.Add((Nouns.HookPosition, Nouns.HookPosition, Units.Metre, Quantities.PositionDrillingQuantity));
    injectionData.Add((Nouns.HookVelocity, Nouns.HookVelocity, Units.MetrePerSecond, Quantities.BlockVelocityDrillingQuantity));
    injectionData.Add((Nouns.FlowRateIn, Nouns.FlowRateIn, Units.CubicMetrePerSecond, Quantities.VolumetricFlowrateDrillingQuantity));
    injectionData.Add((Nouns.SPP, Nouns.SPP, Units.Pascal, Quantities.PressureDrillingQuantity));
    injectionData.Add((Nouns.SurfaceRPM, Nouns.SurfaceRPM, Units.RotationPerSecond, Quantities.RotationalFrequencyQuantity));
    injectionData.Add((Nouns.BitDepth, Nouns.BitDepth, Units.Metre, Quantities.DepthDrillingQuantity));
    injectionData.Add((Nouns.HoleDepth, Nouns.HoleDepth, Units.Metre, Quantities.DepthDrillingQuantity));

    foreach (var injection in injectionData)
    {
        manifest.InjectedNodes.Add(new InjectedNode() { BrowseName = injection.name, DisplayName = injection.name, TypeDictionaryURI = Nouns.DrillingDataPoint, UniqueName = injection.name });
        manifest.ProvidedVariables.Add(new ProvidedVariable() { DataType = "double", VariableID = injection.name });

        manifest.InjectedReferences.Add(new InjectedReference()
        {
            Subject = new NodeIdentifier() { ID = injection.name, NameSpace = manifest.InjectionInformation.InjectedNodesNamespaceAlias },
            VerbURI = "http://ddhub.no/" + Verbs.HasDynamicValue,
            Object = new NodeIdentifier() { ID = injection.name, NameSpace = manifest.InjectionInformation.ProvidedVariablesNamespaceAlias }
        });

        manifest.InjectedReferences.Add(new InjectedReference()
        {
            Subject = new NodeIdentifier() { ID = injection.name, NameSpace = manifest.InjectionInformation.InjectedNodesNamespaceAlias },
            VerbURI = "http://ddhub.no/" + Verbs.BelongsToClass,
            Object = new NodeIdentifier() { ID = "http://ddhub.no/"+ injection.prototype, NameSpace = "http://ddhub.no/" }
        });

        manifest.InjectedReferences.Add(new InjectedReference()
        {
            Subject = new NodeIdentifier() { ID = injection.name, NameSpace = manifest.InjectionInformation.InjectedNodesNamespaceAlias },
            VerbURI = "http://ddhub.no/" + Verbs.BelongsToClass,
            Object = new NodeIdentifier() { ID = "http://ddhub.no/" + Nouns.Measurement, NameSpace = "http://ddhub.no/" }
        });

        manifest.InjectedReferences.Add(new InjectedReference()
        {
            Subject = new NodeIdentifier() { ID = injection.name, NameSpace = manifest.InjectionInformation.InjectedNodesNamespaceAlias },
            VerbURI = "http://ddhub.no/" + Verbs.IsOfMeasurableQuantity,
            Object = new NodeIdentifier() { ID = injection.quantity, NameSpace = "http://ddhub.no/UnitAndQuantity/" }
        });
        manifest.InjectedReferences.Add(new InjectedReference()
        {
            Subject = new NodeIdentifier() { ID = injection.name, NameSpace = manifest.InjectionInformation.ProvidedVariablesNamespaceAlias },
            VerbURI = "http://ddhub.no/" + Verbs.HasUnitOfMeasure,
            Object = new NodeIdentifier() { ID = injection.unit, NameSpace = "http://ddhub.no/UnitAndQuantity/" }
        });
    }

    return manifest;
}

