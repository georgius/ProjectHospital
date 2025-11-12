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

			if (mainCharacter.GetComponent<WalkComponent>().IsSitting())
            {
				mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.SitIdleHoldPhone, true);
				mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Bored, 5f);
				
				__instance.SwitchState(ProcedureScriptNeedBoredomPatch.STATE_USING_PHONE);
			}
			else
            {
				TileObject tileObject = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
					mainCharacter, __instance, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), 
					MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>()), 
					Tags.Vanilla.Distraction, AccessRights.PATIENT, false, null, false);

				if (tileObject != null)
				{
					__instance.m_stateData.m_procedureScene.m_equipment[0] = tileObject;
					mainCharacter.GetComponent<UseComponent>().ReserveObject(__instance.GetEquipment(0));

					mainCharacter.GetComponent<WalkComponent>().SetDestination(__instance.GetEquipment(0).GetDefaultUsePosition(), __instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);
					__instance.SwitchState(ProcedureScriptNeedBoredomPatch.STATE_GOING_TO_OBJECT);
				}
				else
                {
					__instance.SwitchState(ProcedureScriptNeedBoredom.STATE_IDLE);
				}
			}

			return false;
        }

		[HarmonyPrefix]
		[HarmonyPatch(typeof(ProcedureScriptNeedBoredom), nameof(ProcedureScriptNeedBoredom.ScriptUpdate))]
		public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptNeedBoredom __instance)
		{
			if (!ViewSettingsPatch.m_enabled)
			{
				// Allow original method to run
				return true;
			}

			__instance.m_stateData.m_timeInState += deltaTime;
			switch (__instance.m_stateData.m_state)
			{
				case ProcedureScriptNeedBoredomPatch.STATE_GOING_TO_OBJECT:
					ProcedureScriptNeedBoredomPatch.UpdateStateGoingToObject(__instance);
					break;
				case ProcedureScriptNeedBoredomPatch.STATE_USING_OBJECT:
					ProcedureScriptNeedBoredomPatch.UpdateStateUsingObject(__instance);
					break;
				case ProcedureScriptNeedBoredomPatch.STATE_USING_PHONE:
					ProcedureScriptNeedBoredomPatch.UpdateStateUsingPhone(__instance);
					break;
				default:
					break;
			}

			return false;
		}

		public static void UpdateStateGoingToObject(ProcedureScriptNeedBoredom instance)
        {
			Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

			if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
			{
				instance.SwitchState(ProcedureScriptNeedBoredomPatch.STATE_USING_OBJECT);
				mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
			}
		}

		public static void UpdateStateUsingObject(ProcedureScriptNeedBoredom instance)
        {
			Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

			if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
			{
				// free reserved object
				instance.GetEquipment(0).User = null;

				mainCharacter.GetComponent<BehaviorPatient>().ReceiveMessage(new Message(Messages.BOREDOM_REDUCED, 1f));

				instance.SwitchState(ProcedureScriptNeedBoredom.STATE_IDLE);
			}
		}

		public static void UpdateStateUsingPhone(ProcedureScriptNeedBoredom instance)
        {
			Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

			if (instance.m_stateData.m_timeInState > UnityEngine.Random.Range(10f, 15f))
			{
				instance.m_stateData.m_procedureScene.MainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.SitIdle, true);
				mainCharacter.GetComponent<BehaviorPatient>().m_state.m_chair = mainCharacter.GetComponent<WalkComponent>().m_state.m_objectSittingOn;

				mainCharacter.GetComponent<BehaviorPatient>().ReceiveMessage(new Message(Messages.BOREDOM_REDUCED, 1f));
				instance.SwitchState(ProcedureScriptNeedBoredom.STATE_IDLE);
			}
		}

		public const string STATE_GOING_TO_OBJECT = "STATE_GOING_TO_OBJECT";
		public const string STATE_USING_OBJECT = "STATE_USING_OBJECT";
		public const string STATE_USING_PHONE = "STATE_USING_PHONE";
	}
}
