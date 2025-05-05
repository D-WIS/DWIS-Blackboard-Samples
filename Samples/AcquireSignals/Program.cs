using DWIS.Client.ReferenceImplementation.OPCFoundation;
using DWIS.Client.ReferenceImplementation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DWIS.SPARQL.Utils;
using DWIS.Vocabulary.Schemas;
using DWIS.Vocabulary.Development;

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


var subscriptionData = GetSubscriptionData();

AcquiredSignals acquiredSignals = AcquiredSignals.CreateWithSubscription(subscriptionData.queries, subscriptionData.queryNames, 0, client);

PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(.5));

while (await timer.WaitForNextTickAsync())
{
    foreach (var data in acquiredSignals)
    {
        var val = data.Value;

        Console.WriteLine($"Signal name: {data.Key}");

        if (val != null)
        {
            foreach (var signal in val)
            {
                double s = signal.GetValue<double>(double.NaN);
                Console.WriteLine($"\t Value: {s}");
            }
        }
    }
}


Console.ReadLine();

(string[] queries, string[] queryNames) GetSubscriptionData()
{
    string[] nouns = new string[]{
    Nouns.BitDepth,Nouns.HookPosition,Nouns.HookVelocity,Nouns.FlowRateIn,Nouns.SPP,Nouns.SurfaceRPM,Nouns.HoleDepth
    };

    var queries = nouns.Select(noun => { return new QueryBuilder().SelectSignal().SelectDataPoint().AddMeasuredClass(noun).Build(); }).ToArray();
    var names = nouns;
    return (queries, nouns);
}