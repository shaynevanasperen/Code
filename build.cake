//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
{
	CleanDirectories("./src/**/bin/*");
	CleanDirectories("./src/**/obj/*");
	CleanDirectories("./artifacts");
});

Task("Restore")
	.IsDependentOn("Clean")
	.Does(() =>
{
	DotNetCoreRestore(".");
});

Task("Build")
	.IsDependentOn("Restore")
	.Does(() =>
{
	DotNetCoreBuild("./src/Code.sln", new DotNetCoreBuildSettings
	{
		Configuration = configuration
	});
});

Task("Test")
	.IsDependentOn("Restore")
	.Does(() =>
{
	DotNetCoreTest("./test/Code.Tests/Code.Tests.csproj", new DotNetCoreTestSettings
	{
		Configuration = configuration
	});
});

Task("Pack")
	.IsDependentOn("Test")
	.Does(() =>
{
	EnsureDirectoryExists("./artifacts");
	var nuGetPackSettings = new NuGetPackSettings
	{
		Authors = new[] { "Shayne van Asperen" },
		Owners = new[] { "Shayne van Asperen" },
		ProjectUrl  = new Uri("https://github.com/shaynevanasperen/Code"),
		IconUrl = new Uri("https://raw.githubusercontent.com/shaynevanasperen/Code/master/Code.png"),
		LicenseUrl = new Uri("https://github.com/shaynevanasperen/Code/blob/master/LICENSE"),
		RequireLicenseAcceptance = false,
		DevelopmentDependency = true,
    OutputDirectory = "./artifacts"
	};
	var nuspecFiles = GetFiles("./**/*.nuspec");
	NuGetPack(nuspecFiles, nuGetPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
	.IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
