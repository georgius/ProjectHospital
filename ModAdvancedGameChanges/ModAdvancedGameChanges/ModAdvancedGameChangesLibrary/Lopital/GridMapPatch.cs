using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Helpers;
using System;
using System.Collections.Generic;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(GridMap))]
    public static class GridMapPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(GridMap), nameof(GridMap.Recalculate))]
        public static bool Recalculate(int floor, TileWalls[,] tileWalls, Tile[,] tiles, AccessRights[,] accessRights, AccessRights[,] roomAccessRights, List<TileObject> elevators, GridMap __instance)
        {
            if (floor >= __instance.m_floorGraphs.Length)
            {
                Array.Resize<Dictionary<AccessRights, RoomGraph>>(ref __instance.m_floorGraphs, floor + 1);
            }

            if (__instance.m_floorGraphs[floor] == null)
            {
                __instance.m_floorGraphs[floor] = new Dictionary<AccessRights, RoomGraph>();
                __instance.m_floorGraphs[floor][AccessRights.PATIENT] = new RoomGraph(floor);
                __instance.m_floorGraphs[floor][AccessRights.STAFF_ONLY] = new RoomGraph(floor);
            }

            var elevatorsHelper = new PrivateFieldAccessHelper<GridMap, List<TileObject>>("m_elevators", __instance);
            var elevatorSeedsHelper = new PrivateFieldAccessHelper<GridMap, List<KeyValuePair<Vector2i, Direction>>>("m_elevatorSeeds", __instance);

            if (__instance.ElevatorTilesChanged(elevatorsHelper.Field, elevators))
            {
                elevatorSeedsHelper.Field = __instance.CalculateElevatorSeeds(elevators);
            }

            AccessRights[,] combinedAccessRights = __instance.CombineAccessRights(accessRights, roomAccessRights);
            __instance.m_floorGraphs[floor][AccessRights.PATIENT].Recalculate(tileWalls, tiles, combinedAccessRights, AccessRights.PATIENT, floor > 0, elevatorSeedsHelper.Field);
            __instance.m_floorGraphs[floor][AccessRights.STAFF_ONLY].Recalculate(tileWalls, tiles, combinedAccessRights, AccessRights.STAFF_ONLY, floor > 0, elevatorSeedsHelper.Field);

            return false;
        }
    }

    public static class GridMapExtensions
    {
        public static List<KeyValuePair<Vector2i, Direction>> CalculateElevatorSeeds(this GridMap instance, List<TileObject> elevators)
        {
            return MethodAccessHelper.CallMethod<List<KeyValuePair<Vector2i, Direction>>>(instance, "CalculateElevatorSeeds", elevators);
        }

        public static AccessRights[,] CombineAccessRights(this GridMap instance, AccessRights[,] accessRights, AccessRights[,] roomAccessRights)
        {
            return MethodAccessHelper.CallMethod<AccessRights[,]>(instance, "CombineAccessRights", accessRights, roomAccessRights);
        }

        public static bool ElevatorTilesChanged(this GridMap instance, List<TileObject> originalTiles, List<TileObject> newTiles)
        {
            return MethodAccessHelper.CallMethod<bool>(instance, "ElevatorTilesChanged", originalTiles, newTiles);
        }
    }
}
