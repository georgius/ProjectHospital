using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges;
using System.Globalization;
using UnityEngine;

namespace ModGameChanges.Lopital
{
    [HarmonyPatch(typeof(Skill))]
    public static class SkillPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Skill), nameof(Skill.AddPoints))]
        public static bool AddPointsPrefix(int points, Entity character, Skill __instance, ref AddExperienceResult __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enableNonLinearSkillLeveling[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {character.Name} Points: {points.ToString(CultureInfo.InvariantCulture)} Skill: {__instance.m_gameDBSkill.m_id} Level: {__instance.m_level.ToString(CultureInfo.InvariantCulture)}");

            float num = (float)points;

            if (Tweakable.Mod.EnableExtraLevelingPercent() && (__instance.m_gameDBSkill.Entry.ExtraLevelingPercent > 0))
            {
                num *= 0.01f * (100f + (float)__instance.m_gameDBSkill.Entry.ExtraLevelingPercent);
            }

            if (character.GetComponent<PerkComponent>().m_perkSet.HasPerk(Constants.Perks.Vanilla.FastLearner))
            {
                num *= 1.3f;
            }
            else if (character.GetComponent<PerkComponent>().m_perkSet.HasPerk(Constants.Perks.Vanilla.SlowLearner))
            {
                num *= 0.7f;
            }

            num /= (float)__instance.GetPointsNeededForNextLevel();

            __instance.m_level += num;

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {character.Name} Level: {__instance.m_level.ToString(CultureInfo.InvariantCulture)}");

            if (SettingsManager.Instance.m_gameSettings.m_showXPInGame.m_value)
            {
                if (points > 0)
                {
                    NotificationManager.GetInstance().AddFloatingIngameNotification(character, "XP +" + points, new Color(1f, 1f, 0.25f));
                }
                else
                {
                    character.LogWarning("Zero added skill xp");
                }
            }
            __instance.m_level = Mathf.Min(5f, __instance.m_level);

            __result = AddExperienceResult.NONE;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Skill), nameof(Skill.GetPointsNeededForNextLevel))]
        public static bool GetPointsNeededForNextLevel(Skill __instance, ref int __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enableNonLinearSkillLeveling[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            // the minimum __instance.m_level is 1, the maximum is 5
            // we convert the value to 0 - 4
            float level = Mathf.Max(1, Mathf.Min(5, __instance.m_level)) - 1;
            var interval = (float)4 / (float)Tweakable.Mod.SkillLevels();
            var index = (int)Mathf.Max(0, Mathf.Min(4, level / interval));

            __result = Tweakable.Mod.SkillPoints(index);

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Skill: {__instance.m_gameDBSkill.m_id} Level: {__instance.m_level.ToString(CultureInfo.InvariantCulture)} Result: {__result.ToString(CultureInfo.InvariantCulture)}");

            return false;
        }
    }
}
