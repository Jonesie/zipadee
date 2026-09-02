# Zipadee

Zipadee adds a **Zipadee Archive Project** type to Visual Studio: a project whose "build output" is an archive file (zip, 7z, tar, or a self-extracting .exe) assembled from the files, linked files, and other projects' build outputs it contains.

Think of it as a lightweight, in-solution alternative to a separate packaging script - add a Zipadee project alongside the rest of your solution, add the files and project references you want bundled, and every build produces an up-to-date archive.

> **Status:** not yet published to the Visual Studio Marketplace (tracked in [#16](https://github.com/Jonesie/zipadee/issues/16)). Until then, build the VSIX from source (see below) to try it.

## Features

- **Files, links, and project outputs** — add existing files directly, link to files elsewhere on disk, or reference another project in the solution and pull in its build output (with correct build ordering, for free, via standard MSBuild project references).
- **Multiple archive formats** — Zip, 7-Zip (.7z), Tar, gzip-compressed tar (.tar.gz), and RAR (.rar) using your own installed copy of WinRAR - RAR's format is proprietary, so unlike the others this one isn't bundled and needs WinRAR installed separately.
- **Configurable compression** — from Store (no compression) through Ultra.
- **Password protection** — AES-256 encryption for the Zip, 7-Zip, and RAR formats, with header encryption (hides filenames too) applied automatically. The password lives in your local `.user` file, never in the shared project file.
- **Self-extracting archives** — produce a self-extracting `.exe` (7-Zip format) that needs no archive tool to open.
- **Incremental builds** — the archive is only rebuilt when its contents actually change, not on every build. Can be turned off per-project to force a fresh archive every time.
- **Configurable from the IDE** — the settings show up in the Properties window (F4) and on the project's General property page, or edit the project XML directly if you prefer.
- Works with `dotnet build` / `dotnet restore` on the command line and in CI, not just inside Visual Studio.

## What it looks like

An archive project sits in the solution alongside the projects it packages, with the files, links and project references it contains shown as its contents:

![Solution Explorer showing a Zipadee Archive Project alongside a referenced console app project](marketplace/images/solution-explorer.png)

Building it produces the archive as that project's output:

![A completed build producing a self-extracting archive](marketplace/images/build-output.png)

See [`marketplace/overview.md`](marketplace/overview.md) for the full MSBuild project file reference (every archive setting, with examples) and more screenshots.

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

`sample/ZipadeeSample` is a small solution (a console app plus an archive project referencing it) used to manually exercise the extension's build-time behavior - see [`marketplace/overview.md`](marketplace/overview.md) for what each of its project's settings demonstrate.

## Branching

This repo uses [GitFlow](https://github.com/gittower/git-flow-next):

- `main` — released code only
- `develop` — integration branch, base for new work
- `feature/*` — one branch per feature/milestone, branched from and merged back into `develop`
- `release/*` — release stabilization, branched from `develop`, merged into `main` and `develop`
- `hotfix/*` — urgent fixes, branched from `main`, merged into `main` and `develop`

## License

Zipadee is licensed under [GPL-3.0](LICENSE). It bundles 7-Zip (LGPL / BSD 3-clause / BSD 2-clause) to perform the actual archiving.
