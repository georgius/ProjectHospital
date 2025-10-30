using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System.Collections.Generic;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(Behavior))]
    public static class BehaviorPatch
    {
        public static Vector3i GetCommonRoomFreePlace(Behavior instance)
        {
            GameDBRoomType commonRoomType = Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.CommonRoom);
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            List<Room> commonRooms = MapScriptInterface.Instance.FindValidRoomsWithType(commonRoomType, employeeComponent.m_state.m_department.GetEntity());
            int minDistance = int.MaxValue;
            Vector3i selectedVector = Vector3i.ZERO_VECTOR;

            foreach (var commonRoom in commonRooms)
            {
                Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(commonRoom, AccessRights.STAFF);
                if (position != Vector2i.ZERO_VECTOR)
                {
                    Vector3i vector = new Vector3i(position.m_x, position.m_y, commonRoom.GetFloorIndex());
                    int distance = (vector - selectedVector).LengthSquared();

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        selectedVector = vector;
                    }
                }
            }

            if (selectedVector != Vector3i.ZERO_VECTOR)
            {
                return selectedVector;
            }

            // no free space in any common room in department
            foreach (var department in Hospital.Instance.m_departments)
            {
                List<Room> departmentCommonRooms = MapScriptInterface.Instance.FindValidRoomsWithType(commonRoomType, department);

                foreach (var commonRoom in departmentCommonRooms)
                {
                    Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(commonRoom, AccessRights.STAFF);
                    if (position != Vector2i.ZERO_VECTOR)
                    {
                        Vector3i vector = new Vector3i(position.m_x, position.m_y, commonRoom.GetFloorIndex());
                        int distance = (vector - selectedVector).LengthSquared();

                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            selectedVector = vector;
                        }
                    }
                }
            }

            return selectedVector;
        }
    }
}
