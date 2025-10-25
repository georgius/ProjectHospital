using HarmonyLib;
using Lopital;
using ModGameChanges;
using System;
using System.Collections;
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

            bool dlcHospitalServices = Tweakable.Vanilla.DlcHospitalServicesEnabled();

            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patching hospital");

            if (ViewSettingsPatch.m_staffShiftsEqual[SettingsManager.Instance.m_viewSettings].m_value)
            {
                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Making staff shifts equal");

                GameDBSchedule staffDayShift = Database.Instance.GetEntry<GameDBSchedule>(Constants.Schedule.Vanilla.SCHEDULE_OPENING_HOURS_STAFF);
                GameDBSchedule staffNightShift = Database.Instance.GetEntry<GameDBSchedule>(Constants.Schedule.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);
                GameDBSchedule staffMorningChange = Database.Instance.GetEntry<GameDBSchedule>(Constants.Schedule.Vanilla.SCHEDULE_SHIFT_CHANGE_MORNING);
                GameDBSchedule staffEveningChange = Database.Instance.GetEntry<GameDBSchedule>(Constants.Schedule.Vanilla.SCHEDULE_SHIFT_CHANGE_EVENING);

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

            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"DLC Hospital services present - {dlcHospitalServices}");

            if (!dlcHospitalServices)
            {
                ViewSettingsPatch.m_enabledTrainingDepartment = false;

                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Disabling training department");

                // Get the Type of the class
                Type type = typeof(Database);

                // Get the private field using BindingFlags
                FieldInfo fieldInfo = type.GetField("tables", BindingFlags.NonPublic | BindingFlags.Instance);

                // cast to IDictionary since we can’t use the internal generic type
                var tables = (IDictionary)fieldInfo.GetValue(Database.Instance);

                foreach (DictionaryEntry tableEntry in tables)
                {
                    if (tableEntry.Key == typeof(GameDBDepartment))
                    {
                        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Found game departments");

                        // tableEntry.Value is a DatabaseTable (internal) — treat it as IDictionary
                        var dbTable = (IDictionary)tableEntry.Value;

                        foreach (DictionaryEntry entry in dbTable)
                        {
                            if (entry.Key.ToString() == Constants.Departments.Mod.TrainingDepartment)
                            {
                                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Found training department ({Constants.Departments.Mod.TrainingDepartment}), removing");

                                dbTable.Remove(entry.Key);
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            return true;
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
