using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
namespace ScenariosPlugin;

public class ScenariosPlugin : BasePlugin
{
    public override string ModuleName => "Scenarios Plugin";
    public override string ModuleVersion => "0.0.1";

    private List<ScenarioConfig> _scenarioConfigs = new List<ScenarioConfig>();

    [ConsoleCommand("load_scenario", "Loads a scenario. Must be on the correct map, first.")]
    [CommandHelper(minArgs: 1, usage: "[scenario]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void LoadScenarioCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            Logger.LogWarning("Player was null in LoadScenarioCommand");
            return;
        }

        if (command.ArgCount == 0)
        {
            Logger.LogInformation("No arguments passed to LoadScenarioCommand");
            return;
        }

        // Attempt to find the matching scenario
        string scenarioName = command.ArgByIndex(0);
        ScenarioConfig matchingScenario = _scenarioConfigs.Find(delegate (ScenarioConfig config)
        {
            return config.Name == scenarioName;
        });

        if (matchingScenario == null)
        {
            Server.PrintToConsole(String.Format("Could not find scenario: %s", scenarioName));
            return;
        }

        // TODO: check that we're on the correct map
        // TODO: check if there are sufficient players for configuration
        // TODO: restart round, place players
    }

    public override void Load(bool hotReload)
    {
        // Just hard-code a scenario for now, we'll worry about persistence later
        PlayerPlacement TTopMid;
        TTopMid.Position = new Vector3() { x = 259.18f, y = 12.83f, z = -143.31f };
        TTopMid.Angle = new Vector3() { x = -2.43f, y = -161.99f, z = 0f };
        TTopMid.Team = CsTeam.Terrorist;
        TTopMid.IsRequired = true;

        PlayerPlacement CTWindow;
        CTWindow.Position = new Vector3() { x = -1183.97f, y = -811.89f, z = -103.97f };
        CTWindow.Angle = new Vector3() { x = 0f, y = 15.95f, z = 0f };
        CTWindow.Team = CsTeam.CounterTerrorist;
        CTWindow.IsRequired = true;

        var currentConfig = new ScenarioConfig();
        currentConfig.Name = "Mirage.1v1.WindowVsTopMid";
        currentConfig.Map = Map.Mirage;
        currentConfig.PlayerPlacements.Add(TTopMid);
        currentConfig.PlayerPlacements.Add(CTWindow);

        _scenarioConfigs.Add(currentConfig);
    }
}
