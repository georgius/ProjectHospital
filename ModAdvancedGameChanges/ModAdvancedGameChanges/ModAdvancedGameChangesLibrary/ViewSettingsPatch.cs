using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using HarmonyLib;
using ModAdvancedGameChanges;

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
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_staffShiftsEqual = new Dictionary<ViewSettings, GenericFlag<bool>>();

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

                    ViewSettingsPatch.m_debug.Add(__instance, new GenericFlag<bool>("AGC_OPTION_DEBUG", false));
                    ViewSettingsPatch.m_enableNonLinearSkillLeveling.Add(__instance, new GenericFlag<bool>("AGC_OPTION_ENABLE_NON_LINEAR_SKILL_LEVELING", true));
                    ViewSettingsPatch.m_limitClinicDoctorsLevel.Add(__instance, new GenericFlag<bool>("AGC_OPTION_LIMIT_CLINIC_DOCTORS_LEVEL", true));
                    ViewSettingsPatch.m_forceEmployeeLowestHireLevel.Add(__instance, new GenericFlag<bool>("AGC_OPTION_FORCE_EMPLOYEE_LOWEST_HIRE_LEVEL", true));
                    ViewSettingsPatch.m_staffShiftsEqual.Add(__instance, new GenericFlag<bool>("AGC_OPTION_STAFF_SHIFTS_EQUAL", true));

                    var boolFlags = new List<GenericFlag<bool>>(__instance.m_allBoolFlags);

                    boolFlags.Add(ViewSettingsPatch.m_debug[__instance]);
                    boolFlags.Add(ViewSettingsPatch.m_enableNonLinearSkillLeveling[__instance]);
                    boolFlags.Add(ViewSettingsPatch.m_limitClinicDoctorsLevel[__instance]);
                    boolFlags.Add(ViewSettingsPatch.m_forceEmployeeLowestHireLevel[__instance]);
                    boolFlags.Add(ViewSettingsPatch.m_staffShiftsEqual[__instance]);

                    __instance.m_allBoolFlags = boolFlags.ToArray();
                }

                // fix schedules
                GameDBSchedule[] schedules = Database.Instance.GetEntries<GameDBSchedule>();
                foreach (var schedule in schedules)
                {
                    ViewSettingsPatch.FixScheduleTimes(schedule);
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

        internal static void FixScheduleTimes(GameDBSchedule schedule)
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Fixing schedule {schedule.DatabaseID}");

            var scheduleType = typeof(GameDBSchedule);

            var startTimeProperty = scheduleType.GetProperty(nameof(GameDBSchedule.StartTime), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var startTimeSetMethod = startTimeProperty.GetSetMethod(true);

            var endTimeProperty = scheduleType.GetProperty(nameof(GameDBSchedule.EndTime), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var endTimeSetMethod = endTimeProperty.GetSetMethod(true);

            // start time have to be 0 .. 24
            if (schedule.StartTime < 0f)
            {
                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Fixing schedule {schedule.DatabaseID} start time from {schedule.StartTime.ToString(CultureInfo.InvariantCulture)} to 0.");

                startTimeSetMethod.Invoke(schedule, new object[] { 0f });
            }
            if (schedule.StartTime > 24f)
            {
                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Fixing schedule {schedule.DatabaseID} start time from {schedule.EndTime.ToString(CultureInfo.InvariantCulture)} to 24.");

                startTimeSetMethod.Invoke(schedule, new object[] { 24f });
            }

            // end time have to be 0 .. 24
            if (schedule.EndTime < 0f)
            {
                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Fixing schedule {schedule.DatabaseID} end time from {schedule.EndTime.ToString(CultureInfo.InvariantCulture)} to 0.");

                endTimeSetMethod.Invoke(schedule, new object[] { 0f });
            }
            if (schedule.EndTime > 24f)
            {
                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Fixing schedule {schedule.DatabaseID} end time from {schedule.EndTime.ToString(CultureInfo.InvariantCulture)} to 24.");

                endTimeSetMethod.Invoke(schedule, new object[] { 24f });
            }
        }
    }
}
