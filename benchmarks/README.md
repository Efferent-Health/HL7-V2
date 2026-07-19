Set of general benchmarks for testing both different SDK versions, and also looking for improvements

## How to run

```bash
dotnet run -c Release
```

To run a specific benchmark:

```bash
dotnet run -c Release -- --filter '*Serialize*'
dotnet run -c Release -- --filter '*ParseMessage*'
```

## Baseline comparison

Each benchmark suite defines two jobs:

- **NuGet 3.7.2** (baseline) — runs against the published NuGet package
- **Local** — runs against the local source tree

This is controlled by the `HL7V2Version` MSBuild property in `Benchmarks.csproj`:
- When `HL7V2Version` is set (e.g. `/p:HL7V2Version=3.7.2`), the project references the specified NuGet package.
- When it is not set (the default for the Local job), the project references the local source directly via `ProjectReference`.

## What is being measured

### SerializeBench

Measures `Encode()` throughput during serialization. The main improvement is a fast-path in `HL7Encoding.Encode()`: `IndexOfAny` checks upfront whether the value contains any characters that need escaping, skipping the full scan when none are found. The special-character array is also cached and only rebuilt when a delimiter property changes (dirty flag).

### ParseMessageBench

Measures parse + field access throughput. Exercises both 2-component (`Segment.Field`) and 3-component (`Segment.Field.Component`) query paths, plus segment occurrence indexing (`NTE[1].3`). On .NET 7+, query validation uses `[GeneratedRegex]`-backed methods instead of repeated `Regex.Matches` calls with inline pattern strings.
