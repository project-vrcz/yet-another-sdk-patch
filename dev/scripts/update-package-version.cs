#!/usr/bin/dotnet run

#:package System.CommandLine@2.0.0

using System.CommandLine;
using System.Text.Json.Nodes;

var pathToPackageJson = new Argument<FileInfo>("package-json-file-path")
{
    Description = "Path to package.json file"
}.AcceptExistingOnly();


var newVersionArgument = new Argument<string>("new-version")
{
    Description = "Version to release"
};

var dryRunOption = new Option<bool>("--dry-run")
{
    Description = "Run script without modify change log file",
    DefaultValueFactory = _ => false
};

var rootCommand = new RootCommand("Update version field in package.json file");
rootCommand.Arguments.Add(pathToPackageJson);
rootCommand.Arguments.Add(newVersionArgument);
rootCommand.Options.Add(dryRunOption);

rootCommand.SetAction(async parseResult =>
{
    var packageJsonFile = parseResult.GetRequiredValue(pathToPackageJson);
    var newVersion = parseResult.GetRequiredValue(newVersionArgument);
    var dryRun = parseResult.GetValue(dryRunOption);

    var pathToPackageJsonFile = packageJsonFile.FullName;
    var packageJsonRaw = await File.ReadAllTextAsync(pathToPackageJsonFile);

    var jsonNode = JsonNode.Parse(packageJsonRaw) ?? throw new Exception("Failed to parse package.json file");
    jsonNode["version"] = newVersion;

    var updatedPackageJsonRaw = jsonNode.ToJsonString(new System.Text.Json.JsonSerializerOptions
    {
        WriteIndented = true
    });

    Console.WriteLine(updatedPackageJsonRaw);

    if (dryRun)
        return;

    await File.WriteAllTextAsync(pathToPackageJsonFile, updatedPackageJsonRaw);
});

return await rootCommand.Parse(args).InvokeAsync();