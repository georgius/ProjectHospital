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
        public static void ChooseSkillToTrainAndToggleTraining(Behavior instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            List<Skill> skills = new List<Skill>();

            if (employeeComponent.m_state.m_skillSet.m_qualifications != null)
            {
                foreach (var skill in employeeComponent.m_state.m_skillSet.m_qualifications)
                {
                    if (skill.m_level < Skills.SkillLevelMaximum)
                    {
                        skills.Add(skill);
                    }
                }
            }
            if (employeeComponent.m_state.m_skillSet.m_specialization1 != null)
            {
                if (employeeComponent.m_state.m_skillSet.m_specialization1.m_level < Skills.SkillLevelMaximum)
                {
                    skills.Add(employeeComponent.m_state.m_skillSet.m_specialization1);
                }
            }
            if (employeeComponent.m_state.m_skillSet.m_specialization2 != null)
            {
                if (employeeComponent.m_state.m_skillSet.m_specialization2.m_level < Skills.SkillLevelMaximum)
                {
                    skills.Add(employeeComponent.m_state.m_skillSet.m_specialization2);
                }
            }

            if (skills.Count != 0)
            {
                Skill skillToTrain = skills[UnityEngine.Random.Range(0, skills.Count)];

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, training skill {skillToTrain.m_gameDBSkill.Entry.DatabaseID}");
                employeeComponent.ToggleTraining(skillToTrain);
            }
        }

        public static Vector3i GetCommonRoomFreePlace(Behavior instance)
        {
            GameDBRoomType commonRoomType = Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.CommonRoom);
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();
            WalkComponent walkComponent = instance.GetComponent<WalkComponent>();

            Vector3i instancePosition = new Vector3i(walkComponent.GetCurrentTile().m_x, walkComponent.GetCurrentTile().m_y, walkComponent.GetFloorIndex());

            List<Room> commonRooms = MapScriptInterface.Instance.FindValidRoomsWithType(commonRoomType, employeeComponent.m_state.m_department.GetEntity());
            int minDistance = int.MaxValue;
            Vector3i selectedPosition = Vector3i.ZERO_VECTOR;

            foreach (var commonRoom in commonRooms)
            {
                Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(commonRoom, AccessRights.STAFF);
                if (position != Vector2i.ZERO_VECTOR)
                {
                    Vector3i roomPosition = new Vector3i(position.m_x, position.m_y, commonRoom.GetFloorIndex());
                    int distance = (roomPosition - instancePosition).LengthSquared();

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        selectedPosition = roomPosition;
                    }
                }
            }

            if (selectedPosition != Vector3i.ZERO_VECTOR)
            {
                return selectedPosition;
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
                        Vector3i roomPosition = new Vector3i(position.m_x, position.m_y, commonRoom.GetFloorIndex());
                        int distance = (roomPosition - instancePosition).LengthSquared();

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
    }
}
