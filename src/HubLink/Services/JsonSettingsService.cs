using System;
using System.IO;
using System.Text.Json;
using HubLink.Models;

namespace HubLink.Services;

public interface ISettingsService
{
    HubLinkSettings LoadSettings();
}

/// <summary>
/// Loads HubLink connection settings from appsettings.json.
/// </summary>
public sealed class JsonSettingsService : ISettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly string _settingsPath;

    public JsonSettingsService(string? settingsPath = null)
    {
        _settingsPath = settingsPath ?? Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    }

    public HubLinkSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return new HubLinkSettings();
            }

            var json = File.ReadAllText(_settingsPath);
            var root = JsonSerializer.Deserialize<AppSettingsRoot>(json, JsonOptions);
            return root?.HubLinkApi ?? new HubLinkSettings();
        }
        catch
        {
            return new HubLinkSettings();
        }
    }

    private sealed class AppSettingsRoot
    {
        public HubLinkSettings? HubLinkApi { get; set; }
    }
}
