using HarmonyLib;
using Lopital;
using ModGameChanges;
using System;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(SkillSet))]
    public static class SkillSetPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillSet), nameof(SkillSet.CreateDoctorSkillSet))]
        public static bool CreateDoctorSkillSetPrefix(GameDBDepartment department, int level, GameDBSkill requiredSkill, ref SkillSet __result)
        {
            __result = new SkillSet();
            __result.m_qualifications.Add(
                new Skill(
                    Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_DOC_QUALIF_GENERAL_MEDICINE),
                    UnityEngine.Random.Range(Math.Max(1f, (float)level - 1f), (float)level)));
            __result.m_qualifications.Add(
                new Skill(
                    Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_DOC_QUALIF_DIAGNOSIS),
                    UnityEngine.Random.Range(Math.Max(1f, (float)level - 1f), (float)level)));

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillSet), nameof(SkillSet.DEBUG_CreateNurseSkillSet))]
        public static bool DEBUG_CreateNurseSkillSetPrefix(int level, GameDBSkill requiredSkill, ref SkillSet __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_ForceEmployeeLowestHireLevel[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

			__result = new SkillSet();
			__result.m_qualifications.Add(
				new Skill(
					Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_NURSE_QUALIF_PATIENT_CARE),
					UnityEngine.Random.Range(Math.Max(1f, (float)level - 1f), (float)level)));

			return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillSet), nameof(SkillSet.DEBUG_CreateLabSpecialistSkillSet))]
        public static bool DEBUG_CreateLabSpecialistSkillSetPrefix(GameDBDepartment department, int level, GameDBSkill requiredSkill, ref SkillSet __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_ForceEmployeeLowestHireLevel[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            __result = new SkillSet();
            __result.m_qualifications.Add(
                new Skill(
                    Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_LAB_SPECIALIST_QUALIF_SCIENCE_EDUCATION),
                    UnityEngine.Random.Range(Math.Max(1f, (float)level - 1f), (float)level)));

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SkillSet), nameof(SkillSet.DEBUG_CreateJanitorSkillSet))]
        public static bool DEBUG_CreateJanitorSkillSetPrefix(int level, GameDBSkill requiredSkill, ref SkillSet __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_ForceEmployeeLowestHireLevel[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            __result = new SkillSet();
            __result.m_qualifications.Add(
                new Skill(
                    Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_JANITOR_QUALIF_EFFICIENCY),
                    UnityEngine.Random.Range(Math.Max(1f, (float)level - 1f), (float)level)));
            __result.m_qualifications.Add(
                new Skill(
                    Database.Instance.GetEntry<GameDBSkill>(Constants.Skills.Vanilla.SKILL_JANITOR_QUALIF_DEXTERITY),
                    UnityEngine.Random.Range(Math.Max(1f, (float)level - 1f), (float)level)));

            return false;
        }
    }
}
