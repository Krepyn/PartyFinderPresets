using Dalamud.Configuration;
using PartyFinderPresets.Classes;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PartyFinderPresets;


[Serializable]
public class Configuration : IPluginConfiguration
{
    private const int CURRENT_VERSION = 0;

    public int Version { get; set; } = 0;
    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
    public bool PresetsDockVisible { get; set; } = true;

    public static Configuration Load()
        => Services.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

    public void Save()
    {
        Services.PluginInterface.SavePluginConfig(this);
    }

}
