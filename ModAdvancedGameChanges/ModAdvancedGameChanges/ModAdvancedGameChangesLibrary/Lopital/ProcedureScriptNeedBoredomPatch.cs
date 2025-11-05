using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptNeedBoredom))]
    public static class ProcedureScriptNeedBoredomPatch
    {
		[HarmonyPrefix]
		[HarmonyPatch(typeof(ProcedureScriptNeedBoredom), nameof(ProcedureScriptNeedBoredom.Activate))]
		public static bool ActivatePrefix(ProcedureScriptNeedBoredom __instance)
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
				if (tileObject.User == null)
				{
					__instance.m_stateData.m_procedureScene.m_equipment[0] = tileObject;
					mainCharacter.GetComponent<UseComponent>().ReserveObject(__instance.GetEquipment(0));
					mainCharacter.GetComponent<WalkComponent>().SetDestination(__instance.GetEquipment(0).GetDefaultUsePosition(), __instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);
					__instance.SwitchState(ProcedureScriptNeedBoredom.STATE_GOING_TO_OBJECT);

					return false;
				}
			}

			if (mainCharacter.GetComponent<WalkComponent>().IsSitting())
			{
				mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.SitIdleHoldPhone, true);
				mainCharacter.GetComponent<SpeechComponent>().SetBubble("BUBBLE_BORED", 5f);
				__instance.SwitchState(ProcedureScriptNeedBoredom.STATE_ON_PHONE);

				return false;
			}

			mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
			mainCharacter.GetComponent<SpeechComponent>().SetBubble("BUBBLE_BORED", 5f);
			__instance.SwitchState(ProcedureScriptNeedBoredom.STATE_STANDING_YAWNING);

			return false;
        }

		//[HarmonyPrefix]
		//[HarmonyPatch(typeof(ProcedureScriptNeedBoredom), nameof(ProcedureScriptNeedBoredom.ScriptUpdate))]
		//public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptNeedBoredom __instance)
  //      {
		//	if (!ViewSettingsPatch.m_enabled)
		//	{
		//		// Allow original method to run
		//		return true;
		//	}

		//	__instance.m_stateData.m_timeInState += deltaTime;
		//	Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;

  //          switch (__instance.m_stateData.m_state)
  //          {
		//		case ProcedureScriptNeedBoredom.STATE_GOING_TO_OBJECT:
  //                  {
		//				if ((!mainCharacter.GetComponent<WalkComponent>().IsBusy()) && (!mainCharacter.GetComponent<UseComponent>().IsBusy()))
		//				{
		//					__instance.SwitchState(ProcedureScriptNeedBoredom.STATE_USING_OBJECT);
		//					__instance.m_stateData.m_procedureScene.MainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
		//				}
		//			}
		//			break;
		//		case ProcedureScriptNeedBoredom.STATE_USING_OBJECT:
  //                  {
		//				if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
		//				{
		//					mainCharacter.GetComponent<BehaviorPatient>().ReceiveMessage(new Message(Messages.BOREDOM_REDUCED, 70f));
		//					__instance.SwitchState(ProcedureScriptNeedBoredom.STATE_IDLE);
		//				}
		//			}
		//			break;
		//		case ProcedureScriptNeedBoredom.STATE_ON_PHONE:
  //                  {
		//				if (__instance.m_stateData.m_timeInState > 15f)
		//				{
		//					__instance.m_stateData.m_procedureScene.MainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
		//					mainCharacter.GetComponent<BehaviorPatient>().m_state.m_chair = mainCharacter.GetComponent<WalkComponent>().m_state.m_objectSittingOn;
		//					mainCharacter.GetComponent<BehaviorPatient>().ReceiveMessage(new Message(Messages.BOREDOM_REDUCED, 40f));
		//					__instance.SwitchState(ProcedureScriptNeedBoredom.STATE_IDLE);
		//				}
		//			}
		//			break;
		//		case ProcedureScriptNeedBoredom.STATE_STANDING_YAWNING:
  //                  {
		//				if (__instance.m_stateData.m_timeInState > 10f)
		//				{
		//					__instance.m_stateData.m_procedureScene.MainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
		//					mainCharacter.GetComponent<BehaviorPatient>().ReceiveMessage(new Message(Messages.BOREDOM_REDUCED, 40f));
		//					__instance.SwitchState(ProcedureScriptNeedBoredom.STATE_IDLE);
		//				}
		//			}
		//			break;
		//		default:
  //                  break;
  //          }

  //          return false;
		//}
	}
}
