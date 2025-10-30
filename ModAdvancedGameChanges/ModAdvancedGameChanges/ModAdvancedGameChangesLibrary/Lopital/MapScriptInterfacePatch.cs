using GLib;
using HarmonyLib;
using Lopital;
using ModGameChanges;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(MapScriptInterface))]
    public static class MapScriptInterfacePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapScriptInterface), nameof(MapScriptInterface.FindClosestDirtyTileInARoom))]
        public static bool FindClosestDirtyTileInARoomPrefix(Room room, Vector2i characterPosition, MapScriptInterface __instance, ref Vector2i __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            float dirtLevel = 0f;
            float minDistance = float.MaxValue;
            __result = Vector2i.ZERO_VECTOR;
            Floor floor = Hospital.Instance.m_floors[room.GetFloorIndex()];

            for (int i = room.m_roomPersistentData.m_positionBottom.m_x; i <= room.m_roomPersistentData.m_positionTop.m_x; i++)
            {
                for (int j = room.m_roomPersistentData.m_positionBottom.m_y; j <= room.m_roomPersistentData.m_positionTop.m_y; j++)
                {
                    if (room.IsPositionInRoom(new Vector2i(i, j)) && floor.m_mapPersistentData.m_tiles[i, j].m_dirtType == DirtType.BLOOD && floor.m_mapPersistentData.m_tiles[i, j].m_dirtLevel > dirtLevel && floor.m_roomTiles[i, j] != null && floor.m_mapPersistentData.m_tiles[i, j].m_user == null && floor.m_tileObjects[i, j].GetAllObjects().Count == 0 && floor.m_accessibility[i, j] != 2)
                    {
                        float taxiDistanceCost = Pathfinder.getTaxiDistanceCost(characterPosition, new Vector2i(i, j));
                        if (taxiDistanceCost < minDistance)
                        {
                            dirtLevel = floor.m_mapPersistentData.m_tiles[i, j].m_dirtLevel;
                            minDistance = taxiDistanceCost;
                            __result = new Vector2i(i, j);
                        }
                    }
                }
            }

            if (dirtLevel > 0f)
            {
                return false;
            }

            for (int i = room.m_roomPersistentData.m_positionBottom.m_x; i <= room.m_roomPersistentData.m_positionTop.m_x; i++)
            {
                for (int j = room.m_roomPersistentData.m_positionBottom.m_y; j <= room.m_roomPersistentData.m_positionTop.m_y; j++)
                {
                    if (room.IsPositionInRoom(new Vector2i(i, j)) && floor.m_mapPersistentData.m_tiles[i, j].m_dirtLevel > 10f && floor.m_mapPersistentData.m_tiles[i, j].m_dirtLevel > dirtLevel && floor.m_roomTiles[i, j] != null && floor.m_mapPersistentData.m_tiles[i, j].m_user == null && floor.m_tileObjects[i, j].GetAllObjects().Count == 0 && floor.m_accessibility[i, j] != 2)
                    {
                        float taxiDistanceCost2 = Pathfinder.getTaxiDistanceCost(characterPosition, new Vector2i(i, j));
                        if (taxiDistanceCost2 < minDistance)
                        {
                            dirtLevel = floor.m_mapPersistentData.m_tiles[i, j].m_dirtLevel;
                            minDistance = taxiDistanceCost2;
                            __result = new Vector2i(i, j);
                        }
                    }
                }
            }

            return false;
        }

        public static bool FindDirtiestTileInARoom(Room room, bool bloodOnly, int threshold, MapScriptInterface __instance, ref Vector2i __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            float dirtLevel = 0f;
            __result = Vector2i.ZERO_VECTOR;
            Floor floor = Hospital.Instance.m_floors[room.GetFloorIndex()];

            for (int i = room.m_roomPersistentData.m_positionBottom.m_x; i <= room.m_roomPersistentData.m_positionTop.m_x; i++)
            {
                for (int j = room.m_roomPersistentData.m_positionBottom.m_y; j <= room.m_roomPersistentData.m_positionTop.m_y; j++)
                {
                    if (room.IsPositionInRoom(new Vector2i(i, j)) && floor.m_mapPersistentData.m_tiles[i, j].m_dirtType == DirtType.BLOOD && floor.m_mapPersistentData.m_tiles[i, j].m_dirtLevel > dirtLevel && floor.m_roomTiles[i, j] != null && floor.m_mapPersistentData.m_tiles[i, j].m_user == null && floor.m_tileObjects[i, j].GetAllObjects().Count == 0 && floor.m_accessibility[i, j] != 2)
                    {
                        dirtLevel = floor.m_mapPersistentData.m_tiles[i, j].m_dirtLevel;
                        __result = new Vector2i(i, j);
                    }
                }
            }

            if (dirtLevel > 0f)
            {
                return false;
            }
            if (bloodOnly)
            {
                __result = Vector2i.ZERO_VECTOR;
                return false;
            }

            for (int i = room.m_roomPersistentData.m_positionBottom.m_x; i <= room.m_roomPersistentData.m_positionTop.m_x; i++)
            {
                for (int j = room.m_roomPersistentData.m_positionBottom.m_y; j <= room.m_roomPersistentData.m_positionTop.m_y; j++)
                {
                    if (room.IsPositionInRoom(new Vector2i(i, j)) && floor.m_mapPersistentData.m_tiles[i, j].m_dirtLevel > (float)threshold && floor.m_mapPersistentData.m_tiles[i, j].m_dirtLevel > dirtLevel && floor.m_roomTiles[i, j] != null && floor.m_mapPersistentData.m_tiles[i, j].m_user == null && floor.m_tileObjects[i, j].GetAllObjects().Count == 0 && floor.m_accessibility[i, j] != 2)
                    {
                        dirtLevel = floor.m_mapPersistentData.m_tiles[i, j].m_dirtLevel;
                        __result = new Vector2i(i, j);
                    }
                }
            }

            return false;
        }
    }
}
