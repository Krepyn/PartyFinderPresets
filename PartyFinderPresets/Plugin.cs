using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using PartyFinderPresets.Windows;
using PartyFinderPresets.Classes;
using PartyFinderPresets.Controllers;

namespace PartyFinderPresets;

public sealed class Plugin : IDalamudPlugin
{
    public static Configuration Configuration = null!;
    public readonly GameFunctions GameFunctions;
    public readonly Commands Commands;
    public readonly WindowSystem WindowSystem = new("PartyFinderTemplates");
    public RecruitmentDataController RecruitmentDataController;

    public readonly ConfigWindow ConfigWindow;
    public readonly MainWindow MainWindow;
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
        RecruitmentDataController = new RecruitmentDataController(this);

        //Windows
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(ConfigWindow);

#if DEBUG
        DebugWindow = new DebugWindow(this);
        WindowSystem.AddWindow(DebugWindow);
        DebugWindow.IsOpen = true;
#endif

        Services.PluginInterface.UiBuilder.Draw += DrawUI;
    }

    public void Dispose()
    {
        Commands.Dispose();
        GameFunctions.Dispose();
        RecruitmentDataController.Dispose();

        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        MainWindow.Dispose();
#if DEBUG
        DebugWindow.Dispose();
#endif
        Services.PluginInterface.UiBuilder.Draw -= DrawUI;
    }

    private void DrawUI() => WindowSystem.Draw();
}
