using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Helpers;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(BehaviorPedestrian))]
    public static class BehaviorPedestrianPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPedestrian), nameof(BehaviorPedestrian.SwitchState))]
        public static bool SwitchStatePrefix(BehaviorPedestrianState state, BehaviorPedestrian __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enablePedestrianGoToPharmacy[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // allow original method to run
                return true;
            }

            if (__instance.m_state.m_state != state)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, switching state from {__instance.m_state.m_state} to {state}");

                __instance.m_state.m_state = state;
                __instance.m_state.m_timeInState = 0f;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPedestrian), nameof(BehaviorPedestrian.Update))]
        public static bool UpdatePrefix(float deltaTime, BehaviorPedestrian __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enablePedestrianGoToPharmacy[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // allow original method to run
                return true;
            }

            if (deltaTime <= 0f)
            {
                return false;
            }

            __instance.m_state.m_timeInState += deltaTime;

            switch (__instance.m_state.m_state)
            {
                case BehaviorPedestrianState.Uninitialized:
                    __instance.SwitchState(BehaviorPedestrianState.Idle);
                    break;
                case BehaviorPedestrianState.Idle:
                    __instance.UpdateStateIdle();
                    break;
                case BehaviorPedestrianState.Walking:
                    __instance.UpdateStateWalking();
                    break;
                case BehaviorPedestrianState.IdleAtDestination:
                    __instance.UpdateStateIdleAtDestination();
                    break;
                case BehaviorPedestrianState.WalkingBack:
                    __instance.UpdateStateWalkingBack();
                    break;
                default:
                    break;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPedestrian), "UpdateStateIdle")]
        public static bool UpdateStateIdlePrefix(BehaviorPedestrian __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enablePedestrianGoToPharmacy[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // allow original method to run
                return true;
            }

            if (__instance.m_state.m_timeInState > __instance.m_state.m_respawnTimeSeconds)
            {
                __instance.GetComponent<WalkComponent>().SetDestination(MapScriptInterface.Instance.DEBUG_GetPedestiranDestinationPosition(), 0, MovementType.WALKING);
                __instance.SwitchState(BehaviorPedestrianState.Walking);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPedestrian), "UpdateStateIdleAtDestination")]
        public static bool UpdateStateIdleAtDestinationPrefix(BehaviorPedestrian __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enablePedestrianGoToPharmacy[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // allow original method to run
                return true;
            }

            if (__instance.m_state.m_timeInState > __instance.m_state.m_respawnTimeSeconds)
            {
                __instance.GetComponent<WalkComponent>().SetDestination(MapScriptInterface.Instance.DEBUG_GetPedestiranSpawnPosition(), 0, MovementType.WALKING);
                __instance.SwitchState(BehaviorPedestrianState.WalkingBack);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPedestrian), "UpdateStateWalking")]
        public static bool UpdateStateWalkingPrefix(BehaviorPedestrian __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enablePedestrianGoToPharmacy[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                __instance.Reset();
                __instance.SwitchState(BehaviorPedestrianState.IdleAtDestination);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPedestrian), "UpdateStateWalkingBack")]
        public static bool UpdateStateWalkingBackPrefix(BehaviorPedestrian __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enablePedestrianGoToPharmacy[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                __instance.Reset();
                __instance.SwitchState(BehaviorPedestrianState.Idle);
            }

            return false;
        }
    }

    public static class BehaviorPedestrianExtensions
    {
        public static void Reset(this BehaviorPedestrian instance)
        {
            MethodAccessHelper.CallMethod(instance, "Reset");
        }

        public static void UpdateStateIdle(this BehaviorPedestrian instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateIdle");
        }

        public static void UpdateStateIdleAtDestination(this BehaviorPedestrian instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateIdleAtDestination");
        }

        public static void UpdateStateWalking(this BehaviorPedestrian instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateWalking");
        }

        public static void UpdateStateWalkingBack(this BehaviorPedestrian instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateWalkingBack");
        }
    }
}
