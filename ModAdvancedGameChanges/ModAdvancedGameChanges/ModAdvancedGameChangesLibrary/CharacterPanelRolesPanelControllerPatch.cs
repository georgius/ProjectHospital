using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Reflection;

namespace ModAdvancedGameChanges
{
    [HarmonyPatch(typeof(CharacterPanelRolesPanelController))]
    public static class CharacterPanelRolesPanelControllerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterPanelRolesPanelController), nameof(CharacterPanelRolesPanelController.UpdateData))]
        public static bool UpdateDataPrefix(Entity employee, CharacterPanelRolesPanelController __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = employee.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent.GetHomeRoomType();

            if ((homeRoomType != null)
                && (
                    homeRoomType.HasTag(Tags.Mod.DoctorTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace)
                    ))
            {
                __instance.m_rolesSegmentStatic.GetComponent<SegmentController>().SetLayout(0, 2);

                __instance.SetEmployee(employee);
                if (__instance.GetEmployee().GetEntity() != null)
                {
                    __instance.GetEmployee().GetEntity().GetComponent<EmployeeComponent>().m_rolesDirty = false;
                }
                __instance.SetRoleCount(0);
                return false;
            }

            return true;
        }

        private static EntityIDPointer<Entity> GetEmployee(this CharacterPanelRolesPanelController instance)
        {
            // Get the Type of the class
            Type type = typeof(CharacterPanelRolesPanelController);

            // Get the private field using BindingFlags
            FieldInfo m_employeeFieldInfo = type.GetField("m_employee", BindingFlags.NonPublic | BindingFlags.Instance);

            // get objects
            return (EntityIDPointer<Entity>)m_employeeFieldInfo.GetValue(instance);
        }

        private static void SetEmployee(this CharacterPanelRolesPanelController instance, EntityIDPointer<Entity> value)
        {
            // Get the Type of the class
            Type type = typeof(CharacterPanelRolesPanelController);

            // Get the private field using BindingFlags
            FieldInfo m_employeeFieldInfo = type.GetField("m_employee", BindingFlags.NonPublic | BindingFlags.Instance);

            // set objects
            m_employeeFieldInfo.SetValue(instance, value);
        }

        private static int GetRoleCount(this CharacterPanelRolesPanelController instance)
        {
            // Get the Type of the class
            Type type = typeof(CharacterPanelRolesPanelController);

            // Get the private field using BindingFlags
            FieldInfo m_roleCountFieldInfo = type.GetField("m_roleCount", BindingFlags.NonPublic | BindingFlags.Instance);

            // get objects
            return (int)m_roleCountFieldInfo.GetValue(instance);
        }

        private static void SetRoleCount(this CharacterPanelRolesPanelController instance, int value)
        {
            // Get the Type of the class
            Type type = typeof(CharacterPanelRolesPanelController);

            // Get the private field using BindingFlags
            FieldInfo m_roleCountFieldInfo = type.GetField("m_roleCount", BindingFlags.NonPublic | BindingFlags.Instance);

            // set objects
            m_roleCountFieldInfo.SetValue(instance, value);
        }
    }
}
