using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class SfxTests(ZipadeeBuildPackFixture pack)
{
    [Fact]
    public void ProducesAGenuinelySelfExtractingExecutable()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = "SevenZip",
                ["ZipadeeCreateSfx"] = "true",
            });

        Assert.True(build.Succeeded, build.Output);

        var exePath = Path.Combine(build.OutputDirectory, "StubArchive.exe");
        Assert.True(File.Exists(exePath), build.Output);

        // Not just "some .exe exists" - actually run it and prove it extracts, rather than being
        // a bare SFX stub with no payload (confirmed empirically as a real failure mode when
        // ZipadeeCreateSfx was combined with ZipadeeMaxVolumeSize - see the validation error that
        // now guards against it).
        var extractDir = Path.Combine(workspace.Root, "sfx-extracted");
        var extract = ArchiveInspector.ExtractSfx(exePath, extractDir);
        Assert.True(extract.ExitCode == 0, extract.CombinedOutput);

        Assert.True(File.Exists(Path.Combine(extractDir, "Hello.txt")));
        Assert.True(File.Exists(Path.Combine(extractDir, "StubApp.exe")));

        // The extracted apphost is a real, runnable copy of StubApp - the deepest possible proof
        // the SFX actually contains a working payload, not just a stub.
        var run = ProcessRunner.Run(Path.Combine(extractDir, "StubApp.exe"), string.Empty, extractDir);
        Assert.Equal(0, run.ExitCode);
        Assert.Contains("stub-app", run.StdOut);
    }

    [Fact]
    public void WorksWithAPasswordToo()
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = "SevenZip",
                ["ZipadeeCreateSfx"] = "true",
                ["ZipadeePassword"] = "sfx-password",
            });

        Assert.True(build.Succeeded, build.Output);

        var exePath = Path.Combine(build.OutputDirectory, "StubArchive.exe");
        var extractDir = Path.Combine(workspace.Root, "sfx-extracted");

        // The console SFX module prompts for a password interactively rather than accepting one
        // as a plain command-line switch, so this goes through 7z itself (which can open an SFX
        // .exe as an ordinary 7z archive) instead of running the .exe directly.
        var extract = ArchiveInspector.Extract(exePath, extractDir, "sfx-password");
        Assert.True(extract.ExitCode == 0, extract.CombinedOutput);
        Assert.True(File.Exists(Path.Combine(extractDir, "Hello.txt")));
    }
}
