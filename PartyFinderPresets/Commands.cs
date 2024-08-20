using System;
using System.Collections.Generic;
using Dalamud.Game.Command;

namespace PartyFinderPresets
{
    public sealed class Commands : IDisposable
    {
        private Plugin Plugin;
        private static readonly Dictionary<string, string> CommandNames = new()
        {
            ["/pfpc"] = "Toggles the Config",
            ["/pfpm"] = "Toggles Preset Menu",
#if DEBUG
            ["/pfpd"] = "Debug UI"
#endif
        };

        public Commands (Plugin plugin)
        {
            this.Plugin = plugin;

            foreach (var (command, help) in CommandNames)
                Services.CommandManager.AddHandler(command, new CommandInfo(this.OnCommand) {
                    HelpMessage = help,
                });
        }

        private void OnCommand(String command, string args)
        {
            Services.PluginLog.Verbose($"Received Command: {command}, Args: {args}");

            if(command.Equals("/pfpc"))
                this.Plugin.ConfigWindow.Toggle();
            else if(command.Equals("/pfpm"))
                Plugin.Configuration.PresetsDockVisible = !Plugin.Configuration.PresetsDockVisible;
#if DEBUG
            else if(command.Equals("/pfpd"))
                this.Plugin.DebugWindow.Toggle();
#endif
        }

        public void Dispose()
        {
            foreach (var (command, _) in CommandNames)
                Services.CommandManager.RemoveHandler(command);
        }
    }
}
