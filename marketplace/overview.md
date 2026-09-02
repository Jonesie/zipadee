<!--
  This is the Visual Studio Marketplace listing page for Zipadee, referenced by
  publishManifest.json's "overview" field. See VsixPublisher's "assetFiles" in
  publishManifest.json for how the images/ referenced below get uploaded
  alongside this file.
-->

# Zipadee

Zipadee adds a **Zipadee Archive Project** type to Visual Studio: a project whose "build output" is an archive file (zip, 7z, tar, or a self-extracting .exe) assembled from the files, linked files, and other projects' build outputs you add to it.

Think of it as a lightweight, in-solution alternative to a separate packaging script - add a Zipadee project alongside the rest of your solution, add the files and project references you want bundled, and every build produces an up-to-date archive.

## Features

- **Files, links, and project outputs** — add existing files directly, link to files elsewhere on disk, or reference another project in the solution and pull in its build output (with correct build ordering, for free, via standard MSBuild project references).
- **Multiple archive formats** — Zip, 7-Zip (.7z), Tar, and gzip-compressed tar (.tar.gz).
- **Configurable compression** — from Store (no compression) through Ultra.
- **Password protection** — AES-256 encryption for the Zip and 7-Zip formats, with 7-Zip's header encryption (hides filenames too) applied automatically. The password lives in your local `.user` file, never in the shared project file.
- **Self-extracting archives** — produce a self-extracting `.exe` (7-Zip format) that needs no archive tool to open.
- **Incremental builds** — the archive is only rebuilt when its contents actually change, not on every build.
- Works with `dotnet build` / `dotnet restore` on the command line and in CI, not just inside Visual Studio.

## Getting started

1. Install this extension.
2. In Visual Studio, choose **File > New > Project** and search for **Zipadee Archive Project**.
3. Add files via **Add > Existing Item** (or **Add as Link** for files outside the project folder), or add a **Project Reference** to another project in your solution to pull in its build output.
4. Build. The archive appears alongside the project's other build output.

Archive settings (format, compression level, password, self-extracting) are set as MSBuild properties in the project file - see [Project file reference](#project-file-reference) below for the full list.

![Solution Explorer showing a Zipadee Archive Project alongside a referenced console app project](images/solution-explorer.png "Solution Explorer") 

![The Project Properties](images/project-properties.png "Project Properties")

![The New Project dialog with Zipadee Archive Project selected](images/new-project-dialog.png "New Project dialog")


![A completed build producing a self-extracting archive](images/build-output.png "Build output")

## Project file reference

All archive settings are plain MSBuild properties. You can set them either from Visual Studio - select the project node and use the **Properties** window (F4), or the **General** page of the project's **Properties** - or by editing the `.zparchproj` file directly. The password is the one exception to where it gets saved: it goes to a `.zparchproj.user` file rather than the project file (see below), whichever way you set it.

| Property | Values | Default | Notes |
|---|---|---|---|
| `ZipadeeOutputFormat` | `Zip`, `SevenZip`, `Tar`, `GZip` | `Zip` | `GZip` produces a `.tar.gz` (tar, then gzip-compressed) since gzip alone can't hold more than one file. |
| `ZipadeeCompressionLevel` | `Store`, `Fastest`, `Fast`, `Normal`, `Maximum`, `Ultra` | `Normal` | Maps to 7-Zip's `-mx=` levels. Ignored for `Tar` (tar itself doesn't compress). |
| `ZipadeeCreateSfx` | `true`, `false` | `false` | Produces a self-extracting `.exe` instead of a plain archive. **Only valid with `ZipadeeOutputFormat=SevenZip`** - 7-Zip's SFX modules can't self-extract any other format, and the build fails with a clear error if combined with one. |
| `ZipadeePassword` | any string | *(none)* | AES-256 password protection. **Only valid with `Zip` or `SevenZip`** - Tar and GZip have no encryption support in 7-Zip, and the build fails if combined with one. For 7-Zip, filenames are also encrypted (`-mhe=on`) automatically whenever a password is set. |

### Where to put the items being archived

Add files as regular `Content` or `None` items (Solution Explorer's **Add > Existing Item** uses `Content` for this project type):

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
    <PackageReference Include="Zipadee.Build" Version="0.1.0" />
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

Zipadee is licensed under [GPL-3.0](https://github.com/Jonesie/zipadee/blob/main/LICENSE). It bundles 7-Zip (LGPL / BSD 3-clause / BSD 2-clause) to perform the actual archiving.

<a href="https://www.buymeacoffee.com/jonesie" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me a Coffee" style="height: 60px !important;width: 217px !important;" ></a>

## Feedback

Found a bug or have a feature request? [Open an issue](https://github.com/Jonesie/zipadee/issues) on GitHub.
