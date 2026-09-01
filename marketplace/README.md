# Marketplace listing

Files for publishing this extension to the Visual Studio Marketplace (see [issue #16](https://github.com/Jonesie/zipadee/issues/16)):

- `publishManifest.json` — passed to `VsixPublisher.exe` as `-publishManifest`.
- `overview.md` — the actual listing page shown on marketplace.visualstudio.com, referenced by `publishManifest.json`'s `overview` field.
- `images/` — screenshots referenced from `overview.md`. Empty for now; still needs real captures from Visual Studio (Solution Explorer, the New Project dialog, a build in progress) since that can't be automated.

## Before publishing

1. Drop real screenshots into `images/` and uncomment the corresponding `<!-- ![...] -->` lines in `overview.md`.
2. Add an `assetFiles` entry to `publishManifest.json` for each image, e.g.:
   ```json
   "assetFiles": [
       { "pathOnDisk": "images/solution-explorer.png", "targetPath": "images/solution-explorer.png" }
   ]
   ```
3. Fill in the still-missing marketplace-facing fields in `src/Zipadee.Vsix/source.extension.vsixmanifest` (icon, license, tags, preview image, more-info link) — see issue #16 for the full checklist.
4. Build the VSIX in Release configuration, then run:
   ```
   VsixPublisher.exe publish -payload "path\to\Zipadee.Vsix.vsix" -publishManifest "marketplace\publishManifest.json" -personalAccessToken "{PAT}"
   ```
