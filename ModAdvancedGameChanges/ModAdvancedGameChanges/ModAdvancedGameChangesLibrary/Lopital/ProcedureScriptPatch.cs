using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;

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

            float ratio = (characterSkill - Skills.SkillLevelMinimum) / (Skills.SkillLevelMaximum - Skills.SkillLevelMinimum);
            float reduction = UnityEngine.Random.Range(Tweakable.Mod.SkillTimeReductionMinimum(), Tweakable.Mod.SkillTimeReductionMaximum()) * ratio / 100f;

            __result = ((float)characterTimeConstant) * character.GetComponent<EmployeeComponent>().GetEfficiencyTimeMultiplier() * (1 - reduction);
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
