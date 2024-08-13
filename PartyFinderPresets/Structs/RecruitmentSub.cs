using System;
using System.Collections.Generic;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System.Xml.Linq;

namespace PartyFinderPresets.Structs;

public unsafe class RecruitmentSub
{
    private const int SUBSTRUCT_OFFSET = 0x22A0; // Recruitment Sub offset from agent

    // Game Data
    public ushort* AvgItemLv;
    public byte* AvgItemLvEnabled;

    public SelectedCategory* SelectedCategory;
    public ushort* SelectedDutyId;

    public Objective* Objective;
    public byte* BeginnerFriendly;
    public CompletionStatus* CompletionStatus;
    public DutyFinderSetting* DutyFinderSettingFlags;
    public LootRule* LootRule;

    public ushort* Password; // Not enabled is 10000, no password set is 0

    public Language* LanguageFlags;

    public byte* NumberOfSlotsInMainParty;

    public byte* LimitRecruitingToWorld; // 0 is enabled, 1 is disabled

    public byte* OnePlayerPerJob;
    public byte* NumberOfGroups; // 1 Normal, 3 Alliances, 6 Field Operations

    public ulong* _slotFlags; // array size 48
    public byte* _comment;  // array size 192

    // Other
    private readonly AgentLookingForGroup* LookingForGroupAgent;

    private static readonly Dictionary<string, int> dataOffsets = new()
    {
        ["SelectedCategory"] = 0x0C,
        ["SelectedDutyId"] = 0x10,
        ["Objective"] = 0x18,
        ["BeginnerFriendly"] = 0x19,
        ["CompletionStatus"] = 0x1A,
        ["DutyFinderSettingFlags"] = 0x1B,
        ["LootRule"] = 0x1C,
        ["Password"] = 0x22,
        ["LanguageFlags"] = 0x24,
        ["NumberOfSlotsInMainParty"] = 0x25,
        ["LimitRecruitingToWorld"] = 0x26,
        ["OnePlayerPerJob"] = 0x27,
        ["NumberOfGroups"] = 0x28,
        ["_memberContentIds"] = 0x30,
        ["_slotFlags"] = 0x1B0,
        ["_comment"] = 0x330,
        ["AvgItemLv"] = 0x14E4,
        ["AvgItemLvEnabled"] = 0x14E6,
    };

    public RecruitmentSub()
    {
        LookingForGroupAgent = AgentLookingForGroup.Instance();

        // Game Data Pointers
        AvgItemLv = (ushort*)((byte*)LookingForGroupAgent + dataOffsets["AvgItemLv"]);
        AvgItemLvEnabled = (byte*)LookingForGroupAgent + dataOffsets["AvgItemLvEnabled"];
        SelectedCategory = (SelectedCategory*)((byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["SelectedCategory"]);
        SelectedDutyId = (ushort*)((byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["SelectedDutyId"]);
        Objective = (Objective*)((byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["Objective"]);
        BeginnerFriendly = (byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["BeginnerFriendly"];
        CompletionStatus = (CompletionStatus*)((byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["CompletionStatus"]);
        DutyFinderSettingFlags = (DutyFinderSetting*)((byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["DutyFinderSettingFlags"]);
        LootRule = (LootRule*)((byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["LootRule"]);
        Password = (ushort*)((byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["Password"]);
        LanguageFlags = (Language*)((byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["LanguageFlags"]);
        NumberOfSlotsInMainParty = (byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["NumberOfSlotsInMainParty"];
        LimitRecruitingToWorld = (byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["LimitRecruitingToWorld"];
        OnePlayerPerJob = (byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["OnePlayerPerJob"];
        NumberOfGroups = (byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["NumberOfGroups"];
        _slotFlags = (ulong*)((byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["_slotFlags"]);
        _comment = (byte*)LookingForGroupAgent + SUBSTRUCT_OFFSET + dataOffsets["_comment"];
    }
    public static RecruitmentSub GetCurrentData()
    {
        return new RecruitmentSub();
    }
}

public enum Objective : byte
{
    None = 0,
    DutyCompletion = 1,
    Practice = 2,
    Loot = 4,
}
public enum CompletionStatus : byte
{
    None = 0,
    DutyComplete = 2,
    DutyIncomplete = 4,
    DutyCompleteWeeklyUnclaimed = 8,
}

[Flags]
public enum DutyFinderSetting : byte
{
    None = 0,
    UnrestrictedParty = 1,
    MinimumIL = 2,
    SilenceEcho = 4,
}

[Flags]
public enum Language : byte
{
    Japanese = 1,
    English = 2,
    German = 4,
    French = 8,
}

public enum SelectedCategory : ushort
{
    None = 0,
    DutyRoulette = 2,
    Dungeons = 4,
    Guildhests = 8,
    Trials = 16,
    Raids = 32,
    HighendDuty = 64,
    Pvp = 128,
    GoldSaucer = 256,
    Fates = 512,
    TreasureHunt = 1024,
    TheHunt = 2048,
    GatheringForays = 4096,
    DeepDungeons = 8192,
    FieldOperations = 16384,
    VandCDungeonFinder = 32768,
}
