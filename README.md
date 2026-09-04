<img src="marketplace/images/zipadee-icon.png" alt="Zipadee" width="72" height="72" align="left" />

# Zipadee

Zipadee adds a **Zipadee Archive Project** type to Visual Studio: a project whose "build output" is an archive file (zip, 7z, tar, cab, or a self-extracting .exe) assembled from the files, linked files, and other projects' build outputs it contains.

Think of it as a lightweight, in-solution alternative to a separate packaging script - add a Zipadee project alongside the rest of your solution, add the files and project references you want bundled, and every build produces an up-to-date archive.

> **[Get it on the Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=Jonesie.Zipadee)**, grab a pre-built `.vsix` from [GitHub Releases](https://github.com/Jonesie/zipadee/releases), or build it from source (see below).

> **External tools required, per format** - none of them are bundled, and both are expected on `PATH` (neither installer adds itself there automatically - a manual step after installing):
> - `Zip`, `SevenZip`, `Tar`, `GZip` — [7-Zip](https://www.7-zip.org/)'s command-line tool (`7z`).
> - `Rar` — [WinRAR](https://www.rarlab.com/download.htm)'s command-line tool (`rar`).
> - `Cab` — nothing extra; uses `makecab.exe`, built into every Windows install.

## Features

- **Files, links, and project outputs** — add existing files directly, link to files elsewhere on disk, or reference another project in the solution and pull in its build output (with correct build ordering, for free, via standard MSBuild project references).
- **Multiple archive formats** — Zip, 7-Zip (.7z), Tar, gzip-compressed tar (.tar.gz), Windows Cabinet (.cab), and RAR (.rar). None of the underlying tools are bundled - see the requirement above.
- **Configurable compression** — from Store (no compression) through Ultra, one generic scale mapped to each format's own native settings (see [Compression level mapping](https://jonesie.github.io/zipadee/docs/reference#compression-level-mapping)).
- **Password protection** — AES-256 encryption for the Zip, 7-Zip, and RAR formats, with header encryption (hides filenames too) applied automatically. The password lives in your local `.user` file, never in the shared project file.
- **Self-extracting archives** — produce a self-extracting `.exe` (7-Zip format) that needs no archive tool to open.
- **Incremental builds** — the archive is only rebuilt when its contents actually change, not on every build. Can be turned off per-project to force a fresh archive every time.
- **Multi-volume archives** — split the output into multiple numbered files of a set maximum size, for the Zip, 7-Zip, RAR, and Cab formats (see [Multi-volume archives](https://jonesie.github.io/zipadee/docs/reference#multi-volume-archives)).
- **Filter project reference output** — exclude files (by wildcard pattern or exact name) from a referenced project's output, with an override to force specific files back in (see [Filtering project reference output](https://jonesie.github.io/zipadee/docs/reference#filtering-project-reference-output)).
- **Customizable archive file name** — compose the output file name from `{ProjectName}`/`{Version}`/`{Date}`/`{Time}` tokens (see [Customizing the archive file name](https://jonesie.github.io/zipadee/docs/reference#customizing-the-archive-file-name)).
- **Configurable from the IDE** — the settings show up in the Properties window (F4) and on the project's General property page, or edit the project XML directly if you prefer.
- Works with `dotnet build` / `dotnet restore` on the command line and in CI, not just inside Visual Studio.

## What it looks like

An archive project sits in the solution alongside the projects it packages, with the files, links and project references it contains shown as its contents:

![Solution Explorer showing a Zipadee Archive Project alongside a referenced console app project](marketplace/images/solution-explorer.png)

Building it produces the archive as that project's output:

![A completed build producing a self-extracting archive](marketplace/images/build-output.png)

See the [full project file reference](https://jonesie.github.io/zipadee/docs/reference) for every archive setting with examples, or [`marketplace/overview.md`](marketplace/overview.md) for more screenshots.

See the [issue tracker](https://github.com/Jonesie/zipadee/issues) for the current milestone plan (M0–M5) and known gaps.

## Building from source

Requirements:

- Visual Studio 2022 or 2026, with the **Visual Studio extension development** workload installed
- .NET 10 SDK

```
git clone https://github.com/Jonesie/zipadee.git
cd zipadee
```

Open `Zipadee.slnx` in Visual Studio and press **F5** to build and launch an experimental instance of Visual Studio with the extension loaded, or build from the command line:

```
dotnet restore Zipadee.slnx
dotnet build Zipadee.slnx --configuration Release
```

The built `.vsix` is produced at `src/Zipadee.Vsix/bin/Release/net472/Zipadee.Vsix.vsix`; double-click it to install into your own Visual Studio.

`sample/ZipadeeSample` is a small solution used to manually exercise the extension's build-time behavior - a console app (`ConsoleApp`), plus one archive project per format, each referencing it:

| Project | Format | Demonstrates |
|---|---|---|
| `ConsoleZip` | `SevenZip` | Self-extracting `.exe`, `Maximum` compression |
| `ConsoleRar` | `Rar` | Needs WinRAR's `rar` on `PATH` (see the requirement above) |
| `ConsoleCab` | `Cab` | `ConsoleCab.ddf`, a settings-only DDF picked up by naming convention (see [Cab files and DDF support](https://jonesie.github.io/zipadee/docs/reference#cab-files-and-ddf-support)) |
| `ConsoleTar` | `Tar` | Plain, uncompressed tar |
| `ConsoleGZip` | `GZip` | Gzip-compressed tar |

Building the whole solution needs both 7-Zip and WinRAR on `PATH`. If one's missing, only that project's archive step fails - the others still build fine.

## Branching

This repo uses [GitFlow](https://github.com/gittower/git-flow-next):

- `main` — released code only
- `develop` — integration branch, base for new work
- `feature/*` — one branch per feature/milestone, branched from and merged back into `develop`
- `release/*` — release stabilization, branched from `develop`, merged into `main` and `develop`
- `hotfix/*` — urgent fixes, branched from `main`, merged into `main` and `develop`

## License

Zipadee is licensed under [GPL-3.0](LICENSE). It shells out to 7-Zip (LGPL / BSD 3-clause / BSD 2-clause) to perform the actual archiving - not bundled, see the requirement above.

<a href="https://www.buymeacoffee.com/jonesie" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png" alt="Buy Me a Coffee" style="height: 60px !important;width: 217px !important;" ></a>