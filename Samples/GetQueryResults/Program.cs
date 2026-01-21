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
rootCommand.Subcommands.Add(printQueryCommand);

var printResultsCommand = new Command("print-query-results");
rootCommand.Subcommands.Add(printResultsCommand);

var prototypeArgument = new Argument<string>("prototype") { Description = "The prototype noun to be used" };
var measurementOption = new Option<bool>("-m")
{
    DefaultValueFactory = (b) => false,
    Description = "Add the ddhub:Measurement pattern to the query."
};//,  getDefaultValue: () => false, description:"Add the ddhub:Measurement pattern to the query.");
var getUnitsOptions = new Option<bool>("-u") { DefaultValueFactory = (b) => false };
var getSignalOptions = new Option<bool>("-s") { DefaultValueFactory = (b) => true };
var getDataPointOptions = new Option<bool>("-d") { DefaultValueFactory = (b) => false };

printQueryCommand.Arguments.Add(prototypeArgument);
printQueryCommand.Options.Add(measurementOption);
printQueryCommand.Options.Add(getUnitsOptions);
printQueryCommand.Options.Add(getSignalOptions);
printQueryCommand.Options.Add(getDataPointOptions);
printQueryCommand.SetAction( parseResults =>      
    PrintQuery(
        parseResults.GetValue(prototypeArgument), 
        parseResults.GetValue(measurementOption), 
        parseResults.GetValue(getUnitsOptions), 
        parseResults.GetValue(getSignalOptions), 
        parseResults.GetValue(getDataPointOptions)));


printResultsCommand.Arguments.Add(prototypeArgument);
printResultsCommand.Options.Add(measurementOption);
printResultsCommand.Options.Add(getUnitsOptions);
printResultsCommand.Options.Add(getSignalOptions);
printResultsCommand.Options.Add(getDataPointOptions);
printResultsCommand.SetAction(parseResults =>
    PrintQueryResults(
        parseResults.GetValue(prototypeArgument),
        parseResults.GetValue(measurementOption),
        parseResults.GetValue(getUnitsOptions),
        parseResults.GetValue(getSignalOptions),
        parseResults.GetValue(getDataPointOptions)));

//quit
var exitCommand = new Command("exit");
exitCommand.Aliases.Add("quit");
exitCommand.SetAction(parseResults => quit = true);
rootCommand.Subcommands.Add(exitCommand);

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
        await rootCommand.Parse(rd.Split(' ', StringSplitOptions.RemoveEmptyEntries)).InvokeAsync();
    }
}


string GetQuery(string prototype, bool measurement, bool getunits, bool getsignal, bool getdatapoint )
{
    QueryBuilder queryBuilder = new QueryBuilder();
    if (getsignal) { queryBuilder.SelectSignal(); }
    if (getdatapoint) { queryBuilder.SelectDataPoint(); }
    if (getunits) {  queryBuilder.SelectUnit(); }
    if (measurement) { queryBuilder.AddMeasuredClass(prototype); }
    else { queryBuilder.AddClasses(prototype); }

    return queryBuilder.Build();
}

void PrintQuery(string? prototype, bool measurement, bool getunits, bool getsignal, bool getdatapoint)
{
    if (!string.IsNullOrEmpty(prototype))
    {
        string query = GetQuery(prototype, measurement, getunits, getsignal, getdatapoint);
        Console.WriteLine(query);
    }
    else 
    {
        Console.WriteLine("Prototype cannot be null or empty");
    }
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


