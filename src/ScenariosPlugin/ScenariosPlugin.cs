using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
namespace ScenariosPlugin;

public class ScenariosPlugin : BasePlugin
{
    enum ScenarioState
    {
        Inactive,
        Loading,
        Active
    }

    public override string ModuleName => "Scenarios Plugin";
    public override string ModuleVersion => "0.0.1";

    private ScenarioState _state = ScenarioState.Inactive;
    private List<ScenarioConfig> _scenarioConfigs = new List<ScenarioConfig>();
    private static Random _random = new Random();

    // TODO: can take 0 args if there is currently an active scenario
    [ConsoleCommand("sce_start", "Loads a scenario. Must be on the correct map, first.")]
    [CommandHelper(minArgs: 1, usage: "[scenario]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void LoadScenarioCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (_state != ScenarioState.Inactive) 
        {
            Server.PrintToConsole(String.Format("Cannot load scenario: Invalid state (%s)", _state.ToString()));
            return;
        }

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

        if (_scenarioConfigs == null || _scenarioConfigs.Count == 0)
        {
            Logger.LogInformation("No valid scenarios available.");
            return;
        }

        // Attempt to find the matching scenario
        string scenarioName = command.ArgByIndex(0);
        ScenarioConfig? matchingScenario = _scenarioConfigs.Find((ScenarioConfig config) =>
        {
            return config.Name == scenarioName;
        });

        if (matchingScenario == null)
        {
            Server.PrintToConsole(String.Format("Could not find scenario: %s", scenarioName));
            return;
        }

        string scenarioMapString = Map.MapTypeToString(matchingScenario.MapType);
        if (Server.MapName != scenarioMapString)
        {
            Server.PrintToConsole(String.Format("Incorrect map for scenario %s. Please switch to %s", scenarioName, scenarioMapString));
            return;
        }

        // Randomly assign players to placements according to team and restart the round

        // First, shuffle the placements list
        List<PlayerPlacement> shuffledPlacements = List(matchingScenario.PlayerPlacements);
        int n = shuffledPlacements.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            PlayerPlacement value = shuffledPlacements[k];
            shuffledPlacements[k] = shuffledPlacements[n];
            shuffledPlacements[n] = value;
        }

        // Put us in the "loading" state before hooking up any listeners
        _state = ScenarioState.Loading;
        HashSet<string> usedPlacements = new HashSet<string>();
        List<PlacementAssignment> placementAssignments = new List<PlacementAssignment>(); // TODO: This needs to be a member var
        var playerEntities = Utilities.GetPlayers();
        foreach (var playerEntity in playerEntities) 
        { 
            if (!playerEntity.IsValid)
            {
                continue;
            }

            // Find the next available placement for this team
            bool foundNextPlacement = false;
            PlayerPlacement nextPlacement = new PlayerPlacement();
            foreach (var playerPlacement in shuffledPlacements)
            {
                if (usedPlacements.Contains(playerPlacement.Name) || playerPlacement.Team != playerEntity.Team)
                {
                    continue;
                }
                else
                {
                    foundNextPlacement = true;
                    nextPlacement = playerPlacement;
                    break;
                }
            }

            if (foundNextPlacement)
            {
                if (nextPlacement.Name is null)
                {
                    Logger.LogError("Found placement name is unexpectedly null! Skipping player assignment.");
                    continue;
                }

                usedPlacements.Add(nextPlacement.Name);
                placementAssignments.Add(new PlacementAssignment() { PlayerPlacement = nextPlacement, PlayerController = playerEntity });
            }
        }

        // Restart! Player placements will be realized in the OnEntitySpawned handler
        var baseRulesEntity = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("cs_gamerules").First();
        if (baseRulesEntity is null)
        {
            Logger.LogError("Failed to find base game rules entity. Bailing on scenario load.");
            _state = ScenarioState.Inactive;
            return;
        }

        var gameRulesEntity = new CCSGameRules(baseRulesEntity.Handle);
        gameRulesEntity.GameRestart = true;
    }

    private List<T> List<T>(List<T> playerPlacements)
    {
        throw new NotImplementedException();
    }

    public override void Load(bool hotReload)
    {
        // Just hard-code a scenario for now, we'll worry about persistence later
        PlayerPlacement TTopMid = new PlayerPlacement();
        TTopMid.Position = new Vector3() { x = 259.18f, y = 12.83f, z = -143.31f };
        TTopMid.Angle = new Vector3() { x = -2.43f, y = -161.99f, z = 0f };
        TTopMid.Team = CsTeam.Terrorist;
        TTopMid.IsRequired = true;

        PlayerPlacement CTWindow = new PlayerPlacement();
        CTWindow.Position = new Vector3() { x = -1183.97f, y = -811.89f, z = -103.97f };
        CTWindow.Angle = new Vector3() { x = 0f, y = 15.95f, z = 0f };
        CTWindow.Team = CsTeam.CounterTerrorist;
        CTWindow.IsRequired = true;

        var currentConfig = new ScenarioConfig();
        currentConfig.Name = "Mirage.1v1.WindowVsTopMid";
        currentConfig.MapType = Map.MapType.Mirage;
        currentConfig.PlayerPlacements.Add(TTopMid);
        currentConfig.PlayerPlacements.Add(CTWindow);

        _scenarioConfigs.Add(currentConfig);

        RegisterListener<Listeners.OnEntitySpawned>(entity =>
        {
            if (_state == ScenarioState.Loading && entity.DesignerName == "cs_player_controller")
            {
                // TODO: find matching player and assign to a slot
            }
        });
    }
}
