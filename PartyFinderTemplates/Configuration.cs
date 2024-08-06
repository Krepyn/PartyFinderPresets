using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace PartyFinderTemplates;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;
    public bool MainWindowVisible { get; set; } = false;

    public static Configuration Load()
        => Services.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

    public void Save()
    {
        Services.PluginInterface.SavePluginConfig(this);
    }
}
