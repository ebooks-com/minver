using MinVer.Lib;
using System.Diagnostics.CodeAnalysis;

namespace MinVer;

internal sealed class Options
{
    private Options(
        VersionPart? autoIncrement,
        string? buildMeta,
        IEnumerable<string>? defaultPreReleaseIdentifiers,
        string? defaultPreReleasePhase,
        bool? ignoreHeight,
        MajorMinor? minMajorMinor,
        string? tagPrefix,
        Verbosity? verbosity,
        Lib.Version? versionOverride,
        bool? ignorePreReleaseIdentifiers,
        bool? includeBranchName,
        IEnumerable<string>? ignoreBranchNames)
    {
        this.AutoIncrement = autoIncrement;
        this.BuildMeta = buildMeta;
        this.DefaultPreReleaseIdentifiers = defaultPreReleaseIdentifiers;
        this.DefaultPreReleasePhase = defaultPreReleasePhase;
        this.IgnoreHeight = ignoreHeight;
        this.MinMajorMinor = minMajorMinor;
        this.TagPrefix = tagPrefix;
        this.Verbosity = verbosity;
        this.VersionOverride = versionOverride;
        this.IgnorePreReleaseIdentifiers = ignorePreReleaseIdentifiers;
        this.IncludeBranchName = includeBranchName;
        this.IgnoreBranchNames = ignoreBranchNames;
    }

#if MINVER_CLI
    public static bool TryParseEnvVars([NotNullWhen(returnValue: true)] out Options? options)
    {
        options = null;

        VersionPart? autoIncrement = null;
        IEnumerable<string>? defaultPreReleaseIdentifiers = null;
        bool? ignoreHeight = null;
        MajorMinor? minMajorMinor = null;
        Verbosity? verbosity = null;
        Lib.Version? versionOverride = null;
        bool? ignorePreReleaseIdentifiers = null;
        bool? includeBranchName = null;
        IEnumerable<string>? ignoreBranchNames = null;

        var autoIncrementEnvVar = GetEnvVar("MinVerAutoIncrement");
        if (!string.IsNullOrEmpty(autoIncrementEnvVar))
        {
            if (Enum.TryParse<VersionPart>(autoIncrementEnvVar, true, out var versionPart))
            {
                autoIncrement = versionPart;
            }
            else
            {
                Logger.ErrorInvalidEnvVar("MinVerAutoIncrement", autoIncrementEnvVar, VersionPartExtensions.ValidValues);
                return false;
            }
        }

        var buildMeta = GetEnvVar("MinVerBuildMetadata");

        var defaultPreReleaseIdentifiersEnvVar = GetEnvVar("MinVerDefaultPreReleaseIdentifiers");
        if (!string.IsNullOrEmpty(defaultPreReleaseIdentifiersEnvVar))
        {
            defaultPreReleaseIdentifiers = defaultPreReleaseIdentifiersEnvVar.Split('.');
        }

        var defaultPreReleasePhase = GetEnvVar("MinVerDefaultPreReleasePhase");

        var ignoreHeightEnvVar = GetEnvVar("MinVerIgnoreHeight");
        if (!string.IsNullOrEmpty(ignoreHeightEnvVar))
        {
            if (bool.TryParse(ignoreHeightEnvVar, out var value))
            {
                ignoreHeight = value;
            }
            else
            {
                Logger.ErrorInvalidEnvVar("MinVerIgnoreHeight", ignoreHeightEnvVar, "true, false (case insensitive)");
                return false;
            }
        }

        var minMajorMinorEnvVar = GetEnvVar("MinVerMinimumMajorMinor");
        if (!string.IsNullOrEmpty(minMajorMinorEnvVar) && !MajorMinor.TryParse(minMajorMinorEnvVar, out minMajorMinor))
        {
            Logger.ErrorInvalidEnvVar("MinVerMinimumMajorMinor", minMajorMinorEnvVar, MajorMinor.ValidValues);
            return false;
        }

        var tagPrefix = GetEnvVar("MinVerTagPrefix");

        var verbosityEnvVar = GetEnvVar("MinVerVerbosity");
        if (!string.IsNullOrEmpty(verbosityEnvVar) && !VerbosityMap.TryMap(verbosityEnvVar, out verbosity))
        {
            Logger.ErrorInvalidEnvVar("MinVerVerbosity", verbosityEnvVar, VerbosityMap.ValidValues);
            return false;
        }

        var versionOverrideEnvVar = GetEnvVar("MinVerVersionOverride");
        if (!string.IsNullOrEmpty(versionOverrideEnvVar) && !Lib.Version.TryParse(versionOverrideEnvVar, out versionOverride))
        {
            Logger.ErrorInvalidEnvVar("MinVerVersionOverride", versionOverrideEnvVar, "");
            return false;
        }

        var ignorePreReleaseIdentifiersEnvVar = GetEnvVar("MinVerIgnorePreReleaseIdentifiers");
        if (!string.IsNullOrEmpty(ignorePreReleaseIdentifiersEnvVar))
        {
            if (bool.TryParse(ignorePreReleaseIdentifiersEnvVar, out var value))
            {
                ignorePreReleaseIdentifiers = value;
            }
            else
            {
                Logger.ErrorInvalidEnvVar("MinVerIgnorePreReleaseIdentifiers", ignorePreReleaseIdentifiersEnvVar, "true, false (case insensitive)");
                return false;
            }
        }

        var includeBranchNameEnvVar = GetEnvVar("MinVerIncludeBranchName");
        if (!string.IsNullOrEmpty(includeBranchNameEnvVar))
        {
            if (bool.TryParse(includeBranchNameEnvVar, out var value))
            {
                includeBranchName = value;
            }
            else
            {
                Logger.ErrorInvalidEnvVar("MinVerIncludeBranchName", includeBranchNameEnvVar, "true, false (case insensitive)");
                return false;
            }
        }

        var ignoreBranchNamesEnvVar = GetEnvVar("MinVerIgnoreBranchNames");
        if (!string.IsNullOrEmpty(ignoreBranchNamesEnvVar))
        {
            ignoreBranchNames = ignoreBranchNamesEnvVar.Split(';');
            foreach (var ignoreBranchName in ignoreBranchNames)
            {
                _ = Logger.Message($"Ignoring branch '{ignoreBranchName}'");
            }
        }

        options = new Options(autoIncrement, buildMeta, defaultPreReleaseIdentifiers, defaultPreReleasePhase, ignoreHeight, minMajorMinor, tagPrefix, verbosity, versionOverride, ignorePreReleaseIdentifiers, includeBranchName, ignoreBranchNames);

        return true;
    }

    private static string? GetEnvVar(string name)
    {
        var vars = Environment.GetEnvironmentVariables();

        var key = vars.Keys
            .Cast<string>()
            .OrderBy(key => key, StringComparer.Ordinal)
            .FirstOrDefault(key => string.Equals(key, name, StringComparison.OrdinalIgnoreCase));

        return key == null ? null : (string?)vars[key];
    }
#endif

    public static bool TryParse(
        string? autoIncrementOption,
        string? buildMetaOption,
        string? defaultPreReleaseIdentifiersOption,
        string? defaultPreReleasePhaseOption,
        bool? ignoreHeight,
        string? minMajorMinorOption,
        string? tagPrefixOption,
        string? verbosityOption,
#if MINVER
        string? versionOverrideOption,
#endif
        bool? ignorePreReleaseIdentifiersOption,
        bool? includeBranchNameOption,
        string? ignoreBranchNamesOption,
        [NotNullWhen(returnValue: true)] out Options? options)
    {
        options = null;

        VersionPart? autoIncrement = null;
        IEnumerable<string>? defaultPreReleaseIdentifiers = null;
        MajorMinor? minMajorMinor = null;
        Verbosity? verbosity = null;
        Lib.Version? versionOverride = null;
        IEnumerable<string>? ignoreBranchNames = null;

        if (!string.IsNullOrEmpty(autoIncrementOption))
        {
            if (Enum.TryParse<VersionPart>(autoIncrementOption, true, out var versionPart))
            {
                autoIncrement = versionPart;
            }
            else
            {
                Logger.ErrorInvalidAutoIncrement(autoIncrementOption);
                return false;
            }
        }

        if (!string.IsNullOrEmpty(defaultPreReleaseIdentifiersOption))
        {
            defaultPreReleaseIdentifiers = defaultPreReleaseIdentifiersOption.Split('.');
        }

        if (!string.IsNullOrEmpty(minMajorMinorOption) && !MajorMinor.TryParse(minMajorMinorOption, out minMajorMinor))
        {
            Logger.ErrorInvalidMinMajorMinor(minMajorMinorOption);
            return false;
        }

        if (!string.IsNullOrEmpty(verbosityOption) && !VerbosityMap.TryMap(verbosityOption, out verbosity))
        {
            Logger.ErrorInvalidVerbosity(verbosityOption);
            return false;
        }

#if MINVER
        if (!string.IsNullOrEmpty(versionOverrideOption) && !Lib.Version.TryParse(versionOverrideOption, out versionOverride))
        {
            Logger.ErrorInvalidVersionOverride(versionOverrideOption);
            return false;
        }
#endif

        if (!string.IsNullOrEmpty(ignoreBranchNamesOption))
        {
            ignoreBranchNames = ignoreBranchNamesOption.Split(' ');
        }

        options = new Options(autoIncrement, buildMetaOption, defaultPreReleaseIdentifiers, defaultPreReleasePhaseOption, ignoreHeight, minMajorMinor, tagPrefixOption, verbosity, versionOverride, ignorePreReleaseIdentifiersOption, includeBranchNameOption, ignoreBranchNames);

        return true;
    }

#if MINVER_CLI
    public Options Mask(Options other) => new(
        this.AutoIncrement ?? other.AutoIncrement,
        this.BuildMeta ?? other.BuildMeta,
        this.DefaultPreReleaseIdentifiers ?? other.DefaultPreReleaseIdentifiers,
        this.DefaultPreReleasePhase ?? other.DefaultPreReleasePhase,
        this.IgnoreHeight ?? other.IgnoreHeight,
        this.MinMajorMinor ?? other.MinMajorMinor,
        this.TagPrefix ?? other.TagPrefix,
        this.Verbosity ?? other.Verbosity,
        this.VersionOverride ?? other.VersionOverride,
        this.IgnorePreReleaseIdentifiers ?? other.IgnorePreReleaseIdentifiers,
        this.IncludeBranchName ?? other.IncludeBranchName,
        this.IgnoreBranchNames ?? other.IgnoreBranchNames);
#endif

    public VersionPart? AutoIncrement { get; }

    public string? BuildMeta { get; }

    public IEnumerable<string>? DefaultPreReleaseIdentifiers { get; }

    public string? DefaultPreReleasePhase { get; }

    public bool? IgnoreHeight { get; }

    public MajorMinor? MinMajorMinor { get; }

    public string? TagPrefix { get; }

    public Verbosity? Verbosity { get; }

    public Lib.Version? VersionOverride { get; }

    public bool? IgnorePreReleaseIdentifiers { get; set; }

    public bool? IncludeBranchName { get; }

    public IEnumerable<string>? IgnoreBranchNames { get; }
}
