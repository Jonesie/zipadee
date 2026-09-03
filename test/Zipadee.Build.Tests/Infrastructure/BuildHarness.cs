namespace Zipadee.Build.Tests.Infrastructure;

internal sealed record BuildResult(int ExitCode, string Output, string ProjectDirectory)
{
    public bool Succeeded => ExitCode == 0;

    /// <summary>
    /// Every fixture project builds with an unset Configuration (dotnet build's own default,
    /// Debug) and Microsoft.Build.NoTargets doesn't append $(TargetFramework) to $(OutDir) the
    /// way a normal compiled project does - confirmed empirically throughout this project's own
    /// manual testing (e.g. sample/ZipadeeSample/*): output always lands directly in
    /// bin\Debug\, never bin\Debug\net10.0\.
    /// </summary>
    public string OutputDirectory => Path.Combine(ProjectDirectory, "bin", "Debug");

    /// <summary>
    /// Same reasoning as <see cref="OutputDirectory"/> - Microsoft.Build.NoTargets doesn't
    /// append $(TargetFramework), so intermediate output (including the generated Cab DDF) lands
    /// directly in obj\Debug\.
    /// </summary>
    public string IntermediateDirectory => Path.Combine(ProjectDirectory, "obj", "Debug");
}

/// <summary>
/// Builds a fixture project (already copied into an isolated <see cref="FixtureWorkspace"/>) via
/// a real `dotnet build`, with MSBuild properties overridden on the command line - the same
/// mechanism used to manually verify every Zipadee feature throughout this project's development.
/// </summary>
internal static class BuildHarness
{
    public static BuildResult Build(
        FixtureWorkspace workspace,
        string relativeProjectPath,
        string zipadeeBuildVersion,
        IReadOnlyDictionary<string, string>? properties = null)
    {
        var projectPath = workspace.ProjectPath(relativeProjectPath);
        var projectDirectory = Path.GetDirectoryName(projectPath)!;

        var allProperties = new Dictionary<string, string>(properties ?? new Dictionary<string, string>())
        {
            ["ZipadeeBuildTestVersion"] = zipadeeBuildVersion,
        };

        // A single -p: switch can set multiple properties at once as "Name1=Value1;Name2=Value2"
        // - MSBuild's own parser for that switch splits on ';' to find each pair, regardless of
        // OS-level shell quoting around the whole argument. A property value that itself contains
        // a literal ';' (e.g. ZipadeeProjectOutputExclude's pattern list) needs it escaped as
        // %3B, or MSBuild misreads the text after it as the start of a new (invalid) property -
        // confirmed empirically, and the same trick Zipadee.Build.targets itself already uses for
        // ZipadeePasswordEnv for exactly this reason.
        var propertyArgs = string.Join(' ', allProperties.Select(kv => $"\"-p:{kv.Key}={kv.Value.Replace(";", "%3B")}\""));

        var result = ProcessRunner.Run("dotnet", $"build \"{projectPath}\" {propertyArgs}", projectDirectory);

        return new BuildResult(result.ExitCode, result.CombinedOutput, projectDirectory);
    }
}
