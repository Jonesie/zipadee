namespace Zipadee.Build.Tests.Infrastructure;

/// <summary>
/// Copies the entire Fixtures tree into a fresh temp directory per test, so every test builds in
/// isolation - StubArchive's ProjectReference, %(Link), and wildcard-folder-link all depend on
/// relative paths to sibling fixture folders (StubApp, Shared, ExternalFolder), so the whole tree
/// is copied together to keep that layout intact, not just the one project being built.
///
/// Also drops a nuget.config next to the copy pointing at the real repo's LocalPackages feed by
/// absolute path - a copy living under %TEMP% has no ancestor nuget.config of its own, so without
/// this the fixture's PackageReference to Zipadee.Build (which only ever exists in that local
/// feed) would fail to restore.
/// </summary>
internal sealed class FixtureWorkspace : IDisposable
{
    public string Root { get; }

    public FixtureWorkspace()
    {
        Root = Path.Combine(Path.GetTempPath(), "zipadee-build-tests", Guid.NewGuid().ToString("N"));
        CopyDirectory(RepoPaths.FixturesRoot, Root);
        WriteNugetConfig();
    }

    public string ProjectPath(string relativeProjectPath) => Path.Combine(Root, relativeProjectPath);

    /// <summary>
    /// Writes a file into the copy after it's already been made - e.g. a large blob dropped into
    /// StubArchive/volume-data/ (an otherwise-empty wildcard Content include) so MaxVolumeSizeTests
    /// has something big enough to force volume spanning, without a large binary fixture file
    /// sitting in source control for every other test that never needs it.
    /// </summary>
    public string WriteFile(string relativePath, byte[] content)
    {
        var path = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, content);
        return path;
    }

    private void WriteNugetConfig()
    {
        var xml = $"""
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
                <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
                <add key="ZipadeeLocal" value="{RepoPaths.LocalPackages}" />
              </packageSources>
            </configuration>
            """;
        File.WriteAllText(Path.Combine(Root, "nuget.config"), xml);
    }

    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (var sourceDir in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(destination, Path.GetRelativePath(source, sourceDir)));
        }

        foreach (var sourceFile in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            File.Copy(sourceFile, Path.Combine(destination, Path.GetRelativePath(source, sourceFile)), overwrite: true);
        }
    }

    public void Dispose()
    {
        try { Directory.Delete(Root, recursive: true); } catch { /* best-effort cleanup */ }
    }
}
