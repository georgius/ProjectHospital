using HarmonyLib;
using Lopital;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime))]
    public static class ProcedureScriptControlNurseFreeTimePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime), nameof(ProcedureScriptControlNurseFreeTime.ScriptUpdate))]
        public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptControlNurseFreeTime __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            //Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_stateData.m_procedureScene.MainCharacter.Name}  {__instance.m_stateData.m_state}");

            return true;
        }
    }
}
