using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using System.IO;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace PartyFinderTemplates.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    public MainWindow(Plugin plugin)
        : base("My Amazing Window##pluginMainWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Plugin = plugin;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.DrawOnConfig();
        //Toggle Plugin UI from xlplugins
        Services.PluginInterface.UiBuilder.OpenMainUi += Toggle;
    }

    public void Dispose() {
        Services.PluginInterface.UiBuilder.OpenMainUi -= Toggle;
    }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            this.Plugin.ConfigWindow.Toggle();
        }

        ImGui.Spacing();
    }

    public void DrawOnConfig()
    {
        if (Plugin.Configuration.MainWindowVisible)
            IsOpen = true;
    }

    public new bool DrawConditions()
    {
        return Plugin.Configuration.MainWindowVisible;
    }

    public new void OnClose()
    {
        Plugin.Configuration.MainWindowVisible = !Plugin.Configuration.MainWindowVisible;
        Plugin.Configuration.Save();
    }

    public new void Toggle()
    {
        Plugin.Configuration.MainWindowVisible = !Plugin.Configuration.MainWindowVisible;
        Plugin.Configuration.Save();
        IsOpen = !IsOpen;
    }
}
