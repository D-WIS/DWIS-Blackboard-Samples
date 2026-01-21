# DWIS-Blackboard-Samples
Sample .NET 8 console apps showing how to interact with the DWIS semantic blackboard using the reference client.

## Samples
- `Samples/AcquireSignals/` – subscribe to signals via SPARQL query, print values periodically.
- `Samples/GetQueryResults/` – resolve SPARQL queries and display results (includes sample OPC UA config in `config/`).
- `Samples/InjectManifestAndWriteData/` – inject a manifest, then update provided variables periodically (sample OPC UA config in `config/`).
- `Samples/Samples.sln` – solution for building all samples.

## Build & run
From `Samples/`:
- Build: `dotnet build Samples.sln`
- Run a sample, e.g.: `dotnet run --project AcquireSignals/AcquireSignals.csproj`

## Prereqs
- DWIS Blackboard reachable via OPC UA.
- `Quickstarts.ReferenceClient.Config.xml` in each sample’s `config/` points to your server; adjust endpoints/credentials as needed.
- Reference client packages resolved via NuGet; if offline, add `../LocalPackages` from the clients repo or the bundled reference packages as a source.