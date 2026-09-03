namespace Zipadee.Build.Tests.Infrastructure;

/// <summary>
/// Inspects a produced archive via 7z.exe - confirmed empirically (this project's own manual
/// testing) that 7z can read every format Zipadee produces, including .rar and .cab, even though
/// Zipadee itself shells out to different native tools to create some of them (WinRAR for .rar).
/// One inspection tool for every format keeps verification simple and avoids a WinRAR dependency
/// for tests that only ever read a .rar, never write one.
/// </summary>
internal static class ArchiveInspector
{
    private const string SevenZipExe = "7z";

    public static ProcessResult Test(string archivePath, string? password = null)
        => ProcessRunner.Run(SevenZipExe, $"t \"{archivePath}\"{PasswordArg(password)}", Path.GetDirectoryName(archivePath)!);

    /// <summary>
    /// Lists an archive's contents. GZip is a container around a single inner file (the tar), so
    /// `7z l` on a .tar.gz directly shows just that one inner .tar entry, not the files inside it
    /// - confirmed empirically. For .tar.gz specifically, this unwraps the gzip layer to a temp
    /// file first and lists that instead, so callers see the same file-level listing they'd get
    /// for every other format.
    /// </summary>
    public static ProcessResult List(string archivePath, string? password = null)
    {
        if (!archivePath.EndsWith(".tar.gz", StringComparison.OrdinalIgnoreCase))
        {
            return ProcessRunner.Run(SevenZipExe, $"l \"{archivePath}\"{PasswordArg(password)}", Path.GetDirectoryName(archivePath)!);
        }

        var unwrapDir = Path.Combine(Path.GetTempPath(), "zipadee-build-tests-gzip", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(unwrapDir);
            var unwrap = ProcessRunner.Run(SevenZipExe, $"x \"{archivePath}\" -o\"{unwrapDir}\" -y", unwrapDir);
            if (unwrap.ExitCode != 0)
            {
                return new ProcessResult(unwrap.ExitCode, unwrap.StdOut, unwrap.StdErr);
            }

            var innerTar = Directory.GetFiles(unwrapDir, "*.tar").Single();
            return ProcessRunner.Run(SevenZipExe, $"l \"{innerTar}\"{PasswordArg(password)}", unwrapDir);
        }
        finally
        {
            try { Directory.Delete(unwrapDir, recursive: true); } catch { /* best-effort */ }
        }
    }

    public static ProcessResult Extract(string archivePath, string destinationDir, string? password = null)
    {
        Directory.CreateDirectory(destinationDir);
        return ProcessRunner.Run(SevenZipExe, $"x \"{archivePath}\" -o\"{destinationDir}\" -y{PasswordArg(password)}", Path.GetDirectoryName(archivePath)!);
    }

    /// <summary>
    /// Runs a self-extracting archive directly (not via 7z) - the only way to prove it's a
    /// genuinely self-extracting .exe rather than, say, a bare SFX stub with no payload
    /// (confirmed empirically as a real failure mode when ZipadeeCreateSfx was combined with
    /// ZipadeeMaxVolumeSize during this project's development - see the error that now guards
    /// against it). 7-Zip's console SFX module (7zCon.sfx) accepts the same -o/-y switches 7z.exe
    /// itself does.
    /// </summary>
    public static ProcessResult ExtractSfx(string sfxExePath, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);
        return ProcessRunner.Run(sfxExePath, $"-y -o\"{destinationDir}\"", Path.GetDirectoryName(sfxExePath)!);
    }

    private static string PasswordArg(string? password) => password is null ? string.Empty : $" -p{password}";
}
