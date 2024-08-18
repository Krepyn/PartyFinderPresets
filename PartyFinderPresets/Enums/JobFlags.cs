using Dalamud.Game.Gui.PartyFinder.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartyFinderPresets.Enums;

[Flags]
public enum JobFlags : ulong {
    //
    // Summary:
    //     Gladiator (GLD).
    Gladiator = 2,
    //
    // Summary:
    //     Pugilist (PGL).
    Pugilist = 4,
    //
    // Summary:
    //     Marauder (MRD).
    Marauder = 8,
    //
    // Summary:
    //     Lancer (LNC).
    Lancer = 0x10,
    //
    // Summary:
    //     Archer (ARC).
    Archer = 0x20,
    //
    // Summary:
    //     Conjurer (CNJ).
    Conjurer = 0x40,
    //
    // Summary:
    //     Thaumaturge (THM).
    Thaumaturge = 0x80,
    //
    // Summary:
    //     Paladin (PLD).
    Paladin = 0x100,
    //
    // Summary:
    //     Monk (MNK).
    Monk = 0x200,
    //
    // Summary:
    //     Warrior (WAR).
    Warrior = 0x400,
    //
    // Summary:
    //     Dragoon (DRG).
    Dragoon = 0x800,
    //
    // Summary:
    //     Bard (BRD).
    Bard = 0x1000,
    //
    // Summary:
    //     White mage (WHM).
    WhiteMage = 0x2000,
    //
    // Summary:
    //     Black mage (BLM).
    BlackMage = 0x4000,
    //
    // Summary:
    //     Arcanist (ACN).
    Arcanist = 0x8000,
    //
    // Summary:
    //     Summoner (SMN).
    Summoner = 0x10000,
    //
    // Summary:
    //     Scholar (SCH).
    Scholar = 0x20000,
    //
    // Summary:
    //     Rogue (ROG).
    Rogue = 0x40000,
    //
    // Summary:
    //     Ninja (NIN).
    Ninja = 0x80000,
    //
    // Summary:
    //     Machinist (MCH).
    Machinist = 0x100000,
    //
    // Summary:
    //     Dark Knight (DRK).
    DarkKnight = 0x200000,
    //
    // Summary:
    //     Astrologian (AST).
    Astrologian = 0x400000,
    //
    // Summary:
    //     Samurai (SAM).
    Samurai = 0x800000,
    //
    // Summary:
    //     Red mage (RDM).
    RedMage = 0x1000000,
    //
    // Summary:
    //     Blue mage (BLM).
    BlueMage = 0x2000000,
    //
    // Summary:
    //     Gunbreaker (GNB).
    Gunbreaker = 0x4000000,
    //
    // Summary:
    //     Dancer (DNC).
    Dancer = 0x8000000,
    //
    // Summary:
    //     Reaper (RPR).
    Reaper = 0x10000000,
    //
    // Summary:
    //     Sage (SGE).
    Sage = 0x20000000,
    //
    // Summary:
    //     Viper (VPR).
    Viper = 0x40000000,
    //
    // Summary:
    //     Pictomancer (PCT).
    Pictomancer = 0x80000000,
}
