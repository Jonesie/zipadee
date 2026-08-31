using Microsoft.VisualStudio.ProjectSystem.VS;

[assembly: ProjectTypeRegistration(
    projectTypeGuid: Zipadee.Vsix.ProjectTypeGuids.ZipadeeArchiveProjectString,
    displayName: "Zipadee Archive Project",
    displayProjectFileExtensions: "Zipadee Archive Project Files (*.zparchproj)",
    defaultProjectExtension: "zparchproj",
    language: "Zipadee",
    resourcePackageGuid: Zipadee.Vsix.ZipadeePackage.PackageGuidString,
    PossibleProjectExtensions = "zparchproj",
    Capabilities = "Zipadee")]
