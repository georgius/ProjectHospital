using HarmonyLib;
using ModAdvancedGameChanges.Constants;
using ModAdvancedGameChanges.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ModAdvancedGameChanges 
{
    [HarmonyPatch(typeof(ViewSettings))]
    public static class ViewSettingsPatch
    {
        public static bool m_enabled = false;

        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_debug = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_enableModChanges = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_enableNonLinearSkillLeveling = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_enablePedestriansGoToPharmacy = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_forceEmployeeLowestHireLevel = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_labEmployeeBiochemistry = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_limitClinicDoctorsLevel = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_patientsThroughEmergency = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_staffLunchNight = new Dictionary<ViewSettings, GenericFlag<bool>>();
        public static readonly Dictionary<ViewSettings, GenericFlag<bool>> m_staffShiftsEqual = new Dictionary<ViewSettings, GenericFlag<bool>>();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ViewSettings), nameof(ViewSettings.Load))]
        public static bool LoadPrefix(ViewSettings __instance)
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Start");

            try
            {
                ViewSettingsPatch.m_enabled = false;
                Tweakable.CheckConfiguration();

                // mod will be enabled if everything is correct and DLC Hospital Services is present
                ViewSettingsPatch.m_enabled = AdvancedGameChanges.m_enabled && Tweakable.Vanilla.DlcHospitalServicesEnabled();

                if (ViewSettingsPatch.m_enabled)
                {
                    if (!ViewSettingsPatch.m_enableNonLinearSkillLeveling.ContainsKey(__instance))
                    {
                        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Adding settings");

                        ViewSettingsPatch.m_debug.Add(__instance, new GenericFlag<bool>("AGC_OPTION_DEBUG", false));
                        ViewSettingsPatch.m_enableModChanges.Add(__instance, new GenericFlag<bool>("AGC_OPTION_ENABLE_MOD_CHANGES", true));
                        ViewSettingsPatch.m_enableNonLinearSkillLeveling.Add(__instance, new GenericFlag<bool>("AGC_OPTION_ENABLE_NON_LINEAR_SKILL_LEVELING", true));
                        ViewSettingsPatch.m_enablePedestriansGoToPharmacy.Add(__instance, new GenericFlag<bool>("AGC_OPTION_ENABLE_PEDESTRIANS_GO_TO_PHARMACY", true));
                        ViewSettingsPatch.m_forceEmployeeLowestHireLevel.Add(__instance, new GenericFlag<bool>("AGC_OPTION_FORCE_EMPLOYEE_LOWEST_HIRE_LEVEL", true));
                        ViewSettingsPatch.m_labEmployeeBiochemistry.Add(__instance, new GenericFlag<bool>("AGC_OPTION_LAB_EMPLOYEE_BIOCHEMISTRY", true));
                        ViewSettingsPatch.m_limitClinicDoctorsLevel.Add(__instance, new GenericFlag<bool>("AGC_OPTION_LIMIT_CLINIC_DOCTORS_LEVEL", true));
                        ViewSettingsPatch.m_patientsThroughEmergency.Add(__instance, new GenericFlag<bool>("AGC_OPTION_PATIENTS_ONLY_EMERGENCY", true));
                        ViewSettingsPatch.m_staffLunchNight.Add(__instance, new GenericFlag<bool>("AGC_OPTION_STAFF_LUNCH_NIGHT", true));
                        ViewSettingsPatch.m_staffShiftsEqual.Add(__instance, new GenericFlag<bool>("AGC_OPTION_STAFF_SHIFTS_EQUAL", true));

                        var boolFlags = new List<GenericFlag<bool>>(__instance.m_allBoolFlags);

                        boolFlags.Add(ViewSettingsPatch.m_debug[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_enableModChanges[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_enableNonLinearSkillLeveling[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_forceEmployeeLowestHireLevel[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_labEmployeeBiochemistry[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_limitClinicDoctorsLevel[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_patientsThroughEmergency[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_staffShiftsEqual[__instance]);
                        boolFlags.Add(ViewSettingsPatch.m_staffLunchNight[__instance]);

                        if (Tweakable.Vanilla.DlcHospitalServicesEnabled())
                        {
                            boolFlags.Add(ViewSettingsPatch.m_enablePedestriansGoToPharmacy[__instance]);
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

            // allow original method to run
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ViewSettings), nameof(ViewSettings.Load))]
        public static void LoadPostfix(ViewSettings __instance)
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"DLC Hospital services present: {Tweakable.Vanilla.DlcHospitalServicesEnabled()}");

            // enable / disable mod changes
            ViewSettingsPatch.m_enabled &= ViewSettingsPatch.m_enableModChanges.ContainsKey(__instance);
            ViewSettingsPatch.m_enabled &= (ViewSettingsPatch.m_enabled && ViewSettingsPatch.m_enableModChanges[__instance].m_value);

            if (!ViewSettingsPatch.m_enabled)
            {
                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Disabling training department");

                var tablesHelper = new PrivateFieldAccessHelper<Database, IDictionary>("tables", Database.Instance);

                foreach (DictionaryEntry tableEntry in tablesHelper.Field)
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
                ViewSettingsPatch.FixVendingMachines();

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

        private static void FixSchedules()
        {
            // fix schedules
            GameDBSchedule[] schedules = Database.Instance.GetEntries<GameDBSchedule>();
            foreach (var schedule in schedules)
            {
                ViewSettingsPatch.FixScheduleTimes(schedule);
            }
        }

        private static void FixScheduleTimes(GameDBSchedule schedule)
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

        private static void FixVendingMachines()
        {
            // fix vending machines
            // each vending machine must have tags: inventory, vending, ui_refreshments
            // in other objects vending tag will be removed, because vending is used to signal to pay

            var objectType = typeof(GameDBObject);

            var tagsProperty = objectType.GetProperty(nameof(GameDBObject.Tags), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var tagsPropertySetMethod = tagsProperty.GetSetMethod(true);

            GameDBObject[] objects = Database.Instance.GetEntries<GameDBObject>();
            foreach (var item in objects)
            {
                if (item.HasTag(Tags.Vanilla.Vending))
                {
                    Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Object {item.DatabaseID}, tags '{string.Join(", ", item.Tags)}'");

                    if ((!item.HasTag(Tags.Vanilla.Inventory)) || (!item.HasTag(Tags.Vanilla.Refreshments)))
                    {
                        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Object {item.DatabaseID}, removing tag '{Tags.Vanilla.Vending}'");

                        // item is not vending machine, remove vending tag
                        tagsPropertySetMethod.Invoke(item, new object[] { item.Tags.Where(t => (t != Tags.Vanilla.Vending)).ToArray() });
                    }
                }
            }
        }
    }
}
