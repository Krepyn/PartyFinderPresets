using Newtonsoft.Json;
using PartyFinderPresets.Classes;
using PartyFinderPresets.Structs;
using PartyFinderPresets.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json.Converters;
using Dalamud.Interface.ImGuiSeStringRenderer;
using Dalamud.Interface.Utility;
using Dalamud.Game.Text.SeStringHandling;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.System.String;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
using PartyFinderPresets.Utils;
namespace PartyFinderPresets.Controllers;

public unsafe class RecruitmentDataController : IDisposable
{
    public Plugin Plugin;
    public List<RecruitmentData> RecruitmentPresets = [];
    public RecruitmentSub CurrentData;
    private readonly string FileName = Path.Combine(Services.PluginInterface.ConfigDirectory.FullName, "PresetsLibrary.json");
    private nint testStr;

    public RecruitmentDataController(Plugin plugin) { 
        this.Plugin = plugin;
        this.CurrentData = new RecruitmentSub();
        testStr = Marshal.AllocHGlobal(196);

        Load();
    }

    public void Dispose() {
        Marshal.FreeHGlobal(testStr);
    }

    public void Load() {
        if (!File.Exists(FileName)) {
            Services.PluginLog.Verbose("Couln't find presets json.");
            return;
        } try {
            RecruitmentPresets = JsonConvert.DeserializeObject<List<RecruitmentData>>(File.ReadAllText(FileName))!;
        } catch(JsonReaderException e) {
            Services.PluginLog.Error($"Couldn't read presets json, {e}.");
        }

        if(RecruitmentPresets == null) RecruitmentPresets = [];
    }

    public void Save() {        
        if(RecruitmentPresets == null) return;
        File.WriteAllText(FileName, JsonConvert.SerializeObject(RecruitmentPresets, Formatting.Indented, [new StringEnumConverter()]));
        Services.PluginLog.Verbose("PresetLibrary Saved.");
        //File.WriteAllText(FileName, JsonConvert.SerializeObject(RecruitmentPresets, Formatting.Indented));
    }

    public RecruitmentData? GetPreset(int index) {
        if (RecruitmentPresets.Count == 0) return null;
        if (RecruitmentPresets[index] == null) return null;
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

    public int GetPresetCount() {
        if (RecruitmentPresets == null) return 0;
        return RecruitmentPresets.Count;
    }

    // Debug
    public void SaveNewPresetAndPrint(string Name) {
        var x = new RecruitmentData(Name);
        RecruitmentPresets.Add(x);
        this.Save();
        x.PrintData();
    }

    public unsafe void LoadPreset(int index) {
        var listToLoad = RecruitmentPresets[index];

        *CurrentData.AvgItemLvEnabled = (byte)(listToLoad.AvgItemLvEnabled ? 1 : 0);
        if(listToLoad.AvgItemLvEnabled)
            *CurrentData.AvgItemLv = listToLoad.AvgItemLv;
                
        var selectedCategory = listToLoad.SelectedCategory;
        var selectedDutyId = listToLoad.SelectedDutyId;
        var categoryTab = listToLoad.CategoryTab;
        if(DutyIdIsValid(ref selectedCategory, ref categoryTab, selectedDutyId)) {
            *CurrentData.SelectedCategory = selectedCategory;
            *CurrentData.SelectedDutyId = selectedDutyId;
            *CurrentData.CategoryTab = categoryTab;
        }

        var objective = listToLoad.Objective;
        if(Enum.IsDefined(typeof(Objective), objective))
            *CurrentData.Objective = objective;

        *CurrentData.BeginnerFriendly = (byte)(listToLoad.BeginnerFriendly ? 1 : 0);

        *CurrentData.LimitRecruitingToWorld = (byte)(listToLoad.LimitRecruitingToWorld ? 0 : 1);

        var completionStatus = listToLoad.CompletionStatus;
        if(Enum.IsDefined (typeof(CompletionStatus), completionStatus))
            *CurrentData.CompletionStatus = completionStatus;

        var dutyFinderSettings = listToLoad.DutyFinderSettingFlags;
        if((byte)dutyFinderSettings<=7) // 00000111 
            *CurrentData.DutyFinderSettingFlags = dutyFinderSettings;

        var lootRule = listToLoad.LootRule;
        if((byte)lootRule <=2) // 00000010
            *CurrentData.LootRule = lootRule;

        var password = UInt16.Parse(listToLoad.Password);
        *CurrentData.Password = password <= 10000 ? password : (ushort)10000;

        var language = listToLoad.LanguageFlags;
        if((byte)language <= 15) // 00001111
            *CurrentData.LanguageFlags = language;

        *CurrentData.OnePlayerPerJob = (byte)(listToLoad.OnePlayerPerJob ? 1 : 0);

        var slotFlags = listToLoad._slotFlags;
        var numberOfGroups = (listToLoad.NumberOfGroups <= 6 && listToLoad.NumberOfGroups > 0) ? listToLoad.NumberOfGroups : 1;
        for(var i = 1; i < 8 * numberOfGroups; i++) {
            if((ulong)slotFlags[i] % 2 == 1) slotFlags[i]--;
            if((ulong)slotFlags[i] > (ulong)0xFFFFFFFE) { // All roles
                Services.PluginLog.Verbose($"Slot {i + 1} is out of scope: {(ulong)slotFlags[i]}, {(long)slotFlags[i]}.");
                slotFlags[i] = 0;
            }
            if((ulong) slotFlags[i] == 0) shiftSlotsInCurrentParty(ref slotFlags, i);
            CurrentData._slotFlags[i] = (ulong)slotFlags[i];
            Services.PluginLog.Verbose($"Slot {i+1} has been loaded.");
        }

        // TODO load comment
        var commentString = listToLoad._seStrComment;
        var valid = isCommentValid(commentString);
        Services.PluginLog.Info($"{valid}");
        if(valid)
            Marshal.Copy(commentString, 0, (nint)CurrentData._comment, 196);
        else
            Services.PluginLog.Info($"Comment is longer than it is allowed.");

        this.Plugin.GameFunctions.RCRefresh(0, 0);

        Services.PluginLog.Info($"Preset: {listToLoad.Name} has been loaded.");
    }

    // Returns true if the wrapped string is the same as non-wrapped string
    // This should always be fine except if json is manually edited
    public unsafe bool isCommentValid(byte[] bytes) {
        var baseResNode = RaptureAtkUnitManager.Instance()->GetAddonByName("LookingForGroupCondition")->GetNodeById(18);
        var componentNode = (AtkComponentTextInput*)baseResNode->ChildNode->GetComponent();
        var textNode = componentNode->AtkTextNode->GetAsAtkTextNode(); // Actual Text Node
        // var textNode2 = componentNode->AtkTextNode->NextSiblingNode->GetAsAtkTextNode(); // Line Count Node

        //SetText on TextInputComponent to wrap it inside the textinput, so we can get the how many lines it takes value
        Marshal.Copy(bytes, 0, testStr, bytes.Length);
        componentNode->SetText((byte*)testStr);
        var aa = componentNode->UnkText1;
        Services.PluginLog.Info($"{aa.ToString()} .");
        textNode->SetText((byte*)testStr);
        var bb = textNode->NodeText.ToString().Replace("\u0002\u0010\u0001\u0003", "");
        Services.PluginLog.Info($"{bb} .");

        return aa.ToString() == bb;

        // These comments are here in case I wanna try fixing comments instead of 0'ing them
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

    public static bool DutyIdIsValid(ref SelectedCategory selectedCategory, ref CategoryTab categoryTab, ushort dutyId) {
        if(!Enum.IsDefined(typeof(SelectedCategory), selectedCategory)) {
            Services.PluginLog.Verbose($"Selected Category was wrong. ({((ushort)selectedCategory)})");
            return false;
        }
        
        if(selectedCategory == SelectedCategory.None)
            return true;

        SelectedCategory[] LuminaDuties = [SelectedCategory.Dungeons, SelectedCategory.Guildhests, SelectedCategory.Trials, SelectedCategory.Raids,
                                           SelectedCategory.HighendDuty, SelectedCategory.Pvp, SelectedCategory.FieldOperations, SelectedCategory.VandCDungeonFinder];
        
        if(LuminaDuties.Contains<SelectedCategory>(selectedCategory)) {
            var duty = findCondition(dutyId);
            if(duty == null) return false;
            selectedCategory = findDutyCategory(duty);
            categoryTab = CategoryTab.Normal;
            if(selectedCategory == SelectedCategory.Raids || selectedCategory == SelectedCategory.Pvp || selectedCategory == SelectedCategory.FieldOperations)
                categoryTab = findDutyCategoryTab(duty, selectedCategory, categoryTab);
            return true;
        }

        if(selectedCategory == SelectedCategory.TreasureHunt)
            return dutyId <= 23;

        if(selectedCategory == SelectedCategory.Fates)
            return validFateTerritoryType(dutyId);

        if(selectedCategory == SelectedCategory.TheHunt)
            return true;

        if(selectedCategory == SelectedCategory.DeepDungeons)
            return dutyId > 0 && dutyId <= 3;

        if(selectedCategory == SelectedCategory.DutyRoulette)
            return Enum.IsDefined(typeof(DutyRouletteType), dutyId);

        if(selectedCategory == SelectedCategory.GoldSaucer)
            return Enum.IsDefined(typeof(GoldSaucerType), dutyId);

        if(selectedCategory == SelectedCategory.GatheringForays) {
            return Enum.IsDefined(typeof(GatheringForayType), dutyId);
        }

        Services.PluginLog.Verbose($"Selected Category was wrong. ({((ushort)selectedCategory)})");
        return false;
    }

    // null if not, ContentFinderCondition entry if yes
    public static ContentFinderCondition? findCondition(ushort dutyId) {        
        return Services.DataManager.GetExcelSheet<ContentFinderCondition>()!.GetRow(row: dutyId);
    }

    public static bool validFateTerritoryType(uint id) {
        var territoryTypeSheet = Services.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(row: id);
        if(territoryTypeSheet!.TerritoryIntendedUse == 1 && !territoryTypeSheet!.IsPvpZone)
            return true;

        return false;
    }

    public static SelectedCategory findDutyCategory(ContentFinderCondition condition) {
        if(condition.HighEndDuty) return SelectedCategory.HighendDuty; // Highend

        Dictionary<string, int> contentTypeToCategory = new() {
            ["Dungeons"] = 4,
            ["Guildhests"] = 8,
            ["Trials"] = 16,
            ["Raids"] = 32,
            ["PvP"] = 128,
            ["Eureka"] = 16384,
            ["V&C Dungeon Finder"] = 32768,
        };

        var type = condition.ContentType;
        if(contentTypeToCategory.ContainsKey(type.ToString())) // Dungeon, Guildhest, Trials, Raids, PvP, V&C
            return (SelectedCategory)contentTypeToCategory[type.ToString()];

        string[] fieldOperations = ["Zadnor", "Delubrum Reginae", "Delubrum Reginae (Savage)", "the Bozjan Southern Front"];

        var name = condition.Name;
        if(fieldOperations.Contains(name)) // Field Operations
            return SelectedCategory.FieldOperations;

        return SelectedCategory.None;
    }

    public static CategoryTab findDutyCategoryTab(ContentFinderCondition condition, SelectedCategory category, CategoryTab categoryTab) {

        if(category == SelectedCategory.Pvp && condition.Name.RawString.Contains("Crystalline Conflict"))
            return CategoryTab.CustomMatch;
        if(category == SelectedCategory.Raids && isAllianceRaid(condition))
            return CategoryTab.Alliance;
        if(category == SelectedCategory.FieldOperations && categoryTab == CategoryTab.Alliance)
            return CategoryTab.Alliance;

        return CategoryTab.Normal;
    }

    public static bool isAllianceRaid(ContentFinderCondition condition) {
        var territoryTypeSheet = Services.DataManager.GetExcelSheet<TerritoryType>()!.Where(r => r.ContentFinderCondition.Equals(condition));
        var intendedUse = territoryTypeSheet.First().TerritoryIntendedUse;
        return intendedUse == 8 || intendedUse == 52 || intendedUse == 53;
    }

    public static void shiftSlotsInCurrentParty(ref JobFlags[] slots, int partyIndex) {
        var currentParty = partyIndex / 8;
        var temp = slots[partyIndex];
        var shiftLength = 8 * (currentParty + 1) - partyIndex - 1;
        Array.Copy(slots, partyIndex+1, slots, partyIndex, shiftLength);
        slots[(currentParty + 1) * 8 - 1] = temp;
    }
}
