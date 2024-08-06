using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using PartyFinderTemplates.Windows  ;

namespace PartyFinderTemplates;

public sealed class Plugin : IDalamudPlugin
{
    public static Configuration Configuration = null!;
    public readonly WindowSystem WindowSystem = new("PartyFinderTemplates");
    public readonly ConfigWindow ConfigWindow;
    public readonly MainWindow UI;
    public readonly GameFunctions GameFunctions;
    public readonly Commands Commands;

    //private unsafe AddonLookingForGroupDetail
    //    AddonLookingForGroupCondition* AddonNamePlate => (AddonNamePlate*)Services.GameGui.GetAddonByName("NamePlate");

    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Services.Init(pluginInterface);

        Configuration = Configuration.Load();
        Commands = new Commands(this);

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        UI = new MainWindow(this);
        WindowSystem.AddWindow(UI);

        GameFunctions = new GameFunctions();

        Services.PluginInterface.UiBuilder.Draw += DrawUI;
    }

       public void Dispose()
    {
        Commands.Dispose();
        WindowSystem.RemoveAllWindows();
        Services.PluginInterface.UiBuilder.Draw -= DrawUI;

        ConfigWindow.Dispose();
        UI.Dispose();

        GameFunctions.Dispose();
    }

    private void DrawUI() => WindowSystem.Draw();
}
