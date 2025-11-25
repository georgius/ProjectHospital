using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureComponent))]
    public static class ProcedureComponentPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureComponent), nameof(ProcedureComponent.SelectExaminationForMedicalCondition))]
        public static bool SelectExaminationForMedicalConditionPrefix(MedicalCondition medicalCondition, Department department, Room room, bool byPriority, ProcedureComponent __instance, ref GameDBExamination __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __result = null;

            __instance.UpdateAllExaminationsForMedicalCondition(medicalCondition, -1, false);
            if (medicalCondition.m_possibleDiagnoses.Count == 0)
            {
                if (medicalCondition.GetNumberOfUncoveredSymptoms() > 0)
                {
                    __instance.m_entity.LogWarning("Invalid symptoms, they don't match any diagnosis!");
                }

                return false;
            }

            var possibleExaminations = medicalCondition.m_possibleDiagnoses
                .Where(pd => pd.m_diagnosis.Entry.DepartmentRef.Entry == department.GetDepartmentType())
                .SelectMany(pd => pd.m_diagnosis.Entry.Examinations.Select(ex => ex.Entry));

            possibleExaminations = possibleExaminations.Any() ? possibleExaminations : __instance.m_examinationAvailability.m_keys;

            List<GameDBExamination> examinations = byPriority ? __instance.m_examinationAvailability.m_keys.OrderByDescending(e => e.Procedure.Priority).ToList() : __instance.m_examinationAvailability.m_keys;

            __result = examinations
                // filter only possible examinations
                .Where(e => possibleExaminations.Contains(e))
                // check alternative examinations
                // like USG and FAST
                .Where(e => ((e.AlternativeExaminationRef == null)
                    || (!__instance.m_state.m_procedureQueue.HasPlannedExamination(e.AlternativeExaminationRef.Entry)
                        && (!__instance.m_state.m_procedureQueue.HasFinishedExamination(e.AlternativeExaminationRef.Entry) || __instance.m_state.m_procedureQueue.HadSurgery()))))
                // check availability
                .Where(e => ((__instance.m_examinationAvailability[e] == ProcedureSceneAvailability.AVAILABLE)
                    || (__instance.m_examinationAvailability[e] == ProcedureSceneAvailability.STAFF_BUSY)
                    || (__instance.m_examinationAvailability[e] == ProcedureSceneAvailability.EQUIPMENT_BUSY)))
                // order by availability
                .OrderBy(e => __instance.m_examinationAvailability[e])
                // take first
                .FirstOrDefault();

            // if __result is still null, then no examination is available

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity?.Name ?? "NULL"}, select examination for medical condition {medicalCondition?.m_gameDBMedicalCondition.Entry.DatabaseID.ToString() ?? "NULL"}, by priority {byPriority}, selected {__result?.DatabaseID.ToString() ?? "NULL"}");

            return false;
        }

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

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, starting procedure {procedure.DatabaseID}, script {procedure.ProcedureScript}");

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

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{patient?.Name ?? "NULL"}, starting procedure {procedure.DatabaseID}, script {procedure.ProcedureScript}");

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

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{patient?.Name ?? "NULL"}, starting procedure {procedure.DatabaseID}, script {procedure.ProcedureScript}");

            ProcedureScript procedureScript = LopitalEntityFactory.InstantiateProcedureScript(procedure);
            procedureScript.Init(200U);

            ProcedureScene procedureScene = ProcedureSceneFactory.CreateProcedureScene(procedure, patient, doctor, nurse, labSpecialist, accessRights, null, ProcedureSceneType.INSTANTIATION, equipmentListRules);
            procedureScript.m_stateData.m_procedureScene = procedureScene;
            __instance.m_state.m_currentProcedureScript = procedureScript;
            __instance.m_state.m_currentProcedureScript.GetEntity().m_stateData.m_procedure = procedure;
            procedureScript.Activate();

            return false;
        }
    }
}
