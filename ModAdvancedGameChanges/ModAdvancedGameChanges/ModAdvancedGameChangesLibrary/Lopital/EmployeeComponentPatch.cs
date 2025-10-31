using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Globalization;

namespace ModAdvancedGameChanges .Lopital
{
    [HarmonyPatch(typeof(EmployeeComponent))]
    public static class EmployeeComponentPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.AddExperiencePoints))]
        public static bool AddExperiencePoints(int points, EmployeeComponent __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enableNonLinearSkillLeveling[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

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

                        float num = (float)points * Tweakable.Vanilla.LevelingRatePercent();
                        if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.FastLearner))
                        {
                            num *= 1.1f;
                        }
                        else if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.SlowLearner))
                        {
                            num *= 0.9f;
                        }

                        __instance.m_state.m_points += (int)num;

                        int doctorMaxLevel = Levels.Doctors.Specialist;
                        if (__instance.IsClinicEmployee())
                        {
                            if (ViewSettingsPatch.m_limitClinicDoctorsLevel[SettingsManager.Instance.m_viewSettings].m_value)
                            {
                                doctorMaxLevel = Tweakable.Mod.AllowedClinicDoctorsLevel();
                            }
                        }

                        if (__instance.m_state.m_level >= doctorMaxLevel)
                        {
                            __instance.m_state.m_level = doctorMaxLevel;
                            __instance.m_state.m_points = 0;
                        }

                        int nextLevelPoints = __instance.GetPointsNeededForNextLevel();

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, level: {__instance.m_state.m_level.ToString(CultureInfo.InvariantCulture)}, allowed level: {doctorMaxLevel.ToString(CultureInfo.InvariantCulture)}, points: {points.ToString(CultureInfo.InvariantCulture)}, added: {((int)num).ToString(CultureInfo.InvariantCulture)}, actual: {__instance.m_state.m_points.ToString(CultureInfo.InvariantCulture)}, required points: {nextLevelPoints.ToString(CultureInfo.InvariantCulture)}");

                        if (__instance.m_state.m_points >= nextLevelPoints)
                        {
                            __instance.m_state.m_points = 0;
                            if (__instance.m_state.m_level < doctorMaxLevel)
                            {
                                __instance.m_state.m_level++;
                                levelUp = true;
                                string titleLocID = "NOTIF_CHARACTER_LEVELED_UP";
                                if (__instance.m_state.m_level == Levels.Doctors.Resident)
                                {
                                    titleLocID = "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION";
                                }
                                else if ((__instance.m_state.m_level == Levels.Doctors.Fellow) && (__instance.m_entity.GetComponent<BehaviorDoctor>() != null))
                                {
                                    titleLocID = "NOTIF_CHARACTER_LEVELED_UP_SECOND_SPECIALIZATION";
                                }

                                if ((__instance.m_state.m_level > Levels.Doctors.Intern) && (__instance.m_state.m_skillSet.m_specialization1 == null))
                                {
                                    __instance.m_state.m_skillSet.AddFirstSpecialization();
                                }
                                if ((__instance.m_state.m_level > Levels.Doctors.Attending) && (__instance.m_state.m_skillSet.m_specialization1 != null) && (__instance.m_state.m_skillSet.m_specialization2 == null))
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
                    else if (__instance.m_entity.GetComponent<BehaviorNurse>() != null)
                    {
                        bool levelUp = false;

                        float num = (float)points * Tweakable.Vanilla.LevelingRatePercent();
                        if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.FastLearner))
                        {
                            num *= 1.1f;
                        }
                        else if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.SlowLearner))
                        {
                            num *= 0.9f;
                        }

                        __instance.m_state.m_points += (int)num;

                        int maxLevel = Levels.Nurses.NurseSpecialist;

                        if (__instance.m_state.m_level >= maxLevel)
                        {
                            __instance.m_state.m_level = maxLevel;
                            __instance.m_state.m_points = 0;
                        }

                        int nextLevelPoints = __instance.GetPointsNeededForNextLevel();

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, level: {__instance.m_state.m_level.ToString(CultureInfo.InvariantCulture)}, allowed level: {maxLevel.ToString(CultureInfo.InvariantCulture)}, points: {points.ToString(CultureInfo.InvariantCulture)}, added: {((int)num).ToString(CultureInfo.InvariantCulture)}, actual: {__instance.m_state.m_points.ToString(CultureInfo.InvariantCulture)}, required points: {nextLevelPoints.ToString(CultureInfo.InvariantCulture)}");

                        if (__instance.m_state.m_points >= Tweakable.Mod.NurseLevelPoints(1))
                        {
                            if (__instance.m_state.m_skillSet.m_specialization1 == null)
                            {
                                GameDBSkill[] skills = new GameDBSkill[]
                                {
                                    Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_RECEPTIONIST),
                                    Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_MEDICAL_SURGERY),
                                    Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_CLINICAL_SPECIALIST)
                                };

                                __instance.m_state.m_skillSet.m_specialization1 = new Skill(skills[UnityEngine.Random.Range(0, skills.Length)], Skills.SkillLevelMinimum);

                                NotificationManager.GetInstance().AddMessage(
                                    __instance.m_entity, "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION",
                                    StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsLabSpecialist[__instance.m_state.m_level], new string[0]),
                                    string.Empty, string.Empty, 0, 0, 0, 0, null, null);

                                levelUp = true;
                            }
                        }

                        if (__instance.m_state.m_points >= nextLevelPoints)
                        {
                            __instance.m_state.m_points = 0;
                            if (__instance.m_state.m_level < maxLevel)
                            {
                                __instance.m_state.m_level++;
                                levelUp = true;

                                NotificationManager.GetInstance().AddMessage(__instance.m_entity, "NOTIF_CHARACTER_LEVELED_UP", StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsDoctor[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
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
                    else if (__instance.m_entity.GetComponent<BehaviorLabSpecialist>() != null)
                    {
                        bool levelUp = false;

                        float num = (float)points * Tweakable.Vanilla.LevelingRatePercent();
                        if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.FastLearner))
                        {
                            num *= 1.1f;
                        }
                        else if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.SlowLearner))
                        {
                            num *= 0.9f;
                        }

                        __instance.m_state.m_points += (int)num;

                        int maxLevel = Levels.LabSpecialists.MasterScientist;

                        if (__instance.m_state.m_level >= maxLevel)
                        {
                            __instance.m_state.m_level = maxLevel;
                            __instance.m_state.m_points = 0;
                        }

                        int nextLevelPoints = __instance.GetPointsNeededForNextLevel();

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, level: {__instance.m_state.m_level.ToString(CultureInfo.InvariantCulture)}, allowed level: {maxLevel.ToString(CultureInfo.InvariantCulture)}, points: {points.ToString(CultureInfo.InvariantCulture)}, added: {((int)num).ToString(CultureInfo.InvariantCulture)}, actual: {__instance.m_state.m_points.ToString(CultureInfo.InvariantCulture)}, required points: {nextLevelPoints.ToString(CultureInfo.InvariantCulture)}");

                        if (__instance.m_state.m_points >= Tweakable.Mod.LabSpecialistLevelPoints(1))
                        {
                            if (__instance.m_state.m_skillSet.m_specialization1 == null)
                            {
                                GameDBSkill[] skills = new GameDBSkill[]
                                {
                                    Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_BIOCHEMISTRY),
                                    Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_USG),
                                    Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_CARDIOLOGY),
                                    Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_NEUROLOGY)
                                };

                                __instance.m_state.m_skillSet.m_specialization1 = new Skill(skills[UnityEngine.Random.Range(0, skills.Length)], Skills.SkillLevelMinimum);

                                NotificationManager.GetInstance().AddMessage(
                                    __instance.m_entity, "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION", 
                                    StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsLabSpecialist[__instance.m_state.m_level], new string[0]), 
                                    string.Empty, string.Empty, 0, 0, 0, 0, null, null);

                                levelUp = true;
                            }
                        }

                        //if (__instance.m_state.m_points >= Tweakable.Mod.LabSpecialistLevelPoints(3))
                        //{
                        //    if (__instance.m_state.m_skillSet.m_specialization2 == null)
                        //    {
                        //        GameDBSkill[] skills = new GameDBSkill[]
                        //        {
                        //            Database.Instance.GetEntry<GameDBSkill>("SKILL_LAB_SPECIALIST_SPEC_HEMATOLOGY"),
                        //            Database.Instance.GetEntry<GameDBSkill>("SKILL_LAB_SPECIALIST_SPEC_MICROBIOLOGY"),
                        //            Database.Instance.GetEntry<GameDBSkill>("SKILL_LAB_SPECIALIST_SPEC_HISTOLOGY")
                        //        };

                        //        __instance.m_state.m_skillSet.m_specialization1 = new Skill(skills[UnityEngine.Random.Range(0, skills.Length)], Skills.SkillLevelMinimum);

                        //        NotificationManager.GetInstance().AddMessage(
                        //            __instance.m_entity, "NOTIF_CHARACTER_LEVELED_UP_SECOND_SPECIALIZATION",
                        //            StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsLabSpecialist[__instance.m_state.m_level], new string[0]),
                        //            string.Empty, string.Empty, 0, 0, 0, 0, null, null);

                        //        levelUp = true;
                        //    }
                        //}

                        if (__instance.m_state.m_points >= nextLevelPoints)
                        {
                            __instance.m_state.m_points = 0;
                            if (__instance.m_state.m_level < maxLevel)
                            {
                                __instance.m_state.m_level++;
                                levelUp = true;

                                NotificationManager.GetInstance().AddMessage(__instance.m_entity, "NOTIF_CHARACTER_LEVELED_UP", StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsDoctor[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
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
                    else if (__instance.m_entity.GetComponent<BehaviorJanitor>() != null)
                    {
                        bool levelUp = false;

                        float num = (float)points * Tweakable.Vanilla.LevelingRatePercent();
                        if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.FastLearner))
                        {
                            num *= 1.1f;
                        }
                        else if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.SlowLearner))
                        {
                            num *= 0.9f;
                        }

                        __instance.m_state.m_points += (int)num;

                        int maxLevel = Levels.Janitors.MasterJanitor;

                        if (__instance.m_state.m_level >= maxLevel)
                        {
                            __instance.m_state.m_level = maxLevel;
                            __instance.m_state.m_points = 0;
                        }

                        int nextLevelPoints = __instance.GetPointsNeededForNextLevel();

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, level: {__instance.m_state.m_level.ToString(CultureInfo.InvariantCulture)}, allowed level: {maxLevel.ToString(CultureInfo.InvariantCulture)}, points: {points.ToString(CultureInfo.InvariantCulture)}, added: {((int)num).ToString(CultureInfo.InvariantCulture)}, actual: {__instance.m_state.m_points.ToString(CultureInfo.InvariantCulture)}, required points: {nextLevelPoints.ToString(CultureInfo.InvariantCulture)}");

                        if (__instance.m_state.m_points >= Tweakable.Mod.JanitorLevelPoints(1))
                        {
                            if (Tweakable.Vanilla.DlcHospitalServicesEnabled() && (__instance.m_state.m_skillSet.m_specialization1 == null))
                            {
                                GameDBSkill[] skills = new GameDBSkill[]
                                {
                                    Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_VENDOR),
                                    Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_MANAGER)
                                };

                                __instance.m_state.m_skillSet.m_specialization1 = new Skill(skills[UnityEngine.Random.Range(0, skills.Length)], Skills.SkillLevelMinimum);

                                NotificationManager.GetInstance().AddMessage(
                                    __instance.m_entity, "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION",
                                    StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsLabSpecialist[__instance.m_state.m_level], new string[0]),
                                    string.Empty, string.Empty, 0, 0, 0, 0, null, null);

                                levelUp = true;
                            }
                        }

                        if (__instance.m_state.m_points >= nextLevelPoints)
                        {
                            __instance.m_state.m_points = 0;
                            if (__instance.m_state.m_level < maxLevel)
                            {
                                __instance.m_state.m_level++;
                                levelUp = true;

                                NotificationManager.GetInstance().AddMessage(__instance.m_entity, "NOTIF_CHARACTER_LEVELED_UP", StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsDoctor[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
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
                        int doctorMaxLevel = Levels.Doctors.Specialist;
                        if (__instance.IsClinicEmployee())
                        {
                            doctorMaxLevel = Tweakable.Mod.AllowedClinicDoctorsLevel();
                        }

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, level: {__instance.m_state.m_level.ToString(CultureInfo.InvariantCulture)}, allowed level: {doctorMaxLevel.ToString(CultureInfo.InvariantCulture)}");

                        if (__instance.m_state.m_level >= doctorMaxLevel)
                        {
                            __instance.m_state.m_level = doctorMaxLevel;
                            __instance.m_state.m_points = 0;

                        }

                        runOriginalCode = false;
                    }
                }
            }

            if (runOriginalCode)
            {
                float num = (float)points * Tweakable.Vanilla.LevelingRatePercent();
                if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.FastLearner))
                {
                    num *= 1.1f;
                }
                else if (__instance.m_entity.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.SlowLearner))
                {
                    num *= 0.9f;
                }

                __instance.m_state.m_points += (int)num;
                int maxLevel = (__instance.m_entity.GetComponent<BehaviorDoctor>() == null) ? Levels.Nurses.NurseSpecialist : Levels.Doctors.Specialist;
                int nextLevelPoints = __instance.GetPointsNeededForNextLevel();

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, points: {points.ToString(CultureInfo.InvariantCulture)}, added: {((int)num).ToString(CultureInfo.InvariantCulture)}, actual: {__instance.m_state.m_points.ToString(CultureInfo.InvariantCulture)}, required points: {nextLevelPoints.ToString(CultureInfo.InvariantCulture)}");

                if ((__instance.m_state.m_points >= nextLevelPoints) && (__instance.m_state.m_level < maxLevel))
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

                    if (__instance.m_entity.GetComponent<BehaviorDoctor>() != null)
                    {
                        if ((__instance.m_state.m_level > Levels.Doctors.Intern) && (__instance.m_state.m_skillSet.m_specialization1 == null))
                        {
                            __instance.m_state.m_skillSet.AddFirstSpecialization();
                        }
                        if ((__instance.m_state.m_level > Levels.Doctors.Attending) && (__instance.m_state.m_skillSet.m_specialization1 != null) && (__instance.m_state.m_skillSet.m_specialization2 == null))
                        {
                            __instance.m_state.m_skillSet.AddSecondSpecialization(__instance.m_state.m_department.GetEntity().GetDepartmentType(), 1f);
                        }

                        string titleLocID = "NOTIF_CHARACTER_LEVELED_UP";
                        switch (__instance.m_state.m_level)
                        {
                            case Levels.Doctors.Resident:
                                titleLocID = "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION";
                                break;
                            case Levels.Doctors.Fellow:
                                titleLocID = "NOTIF_CHARACTER_LEVELED_UP_SECOND_SPECIALIZATION";
                                break;
                            default:
                                break;
                        }

                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, titleLocID, StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsDoctor[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                    else if (__instance.m_entity.GetComponent<BehaviorNurse>() != null)
                    {
                        if ((__instance.m_state.m_level > Levels.Nurses.NursingIntern) && (__instance.m_state.m_skillSet.m_specialization1 == null))
                        {
                            GameDBSkill[] array = new GameDBSkill[]
                            {
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_RECEPTIONIST),
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_MEDICAL_SURGERY),
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_CLINICAL_SPECIALIST)
                            };
                            __instance.m_state.m_skillSet.m_specialization1 = new Skill(array[UnityEngine.Random.Range(0, array.Length)], Skills.SkillLevelMinimum);
                        }

                        string titleLocID = "NOTIF_CHARACTER_LEVELED_UP";
                        switch (__instance.m_state.m_level)
                        {
                            case Levels.Nurses.RegisteredNurse:
                                titleLocID = "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION";
                                break;
                            default:
                                break;
                        }

                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, titleLocID, StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsNurse[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                    else if (__instance.m_entity.GetComponent<BehaviorLabSpecialist>() != null)
                    {
                        if ((__instance.m_state.m_level > Levels.LabSpecialists.JuniorScientist) && (__instance.m_state.m_skillSet.m_specialization1 == null))
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

                        string titleLocID = "NOTIF_CHARACTER_LEVELED_UP";
                        switch (__instance.m_state.m_level)
                        {
                            case Levels.LabSpecialists.SeniorScientist:
                                titleLocID = "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION";
                                break;
                            default:
                                break;
                        }

                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, titleLocID, StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsLabSpecialist[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                    else if (__instance.m_entity.GetComponent<BehaviorJanitor>() != null)
                    {
                        string titleLocID = "NOTIF_CHARACTER_LEVELED_UP";

                        if (Tweakable.Vanilla.DlcHospitalServicesEnabled() && (__instance.m_state.m_level > Levels.Janitors.Janitor) && (__instance.m_state.m_skillSet.m_specialization1 == null))
                        {
                            GameDBSkill[] array = new GameDBSkill[]
                            {
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_VENDOR),
                                Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_MANAGER)
                            };

                            switch (__instance.m_state.m_level)
                            {
                                case Levels.Janitors.SeniorJanitor:
                                    titleLocID = "NOTIF_CHARACTER_LEVELED_UP_FIRST_SPECIALIZATION";
                                    break;
                                default:
                                    break;
                            }

                            __instance.m_state.m_skillSet.m_specialization1 = new Skill(array[UnityEngine.Random.Range(0, array.Length)], Skills.SkillLevelMinimum);
                        }
                            
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, titleLocID, StringTable.GetInstance().GetLocalizedText(EmployeeComponent.sm_levelLocalizationIDsJanitor[__instance.m_state.m_level], new string[0]), string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                    else
                    {
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, "NOTIF_CHARACTER_LEVELED_UP", "TODO", string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.CheckChiefNodiagnoseDepartment))]
        public static bool CheckChiefNodiagnoseDepartmentPrefix(bool janitorCheck, EmployeeComponent __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            if (__instance.m_state.m_department.GetEntity().GetDepartmentType() == Database.Instance.GetEntry<GameDBDepartment>(Departments.Mod.TrainingDepartment))
            {
                // employee is in training department

                if (janitorCheck && Tweakable.Vanilla.DlcHospitalServicesEnabled())
                {
                    Hospital.Instance.UpdateJanitorBosses();
                    return false;
                }

                Department departmentOfType = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Emergency));

                if (departmentOfType.m_departmentPersistentData.m_chiefDoctor != null)
                {
                    __instance.m_state.m_supervisor = departmentOfType.m_departmentPersistentData.m_chiefDoctor;
                    __instance.CheckBossModifiers();
                }

                return false;
            }

            return true;
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
                __result = Tweakable.Mod.DoctorLevelPoints(Math.Max(Levels.Doctors.Intern, Math.Min(Levels.Doctors.Fellow, __instance.m_state.m_level)));
            }
            else if (__instance.m_entity.GetComponent<BehaviorNurse>() != null)
            {
                // between Levels.Nurses.NursingIntern and Levels.Nurses.RegisteredNurse are two "levels":
                // AGC_TWEAKABLE_NURSE_LEVEL_POINTS_1 and AGC_TWEAKABLE_NURSE_LEVEL_POINTS_2

                // between Levels.Nurses.RegisteredNurse and Levels.Nurses.NurseSpecialist are two "levels":
                // AGC_TWEAKABLE_NURSE_LEVEL_POINTS_3 and AGC_TWEAKABLE_NURSE_LEVEL_POINTS_4

                // we have return correct sum by __instance.m_state.m_level

                switch (__instance.m_state.m_level)
                {
                    case Levels.Nurses.NursingIntern:
                        {
                            __result = Tweakable.Mod.JanitorLevelPoints(1) + Tweakable.Mod.JanitorLevelPoints(2);
                        }
                        break;
                    case Levels.Nurses.RegisteredNurse:
                        {
                            __result = Tweakable.Mod.JanitorLevelPoints(3) + Tweakable.Mod.JanitorLevelPoints(4);
                        }
                        break;
                    default:
                        {
                            __result = Tweakable.Mod.JanitorLevelPoints(4);
                        }
                        break;
                }
            }
            else if (__instance.m_entity.GetComponent<BehaviorLabSpecialist>() != null)
            {
                // between Levels.LabSpecialists.JuniorScientist and Levels.LabSpecialists.SeniorScientist are two "levels":
                // AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_1 and AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_2

                // between Levels.LabSpecialists.SeniorScientist and Levels.LabSpecialists.MasterScientist are two "levels":
                // AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_3 and AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_4

                // we have return correct sum by __instance.m_state.m_level

                switch (__instance.m_state.m_level)
                {
                    case Levels.LabSpecialists.JuniorScientist:
                        {
                            __result = Tweakable.Mod.LabSpecialistLevelPoints(1) + Tweakable.Mod.LabSpecialistLevelPoints(2);
                        }
                        break;
                    case Levels.LabSpecialists.SeniorScientist:
                        {
                            __result = Tweakable.Mod.LabSpecialistLevelPoints(3) + Tweakable.Mod.LabSpecialistLevelPoints(4);
                        }
                        break;
                    default:
                        {
                            __result = Tweakable.Mod.LabSpecialistLevelPoints(4);
                        }
                        break;
                }
            }
            else
            {
                // between Levels.Janitors.Janitor and Levels.Janitors.SeniorJanitor are two "levels":
                // AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_1 and AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_2

                // between Levels.Janitors.SeniorJanitor and Levels.Janitors.MasterJanitor are two "levels":
                // AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_3 and AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_4

                // we have return correct sum by __instance.m_state.m_level

                switch (__instance.m_state.m_level)
                {
                    case Levels.Janitors.Janitor:
                        {
                            __result = Tweakable.Mod.JanitorLevelPoints(1) + Tweakable.Mod.JanitorLevelPoints(2);
                        }
                        break;
                    case Levels.Janitors.SeniorJanitor:
                        {
                            __result = Tweakable.Mod.JanitorLevelPoints(3) + Tweakable.Mod.JanitorLevelPoints(4);
                        }
                        break;
                    default:
                        {
                            __result = Tweakable.Mod.JanitorLevelPoints(4);
                        }
                        break;
                }
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
            Department administrativeDepartment = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.AdministrativeDepartment));
            GameDBProcedure staffTraining = Database.Instance.GetEntry<GameDBProcedure>(Procedures.Vanilla.StaffTraining);
            GameDBRoomType homeRoomType = __instance.GetHomeRoomType();

            if ((homeRoomType != null)
                && (
                    homeRoomType.HasTag(Tags.Mod.DoctorTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace)
                    || homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace)
                    ))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} in training workplace");

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
            else
            {
                // trainee in regular place
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} not in training workplace");

                if (procedureComponent.GetProcedureAvailabilty(staffTraining, __instance.m_entity, administrativeDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE) == ProcedureSceneAvailability.AVAILABLE)
                {
                    BehaviorDoctor doctor = __instance.m_entity.GetComponent<BehaviorDoctor>();
                    if (doctor != null)
                    {
                        doctor.CurrentPatient = null;
                        foreach (Entity entity in Hospital.Instance.m_characters)
                        {
                            if (entity.GetComponent<BehaviorPatient>() != null && entity.GetComponent<BehaviorPatient>().m_state.m_doctor == __instance.m_entity)
                            {
                                entity.GetComponent<BehaviorPatient>().SetDoctor(null);
                            }
                        }
                    }

                    BehaviorNurse nurse = __instance.m_entity.GetComponent<BehaviorNurse>();
                    if (nurse != null)
                    {
                        nurse.CurrentPatient = null;
                        foreach (Entity entity in Hospital.Instance.m_characters)
                        {
                            if (entity.GetComponent<BehaviorPatient>() != null && entity.GetComponent<BehaviorPatient>().m_state.m_nurse == __instance.m_entity)
                            {
                                entity.GetComponent<BehaviorPatient>().m_state.m_nurse = null;
                            }
                        }
                    }

                    BehaviorLabSpecialist labSpecialist = __instance.m_entity.GetComponent<BehaviorLabSpecialist>();
                    if (labSpecialist != null)
                    {
                        labSpecialist.CurrentPatient = null;
                        foreach (Entity entity in Hospital.Instance.m_characters)
                        {
                            if (entity.GetComponent<BehaviorPatient>() != null && entity.GetComponent<BehaviorPatient>().m_state.m_labSpecialist == __instance.m_entity)
                            {
                                entity.GetComponent<BehaviorPatient>().m_state.m_labSpecialist = null;
                            }
                        }
                    }

                    procedureComponent.StartProcedure(staffTraining, __instance.m_entity, administrativeDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} starting training in administrative department");

                    __result = true;
                }
                else if (procedureComponent.GetProcedureAvailabilty(staffTraining, __instance.m_entity, trainingDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE) == ProcedureSceneAvailability.AVAILABLE)
                {
                    BehaviorDoctor doctor = __instance.m_entity.GetComponent<BehaviorDoctor>();
                    if (doctor != null)
                    {
                        doctor.CurrentPatient = null;
                        foreach (Entity entity in Hospital.Instance.m_characters)
                        {
                            if (entity.GetComponent<BehaviorPatient>() != null && entity.GetComponent<BehaviorPatient>().m_state.m_doctor == __instance.m_entity)
                            {
                                entity.GetComponent<BehaviorPatient>().SetDoctor(null);
                            }
                        }
                    }

                    BehaviorNurse nurse = __instance.m_entity.GetComponent<BehaviorNurse>();
                    if (nurse != null)
                    {
                        nurse.CurrentPatient = null;
                        foreach (Entity entity in Hospital.Instance.m_characters)
                        {
                            if (entity.GetComponent<BehaviorPatient>() != null && entity.GetComponent<BehaviorPatient>().m_state.m_nurse == __instance.m_entity)
                            {
                                entity.GetComponent<BehaviorPatient>().m_state.m_nurse = null;
                            }
                        }
                    }

                    BehaviorLabSpecialist labSpecialist = __instance.m_entity.GetComponent<BehaviorLabSpecialist>();
                    if (labSpecialist != null)
                    {
                        labSpecialist.CurrentPatient = null;
                        foreach (Entity entity in Hospital.Instance.m_characters)
                        {
                            if (entity.GetComponent<BehaviorPatient>() != null && entity.GetComponent<BehaviorPatient>().m_state.m_labSpecialist == __instance.m_entity)
                            {
                                entity.GetComponent<BehaviorPatient>().m_state.m_labSpecialist = null;
                            }
                        }
                    }

                    procedureComponent.StartProcedure(staffTraining, __instance.m_entity, trainingDepartment, AccessRights.STAFF, EquipmentListRules.ONLY_FREE);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} starting training in training department");

                    __result = true;
                }
                else
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} not found free space for training");

                    NotificationManager.GetInstance().AddMessage(
                        __instance.m_entity, 
                        "NOTIF_TRAINING_FULL", 
                        StringTable.GetInstance().GetLocalizedText(__instance.m_state.m_trainingData.m_trainingSkillsToTrain[0].m_id.ToString(), new string[0]), 
                        string.Empty, string.Empty, 0, 0, 0, 0, null, null);

                    __instance.m_state.m_trainingData.m_trainingSkillsToTrain.Clear();
                    __instance.m_state.m_trainingData.m_trainingRemainingHours = 0;

                    __result = false;
                }

                return false;
            }
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
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.ResetWorkspace))]
        public static bool ResetWorkspace(bool resetRoom, EmployeeComponent __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_staffShiftsEqual[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            BehaviorJanitor behaviorJanitor = __instance.m_entity.GetComponent<BehaviorJanitor>();
            if (behaviorJanitor != null)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, returned cart");

                behaviorJanitor.m_state.m_cart.GetEntity().User = null;
                behaviorJanitor.m_state.m_cart.GetEntity().SetAttachedToCharacter(false);
                behaviorJanitor.m_state.m_cart.GetEntity().StopSounds();
                behaviorJanitor.m_state.m_cartAvailable = false;

                if (MapScriptInterface.Instance.MoveObject(behaviorJanitor.m_state.m_cart.GetEntity(), behaviorJanitor.m_state.m_cartHomeTile))
                {
                    behaviorJanitor.m_state.m_cart.GetEntity().Orientation = behaviorJanitor.m_state.m_cartHomeOrientation;
                }
                else
                {
                    Hospital.Instance.m_floors[behaviorJanitor.m_state.m_cart.GetEntity().GetFloorIndex()].m_movingObjects.Remove(behaviorJanitor.m_state.m_cart.GetEntity());
                    behaviorJanitor.m_state.m_cart.GetEntity().m_state.m_department.GetEntity().RemoveObject(behaviorJanitor.m_state.m_cart.GetEntity());
                    behaviorJanitor.m_state.m_cart.GetEntity().Destroy();
                }
                behaviorJanitor.m_state.m_cart.GetEntity().StopSounds();
                behaviorJanitor.m_state.m_cart = null;

                behaviorJanitor.m_state.m_cartAvailable = false;
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
        [HarmonyPatch(typeof(EmployeeComponent), nameof(EmployeeComponent.SwitchDepartment))]
        public static bool SwitchDepartmentPrefix(Department department, EmployeeComponent __instance)
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
                // employee in training department, employee is in training workspace

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, switch department, reset training");

                __instance.m_state.m_trainingData.m_trainingRemainingHours = 0;
                __instance.m_state.m_trainingData.m_trainingSkillsToTrain.Clear();
            }

            __instance.m_state.m_department.GetEntity().RemoveCharacter(__instance.m_entity);
            __instance.m_state.m_department = department;
            __instance.m_state.m_department.GetEntity().AddCharacter(__instance.m_entity);
            Hospital.Instance.ValidateDepartments();
            __instance.m_state.m_department.GetEntity().SetChiefDoctor(0, false);
            __instance.ResetWorkspace(false);
            __instance.SwitchDressCodeColors(department, true);

            // go to common room
            if (__instance.m_entity.GetComponent<BehaviorDoctor>() != null)            
            {
                BehaviorDoctor doctor = __instance.m_entity.GetComponent<BehaviorDoctor>();
                Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(doctor);

                if (position != Vector3i.ZERO_VECTOR)
                {
                    __instance.m_entity.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
                }

                doctor.SwitchState(DoctorState.FillingFreeTime);
            }
            if (__instance.m_entity.GetComponent<BehaviorNurse>() != null)
            {
                BehaviorNurse nurse = __instance.m_entity.GetComponent<BehaviorNurse>();
                Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(nurse);

                if (position != Vector3i.ZERO_VECTOR)
                {
                    __instance.m_entity.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
                }

                nurse.SwitchState(NurseState.FillingFreeTime);
            }
            if (__instance.m_entity.GetComponent<BehaviorLabSpecialist>() != null)
            {
                BehaviorLabSpecialist labSpecialist = __instance.m_entity.GetComponent<BehaviorLabSpecialist>();
                Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(labSpecialist);

                if (position != Vector3i.ZERO_VECTOR)
                {
                    __instance.m_entity.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
                }

                labSpecialist.SwitchState(LabSpecialistState.FillingFreeTime);
            }
            if (__instance.m_entity.GetComponent<BehaviorJanitor>() != null)
            {
                BehaviorJanitor janitor = __instance.m_entity.GetComponent<BehaviorJanitor>();
                Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(janitor);

                if (position != Vector3i.ZERO_VECTOR)
                {
                    __instance.m_entity.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
                }

                janitor.SwitchState(BehaviorJanitorState.FillingFreeTime);
            }

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

                if (__instance.m_state.m_trainingData.m_trainingSkillsToTrain.Contains(skill.m_gameDBSkill.Entry))
                {
                    if (__instance.m_state.m_trainingData.m_trainingSkillsToTrain.IndexOf(skill.m_gameDBSkill.Entry) == 0)
                    {
                        if (__instance.m_state.m_trainingData.m_trainingSkillsToTrain.Count != 1)
                        {
                            __instance.m_state.m_trainingData.m_trainingRemainingHours = 1;
                        }
                    }
                    __instance.m_state.m_trainingData.m_trainingSkillsToTrain.Remove(skill.m_gameDBSkill.Entry);
                }
                else
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
                Skill skill = __instance.m_state.m_skillSet.GetSkill(__instance.m_state.m_trainingData.m_trainingSkillsToTrain[UnityEngine.Random.Range(0, __instance.m_state.m_trainingData.m_trainingSkillsToTrain.Count)].Entry);
                if (skill == null)
                {
                    __result = true;
                    return false;
                }

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} training finished, skill {skill.m_gameDBSkill.Entry.DatabaseID}");

                float points = Tweakable.Mod.TrainingHourPoints();
                points *= UnityEngine.Random.Range(0f, 3f);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, points {points.ToString(CultureInfo.InvariantCulture)}");

                if (__instance.m_state.m_supervisor != null)
                {
                    // employee has chief

                    GLib.Entity chief = __instance.m_state.m_supervisor.GetEntity();
                    EmployeeComponent chiefEmployeeComponent = chief.GetComponent<EmployeeComponent>();
                    BehaviorDoctor doctor = chief.GetComponent<BehaviorDoctor>();
                    BehaviorJanitor janitor = chief.GetComponent<BehaviorJanitor>();

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, chief {chief.Name}, doctor {doctor != null}, janitor {janitor != null}");

                    if (janitor != null)
                    {
                        // chief is janitor => janitor manager

                        Skill managementSkill = chiefEmployeeComponent.m_state.m_skillSet.GetSkill(Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_MANAGER));

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, management skill level {(managementSkill?.m_level ?? 0f).ToString(CultureInfo.InvariantCulture)}");

                        if (managementSkill != null)
                        {
                            points *= UnityEngine.Random.Range(0f, managementSkill.m_level);
                        }

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, points {points.ToString(CultureInfo.InvariantCulture)}");
                    }
                    if (doctor != null)
                    {
                        // chief is doctor

                        Skill medicineSkill = chiefEmployeeComponent.m_state.m_skillSet.GetSkill(Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_DOC_QUALIF_GENERAL_MEDICINE));

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, general medicine skill level {(medicineSkill?.m_level ?? 0f).ToString(CultureInfo.InvariantCulture)}");

                        if (medicineSkill != null)
                        {
                            points *= UnityEngine.Random.Range(0f, medicineSkill.m_level);
                        }

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, points {points.ToString(CultureInfo.InvariantCulture)}");
                    }
                }

                skill.AddPoints((int)points, __instance.m_entity);
                __instance.AddExperiencePoints((int)points);

                if (skill.m_level >= Skills.SkillLevelMaximum)
                {
                    // skill was trained to maximum level
                    // remove skill from training

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} training finished, skill {skill.m_gameDBSkill.Entry.DatabaseID} on maximum level, removing from training");

                    NotificationManager.GetInstance().AddMessage(
                        __instance.m_entity, 
                        "NOTIF_TRAINING_FINISHED", 
                        StringTable.GetInstance().GetLocalizedText(skill.m_gameDBSkill.m_id.ToString(), new string[0]), 
                        string.Empty, 
                        string.Empty, 
                        0, 0, 0, 0, null, null);

                    if (__instance.m_state.m_trainingData.m_trainingSkillsToTrain.Contains(skill.m_gameDBSkill.Entry))
                    {
                        __instance.m_state.m_trainingData.m_trainingSkillsToTrain.Remove(skill.m_gameDBSkill.Entry);
                    }
                }

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

