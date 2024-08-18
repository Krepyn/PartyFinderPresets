using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using System.IO;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Interface.Utility.Raii;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using static FFXIVClientStructs.FFXIV.Client.UI.RaptureAtkHistory.Delegates;
using Dalamud.Interface;
using PartyFinderPresets.Controllers;

namespace PartyFinderPresets.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private readonly Plugin Plugin;
    private readonly RecruitmentDataController RecruitmentDataController;
    public bool isCollapsed;
    private Vector2 windowPos;
    private int selectedIndex;
    private bool deleteConfirm;

    public MainWindow(Plugin plugin)
        : base("Presets##PFPDockWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking)
    {
        this.Plugin = plugin;
        this.RecruitmentDataController = plugin.RecruitmentDataController;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(180, 330),
            MaximumSize = new Vector2(180, 330)
        };

        CollapsedCondition = ImGuiCond.FirstUseEver;

        //Toggle Plugin UI from xlplugins
        Services.PluginInterface.UiBuilder.OpenMainUi += Toggle;
    }

    public void Dispose() {
        Services.PluginInterface.UiBuilder.OpenMainUi -= Toggle;
    }

    public unsafe override void Update()
    {
        var lfgc = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition")->RootNode;
        var positionX = lfgc->X;
        var positionY = lfgc->Y;
        var sizeH = lfgc->Height;
        var sizeW = lfgc->Width;
        var scale = new Vector2(lfgc->ScaleX, lfgc->ScaleY);
        windowPos = new Vector2(positionX + sizeW * scale.X + 7, positionY + 7);

        if (isCollapsed) ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always);

        //get first frame
    }

    public override Boolean DrawConditions()
    {
        return Plugin.Configuration.PresetsDockVisible;
    }

    public override void Draw()
    {
        isCollapsed = false;

        // Code for auto resize
        //     ImGui.SetNextWindowSizeConstraints(new Vector2(300f, ImGui.GetTextLineHeightWithSpacing() * 1), new Vector2(300f, ImGui.GetTextLineHeightWithSpacing() * 5));
        //     Inside child => new Vector2(300.0f * ImGuiHelpers.GlobalScale, Plugin.RecruitmentDataController.GetPresetCount() * ImGui.GetFrameHeightWithSpacing() + ImGui.GetFrameHeight())


        DrawPresetList();

#if DEBUG
        // Print Button
        if (ImGui.Button($"Print##print"))
        {
            if(selectedIndex >= 0) RecruitmentDataController.GetPreset(selectedIndex)?.PrintData();
        }
        ImGui.SameLine();
#endif
        // Load Button
        if(ImGui.Button($"Load##load")) {
            if(selectedIndex >= 0) {
                RecruitmentDataController.LoadPreset(selectedIndex);
            }
        }

        // Update Button
        var ctrl = !ImGui.GetIO().KeyCtrl;
        using (ImRaii.Disabled(ctrl))
        {
            if (ImGui.Button($"Update##update"))
            {
                if(selectedIndex >= 0)
                {
                    var preset = RecruitmentDataController.GetPreset(selectedIndex);
                    preset?.MakePresetFromCurrentData(preset.Name);
                }
            }
        }
        if (ctrl && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted("Hold Ctrl to update.");
            ImGui.EndTooltip();
        }

        // Delete Button
        ImGui.SameLine();
        var ctrlShift = !(ImGui.GetIO().KeyCtrl && ImGui.GetIO().KeyShift);
        using (ImRaii.Disabled(ctrlShift))
        {
            if (ImGui.Button($"Delete##delete"))
            {
                if(selectedIndex >= 0)
                {
                    RecruitmentDataController.DeletePreset(selectedIndex);
                }
            }
        }
        if (ctrlShift && ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted("Hold Ctrl + Shift to delete.");
            ImGui.EndTooltip();
        }

        ImGui.SetWindowPos(windowPos, ImGuiCond.Always); // Dock
    }

    public void DrawPresetList()
    {
        using var child = ImRaii.Child("Presets", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y * 0.90f), false);

        if (child)
        {
            ImGui.AlignTextToFramePadding();
            var presetIndex = 0;
            foreach (var preset in RecruitmentDataController.RecruitmentPresets)
            {
                int? switchTo = null;
                if (ImGui.Selectable($"{preset.Name}##{presetIndex}", this.selectedIndex == presetIndex))
                {
                    switchTo = presetIndex;
                }

                //if (this.selectedIndex == presetIndex)
                //{
                //    ImGui.SetScrollHereY(1f);
                //}                

                if (switchTo != null && switchTo >= 0) selectedIndex = switchTo.Value;

                presetIndex++;
            }
        }
        child.Dispose();
    }

    public void DrawButtons()
    {
        return;
    }

    public override void PreDraw()
    {
        isCollapsed = true;
    }
}
