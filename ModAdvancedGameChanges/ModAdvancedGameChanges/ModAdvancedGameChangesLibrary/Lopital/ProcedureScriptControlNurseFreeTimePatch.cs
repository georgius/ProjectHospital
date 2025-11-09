using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Reflection;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime))]
    public static class ProcedureScriptControlNurseFreeTimePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime), nameof(ProcedureScriptControlNurseFreeTime.Activate))]
        public static bool ActivatePrefix(ProcedureScriptControlNurseFreeTime __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_fulfillingNeedsChanges[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, activating script {__instance.m_stateData.m_scriptName}");

            __instance.SetParam(ProcedureScriptControlNurseFreeTime.PARAM_CYCLES, 0f);
            __instance.ChooseSomethingInternal();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime), "ChooseSomething")]
        public static bool ChooseSomethingPrefix(ProcedureScriptControlNurseFreeTime __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_fulfillingNeedsChanges[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            Department department = mainCharacter.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();

            __instance.SetParam(0, 0f);

            string tag = Tags.Vanilla.Rest;
            //if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.Scholar))
            //{
            //    tag = (((int)__instance.GetParam(ProcedureScriptControlNurseFreeTime.PARAM_CYCLES) % 2 != 0) ? Tags.Vanilla.Rest : Tags.Vanilla.Education);
            //}

            // try to find appropriate place in home room
            if ((__instance.m_stateData.m_procedureScene.m_equipment[0] == null) && (mainCharacter.GetComponent<EmployeeComponent>().m_state.m_homeRoom != null))
            {
                __instance.m_stateData.m_procedureScene.m_equipment[0] = MapScriptInterface.Instance.FindClosestFreeObjectWithTags(
                    mainCharacter, mainCharacter, 
                    mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), 
                    mainCharacter.GetComponent<EmployeeComponent>().m_state.m_homeRoom.GetEntity(), 
                    new string[] { tag }, AccessRights.STAFF, false, null, false);
            }

            // try to find appropriate place in department in common room
            if (__instance.m_stateData.m_procedureScene.m_equipment[0] == null)
            {
                __instance.m_stateData.m_procedureScene.m_equipment[0] = MapScriptInterface.Instance.FindClosestFreeObjectWithTags(
                    mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), mainCharacter.GetComponent<WalkComponent>().GetFloorIndex(), 
                    department, new string[] { tag }, AccessRights.STAFF, Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.CommonRoom));
            }

            // try to find appropriate place in and common room
            if (__instance.m_stateData.m_procedureScene.m_equipment[0] == null)
            {
                __instance.m_stateData.m_procedureScene.m_equipment[0] = MapScriptInterface.Instance.FindClosestCenterObjectWithTagShortestPath(
                    mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), mainCharacter.GetComponent<WalkComponent>().GetFloorIndex(), 
                    tag, AccessRights.STAFF, new string[] { RoomTypes.Vanilla.CommonRoom }, false, true, null, null);
            }

            if (__instance.m_stateData.m_procedureScene.m_equipment[0] != null)
            {
                __instance.MoveCharacter(mainCharacter, __instance.GetEquipment(0).GetDefaultUsePosition(), __instance.GetEquipment(0).GetFloorIndex());
                __instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_GOING_TO_OBJECT);
            }
            else
            {
                if (mainCharacter.GetComponent<MoodComponent>().HasSatisfactionModifier(SatisfationModifiers.Vanilla.Entertained))
                {
                    mainCharacter.GetComponent<MoodComponent>().RemoveSatisfactionModifier(SatisfationModifiers.Vanilla.Entertained);
                }

                if (!mainCharacter.GetComponent<MoodComponent>().HasSatisfactionModifier(SatisfationModifiers.Vanilla.Bored))
                {
                    mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Bored, 5f);
                    mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.Bored);
                }

                __instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_BLOCKED_CONFUSED);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime), "UpdateStateBlocked")]
        public static bool UpdateStateBlockedPrefix(ProcedureScriptControlNurseFreeTime __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_fulfillingNeedsChanges[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            __instance.ChooseSomethingInternal();

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime), "UpdateStateGoingToObject")]
        public static bool UpdateStateGoingToObjectPrefix(ProcedureScriptControlNurseFreeTime __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_fulfillingNeedsChanges[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (__instance.GetEquipment(0).User == null)
                {
                    // object is not reserved
                    mainCharacter.GetComponent<UseComponent>().ReserveObject(__instance.GetEquipment(0));

                    // sit on object
                    mainCharacter.GetComponent<WalkComponent>().GoSit(__instance.GetEquipment(0), MovementType.WALKING);

                    // find TV
                    Department department = mainCharacter.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();
                    TileObject television = MapScriptInterface.Instance.FindClosestObjectWithTag(
                        mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), mainCharacter.GetComponent<WalkComponent>().GetFloorIndex(), 
                        department, Tags.Vanilla.Television, AccessRights.STAFF, null, false, true, false, false);

                    if ((television != null) && (!television.m_state.m_lightEnabled))
                    {
                        television.SetLightEnabled(true);
                        television.GetComponent<AnimatedObjectComponent>().ForceFrame(1);
                        television.PlayStartUseSound();
                        mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Television, 3f);
                    }
                    else
                    {
                        mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.FreeTime, 3f);
                    }

                    __instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_RESTING);
                }
                else
                {
                    mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Question, -1f);
                    __instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_BLOCKED_CONFUSED);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime), "UpdateStateResting")]
        public static bool UpdateStateRestingPrefix(ProcedureScriptControlNurseFreeTime __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_fulfillingNeedsChanges[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            if (DayTime.Instance.IngameTimeHoursToRealTimeSeconds(0.5f) < __instance.m_stateData.m_timeInState)
            {
                mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.REST_REDUCED, 1f));
                __instance.m_stateData.m_timeInState = 0f;

                // free reserved object
                if (__instance.GetEquipment(0).User == __instance)
                {
                    mainCharacter.GetComponent<UseComponent>().Deactivate();
                    __instance.GetEquipment(0).User = null;
                }

                // stand up
                if (!mainCharacter.GetComponent<WalkComponent>().IsSitting())
                {
                    mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
                }

                __instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_IDLE);
            }

            return false;
        }

        private static void ChooseSomethingInternal(this ProcedureScriptControlNurseFreeTime instance)
        {
            Type type = typeof(ProcedureScriptControlNurseFreeTime);
            MethodInfo methodInfo = type.GetMethod("ChooseSomething", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }
    }
}
