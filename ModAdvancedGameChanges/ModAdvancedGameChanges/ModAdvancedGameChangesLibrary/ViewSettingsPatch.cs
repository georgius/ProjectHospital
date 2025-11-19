using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using HarmonyLib;
using ModAdvancedGameChanges.Constants;

namespace ModAdvancedGameChanges 
{
    [HarmonyPatch(typeof(ViewSettings))]
    public static class ViewSettingsPatch
    {
        public static bool m_enabled = false;
        public static bool m_enabledTrainingDepartment = true;

        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_debug = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_enableNonLinearSkillLeveling = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_forceEmployeeLowestHireLevel = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_labEmployeeBiochemistry = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_limitClinicDoctorsLevel = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_patientsThroughEmergency = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_staffLunchNight = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_staffShiftsEqual = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_trainingDepartment = new Dictionary<ViewSettings, GenericFlag<bool>>();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ViewSettings), nameof(ViewSettings.Load))]
        public static bool LoadPrefix(ViewSettings __instance)
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Start");

            try
            {
                Tweakable.CheckConfiguration();

                ViewSettingsPatch.m_enabled = AdvancedGameChanges.m_enabled;

                if (ViewSettingsPatch.m_enabled)
                {
                    if (!ViewSettingsPatch.m_enableNonLinearSkillLeveling.ContainsKey(__instance))
                    {
                        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Adding settings");

                        ViewSettingsPatch.m_debug.Add(__instance, new GenericFlag<bool>("AGC_OPTION_DEBUG", false));
                        ViewSettingsPatch.m_enableNonLinearSkillLeveling.Add(__instance, new GenericFlag<bool>("AGC_OPTION_ENABLE_NON_LINEAR_SKILL_LEVELING", true));
                        ViewSettingsPatch.m_forceEmployeeLowestHireLevel.Add(__instance, new GenericFlag<bool>("AGC_OPTION_FORCE_EMPLOYEE_LOWEST_HIRE_LEVEL", true));
                        ViewSettingsPatch.m_labEmployeeBiochemistry.Add(__instance, new GenericFlag<bool>("AGC_OPTION_LAB_EMPLOYEE_BIOCHEMISTRY", true));
                        ViewSettingsPatch.m_limitClinicDoctorsLevel.Add(__instance, new GenericFlag<bool>("AGC_OPTION_LIMIT_CLINIC_DOCTORS_LEVEL", true));
                        ViewSettingsPatch.m_patientsThroughEmergency.Add(__instance, new GenericFlag<bool>("AGC_OPTION_PATIENTS_ONLY_EMERGENCY", true));
                        ViewSettingsPatch.m_staffLunchNight.Add(__instance, new GenericFlag<bool>("AGC_OPTION_STAFF_LUNCH_NIGHT", true));
                        ViewSettingsPatch.m_staffShiftsEqual.Add(__instance, new GenericFlag<bool>("AGC_OPTION_STAFF_SHIFTS_EQUAL", true));

                        if (Tweakable.Vanilla.DlcHospitalServicesEnabled())
                        {
                            ViewSettingsPatch.m_trainingDepartment.Add(__instance, new GenericFlag<bool>("AGC_OPTION_TRAINING_DEPARTMENT", true));
                        }

                        var boolFlags = new List<GenericFlag<bool>>(__instance.m_allBoolFlags);

                        boolFlags.Add(ViewSettingsPatch.m_debug[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_enableNonLinearSkillLeveling[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_forceEmployeeLowestHireLevel[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_labEmployeeBiochemistry[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_limitClinicDoctorsLevel[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_patientsThroughEmergency[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_staffShiftsEqual[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_staffLunchNight[__instance]);

                        if (Tweakable.Vanilla.DlcHospitalServicesEnabled())
                        {
                            boolFlags.Add(ViewSettingsPatch.m_trainingDepartment[__instance]);
                        }

                        __instance.m_allBoolFlags = boolFlags.ToArray();
                    }
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ViewSettings), nameof(ViewSettings.Load))]
        public static void LoadPostfix(ViewSettings __instance)
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"DLC Hospital services present: {Tweakable.Vanilla.DlcHospitalServicesEnabled()}");

            bool enableTrainingDepartment = Tweakable.Vanilla.DlcHospitalServicesEnabled();
            enableTrainingDepartment &= ViewSettingsPatch.m_trainingDepartment.ContainsKey(__instance);
            enableTrainingDepartment &= (enableTrainingDepartment && ViewSettingsPatch.m_trainingDepartment[__instance].m_value);

            if (!enableTrainingDepartment)
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
                            if (entry.Key.ToString() == Departments.Mod.TrainingDepartment)
                            {
                                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Found training department ({Departments.Mod.TrainingDepartment}), removing");

                                dbTable.Remove(entry.Key);
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            if (ViewSettingsPatch.m_enabled)
            {
                ViewSettingsPatch.FixSchedules();

                if (ViewSettingsPatch.m_labEmployeeBiochemistry[__instance].m_value)
                {
                    var departmentType = typeof(GameDBDepartment);

                    var lab = Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.MedicalLaboratories);

                    var labQualificationProperty = departmentType.GetProperty(nameof(GameDBDepartment.LabQualification), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var labQualificationPropertySetMethod = labQualificationProperty.GetSetMethod(true);

                    labQualificationPropertySetMethod.Invoke(lab, new object[] { new DatabaseEntryRef<GameDBSkill>(Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_BIOCHEMISTRY)) });

                    Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Medical laboratories department, changed lab qualification to {Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_BIOCHEMISTRY}");

                    var scienceSkill = Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_QUALIF_SCIENCE_EDUCATION);

                    var roomType = typeof(GameDBRoomType);

                    var requiredSkillProperty = roomType.GetProperty(nameof(GameDBRoomType.RequiredSkill), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var requiredSkillPropertySetMethod = requiredSkillProperty.GetSetMethod(true);

                    foreach (var requiredRoom in lab.RequiredRoomsClinic)
                    {
                        var requiredRoomType = requiredRoom.RoomDatabaseEntryRef.Entry;

                        if ((requiredRoomType.RequiredSkill != null) && (requiredRoomType.RequiredSkill.Entry == scienceSkill))
                        {
                            requiredSkillPropertySetMethod.Invoke(requiredRoomType, new object[] { new DatabaseEntryRef<GameDBSkill>(Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_BIOCHEMISTRY)) });

                            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Medical laboratories department, room type {requiredRoomType.DatabaseID}, changed required skill to {Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_BIOCHEMISTRY}");
                        }
                    }
                }
            }
        }

        internal static void FixSchedules()
        {
            // fix schedules
            GameDBSchedule[] schedules = Database.Instance.GetEntries<GameDBSchedule>();
            foreach (var schedule in schedules)
            {
                ViewSettingsPatch.FixScheduleTimes(schedule);
            }
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
