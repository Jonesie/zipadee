using Microsoft.VisualStudio.ProjectSystem.VS;

[assembly: ProjectTypeRegistration(
    projectTypeGuid: Zipadee.Vsix.ProjectTypeGuids.ZipadeeArchiveProjectString,
    displayName: "Zipadee Archive Project",
    displayProjectFileExtensions: "Zipadee Archive Project Files (*.zparchproj)",
    defaultProjectExtension: "zparchproj",
    language: "Zipadee",
    resourcePackageGuid: Zipadee.Vsix.ZipadeePackage.PackageGuidString,
    PossibleProjectExtensions = "zparchproj",
    // AppDesigner is what makes VS route the "Properties" command to the tabbed CPS Property
    // Pages designer (where PropertyPageSchema-registered Rule XAML pages render) instead of
    // just focusing the classic Properties tool window. Every built-in managed project type
    // (C#/VB/F#) carries it as part of a shared "Default" capability set - see
    // ProjectTypeCapabilities.cs in https://github.com/dotnet/project-system - but it isn't
    // implied by Microsoft.Build.NoTargets or by this attribute, so it has to be listed here
    // explicitly.
    Capabilities = "AppDesigner;Zipadee")]
