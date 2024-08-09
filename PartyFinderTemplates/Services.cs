using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace PartyFinderTemplates
{
    public sealed class Services
    {
        public static void Init(IDalamudPluginInterface pluginInterface) => pluginInterface.Create<Services>();

        [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; } = null!;
        [PluginService] public static ITextureProvider TextureProvider { get; set; } = null!;
        [PluginService] public static ICommandManager CommandManager { get; set; } = null!;
        [PluginService] public static IGameGui GameGui { get; set; } = null!;
        [PluginService] public static IGameInteropProvider GameInteropProvider { get; set; } = null!;
        [PluginService] public static IPluginLog PluginLog { get; set; } = null!;
        [PluginService] public static IFramework Framework { get; set; } = null!;
        [PluginService] public static IAddonLifecycle AddonLifecycle { get; set; } = null!;
    }
}
