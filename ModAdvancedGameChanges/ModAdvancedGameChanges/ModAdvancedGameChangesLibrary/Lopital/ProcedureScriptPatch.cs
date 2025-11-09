using GLib;
using HarmonyLib;
using Lopital;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScript))]
    public static class ProcedureScriptPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScript), nameof(ProcedureScript.SwitchState))]
        public static bool SwitchStatePrefix(string state, ProcedureScript __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
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
