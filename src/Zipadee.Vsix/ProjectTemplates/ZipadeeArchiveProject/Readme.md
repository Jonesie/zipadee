# Zipadee Archive Project

This project's "build output" is an archive file (zip, 7z, tar, cab, or a self-extracting .exe) assembled from the files, linked files, and other projects' build outputs you add to it. Every build produces an up-to-date archive - delete this file once you're comfortable with how it works, it isn't part of the archive itself (it's a `None` item, not `Content` - see below).

**Note:**  This README will _not_ be included in your archive.  If you do want a README included, you can clear the body of this file and set the Build Action to Content. 

## Adding things to archive

Add files via **Add > Existing Item** (or **Add as Link** for files elsewhere on disk) - only `Content` items are archived. `None` items (like this readme) are deliberately left out, so the project can hold files it doesn't want shipped without them silently ending up in the output:

```xml
<ItemGroup>
  <Content Include="readme.txt" />
</ItemGroup>
```

A linked file (**Add > Existing Item > Add as Link**) is archived at its `%(Link)` path, not its on-disk path:

```xml
<ItemGroup>
  <Content Include="..\shared\LICENSE.txt" Link="docs\LICENSE.txt" />
</ItemGroup>
```

Add a **Project Reference** to pull in another project's build output - it's built first, and its output (including the compiled assembly, for a runnable apphost-style `.exe`) is archived at the archive's root:

```xml
<ItemGroup>
  <ProjectReference Include="..\ConsoleApp\ConsoleApp.csproj" />
</ItemGroup>
```

`ZipadeeProjectOutputExclude`/`ZipadeeProjectOutputInclude` (see the table below) can filter what a project reference brings in, by wildcard pattern or exact file name.

## Archive settings

Set these either from the **Properties** window (F4) or the project's General **Properties** page in Visual Studio, or by editing this project file directly. All are plain MSBuild properties.

| Property | Values | Default | Notes |
|---|---|---|---|
| `ZipadeeOutputFormat` | `Zip`, `SevenZip`, `Tar`, `GZip`, `Cab`, `Rar` | `Zip` | `Zip`/`SevenZip`/`Tar`/`GZip` need 7-Zip's command-line tool (`7z`) on `PATH`; `Rar` needs WinRAR's `rar` on `PATH` too. None of them are bundled. `GZip` produces a `.tar.gz`. `Cab` uses `makecab.exe`, built into every Windows install - nothing extra needed. |
| `ZipadeeCompressionLevel` | `Store`, `Fastest`, `Fast`, `Normal`, `Maximum`, `Ultra` | `Normal` | One generic scale across every format - see [Compression level mapping](#compression-level-mapping) below. Ignored for `Tar`. |
| `ZipadeeCreateSfx` | `true`, `false` | `false` | Produces a self-extracting `.exe` instead of a plain archive. **Only valid with `ZipadeeOutputFormat=SevenZip`.** Can't be combined with `ZipadeeMaxVolumeSize`. |
| `ZipadeePassword` | any string | *(none)* | AES-256 password protection. **Only valid with `Zip`, `SevenZip`, or `Rar`.** Filenames are encrypted too. Set it from the **Properties** window so it's written to a local `.zparchproj.user` file, not this project file - never commit a password. |
| `ZipadeeIncrementalBuild` | `true`, `false` | `true` | Skip re-archiving when nothing changed. Set to `false` to force a fresh archive every build. |
| `ZipadeeMaxVolumeSize` | a positive integer (bytes) | *(none, single file)* | Splits the archive into multiple numbered volumes of at most this many bytes each. Valid for `Zip`, `SevenZip`, `Rar`, and `Cab` (must be a multiple of 512 for `Cab`); ignored for `Tar`/`GZip`. |
| `ZipadeeProjectOutputExclude` | `;`-separated wildcard patterns (`*.pdb`) or exact file names | *(none)* | Leaves matching files out of a referenced project's output. Doesn't affect this project's own `Content`/`None` items. |
| `ZipadeeProjectOutputInclude` | `;`-separated wildcard patterns or exact file names | *(none)* | Overrides `ZipadeeProjectOutputExclude`, forcing matching files back in. Has no effect without `ZipadeeProjectOutputExclude` also set. |
| `ZipadeeArchiveFileName` | text with `{ProjectName}`/`{Version}`/`{Date}`/`{Time}` tokens | `{ProjectName}` | The archive's output file name (without extension) - e.g. `{ProjectName}-{Date}` produces `MyArchive-20260901.zip`. `{Date}`/`{Time}` use `ZipadeeArchiveDateFormat`/`ZipadeeArchiveTimeFormat` (default `yyyyMMdd`/`HHmmss`). Ignored entirely if `ZipadeeArchiveOutputPath` is also set. |

### Compression level mapping

`ZipadeeCompressionLevel` is one generic 6-level scale shared across every format, since none of the underlying tools agree with each other:

| Level | Zip / SevenZip / GZip (7-Zip `-mx=`) | Rar (`-m`) | Cab |
|---|---|---|---|
| `Store` | 0 (no compression) | 0 (no compression) | `Compress=off` |
| `Fastest` | 1 | 1 | MSZIP |
| `Fast` | 3 | 2 | MSZIP |
| `Normal` | 5 | 3 | LZX, `CompressionMemory=15` |
| `Maximum` | 7 | 4 | LZX, `CompressionMemory=18` |
| `Ultra` | 9 | 5 | LZX, `CompressionMemory=21` |

### Cab files and DDF support

`Cab` has two hard limitations: no encryption support at all, and no self-extracting option. With no further settings Zipadee generates the whole DDF itself. For anything `makecab.exe` supports beyond Zipadee's own settings (for example a fixed `MaxDiskSize`), add a DDF file next to this project file with the same base name (e.g. `MyArchive.zparchproj` looks for `MyArchive.ddf`), containing only `.Set` directives - no file list. It's always excluded from the cab's own contents, even if also added as a `Content` item.

### Multi-volume archives

`ZipadeeMaxVolumeSize` splits the archive into multiple numbered files once the content exceeds that size - each format names its volumes differently (native tool behavior):

| Format | Volume naming |
|---|---|
| `Zip` / `SevenZip` | `MyArchive.zip.001`, `MyArchive.zip.002`, ... |
| `Rar` | `MyArchive.part1.rar`, `MyArchive.part2.rar`, ... |
| `Cab` | `MyArchive1.cab`, `MyArchive2.cab`, ... |

Forces a fresh archive on every build regardless of `ZipadeeIncrementalBuild` (the number of volumes produced can change between builds, which incremental build's staleness check can't track precisely) - stale volumes from a previous build are always cleaned up first.

## More

Full documentation, screenshots, and sample projects for every format live in the [Zipadee GitHub repo](https://github.com/Jonesie/zipadee). Found a bug or have a feature request? [Open an issue](https://github.com/Jonesie/zipadee/issues) there.

Zipadee is licensed under [GPL-3.0](https://github.com/Jonesie/zipadee/blob/main/LICENSE). It shells out to 7-Zip (LGPL / BSD 3-clause / BSD 2-clause) and/or WinRAR to perform the actual archiving - neither is bundled.

If you find Zipadee useful, please consider [sponsoring the project](https://www.buymeacoffee.com/jonesie).