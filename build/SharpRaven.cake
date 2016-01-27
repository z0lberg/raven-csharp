#!./packages/Cake.0.8.0/Cake.exe

var target = Argument("target", "Default");

/*Task("Clean")
    .Does(() =>
{
});*/

Task("NuGet-Restore")
//    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("./SharpRaven.sln");
});


Task("Build")
    .IsDependentOn("NuGet-Restore")
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


Task("Test")
    .IsDependentOn("Build")
    .Does(() => 
{
    var testFiles = GetFiles("./tests/**/Release/**/*.UnitTests.dll");
    if (!testFiles.Any())
        throw new FileNotFoundException("Could not find any tests");

    NUnit(testFiles, new NUnitSettings
    {
        ToolPath = GetFiles("./packages/**/tools/nunit-console.exe").First().ToString()
    });
});


Task("NuGet-Pack")
    .IsDependentOn("Test")
    .Does(() =>
{
    var gitVersion = IsRunningOnWindows() ? GitVersion(new GitVersionSettings
    {
        OutputType          = GitVersionOutput.Json,
        UpdateAssemblyInfo  = false
    }) : null;

    var semver = gitVersion != null ? gitVersion.NuGetVersion : "0.0.1-mono";

    Information("Version: {0}", semver);

    NuGetPack("../src/app/SharpRaven/SharpRaven.nuspec", new NuGetPackSettings
    {
        Version 		= semver,
        Symbols         = true,
        ReleaseNotes	= new[] { "Test" }
    });

    NuGetPack("./app/SharpRaven.Nancy/SharpRaven.Nancy.nuspec", new NuGetPackSettings
    {
        Version 		= semver,
        Symbols         = true,
        ReleaseNotes	= new[] { "Test" }
    });
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);