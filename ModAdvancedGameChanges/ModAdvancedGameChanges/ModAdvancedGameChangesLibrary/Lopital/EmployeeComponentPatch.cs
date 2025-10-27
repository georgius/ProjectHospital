using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges;
using ModAdvancedGameChanges.Constants;
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
                        if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.FastLearner))
                        {
                            num *= 1.1f;
                        }
                        else if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.SlowLearner))
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
                            if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.FastLearner))
                            {
                                __instance.m_entity.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Perks.Vanilla.FastLearner);
                            }
                            if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.SlowLearner))
                            {
                                __instance.m_entity.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Perks.Vanilla.SlowLearner);
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

                float num = (float)points * (Database.Instance.GetEntry<GameDBTweakableFloat>(Tweakables.Vanilla.LevelingRatePercent).Value / 100f);
                if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.FastLearner))
                {
                    __instance.m_state.m_points += (int)num * 110 / 100;
                }
                else if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.SlowLearner))
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
                    if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.FastLearner))
                    {
                        __instance.m_entity.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Perks.Vanilla.FastLearner);
                    }
                    if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.SlowLearner))
                    {
                        __instance.m_entity.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Perks.Vanilla.SlowLearner);
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
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_RECEPTIONIST),
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_MEDICAL_SURGERY),
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_CLINICAL_SPECIALIST)
                            };
                            __instance.m_state.m_skillSet.m_specialization1 = new Skill(array[UnityEngine.Random.Range(0, array.Length)], Skills.SkillLevelMinimum);
                        }
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, titleLocID, StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsNurse[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                    else if (__instance.m_entity.GetComponent<BehaviorLabSpecialist>() != null)
                    {
                        if (__instance.m_state.m_level > 1 && __instance.m_state.m_skillSet.m_specialization1 == null)
                        {
                            GameDBSkill[] array2 = new GameDBSkill[]
                            {
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_BIOCHEMISTRY),
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_USG),
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_CARDIOLOGY),
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_NEUROLOGY)
                            };
                            __instance.m_state.m_skillSet.m_specialization1 = new Skill(array2[UnityEngine.Random.Range(0, array2.Length)], Skills.SkillLevelMinimum);
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
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_VENDOR),
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_MANAGER)
                            };

                            __instance.m_state.m_skillSet.m_specialization1 = new Skill(array[UnityEngine.Random.Range(0, array.Length)], Skills.SkillLevelMinimum);
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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.GoToTraining))]
        public static bool GoToTrainingPrefix(ProcedureComponent procedureComponent, EmployeeComponent __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            Department trainingDepartment = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Mod.TrainingDepartment));

            if (__instance.m_state.m_department.GetEntity() == trainingDepartment)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} in training department");

                Department administrativeDepartment = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.AdministrativeDepartment));
                GameDBProcedure staffTraining = Database.Instance.GetEntry<GameDBProcedure>(Procedures.Vanilla.StaffTraining);

                if (procedureComponent.GetProcedureAvailabilty(staffTraining, __instance.m_entity, trainingDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE) == ProcedureSceneAvailability.AVAILABLE)
                {
                    procedureComponent.StartProcedure(staffTraining, __instance.m_entity, trainingDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} starting training in training department");

                    __result = true;
                }
                else if (procedureComponent.GetProcedureAvailabilty(staffTraining, __instance.m_entity, administrativeDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE) == ProcedureSceneAvailability.AVAILABLE)
                {
                    procedureComponent.StartProcedure(staffTraining, __instance.m_entity, administrativeDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} starting training in administrative department");

                    __result = true;
                }
                else
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} not found free space for training");

                    __result = false;
                }

                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.OnDayStart))]
        public static bool OnDayStartPrefix(EmployeeComponent __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_staffShiftsEqual[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            __instance.m_state.m_efficiency = 1f;

            PerkComponent perkComponent = __instance.m_entity.GetComponent<PerkComponent>();

            // start commuting between one hour and half hour before shift
            __instance.m_state.m_commuteTime = UnityEngine.Random.Range(-1f, -0.5f);

            // if long commute, add between 0.25 and 2 hours
            __instance.m_state.m_commuteTime += perkComponent.m_perkSet.HasPerk(Perks.Vanilla.LongCommute) ? UnityEngine.Random.Range(0.25f, 2f) : 0;

            if (perkComponent.m_perkSet.HasPerk(Perks.Vanilla.Alcoholism) && (__instance.m_state.m_shift == Shift.DAY)
                && (UnityEngine.Random.Range(0, 100) < 4))
            {
                __instance.m_state.m_commuteTime = 2f + UnityEngine.Random.Range(0f, 1.5f);
                __instance.m_state.m_efficiency = 0.5f;

                if (perkComponent.m_perkSet.HasHiddenPerk(Perks.Vanilla.Alcoholism))
                {
                    NotificationManager.GetInstance().AddMessage(__instance.m_entity, "NOTIF_EMPLOYEE_GOT_DRUNK", string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    perkComponent.m_perkSet.RevealPerk(Perks.Vanilla.Alcoholism);
                }

                __instance.m_entity.GetComponent<MoodComponent>().AddSatisfactionModifier(Moods.Vanilla.Hangover);
            }

            BehaviorJanitor behaviorJanitor = __instance.m_entity.GetComponent<BehaviorJanitor>();
            if (behaviorJanitor != null)
            {
                // janitors don't have commuting state as doctors, nurses and lab specialists
                // we just keep them at home longer time

                float commuteTime = __instance.m_state.m_commuteTime;

                __instance.SetSimpleCommuteDuration();

                __instance.m_state.m_commuteTime += commuteTime;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} Commute time: {__instance.m_state.m_commuteTime.ToString(CultureInfo.InvariantCulture)}");

            __instance.ResetNoWorkSpaceFlags();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.ToggleTraining))]
        public static bool ToggleTrainingPrefix(Skill skill, EmployeeComponent __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            GameDBRoomType homeRoomType = __instance.GetHomeRoomType();

            if ((homeRoomType != null)
                && (
                    homeRoomType.HasTag(Tags.Mod.DoctorTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace)
                    ))
            {
                // employee is in training department, employee is in training workspace

                if (!__instance.m_state.m_trainingData.m_trainingSkillsToTrain.Contains(skill.m_gameDBSkill.Entry))
                {
                    if (__instance.m_state.m_trainingData.m_trainingSkillsToTrain.Count == 0)
                    {
                        __instance.m_state.m_trainingData.m_trainingRemainingHours = 1;
                    }

                    __instance.m_state.m_trainingData.m_trainingSkillsToTrain.Add(skill.m_gameDBSkill.Entry);
                    __instance.m_state.m_trainingData.m_trainingInitialSkillLevels.Put(skill.m_gameDBSkill.Entry, skill.m_level);
                }

                __instance.m_state.m_department.GetEntity().Validate();

                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.ShouldStartCommuting))]
        public static bool ShouldStartCommutingPrefix(EmployeeComponent __instance, ref bool __result)
        {
            // it will be very difficult if employee should start commuting when staff shifts are not "reasonable"
            // in such case, the original algorithm is used

            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_staffShiftsEqual[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            // the staff day shift starts at least at 3h and at most 20h
            // each shift will be 12h

            GameDBSchedule shift = (__instance.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            // because ViewSettingsPatch.FixScheduleTimes is fixing all schedules in game,
            // then values in game are surely between 0 and 24

            // commute window is hour before shift starts and ends
            float startCommuteTime = shift.StartTime - 1f;      // 2 .. 19
            float endCommuteTime = shift.EndTime - 1f;          // 2 .. 19

            // if end commute time is before start commute time (like interval from 20 till 8),
            // it means that end commute time is over midnight
            bool overMidnight = (endCommuteTime < startCommuteTime);
            float dayTimeHours = DayTime.Instance.GetDayTimeHours();    // 0 .. <24 (always below 24)

            if ((!overMidnight) && (dayTimeHours >= startCommuteTime) && (dayTimeHours <= endCommuteTime)
                && (dayTimeHours > (shift.StartTime + __instance.m_state.m_commuteTime)))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} Commute time: {__instance.m_state.m_commuteTime.ToString(CultureInfo.InvariantCulture)} Shift start time: {shift.StartTime.ToString(CultureInfo.InvariantCulture)} Day time: {dayTimeHours.ToString(CultureInfo.InvariantCulture)}");

                __result = true;
            }
            else if (overMidnight && ((dayTimeHours >= startCommuteTime) || (dayTimeHours <= endCommuteTime))
                && (dayTimeHours > (shift.StartTime + __instance.m_state.m_commuteTime)))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} Commute time: {__instance.m_state.m_commuteTime.ToString(CultureInfo.InvariantCulture)} Shift start time: {shift.StartTime.ToString(CultureInfo.InvariantCulture)} Day time: {dayTimeHours.ToString(CultureInfo.InvariantCulture)}, over midnight");

                __result = true;
            }
            else
            {
                __result = false;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.UpdateTraining))]
        public static bool UpdateTrainingPrefix(ProcedureComponent procedureComponent, EmployeeComponent __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            if (procedureComponent.IsBusy())
            {
                __result = false;
                return false;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} training finished");

            GameDBRoomType homeRoomType = __instance.GetHomeRoomType();

            if ((homeRoomType != null)
                && (
                    homeRoomType.HasTag(Tags.Mod.DoctorTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace)
                    ))
            {
                // employee in training department, employee is in training workspace
                __instance.m_state.m_trainingData.m_trainingRemainingHours = 0;

                if (__instance.m_state.m_trainingData.m_trainingSkillsToTrain.Count == 0)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} training finished, but there are no trained skills");

                    __result = true;
                    return false;
                }

                // randomly choose skill which is trained
                Skill skill = __instance.m_state.m_skillSet.GetSkill(__instance.m_state.m_trainingData.m_trainingSkillsToTrain[0].Entry);
                if (skill == null)
                {
                    __result = true;
                    return false;
                }

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} training finished, skill {skill.m_gameDBSkill.Entry.DatabaseID}");

                // __instance.m_state.m_level

                float points = skill.m_level * 10f;
                skill.AddPoints((int)points, __instance.m_entity);
                __instance.AddExperiencePoints((int)(points * UnityEngine.Random.Range(0f, 1f)));

                // remove all skills to train
                __instance.m_state.m_trainingData.m_trainingSkillsToTrain.Clear();

                __result = true;
                return false;
            }

            // employee not in training department
            // training was requested by player
            // return to original code
            return true;
        }
    }
}

