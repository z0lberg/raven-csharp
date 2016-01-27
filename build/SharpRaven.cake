#!../src/packages/Cake.0.8.0/Cake.exe

var target = Argument("target", "Default");


Task("Clean")
    .Does(() =>
{
    CreateDirectory("./artifacts");
    CleanDirectory("./artifacts");
});


Task("NuGet-Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore("../src/SharpRaven.sln");
});


Task("Build")
    .IsDependentOn("NuGet-Restore")
    .Does(() =>
{
    if (IsRunningOnWindows())
    {
      // Use MSBuild
      MSBuild("../src/SharpRaven.sln", settings =>
        settings.SetConfiguration("Release 4.0"));

      MSBuild("../src/SharpRaven.sln", settings =>
        settings.SetConfiguration("Release 4.5"));
    }
    else
    {
      // Use XBuild
      XBuild("../src/SharpRaven.sln", settings =>
        settings.SetConfiguration("Release 4.0"));

      XBuild("../src/SharpRaven.sln", settings =>
        settings.SetConfiguration("Release 4.5"));
    }
});


Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    var testFiles = GetFiles("../src/tests/**/Release/**/*.UnitTests.dll");
    if (!testFiles.Any())
        throw new FileNotFoundException("Could not find any tests");

    NUnit(testFiles, new NUnitSettings
    {
        ResultsFile = "./artifacts/TestResults.xml",
        Exclude = IsRunningOnWindows() ? null : "NoMono",
        ToolPath = GetFiles("../src/packages/**/tools/nunit-console.exe").First().ToString(),
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
        ReleaseNotes	= new[] { "Test" },
        OutputDirectory = "./artifacts/"
    });

    NuGetPack("../src/app/SharpRaven.Nancy/SharpRaven.Nancy.nuspec", new NuGetPackSettings
    {
        Version 		= semver,
        Symbols         = true,
        ReleaseNotes	= new[] { "Test" },
        OutputDirectory = "./artifacts/"
    });
});


Task("Default")
    .IsDependentOn("Build");


RunTarget(target);