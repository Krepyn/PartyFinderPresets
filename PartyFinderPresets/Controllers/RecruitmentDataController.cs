using Newtonsoft.Json;
using PartyFinderPresets.Classes;
//using PartyFinderPresets.Structs;
using PartyFinderPresets.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lumina.Excel.Sheets;
using Newtonsoft.Json.Converters;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using PartyFinderPresets.Utils;
using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using Dalamud.Plugin.Services;
namespace PartyFinderPresets.Controllers;

public unsafe class RecruitmentDataController : IDisposable
{
    public Plugin Plugin;
    public List<RecruitmentData> RecruitmentPresets = [];
    public AgentLookingForGroup* CurrentAgent;
    public AgentLookingForGroup.RecruitmentSub* CurrentData;
    private readonly string fileName = Path.Combine(Services.PluginInterface.ConfigDirectory.FullName, "PresetsLibrary.json");
    private nint testStr;

    public RecruitmentDataController(Plugin plugin) {
        this.Plugin = plugin;
        CurrentAgent = AgentLookingForGroup.Instance();
        CurrentData = &CurrentAgent->StoredRecruitmentInfo;
        testStr = Marshal.AllocHGlobal(196);

        Load();
    }

    public void Dispose() {
        Marshal.FreeHGlobal(testStr);
    }

    public void Load() {
        if (!File.Exists(fileName)) {
            Services.PluginLog.Verbose("Couldn't find presets json.");
            return;
        } try {
            RecruitmentPresets = JsonConvert.DeserializeObject<List<RecruitmentData>>(File.ReadAllText(fileName))!;
        } catch(JsonReaderException e) {
            Services.PluginLog.Error($"Couldn't read presets json, {e}.");
        }

        RecruitmentPresets ??= [];
    }

    public void Save() {
        if(RecruitmentPresets == null) return;
        File.WriteAllText(fileName, JsonConvert.SerializeObject(RecruitmentPresets, Formatting.Indented, [new StringEnumConverter()]));
        Services.PluginLog.Verbose("PresetLibrary Saved.");
        //File.WriteAllText(FileName, JsonConvert.SerializeObject(RecruitmentPresets, Formatting.Indented));
    }

    public RecruitmentData? GetPreset(int index) {
        if (RecruitmentPresets.Count == 0) return null;
        return RecruitmentPresets[index];
    }

    public void DeletePreset(int index) {
        RecruitmentPresets.RemoveAt(index);
        this.Save();
    }

    public void SaveNewPreset(string Name) {
        RecruitmentPresets.Add(new RecruitmentData(Name));
        this.Save();
    }

    public void UpdatePreset(int index) {
        var preset = RecruitmentPresets[index];
        preset.MakePresetFromCurrentData(preset.Name);
        this.Save();

        Services.PluginLog.Verbose($"Preset {preset.Name} ({index}) has been updated.");
    }

    public void UpdatePreset(int index, RecruitmentData updatedPreset) {
        RecruitmentPresets[index] = updatedPreset;
        this.Save();

        Services.PluginLog.Verbose($"Preset {RecruitmentPresets[index].Name} ({index}) has been updated.");
    }

    public void RenamePreset(int index, string presetName) {
        var preset = RecruitmentPresets[index];
        preset.Name = presetName;
        this.Save();
    }

    public int GetPresetCount() {
        if (RecruitmentPresets == null) return 0;
        return RecruitmentPresets.Count;
    }

    private void ErrorMessage(string message) {
        Services.PluginLog.Verbose(message);
        Services.ChatGui.Print(message);
    }

    // TODO:    Test the preset update and errors when faulty preset
    //          Colored Error Messages
    public void LoadPreset(int index) {
        var presetToLoad = RecruitmentPresets[index];
        Services.PluginLog.Info($"--- Preset: \"{presetToLoad.Name}\" is loading. ---");

        var categoryTab = presetToLoad.CategoryTab;
        if(!(DutyIdIsValid(ref presetToLoad.recruitmentSub.SelectedCategory, ref categoryTab, presetToLoad.recruitmentSub.SelectedDutyId))) {
            presetToLoad.recruitmentSub.SelectedDutyId = 0;
            presetToLoad.recruitmentSub.SelectedCategory = 0;
            UpdatePreset(index, presetToLoad);
            ErrorMessage("Error while loading the Duty ID, please select the correct duty again and update the preset.");
            // Services.PluginLog.Verbose($"{DutyIdIsValid(ref presetToLoad.recruitmentSub.SelectedCategory, ref categoryTab, presetToLoad.recruitmentSub.SelectedDutyId)}");
        }

        var objective = presetToLoad.recruitmentSub.Objective;
        if(!(Enum.IsDefined(objective))) {
            presetToLoad.recruitmentSub.Objective = AgentLookingForGroup.Objective.None;
            UpdatePreset(index, presetToLoad);
            ErrorMessage("Error while loading the Duty Objective, please select the correct objective again and update the preset.");
        }

        var completionStatus = presetToLoad.recruitmentSub.CompletionStatus;
        if(!Enum.IsDefined(completionStatus) && (byte)completionStatus != 1) {
            presetToLoad.recruitmentSub.CompletionStatus = (AgentLookingForGroup.CompletionStatus)1;
            UpdatePreset(index, presetToLoad);
            ErrorMessage("Error while loading the Completion Status, please select the correct Completion Status again and update the preset.");
        }

        var dutyFinderSettings = presetToLoad.recruitmentSub.DutyFinderSettingFlags;
        if(!(Enum.IsDefined(typeof(AgentLookingForGroup.DutyFinderSetting), dutyFinderSettings))) { // 00000111 
            presetToLoad.recruitmentSub.DutyFinderSettingFlags = AgentLookingForGroup.DutyFinderSetting.None;
            UpdatePreset(index, presetToLoad);
            ErrorMessage("Error while loading the Duty Finder Settings, please select the correct Duty Finder Settings(Unrestricted Party, Minimum IL, Silence Echo) again and update the preset.");
        }

        var lootRule = presetToLoad.recruitmentSub.LootRule;
        if(!Enum.IsDefined(lootRule)) { // 00000010
            presetToLoad.recruitmentSub.LootRule = AgentLookingForGroup.LootRule.Normal;
            UpdatePreset(index, presetToLoad);
            ErrorMessage("Error while loading the Loot Rule, please select the correct Loot Rule again and update the preset.");
        }

        //var password = UInt16.Parse(listToLoad.Password);
        var password = presetToLoad.recruitmentSub.Password;
        if(password > 10000) {
            presetToLoad.recruitmentSub.Password = 10000;
            UpdatePreset(index, presetToLoad);
            ErrorMessage("Error while loading the password, please set the password again and update the preset.");
        }

        var language = presetToLoad.recruitmentSub.LanguageFlags;
        if((byte)language > 15) { // 00001111
            presetToLoad.recruitmentSub.LanguageFlags = (AgentLookingForGroup.Language)15;
            UpdatePreset(index, presetToLoad);
            ErrorMessage("Error while loading the Language, please set the correct Language(s) again and update the preset.");
        }

        CurrentAgent->AvgItemLvEnabled = presetToLoad.AvgItemLvEnabled;
        if(presetToLoad.AvgItemLvEnabled == 1)
            CurrentAgent->AvgItemLv = presetToLoad.AvgItemLv;
        CurrentAgent->GroupTypeTab = (byte)categoryTab;
        // Services.PluginLog.Verbose($"Current category tab {categoryTab}");

        *CurrentData = presetToLoad.recruitmentSub;

        // TODO add slot shifting depending on current party members
        var slotFlags = presetToLoad.SlotFlags;
        var numberOfGroups = (presetToLoad.recruitmentSub.NumberOfGroups <= 6 && presetToLoad.recruitmentSub.NumberOfGroups > 0) ? presetToLoad.recruitmentSub.NumberOfGroups : 1;
        // Services.PluginLog.Verbose($"Number of Groups: {numberOfGroups}");
        for(var i = 1; i < 8 * numberOfGroups; i++) { // shifts the "Omit" slots to the right like how to game handles it by default
            if((ulong)slotFlags[i] % 2 == 1) slotFlags[i]--;
            if((ulong)slotFlags[i] > (ulong)0xFFFFFFFE) { // 0xFFFFFFFE = All roles selected
                Services.PluginLog.Verbose($"Slot {i + 1} is out of scope: {(ulong)slotFlags[i]}, {(long)slotFlags[i]}.");
                slotFlags[i] = 0;
            }
            if((ulong) slotFlags[i] == 0) shiftSlotsInCurrentParty(ref slotFlags, i);
            (*CurrentData).SlotFlags[i] = (ulong)slotFlags[i];
            // Services.PluginLog.Verbose($"Slot {i+1} has been loaded.");
        }

        var commentString = presetToLoad.SeStrComment;
        var valid = isCommentValid(commentString);
        Services.PluginLog.Info($"Is comment valid: {valid}");
        //if(valid)
        //    Marshal.Copy(commentString, 0, (nint)(*CurrentData).Comment, 196);
        //else
        //    Services.PluginLog.Info("Comment is longer than it is allowed.");
        this.Plugin.GameFunctions.RCRefresh(0, 0);

        Services.PluginLog.Info($"--- Preset: \"{presetToLoad.Name}\" has been loaded. ---");
    }

    // Returns true if the AtkTextInputComponent wrapped comment is the same as non-wrapped comment
    // This should always return true except if json was manually edited
    public bool isCommentValid(byte[] bytes) {
        var baseResNode = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition")->GetNodeById(18);
        var componentNode = (AtkComponentTextInput*)baseResNode->ChildNode->GetComponent();
        var textNode = componentNode->AtkTextNode->GetAsAtkTextNode(); // Actual Text Node
        // var textNode2 = componentNode->AtkTextNode->NextSiblingNode->GetAsAtkTextNode(); // Line Count Node

        // SetText on TextInputComponent to wrap it inside the textinput, so we can get the how many lines it takes value
        Marshal.Copy(bytes, 0, testStr, bytes.Length);
        componentNode->SetText((byte*)testStr);
        var aa = componentNode->UnkText1;
        Services.PluginLog.Info($"UnkText1: {aa.ToString()} .");
        textNode->SetText((byte*)testStr);
        var bb = textNode->NodeText.ToString().Replace("\u0002\u0010\u0001\u0003", "");
        Services.PluginLog.Info($"NodeText: {bb.ToString()} .");

        return aa.ToString() == bb;

        // These comments are here in case I wanna try cutting the comments instead of zero-ing them but probably not :3
        //Services.PluginLog.Verbose($"{textNode->NodeText.ToString()}");
        //Services.PluginLog.Verbose($"{textNode2->NodeText.ToString()}");
        //var nodeText = textNode->NodeText;
        //var bytesBefore = bytes.ToArray();
        //Services.PluginLog.Verbose($"bytes before: {Helpers.ByteArrayToString(bytes)}");
        //Marshal.Copy((nint)nodeText.StringPtr, bytes, 0, 196); // Copy the new node text into comment, since this ensures comment fits into the textinput
        //Services.PluginLog.Verbose($"bytes after: {Helpers.ByteArrayToString(bytes)}");
        //if(!bytesBefore.SequenceEqual(bytes))
        //    Services.PluginLog.Verbose($"Comment is bigger than it is allowed.");
    }

    public static bool DutyIdIsValid(ref ushort selectedCategory, ref CategoryTab categoryTab, ushort dutyId) {
        if(!Enum.IsDefined(typeof(SelectedCategory), selectedCategory)) {
            Services.PluginLog.Verbose($"Selected Category was wrong. ({(ushort)selectedCategory})");
            return false;
        }

        if((SelectedCategory)selectedCategory == SelectedCategory.None)
            return true;

        SelectedCategory[] LuminaDuties = [SelectedCategory.Dungeons, SelectedCategory.Guildhests, SelectedCategory.Trials, SelectedCategory.Raids,
                                           SelectedCategory.HighendDuty, SelectedCategory.Pvp, SelectedCategory.FieldOperations, SelectedCategory.VandCDungeonFinder];

        if(LuminaDuties.Contains<SelectedCategory>((SelectedCategory)selectedCategory)) {
            var duty = findCondition(dutyId);
            if(duty == null) return false;
            selectedCategory = (ushort)findDutyCategory((ContentFinderCondition)duty);

            // Chaotic Cloud of Darkness(Id = 1010) can be queued as either Normal or Alliance with the same ID soooo...
            if(dutyId == 1010) {
                if(categoryTab == CategoryTab.CustomMatch)
                    categoryTab = CategoryTab.Normal;
            } else {
                categoryTab = CategoryTab.Normal;
            }

            if((SelectedCategory)selectedCategory == SelectedCategory.Raids || (SelectedCategory)selectedCategory == SelectedCategory.Pvp || (SelectedCategory)selectedCategory == SelectedCategory.FieldOperations)
                categoryTab = findDutyCategoryTab((ContentFinderCondition)duty, (SelectedCategory)selectedCategory, categoryTab);
            return true;
        } 

        if((SelectedCategory)selectedCategory == SelectedCategory.TreasureHunt)
            return dutyId <= 23;

        if((SelectedCategory)selectedCategory == SelectedCategory.Fates) {
            if(dutyId == 0) return true;
            else return validFateTerritoryType(dutyId);
        }

        if((SelectedCategory)selectedCategory == SelectedCategory.TheHunt)
            return true;

        if((SelectedCategory)selectedCategory == SelectedCategory.DeepDungeons)
            return dutyId > 0 && dutyId <= 3;

        if((SelectedCategory)selectedCategory == SelectedCategory.DutyRoulette)
            return Enum.IsDefined(typeof(DutyRouletteType), dutyId);

        if((SelectedCategory)selectedCategory == SelectedCategory.GoldSaucer)
            return Enum.IsDefined(typeof(GoldSaucerType), dutyId);

        if((SelectedCategory)selectedCategory == SelectedCategory.GatheringForays) {
            return Enum.IsDefined(typeof(GatheringForayType), dutyId);
        }

        Services.PluginLog.Verbose($"Selected Category was wrong. ({selectedCategory})");
        return false;
    }

    public static ContentFinderCondition? findCondition(ushort dutyId) {
        return Services.DataManager.GetExcelSheet<ContentFinderCondition>()!.GetRow(dutyId);
    }

    public static bool validFateTerritoryType(uint id) {
        var territoryTypeSheet = Services.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(id);
        if(territoryTypeSheet!.TerritoryIntendedUse.RowId == 1 && !territoryTypeSheet!.IsPvpZone)
            return true;

        return false;
    }

    public static SelectedCategory findDutyCategory(ContentFinderCondition condition) {
        if(condition.HighEndDuty) return SelectedCategory.HighendDuty; // Highend

        Dictionary<string, int> contentTypeToCategory = new() {
            ["Dungeons"] = 4, ["Guildhests"] = 8, ["Trials"] = 16, ["Raids"] = 32, ["PvP"] = 128, ["Eureka"] = 16384, ["V&C Dungeon Finder"] = 32768,
        };

        var type = condition.ContentType;
        if(contentTypeToCategory.ContainsKey(type.ToString())) // Dungeon, Guildhest, Trials, Raids, PvP, V&C
            return (SelectedCategory)contentTypeToCategory[type.ToString()];

        string[] fieldOperations = ["Zadnor", "Delubrum Reginae", "Delubrum Reginae (Savage)", "the Bozjan Southern Front"];

        var name = condition.Name.ExtractText();
        if(fieldOperations.Contains(name)) // Field Operations
           return SelectedCategory.FieldOperations;

        return SelectedCategory.None;
    }

    public static CategoryTab findDutyCategoryTab(ContentFinderCondition condition, SelectedCategory category, CategoryTab categoryTab) {
        if(category == SelectedCategory.Pvp && condition.Name.ExtractText().Contains("Crystalline Conflict"))
            return CategoryTab.CustomMatch;
        if(category == SelectedCategory.Raids && isAllianceContent(condition))
            return CategoryTab.Alliance;
        if(category == SelectedCategory.FieldOperations && categoryTab == CategoryTab.Alliance)
            return CategoryTab.Alliance;

        return CategoryTab.Normal;
    }

    public static bool isAllianceContent(ContentFinderCondition condition) {
        var territoryTypeSheet = Services.DataManager.GetExcelSheet<TerritoryType>()!.Where(r => r.ContentFinderCondition.Equals(condition));
        var intendedUse = territoryTypeSheet.First().TerritoryIntendedUse;
        return intendedUse.RowId == 8 || intendedUse.RowId == 52 || intendedUse.RowId == 53;
    }

    public static void shiftSlotsInCurrentParty(ref ulong[] slots, int partyIndex) {
        var currentParty = partyIndex / 8;
        var temp = slots[partyIndex];
        var shiftLength = (8 * (currentParty + 1)) - partyIndex - 1;
        Array.Copy(slots, partyIndex+1, slots, partyIndex, shiftLength);
        slots[((currentParty + 1) * 8) - 1] = temp;
    }

    // Debug
    public void SaveNewPresetAndPrint(string Name) {
        var x = new RecruitmentData(Name);
        RecruitmentPresets.Add(x);
        this.Save();
        x.PrintData();
    }
}
