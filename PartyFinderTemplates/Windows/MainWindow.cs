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
    //private string GoatImagePath = Path.Combine(Services.PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

    // We give this window a hidden ID using ##
    // So that the user will see "My Amazing Window" as window title,
    // but for ImGui the ID is "My Amazing Window##With a hidden ID"
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
/*
        AddonStuff();
        if (LookingForGroupCondition != null)
        {
            ImGui.Text($"{this.LookingForGroupCondition->AtkValues[44].Int}");
            ImGui.Text($"{this.LookingForGroupCondition->AtkValues[249].Int}");
            ImGui.Text($"{this.LookingForGroupCondition->AtkValues[260].Int}");
        }*/
    }

    public void DrawOnConfig()
    {
        if (Plugin.Configuration.MainWindowVisible)
            IsOpen = true;
    }
    /*
        private void AddonStuff()
        {
            LookingForGroupCondition = (AtkUnitBase*)Services.GameGui.GetAddonByName("LookingForGroupCondition").ToPointer();
            if (LookingForGroupCondition != null)
            {
                return;
            }
        }*/

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
