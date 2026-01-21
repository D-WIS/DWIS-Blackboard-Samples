# DWIS-Blackboard-Samples
Sample .NET 8 console apps showing how to interact with the DWIS semantic blackboard using the reference client.

## Samples
- `Samples/AcquireSignals/` � subscribe to signals via SPARQL query, print values periodically.
- `Samples/GetQueryResults/` � resolve SPARQL queries and display results (includes sample OPC UA config in `config/`).
- `Samples/InjectManifestAndWriteData/` � inject a manifest, then update provided variables periodically (sample OPC UA config in `config/`).
- `Samples/Samples.sln` � solution for building all samples.

## Build & run
From `Samples/`:
- Build: `dotnet build Samples.sln`
- Run a sample, e.g.: `dotnet run --project AcquireSignals/AcquireSignals.csproj`

## Prereqs
- DWIS Blackboard reachable via OPC UA.
- `Quickstarts.ReferenceClient.Config.xml` in each sample�s `config/` points to your server; adjust endpoints/credentials as needed. **Important: you may have to change the `Copy to Output Directory` property of the configuration file to `Copy if newer`, or `Copy always`**
- Reference client packages resolved via NuGet; 

## Examples

### GetQueryResults

- `print-query SPP`: shows the SparQL query asking for a SPP signal. The result should look like: 
```sparql
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>

SELECT ?signal
WHERE {
                        ?dataPoint ddhub:HasDynamicValue ?signal .
                        ?dataPoint rdf:type ddhub:SPP .
}
```
- `print-query SPP -m true -d true`: shows the SparQL query asking for a **Measured** SPP, and the corresponding data point in addition to the signal:
```
PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
PREFIX ddhub: <http://ddhub.no/>

SELECT ?signal ?dataPoint
WHERE {
                        ?dataPoint ddhub:HasDynamicValue ?signal .
                        ?dataPoint rdf:type ddhub:Measurement .
                        ?dataPoint rdf:type ddhub:SPP .
}
```
-`print-query-results SPP -d true -m true`: displays the results of the query. Depending of the contents of your blackboard, the result can look like: 
```
┌───────────────────────────────────────────────┬─────────────────────────────┐
│ ?SIGNAL                                       │ ?DATAPOINT                  │
├───────────────────────────────────────────────┼─────────────────────────────┤
│ http://ddhub.no/openLAB/Variables/openLAB.SPP │ http://ddhub.no/openLAB/SPP │
└───────────────────────────────────────────────┴─────────────────────────────┘
```