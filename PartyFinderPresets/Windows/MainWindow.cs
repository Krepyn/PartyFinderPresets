using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Interface.Utility.Raii;
using static Dalamud.Interface.Utility.Raii.ImRaii;
using PartyFinderPresets.Controllers;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Interface;

namespace PartyFinderPresets.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private readonly Plugin Plugin;
    private readonly RecruitmentDataController RecruitmentDataController;
    public bool isCollapsed;
    private Vector2 windowPos;
    private int selectedIndex;
    private string presetName = "";

    public MainWindow(Plugin plugin)
        : base("Presets##PFPDock", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoNav | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking)
    {
        this.Plugin = plugin;
        this.RecruitmentDataController = plugin.RecruitmentDataController;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(180, 400),
            MaximumSize = new Vector2(180, 400)
        };

        // Thank you Kami
        TitleBarButtons.Add(new TitleBarButton {
            Click = _ => this.Plugin.ConfigWindow.Toggle(),
            Icon = FontAwesomeIcon.Cog,
            ShowTooltip = () => ImGui.SetTooltip("Open Configuration"),
            IconOffset = new Vector2(2.0f, 1.0f),
        });

        CollapsedCondition = ImGuiCond.FirstUseEver;

        //Toggle Plugin UI from xlplugins
        Services.PluginInterface.UiBuilder.OpenMainUi += Toggle;
    }

    public void Dispose() {
        Services.PluginInterface.UiBuilder.OpenMainUi -= Toggle;
    }

    public override unsafe void Update()
    {
        var lfgc = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition")->RootNode;
        var positionX = lfgc->X;
        var positionY = lfgc->Y;
        var sizeH = lfgc->Height;
        var sizeW = lfgc->Width;
        var scale = new Vector2(lfgc->ScaleX, lfgc->ScaleY);
        windowPos = new Vector2(positionX + (sizeW * scale.X) + 7, positionY + 7);

        if (isCollapsed) ImGui.SetNextWindowPos(windowPos, ImGuiCond.Always);
    }

    public override Boolean DrawConditions()
    {
        return Plugin.Configuration.PresetsDockVisible && Plugin.GameFunctions.LastRefreshCondition == 0;
    }
    public override void PreDraw() {
        isCollapsed = true;
    }

    public override void Draw()
    {
        isCollapsed = false;

        DrawPresetList();

        ImguiUtils.NoFrameRounding();
        ImguiUtils.NoItemSpacingX();
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(10, ImGui.GetStyle().FramePadding.Y));

#if DEBUG
        // Print Button
        if (ImGui.Button("Print##print"))
        {
            if(selectedIndex >= 0) RecruitmentDataController.GetPreset(selectedIndex)?.PrintData();
        }
        ImGui.SameLine();
#endif

        // Save Button + Save Popup
        if (ImGui.Button("Save##save"))
        {
            ImGui.OpenPopup("Preset Save");
        }

        DrawSavePopup();

        // Load Button
        if(ImGui.Button("Load##load")) {
            if(selectedIndex >= 0) {
                RecruitmentDataController.LoadPreset(selectedIndex);
            }
        }
        ImGui.SameLine();
        // Update Button
        var ctrl = !ImGui.GetIO().KeyCtrl;
        using (ImRaii.Disabled(ctrl))
        {
            if (ImGui.Button("Update##update"))
            {
                if(selectedIndex >= 0) {
                    RecruitmentDataController.UpdatePreset(selectedIndex);
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
        using (ImRaii.Disabled(ctrlShift)) {
            if (ImGui.Button("Delete##delete")) {
                if(selectedIndex >= 0) {
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
        ImGui.PopStyleVar(3);

        ImGui.SetWindowPos(windowPos, ImGuiCond.Always); // Dock
    }
    
    public void DrawPresetList()
    {
        using var child = ImRaii.Child("Presets", new Vector2(ImGui.GetContentRegionAvail().X, ImGui.GetContentRegionAvail().Y * 0.85f), false);

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
                DrawRenameContextPopupItem(presetIndex);
                if (switchTo != null && switchTo >= 0) selectedIndex = switchTo.Value;

                presetIndex++;
            }
        }
        child.Dispose();
    }

    private void DrawSavePopup() {
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 5);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));

        using var popup = ImRaii.Popup("Preset Save", ImGuiWindowFlags.NoMove);
        if(popup) {
            ImGui.SetNextItemWidth(150);
            ImGui.InputText("", ref presetName, 128);
            ImGui.SameLine();
            if(ImGui.Button("Save")) {
                Plugin.RecruitmentDataController.SaveNewPreset(presetName);
                presetName = "";
            }
        }

        ImGui.PopStyleVar(2);
    }

    private void DrawRenameContextPopupItem(int presetIndex) {
        ImguiUtils.NoFrameRounding();
        ImguiUtils.NoItemSpacingX();
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 5);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4));

        using var popup = ImRaii.ContextPopupItem($"Preset Rename##{presetIndex}");
        if(popup) {


            ImGui.SetNextItemWidth(150);
            ImGui.InputText("", ref presetName, 128);
            ImGui.SameLine();
            if(ImGui.Button("Rename")) {
                Plugin.RecruitmentDataController.RenamePreset(presetIndex, presetName);
                presetName = "";
                
                ImGui.CloseCurrentPopup();
            }
        }

        ImGui.PopStyleVar(4);
    }

}
