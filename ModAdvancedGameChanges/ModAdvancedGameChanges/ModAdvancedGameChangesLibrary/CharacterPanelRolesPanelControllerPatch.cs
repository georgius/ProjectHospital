using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using ModAdvancedGameChanges.Helpers;

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

                var employeeHelper = new PrivateFieldAccessHelper<CharacterPanelRolesPanelController, EntityIDPointer<Entity>>("m_employee", __instance);
                var roleCountHelper = new PrivateFieldAccessHelper<CharacterPanelRolesPanelController, int>("m_roleCount", __instance);

                employeeHelper.Field = employee;
                if (employeeHelper.Field.GetEntity() != null)
                {
                    employeeHelper.Field.GetEntity().GetComponent<EmployeeComponent>().m_rolesDirty = false;
                }

                roleCountHelper.Field = 0;
                return false;
            }

            return true;
        }
    }
}
