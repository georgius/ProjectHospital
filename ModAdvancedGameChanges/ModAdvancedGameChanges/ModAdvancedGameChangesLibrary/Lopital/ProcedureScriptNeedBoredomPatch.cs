using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptNeedBoredom))]
    public static class ProcedureScriptNeedBoredomPatch
    {
        public static bool Activate(ProcedureScriptNeedBoredom __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

			Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;

			TileObject tileObject = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
				mainCharacter, __instance, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), 
				MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>()), 
				Tags.Vanilla.Distraction, AccessRights.PATIENT, false, null, false);

			if (tileObject != null)
			{
				__instance.m_stateData.m_procedureScene.m_equipment[0] = tileObject;
				mainCharacter.GetComponent<UseComponent>().ReserveObject(__instance.GetEquipment(0));
				mainCharacter.GetComponent<WalkComponent>().SetDestination(__instance.GetEquipment(0).GetDefaultUsePosition(), __instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);
				__instance.SwitchState("GOING_TO_OBJECT");

				return false;
			}

			if (mainCharacter.GetComponent<WalkComponent>().IsSitting())
			{
				mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.SitIdleHoldPhone, true);
				mainCharacter.GetComponent<SpeechComponent>().SetBubble("BUBBLE_BORED", 5f);
				__instance.SwitchState("ON_PHONE");

				return false;
			}

			mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
			mainCharacter.GetComponent<SpeechComponent>().SetBubble("BUBBLE_BORED", 5f);
			__instance.SwitchState("STANDING_YAWNING");

			return false;
        }
    }
}
