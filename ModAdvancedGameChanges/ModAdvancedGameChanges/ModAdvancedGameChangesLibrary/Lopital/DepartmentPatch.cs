using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Linq;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(Department))]
    public static class DepartmentPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Department), nameof(Department.GetAverageSatisfactionPatients))]
        public static bool GetAverageSatisfactionPatientsPrefix(Department __instance, ref float __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            int totalSatisfaction = 0;
            int count = 0;

            foreach (EntityIDPointer<Entity> entityIDPointer in __instance.m_departmentPersistentData.m_patients)
            {
                if (__instance.m_departmentPersistentData.m_departmentType != Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Pathology)
                    && !entityIDPointer.GetEntity().GetComponent<BehaviorPatient>().IsHidden())
                {
                    totalSatisfaction += entityIDPointer.GetEntity().GetComponent<MoodComponent>().GetTotalSatisfaction();
                    count++;
                }
            }

            __result = (count == 0)
                ? __instance.m_departmentPersistentData.m_todaysStatistics.m_averageSatisfactionPatients
                : (float)totalSatisfaction / (100f * (float)count);

            return false;
        }
    }
}
