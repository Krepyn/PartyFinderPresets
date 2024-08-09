using System;
using System.Collections.Generic;
using Dalamud.Game.Command;

namespace PartyFinderTemplates
{
    public sealed class Commands : IDisposable
    {
        private static readonly Dictionary<string, string> CommandNames = new()
        {
            ["/pft"] = "Toggles the UI",
#if DEBUG
            ["/pftd"] = "Debug UI"
#endif
        };

        private Plugin Plugin { get; }

        public Commands (Plugin plugin)
        {
            this.Plugin = plugin;

            foreach (var (command, help) in CommandNames)
            {
                Services.CommandManager.AddHandler(command, new CommandInfo(this.OnCommand) {
                    HelpMessage = help,
                });
            }
        }

        private void OnCommand(String command, string args)
        {
            Services.PluginLog.Verbose($"Received Command: {command}, Args: {args}");


            if (command.Equals("/pft"))
                this.Plugin.UI.Toggle();
#if DEBUG
            else if (command.Equals("/pftd"))
                this.Plugin.DebugWindow.Toggle();
#endif
        }

        public void Dispose()
        {
            foreach (var (command, _) in CommandNames)
            {
                Services.CommandManager.RemoveHandler(command);
            }
        }
    }
}
