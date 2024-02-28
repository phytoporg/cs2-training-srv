using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using System.Collections.Generic;

namespace ScenariosPlugin
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    public struct SpawnPoint
    {
        // The 3D position of a player on the target map
        public Vector3 Position;

        // The player's angle/look vector
        public Vector3 Angle;
    }

    public struct SpawnInfo
    {
        // The name of this spawn "i.e. topmid_close"
        public string Name;

        // Team assignment for the player
        public CsTeam Team;

        // Spawn location and orientation
        public SpawnPoint SpawnPoint;

        // TODO: initial weapons state
        // TODO: initial utility state
        // TODO: win condition(s)
    }

    public struct SpawnPlayerAssignment
    {
        public SpawnInfo PlayerPlacement;
        public CCSPlayerController? PlayerController;
    }

    public class ScenarioConfig
    {
        // The name of this configuration
        public string Name { get; set; }

        // The map for this scenario
        public Map.MapType MapType { get; set; }

        // Time limit for the scenario, or -1 if there is no limit
        public int TimeLimitSeconds { get; set; }

        // A list of initial player placements
        public List<SpawnInfo> SpawnInfos { get; set; }

        public ScenarioConfig()
        {
            Name = "Unnamed";
            MapType = Map.MapType.Invalid;
            SpawnInfos = new List<SpawnInfo>();
            TimeLimitSeconds = -1;
        }
    }
}
