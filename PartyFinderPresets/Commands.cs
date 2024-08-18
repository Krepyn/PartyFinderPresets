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
            ["/pftc"] = "Toggles the Config",
#if DEBUG
            ["/pftd"] = "Debug UI"
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

            if (command.Equals("/pftc"))
                this.Plugin.ConfigWindow.Toggle();
#if DEBUG
            else if (command.Equals("/pftd"))
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
