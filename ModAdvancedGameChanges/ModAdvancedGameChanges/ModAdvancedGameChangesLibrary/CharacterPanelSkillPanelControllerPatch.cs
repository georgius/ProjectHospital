using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using ModGameChanges;

namespace ModAdvancedGameChanges
{
    [HarmonyPatch(typeof(CharacterPanelSkillPanelController))]
    public static class CharacterPanelSkillPanelControllerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterPanelSkillPanelController), "IsTrainingEnabled")]
        public static bool IsTrainingEnabledPrefix(CharacterPanelSkillPanelController __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            __result = ViewSettingsPatch.m_enabledTrainingDepartment || Tweakable.Vanilla.DlcHospitalServicesEnabled();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterPanelSkillPanelController), "IsTrainingRoomAvailable")]
        public static bool IsTrainingRoomAvailablePrefix(CharacterPanelSkillPanelController __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            __result = MapScriptInterface.Instance.FindValidRoomWithTags(
                new string[] { Tags.Vanilla.Classroom }, 
                MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.AdministrativeDepartment))) != null;

            __result |= MapScriptInterface.Instance.FindValidRoomWithTags(
                new string[] { Tags.Vanilla.Classroom },
                MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Mod.TrainingDepartment))) != null;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterPanelSkillPanelController), "IsTrainingRoomFree")]
        public static bool IsTrainingRoomFreePrefix(Entity employee, CharacterPanelSkillPanelController __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            GameDBProcedure staffTraining = Database.Instance.GetEntry<GameDBProcedure>(Procedures.Vanilla.StaffTraining);
            Department administrativeDepartment = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.AdministrativeDepartment));
            Department trainingDepartment = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Mod.TrainingDepartment));

            __result = employee.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(staffTraining, employee, administrativeDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE) == ProcedureSceneAvailability.AVAILABLE;
            __result |= employee.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(staffTraining, employee, trainingDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE) == ProcedureSceneAvailability.AVAILABLE;

            return false;
        }
    }
}
