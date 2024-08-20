using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Dalamud.Game.Addon.Lifecycle;
using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

namespace PartyFinderPresets;

public sealed unsafe class GameFunctions : IDisposable
{
    private readonly Plugin Plugin;
    private readonly AgentLookingForGroup* LookingForGroupAgent;
    private bool conditionEnabled = false; // LookingForGroupCondition
    public int LastRefreshCondition = 1; // Current Recruitment Status => 0 = Currently Not Recruiting, 1 = Currently Recruiting

    // For changing fields through a function
    // Param2 Array of AtkValues, 3 values max(?) -> [0] is operation id, [1] & [2] is needed data
    private delegate void RCUpdateValuesDelegate(AgentLookingForGroup* param1, AtkValue* param2);
    [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 2B E0 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B D9")]
    private readonly RCUpdateValuesDelegate? _updateValues;
    [Signature("48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 8D AC 24 ?? ?? ?? ?? B8 ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 2B E0 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 85 ?? ?? ?? ?? 48 8B D9", DetourName = nameof(RCUpdateValuesHook))]
    private readonly Hook<RCUpdateValuesDelegate>? _updateValuesHook;

    // Refresh LookingForGroupCondition Addon (Recruitment Criteria Menu)
    // param1: Current Recruitment Status => 0 = Currently Not Recruiting, 1 = Currently Recruiting
    // param2: Unknown => 0 or 1, dunno what it does??
    private delegate void RCRefreshDelegate(AgentLookingForGroup* param1, ulong param2, char param3);
    [Signature("E8 ?? ?? ?? ?? 4D 89 BE ?? ?? ?? ?? 4D 89 BE")]
    private readonly RCRefreshDelegate? _addonRefresh;
    [Signature("E8 ?? ?? ?? ?? 4D 89 BE ?? ?? ?? ?? 4D 89 BE", DetourName = nameof(RCRefreshHook))]
    private readonly Hook<RCRefreshDelegate>? _addonRefreshHook;

    public GameFunctions(Plugin Plugin)
    {
        this.Plugin = Plugin;
        LookingForGroupAgent = AgentLookingForGroup.Instance();
        Services.GameInteropProvider.InitializeFromAttributes(this);

        _addonRefreshHook?.Enable();

        Services.AddonLifecycle.RegisterListener(AddonEvent.PreDraw, addonName:"LookingForGroupCondition", OnConditionEvent);
        Services.AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, addonName: "LookingForGroupCondition", OnConditionEvent);
    }

    public void Dispose() {
        Services.AddonLifecycle.UnregisterListener(OnConditionEvent);

        this._updateValuesHook?.Dispose();
        this._addonRefreshHook?.Dispose();
    }

    public void RCRefresh(ulong param1, int param2 = 0) {
        if(conditionEnabled) {
            this._addonRefresh!.Invoke(LookingForGroupAgent, param1, (char)param2);
        } else {
            Services.PluginLog.Verbose("LookingForGroupCondition is not open.");
        }
    }

    private void RCRefreshHook(AgentLookingForGroup* param1, ulong param2, char param3)
    {
        try {
            Services.PluginLog.Verbose($"Recruitment Criteria Opened: {param2}, {(int)param3}");
            LastRefreshCondition = (int)param2;
        } catch(Exception ex) {
            Services.PluginLog.Error($"An error happened while hooking RCRefresh: {ex}");
        }

        _addonRefreshHook!.Original(param1, param2, param3);
    }
    private void RCUpdateValues(AtkValue* Value) {
        this._updateValues!.Invoke(LookingForGroupAgent, Value);
    }

    public void RCUpdateValuesHook(AgentLookingForGroup* param1, AtkValue* param2)
    {
        try {
            if(this._updateValuesHook == null) {
                throw new InvalidOperationException("ConditionUpdate signature wasn't found");
            }

            Services.PluginLog.Verbose("------");
            Services.PluginLog.Verbose($"AtkValue #0: Type = {param2[0].Type}, Value = {param2[0].GetValueAsString()}");
            Services.PluginLog.Verbose($"AtkValue #1: Type = {param2[1].Type}, Value = {param2[1].GetValueAsString()}");
            Services.PluginLog.Verbose($"AtkValue #2: Type = {param2[2].Type}, Value = {param2[2].GetValueAsString()}");
        } catch(Exception ex) {
            Services.PluginLog.Error($"An error happened while hooking RCUpdateValues: {ex}");
        }
       
        _updateValuesHook!.Original(param1, param2);
        return;
    }

    private void OnConditionEvent(AddonEvent type, AddonArgs args)
    {
        if(type == AddonEvent.PreDraw) {
            Plugin.MainWindow.IsOpen = true;
            conditionEnabled = true;
        } else if(type == AddonEvent.PreFinalize) {
            Plugin.MainWindow.IsOpen = false;
            conditionEnabled = false;
        }

        // Debug to see what events are invoked except above
        // Services.PluginLog.Verbose($"Event Called: {type} to {args.AddonName}");
    }

    // Debug Functions
    public void AvgItemLv(bool status)
    {
        Services.PluginLog.Verbose($"Avg Item Level has been turned: {status}.");

        var Value = new AtkValue[3];
        Value[0].SetInt(11);
        Value[1].SetUInt(6);
        Value[2].SetBool(status);

        fixed (AtkValue* ValuePtr = &Value[0]) {
            RCUpdateValues(ValuePtr);
        }
    }

    public void ToggleUpdateHook()
    {
        if (this._updateValuesHook == null) return;

        if (this._updateValuesHook.IsEnabled) {
            Services.PluginLog.Verbose("Disabled Update Hook.");
            _updateValuesHook?.Disable();
        } else {
            Services.PluginLog.Verbose("Enabled Update Hook.");
            _updateValuesHook?.Enable();
        }
    }

    public void ToggleCriteriaWindowHook()
    {
        if(this._addonRefreshHook == null) return;

        if (this._addonRefreshHook.IsEnabled) {
            Services.PluginLog.Verbose("Disabled Window Hook.");
            this._addonRefreshHook?.Disable();
        } else {
            Services.PluginLog.Verbose("Enabled Window Hook.");
            this._addonRefreshHook?.Enable();
        }
    }
}
