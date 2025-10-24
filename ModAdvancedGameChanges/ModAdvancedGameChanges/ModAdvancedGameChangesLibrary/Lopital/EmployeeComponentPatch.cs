using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges;
using System;
using System.Globalization;

namespace ModGameChanges.Lopital
{
    [HarmonyPatch(typeof(EmployeeComponent))]
    public static class EmployeeComponentPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.AddExperiencePoints))]
        public static void AddExperiencePoints(int points, EmployeeComponent __instance)
        {
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"01 Employee: {__instance.m_entity.Name} Points: {points.ToString(CultureInfo.InvariantCulture)}");

            // it seems that original code is "optimized" or something similar
            // the GetPointsNeededForNextLevel() patched method is not called for unknown reason
            bool runOriginalCode = true;

            if (ViewSettingsPatch.m_enabled)
            {
                if (ViewSettingsPatch.m_enableNonLinearSkillLeveling[SettingsManager.Instance.m_viewSettings].m_value)
                {
                    if (__instance.m_entity.GetComponent<BehaviorDoctor>() != null)
                    {
                        bool levelUp = false;

                        float num = (float)points * (Tweakable.Vanilla.LevelingRatePercent() / 100f);
                        if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Constants.Perks.Vanilla.FastLearner))
                        {
                            num *= 1.1f;
                        }
                        else if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Constants.Perks.Vanilla.SlowLearner))
                        {
                            num *= 0.9f;
                        }

                        __instance.m_state.m_points += (int)num;

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"02 Employee: {__instance.m_entity.Name} Points: {points.ToString(CultureInfo.InvariantCulture)}");

                        int doctorMaxLevel = 5;
                        if (__instance.IsClinicEmployee())
                        {
                            if (ViewSettingsPatch.m_limitClinicDoctorsLevel[SettingsManager.Instance.m_viewSettings].m_value)
                            {
                                doctorMaxLevel = Tweakable.Mod.AllowedClinicDoctorsLevel();
                            }
                        }

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"03 Employee: {__instance.m_entity.Name} Level: {__instance.m_state.m_level.ToString(CultureInfo.InvariantCulture)} Allowed level: {doctorMaxLevel.ToString(CultureInfo.InvariantCulture)}");

                        if (__instance.m_state.m_level >= doctorMaxLevel)
                        {
                            __instance.m_state.m_level = doctorMaxLevel;
                            __instance.m_state.m_points = 0;
                        }

                        int nextLevelPoints = __instance.GetPointsNeededForNextLevel();

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"04 Employee: {__instance.m_entity.Name} Level: {__instance.m_state.m_level.ToString(CultureInfo.InvariantCulture)} Allowed level: {doctorMaxLevel.ToString(CultureInfo.InvariantCulture)} Points: {__instance.m_state.m_points.ToString(CultureInfo.InvariantCulture)} Required points: {nextLevelPoints.ToString(CultureInfo.InvariantCulture)}");

                        if (__instance.m_state.m_points >= nextLevelPoints)
                        {
                            __instance.m_state.m_points = 0;
                            if (__instance.m_state.m_level < doctorMaxLevel)
                            {
                                __instance.m_state.m_level++;
                                levelUp = true;
                                string titleLocID = "NOTIF_CHARACTER_LEVELED_UP";
                                if (__instance.m_state.m_level == 2)
                                {
                                    titleLocID = "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION";
                                }
                                else if (__instance.m_state.m_level == 4 && __instance.m_entity.GetComponent<BehaviorDoctor>() != null)
                                {
                                    titleLocID = "NOTIF_CHARACTER_LEVELED_UP_SECOND_SPECIALIZATION";
                                }
                                if (__instance.m_state.m_level > 1 && __instance.m_state.m_skillSet.m_specialization1 == null)
                                {
                                    __instance.m_state.m_skillSet.AddFirstSpecialization();
                                }
                                if (__instance.m_state.m_level > 3 && __instance.m_state.m_skillSet.m_specialization1 != null && __instance.m_state.m_skillSet.m_specialization2 == null)
                                {
                                    __instance.m_state.m_skillSet.AddSecondSpecialization(__instance.m_state.m_department.GetEntity().GetDepartmentType(), 1f);
                                }
                                NotificationManager.GetInstance().AddMessage(__instance.m_entity, titleLocID, StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsDoctor[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                            }
                        }

                        if (levelUp)
                        {
                            __instance.m_state.m_leveledUpAfterHire = true;
                            if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Constants.Perks.Vanilla.FastLearner))
                            {
                                __instance.m_entity.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Constants.Perks.Vanilla.FastLearner);
                            }
                            if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Constants.Perks.Vanilla.SlowLearner))
                            {
                                __instance.m_entity.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Constants.Perks.Vanilla.SlowLearner);
                            }
                        }

                        runOriginalCode = false;
                    }
                }
                else if (ViewSettingsPatch.m_limitClinicDoctorsLevel[SettingsManager.Instance.m_viewSettings].m_value)
                {
                    if (__instance.m_entity.GetComponent<BehaviorDoctor>() != null)
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"05 Employee: {__instance.m_entity.Name} Points: {points.ToString(CultureInfo.InvariantCulture)}");

                        int doctorMaxLevel = 5;
                        if (__instance.IsClinicEmployee())
                        {
                            doctorMaxLevel = Tweakable.Mod.AllowedClinicDoctorsLevel();
                        }

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"06 Employee: {__instance.m_entity.Name} Level: {__instance.m_state.m_level.ToString(CultureInfo.InvariantCulture)} Allowed level: {doctorMaxLevel.ToString(CultureInfo.InvariantCulture)}");

                        if (__instance.m_state.m_level >= doctorMaxLevel)
                        {
                            __instance.m_state.m_level = doctorMaxLevel;
                            __instance.m_state.m_points = 0;

                        }

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"07 Employee: {__instance.m_entity.Name} Level: {__instance.m_state.m_level.ToString(CultureInfo.InvariantCulture)} Allowed level: {doctorMaxLevel.ToString(CultureInfo.InvariantCulture)} Points: {__instance.m_state.m_points.ToString(CultureInfo.InvariantCulture)}");

                        runOriginalCode = false;
                    }
                }
            }

            if (runOriginalCode)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"08 Employee: {__instance.m_entity.Name}");

                float num = (float)points * (Database.Instance.GetEntry<GameDBTweakableFloat>(Constants.Tweakables.Vanilla.LevelingRatePercent).Value / 100f);
                if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Constants.Perks.Vanilla.FastLearner))
                {
                    __instance.m_state.m_points += (int)num * 110 / 100;
                }
                else if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Constants.Perks.Vanilla.SlowLearner))
                {
                    __instance.m_state.m_points += (int)num * 90 / 100;
                }
                else
                {
                    __instance.m_state.m_points += (int)num;
                }

                int maxLevel = (__instance.m_entity.GetComponent<BehaviorDoctor>() == null) ? 3 : 5;
                int nextLevelPoints = __instance.GetPointsNeededForNextLevel();

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} Required points: {nextLevelPoints.ToString(CultureInfo.InvariantCulture)}");

                if (__instance.m_state.m_points >= nextLevelPoints && __instance.m_state.m_level < maxLevel)
                {
                    __instance.m_state.m_points -= nextLevelPoints;
                    __instance.m_state.m_level++;
                    __instance.m_state.m_leveledUpAfterHire = true;
                    if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Constants.Perks.Vanilla.FastLearner))
                    {
                        __instance.m_entity.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Constants.Perks.Vanilla.FastLearner);
                    }
                    if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Constants.Perks.Vanilla.SlowLearner))
                    {
                        __instance.m_entity.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Constants.Perks.Vanilla.SlowLearner);
                    }
                    string titleLocID = "NOTIF_CHARACTER_LEVELED_UP";
                    if (__instance.m_state.m_level == 2)
                    {
                        titleLocID = "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION";
                    }
                    else if (__instance.m_state.m_level == 4 && __instance.m_entity.GetComponent<BehaviorDoctor>() != null)
                    {
                        titleLocID = "NOTIF_CHARACTER_LEVELED_UP_SECOND_SPECIALIZATION";
                    }
                    if (__instance.m_entity.GetComponent<BehaviorDoctor>() != null)
                    {
                        if (__instance.m_state.m_level > 1 && __instance.m_state.m_skillSet.m_specialization1 == null)
                        {
                            __instance.m_state.m_skillSet.AddFirstSpecialization();
                        }
                        if (__instance.m_state.m_level > 3 && __instance.m_state.m_skillSet.m_specialization1 != null && __instance.m_state.m_skillSet.m_specialization2 == null)
                        {
                            __instance.m_state.m_skillSet.AddSecondSpecialization(__instance.m_state.m_department.GetEntity().GetDepartmentType(), 1f);
                        }
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, titleLocID, StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsDoctor[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                    else if (__instance.m_entity.GetComponent<BehaviorNurse>() != null)
                    {
                        if (__instance.m_state.m_level > 1 && __instance.m_state.m_skillSet.m_specialization1 == null)
                        {
                            GameDBSkill[] array = new GameDBSkill[]
                            {
                                Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_NURSE_SPEC_RECEPTIONIST),
                                Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_NURSE_SPEC_MEDICAL_SURGERY),
                                Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_NURSE_SPEC_CLINICAL_SPECIALIST)
                            };
                            __instance.m_state.m_skillSet.m_specialization1 = new Skill(array[UnityEngine.Random.Range(0, array.Length)], 1f);
                        }
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, titleLocID, StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsNurse[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                    else if (__instance.m_entity.GetComponent<BehaviorLabSpecialist>() != null)
                    {
                        if (__instance.m_state.m_level > 1 && __instance.m_state.m_skillSet.m_specialization1 == null)
                        {
                            GameDBSkill[] array2 = new GameDBSkill[]
                            {
                                Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_BIOCHEMISTRY),
                                Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_USG),
                                Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_CARDIOLOGY),
                                Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_NEUROLOGY)
                            };
                            __instance.m_state.m_skillSet.m_specialization1 = new Skill(array2[UnityEngine.Random.Range(0, array2.Length)], 1f);
                        }
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, titleLocID, StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsLabSpecialist[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                    else if (__instance.m_entity.GetComponent<BehaviorJanitor>() != null)
                    {
                        if (Tweakable.Vanilla.DlcHospitalServicesEnabled() && __instance.m_state.m_level > 1 && __instance.m_state.m_skillSet.m_specialization1 == null)
                        {
                            titleLocID = "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION";

                            GameDBSkill[] array = new GameDBSkill[]
                            {
                                Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_VENDOR),
                                Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_MANAGER)
                            };

                            __instance.m_state.m_skillSet.m_specialization1 = new Skill(array[UnityEngine.Random.Range(0, array.Length)], 1f);
                        }
                        else
                        {
                            titleLocID = "NOTIF_CHARACTER_LEVELED_UP";
                        }
                            
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, titleLocID, StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsJanitor[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                    else
                    {
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, "NOTIF_CHARACTER_LEVELED_UP", "TODO", string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.GetPointsNeededForNextLevel))]
        public static bool GetPointsNeededForNextLevelPrefix(EmployeeComponent __instance, ref int __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enableNonLinearSkillLeveling[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            if (__instance.m_entity.GetComponent<BehaviorDoctor>() != null)
            {
                __result = Tweakable.Mod.DoctorLevelPoints(Math.Max(1, Math.Min(4, __instance.m_state.m_level)));
            }
            else if (__instance.m_entity.GetComponent<BehaviorNurse>() != null)
            {
                __result = Tweakable.Mod.NurseLevelPoints(Math.Max(1, Math.Min(2, __instance.m_state.m_level)));
            }
            else if (__instance.m_entity.GetComponent<BehaviorLabSpecialist>() != null)
            {
                __result = Tweakable.Mod.LabSpecialistLevelPoints(Math.Max(1, Math.Min(2, __instance.m_state.m_level)));
            }
            else
            {
                __result = Tweakable.Mod.JanitorLevelPoints(Math.Max(1, Math.Min(2, __instance.m_state.m_level)));
            }

            return false;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.GoToTraining))]
        //public static bool GoToTrainingPrefix(ProcedureComponent procedureComponent, EmployeeComponent __instance, ref bool __result)
        //{
        //    if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
        //    {
        //        // Allow original method to run
        //        return true;
        //    }

        //    Department trainingDepartment = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Constants.Departments.Mod.TrainingDepartment));

        //    if (__instance.m_state.m_department.GetEntity() == trainingDepartment)
        //    {
        //        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} in training department");

        //        Department administrativeDepartment = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Constants.Departments.Vanilla.AdministrativeDepartment));
        //        GameDBProcedure staffTraining = Database.Instance.GetEntry<GameDBProcedure>(Constants.Procedures.Vanilla.StaffTraining);

        //        if (procedureComponent.GetProcedureAvailabilty(staffTraining, __instance.m_entity, trainingDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE) == ProcedureSceneAvailability.AVAILABLE)
        //        {
        //            procedureComponent.StartProcedure(staffTraining, __instance.m_entity, trainingDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE);

        //            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} starting training in training department");

        //            __result = true;
        //        }
        //        else if (procedureComponent.GetProcedureAvailabilty(staffTraining, __instance.m_entity, administrativeDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE) == ProcedureSceneAvailability.AVAILABLE)
        //        {
        //            procedureComponent.StartProcedure(staffTraining, __instance.m_entity, administrativeDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE);

        //            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} starting training in administrative department");

        //            __result = true;
        //        }
        //        else
        //        {
        //            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} not found free space for training");

        //            __result = false;
        //        }

        //        return false;
        //    }

        //    return true;
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.OnDayStart))]
        public static bool OnDayStartPrefix(EmployeeComponent __instance)
        {
            __instance.m_state.m_efficiency = 1f;

            PerkComponent perkComponent = __instance.m_entity.GetComponent<PerkComponent>();

            // start commuting between one hour and half hour before shift
            __instance.m_state.m_commuteTime = UnityEngine.Random.Range(-1f, -0.5f);

            // if long commute, add between 0.25 and 2 hours
            __instance.m_state.m_commuteTime += perkComponent.m_perkSet.HasPerk(Constants.Perks.Vanilla.LongCommute) ? UnityEngine.Random.Range(0.25f, 2f) : 0;

            if (perkComponent.m_perkSet.HasPerk(Constants.Perks.Vanilla.Alcoholism) && (__instance.m_state.m_shift == Shift.DAY)
                && (UnityEngine.Random.Range(0, 100) < 4))
            {
                __instance.m_state.m_commuteTime = 2f + UnityEngine.Random.Range(0f, 1.5f);
                __instance.m_state.m_efficiency = 0.5f;

                if (perkComponent.m_perkSet.HasHiddenPerk(Constants.Perks.Vanilla.Alcoholism))
                {
                    NotificationManager.GetInstance().AddMessage(__instance.m_entity, "NOTIF_EMPLOYEE_GOT_DRUNK", string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    perkComponent.m_perkSet.RevealPerk(Constants.Perks.Vanilla.Alcoholism);
                }

                __instance.m_entity.GetComponent<MoodComponent>().AddSatisfactionModifier(Constants.Mood.Vanilla.Hangover);
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} Commute time: {__instance.m_state.m_commuteTime.ToString(CultureInfo.InvariantCulture)}");

            __instance.ResetNoWorkSpaceFlags();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.ShouldStartCommuting))]
        public static bool ShouldStartCommutingPrefix(EmployeeComponent __instance, ref bool __result)
        {
            GameDBSchedule shift = (__instance.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Constants.Schedule.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Constants.Schedule.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            float startCommuteTime = shift.StartTime - 1f;
            float endCommuteTime = shift.StartTime + 3.5f;
            float dayTimeHours = DayTime.Instance.GetDayTimeHours();

            if ((dayTimeHours >= startCommuteTime) && (dayTimeHours <= endCommuteTime) && (dayTimeHours > (shift.StartTime + __instance.m_state.m_commuteTime)))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} Commute time: {__instance.m_state.m_commuteTime.ToString(CultureInfo.InvariantCulture)} Shift start time: {shift.StartTime.ToString(CultureInfo.InvariantCulture)} Day time: {dayTimeHours.ToString(CultureInfo.InvariantCulture)}");

                __result = true;
            }
            else
            {
                __result = false;
            }

            return false;
        }
    }
}

