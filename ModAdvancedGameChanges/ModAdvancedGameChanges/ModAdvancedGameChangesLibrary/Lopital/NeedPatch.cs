using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System.Globalization;
using UnityEngine;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(Need))]
    public static class NeedPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Need), nameof(Need.ReduceRandomized))]
        public static bool ReduceRandomizedPrefix(float value, MoodComponent moodComponent, Need __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            float reduction = UnityEngine.Random.Range(Tweakable.Mod.FulfillNeedsReductionMinimum(), Tweakable.Mod.FulfillNeedsReductionMaximum());

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{moodComponent.m_entity.Name}, need {__instance.m_gameDBNeed.Entry.DatabaseID}, current value {__instance.m_currentValue.ToString(CultureInfo.InvariantCulture)}, reduction {reduction.ToString(CultureInfo.InvariantCulture)}");

            __instance.m_currentValue -= reduction;
            __instance.m_currentValue = Mathf.Max(Needs.NeedMinimum, Mathf.Min(Needs.NeedMaximum, __instance.m_currentValue));

            if ((__instance.m_currentValue < Tweakable.Mod.FulfillNeedsCriticalThreshold()) && (__instance.m_gameDBNeed.Entry.SatisfactionModifierCritical != null))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{moodComponent.m_entity.Name}, need {__instance.m_gameDBNeed.Entry.DatabaseID}, removed {__instance.m_gameDBNeed.Entry.SatisfactionModifierCritical.Entry.DatabaseID}");

                moodComponent.RemoveSatisfactionModifier(__instance.m_gameDBNeed.Entry.SatisfactionModifierCritical.Entry.DatabaseID.ToString());
            }
            if ((__instance.m_currentValue < Tweakable.Mod.FulfillNeedsThreshold()) && (__instance.m_gameDBNeed.Entry.SatisfactionModifierSatisfied != null))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{moodComponent.m_entity.Name}, need {__instance.m_gameDBNeed.Entry.DatabaseID}, added {__instance.m_gameDBNeed.Entry.SatisfactionModifierSatisfied.Entry.DatabaseID}");

                moodComponent.AddSatisfactionModifier(__instance.m_gameDBNeed.Entry.SatisfactionModifierSatisfied.Entry.DatabaseID.ToString());
            }

            return false;
        }
    }
}
