using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Reflection;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(InsuranceCompany))]
    public static class InsuranceCompanyPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InsuranceCompany), "AddGeneratedPatient")]
        public static bool AddGeneratedPatientPrefix(int index, int patientCounter, PatientMobility mobility, bool smoothDistribution, InsuranceCompany __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_patientsThroughEmergency[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            if (mobility == PatientMobility.MOBILE)
            {
                Floor groundFloor = Hospital.Instance.GetGroundFloor();
                Vector2i randomSpawnPosition = MapScriptInterface.Instance.GetRandomSpawnPosition();
                PatientSpawnSlot patientSpawnSlot = __instance.m_patientsSpawnedOutpatients[index];
                Entity entity = LopitalEntityFactory.CreateCharacterPatient(groundFloor, randomSpawnPosition, mobility);
                BehaviorPatient component = entity.GetComponent<BehaviorPatient>();

                if (component.m_state.m_medicalCondition.HasImmobileSymptom())
                {
                    UnityEngine.Debug.LogWarning("Newly spawned patient should be mobile! " + component.m_state.m_medicalCondition.m_gameDBMedicalCondition.Entry.DatabaseID);
                }

                Department emergencyDepartment = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Emergency));
                entity.GetComponent<BehaviorPatient>().m_state.m_fVisitTime = __instance.GetVisitTimeInternal((float)index, (float)patientCounter, smoothDistribution, emergencyDepartment);

                entity.GetComponent<BehaviorPatient>().m_state.m_patientState = PatientState.Spawned;
                entity.GetComponent<CharacterPersonalInfoComponent>().m_personalInfo.m_insuranceCompany = __instance.m_gameDBInsuranceCompany.Entry;
                patientSpawnSlot.m_patient = entity;
                groundFloor.AddCharacter(entity);

                return false;
            }

            return true;
        }

        private static float GetVisitTimeInternal(this InsuranceCompany instance, float patientIndex, float totalPatients, bool smoothDistribution, Department department)
        {
            Type type = typeof(InsuranceCompany);
            MethodInfo methodInfo = type.GetMethod("GetVisitTime", BindingFlags.NonPublic | BindingFlags.Instance);

            return (float)methodInfo.Invoke(instance, new object[] { patientIndex, totalPatients, smoothDistribution, department });
        }
    }
}
