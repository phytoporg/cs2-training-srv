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

    public struct PlayerPlacement
    {
        // The name of this placement "i.e. topmid_close"
        public string Name;

        // The 3D position of a player on the target map
        public Vector3 Position;

        // The player's angle/look vector
        public Vector3 Angle;

        // Team assignment for the player
        public CsTeam Team;

        // Must this placement be filled by a player for a scenario, or can it be empty?
        public bool IsRequired;

        // TODO: initial weapons state
        // TODO: initial utility state
        // TODO: win condition(s)
    }

    public struct PlacementAssignment
    {
        public PlayerPlacement PlayerPlacement;
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
        public List<PlayerPlacement> PlayerPlacements { get; set; }

        public ScenarioConfig()
        {
            Name = "Unnamed";
            MapType = Map.MapType.Invalid;
            PlayerPlacements = new List<PlayerPlacement>();
            TimeLimitSeconds = -1;
        }
    }
}
