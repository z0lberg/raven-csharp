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
    return;

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

    var sharpRavenFiles = GetFiles("../src/app/SharpRaven/bin/Release/**/SharpRaven.*")
        .Select(f => new NuSpecContent { Source = f.ToString(), Target = "bin" })
        .ToList();

    foreach (var file in sharpRavenFiles)
    {
        Information("{0}", file.Source);
    }

    var sharpRavenNancyFiles = GetFiles("../src/app/SharpRaven.Nancy/bin/Release/**/SharpRaven.Nancy.*")
        .Select(f => new NuSpecContent { Source = f.ToString(), Target = "lib" })
        .ToList();

    NuGetPack(/*"../src/app/SharpRaven/SharpRaven.nuspec",*/ new NuGetPackSettings
    {
        Id                          = "SharpRaven",
        Version                     = semver,
        Title                       = ".NET client for Sentry (getsentry.com)",
        Authors                     = new[] { "Sentry Team and individual contributors" },
        Owners                      = new[] { "jsk", "asbjornu", "gmaclellan" },
        Description                 = "Raven is a C# client for Sentry (getsentry.com and github.com/getsentry/sentry)",
        Summary                     = "SharpRaven allows you to capture and send error messages to Sentry.",
        ProjectUrl                  = new Uri("https://github.com/getsentry/raven-csharp"),
        IconUrl                     = new Uri("https://raw.githubusercontent.com/getsentry/raven-csharp/master/sentry-icon.png"),
        LicenseUrl                  = new Uri("https://github.com/getsentry/raven-csharp/blob/master/LICENSE"),
        Copyright                   = "Copyright 2016 The Sentry Team and individual contributors",
        ReleaseNotes                = new[] { "Test" },
        Tags                        = new [] { "raven", "sentry", "logging", "exception", "error" },
        RequireLicenseAcceptance    = false,
        Symbols                     = true,
        NoPackageAnalysis           = true,
        Files                       = sharpRavenFiles,
        BasePath                    = "././",
        OutputDirectory             = "./artifacts/"
        // OutputDirectory             = "/Volumes/Dev/Misc/raven-csharp/build/artifacts/"
    });

    NuGetPack(/*"../src/app/SharpRaven.Nancy/SharpRaven.Nancy.nuspec",*/ new NuGetPackSettings
    {
        Id                          = "SharpRaven.Nancy",
        Version                     = semver,
        Title                       = ".NET client for Sentry (getsentry.com) running on Nancy",
        Authors                     = new[] { "Sentry Team and individual contributors" },
        Owners                      = new[] { "asbjornu", "xpicio" },
        Description                 = "Raven is a C# client for Sentry (getsentry.com and github.com/getsentry/sentry)",
        Summary                     = "SharpRaven allows you to capture and send error messages to Sentry.",
        ProjectUrl                  = new Uri("https://github.com/getsentry/raven-csharp"),
        IconUrl                     = new Uri("https://raw.githubusercontent.com/getsentry/raven-csharp/master/sentry-icon.png"),
        LicenseUrl                  = new Uri("https://github.com/getsentry/raven-csharp/blob/master/LICENSE"),
        Copyright                   = "Copyright 2016 The Sentry Team and individual contributors",
        ReleaseNotes                = new[] { "Test" },
        Tags                        = new [] { "raven", "sentry", "logging", "exception", "error", "nancy" },
        RequireLicenseAcceptance    = false,
        Symbols                     = true,
        NoPackageAnalysis           = true,
        // Files                       = sharpRavenNancyFiles,
        BasePath                    = "././",
        OutputDirectory             = "./artifacts/"
        //OutputDirectory             = "/Volumes/Dev/Misc/raven-csharp/build/artifacts/"
    });
});


Task("Default")
    .IsDependentOn("Build");


RunTarget(target);