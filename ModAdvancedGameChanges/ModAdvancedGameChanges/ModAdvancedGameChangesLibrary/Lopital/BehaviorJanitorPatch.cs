using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Globalization;
using System.Reflection;

namespace ModGameChanges.Lopital
{
    [HarmonyPatch(typeof(BehaviorJanitor))]
    public static class BehaviorJanitorPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), nameof(BehaviorJanitor.AddToHospital))]
        public static bool AddToHospitalPrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            if (homeRoomType != null && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
            {
                // janitor in training, go to working place
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - going to training workplace");

                employeeComponent.CheckChiefNodiagnoseDepartment(true);

                __instance.GetComponent<WalkComponent>().GoSit(employeeComponent.GetWorkChair(), MovementType.WALKING);
                __instance.SwitchState(BehaviorJanitorState.GoingToWorkplace);

                __instance.GetComponent<WalkComponent>().Floor = Hospital.Instance.GetCurrentFloor();

                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateAtHome")]
        public static bool UpdateStateAtHomePrefix(BehaviorJanitor __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            employeeComponent.CheckNoWorkplaceAtHome();

            if (__instance.m_state.m_timeInState > 30f)
            {
                __instance.GetComponent<MoodComponent>().Reset(10f);
                __instance.m_state.m_timeInState = 0f;
            }

            if (employeeComponent.m_state.m_shift == Shift.NIGHT)
            {
                __instance.m_state.m_hadLunch = true;
            }

            // janitors don't have commuting state as doctors, nurses and lab specialists
            // we just keep them at home longer time

            if (ViewSettingsPatch.m_enabledTrainingDepartment)
            {
                if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace)
                    && employeeComponent.ShouldStartCommuting() && (!__instance.GetDepartment().IsClosed()))
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - going to workplace");

                    __instance.GoToWorkPlace();
                    return false;

                }

                if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Vanilla.JanitorAdminWorkplace)
                    && employeeComponent.ShouldStartCommuting() && (!__instance.GetDepartment().IsClosed()))
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - going to workplace");

                    __instance.GoToWorkPlace();
                    return false;
                }

                if ((homeRoomType != null)
                    && employeeComponent.ShouldStartCommuting() && (!__instance.GetDepartment().IsClosed()))
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - going to workplace");

                    __instance.GoToWorkPlace();
                    return false;
                }
            }
            else
            {
                // replace in-game method

                if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Vanilla.JanitorAdminWorkplace)
                    && employeeComponent.ShouldStartCommuting() && (!__instance.GetDepartment().IsClosed()))
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - going to workplace");

                    __instance.GoToWorkPlace();
                    return false;
                }

                if ((homeRoomType != null)
                    && employeeComponent.ShouldStartCommuting() && (!__instance.GetDepartment().IsClosed()))
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - going to workplace");

                    __instance.GoToWorkPlace();
                    return false;
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateGoingToWorkplace")]
        public static bool UpdateStateGoingToWorkplacePrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            if (homeRoomType != null && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
            {
                if (!__instance.GetComponent<WalkComponent>().IsBusy())
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - arrived to training workplace");

                    TileObject entity = employeeComponent.m_state.m_workDesk.GetEntity();
                    if (entity != null)
                    {
                        entity.SetLightEnabled(true);
                        entity.GetComponent<AnimatedObjectComponent>().ForceFrame(1);
                    }

                    employeeComponent.CheckChiefNodiagnoseDepartment(true);
                    employeeComponent.CheckRoomSatisfactionBonuses();
                    employeeComponent.CheckMoodModifiers(__instance.IsBookmarked());
                    employeeComponent.CheckBossModifiers();
                    employeeComponent.UpdateHomeRoom();

                    if (!employeeComponent.ShouldGoToTraining())
                    {
                        if (employeeComponent.m_state.m_skillSet.m_qualifications != null)
                        {
                            foreach (var skill in employeeComponent.m_state.m_skillSet.m_qualifications)
                            {
                                employeeComponent.ToggleTraining(skill);
                            }
                        }
                        if (employeeComponent.m_state.m_skillSet.m_specialization1 != null)
                        {
                            employeeComponent.ToggleTraining(employeeComponent.m_state.m_skillSet.m_specialization1);
                        }
                        if (employeeComponent.m_state.m_skillSet.m_specialization2 != null)
                        {
                            employeeComponent.ToggleTraining(employeeComponent.m_state.m_skillSet.m_specialization2);
                        }
                    }

                    __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - filling free time");

                    return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateFulfillingNeeds")]
        public static bool UpdateStateFulfillingNeedsPrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            if (homeRoomType != null && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
            {
                if (!__instance.GetComponent<ProcedureComponent>().IsBusy())
                {
                    if (BehaviorJanitorPatch.GoHomeMod(__instance))
                    {
                    }
                    else if (BehaviorJanitorPatch.CheckNeedsMod(__instance))
                    {
                        __instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                        __instance.SwitchState(BehaviorJanitorState.FulfillingNeeds);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - fulfilling needs");
                    }
                    else if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(__instance.GetComponent<ProcedureComponent>()))
                    {
                        __instance.SwitchState(BehaviorJanitorState.Training);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - training");
                    }
                    else
                    {
                        __instance.GoToWorkPlace();

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - going to training workplace");
                    }
                }

                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateFillingFreeTime")]
        public static bool UpdateStateFillingFreeTimePrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            if (homeRoomType != null && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
            {
                if (!__instance.GetComponent<ProcedureComponent>().IsBusy())
                {
                    if (BehaviorJanitorPatch.GoHomeMod(__instance))
                    {
                    }
                    else if (BehaviorJanitorPatch.CheckNeedsMod(__instance))
                    {
                        __instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                        __instance.SwitchState(BehaviorJanitorState.FulfillingNeeds);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - fulfilling needs");
                    }
                    else if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(__instance.GetComponent<ProcedureComponent>()))
                    {
                        __instance.SwitchState(BehaviorJanitorState.Training);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - training");
                    }
                }

                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), nameof(BehaviorJanitor.UpdateTraining))]
        public static bool UpdateTrainingPrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            if (homeRoomType != null && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
            {
                if (employeeComponent.UpdateTraining(__instance.GetComponent<ProcedureComponent>()))
                {
                    __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - filling free time");
                }

                return false;
            }

            return true;
        }

        public static bool CheckNeedsMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("CheckNeeds", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static bool GoHomeMod(BehaviorJanitor instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            if (employeeComponent.IsFired() || instance.GetDepartment().IsClosed() || (DayTime.Instance.GetShift() != employeeComponent.m_state.m_shift))
            {
                instance.GetComponent<WalkComponent>().SetDestination(MapScriptInterface.Instance.GetRandomSpawnPosition(), 0, MovementType.WALKING);
                instance.m_state.m_finished = true;
                instance.SwitchState(BehaviorJanitorState.GoingHome);

                TileObject entity = employeeComponent.m_state.m_workDesk.GetEntity();
                if (entity != null)
                {
                    entity.SetLightEnabled(false);
                    entity.GetComponent<AnimatedObjectComponent>().ForceFrame(0);
                }

                instance.m_state.m_hadLunch = false;

                if (employeeComponent.IsFired())
                {
                    employeeComponent.ResetWorkspace(false);
                }
                employeeComponent.ResetNoWorkSpaceFlags();

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name} - going home");

                return true;
            }

            return false;
        }
    }
}
