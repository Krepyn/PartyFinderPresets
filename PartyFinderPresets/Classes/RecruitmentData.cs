using Dalamud.Game.Gui.PartyFinder.Types;
using PartyFinderPresets.Structs;
using System;
using System.Runtime.InteropServices;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder;
using Lumina;
using Lumina.Excel.GeneratedSheets;
using Dalamud.Plugin.Services;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.CompilerServices;

namespace PartyFinderPresets.Classes;

//[JsonConverter(typeof(RecruitmentDataJsonConverter))]
public class RecruitmentData
{
    // Preset Related

    [JsonProperty(Order = int.MinValue)]
    public string Name { get; set; } = "Preset";
    public string Password = null!; // Not enabled is 10000, no password set is 0
    public string _comment = null!;  // array size 192 long

    public bool AvgItemLvEnabled;
    public bool BeginnerFriendly;
    public bool LimitRecruitingToWorld; // 0 is enabled, 1 is disabled
    public bool OnePlayerPerJob;

    public ushort AvgItemLv;
    public ushort SelectedDutyId;

    public int NumberOfSlotsInMainParty;
    public int NumberOfGroups; // 1 Normal, 3 Alliances, 6 Field Operations

    public SelectedCategory SelectedCategory;
    public Objective Objective;
    public CompletionStatus CompletionStatus;
    public DutyFinderSetting DutyFinderSettingFlags;
    public LootRule LootRule;
    public JobFlags[] _slotFlags = new JobFlags[48];
    public Language LanguageFlags;

    public RecruitmentData() {
        
    }

    public RecruitmentData(string Name)
    {
        MakePresetFromCurrentData();
        if(Name != "") this.Name = Name;
    }
    public unsafe void MakePresetFromCurrentData()
    {
        var current = RecruitmentSub.GetCurrentData();

        this.Password = current.Password->ToString("D4");
        this._comment = System.Text.Encoding.UTF8.GetString(current._comment, 192).Split(['\u0000'])[0];

        this.AvgItemLvEnabled = *current.AvgItemLvEnabled == 1;
        this.BeginnerFriendly = *current.BeginnerFriendly == 1;
        this.LimitRecruitingToWorld = *current.LimitRecruitingToWorld == 0;
        this.OnePlayerPerJob = *current.OnePlayerPerJob == 1;

        this.AvgItemLv = *current.AvgItemLv;
        this.SelectedCategory = *current.SelectedCategory;
        this.SelectedDutyId = *current.SelectedDutyId;

        this.NumberOfSlotsInMainParty = *current.NumberOfSlotsInMainParty;
        this.NumberOfGroups = *current.NumberOfGroups;

        this.Objective = *current.Objective;
        this.CompletionStatus = *current.CompletionStatus;
        this.DutyFinderSettingFlags = *current.DutyFinderSettingFlags;

        this._slotFlags = SlotFlagsPointerToArray((IntPtr) current._slotFlags, this.NumberOfGroups);
        this.LanguageFlags = *current.LanguageFlags;
    }

    public static unsafe JobFlags[] SlotFlagsPointerToArray(IntPtr currentFlags, int NumberOfGroups)
    {
        var slotFlags = new JobFlags[48];

        var slotFlagsL = new long[48];
        Marshal.Copy(source: currentFlags, slotFlagsL, startIndex: 0, length: 48);

        for (int i = 0; i<NumberOfGroups*8; i++)
        {
            slotFlags[i] = (JobFlags) ((ulong)slotFlagsL[i]);
        }

        return slotFlags;
    }

    // For debugging
    public void PrintData()
    {
        SelectedCategory[] LuminaDuties = [SelectedCategory.Dungeons,
                                            SelectedCategory.VandCDungeonFinder,
                                            SelectedCategory.Trials,
                                            SelectedCategory.FieldOperations,
                                            SelectedCategory.Guildhests,
                                            SelectedCategory.Raids,
                                            SelectedCategory.Pvp,
                                            SelectedCategory.HighendDuty,
                                            ];


        Services.PluginLog.Verbose($"Preset Name: {Name}");
        Services.PluginLog.Verbose($"AvgItemLv: {AvgItemLv}");
        Services.PluginLog.Verbose($"AvgItemLvEnabled: {AvgItemLvEnabled}");
        Services.PluginLog.Verbose($"Selected Category Type: {SelectedCategory}");
        if (LuminaDuties.Contains<SelectedCategory>(SelectedCategory))
        {
            var duty = Services.DataManager.GetExcelSheet<ContentFinderCondition>()!.GetRow(row: SelectedDutyId);
            if (duty!.HighEndDuty == true) Services.PluginLog.Verbose($"Selected Duty Type: High-end Duty");
            else Services.PluginLog.Verbose($"Selected Duty Type: {duty!.ContentType.Value!.Name}");
            Services.PluginLog.Verbose($"Selected Duty Name: {duty!.Name}");
        } else if (SelectedCategory == SelectedCategory.DutyRoulette) {

        }
        if (Password == "10000") Services.PluginLog.Verbose($"Password: None");
        else Services.PluginLog.Verbose($"Password: {Password}");
        Services.PluginLog.Verbose($"Password: {LanguageFlags}");
        Services.PluginLog.Verbose($"Second Slot Allowed Classes: {_slotFlags[1]}");
        Services.PluginLog.Verbose($"Comment: {_comment}");
        Services.PluginLog.Verbose($"----");        
    }
}
