using Microsoft.Extensions.Configuration;
using Suity.Editor.Services;
using System;

internal sealed class AppConfiguration : IAppConfig
{
    public static readonly AppConfiguration Instance = new();
    private readonly IConfiguration _configuration;

    private AppConfiguration()
    {
        // Build configuration source
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory) // Locate to the program's directory
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    }

    public string? GetSetting(string name)
    {
        // Similarly simple call
        return _configuration[name];
    }
}