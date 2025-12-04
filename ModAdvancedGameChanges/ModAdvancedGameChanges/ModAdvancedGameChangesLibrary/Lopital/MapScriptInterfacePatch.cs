using GLib;
using HarmonyLib;
using Lopital;
using System;
using System.Collections.Generic;

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
                // allow original method to run
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapScriptInterface), nameof(MapScriptInterface.FindClosestFreeObjectWithTags))]
        [HarmonyPatch(new Type[] { typeof(Entity), typeof(Entity), typeof(Vector2i), typeof(Room), typeof(string[]), typeof(AccessRights), typeof(bool), typeof(DatabaseEntryRef<GameDBRoomType>[]), typeof(bool) })]
        public static bool FindClosestFreeObjectWithTagsPrefix(Entity character, Entity owner, Vector2i position, Room room, string[] tags, AccessRights accessRights, bool allowObjectsWithAttachments, DatabaseEntryRef<GameDBRoomType>[] roomTypes, bool allowedOutsideOfRoom, MapScriptInterface __instance, ref TileObject __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            float distance = float.MaxValue;
            __result = null;

            if (room == null)
            {
                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Looking for object in NULL room!");
                return false;
            }

            Floor floor = Hospital.Instance.m_floors[room.GetFloorIndex()];

            for (int i = room.m_roomPersistentData.m_positionBottom.m_x; i <= room.m_roomPersistentData.m_positionTop.m_x; i++)
            {
                for (int j = room.m_roomPersistentData.m_positionBottom.m_y; j <= room.m_roomPersistentData.m_positionTop.m_y; j++)
                {
                    if (room.IsPositionInRoom(new Vector2i(i, j)) 
                        && ((floor.m_mapPersistentData.m_mapLogisticsLayer.m_accessRights[i, j] <= accessRights) 
                            || ((floor.m_mapPersistentData.m_mapLogisticsLayer.m_accessRights[i, j] == AccessRights.BIOHAZARD) && (accessRights == AccessRights.PATIENT_PROCEDURE))
                            || ((accessRights == AccessRights.PATIENT) && (floor.m_accessibilityPatients[i, j] != 2))))
                    {
                        TileObjects tileObjects = floor.m_tileObjects[i, j];
                        Room roomAt = floor.m_roomTiles[i, j];

                        bool validRoom = (roomAt == null);
                        validRoom |= (roomAt?.m_roomPersistentData.m_valid == RoomValidity.OK);
                        validRoom |= (roomAt?.m_roomPersistentData.m_valid == RoomValidity.MISSING_STAFF);
                        validRoom |= (roomAt?.m_roomPersistentData.m_valid == RoomValidity.DEPARTMENT_CLOSED);
                        validRoom |= ((accessRights >= AccessRights.STAFF) && (roomAt?.m_roomPersistentData.m_valid == RoomValidity.INACCESSIBLE_PATIENTS));

                        if ((character.GetComponent<HospitalizationComponent>() != null) 
                            && character.GetComponent<HospitalizationComponent>().IsHospitalized() 
                            && (roomAt != null)
                            && roomAt.m_roomPersistentData.m_roomType.Entry.AcceptsOutpatients)
                        {
                            validRoom = false;
                        }

                        bool evaluate = (roomTypes == null) || (allowedOutsideOfRoom && (roomAt == null));

                        if ((!evaluate) && validRoom && (roomTypes != null) && (roomAt != null))
                        {
                            foreach (DatabaseEntryRef<GameDBRoomType> databaseEntryRef in roomTypes)
                            {
                                if (databaseEntryRef.Entry == roomAt.m_roomPersistentData.m_roomType.Entry)
                                {
                                    evaluate = true;
                                }
                            }
                        }

                        if (evaluate && validRoom)
                        {
                            if ((tileObjects.m_centerObject != null) 
                                && tileObjects.m_centerObject.HasAllTags(tags) 
                                && ((tileObjects.m_centerObject.User == null) || (tileObjects.m_centerObject.User == character)) 
                                && ((tileObjects.m_centerObject.Owner == null) || (tileObjects.m_centerObject.Owner == owner)) 
                                && (!tileObjects.m_centerObject.IsBroken()) 
                                && tileObjects.m_centerObject.IsValid() 
                                && (allowObjectsWithAttachments || (tileObjects.m_attachmentObject == null)))
                            {
                                if ((tileObjects.m_centerObject.m_state.m_compositeParent.GetEntity() != null) 
                                    && tileObjects.m_centerObject.m_state.m_compositeParent.GetEntity().HasUnusablePart())
                                {
                                    continue;
                                }

                                float tempDistance = GridMap.GetInstance().GetDistance(
                                    character.GetComponent<WalkComponent>().GetFloorIndex(), 
                                    character.GetComponent<WalkComponent>().GetCurrentTile(), 
                                    room.GetFloorIndex(), 
                                    new Vector2i(i, j), 
                                    accessRights);

                                if ((tempDistance >= 0f) && (tempDistance < distance))
                                {
                                    __result = tileObjects.m_centerObject;
                                    distance = tempDistance;
                                }
                            }

                            foreach (TileObject tileObject in tileObjects.GetAllNonCenterObjects())
                            {
                                if ((tileObject != null) 
                                    && tileObject.HasAllTags(tags) 
                                    && ((tileObject.User == null) || (tileObject.User == character)) 
                                    && ((tileObject.Owner == null) || (tileObject.Owner == owner)) 
                                    && (!tileObject.IsBroken()) 
                                    && tileObject.IsValid())
                                {
                                    float tempDistance = GridMap.GetInstance().GetDistance(
                                        character.GetComponent<WalkComponent>().GetFloorIndex(), 
                                        character.GetComponent<WalkComponent>().GetCurrentTile(), 
                                        room.GetFloorIndex(), 
                                        new Vector2i(i, j), 
                                        accessRights);

                                    if ((tempDistance >= 0f) && (tempDistance < distance))
                                    {
                                        __result = tileObject;
                                        distance = tempDistance;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapScriptInterface), nameof(MapScriptInterface.FindClosestFreeObjectWithTagsAndRoomTags))]
        public static bool FindClosestFreeObjectWithTagsAndRoomTagsPrefix(Vector2i position, int floorIndex, Department department, string[] tags, AccessRights accessRights, string[] roomTags, MapScriptInterface __instance, ref TileObject __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            int distance = int.MaxValue;
            __result = null;

            foreach (EntityIDPointer<TileObject> entityIDPointer in department.m_departmentPersistentData.m_objects)
            {
                TileObject entity = entityIDPointer.GetEntity();
                Floor floor = Hospital.Instance.m_floors[entity.GetFloorIndex()];

                if (floor.m_mapPersistentData.m_mapLogisticsLayer.m_accessRights[entity.m_state.m_position.m_x, entity.m_state.m_position.m_y] <= accessRights)
                {
                    int tempFloorDistance = Math.Abs(floorIndex - entity.GetFloorIndex()) * 100;
                    int tempDistance = (entity.m_state.m_position.m_x - position.m_x) * (entity.m_state.m_position.m_x - position.m_x) + (entity.m_state.m_position.m_y - position.m_y) * (entity.m_state.m_position.m_y - position.m_y) + tempFloorDistance * tempFloorDistance;

                    if (entity.HasAllTags(tags) && (entity.User == null) && (entity.Owner == null) && (!entity.IsBroken()) && (tempDistance < distance) && entity.IsValid())
                    {
                        Room roomAt = __instance.GetRoomAt(entity.m_state.m_position, entity.GetFloorIndex());

                        bool validRoom = (roomAt == null);
                        validRoom |= (roomAt?.m_roomPersistentData.m_valid == RoomValidity.OK);
                        validRoom |= (roomAt?.m_roomPersistentData.m_valid == RoomValidity.MISSING_STAFF);
                        validRoom |= (roomAt?.m_roomPersistentData.m_valid == RoomValidity.DEPARTMENT_CLOSED);
                        validRoom |= ((accessRights >= AccessRights.STAFF) && (roomAt?.m_roomPersistentData.m_valid == RoomValidity.INACCESSIBLE_PATIENTS));

                        AccessRights roomAccessRights = roomAt?.m_roomPersistentData.m_roomType.Entry.AccessRights ?? accessRights;

                        if (validRoom
                            && (roomAccessRights <= accessRights)
                            && ((roomTags == null) || ((roomTags != null) && (roomAt != null) && roomAt.m_roomPersistentData.m_roomType.Entry.HasAnyTag(roomTags))))
                        {
                            __result = entity;
                            distance = tempDistance;
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
                // allow original method to run
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

        public static Vector3i GetRandomFreePlaceInRoomTypeInDepartment(WalkComponent walkComponent, string roomType, Department department, AccessRights accessRights)
        {
            GameDBRoomType gameRoomType = Database.Instance.GetEntry<GameDBRoomType>(roomType);
            if (gameRoomType == null)
            {
                return Vector3i.ZERO_VECTOR;
            }

            Vector3i instancePosition = new Vector3i(walkComponent.GetCurrentTile().m_x, walkComponent.GetCurrentTile().m_y, walkComponent.GetFloorIndex());

            List<Room> departmentRooms = MapScriptInterface.Instance.FindValidRoomsWithType(gameRoomType, department);
            int minDistance = int.MaxValue;
            Vector3i selectedPosition = Vector3i.ZERO_VECTOR;

            foreach (var departmentRoom in departmentRooms)
            {
                Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(departmentRoom, accessRights);
                if (position != Vector2i.ZERO_VECTOR)
                {
                    Vector3i roomPosition = new Vector3i(position.m_x, position.m_y, departmentRoom.GetFloorIndex());
                    int distance = (roomPosition - instancePosition).LengthSquaredWithPenalty();

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        selectedPosition = roomPosition;
                    }
                }
            }

            return selectedPosition;
        }

        public static Vector3i GetRandomFreePlaceInRoomTypeAnywhere(WalkComponent walkComponent, string roomType, AccessRights accessRights)
        {
            GameDBRoomType gameRoomType = Database.Instance.GetEntry<GameDBRoomType>(roomType);
            if (gameRoomType == null)
            {
                return Vector3i.ZERO_VECTOR;
            }

            Vector3i instancePosition = new Vector3i(walkComponent.GetCurrentTile().m_x, walkComponent.GetCurrentTile().m_y, walkComponent.GetFloorIndex());

            int minDistance = int.MaxValue;
            Vector3i selectedPosition = Vector3i.ZERO_VECTOR;

            foreach (var department in Hospital.Instance.m_departments)
            {
                List<Room> departmentRooms = MapScriptInterface.Instance.FindValidRoomsWithType(gameRoomType, department);

                foreach (var departmentRoom in departmentRooms)
                {
                    Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(departmentRoom, accessRights);
                    if (position != Vector2i.ZERO_VECTOR)
                    {
                        Vector3i roomPosition = new Vector3i(position.m_x, position.m_y, departmentRoom.GetFloorIndex());
                        int distance = (roomPosition - instancePosition).LengthSquaredWithPenalty();

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            selectedPosition = roomPosition;
                        }
                    }
                }
            }

            return selectedPosition;
        }

        public static Vector3i GetRandomFreePlaceInRoomTypePreferDepartment(WalkComponent walkComponent, string roomType, Department department, AccessRights accessRights)
        {
            Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypeInDepartment(walkComponent, roomType, department, accessRights);
            position = (position != Vector3i.ZERO_VECTOR) ? position : MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypeAnywhere(walkComponent, roomType, accessRights);

            return position;
        }

        public static Vector3i GetRandomFreePlaceInRoomTypePreferDepartment(Behavior instance, string roomType, Department department, AccessRights accessRights)
        {
            return MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypePreferDepartment(instance.GetComponent<WalkComponent>(), roomType, department, accessRights);
        }

        public static List<Entity> FindNursesAssignedToRoom(Room room, bool hasToBeFree, Department department, GameDBEmployeeRole role, GameDBSkill skill)
        {
            List<Entity> result = new List<Entity>();

            List<Department> departments = new List<Department>();
            if (department != null)
            {
                departments.Add(department);
            }
            else
            {
                departments.AddRange(Hospital.Instance.m_departments);
            }

            foreach (Department queryDepartment in departments)
            {
                List<EntityIDPointer<Entity>> nurses = queryDepartment.m_departmentPersistentData.m_nurses;

                foreach (EntityIDPointer<Entity> nursePointer in nurses)
                {
                    Entity nurse = nursePointer.GetEntity();
                    BehaviorNurse behavior = nurse.GetComponent<BehaviorNurse>();
                    EmployeeComponent employee = nurse.GetComponent<EmployeeComponent>();

                    if ((employee != null)
                        && (DayTime.Instance.GetShift() == employee.m_state.m_shift)
                        && (employee.m_state.m_homeRoom != null)
                        && (!employee.IsFired())
                        && ((room == null) || (employee.m_state.m_homeRoom.GetEntity() == room))
                        && ((department == null) || (employee.m_state.m_department.GetEntity() == department))
                        && ((role == null) || employee.HasRole(role))
                        && ((skill == null) || employee.m_state.m_skillSet.HasSkill(skill))
                        && ((!hasToBeFree) || behavior.IsFree()))
                    {
                        result.Add(nurse);
                    }
                }
            }

            return result;
        }


        public static bool IsInDestinationRoom(Entity entity)
        {
            Room currentRoom = MapScriptInterface.Instance.GetRoomAt(entity.GetComponent<WalkComponent>());
            Room destinationRoom = MapScriptInterface.Instance.GetRoomAt(entity.GetComponent<WalkComponent>().GetDestinationTile(), entity.GetComponent<WalkComponent>().m_state.m_destinationFloor);

            return ((currentRoom != null) && (destinationRoom != null) && (currentRoom == destinationRoom));
        }
    }
}