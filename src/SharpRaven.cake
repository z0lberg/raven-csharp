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
    // mono --debug --runtime=v4.0.30319 ./src/packages/NUnit.Runners.2.6.4/tools/nunit-console.exe ./src/tests/SharpRaven.UnitTests/bin/Release/net45/SharpRaven.UnitTests.dll -exclude=NuGet,NoMono -nodots
    var testFiles = GetFiles("./tests/**/Release/**/*.UnitTests.dll");
    if (!testFiles.Any())
        throw new FileNotFoundException("Could not find any tests");

    NUnit(testFiles);

    /*NUnit(new[] { "", "" }, new NUnitSettings
    {
        Framework = 
    });*/
});


Task("NuGet-Pack")
    .IsDependentOn("Test")
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
        Version 		= gitVersion.NuGetVersion,
        Symbols         = true,
        ReleaseNotes	= new[] { "Test" }
    });

    NuGetPack("./app/SharpRaven.Nancy/SharpRaven.Nancy.nuspec", new NuGetPackSettings
    {
        Version 		= gitVersion.NuGetVersion,
        Symbols         = true,
        ReleaseNotes	= new[] { "Test" }
    });
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);