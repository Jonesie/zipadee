<!--
  This is the Visual Studio Marketplace listing page for Zipadee, referenced by
  publishManifest.json's "overview" field. See VsixPublisher's "assetFiles" in
  publishManifest.json for how the images/ referenced below get uploaded
  alongside this file.
-->

<img src="images/zipadee-icon.png" alt="Zipadee" width="72" height="72" align="left" />

# Zipadee

Zipadee adds a **Zipadee Archive Project** type to Visual Studio: a project whose "build output" is an archive file (zip, 7z, tar, cab, or a self-extracting .exe) assembled from the files, linked files, and other projects' build outputs you add to it.

Think of it as a lightweight, in-solution alternative to a separate packaging script - add a Zipadee project alongside the rest of your solution, add the files and project references you want bundled, and every build produces an up-to-date archive.

## Features

- **Files, links, and project outputs** — add existing files directly, link to files elsewhere on disk, or reference another project in the solution and pull in its build output (with correct build ordering, for free, via standard MSBuild project references).
- **Multiple archive formats** — Zip, 7-Zip (.7z), Tar, gzip-compressed tar (.tar.gz), Windows Cabinet (.cab), and RAR (.rar). None of the underlying tools are bundled with Zipadee itself - see Getting started below for what each format needs installed.
- **Configurable compression** — from Store (no compression) through Ultra.
- **Password protection** — AES-256 encryption for the Zip, 7-Zip, and RAR formats, with header encryption (hides filenames too) applied automatically. The password lives in your local `.user` file, never in the shared project file.
- **Self-extracting archives** — produce a self-extracting `.exe` (7-Zip format) that needs no archive tool to open.
- **Incremental builds** — the archive is only rebuilt when its contents actually change, not on every build. Can be turned off per-project to force a fresh archive every time.
- Works with `dotnet build` / `dotnet restore` on the command line and in CI, not just inside Visual Studio.

## Getting started

1. Install whatever external tool your chosen format needs - none of them are bundled, and both are expected on `PATH` (neither installer adds itself there automatically - a manual step after installing):
   - `Zip`, `SevenZip`, `Tar`, `GZip` — [7-Zip](https://www.7-zip.org/)'s command-line tool (`7z`).
   - `Rar` — [WinRAR](https://www.rarlab.com/download.htm)'s command-line tool (`rar`).
   - `Cab` — nothing extra; uses `makecab.exe`, built into every Windows install.
2. Install this extension.
3. In Visual Studio, choose **File > New > Project** and search for **Zipadee Archive Project**.
4. Add files via **Add > Existing Item** (or **Add as Link** for files outside the project folder), or add a **Project Reference** to another project in your solution to pull in its build output.
5. Build. The archive appears alongside the project's other build output.

Archive settings (format, compression level, password, self-extracting) are set as MSBuild properties in the project file - see [Project file reference](#project-file-reference) below for the full list.

![Solution Explorer showing a Zipadee Archive Project alongside a referenced console app project](images/solution-explorer.png "Solution Explorer") 

![The Project Properties](images/project-properties.png "Project Properties")

![The New Project dialog with Zipadee Archive Project selected](images/new-project-dialog.png "New Project dialog")


![A completed build producing a self-extracting archive](images/build-output.png "Build output")

## Project file reference

All archive settings are plain MSBuild properties. You can set them either from Visual Studio - select the project node and use the **Properties** window (F4), or the **General** page of the project's **Properties** - or by editing the `.zparchproj` file directly. The password is the one exception to where it gets saved: it goes to a `.zparchproj.user` file rather than the project file (see below), whichever way you set it.

| Property | Values | Default | Notes |
|---|---|---|---|
| `ZipadeeOutputFormat` | `Zip`, `SevenZip`, `Tar`, `GZip`, `Cab`, `Rar` | `Zip` | `Zip`/`SevenZip`/`Tar`/`GZip` need 7-Zip's command-line tool on `PATH`; `Rar` needs WinRAR's `rar` on `PATH` too - see Getting started above. None of them are bundled. `GZip` produces a `.tar.gz` (tar, then gzip-compressed) since gzip alone can't hold more than one file. `Cab` produces a Windows Cabinet using the `makecab.exe` built into every Windows install - nothing extra needed - see [Cab files and DDF support](#cab-files-and-ddf-support) below. |
| `ZipadeeCompressionLevel` | `Store`, `Fastest`, `Fast`, `Normal`, `Maximum`, `Ultra` | `Normal` | One generic scale across every format - each maps it to its own native settings differently. See [Compression level mapping](#compression-level-mapping) below. Ignored for `Tar` (tar itself doesn't compress). |
| `ZipadeeCreateSfx` | `true`, `false` | `false` | Produces a self-extracting `.exe` instead of a plain archive (console-style extraction, since 7-Zip's default SFX module is used). **Only valid with `ZipadeeOutputFormat=SevenZip`** - 7-Zip's SFX modules can't self-extract any other format, and the build fails with a clear error if combined with one. |
| `ZipadeePassword` | any string | *(none)* | AES-256 password protection. **Only valid with `Zip`, `SevenZip`, or `Rar`** - Tar, GZip, and Cab have no encryption support, and the build fails if combined with one. Filenames are also encrypted automatically whenever a password is set (7-Zip's `-mhe=on`, or RAR's `-hp`, which does this as part of the same switch that sets the password). |
| `ZipadeeIncrementalBuild` | `true`, `false` | `true` | Skip re-archiving when nothing changed. Set to `false` to force a fresh archive on every build. |

### Where to put the items being archived

Add files as `Content` items (Solution Explorer's **Add > Existing Item** uses `Content` for this project type) - only `Content` items are archived. `None` items are deliberately left out, so the project file can hold a file it doesn't want in the archive itself - a settings-only DDF for the `Cab` format (see below) being the main example - without it silently ending up in the output:

```xml
<ItemGroup>
  <Content Include="readme.txt" />
</ItemGroup>
```

Link to a file that lives outside the project folder (Solution Explorer's **Add > Existing Item > Add as Link**) with `%Link%` metadata - it's archived at the link path, not its on-disk path:

```xml
<ItemGroup>
  <Content Include="..\shared\LICENSE.txt" Link="docs\LICENSE.txt" />
</ItemGroup>
```

Link in a *whole folder* from outside the project by combining a wildcard `Include` with `%(RecursiveDir)` in `Link` - every file under it, including subfolders, is archived preserving that folder's structure. There's no dedicated "add folder as link" command in Solution Explorer for this - add the `ItemGroup` by editing the `.zparchproj` file directly:

```xml
<ItemGroup>
  <Content Include="..\shared-assets\**\*.*">
    <Link>assets\%(RecursiveDir)%(Filename)%(Extension)</Link>
  </Content>
</ItemGroup>
```

Pull in another project's build output with a normal `ProjectReference` - the referenced project is built first, and its output (including the compiled assembly, for a runnable apphost-style `.exe`) is archived at the root of the archive:

```xml
<ItemGroup>
  <ProjectReference Include="..\ConsoleApp\ConsoleApp.csproj" />
</ItemGroup>
```

### Setting the password

Since a password must never end up committed to source control, it isn't stored in the `.zparchproj` file itself. Setting it in the **Properties** window handles this for you - Visual Studio writes it to a `<ProjectFileName>.zparchproj.user` file alongside the project, which the standard Visual Studio `.gitignore` template already excludes.

To set it by hand instead, create that `.user` file yourself:

```xml
<Project>
  <PropertyGroup>
    <ZipadeePassword>YourPasswordHere</ZipadeePassword>
  </PropertyGroup>
</Project>
```

### Compression level mapping

`ZipadeeCompressionLevel` is one generic 6-level scale shared across every format - it exists because none of the underlying tools agree with each other: 7-Zip has a sparse 0/1/3/5/7/9 scale, RAR a clean 0-5 range, and Cab only has two compression methods at all (MSZIP and LZX), with LZX taking a separate memory/dictionary-size setting rather than a level. `ZipadeeCompressionLevel` maps onto whichever of those the selected `ZipadeeOutputFormat` actually uses:

| Level | Zip / SevenZip / GZip (7-Zip `-mx=`) | Rar (`-m`) | Cab |
|---|---|---|---|
| `Store` | 0 (no compression) | 0 (no compression) | `Compress=off` (no compression) |
| `Fastest` | 1 | 1 | MSZIP |
| `Fast` | 3 | 2 | MSZIP |
| `Normal` | 5 | 3 | LZX, `CompressionMemory=15` |
| `Maximum` | 7 | 4 | LZX, `CompressionMemory=18` |
| `Ultra` | 9 | 5 | LZX, `CompressionMemory=21` |

Ignored entirely for `Tar` - tar itself doesn't compress, and `GZip`'s tar step ahead of the actual gzip compression doesn't use it either (only the gzip step does, at the 7-Zip `-mx=` values above).

### Cab files and DDF support

`ZipadeeOutputFormat=Cab` produces a Windows Cabinet using `makecab.exe`, which ships with every Windows install - nothing extra to install, unlike `Rar`. Cab has two hard format limitations: no encryption support at all (a password fails the build, same as `Tar`/`GZip`), and no self-extracting option.

With no further settings, Zipadee generates the whole DDF itself - the file list comes from the project's `Content` items, the same as every other format. For anything makecab supports beyond what Zipadee's own settings expose (for example `MaxDiskSize` for a multi-disk cabinet), add a DDF file **next to the project file, with the same base name** (a project called `MyArchive.zparchproj` looks for `MyArchive.ddf`) containing only `.Set` directives - **no file list**. If found, the build appends its own computed settings first (so yours can override them), then its own mandatory settings last, then the file list itself, generated automatically from the project's `Content` items - you don't list files in this DDF, only settings:

```
; MyArchive.ddf
.Set MaxDiskSize=0
```

This file is always excluded from the cab's own contents, even if it's also added to the project as a `Content` item (e.g. so it shows up in Solution Explorer) - unlike every other format, where a file with this same name is just an ordinary file and follows the normal Content/None rule above.

See `sample/ZipadeeSample/ConsoleCab` in the repo for a real, working example, including a DDF listing every documented `makecab.exe` directive.

### Full example

A project that produces a password-protected, self-extracting 7-Zip archive at maximum compression, containing a content file, a linked file, and another project's build output:

```xml
<Project Sdk="Microsoft.Build.NoTargets/3.7.0">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ZipadeeOutputFormat>SevenZip</ZipadeeOutputFormat>
    <ZipadeeCompressionLevel>Maximum</ZipadeeCompressionLevel>
    <ZipadeeCreateSfx>true</ZipadeeCreateSfx>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Zipadee.Build" Version="0.1.3" />
  </ItemGroup>
  <ItemGroup>
    <Content Include="Hello.txt" />
    <Content Include="..\shared\LICENSE.txt" Link="docs\LICENSE.txt" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\ConsoleApp\ConsoleApp.csproj" />
  </ItemGroup>
</Project>
```

with a `ConsoleZip.zparchproj.user` file alongside it setting `ZipadeePassword`, as shown above.

## License

Zipadee is licensed under [GPL-3.0](https://github.com/Jonesie/zipadee/blob/main/LICENSE). It shells out to 7-Zip (LGPL / BSD 3-clause / BSD 2-clause) to perform the actual archiving - not bundled, see Getting started above.

<a href="https://www.buymeacoffee.com/jonesie" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me a Coffee" style="height: 60px !important;width: 217px !important;" ></a>

## Feedback

Found a bug or have a feature request? [Open an issue](https://github.com/Jonesie/zipadee/issues) on GitHub.
