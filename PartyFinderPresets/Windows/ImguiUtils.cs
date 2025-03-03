using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PartyFinderPresets.Windows;

internal static class ImguiUtils {
    public static ImRaii.Style NoItemSpacingX() =>
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));

    public static ImRaii.Style NoFrameRounding() =>
        ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);

    //public static void SelectableHoverOverDelete() {
    //            if (ImGui.IsItemHovered())
    //            {
    //                ImGui.PushFont(UiBuilder.IconFont);
    //                var deleteWidth = ImGui.CalcTextSize(FontAwesomeIcon.Times.ToIconString()).X;
    //ImGui.SameLine(ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemInnerSpacing.X* 2 - deleteWidth);
    //                ImGui.TextUnformatted(FontAwesomeIcon.Times.ToIconString());
    //                ImGui.PopFont();

    //            }
    //    int? switchTo;
    //    bool deleteConfirm;

    //    var mouseDown = ImGui.IsMouseDown(ImGuiMouseButton.Left);
    //    var mouseClicked = ImGui.IsMouseReleased(ImGuiMouseButton.Left);
    //    if (ImGui.IsItemHovered() || mouseDown)
    //    {
    //        if (mouseClicked)
    //        {
    //            switchTo = null;

    //            if (deleteConfirm)
    //            {
    //                deleteConfirm = false;
    //                if (selectedIndex == presetIndex)
    //                {
    //                    switchTo = -1;
    //                }

    //                Plugin.RecruitmentDataController.RecruitmentPresets.RemoveAt(presetIndex);
    //            }
    //                                    else
    //                                    {
    //                deleteConfirm = true;
    //            }
    //            }
    //    }
    //    else
    //    {
    //        deleteConfirm = false;
    //    }

    //    if (deleteConfirm)
    //    {
    //        ImGui.BeginTooltip();
    //        ImGui.TextUnformatted("Click delete again to confirm.");
    //        ImGui.EndTooltip();
    //    }
    //}
    //}

}
