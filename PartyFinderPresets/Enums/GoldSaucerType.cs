using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartyFinderPresets.Enums;

// Gold Sucer Duty Type, Based on their id on party finder recruitment
// CR = Chocobo Race, NR = No Rewards, FM = Full Match, QM = Quick Match
public enum GoldSaucerType : ushort {

    GATEs = 11,
    CRRandom = 12,
    CRSagoliiRoad = 13,
    CRCosta_delSol = 14,
    CRTranquilPaths = 15,
    CRRandomNR = 16,
    CRSagoliiRoadNR = 17,
    CRCosta_delSolNR = 18,
    CRTranquilPathsNs = 19,
    TheTripleTriadBattlehall = 20,
    TripleTriadInvitationalParlor = 21,
    MahjongFMKuitanEnabled = 23,
    MahjongFMKuitanDisabled = 24,
    MahjongQMKuitanEnabled = 25,
    MahjongQMKuitanDisabled = 26,
}
