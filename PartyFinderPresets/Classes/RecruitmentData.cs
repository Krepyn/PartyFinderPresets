using PartyFinderPresets.Structs;
using PartyFinderPresets.Enums;
using System;
using System.Runtime.InteropServices;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder;
using Lumina.Excel.GeneratedSheets;
using System.Linq;
using Newtonsoft.Json;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.Text.SeStringHandling;

namespace PartyFinderPresets.Classes;

public class RecruitmentData
{
    // Preset Related 
    [JsonProperty(Order = -2)]
    public string Name { get; set; } = "Preset";
    public string Password = null!; // Not enabled is 10000, no password set is 0
    public string Comment = null!;  // array size 192 long
    public byte[] SeStrComment = new byte[196];


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
    public CategoryTab CategoryTab;

    public JobFlags[] SlotFlags = new JobFlags[48];
    public Language LanguageFlags;

    public RecruitmentData() {
        
    }

    public RecruitmentData(string Name) {
        MakePresetFromCurrentData(Name);
    }
    public unsafe void MakePresetFromCurrentData(string Name = "Preset")
    {
        var current = RecruitmentSub.GetCurrentData();

        this.Name = Name;

        this.Password = current.Password->ToString("D4");
        this.Comment = SeString.Parse(current.Comment, 196).ToString();
        this.SeStrComment = new byte[196];
        Marshal.Copy((IntPtr)current.Comment, this.SeStrComment, 0, 196);

        this.AvgItemLvEnabled = *current.AvgItemLvEnabled == 1;
        this.BeginnerFriendly = *current.BeginnerFriendly == 1;
        this.LimitRecruitingToWorld = *current.LimitRecruitingToWorld == 0;
        this.OnePlayerPerJob = *current.OnePlayerPerJob == 1;

        this.AvgItemLv = *current.AvgItemLv;
        this.SelectedCategory = *current.SelectedCategory;
        this.SelectedDutyId = *current.SelectedDutyId;

        this.NumberOfSlotsInMainParty = *current.NumberOfSlotsInMainParty;
        this.NumberOfGroups = *current.NumberOfGroups;

        this.CategoryTab = *current.CategoryTab;
        this.Objective = *current.Objective;
        this.CompletionStatus = *current.CompletionStatus;
        this.DutyFinderSettingFlags = *current.DutyFinderSettingFlags;

        this.SlotFlags = SlotFlagsPointerToArray((IntPtr) current.SlotFlags, this.NumberOfGroups);
        this.LanguageFlags = *current.LanguageFlags;
    }

    public static unsafe JobFlags[] SlotFlagsPointerToArray(IntPtr currentFlags, int NumberOfGroups)
    {
        var slotFlags = new JobFlags[48];
        var slotFlagsL = new long[48];
        Marshal.Copy(source: currentFlags, slotFlagsL, startIndex: 0, length: 48);

        // Marshal doesnt copy unsigned longs, so have to reassign (or I couldn't do it)
        for (var i = 0; i<NumberOfGroups*8; i++)
        {
            slotFlags[i] = (JobFlags) ((ulong)slotFlagsL[i]);
        }

        return slotFlags;
    }

    // For debugging
    public void PrintData()
    {
        // These categories have lumina entries so will check them from there for safety
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
        Services.PluginLog.Verbose($"Second Slot Allowed Classes: {SlotFlags[1]}");
        Services.PluginLog.Verbose($"Third Slot Allowed Classes: {SlotFlags[2]}");
        Services.PluginLog.Verbose($"Fourth Slot Allowed Classes: {SlotFlags[4]}");
        Services.PluginLog.Verbose($"Fifth Slot Allowed Classes: {SlotFlags[5]}");
        Services.PluginLog.Verbose($"Sixth Slot Allowed Classes: {SlotFlags[6]}");
        Services.PluginLog.Verbose($"Seventh Slot Allowed Classes: {SlotFlags[7]}");
        Services.PluginLog.Verbose($"Eight Slot Allowed Classes: {SlotFlags[8]}");
        Services.PluginLog.Verbose($"Comment: {Comment}");
        Services.PluginLog.Verbose($"----");        
    }
}
