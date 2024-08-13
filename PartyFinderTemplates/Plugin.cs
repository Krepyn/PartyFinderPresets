using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using PartyFinderTemplates.Windows;
using PartyFinderTemplates.Classes;
using PartyFinderTemplates.Controllers;

namespace PartyFinderTemplates;

public sealed class Plugin : IDalamudPlugin
{
    public static Configuration Configuration = null!;
    public readonly GameFunctions GameFunctions;
    public readonly Commands Commands;
    public readonly WindowSystem WindowSystem = new("PartyFinderTemplates");
    public RecruitmentDataController RecruitmentDataController;

    public readonly ConfigWindow ConfigWindow;
    public readonly MainWindow UI;
#if DEBUG
    public readonly DebugWindow DebugWindow;
#endif


    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Services.Init(pluginInterface);

        //Systems
        Configuration = Configuration.Load();
        GameFunctions = new GameFunctions(this);
        Commands = new Commands(this);
        RecruitmentDataController = new RecruitmentDataController();

        //Windows
        ConfigWindow = new ConfigWindow(this);
        UI = new MainWindow(this);
        WindowSystem.AddWindow(UI);
        WindowSystem.AddWindow(ConfigWindow);

#if DEBUG
        DebugWindow = new DebugWindow(this);
        WindowSystem.AddWindow(DebugWindow);
#endif

        Services.PluginInterface.UiBuilder.Draw += DrawUI;
    }

    public void Dispose()
    {
        Commands.Dispose();
        GameFunctions.Dispose();

        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        UI.Dispose();
#if DEBUG
        DebugWindow.Dispose();
#endif
        Services.PluginInterface.UiBuilder.Draw -= DrawUI;
    }

    private void DrawUI() => WindowSystem.Draw();
}
