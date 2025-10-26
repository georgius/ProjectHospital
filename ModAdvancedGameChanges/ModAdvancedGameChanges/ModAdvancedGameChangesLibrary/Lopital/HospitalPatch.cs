using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using ModGameChanges;
using System.Globalization;
using System.Reflection;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(Hospital))]
    public static class HospitalPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Hospital), nameof(Hospital.CreateNew))]
        public static bool CreateNewPrefix()
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patching hospital");

            if (ViewSettingsPatch.m_staffShiftsEqual[SettingsManager.Instance.m_viewSettings].m_value)
            {
                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Making staff shifts equal");

                GameDBSchedule staffDayShift = Database.Instance.GetEntry<GameDBSchedule>(Constants.Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF);
                GameDBSchedule staffNightShift = Database.Instance.GetEntry<GameDBSchedule>(Constants.Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);
                GameDBSchedule staffMorningChange = Database.Instance.GetEntry<GameDBSchedule>(Constants.Schedules.Vanilla.SCHEDULE_SHIFT_CHANGE_MORNING);
                GameDBSchedule staffEveningChange = Database.Instance.GetEntry<GameDBSchedule>(Constants.Schedules.Vanilla.SCHEDULE_SHIFT_CHANGE_EVENING);

                // we need that staff day shift starts at least at 3h and at most 20h
                // each shift will be 12h
                float staffDayShiftStart = UnityEngine.Mathf.Min(20f, UnityEngine.Mathf.Max(3f, staffDayShift.StartTime));
                float staffDayShiftEnd = staffDayShiftStart + 12f;
                if (staffDayShiftEnd > 24f)
                {
                    staffDayShiftEnd -= 24f;
                }

                HospitalPatch.SetScheduleTimes(staffDayShift, staffDayShiftStart, staffDayShiftEnd);
                HospitalPatch.SetScheduleTimes(staffNightShift, staffDayShiftEnd, staffDayShiftStart);

                // morning and evening shift change should be 0.5 hour around shuft start time
                HospitalPatch.SetScheduleTimes(staffMorningChange, staffDayShift.StartTime - 0.5f, staffDayShift.StartTime + 0.5f);
                HospitalPatch.SetScheduleTimes(staffEveningChange, staffNightShift.StartTime - 0.5f, staffNightShift.StartTime + 0.5f);
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Hospital), nameof(Hospital.CheckDepartmentJanitorBoss))]
        public static bool CheckDepartmentJanitorBossPrefix(Entity janitor, Hospital __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Department department = janitor.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();
            Department departmentOfType = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Emergency));

            if (department.m_departmentPersistentData.m_chiefDoctor != null)
            {
                janitor.GetComponent<EmployeeComponent>().m_state.m_supervisor = department.m_departmentPersistentData.m_chiefDoctor;
            }
            else if (departmentOfType.m_departmentPersistentData.m_chiefDoctor != null)
            {
                janitor.GetComponent<EmployeeComponent>().m_state.m_supervisor = departmentOfType.m_departmentPersistentData.m_chiefDoctor;
            }

            return false;
        }

        private static void SetScheduleTimes(GameDBSchedule schedule, float startTime, float endTime)
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Setting schedule {schedule.DatabaseID}, start time {startTime.ToString(CultureInfo.InvariantCulture)}, end time {endTime.ToString(CultureInfo.InvariantCulture)}");

            var scheduleType = typeof(GameDBSchedule);

            var startTimeProperty = scheduleType.GetProperty(nameof(GameDBSchedule.StartTime), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var startTimeSetMethod = startTimeProperty.GetSetMethod(true);

            var endTimeProperty = scheduleType.GetProperty(nameof(GameDBSchedule.EndTime), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var endTimeSetMethod = endTimeProperty.GetSetMethod(true);

            startTimeSetMethod.Invoke(schedule, new object[] { startTime });
            endTimeSetMethod.Invoke(schedule, new object[] { endTime });
        }
    }
}
