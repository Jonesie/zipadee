<!--
  This is the Visual Studio Marketplace listing page for Zipadee, referenced by
  publishManifest.json's "overview" field. Screenshots below are placeholders -
  replace images/*.png with real captures before publishing (see issue #16), and
  remove this comment once that's done. See VsixPublisher's "assetFiles" in
  publishManifest.json for how these images get uploaded alongside this file.
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

Archive settings (format, compression level, password, self-extracting) are set as MSBuild properties in the project file - see the [README](https://github.com/Jonesie/zipadee) for the full list.

<!-- ![Solution Explorer showing a Zipadee Archive Project alongside a referenced console app project](images/solution-explorer.png "Solution Explorer") -->

<!-- ![The New Project dialog with Zipadee Archive Project selected](images/new-project-dialog.png "New Project dialog") -->

<!-- ![A completed build producing a self-extracting archive](images/build-output.png "Build output") -->

## License

Zipadee is licensed under [GPL-3.0](https://github.com/Jonesie/zipadee/blob/main/LICENSE). It bundles 7-Zip (LGPL / BSD 3-clause / BSD 2-clause) to perform the actual archiving.

## Feedback

Found a bug or have a feature request? [Open an issue](https://github.com/Jonesie/zipadee/issues) on GitHub.
