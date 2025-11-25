using DWIS.Client.ReferenceImplementation.OPCFoundation;
using DWIS.Client.ReferenceImplementation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using Org.BouncyCastle.Security;
using DWIS.SPARQL.Utils;
using DWIS.API.DTO;
using Spectre.Console;


var builder = Host.CreateDefaultBuilder();
builder.ConfigureServices(services =>
services.AddLogging(config => config.AddConsole().SetMinimumLevel(LogLevel.Warning)));

var host = builder.Build();
var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
var logger = loggerFactory.CreateLogger<Program>();

//var voc = DWIS.Vocabulary.sta

var configuration = DefaultDWISClientConfiguration.LoadDefault();
if (args != null && args.Length == 1)
{
    configuration.ServerAddress = args[0];
}

IOPCUADWISClient client = new DWISClientOPCF(configuration, loggerFactory.CreateLogger<DWISClientOPCF>());



bool quit = false;

string lineStart = "dwis-blackboard> ";

var rootCommand = new RootCommand("Sample app for get query results method");

var printQueryCommand = new Command("print-query");
rootCommand.AddCommand(printQueryCommand);

var printResultsCommand = new Command("print-query-results");
rootCommand.AddCommand(printResultsCommand);

var prototypeArgument = new Argument<string>("prototype", "The prototype noun to be used");
var measurementOption = new Option<bool>("-m", getDefaultValue: () => false, description:"Add the ddhub:Measurement pattern to the query.");
var getUnitsOptions = new Option<bool>("-u", getDefaultValue: () => false);
var getSignalOptions = new Option<bool>("-s", getDefaultValue: () => true);
var getDataPointOptions = new Option<bool>("-d", getDefaultValue: () => true);

printQueryCommand.AddArgument(prototypeArgument);
printQueryCommand.AddOption(measurementOption);
printQueryCommand.AddOption(getUnitsOptions);
printQueryCommand.AddOption(getSignalOptions);
printQueryCommand.AddOption(getDataPointOptions);
printQueryCommand.SetHandler(PrintQuery, prototypeArgument, measurementOption, getUnitsOptions, getSignalOptions, getDataPointOptions);


printResultsCommand.AddArgument(prototypeArgument);
printResultsCommand.AddOption(measurementOption);
printResultsCommand.AddOption(getUnitsOptions);
printResultsCommand.AddOption(getSignalOptions);
printResultsCommand.AddOption(getDataPointOptions);
printResultsCommand.SetHandler(PrintQueryResults, prototypeArgument, measurementOption, getUnitsOptions, getSignalOptions, getDataPointOptions);


//quit
var exitCommand = new Command("exit");
exitCommand.AddAlias("quit");
exitCommand.SetHandler(() => quit = true);
rootCommand.AddCommand(exitCommand);

AnsiConsole.Write(
new FigletText("D-WIS Blackboard")
.LeftJustified()
.Color(Color.Red));

while (!quit)
{
    AnsiConsole.MarkupInterpolated($"[bold cyan]{lineStart}[/]");
    string? rd = Console.ReadLine();
    if (!string.IsNullOrEmpty(rd))
    {
        await rootCommand.InvokeAsync(rd.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}


string GetQuery(string prototype, bool measurement, bool getunits, bool getsignal, bool getdatapoint )
{
    QueryBuilder queryBuilder = new QueryBuilder();
    if (getsignal) { queryBuilder.SelectSignal(); }
    if (getdatapoint) { queryBuilder.SelectDataPoint(); }
    if (measurement) { queryBuilder.AddMeasuredClass(prototype); }
    else { queryBuilder.AddClasses(prototype); }

    return queryBuilder.Build();
}

void PrintQuery(string prototype, bool measurement, bool getunits, bool getsignal, bool getdatapoint)
{
    string query = GetQuery(prototype, measurement, getunits, getsignal, getdatapoint);
    Console.WriteLine(query);   
}

void PrintQueryResults(string prototype, bool measurement, bool getunits, bool getsignal, bool getdatapoint)
{
    string query = GetQuery(prototype, measurement, getunits, getsignal, getdatapoint);
    var queryResult = client.GetQueryResult(query);
    if (queryResult != null)
    {
        Table table = new Table();
        table.AddColumns(queryResult.VariablesHeader.ToArray());
        foreach (var row in queryResult)
        {
            table.AddRow(row.Items.Select(i => i.NameSpace + i.ID).ToArray());
        }
        AnsiConsole.Write(table);
    }
}


