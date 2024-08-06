using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using System;
using FFXIVClientStructs.FFXIV.Component;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace PartyFinderTemplates
{
    public sealed unsafe class GameFunctions : IDisposable
    {
        private delegate void LookingForGroupConditionUpdateDelegate(AgentLookingForGroup* param1, long param2);

        [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 2B E0 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B D9")]
        private readonly Hook<LookingForGroupConditionUpdateDelegate>? _conditionUpdate;

        /*private delegate uint* IDontKnowWhatThisIsDelegate(long param_1, uint* param_2, byte* param_3, int param_4);

        [Signature("40 56 48 83 EC ?? C7 02 ?? ?? ?? ?? 48 8B F2 C6 42 ?? ?? 41 83 F9")]
        private readonly Hook<IDontKnowWhatThisIsDelegate>? _iDontKnow;
*/

        public GameFunctions()
        {
            Services.GameInteropProvider.InitializeFromAttributes(this);
            this._conditionUpdate?.Enable();
            //this._iDontKnow?.Enable();
        }

        public void Dispose() {
            this._conditionUpdate?.Dispose();
            //this._iDontKnow?.Dispose();
        }

        //Param 1 Client::UI::Agent::AgentLookingForGroup*
        //Offsets in this function for param 1, for different things are tied to Recruitment Criteria window and an offset from LookingForGroup Agent
        //Param 2 seems to be the callback - Array of AtkValues
        public void LookingForGroupConditionUpdate(AgentLookingForGroup* param1, long param2)
        {
            if (this._conditionUpdate == null)
            {
                throw new InvalidOperationException("ConditionUpdate signature wasn't found");
            }

            AtkValue* test2 = (AtkValue*)(param2); //Union Values??? 0x10 Int, 0x20 Bool
            AtkValue* test3 = (AtkValue*)(param2 + sizeof(AtkValue));
            AtkValue* test4 = (AtkValue*)(param2 + (2 * sizeof(AtkValue)));

            AgentLookingForGroup A = *param1;

            //Services.PluginLog.Verbose($"Param1: {*(param1->OpenListing())}");
            //Services.PluginLog.Verbose($"Param1 casted on AtkValue->Type: {(*test).Type}");
            //Services.PluginLog.Verbose($"Param1 casted on ulong: {a}");
            //Services.PluginLog.Verbose($"Param2 casted on AtkValue: {*((bool*)param2 + 32)}");
            //Services.PluginLog.Verbose($"Param2: {param2}");
            //Services.PluginLog.Verbose($"Param2: {param2:X}");
            //Services.PluginLog.Verbose($"Param2 + 0x10: {(param2)}");
            Services.PluginLog.Verbose($"Param2 casted on AtkValue: {(*test2).Int}");
            Services.PluginLog.Verbose($"Param2 casted on AtkValue: {(*test3).UInt}");
            Services.PluginLog.Verbose($"Param2 casted on AtkValue: {(*test4).Bool}");
            //Services.PluginLog.Verbose($"Param2+32 to Bool: {*((bool*)param2 + sizeof(int) + sizeof(uint))}");

            (*test4).Bool = false;

            Services.PluginLog.Verbose($"Param2 casted on AtkValue: {(*test4).Bool}");

            *((byte*)(param1 + 5350)) = 0;

            this._conditionUpdate!.Original(param1, param2);
            return;
        }
/*
        public uint* IDontKnowWhatThisIs(long param_1, uint* param_2, byte* param_3, int param_4)
        {
            if (this._iDontKnow == null)
            {
                throw new InvalidOperationException("IDontKnow signature wasn't found");
            }

            //Services.PluginLog.Verbose($"AAAA {param_1}");
            //Services.PluginLog.Verbose($"BBBB {*param_2}");
            //Services.PluginLog.Verbose($"CCCC {*param_3}");
            //Services.PluginLog.Verbose($"DDDD {param_4}");

            return _iDontKnow!.Original(param_1, param_2, param_3, param_4);
        }
*/
    }
}
