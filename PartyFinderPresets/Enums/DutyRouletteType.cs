using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartyFinderPresets.Enums;

// Duty Roulette Keys, only consists of the ones available in Party Finder Recruitment
public enum DutyRouletteType : ushort {
    Leveling = 1,
    High_levelDungeons = 2,
    MainScenario = 3,
    Guildhests = 4,
    Expert = 5,
    Trials = 6,
    Frontline = 7,
    AllianceRaids = 15,
    NormalRaids = 17,
}
