using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.String;
using System;
using FFXIVClientStructs.FFXIV.Component;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Lumina.Data.Parsing;
using static FFXIVClientStructs.FFXIV.Client.UI.UIModule.Delegates;
using static FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentInterface.Delegates;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using System.Runtime.CompilerServices;
using static FFXIVClientStructs.FFXIV.Component.GUI.AtkUnitManager.Delegates;
using FFXIVClientStructs.Attributes;
using Dalamud.Utility;

namespace PartyFinderTemplates
{

    // TODO Make it so templates only set if last param2 from loadrecruitmentcriteriahook is 0, do this by holding onto the param2 everytime the window is called and hold it -> give error message if not
    // TODO Check if LoadRecruitmentCriteriaHook has anything to do with being in a party or not, by opening recruitment criteria while in a party

    public sealed unsafe class GameFunctions : IDisposable
    {
        private readonly Plugin Plugin;
        private readonly AgentLookingForGroup* LookingForGroupAgent; // LookingForGroupAgent Address
        private bool conditionEnabled = false;

        // For interjecting game updates
        private delegate void LookingForGroupConditionUpdateDelegate(AgentLookingForGroup* param1, AtkValue* param2);
        [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 2B E0 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B D9")]
        private readonly Hook<LookingForGroupConditionUpdateDelegate>? _conditionUpdate;

        // For sending manual updates
        private delegate void LookingForGroupConditionSendDelegate(AgentLookingForGroup* param1, AtkValue* param2);
        [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 2B E0 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B D9")]
        private readonly LookingForGroupConditionSendDelegate? _conditionUpdateDelegate;

        // For when LookingForGroupCondition is Redrawn
        private delegate void LoadRecruitmentCriteriaDelegate(AgentLookingForGroup* param1, ulong param2, char param3);
        [Signature("E8 ?? ?? ?? ?? 4D 89 BE ?? ?? ?? ?? 4D 89 BE")]
        private readonly LoadRecruitmentCriteriaDelegate? _addonUpdate;

        // For manually redrawing LookingForGroupCondition
        private delegate void LoadRecruitmentCriteriaHookDelegate(AgentLookingForGroup* param1, ulong param2, char param3);
        [Signature("E8 ?? ?? ?? ?? 4D 89 BE ?? ?? ?? ?? 4D 89 BE")]
        private readonly Hook<LoadRecruitmentCriteriaHookDelegate>? _addonUpdateHook;


        public GameFunctions(Plugin Plugin)
        {
            this.Plugin = Plugin;

            LookingForGroupAgent = AgentLookingForGroup.Instance();

            Services.GameInteropProvider.InitializeFromAttributes(this);

            // Register Listeners for Opening and Closing Recruitment Criteria Window
            Services.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, addonName:"LookingForGroupCondition", OnConditionEvent);
            Services.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, addonName: "LookingForGroupCondition", OnConditionEvent);

        }

        public void Dispose() {
            // Unregister Listeners for Recruitment Criteria Window
            Services.AddonLifecycle.UnregisterListener(OnConditionEvent);
            //Services.AddonLifecycle.UnregisterListener(OnEvent);

            this._conditionUpdate?.Dispose();
            this._addonUpdateHook?.Dispose();
        }

        private void LoadRecruitmentCriteriaHook(AgentLookingForGroup* param1, ulong param2, char param3)
        {
            Services.PluginLog.Verbose($"Open Recruitment Criteria: {param2}, {(int)param3}");

            _addonUpdateHook!.Original(param1, param2, param3);
        }

        //Offsets in this function for param 1, for different things are tied to Recruitment Criteria window and an offset from LookingForGroup Agent
        //Param 2 Array of AtkValues, 3 values max(?)
        public void LookingForGroupConditionUpdate(AgentLookingForGroup* param1, AtkValue* param2)
        {
            if (this._conditionUpdate == null)
            {
                throw new InvalidOperationException("ConditionUpdate signature wasn't found");
            }

            Services.PluginLog.Verbose("------");
            Services.PluginLog.Verbose($"AtkValue #0.Type: {param2[0].Type}");
            Services.PluginLog.Verbose($"AtkValue #0.Type: {param2[0].GetValueAsString()}");
            Services.PluginLog.Verbose($"AtkValue #1.Type: {param2[1].Type}");
            Services.PluginLog.Verbose($"AtkValue #1.Type: {param2[1].GetValueAsString()}");
            Services.PluginLog.Verbose($"AtkValue #2.Type: {param2[2].Type}");
            Services.PluginLog.Verbose($"AtkValue #2.Type: {param2[2].GetValueAsString()}");

            _conditionUpdate!.Original(param1, param2);
            return;
        }

        public void AvgItemLvOff()
        {
            Services.PluginLog.Verbose("Avg Item Level has been turned off.");

            AtkValue[] Value = new AtkValue[3];
            Value[0].SetInt(11);
            Value[1].SetUInt(6);
            Value[2].SetBool(false);

            fixed(AtkValue* ValuePtr = &Value[0])
            LookingForGroupConditionSend(ValuePtr);
        }

        public void AvgItemLvOn()
        {
            Services.PluginLog.Verbose("Avg Item Level has been turned on.");

            AtkValue[] Value = new AtkValue[3];
            Value[0].SetInt(11);
            Value[1].SetUInt(6);
            Value[2].SetBool(true);

            fixed (AtkValue* ValuePtr = &Value[0])
            LookingForGroupConditionSend(ValuePtr);

            //AtkValue Value2 = new AtkValue();
            //Value2.SetInt(-1);
            //AtkValue* Value2Ptr = &Value2;
            //LookingForGroupConditionSend(Value2Ptr);
        }

        // Send AtkValue Array with data to change recruitment criteria fields in AgentLookingForGroup
        // Will change the functionality with RecruitmentSub from AgentLookingForGroup once Infi's pr is gone through
        private void LookingForGroupConditionSend(AtkValue* Value) {
            if (conditionEnabled)
            {
                this._conditionUpdateDelegate(LookingForGroupAgent, Value);
            }
        }

        /// <summary>
        ///     Refresh Recruitment Criteria Menu
        /// </summary>
        /// <param name="param1">Current Recruitment Status <br /> 0 = Not Currently Reloading <br /> 1 = Currently Recruiting</param> 
        /// <param name="param2">Unknown <br /> 0 or 1, doesn't change the outcome</param>
        public void LoadRecruitmentCriteriaUpdate(ulong param1, int param2 = 0)
        {
            if (conditionEnabled)
            {
                this._addonUpdate(LookingForGroupAgent, param1, (char)param2);
            }
        }

        // Event Handler
        private void OnConditionEvent(AddonEvent type, AddonArgs args)
        {
            if (type == AddonEvent.PreDraw)
            {
#if DEBUG
                Plugin.DebugWindow.IsOpen = true;
#endif
                conditionEnabled = true;
            }
            else if (type == AddonEvent.PreFinalize)
            {
#if DEBUG
                Plugin.DebugWindow.IsOpen = false;
#endif
                conditionEnabled = false;
            }
        }

        // Temporary Eventhandler to see what events are invoked
        private void OnEvent(AddonEvent type, AddonArgs args) {

            Services.PluginLog.Verbose($"Event Called: {type} to {args.AddonName}");

        }

        // Toggles _conditionUpdate hook
        public void ToggleUpdateHook()
        {
            if (this._conditionUpdate == null)
                return;

            if (this._conditionUpdate.IsEnabled)
            {
                Services.PluginLog.Verbose("Disabled Update Hook.");
                _conditionUpdate?.Disable();
            } else {
                Services.PluginLog.Verbose("Enabled Update Hook.");
                _conditionUpdate?.Enable();
            }
        }

        // Toggles _addonUpdateHook 
        public void ToggleCriteriaWindowHook()
        {
            if(this._addonUpdateHook == null) return;

            if (this._addonUpdateHook.IsEnabled)
            {
                Services.PluginLog.Verbose("Disabled Window Hook.");
                this._addonUpdateHook?.Disable();
            } else
            {
                Services.PluginLog.Verbose("Enabled Window Hook.");
                this._addonUpdateHook?.Enable();
            }
        }

    }
}
