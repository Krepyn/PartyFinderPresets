using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI;
using Dalamud.Interface.Utility.Raii;
using PartyFinderPresets.Utils;
using PartyFinderPresets.Enums;
using Dalamud.Game.Text.SeStringHandling;
using System.Runtime.InteropServices;
using Dalamud.Interface.Colors;

namespace PartyFinderPresets.Windows;

public sealed unsafe class DebugWindow : Window, IDisposable
{
    private readonly Plugin Plugin;
    public AtkValue[] AtkValues = null!;
    private string presetName = "";
    private string longValueS = "1";
    private long longValue = 0;
    //private string presetIndex = "0";
    private nint testStr;

    Vector4 OrangeText = new Vector4(1f, 0.57f, 0.21f, 1f);

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

    public override void Draw()
    {
        DrawPresetLibraryTesting();
        DrawRecruitmentTesting();
        DrawFormattingTesting();
        DrawNativeUITesting();
        DrawLiveData();
    }

    private void DrawPresetLibraryTesting() {
        using var node = ImRaii.TreeNode("Preset Library Stuff");
        if(!node) return;
        if(ImGui.Button("Save Preset Library")) {
            Plugin.RecruitmentDataController.Save();
        }
        ImGui.SameLine();
        if(ImGui.Button("Load Preset Library")) {
            Plugin.RecruitmentDataController.Load();
        }

        ImGui.InputText("##savepreset", ref presetName, 128);
        ImGui.SameLine();
        if(ImGui.Button("Save Preset")) {
            Plugin.RecruitmentDataController.SaveNewPreset(presetName);
            presetName = "";
        }

        if(ImGui.Button("Save Current Preset and Print")) {
            Plugin.RecruitmentDataController.SaveNewPresetAndPrint("");
        }

    }

    private void DrawFormattingTesting() {
        using var node = ImRaii.TreeNode("Formatting Stuff");
        if(!node) return;

        // Formatting
        var x = SelectedCategory.VandCDungeonFinder;

        using(ImguiUtils.NoItemSpacingX()) {
            ImGui.TextColored(OrangeText, "V&C Dungeon Finder: ");
            ImGui.SameLine();
            ImGui.Text($"{Helpers.GapsBeforeCapitals(x.ToString(), true)}");
        }

        // Enum Defined
#pragma warning disable RCS1257 // Use enum field explicitly || This is for testing
        x = (SelectedCategory)2;

        //using(ImRaii.PushColor(ImGuiCol.Text, new Vector4(247, 153, 26, 255))) {
        ImGui.TextColored(OrangeText, $"Can Enum Defined By Int:");
        ImGui.SameLine();
        ImGui.Text($"{ Enum.IsDefined(typeof(SelectedCategory), x)}");
        //}
#pragma warning restore RCS1257 // Use enum field explicitly

        // Long to ULong
        ImGui.TextColored(OrangeText, "Long to ulong:");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(50);
        ImGui.InputText("##longtoulong", ref longValueS, 30);
        longValue = long.Parse(longValueS);
        ImGui.SameLine();
        ImGui.Text(" | ");
        ImGui.SameLine();
        ImGui.Text($"{(ulong)longValue}");
    }

    private void DrawRecruitmentTesting() {
        using var node = ImRaii.TreeNode("Recruitment Window Stuff");
        if(!node) return;

        if(ImGui.Button("RecruitmentSub - Turn ilvl on")) {
            Plugin.RecruitmentDataController.CurrentAgent->AvgItemLvEnabled = 1;
        }

        if(ImGui.Button("RC Refresh #params (0,0)")) this.Plugin.GameFunctions.RCRefresh(0, 0);

        if(ImGui.Button("RC Refresh #params (0,1)")) this.Plugin.GameFunctions.RCRefresh(0, 1);
      
        if(ImGui.Button("Toggle Recruitment Update Hook")) this.Plugin.GameFunctions.ToggleUpdateHook();

        if(ImGui.Button("Toggle Recruitment Window Hook")) this.Plugin.GameFunctions.ToggleCriteriaWindowHook();
        
        if(ImGui.Button("AvgItemLv On")) this.Plugin.GameFunctions.AvgItemLv(true);

        if(ImGui.Button("AvgItemLv Off")) this.Plugin.GameFunctions.AvgItemLv(false);

        if(ImGui.Button("Refresh Test")) {
            var baseResNode = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition")->GetNodeById(18);
            var componentNode = (AtkComponentTextInput*)baseResNode->ChildNode->GetComponent();
            var textNode = componentNode->AtkTextNode->GetAsAtkTextNode();
            var textNodeStr = textNode->NodeText;

            var seStringB = new SeStringBuilder();
            seStringB.AddText("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            var seString = seStringB.BuiltString;
            var bytes = seString.EncodeWithNullTerminator();

            testStr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, testStr, bytes.Length);
            componentNode->SetText((byte*)testStr);

            Services.PluginLog.Verbose("refreshed");
        }
    }

    private void DrawNativeUITesting() {
        using var node = ImRaii.TreeNode("Native UI Stuff");
        if(!node) return;

        if(ImGui.Button("node traverse test")) {
            var baseResNode = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition")->GetNodeById(18);
            var componentNode = (AtkComponentTextInput*)baseResNode->ChildNode->GetComponent();
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


        if(AtkValues != null) {
            using(ImRaii.Table("AtkValues", AtkValues.Length)) {
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

        if(ImGui.Button("print str")) {
            var baseResNode = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition")->GetNodeById(18);
            var componentNode = (AtkComponentTextInput*)baseResNode->ChildNode->GetComponent();
            var textNode = componentNode->AtkTextNode->GetAsAtkTextNode();
            var textNodeStr = textNode->NodeText;

            Services.PluginLog.Verbose($"{textNodeStr}");
        }

        if(ImGui.Button("Send Chat Echo")) {
            Services.ChatGui.Print("aa");
        }
    }

    private void DrawLiveData() {
        using var node = ImRaii.TreeNode("Data");
        if(!node) return;

        ImGui.TextColored(OrangeText, "Selected Tab: ");
        ImGui.SameLine();
        ImGui.Text($"{(CategoryTab)Plugin.RecruitmentDataController.CurrentAgent->GroupTypeTab} ({Plugin.RecruitmentDataController.CurrentAgent->GroupTypeTab})");

        ImGui.TextColored(OrangeText, "Selected Category: ");
        ImGui.SameLine();
        ImGui.Text($"{(SelectedCategory)Plugin.RecruitmentDataController.CurrentData->SelectedCategory} ({Plugin.RecruitmentDataController.CurrentData->SelectedCategory})");

        ImGui.TextColored(OrangeText, "Duty ID: ");
        ImGui.SameLine();
        ImGui.Text($"{Plugin.RecruitmentDataController.CurrentData->SelectedDutyId}");

        ImGui.TextColored(OrangeText, "Duty Objective: ");
        ImGui.SameLine();
        ImGui.Text($"{Plugin.RecruitmentDataController.CurrentData->Objective} ({(byte)Plugin.RecruitmentDataController.CurrentData->Objective})");

        ImGui.TextColored(OrangeText, "Completion Status: ");
        ImGui.SameLine();
        ImGui.Text($"{Plugin.RecruitmentDataController.CurrentData->CompletionStatus} ({(byte)Plugin.RecruitmentDataController.CurrentData->CompletionStatus})");

        ImGui.TextColored(OrangeText, "Duty Finder Settings: ");
        ImGui.SameLine();
        ImGui.Text($"{Plugin.RecruitmentDataController.CurrentData->DutyFinderSettingFlags} ({(byte)Plugin.RecruitmentDataController.CurrentData->DutyFinderSettingFlags})");

        ImGui.TextColored(OrangeText, "Loot Rule ");
        ImGui.SameLine();
        ImGui.Text($"{Plugin.RecruitmentDataController.CurrentData->LootRule} ({(byte)Plugin.RecruitmentDataController.CurrentData->LootRule})");

        ImGui.TextColored(OrangeText, "Password: ");
        ImGui.SameLine();
        ImGui.Text($"{Plugin.RecruitmentDataController.CurrentData->Password}");

        ImGui.TextColored(OrangeText, "Language Flags: ");
        ImGui.SameLine();
        ImGui.Text($"{Plugin.RecruitmentDataController.CurrentData->LanguageFlags} ({(byte)Plugin.RecruitmentDataController.CurrentData->LanguageFlags})");

        ImGui.TextColored(OrangeText, "Average Item Level Enabled: ");
        ImGui.SameLine();
        ImGui.Text($"{Plugin.RecruitmentDataController.CurrentAgent->AvgItemLvEnabled}");

        ImGui.TextColored(OrangeText, "Average Item Level: ");
        ImGui.SameLine();
        ImGui.Text($"{Plugin.RecruitmentDataController.CurrentAgent->AvgItemLv}");


    }
}
