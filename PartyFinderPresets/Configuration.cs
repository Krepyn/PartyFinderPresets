using Dalamud.Configuration;
using System;
namespace PartyFinderPresets;


[Serializable]
public class Configuration : IPluginConfiguration
{
    private const int CURRENT_VERSION = 0;

    public int Version { get; set; } = 0;
    public bool IsConfigWindowMovable { get; set; } = true;
    public bool PresetsDockVisible { get; set; } = true;

    public static Configuration Load()
        => Services.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

    public void Save() => Services.PluginInterface.SavePluginConfig(this);
}
