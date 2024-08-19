using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace PartyFinderPresets.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Configuration Configuration;
    private readonly Plugin Plugin;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("PartyFinderPresets Config###PFPConfigWindow")
    {
        this.Plugin = plugin;

        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        //SizeCondition = ImGuiCond.Always;

        Configuration = Plugin.Configuration;

        Services.PluginInterface.UiBuilder.OpenConfigUi += Toggle;
    }

    public void Dispose()
    {
        Services.PluginInterface.UiBuilder.OpenConfigUi -= Toggle;
    }

    public override void PreDraw()
    {
        if (Configuration.IsConfigWindowMovable)
            Flags &= ~ImGuiWindowFlags.NoMove;
        else
            Flags |= ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        var movable = Configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            Configuration.IsConfigWindowMovable = movable;
            Configuration.Save();
        }

        var presetsDockVisible = Configuration.PresetsDockVisible;
        if (ImGui.Checkbox("Main Window Visible", ref presetsDockVisible))
        {
            Configuration.PresetsDockVisible = presetsDockVisible;
            Configuration.Save();
        }
    }
}
