using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Dalamud.Interface.Utility.Raii;

namespace PartyFinderTemplates.Windows;

public sealed class DebugWindow : Window, IDisposable
{
    private readonly Plugin Plugin;
    public AtkValue[] AtkValues = null!;

    public DebugWindow(Plugin plugin)
        : base("Debug##pluginDebugWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.Plugin = plugin;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() {
    }

    public override void Draw()
    {
        if (ImGui.Button("Toggle Recruitment Update Hook"))
        {
            this.Plugin.GameFunctions.ToggleUpdateHook();
        }
        if (ImGui.Button("AtkValue Test"))
        {
            this.Plugin.GameFunctions.ToggleUpdateHook();
        }
        if (ImGui.Button("AvgItemLv On"))
        {
            this.Plugin.GameFunctions.AvgItemLvOn();
        }
        if (ImGui.Button("AvgItemLv Off"))
        {
            this.Plugin.GameFunctions.AvgItemLvOff();
        }

        if (AtkValues != null)
        {
            using (ImRaii.Table("AtkValues", AtkValues.Length))
            {
                ImGui.TableSetupColumn("Func", ImGuiTableColumnFlags.WidthFixed, 40);
                ImGui.TableSetupColumn("Int", ImGuiTableColumnFlags.WidthFixed, 100);
                ImGui.TableSetupColumn("Bool", ImGuiTableColumnFlags.WidthFixed, 40);


                ImGui.TableNextColumn();
                ImGui.Text($"{AtkValues[0].Int}");
                ImGui.TableNextColumn();
                ImGui.Text($"{AtkValues[1].UInt}");
                ImGui.TableNextColumn();
                ImGui.Text($"{AtkValues[2].Bool}");
            }
        }
            ImGui.Spacing();
        //AddonStuff();
    }

    /*
    private void AddonStuff()
    {
        LookingForGroupCondition = (AtkUnitBase*)Services.GameGui.GetAddonByName("LookingForGroupCondition").ToPointer();
        if (LookingForGroupCondition != null)
        {
            return;
        }

            
        if (LookingForGroupCondition != null)
        {
            ImGui.Text($"{this.LookingForGroupCondition->AtkValues[44].Int}");
            ImGui.Text($"{this.LookingForGroupCondition->AtkValues[249].Int}");
            ImGui.Text($"{this.LookingForGroupCondition->AtkValues[260].Int}");
        }
    }*/
}
