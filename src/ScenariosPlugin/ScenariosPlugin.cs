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
        Active
    }
    public override string ModuleName => "Scenarios Plugin";
    public override string ModuleVersion => "0.0.1";

    private ScenarioState _state = ScenarioState.Inactive;
    private List<ScenarioConfig> _scenarioConfigs = new List<ScenarioConfig>();
    private ScenarioConfig? _currentScenario = null;
    private List<SpawnPlayerAssignment> _spawnPlayerAssignments = new List<SpawnPlayerAssignment>();
    private static Random _random = new Random();

    private void SetState(ScenarioState newState)
    {
        if (_state != newState)
        {
            Logger.LogDebug(String.Format("[Scenarios] Plugin state %s -> %s"), _state.ToString(), newState.ToString());
            _state = newState;
        }
    }

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

        // Randomly assign players to spawns according to team and restart the round

        // First, shuffle the spawns list
        List<SpawnInfo> shuffledSpawnInfos = new List<SpawnInfo>(matchingScenario.SpawnInfos);
        int n = shuffledSpawnInfos.Count;
        while (n > 1)
        {
            n--;
            int k = _random.Next(n + 1);
            (shuffledSpawnInfos[k], shuffledSpawnInfos[n]) = (shuffledSpawnInfos[n], shuffledSpawnInfos[k]);
        }

        // Put us in the "loading" state so listeners behave accordingly
        _currentScenario = matchingScenario;
        SetState(ScenarioState.Active);

        HashSet<string> usedSpawnNames = new HashSet<string>();
        _spawnPlayerAssignments.Clear();
        var playerEntities = Utilities.GetPlayers();
        foreach (var playerEntity in playerEntities) 
        { 
            if (!playerEntity.IsValid)
            {
                continue;
            }

            // Find the next available placement for this team
            bool foundNextSpawnInfo = false;
            SpawnInfo nextSpawnInfo = new SpawnInfo();
            foreach (var playerSpawnInfo in shuffledSpawnInfos)
            {
                if (usedSpawnNames.Contains(playerSpawnInfo.Name) || playerSpawnInfo.Team != playerEntity.Team)
                {
                    continue;
                }
                else
                {
                    foundNextSpawnInfo = true;
                    nextSpawnInfo = playerSpawnInfo;
                    break;
                }
            }

            if (foundNextSpawnInfo)
            {
                if (nextSpawnInfo.Name is null)
                {
                    Logger.LogError("Found placement name is unexpectedly null! Skipping player assignment.");
                    continue;
                }

                usedSpawnNames.Add(nextSpawnInfo.Name);
                _spawnPlayerAssignments.Add(new SpawnPlayerAssignment() { PlayerPlacement = nextSpawnInfo, PlayerController = playerEntity });
            }
        }

        // Restart! Player placements will be realized in the OnEntitySpawned handler
        var baseRulesEntity = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("cs_gamerules").First();
        if (baseRulesEntity is null)
        {
            Logger.LogError("Failed to find base game rules entity. Bailing on scenario load.");
            _spawnPlayerAssignments.Clear();
            SetState(ScenarioState.Inactive);
            _currentScenario = null;
            return;
        }

        var gameRulesEntity = new CCSGameRules(baseRulesEntity.Handle);
        gameRulesEntity.GameRestart = true;
    }

    public override void Load(bool hotReload)
    {
        // Just hard-code a scenario for now, we'll worry about persistence later
        SpawnInfo TTopMid = new SpawnInfo();
        TTopMid.SpawnPoint.Position = new Vector3() { x = 259.18f, y = 12.83f, z = -143.31f };
        TTopMid.SpawnPoint.Angle = new Vector3() { x = -2.43f, y = -161.99f, z = 0f };
        TTopMid.Team = CsTeam.Terrorist;

        SpawnInfo CTWindow = new SpawnInfo();
        CTWindow.SpawnPoint.Position = new Vector3() { x = -1183.97f, y = -811.89f, z = -103.97f };
        CTWindow.SpawnPoint.Angle = new Vector3() { x = 0f, y = 15.95f, z = 0f };
        CTWindow.Team = CsTeam.CounterTerrorist;

        var currentConfig = new ScenarioConfig();
        currentConfig.Name = "Mirage.1v1.WindowVsTopMid";
        currentConfig.MapType = Map.MapType.Mirage;
        currentConfig.SpawnInfos.Add(TTopMid);
        currentConfig.SpawnInfos.Add(CTWindow);

        _scenarioConfigs.Add(currentConfig);
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        Logger.LogDebug("[Scenarios] OnRoundStart++");
        if (_state == ScenarioState.Active)
        {
            if (_currentScenario is null)
            {
                Logger.LogDebug("[Scenarios] Current state is active, but there is no active scenario. Cannot start round, returning to inactive state.");
                SetState(ScenarioState.Inactive);
                return HookResult.Continue;
            }
        }
        Logger.LogDebug("[Scenarios] OnRoundStart--");
        return HookResult.Continue;
    }
}
