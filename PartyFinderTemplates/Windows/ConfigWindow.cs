using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace PartyFinderTemplates.Windows;

public sealed class ConfigWindow : Window, IDisposable
{
    private readonly Configuration Configuration;
    private readonly Plugin Plugin;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("A Wonderful Configuration Window###PluginConfigWindow")
    {
        this.Plugin = plugin;

        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        //SizeCondition = ImGuiCond.Always;

        Configuration = Plugin.Configuration;

        //Open Config Menu from xlplugins
        Services.PluginInterface.UiBuilder.OpenConfigUi += Toggle;
    }

    public void Dispose()
    {
        Services.PluginInterface.UiBuilder.OpenConfigUi -= Toggle;
    }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (Configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        // can't ref a property, so use a local copy
        var configValue = Configuration.SomePropertyToBeSavedAndWithADefault;
        if (ImGui.Checkbox("Random Config Bool", ref configValue))
        {
            Configuration.SomePropertyToBeSavedAndWithADefault = configValue;
            // can save immediately on change, if you don't want to provide a "Save and Close" button
            Configuration.Save();
        }

        var movable = Configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            Configuration.IsConfigWindowMovable = movable;
            Configuration.Save();
        }

        var mainWindowVisible = Configuration.MainWindowVisible;
        if (ImGui.Checkbox("Main Window Visible", ref mainWindowVisible))
        {
            Plugin.UI.Toggle();
        }
    }
}
