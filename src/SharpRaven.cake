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
    .Does(() =>
{
    // NuGet pack app\SharpRaven\SharpRaven.csproj -Properties ReleaseNotes='Test'
    // NuGet pack app\SharpRaven.Nancy\SharpRaven.Nancy.csproj -Properties ReleaseNotes='Test'

    NuGetPack("./app/SharpRaven/SharpRaven.csproj", new NuGetPackSettings
    {
        Id                      = "TestNuget",
        Version                 = "0.0.0.1",
        Title                   = "The tile of the package",
        Authors                 = new[] {"John Doe"},
        Owners                  = new[] {"Contoso"},
        Description             = "The description of the package",
        Summary                 = "Excellent summary of what the package does",
        ProjectUrl              = new Uri("https://github.com/SomeUser/TestNuget/"),
        IconUrl                 = new Uri("http://cdn.rawgit.com/SomeUser/TestNuget/master/icons/testnuget.png"),
        LicenseUrl              = new Uri("https://github.com/SomeUser/TestNuget/blob/master/LICENSE.md"),
        Copyright               = "Some company 2015",
        ReleaseNotes            = new [] {"Bug fixes", "Issue fixes", "Typos"},
        Tags                    = new [] {"Cake", "Script", "Build"},
        RequireLicenseAcceptance= false,
        Symbols                 = false,
        NoPackageAnalysis       = true,
        Files                   = new [] { new NuSpecContent {Source = "bin/TestNuget.dll", Target = "bin"}, },
        BasePath                = "./src/TestNuget/bin/release",
        OutputDirectory         = "./nuget"
    });
});

Task("Default")
    .IsDependentOn("Build");


RunTarget(target);