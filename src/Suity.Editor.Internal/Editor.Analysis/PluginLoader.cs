using Suity.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Suity.Editor.Analysis;

/// <summary>
/// Loads a plugin assembly from a specified workspace and resolves dependent assemblies from the plugin's bin path.
/// </summary>
public class PluginLoader
{
    /// <summary>
    /// Gets the name of the workspace this plugin loader is associated with.
    /// </summary>
    public string WorkSpaceName { get; }

    /// <summary>
    /// Gets or sets the full path to the plugin assembly file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets the directory path containing the plugin's binary output.
    /// </summary>
    public string BinPath { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLoader"/> class.
    /// </summary>
    /// <param name="workSpaceName">The name of the workspace containing the plugin.</param>
    /// <param name="fileName">The full path to the plugin assembly file.</param>
    public PluginLoader(string workSpaceName, string fileName)
    {
        WorkSpaceName = workSpaceName;
        FileName = fileName;
        BinPath = Path.GetDirectoryName(fileName);
    }

    /// <summary>
    /// Loads the plugin assembly from the specified file and adds it to the collector.
    /// Registers a temporary assembly resolve handler to load dependent assemblies from the plugin's bin path.
    /// </summary>
    /// <param name="collector">The collection to add the loaded assembly to.</param>
    public void LoadAssembly(ICollection<Assembly> collector)
    {
        try
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var dir = new DirectoryInfo(BinPath);
            if (!dir.Exists)
            {
                Logs.LogError($"Failed to load plugin : {WorkSpaceName}, path not exist.");

                return;
            }

            var assembly = Assembly.LoadFrom(FileName);
            collector.Add(assembly);
        }
        catch (Exception err)
        {
            err.LogError($"Failed to load plugin : {WorkSpaceName}");

            return;
        }
        finally
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }
    }

    /// <summary>
    /// Handles assembly resolution events by searching for the requested assembly in the plugin's bin path.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The assembly resolve event arguments containing the assembly name to resolve.</param>
    /// <returns>The loaded assembly if found in the bin path; otherwise, null.</returns>
    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        var name = new AssemblyName(args.Name);

        var dllPath = BinPath.PathAppend($"{name.Name}.dll");
        if (File.Exists(dllPath))
        {
            return Assembly.LoadFrom(dllPath);
        }

        var exePath = BinPath.PathAppend($"{name.Name}.exe");
        if (File.Exists(exePath))
        {
            return Assembly.LoadFrom(exePath);
        }

        return null;
    }
}
