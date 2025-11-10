using GLib;
using HarmonyLib;
using Lopital;
using System;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureComponent))]
    public static class ProcedureComponentPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureComponent), nameof(ProcedureComponent.StartProcedure))]
        [HarmonyPatch(new Type[] { typeof(GameDBProcedure), typeof(Entity), typeof(Department), typeof(AccessRights), typeof(EquipmentListRules) })]
        public static bool StartProcedurePrefix1(GameDBProcedure procedure, Entity mainCharacter, Department department, AccessRights accessRights, EquipmentListRules equipmentListRules, ProcedureComponent __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            ProcedureScript procedureScript = LopitalEntityFactory.InstantiateProcedureScript(procedure);
            procedureScript.Init(200U);

            ProcedureScene procedureScene = ProcedureSceneFactory.CreateProcedureScene(procedure, mainCharacter, department, null, accessRights, ProcedureSceneType.INSTANTIATION, equipmentListRules, StaffSelectionRules.IGNORE);
            procedureScript.m_stateData.m_procedureScene = procedureScene;
            __instance.m_state.m_currentProcedureScript = procedureScript;
            __instance.m_state.m_currentProcedureScript.GetEntity().m_stateData.m_procedure = procedure;
            procedureScript.Activate();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureComponent), nameof(ProcedureComponent.StartProcedure))]
        [HarmonyPatch(new Type[] { typeof(GameDBProcedure), typeof(Entity), typeof(Department), typeof(Room), typeof(AccessRights), typeof(EquipmentListRules) })]
        public static bool StartProcedurePrefix2(GameDBProcedure procedure, Entity patient, Department department, Room room, AccessRights accessRights, EquipmentListRules equipmentListRules, ProcedureComponent __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            ProcedureScript procedureScript = LopitalEntityFactory.InstantiateProcedureScript(procedure);
            procedureScript.Init(200U);

            ProcedureScene procedureScene = ProcedureSceneFactory.CreateProcedureScene(procedure, patient, department, room, accessRights, ProcedureSceneType.INSTANTIATION, equipmentListRules, StaffSelectionRules.IGNORE);
            procedureScript.m_stateData.m_procedureScene = procedureScene;
            __instance.m_state.m_currentProcedureScript = procedureScript;
            __instance.m_state.m_currentProcedureScript.GetEntity().m_stateData.m_procedure = procedure;
            procedureScript.Activate();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureComponent), nameof(ProcedureComponent.StartProcedure))]
        [HarmonyPatch(new Type[] { typeof(GameDBProcedure), typeof(Entity), typeof(Entity), typeof(Entity), typeof(Entity), typeof(AccessRights), typeof(EquipmentListRules) })]
        public static bool StartProcedurePrefix3(GameDBProcedure procedure, Entity patient, Entity doctor, Entity nurse, Entity labSpecialist, AccessRights accessRights, EquipmentListRules equipmentListRules, ProcedureComponent __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            ProcedureScript procedureScript = LopitalEntityFactory.InstantiateProcedureScript(procedure);
            procedureScript.Init(200U);

            ProcedureScene procedureScene = ProcedureSceneFactory.CreateProcedureScene(procedure, patient, doctor, nurse, labSpecialist, accessRights, null, ProcedureSceneType.INSTANTIATION, equipmentListRules);
            procedureScript.m_stateData.m_procedureScene = procedureScene;
            __instance.m_state.m_currentProcedureScript = procedureScript;
            procedureScript.Activate();

            return false;
        }
    }
}
