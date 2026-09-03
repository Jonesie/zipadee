using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class PasswordTests(ZipadeeBuildPackFixture pack)
{
    private const string Password = "correct-horse-battery-staple";

    [Theory]
    [InlineData("Zip", "StubArchive.zip")]
    [InlineData("SevenZip", "StubArchive.7z")]
    [InlineData("Rar", "StubArchive.rar")]
    public void CorrectPasswordExtractsAndWrongPasswordIsRejected(string format, string expectedFileName)
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = format,
                ["ZipadeePassword"] = Password,
            });

        Assert.True(build.Succeeded, build.Output);
        Assert.DoesNotContain(Password, build.Output);

        var archivePath = Path.Combine(build.OutputDirectory, expectedFileName);
        Assert.True(File.Exists(archivePath));

        var correct = ArchiveInspector.Extract(archivePath, Path.Combine(workspace.Root, "extracted-correct"), Password);
        Assert.True(correct.ExitCode == 0, correct.CombinedOutput);
        Assert.True(File.Exists(Path.Combine(workspace.Root, "extracted-correct", "Hello.txt")));

        var wrong = ArchiveInspector.Extract(archivePath, Path.Combine(workspace.Root, "extracted-wrong"), "definitely-the-wrong-password");
        Assert.NotEqual(0, wrong.ExitCode);
    }

    // SevenZip and Rar hide file names too (7-Zip's -mhe=on, RAR's -hp) when a password is set -
    // Zip is deliberately excluded here, since the .zip format itself has no header-encryption
    // concept at all (its central directory always stores names in the clear, regardless of AES
    // content encryption) - confirmed empirically, not a Zipadee gap. Content is still genuinely
    // encrypted for all three, per the wrong-password rejection covered above.
    [Theory]
    [InlineData("SevenZip", ".7z")]
    [InlineData("Rar", ".rar")]
    public void FileNamesAreEncryptedTooWhenAPasswordIsSet(string format, string extension)
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = format,
                ["ZipadeePassword"] = Password,
            });

        Assert.True(build.Succeeded, build.Output);

        var archivePath = Path.Combine(build.OutputDirectory, "StubArchive" + extension);

        var list = ArchiveInspector.List(archivePath);
        Assert.DoesNotContain("Hello.txt", list.StdOut);
    }
}
