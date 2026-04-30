using Suity.Synchonizing;
using System;

namespace Suity.Editor.Helpers;

/// <summary>
/// Provides extension methods for version synchronization and manipulation operations.
/// </summary>
public static class VersionHelper
{
    /// <summary>
    /// Synchronizes a <see cref="Version"/> value with a property sync source.
    /// If the sync is in getter mode, it sends the current version as a string.
    /// If the sync is in setter mode, it retrieves a version string and parses it.
    /// Defaults to version 1.0.0.0 if the input version is null.
    /// </summary>
    /// <param name="sync">The property synchronization interface.</param>
    /// <param name="name">The name of the property to synchronize.</param>
    /// <param name="version">The version value to sync or use as fallback.</param>
    /// <returns>The synchronized version, or the original/default version if parsing fails.</returns>
    public static Version SyncVersion(this IPropertySync sync, string name, Version version)
    {
        if (version == null)
        {
            version = new Version(1, 0, 0, 0);
        }

        if (sync.IsGetterOf(name))
        {
            sync.Sync(name, version.ToString(), SyncFlag.NotNull);
        }
        else if (sync.IsSetterOf(name))
        {
            string value = sync.Sync(name, string.Empty, SyncFlag.NotNull);
            if (Version.TryParse(value, out Version result))
            {
                return result;
            }
        }

        return version;
    }

    /// <summary>
    /// Increases the major component of a <see cref="Version"/> by the specified increment.
    /// </summary>
    /// <param name="version">The version to increase.</param>
    /// <param name="increment">The amount to add to the major component. Defaults to 1.</param>
    /// <returns>A new <see cref="Version"/> with the increased major value.</returns>
    public static Version IncreaseMajor(this Version version, int increment = 1)
    {
        return new Version(version.Major + increment, version.Minor, version.Build, version.Revision);
    }

    /// <summary>
    /// Increases the minor component of a <see cref="Version"/> by the specified increment.
    /// </summary>
    /// <param name="version">The version to increase.</param>
    /// <param name="increment">The amount to add to the minor component. Defaults to 1.</param>
    /// <returns>A new <see cref="Version"/> with the increased minor value.</returns>
    public static Version IncreaseMinor(this Version version, int increment = 1)
    {
        return new Version(version.Major, version.Minor + increment, version.Build, version.Revision);
    }

    /// <summary>
    /// Increases the build component of a <see cref="Version"/> by the specified increment.
    /// If the build number is undefined, falls back to increasing the minor component instead.
    /// </summary>
    /// <param name="version">The version to increase.</param>
    /// <param name="increment">The amount to add to the build component. Defaults to 1.</param>
    /// <returns>A new <see cref="Version"/> with the increased build value.</returns>
    public static Version IncreaseBuild(this Version version, int increment = 1)
    {
        if (version.Build >= 0)
        {
            return new Version(version.Major, version.Minor, version.Build + increment, version.Revision);
        }
        else
        {
            return new Version(version.Major, version.Minor + increment);
        }
    }

    /// <summary>
    /// Increases the revision component of a <see cref="Version"/> by the specified increment.
    /// If the build or revision number is undefined, falls back to increasing the minor component instead.
    /// </summary>
    /// <param name="version">The version to increase.</param>
    /// <param name="increment">The amount to add to the revision component. Defaults to 1.</param>
    /// <returns>A new <see cref="Version"/> with the increased revision value.</returns>
    public static Version IncreaseRevision(this Version version, int increment = 1)
    {
        if (version.Build >= 0 && version.Revision >= 0)
        {
            return new Version(version.Major, version.Minor, version.Build, version.Revision + increment);
        }
        else
        {
            return new Version(version.Major, version.Minor + increment);
        }
    }

    /// <summary>
    /// Compares two <see cref="Version"/> objects and returns their relative order.
    /// </summary>
    /// <param name="version1">The first version to compare.</param>
    /// <param name="version2">The second version to compare.</param>
    /// <returns>A negative value if version1 is less than version2, zero if equal, or a positive value if version1 is greater.</returns>
    public static int CompareVersions(Version version1, Version version2)
    {
        return version1.CompareTo(version2);
    }

    /// <summary>
    /// Compares two version strings by parsing them into <see cref="Version"/> objects.
    /// Returns 0 if parsing fails for either string.
    /// </summary>
    /// <param name="version1">The first version string to compare.</param>
    /// <param name="version2">The second version string to compare.</param>
    /// <returns>A negative value if version1 is less than version2, zero if equal or parsing fails, or a positive value if version1 is greater.</returns>
    public static int CompareVersions(string version1, string version2)
    {
        try
        {
            var v1 = new Version(version1);
            var v2 = new Version(version2);
            return v1.CompareTo(v2);
        }
        catch (Exception)
        {
            return 0;
        }
    }
}