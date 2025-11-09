using GLib;
using HarmonyLib;
using Lopital;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptStaffLunch))]
    public static class ProcedureScriptStaffLunchPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptStaffLunch), "UpdateStateEatingFinished")]
        public static bool UpdateStateEatingFinishedPrefix(ProcedureScriptStaffLunch __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;

            if (mainCharacter.GetComponent<BehaviorDoctor>() != null)
            {
                mainCharacter.GetComponent<BehaviorDoctor>().m_state.m_hadLunch = true;
            }

            if (mainCharacter.GetComponent<BehaviorNurse>() != null)
            {
                mainCharacter.GetComponent<BehaviorNurse>().m_state.m_hadLunch = true;
            }

            if (mainCharacter.GetComponent<BehaviorLabSpecialist>() != null)
            {
                mainCharacter.GetComponent<BehaviorLabSpecialist>().m_state.m_hadLunch = true;
            }

            if (mainCharacter.GetComponent<BehaviorJanitor>() != null)
            {
                mainCharacter.GetComponent<BehaviorJanitor>().m_state.m_hadLunch = true;
            }

            return true;
        }
    }
}
