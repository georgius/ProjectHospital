using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using ModAdvancedGameChanges.Helpers;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ModAdvancedGameChanges
{
    [HarmonyPatch(typeof(MapEditorController))]
    public static class MapEditorControllerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapEditorController), nameof(MapEditorController.UpdateAfterObject))]
        public static bool UpdateAfterObject(Floor floor, Vector2i position, MapEditorController __instance)
        {
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"position [{position.m_x}, {position.m_y}]");

            return true;
        }
    }

    [HarmonyPatch(typeof(ObjectMover))]
    public static class ObjectMoverPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ObjectMover), nameof(ObjectMover.PickUpObject))]
        public static bool  PickUpObjectPrefix(TileObject tileObject, Floor floor, ObjectMover __instance)
        {
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"object {tileObject?.m_state.m_gameDBObject.Entry.DatabaseID.ToString() ?? "NULL"}");

            return true;
        }
    }


    [HarmonyPatch(typeof(Floor))]
    public static class FloorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Floor), nameof(Floor.RemoveObject))]
        public static bool RemoveObjectPrefix(Vector2i position, bool deleteElevatorsOnOtherFloors, bool doRefund, TileObject topObject, Floor __instance)
        {
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"position [{position.m_x}, {position.m_y}], deleteElevatorsOnOtherFloors {deleteElevatorsOnOtherFloors}, doRefund {doRefund}, object {topObject?.m_state.m_gameDBObject.Entry.DatabaseID.ToString() ?? "NULL"}");

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Floor), nameof(Floor.RemoveObjectsGradually))]
        public static bool RemoveObjectsGraduallyPrefix(Vector2i position, bool deleteElevatorsOnOtherFloors, bool doRefund, Floor __instance)
        {
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"position [{position.m_x}, {position.m_y}], deleteElevatorsOnOtherFloors {deleteElevatorsOnOtherFloors}, doRefund {doRefund}");

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Floor), nameof(Floor.UpdateStaticNavigationData))]
        public static bool UpdateStaticNavigationData(Floor __instance)
        {
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"floor {__instance.m_floorIndex}");

            List<TileObject> elevators = __instance.m_elevators;
            __instance.m_elevators = MapScriptInterface.Instance.FindAllCenterObjectsWithTag(__instance.m_floorIndex, Tags.Vanilla.Elevator);

            GridMap.GetInstance().Recalculate(__instance.m_floorIndex, __instance.m_mapPersistentData.m_tileWalls, __instance.m_mapPersistentData.m_tiles,
                    __instance.m_mapPersistentData.m_mapLogisticsLayer.m_accessRights, __instance.m_roomAccessRights, __instance.m_elevators);

            if (__instance.ElevatorChangedInternal(elevators, __instance.m_elevators))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"elevator changed");

                UndoManager.sm_Instance.DiscardSnapshot(true);
            }

            for (int i = 0; i < __instance.m_size.m_x; i++)
            {
                for (int j = 0; j < __instance.m_size.m_y; j++)
                {
                    __instance.UpdateStaticMovementCostInternal(new Vector2i(i, j));
                    __instance.UpdateStaticLookAheadMovementCostInternal(new Vector2i(i, j));

                    __instance.m_roomAccessRights[i, j] = AccessRights.PEDESTRIAN;

                    if (__instance.m_roomTiles[i, j] != null && __instance.m_roomTiles[i, j].m_roomPersistentData.m_roomType.Entry.AccessRights > AccessRights.PEDESTRIAN)
                    {
                        __instance.m_roomAccessRights[i, j] = __instance.m_roomTiles[i, j].m_roomPersistentData.m_roomType.Entry.AccessRights;
                    }
                }
            }

            return false;
        }

        private static void UpdateStaticMovementCostInternal(this Floor instance, Vector2i position)
        {
            Type type = typeof(Floor);
            MethodInfo methodInfo = type.GetMethod("UpdateStaticMovementCost", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, new object[] { position });
        }

        private static void UpdateStaticLookAheadMovementCostInternal(this Floor instance, Vector2i position)
        {
            Type type = typeof(Floor);
            MethodInfo methodInfo = type.GetMethod("UpdateStaticLookAheadMovementCost", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, new object[] { position });
        }

        private static bool ElevatorChangedInternal(this Floor instance, List<TileObject> originalElevators, List<TileObject> newElevators)
        {
            Type type = typeof(Floor);
            MethodInfo methodInfo = type.GetMethod("ElevatorChanged", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, new object[] { originalElevators, newElevators });
        }
    }

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

            __instance.m_floorGraphs[floor] = null;

            if (__instance.m_floorGraphs[floor] == null)
            {
                __instance.m_floorGraphs[floor] = new Dictionary<AccessRights, RoomGraph>();
                __instance.m_floorGraphs[floor][AccessRights.PATIENT] = new RoomGraph(floor);
                __instance.m_floorGraphs[floor][AccessRights.STAFF_ONLY] = new RoomGraph(floor);
            }

            var elevatorsHelper = new PrivateFieldAccessHelper<GridMap, List<TileObject>>("m_elevators", __instance);
            var elevatorSeedsHelper = new PrivateFieldAccessHelper<GridMap, List<KeyValuePair<Vector2i, Direction>>>("m_elevatorSeeds", __instance);
            
            if (__instance.CallMethod<bool>("ElevatorTilesChanged", elevatorsHelper.Field, elevators))
            {
                elevatorSeedsHelper.Field = __instance.CallMethod<List<KeyValuePair<Vector2i, Direction>>>("CalculateElevatorSeeds", elevators);
            }

            AccessRights[,] combinedAccessRights = __instance.CallMethod<AccessRights[,]>("CombineAccessRights", accessRights, roomAccessRights);
            __instance.m_floorGraphs[floor][AccessRights.PATIENT].Recalculate(tileWalls, tiles, combinedAccessRights, AccessRights.PATIENT, floor > 0, elevatorSeedsHelper.Field);
            __instance.m_floorGraphs[floor][AccessRights.STAFF_ONLY].Recalculate(tileWalls, tiles, combinedAccessRights, AccessRights.STAFF_ONLY, floor > 0, elevatorSeedsHelper.Field);

            return false;
        }
    }
}