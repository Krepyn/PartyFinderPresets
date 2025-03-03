//using PartyFinderPresets.Structs;
using PartyFinderPresets.Enums;
using System;
using System.Runtime.InteropServices;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder;
using Lumina.Excel.Sheets;
using System.Linq;
using Newtonsoft.Json;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace PartyFinderPresets.Classes;

public class RecruitmentData
{
    // Preset Related 
    [JsonProperty(Order = -2)]
    public string Name { get; set; } = "Preset";
    public string Comment = null!;  // array size 192 long
    public byte[] SeStrComment = new byte[196];

    public byte AvgItemLvEnabled;
    public ushort AvgItemLv;
    public ulong[] SlotFlags = new ulong[48];

    public AgentLookingForGroup.RecruitmentSub recruitmentSub;
    public CategoryTab CategoryTab;

    public RecruitmentData(string Name) {
        MakePresetFromCurrentData(Name);
    }
    public unsafe void MakePresetFromCurrentData(string Name = "Preset")
    {
        var agentInstance = AgentLookingForGroup.Instance();
        var current = agentInstance->StoredRecruitmentInfo;
        recruitmentSub = current;

        this.Name = Name;

        //this.Comment = SeString.Parse(current.Comment, 196).ToString();
        this.Comment = SeString.Parse(current.Comment).ToString();
        this.SeStrComment = current.Comment.ToArray();
            // new byte[196];
        //Marshal.Copy((IntPtr)current.Comment, this.SeStrComment, 0, 196);

        this.AvgItemLvEnabled = agentInstance->AvgItemLvEnabled;
        this.AvgItemLv = agentInstance->AvgItemLv;
        this.CategoryTab = (CategoryTab)agentInstance->GroupTypeTab;

        this.SlotFlags = current.SlotFlags.ToArray();
        //SlotFlagsPointerToArray((IntPtr) current.SlotFlags, this.recruitmentSub.NumberOfGroups);
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

#if DEBUG
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
        Services.PluginLog.Verbose($"Category Tab: {CategoryTab}");
        Services.PluginLog.Verbose($"Duty Type: {(SelectedCategory)recruitmentSub.SelectedCategory}");
        if (LuminaDuties.Contains<SelectedCategory>((SelectedCategory)recruitmentSub.SelectedCategory))
        {
            var duty = Services.DataManager.GetExcelSheet<ContentFinderCondition>()!.GetRow(recruitmentSub.SelectedDutyId);
            if (duty!.HighEndDuty == true) Services.PluginLog.Verbose($"Selected Duty Type: High-end Duty");
            else Services.PluginLog.Verbose($"Selected Duty Type: {duty!.ContentType.Value!.Name}");
            Services.PluginLog.Verbose($"Selected Duty Name: {duty!.Name}");
        } else if ((SelectedCategory)recruitmentSub.SelectedCategory == SelectedCategory.DutyRoulette) {

        }

        Services.PluginLog.Verbose($"Number of Groups: {recruitmentSub.NumberOfGroups}");
        if (recruitmentSub.Password == 10000) Services.PluginLog.Verbose($"Password: None");
        else Services.PluginLog.Verbose($"Password: {recruitmentSub.Password}");
        Services.PluginLog.Verbose($"Languages: {recruitmentSub.LanguageFlags}");
        Services.PluginLog.Verbose($"Second Slot Allowed Classes: {(JobFlags)SlotFlags[1]}");
        Services.PluginLog.Verbose($"Third Slot Allowed Classes: {(JobFlags)SlotFlags[2]}");
        Services.PluginLog.Verbose($"Fourth Slot Allowed Classes: {(JobFlags)SlotFlags[4]}");
        Services.PluginLog.Verbose($"Fifth Slot Allowed Classes: {(JobFlags)SlotFlags[5]}");
        Services.PluginLog.Verbose($"Sixth Slot Allowed Classes: {(JobFlags)SlotFlags[6]}");
        Services.PluginLog.Verbose($"Seventh Slot Allowed Classes: {(JobFlags)SlotFlags[7]}");
        Services.PluginLog.Verbose($"Eight Slot Allowed Classes: {(JobFlags)SlotFlags[8]}");
        Services.PluginLog.Verbose($"Comment: {Comment}");
        Services.PluginLog.Verbose($"----");        
    }
#endif

}
