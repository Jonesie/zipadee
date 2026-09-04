# Zipadee.Build

Build-time MSBuild logic for **Zipadee Archive Projects** - shells out to
7-Zip, WinRAR, or `makecab.exe` (whichever is configured) to package a
project's output into a zip/7z/rar/cab archive as part of the build.

This package is not meant to be installed directly. It's pulled in
automatically via `PackageReference` by the `.zparchproj` project template
that ships with the [Zipadee Visual Studio extension](https://marketplace.visualstudio.com/items?itemName=Jonesie.Zipadee)
when you create a new Zipadee Archive Project.

See the [project file reference](https://jonesie.github.io/zipadee/docs/reference)
for the full list of supported MSBuild properties, or the
[extension's marketplace page](https://marketplace.visualstudio.com/items?itemName=Jonesie.Zipadee)
for an overview and getting-started guide.
