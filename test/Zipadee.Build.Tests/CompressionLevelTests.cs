using Xunit;
using Zipadee.Build.Tests.Infrastructure;

namespace Zipadee.Build.Tests;

[Collection(ZipadeeBuildPackCollection.Name)]
public sealed class CompressionLevelTests(ZipadeeBuildPackFixture pack)
{
    [Theory]
    [InlineData("Store")]
    [InlineData("Fastest")]
    [InlineData("Fast")]
    [InlineData("Normal")]
    [InlineData("Maximum")]
    [InlineData("Ultra")]
    public void EveryLevelBuildsForEveryFormat(string level)
    {
        foreach (var format in new[] { "Zip", "SevenZip", "Tar", "GZip", "Rar", "Cab" })
        {
            using var workspace = new FixtureWorkspace();

            var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
                new Dictionary<string, string>
                {
                    ["ZipadeeOutputFormat"] = format,
                    ["ZipadeeCompressionLevel"] = level,
                });

            Assert.True(build.Succeeded, $"{format}/{level}:{Environment.NewLine}{build.Output}");
        }
    }

    // Cab is the one format with a genuinely different internal representation per level (MSZIP
    // vs LZX, plus LZX's CompressionMemory) - see the mapping table in marketplace/overview.md -
    // so it's worth asserting the generated DDF actually got the right directives, not just that
    // the build succeeded.
    [Theory]
    [InlineData("Store", "off", null, null)]
    [InlineData("Fastest", "on", "MSZIP", null)]
    [InlineData("Fast", "on", "MSZIP", null)]
    [InlineData("Normal", "on", "LZX", "15")]
    [InlineData("Maximum", "on", "LZX", "18")]
    [InlineData("Ultra", "on", "LZX", "21")]
    public void CabDdfGetsTheRightCompressionDirectives(string level, string expectedCompress, string? expectedType, string? expectedMemory)
    {
        using var workspace = new FixtureWorkspace();

        var build = BuildHarness.Build(workspace, "StubArchive/StubArchive.zparchproj", pack.Version,
            new Dictionary<string, string>
            {
                ["ZipadeeOutputFormat"] = "Cab",
                ["ZipadeeCompressionLevel"] = level,
            });

        Assert.True(build.Succeeded, build.Output);

        var ddf = File.ReadAllText(Path.Combine(build.IntermediateDirectory, "StubArchive.ddf"));

        Assert.Contains($".Set Compress={expectedCompress}", ddf);
        if (expectedType is not null)
        {
            Assert.Contains($".Set CompressionType={expectedType}", ddf);
        }
        if (expectedMemory is not null)
        {
            Assert.Contains($".Set CompressionMemory={expectedMemory}", ddf);
        }
    }
}
