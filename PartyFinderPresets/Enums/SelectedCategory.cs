using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartyFinderPresets.Enums;
public enum SelectedCategory : ushort {
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
