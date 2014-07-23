// Get arguments passed to the script

var target = Argument("target", "All");
var configuration = Argument("configuration", "Release");
var buildLabel = Argument("buildLabel", string.Empty);
var buildInfo = Argument("buildInfo", string.Empty);

// Parse release notes.
var releaseNotes = ParseReleaseNotes("./ReleaseNotes.md");

// Set version.
var version = releaseNotes.Version.ToString();
var semVersion = version + (buildLabel != "" ? ("-" + buildLabel) : string.Empty);
Information("Building version {0} of DocLite.", version);

// Define directories.
var buildDir = "./src/DocLite/bin/" + configuration;
var buildResultDir = "./build/" + "v" + semVersion;
var testResultsDir = buildResultDir + "/test-results";
var nugetRoot = buildResultDir + "/nuget";
var binDir = buildResultDir + "/bin";

//////////////////////////////////////////////////////////////////////////

Task("Clean")
	.Does(() =>
{
	CleanDirectories(new DirectoryPath[] {
		buildResultDir, binDir, testResultsDir, nugetRoot});    
});

Task("Restore-NuGet-Packages")
	.IsDependentOn("Clean")
	.Does(context =>
{
	// Restore NuGet packages.
	NuGetRestore("./src/DocLite.sln");    
});

Task("Patch-Assembly-Info")
	.Description("Patches the AssemblyInfo files.")
	.IsDependentOn("Restore-NuGet-Packages")
	.Does(() =>
{
	var file = "./src/SolutionInfo.cs";
	CreateAssemblyInfo(file, new AssemblyInfoSettings {
		Product = "DocLite",
		Version = version,
		FileVersion = version,
		InformationalVersion = (version + buildInfo).Trim(),
		Copyright = "Copyright (c) Jeff Doolittle 2014"
	});
});

Task("Build")
	.IsDependentOn("Patch-Assembly-Info")
	.Does(() =>
{
	MSBuild("./src/DocLite.sln", s => 
		{ 
			s.Configuration = configuration;
			s.ToolVersion = MSBuildToolVersion.NET40;
		});
});

Task("Run-Unit-Tests")
	.IsDependentOn("Build")
	.Does(() =>
{
	XUnit("./src/**/bin/" + configuration + "/*.Tests.dll", new XUnitSettings {
		OutputDirectory = testResultsDir,
		XmlReport = true,
		HtmlReport = true
	});
});

Task("Copy-Files")
	.IsDependentOn("Run-Unit-Tests")
	.Does(() =>
{
	CopyFileToDirectory(buildDir + "/DocLite.dll", binDir);
	CopyFiles(new FilePath[] { "LICENSE", "README.md", "ReleaseNotes.md" }, binDir);
});

Task("ILMerge")
	.IsDependentOn("Copy-Files")
	.Does(context => 
{
	var outputFile = binDir + "/DocLite.dll";

	var sourcePath = "./src/DocLite/bin/" + configuration;    
	var primaryAssembly = new FilePath(sourcePath + "/DocLite.dll");
	var assemblyPaths = new List<FilePath>();
	assemblyPaths.Add(new FilePath(sourcePath + "/Esent.Collections.dll"));
	assemblyPaths.Add(new FilePath(sourcePath + "/Esent.Interop.dll"));
	assemblyPaths.Add(new FilePath(sourcePath + "/Newtonsoft.Json.dll"));

	var settings = new ILMergeSettings();
	context.ILMerge(outputFile, primaryAssembly, assemblyPaths, settings);
});

Task("Create-NuGet-Package")
	.IsDependentOn("ILMerge")
	.Does(() =>
{
	NuGetPack("./src/DocLite/DocLite.nuspec", new NuGetPackSettings {
		Version = version,
		BasePath = binDir,
		OutputDirectory = nugetRoot,
		Symbols = false,
		NoPackageAnalysis = true
	});
});

Task("All")
	.Description("Final target.")
	.IsDependentOn("Create-NuGet-Package");

//////////////////////////////////////////////////////////////////////////

RunTarget(target);