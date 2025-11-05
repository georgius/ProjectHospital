using GLib;
using HarmonyLib;
using Lopital;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptNeedHunger))]
    public static class ProcedureScriptNeedHungerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptNeedHunger), nameof(ProcedureScriptNeedHunger.Activate))]
        public static bool ActivatePreifx(ProcedureScriptNeedHunger __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            mainCharacter.GetComponent<WalkComponent>().SetDestination(__instance.GetEquipment(0).GetDefaultUsePosition(), __instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);
            __instance.SwitchState("GOING_TO_OBJECT");
            __instance.SpeakCharacter(mainCharacter, null, "BUBBLE_FOOD", 4f);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptNeedHunger), "UpdateStateGoingToObject")]
        public static bool UpdateStateGoingToObjectPrefix(ProcedureScriptNeedHunger __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (__instance.GetEquipment(0).User == null)
                {
                    // no user, we can reserve
                    mainCharacter.GetComponent<UseComponent>().ReserveObject(__instance.GetEquipment(0));

                    __instance.SwitchState("USING_OBJECT");
                    mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
                }
            }

            return false;
        }
    }
}
