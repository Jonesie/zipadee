# Zipadee.Build.Tests

Integration tests for `Zipadee.Build`. Since almost all of Zipadee's actual archiving logic lives
in MSBuild XML (`Zipadee.Build.targets`), not C#, these tests work by actually building real
fixture `.zparchproj` projects (under `Fixtures/`) via `dotnet build`, varying MSBuild properties
per test, then inspecting the produced archive - the same way every feature in this repo has been
manually verified throughout its development. See issue #21 for the background on why this
approach was chosen over refactoring the archiving logic into C# just to get a coverage number.

## Running locally

Needs the same external tools any real Zipadee consumer needs on `PATH` - neither installer adds
itself there automatically:

- [7-Zip](https://www.7-zip.org/)'s command-line tool (`7z`) - used both to build `Zip`/`SevenZip`/
  `Tar`/`GZip` fixtures and, since it can read every format Zipadee produces (including `.rar` and
  `.cab`), to inspect the results of every test regardless of format.
- [WinRAR](https://www.rarlab.com/download.htm)'s command-line tool (`rar`) - needed to build `Rar`
  fixtures (trial mode's CLI is unrestricted for this).

`Cab` needs nothing extra (`makecab.exe` ships with Windows).

Then:

```
dotnet test test/Zipadee.Build.Tests/Zipadee.Build.Tests.csproj
```

With a coverage report (matches what CI collects - `coverlet.runsettings` enables
`IncludeTestAssembly`, since the C# actually worth measuring here, `Infrastructure/`, lives in the
test assembly itself, not a separate library coverlet would pick up by default):

```
dotnet test test/Zipadee.Build.Tests/Zipadee.Build.Tests.csproj --collect:"XPlat Code Coverage" --settings test/Zipadee.Build.Tests/coverlet.runsettings --results-directory TestResults
```

## How it works

- `Infrastructure/ZipadeeBuildPackFixture.cs` packs the **current** `Zipadee.Build` source into
  the repo's `LocalPackages` feed once per test run, under a version generated at run time
  (`0.0.0-test<timestamp>`) that no NuGet cache has ever seen - every fixture always tests today's
  code, never a stale cached package.
- `Infrastructure/FixtureWorkspace.cs` copies the whole `Fixtures/` tree into an isolated temp
  directory per test (fixture projects reference each other by relative path - `StubArchive`'s
  `ProjectReference` to `StubApp`, its `%(Link)` to `Shared/Linked.txt`, its wildcard link to
  `ExternalFolder/` - so the whole tree needs to move together, not just the one project under
  test) and drops a `nuget.config` there pointing at `LocalPackages` by absolute path.
- `Infrastructure/BuildHarness.cs` runs `dotnet build` against a fixture in its isolated copy,
  with MSBuild properties overridden via `-p:`.
- `Infrastructure/ArchiveInspector.cs` wraps `7z.exe` to test/list/extract whatever came out -
  confirmed empirically that 7z can read every format Zipadee can produce.

All test classes share one xUnit collection (`ZipadeeBuildPackCollection`), which both shares the
one pack across the whole run and serializes every test in the project - correctness over speed
for a first pass. If the suite gets slow enough to matter, splitting into a handful of
collections (so tests within a feature area stay serialized but different areas can run in
parallel) is the natural next step - the version-per-run packing already makes that safe to do,
since nothing depends on build order across areas.
