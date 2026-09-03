namespace Zipadee.Build.Tests.Infrastructure;

/// <summary>
/// Locates repo-relative paths from wherever the test assembly happens to be running, rather than
/// hard-coding an absolute path to the repo (which would break the moment the working copy - or
/// its containing folder - moves, exactly as happened mid-session here).
/// </summary>
internal static class RepoPaths
{
    public static string Root { get; } = FindRoot();

    public static string LocalPackages => Path.Combine(Root, "LocalPackages");

    public static string ZipadeeBuildCsproj => Path.Combine(Root, "build", "Zipadee.Build", "Zipadee.Build.csproj");

    public static string FixturesRoot => Path.Combine(Root, "test", "Zipadee.Build.Tests", "Fixtures");

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Zipadee.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName
            ?? throw new InvalidOperationException(
                $"Could not locate the repo root (a Zipadee.slnx file) above '{AppContext.BaseDirectory}'.");
    }
}
