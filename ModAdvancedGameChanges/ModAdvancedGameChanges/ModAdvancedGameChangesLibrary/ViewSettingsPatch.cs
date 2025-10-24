using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using HarmonyLib;

namespace ModGameChanges
{
    [HarmonyPatch(typeof(ViewSettings))]
    public static class ViewSettingsPatch
    {
        public static bool m_enabled = false;
        public static bool m_enabledTrainingDepartment = true;

        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_debug = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_enableNonLinearSkillLeveling = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_limitClinicDoctorsLevel = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_forceEmployeeLowestHireLevel = new Dictionary<ViewSettings, GenericFlag<bool>>();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ViewSettings), nameof(ViewSettings.Load))]
        public static bool LoadPrefix(ViewSettings __instance)
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Start");

            try
            {
                Tweakable.CheckConfiguration();

                ViewSettingsPatch.m_enabled = true;

                if (!ViewSettingsPatch.m_enableNonLinearSkillLeveling.ContainsKey(__instance))
                {
                    Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Adding settings");

                    ViewSettingsPatch.m_debug.Add(__instance, new GenericFlag<bool>("AGC_DEBUG", false));
                    ViewSettingsPatch.m_enableNonLinearSkillLeveling.Add(__instance, new GenericFlag<bool>("AGC_ENABLE_NON_LINEAR_SKILL_LEVELING", true));
                    ViewSettingsPatch.m_limitClinicDoctorsLevel.Add(__instance, new GenericFlag<bool>("AGC_LIMIT_CLINIC_DOCTORS_LEVEL", true));
                    ViewSettingsPatch.m_forceEmployeeLowestHireLevel.Add(__instance, new GenericFlag<bool>("AGC_FORCE_EMPLOYEE_LOWEST_HIRE_LEVEL", true));

                    var boolFlags = new List<GenericFlag<bool>>(__instance.m_allBoolFlags);

                    boolFlags.Add(ViewSettingsPatch.m_debug[__instance]);
                    boolFlags.Add(ViewSettingsPatch.m_enableNonLinearSkillLeveling[__instance]);
                    boolFlags.Add(ViewSettingsPatch.m_limitClinicDoctorsLevel[__instance]);
                    boolFlags.Add(ViewSettingsPatch.m_forceEmployeeLowestHireLevel[__instance]);

                    __instance.m_allBoolFlags = boolFlags.ToArray();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(System.Reflection.MethodBase.GetCurrentMethod(), "", ex);
            }

            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "End");

            // Allow original method to run
            return true;
        }
    }
}
