#!./packages/Cake.0.8.0/Cake.exe

var target = Argument("target", "Default");

/*Task("Clean")
    .Does(() =>
{
});*/

Task("Restore-NuGet-Packages")
//    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./SharpRaven.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    if (IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("./SharpRaven.sln", settings =>
        settings.SetConfiguration("Release 4.0"));

      MSBuild("./SharpRaven.sln", settings =>
        settings.SetConfiguration("Release 4.5"));
    }
    else
    {
      // Use XBuild
      XBuild("./SharpRaven.sln", settings =>
        settings.SetConfiguration("Release 4.0"));

      XBuild("./SharpRaven.sln", settings =>
        settings.SetConfiguration("Release 4.5"));
    }
});

Task("NuGet-Pack")
    .IsDependentOn("Build")
    .Does(() =>
{
    var gitVersion = GitVersion(new GitVersionSettings
    {
        OutputType          = GitVersionOutput.Json,
        UpdateAssemblyInfo  = false
    });

    Information("Version: {0}", gitVersion.NuGetVersion);

    NuGetPack("./app/SharpRaven/SharpRaven.nuspec", new NuGetPackSettings
    {
        Id      		= "TestNuget",
        Version 		= gitVersion.NuGetVersion,
        Symbols         = true,
        ReleaseNotes	= new[] { "Test" }
    });

    NuGetPack("./app/SharpRaven.Nancy/SharpRaven.Nancy.nuspec", new NuGetPackSettings
    {
        Id      		= "TestNuget",
        Version 		= gitVersion.NuGetVersion,
        Symbols         = true,
        ReleaseNotes	= new[] { "Test" }
    });
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);