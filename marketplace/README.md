# Marketplace listing

Files for publishing this extension to the Visual Studio Marketplace (see [issue #16](https://github.com/Jonesie/zipadee/issues/16)):

- `publishManifest.json` — passed to `VsixPublisher.exe` as `-publishManifest`. Already references the three screenshots in `images/` via `assetFiles`.
- `overview.md` — the actual listing page shown on marketplace.visualstudio.com, referenced by `publishManifest.json`'s `overview` field. Includes a full MSBuild property reference for the project file.
- `images/` — screenshots referenced from `overview.md` (Solution Explorer, the New Project dialog, a completed build).

## Before publishing

1. Fill in the still-missing marketplace-facing fields in `src/Zipadee.Vsix/source.extension.vsixmanifest` (icon, license, tags, preview image, more-info link) — see issue #16 for the full checklist.
2. Build the VSIX in Release configuration, then run:
   ```
   VsixPublisher.exe publish -payload "path\to\Zipadee.Vsix.vsix" -publishManifest "marketplace\publishManifest.json" -personalAccessToken "{PAT}"
   ```
