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
        [HarmonyPatch(typeof(Floor), nameof(Floor.OnDayStart))]
        public static bool OnDayStartPrefix(Floor __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"floor {__instance.m_floorIndex}, on day start");

            int pedestrians = 0;
            foreach (Entity entity in Hospital.Instance.m_characters)
            {
                entity.GetComponent<EmployeeComponent>()?.OnDayStart();

                if (entity.GetComponent<BehaviorPedestrian>() != null)
                {
                    entity.GetComponent<BehaviorPedestrian>().m_state.m_respawnTimeSeconds = DayTime.Instance.IngameTimeHoursToRealTimeSeconds(UnityEngine.Random.Range(0.5f, 2f));
                    pedestrians++;
                }
            }

            if (__instance.m_floorIndex == 0)
            {
                for (int i = pedestrians; i < Tweakable.Mod.PedestrianCount(); i++)
                {
                    var entity = LopitalEntityFactory.CreateCharacterPedestrian(__instance, new Vector2i(0, 2));
                    entity.GetComponent<BehaviorPedestrian>().m_state.m_respawnTimeSeconds = DayTime.Instance.IngameTimeHoursToRealTimeSeconds(UnityEngine.Random.Range(0.5f, 2f));
                    __instance.AddCharacter(entity);
                }

                // remove pedestrians over allowed count (if possible)
                if (pedestrians > Tweakable.Mod.PedestrianCount())
                {
                    int j = 0;
                    while (j < Hospital.Instance.m_characters.Count)
                    {
                        Entity entity = Hospital.Instance.m_characters[j];

                        if (entity.GetComponent<BehaviorPedestrian>() != null)
                        {
                            switch (entity.GetComponent<BehaviorPedestrian>().m_state.m_state)
                            {
                                case BehaviorPedestrianState.Uninitialized:
                                case BehaviorPedestrianState.Idle:
                                    // safe to remove
                                    Hospital.Instance.m_characters.RemoveAt(j);
                                    break;
                                case BehaviorPedestrianState.Walking:
                                case BehaviorPedestrianState.IdleAtDestination:
                                case BehaviorPedestrianState.WalkingBack:
                                    // unsafe to remove, skip
                                    j++;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            j++;
                        }
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Floor), nameof(Floor.UpdateStaticNavigationData))]
        public static bool UpdateStaticNavigationData(Floor __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
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