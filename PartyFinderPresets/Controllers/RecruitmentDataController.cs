using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using PartyFinderPresets.Classes;
using PartyFinderPresets.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static FFXIVClientStructs.FFXIV.Client.Game.UI.ContentsFinder;
using static FFXIVClientStructs.FFXIV.Common.Component.BGCollision.MeshPCB;
using System.IO;

namespace PartyFinderPresets.Controllers;

public class RecruitmentDataController
{
    public List<RecruitmentData> RecruitmentPresets { get; set; } = [];
    public RecruitmentSub CurrentData;

    public RecruitmentDataController() { 
        CurrentData = new RecruitmentSub();
        LoadPresetList();
    }
    public void LoadPresetList()
    {

    }

    public void SavePresetList()
    {
        var fileName = Path.Combine(Services.PluginInterface.ConfigDirectory.FullName, "Presets.json");
        File.WriteAllText(fileName, JsonConvert.SerializeObject(RecruitmentPresets, Formatting.Indented, [new StringEnumConverter()]));
    }

    public void SaveNewPreset(string Name) => RecruitmentPresets.Add(new RecruitmentData(Name));

    // Debug
    public void SaveNewPresetAndPrint(string Name) {
        var x = new RecruitmentData(Name);
        RecruitmentPresets.Add(x);
        x.PrintData();
    }
}
