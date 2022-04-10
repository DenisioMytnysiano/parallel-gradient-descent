#tool "nuget:?package=NuGet.CommandLine&version=5.8.1"

var target = Argument("target", "Build");
var configuration = Argument("configuration", "Release");
var framework = Argument("framework", "netcoreapp2.0");


Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
    {
        CleanDirectory($"./PGD.Cli/bin/{configuration}");
        CleanDirectory($"./PGD.Benchmark/bin/{configuration}");
        CleanDirectory($"./PGD.Core/bin/{configuration}");
    });

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore("./ParallelGradientDescent.sln");
    });

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
    {
        DotNetCoreBuild("./ParallelGradientDescent.sln", new DotNetCoreBuildSettings
        {
            Configuration = configuration,
        });
    });

RunTarget(target);
