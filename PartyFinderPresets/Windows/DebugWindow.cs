using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI;
using PartyFinderPresets.Controllers;
using PartyFinderPresets.Classes;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;
using System.Xml.Linq;

namespace PartyFinderPresets.Windows;

public sealed class DebugWindow : Window, IDisposable
{
    private readonly Plugin Plugin;
    public AtkValue[] AtkValues = null!;
    private string presetName = "";

    public DebugWindow(Plugin plugin)
        : base("Debug##pluginDebugWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Plugin = plugin;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() {
    }

    public unsafe override void Draw()
    {  
        if (ImGui.Button("Get Current Recruitment Data"))
        {
            Plugin.RecruitmentDataController.SaveNewPresetAndPrint("");
        }
        if (ImGui.Button("Save Current Data as Json"))       
        {
            Plugin.RecruitmentDataController.SavePresetList();
        }
        if (ImGui.Button("RecruitmentSub - Turn ilvl on"))
        {
            *(Plugin.RecruitmentDataController.CurrentData.AvgItemLvEnabled) = 1;
        }


        ImGui.InputText($"{presetName}", ref presetName, 128);
        ImGui.SameLine();
        if (ImGui.Button("Save Preset"))
        {
            Plugin.RecruitmentDataController.SaveNewPreset(presetName);
        }

        ImGui.Spacing();
        ImGui.Text("---Old---");
        ImGui.Spacing();

        if (ImGui.Button("RC Refresh #params (0,0)"))
        {
            this.Plugin.GameFunctions.RCRefresh(0, 0);
        }
        if (ImGui.Button("Toggle Recruitment Update Hook"))
        {
            this.Plugin.GameFunctions.ToggleUpdateHook();
        }
        if (ImGui.Button("Toggle Recruitment Window Hook"))
        {
            this.Plugin.GameFunctions.ToggleCriteriaWindowHook();
        }
        if (ImGui.Button("AvgItemLv On"))
        {
            this.Plugin.GameFunctions.AvgItemLvOn();
        }
        
        if (ImGui.Button("AvgItemLv Off"))
        {
            this.Plugin.GameFunctions.AvgItemLvOff();
        }

        if (AtkValues != null)
        {
            using (ImRaii.Table("AtkValues", AtkValues.Length))
            {
                ImGui.TableSetupColumn("Func", ImGuiTableColumnFlags.WidthFixed, 40);
                ImGui.TableSetupColumn("Int", ImGuiTableColumnFlags.WidthFixed, 100);
                ImGui.TableSetupColumn("Bool", ImGuiTableColumnFlags.WidthFixed, 40);


                ImGui.TableNextColumn();
                ImGui.Text($"{AtkValues[0].Int}");
                ImGui.TableNextColumn();
                ImGui.Text($"{AtkValues[1].UInt}");
                ImGui.TableNextColumn();
                ImGui.Text($"{AtkValues[2].Bool}");
            }
        }

        //onRequestedUpdate Test on addon
        //if (ImGui.Button("Refresh Test"))
        //{
        //    var x = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition");
        //    x->OnRequestedUpdate(&AtkStage.Instance()->AtkArrayDataHolder->NumberArrays[71], &AtkStage.Instance()->AtkArrayDataHolder->StringArrays[66]);
        //    Services.PluginLog.Verbose($"{x->NameString}");
        //}
    }
}
