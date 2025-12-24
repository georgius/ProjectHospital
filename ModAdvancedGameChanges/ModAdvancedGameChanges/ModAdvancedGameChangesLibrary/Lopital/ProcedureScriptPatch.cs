using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System.Globalization;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScript))]
    public static class ProcedureScriptPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScript), nameof(ProcedureScript.GetActionTime))]
        public static bool GetActionTimePrefix(Entity character, int characterTimeConstant, float characterSkill, ProcedureScript __instance, ref float __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            float skillRatio = UnityEngine.Mathf.Max(Tweakable.Mod.EfficiencyMinimum() / 100f, (characterSkill - Skills.SkillLevelMinimum) / (Skills.SkillLevelMaximum - Skills.SkillLevelMinimum));
            float efficiency = character.GetComponent<EmployeeComponent>().GetEfficiencyTimeMultiplier();

            __result = ((float)characterTimeConstant) * efficiency / skillRatio;

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{character?.Name ?? "NULL"}, script {__instance.m_stateData.m_scriptName}, base {characterTimeConstant.ToString(CultureInfo.InvariantCulture)}, efficiency {efficiency.ToString(CultureInfo.InvariantCulture)}, skill ratio {skillRatio.ToString(CultureInfo.InvariantCulture)}, result {__result.ToString(CultureInfo.InvariantCulture)}");

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScript), nameof(ProcedureScript.SwitchState))]
        public static bool SwitchStatePrefix(string state, ProcedureScript __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            if (__instance.m_stateData.m_state != state)
            {
                Entity entity = (__instance.m_stateData.m_procedureScene.m_patient != null) ? __instance.m_stateData.m_procedureScene.m_patient.GetEntity() : null;
                entity = entity ?? __instance.m_stateData.m_procedureScene.EmployeeCharacter;

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{entity?.Name ?? "NULL"}, script {__instance.m_stateData.m_scriptName}, switching state from {__instance.m_stateData.m_state} to {state}");
            }

            return true;
        }

        public const string STATE_RESERVED = "STATE_RESERVED";
    }
}
