#!/usr/bin/dotnet run

#:package System.CommandLine@2.0.0

using System.CommandLine;
using System.Text.RegularExpressions;

const string githubComparePath = "/compare/";
const string githubReleasePath = "/releases/tag/";

var pathToChangeLogArgument = new Argument<FileInfo>("change-log-file-path")
{
    Description = "Path to CHANGELOG.md file"
}.AcceptExistingOnly();

var newVersionArgument = new Argument<string>("new-version")
{
    Description = "Version to release"
};

var githubRepoUrlArgument = new Argument<string>("github-repo-url")
{
    Description = "GitHub repository URL"
};

var gitTagPrefixOption = new Option<string>("--git-tag-prefix")
{
    Description = "Git tag prefix, will append as {git-tag-prefix}v{version}",
    DefaultValueFactory = _ => ""
};

var keepUnreleasedChangesOption = new Option<bool>("--keep-unreleased-changes")
{
    Description = "Keep unreleased changes in the changelog",
    DefaultValueFactory = _ => false
};

var printChangesToStdOption = new Option<bool>("--print-changes-to-stdout")
{
    Description = "Print result to stdout",
    DefaultValueFactory = _ => true
};

var dryRunOption = new Option<bool>("--dry-run")
{
    Description = "Run script without modify change log file",
    DefaultValueFactory = _ => false
};

var rootCommand = new RootCommand("Update Changelog when release new version");
rootCommand.Arguments.Add(pathToChangeLogArgument);
rootCommand.Arguments.Add(newVersionArgument);
rootCommand.Arguments.Add(githubRepoUrlArgument);
rootCommand.Options.Add(gitTagPrefixOption);
rootCommand.Options.Add(printChangesToStdOption);
rootCommand.Options.Add(dryRunOption);
rootCommand.Options.Add(keepUnreleasedChangesOption);

rootCommand.SetAction(async parseResult =>
{
    var changeLogFileInfo = parseResult.GetRequiredValue(pathToChangeLogArgument);
    var newVersion = parseResult.GetRequiredValue(newVersionArgument);
    var githubRepoUrl = parseResult.GetRequiredValue(githubRepoUrlArgument);
    var gitTagPrefix = parseResult.GetRequiredValue(gitTagPrefixOption);
    var printChangesToStd = parseResult.GetRequiredValue(printChangesToStdOption);
    var keepUnreleasedChanges = parseResult.GetRequiredValue(keepUnreleasedChangesOption);
    var dryRun = parseResult.GetRequiredValue(dryRunOption);

    var changeLogFileName = changeLogFileInfo.FullName;

    var date = DateOnly.FromDateTime(DateTime.Now);

    var changeLogRaw = await File.ReadAllTextAsync(changeLogFileName);

    var changes = ExtractUnreleaseChanges(changeLogRaw);

    changeLogRaw = UpdateReleasesLink(changeLogRaw, newVersion, githubRepoUrl, gitTagPrefix);
    changeLogRaw = InsertNewVersionHeading(changeLogRaw, newVersion, date);

    if (keepUnreleasedChanges)
    {
        var unreleasedHeadingEndIndex = GetUnreleasedHeadingEndIndex(changeLogRaw);
        changeLogRaw = changeLogRaw.Insert(unreleasedHeadingEndIndex, $"\n\n{changes}");
    }

    if (printChangesToStd)
        Console.Write(changes);
    else
        Console.Write(changeLogRaw);

    if (dryRun)
        return;

    await File.WriteAllTextAsync(changeLogFileName, changeLogRaw);
});

return await rootCommand.Parse(args).InvokeAsync();

static string ExtractUnreleaseChanges(string input)
{
    var unreleasedHeadingEndIndex = GetUnreleasedHeadingEndIndex(input);
    var latestReleaseHeadingStartIndex = GetLastetReleaseHeadingStartIndex(input);

    var changelog = input;
    if (latestReleaseHeadingStartIndex > 0)
    {
        changelog = input.Remove(latestReleaseHeadingStartIndex, input.Length - latestReleaseHeadingStartIndex);
    }
    else
    {
        var unreleasedLinkIndex = GetUnreleasedLinkIndex(changelog);
        changelog = input.Remove(unreleasedLinkIndex, input.Length - unreleasedLinkIndex);
    }

    changelog = changelog.Remove(0, unreleasedHeadingEndIndex);

    return changelog.TrimStart().TrimEnd();
}

static int GetLastetReleaseHeadingStartIndex(string input)
{
    var regex = ReleasesHeadingRegex();
    return regex.Match(input).Groups[0].Index;
}

static int GetUnreleasedLinkIndex(string input)
{
    var regex = UnreleasedLinkRegex();
    return regex.Match(input).Groups[0].Index;
}

static string UpdateReleasesLink(string input, string newVersion, string githubRepoUrl, string gitTagPrefix)
{
    var changeLog = input;

    var regex = UnreleasedLinkRegex();
    var unreleasedLinkGroup = regex.Match(changeLog).Groups[0];
    var unreleasedLinkGroupEndIndex = unreleasedLinkGroup.Index + unreleasedLinkGroup.Length;

    var latestVersion = GetLastetVersion(changeLog);

    var compareLink = latestVersion is null ?
        githubRepoUrl + githubReleasePath + $"{gitTagPrefix}v{newVersion}" :
        githubRepoUrl + githubComparePath + $"{gitTagPrefix}v{latestVersion}...{gitTagPrefix}v{newVersion}";

    var unreleasedCompareLink = githubRepoUrl + githubComparePath + $"{gitTagPrefix}v{newVersion}...HEAD";

    changeLog = changeLog.Insert(unreleasedLinkGroupEndIndex, $"[{newVersion}]: {compareLink}");
    changeLog = changeLog.Insert(unreleasedLinkGroupEndIndex, $"[unreleased]: {unreleasedCompareLink}\n");

    changeLog = changeLog.Remove(unreleasedLinkGroup.Index, unreleasedLinkGroup.Length);

    return changeLog;
}

static string? GetLastetVersion(string input)
{
    var regex = ReleasesHeadingRegex();
    var matchGroups = regex.Match(input).Groups;
    if (matchGroups.Count == 0)
        return null;

    var captures = matchGroups[1].Captures;

    if (captures.Count == 0)
        return null;

    return captures[0].Value;
}

static string InsertNewVersionHeading(string input, string version, DateOnly date)
{
    var unreleasedHeadingEndIndex = GetUnreleasedHeadingEndIndex(input);

    if (unreleasedHeadingEndIndex <= 0)
        throw new Exception("Unable to locate Unreleased Heading Posistion");

    return input.Insert(unreleasedHeadingEndIndex, $"\n\n## [{version}] - {date:O}");
}

static int GetUnreleasedHeadingEndIndex(string input)
{
    var unreleasedHeadingRegex = UnreleasedHeadingRegex();

    var unreleasedHeadingGroup = unreleasedHeadingRegex.Match(input).Groups[0];
    var unreleasedHeadingEndIndex = unreleasedHeadingGroup.Index + unreleasedHeadingGroup.Length;

    return unreleasedHeadingEndIndex;
}

partial class Program
{
    [GeneratedRegex(@"^## \[Unreleased\]", RegexOptions.Multiline)]
    private static partial Regex UnreleasedHeadingRegex();

    [GeneratedRegex(@"^\[unreleased\]: .+", RegexOptions.Multiline)]
    private static partial Regex UnreleasedLinkRegex();

    [GeneratedRegex(@"^## \[(?<Version>.+?)\] - (?<Date>\d\d\d\d-\d\d-\d\d)", RegexOptions.Multiline)]
    private static partial Regex ReleasesHeadingRegex();
}