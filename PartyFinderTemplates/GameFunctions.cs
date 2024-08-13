using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Dalamud.Game.Addon.Lifecycle;
using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

namespace PartyFinderTemplates;


public sealed unsafe class GameFunctions : IDisposable
{
    private readonly Plugin Plugin;
    private readonly AgentLookingForGroup* LookingForGroupAgent; // LookingForGroupAgent Address
    private bool conditionEnabled = false;

    // For sending manual updates
    private delegate void RCUpdateValuesDelegate(AgentLookingForGroup* param1, AtkValue* param2);
    [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 2B E0 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B D9")]
    private readonly RCUpdateValuesDelegate? _updateValues;

    // For interjecting game updates
    private delegate void RCUpdateValuesHookDelegate(AgentLookingForGroup* param1, AtkValue* param2);
    [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 2B E0 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B D9")]
    private readonly Hook<RCUpdateValuesHookDelegate>? _updateValuesHook;

    // For when LookingForGroupCondition is Refreshed
    private delegate void RCRefreshDelegate(AgentLookingForGroup* param1, ulong param2, char param3);
    [Signature("E8 ?? ?? ?? ?? 4D 89 BE ?? ?? ?? ?? 4D 89 BE")]
    private readonly RCRefreshDelegate? _addonRefresh;

    // For manually refreshing LookingForGroupCondition
    private delegate void RCRefreshHookDelegate(AgentLookingForGroup* param1, ulong param2, char param3);
    [Signature("E8 ?? ?? ?? ?? 4D 89 BE ?? ?? ?? ?? 4D 89 BE")]
    private readonly Hook<RCRefreshHookDelegate>? _addonRefreshHook;


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

        this._updateValuesHook?.Dispose();
        this._addonRefreshHook?.Dispose();
    }

    private void RCRefreshHook(AgentLookingForGroup* param1, ulong param2, char param3)
    {
        Services.PluginLog.Verbose($"Open Recruitment Criteria: {param2}, {(int)param3}");

        _addonRefreshHook!.Original(param1, param2, param3);
    }

    //Offsets in this function for param 1, for different things are tied to Recruitment Criteria window and an offset from LookingForGroup Agent
    //Param 2 Array of AtkValues, 3 values max(?)
    public void RCUpdateValuesHook(AgentLookingForGroup* param1, AtkValue* param2)
    {
        if (this._updateValuesHook == null)
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

        _updateValuesHook!.Original(param1, param2);
        return;
    }

    // Send AtkValue Array with data to change recruitment criteria fields in AgentLookingForGroup
    // Will change the functionality with RecruitmentSub from AgentLookingForGroup once Infi's pr is gone through
    private void RCUpdateValues(AtkValue* Value) {
        //if (conditionEnabled)
        //{
            this._updateValues!.Invoke(LookingForGroupAgent, Value);
        //}
    }

    // Refresh Recruitment Criteria Menu
    // Current Recruitment Status => 0 = Not Currently Reloading, 1 = Currently Recruiting
    // Unknown => 0 or 1, doesn't change the outcome
    public void RCRefresh(ulong param1, int param2 = 0)
    {
        if (conditionEnabled)
        {
            this._addonRefresh!.Invoke(LookingForGroupAgent, param1, (char)param2);
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

    // Debug Functions

    // Temporary Eventhandler to see what events are invoked
    private void OnEvent(AddonEvent type, AddonArgs args) {

        Services.PluginLog.Verbose($"Event Called: {type} to {args.AddonName}");

    }

    public void AvgItemLvOff()
    {
        Services.PluginLog.Verbose("Avg Item Level has been turned off.");

        AtkValue[] Value = new AtkValue[3];
        Value[0].SetInt(11);
        Value[1].SetUInt(6);
        Value[2].SetBool(false);

        fixed (AtkValue* ValuePtr = &Value[0])
            RCUpdateValues(ValuePtr);
    }

    public void AvgItemLvOn()
    {
        Services.PluginLog.Verbose($"{conditionEnabled}");
        Services.PluginLog.Verbose("Avg Item Level has been turned on.");

        AtkValue[] Value = new AtkValue[3];
        Value[0].SetInt(11);
        Value[1].SetUInt(6);
        Value[2].SetBool(true);

        fixed (AtkValue* ValuePtr = &Value[0])
            RCUpdateValues(ValuePtr);
    }

    // Toggles _conditionUpdate hook
    public void ToggleUpdateHook()
    {
        if (this._updateValuesHook == null)
            return;

        if (this._updateValuesHook.IsEnabled)
        {
            Services.PluginLog.Verbose("Disabled Update Hook.");
            _updateValuesHook?.Disable();
        } else {
            Services.PluginLog.Verbose("Enabled Update Hook.");
            _updateValuesHook?.Enable();
        }
    }

    // Toggles _addonUpdateHook 
    public void ToggleCriteriaWindowHook()
    {
        if(this._addonRefreshHook == null) return;

        if (this._addonRefreshHook.IsEnabled)
        {
            Services.PluginLog.Verbose("Disabled Window Hook.");
            this._addonRefreshHook?.Disable();
        } else
        {
            Services.PluginLog.Verbose("Enabled Window Hook.");
            this._addonRefreshHook?.Enable();
        }
    }

}
