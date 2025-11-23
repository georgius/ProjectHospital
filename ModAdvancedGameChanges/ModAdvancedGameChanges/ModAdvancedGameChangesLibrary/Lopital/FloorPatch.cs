using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using ModAdvancedGameChanges.Helpers;
using System.Collections.Generic;

namespace ModAdvancedGameChanges
{
    [HarmonyPatch(typeof(Floor))]
    public static class FloorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Floor), nameof(Floor.UpdateStaticNavigationData))]
        public static bool UpdateStaticNavigationData(Floor __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            List<TileObject> elevators = __instance.m_elevators;
            __instance.m_elevators = MapScriptInterface.Instance.FindAllCenterObjectsWithTag(__instance.m_floorIndex, Tags.Vanilla.Elevator);

            for (int i = 0; i < __instance.m_size.m_x; i++)
            {
                for (int j = 0; j < __instance.m_size.m_y; j++)
                {
                    __instance.m_roomAccessRights[i, j] = AccessRights.PEDESTRIAN;

                    if (__instance.m_roomTiles[i, j] != null && __instance.m_roomTiles[i, j].m_roomPersistentData.m_roomType.Entry.AccessRights > AccessRights.PEDESTRIAN)
                    {
                        __instance.m_roomAccessRights[i, j] = __instance.m_roomTiles[i, j].m_roomPersistentData.m_roomType.Entry.AccessRights;
                    }
                }
            }

            GridMap.GetInstance().Recalculate(__instance.m_floorIndex, __instance.m_mapPersistentData.m_tileWalls, __instance.m_mapPersistentData.m_tiles,
                    __instance.m_mapPersistentData.m_mapLogisticsLayer.m_accessRights, __instance.m_roomAccessRights, __instance.m_elevators);

            if (__instance.ElevatorChanged(elevators, __instance.m_elevators))
            {
                UndoManager.sm_Instance.DiscardSnapshot(true);
            }

            for (int i = 0; i < __instance.m_size.m_x; i++)
            {
                for (int j = 0; j < __instance.m_size.m_y; j++)
                {
                    __instance.UpdateStaticMovementCost(new Vector2i(i, j));
                    __instance.UpdateStaticLookAheadMovementCost(new Vector2i(i, j));
                }
            }

            return false;
        }
    }

    public static class FloorExtensions
    {
        public static void UpdateStaticMovementCost(this Floor instance, Vector2i position)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStaticMovementCost", position);
        }

        public static void UpdateStaticLookAheadMovementCost(this Floor instance, Vector2i position)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStaticLookAheadMovementCost", position);
        }

        public static bool ElevatorChanged(this Floor instance, List<TileObject> originalElevators, List<TileObject> newElevators)
        {
            return MethodAccessHelper.CallMethod<bool>(instance, "ElevatorChanged", originalElevators, newElevators);
        }
    }
}