using System;
using System.Collections.Generic;
using Dalamud.Game.Command;

namespace PartyFinderTemplates
{
    public sealed class Commands : IDisposable
    {
        private static readonly Dictionary<string, string> CommandNames = new()
        {
            ["/pmycommand"] = "A useful message to display in /xlhelp",
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
            this.Plugin.UI.Toggle();
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
