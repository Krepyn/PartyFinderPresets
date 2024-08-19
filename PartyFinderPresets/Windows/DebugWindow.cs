using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.UI;
using PartyFinderPresets.Structs;
using PartyFinderPresets.Utils;
using Dalamud.Game.Text.SeStringHandling;
using System.Runtime.InteropServices;

namespace PartyFinderPresets.Windows;

public unsafe sealed class DebugWindow : Window, IDisposable
{
    private readonly Plugin Plugin;
    public AtkValue[] AtkValues = null!;
    private string presetName = "";
    private string longValueS = "1";
    private long longValue = 0;
    //private string presetIndex = "0";
    private nint testStr;

    public DebugWindow(Plugin plugin)
        : base("Debug##PFPDebugWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize)
    {
        this.Plugin = plugin;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() {
        Marshal.FreeHGlobal((nint)testStr);
    }

    public unsafe override void Draw()
    {
        if (ImGui.Button("Save Preset Library"))
        {
            Plugin.RecruitmentDataController.Save();
        }
        if (ImGui.Button("Load Preset Library"))
        {
            Plugin.RecruitmentDataController.Load();
        }

        var x = SelectedCategory.VandCDungeonFinder;        
        ImGui.Text($"{Helpers.GapsBeforeCapitals(x.ToString(), true)}");

        x = (SelectedCategory)2;
        ImGui.Text($"{Enum.IsDefined(typeof(SelectedCategory), x)}");

        ImGui.InputText($"##longtoulong", ref longValueS, 128);
        longValue = long.Parse(longValueS);
        ImGui.SameLine();
        ImGui.Text($" | ");
        ImGui.SameLine();
        ImGui.Text($"{(ulong)longValue}");

        if(ImGui.Button("node traverse test")) {
            var baseResNode = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition")->GetNodeById(18);
            var componentNode = (AtkComponentTextInput*) baseResNode->ChildNode->GetComponent();
            var textNode = componentNode->AtkTextNode->NextSiblingNode->GetAsAtkTextNode();
            Services.PluginLog.Verbose($"{textNode->NodeText}");
            //testStr = (byte*)Marshal.AllocHGlobal(128);

            var str = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

            // Utf8String aastr = new Utf8String();
            // var input = new Utf8String(str);
            // var output = new Utf8String();
            // input.Copy(PronounModule.Instance()->ProcessString(&input, true));
            // output.Copy(PronounModule.Instance()->ProcessString(&input, false));
            // return ouput.AsSpan().ToArray();           

        }

        //        var seStringDrawParams = new SeStringDrawParams();
        //        //seStringDrawParams.TargetDrawList = null;
        //        seStringDrawParams.WrapWidth = 408;
        //        seStringDrawParams.FontSize = 18;
        //        ImGui.PushFont(UiBuilder.DefaultFont);
        //        var stringi = "iiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii";
        //        var stringa = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        //        var stringa2 = "OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOii";
        //#pragma warning disable SeStringRenderer
        //        var seString = ImGuiHelpers.CompileSeStringWrapped(stringi, seStringDrawParams);
        //        var seString2 = ImGuiHelpers.CompileSeStringWrapped(stringa, seStringDrawParams);
        //        var seString3 = ImGuiHelpers.CompileSeStringWrapped(stringa2, seStringDrawParams);
        //        ImGui.PopFont();

        ImGui.InputText($"##savepreset", ref presetName, 128);
        ImGui.SameLine();
        if (ImGui.Button("Save Preset"))
        {
            Plugin.RecruitmentDataController.SaveNewPreset(presetName);
            presetName = "";
        }

        if (ImGui.Button("RecruitmentSub - Turn ilvl on"))
        {
            *(Plugin.RecruitmentDataController.CurrentData.AvgItemLvEnabled) = 1;
        }

        ImGui.Spacing();
        ImGui.Text("---Old---");
        ImGui.Spacing();

        if(ImGui.Button("Save Current Preset and Print")) {
            Plugin.RecruitmentDataController.SaveNewPresetAndPrint("");
        }
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
            this.Plugin.GameFunctions.AvgItemLv(true);
        }
        
        if (ImGui.Button("AvgItemLv Off"))
        {
            this.Plugin.GameFunctions.AvgItemLv(false);
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

        if (ImGui.Button("Refresh Test"))
        {
            var baseResNode = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition")->GetNodeById(18);
            var componentNode = (AtkComponentTextInput*)baseResNode->ChildNode->GetComponent();
            var textNode = componentNode->AtkTextNode->GetAsAtkTextNode();
            var textNodeStr = textNode->NodeText;

            SeStringBuilder seStringB = new SeStringBuilder();
            seStringB.AddText("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            SeString seString = seStringB.BuiltString;
            byte[] bytes = seString.EncodeWithNullTerminator();

            testStr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, testStr, bytes.Length);
            componentNode->SetText((byte*)testStr);

            Services.PluginLog.Verbose($"refreshed");
        }

        if(ImGui.Button("print str")) {
            var baseResNode = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition")->GetNodeById(18);
            var componentNode = (AtkComponentTextInput*)baseResNode->ChildNode->GetComponent();
            var textNode = componentNode->AtkTextNode->GetAsAtkTextNode();
            var textNodeStr = textNode->NodeText;

            Services.PluginLog.Verbose($"{textNodeStr}");
        }

    }
}
