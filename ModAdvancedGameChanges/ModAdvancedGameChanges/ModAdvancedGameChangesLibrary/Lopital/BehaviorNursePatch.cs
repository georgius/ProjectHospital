using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using ModAdvancedGameChanges.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(BehaviorNurse))]
    public static class BehaviorNursePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.AddToHospital))]
        public static bool AddToHospitalPrefix(BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, added to hospital, trying to find common room");

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

            employeeComponent.m_state.m_department = MapScriptInterface.Instance.GetActiveDepartment();
            employeeComponent.m_state.m_startDay = DayTime.Instance.GetDay();

            Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypePreferDepartment(__instance, RoomTypes.Vanilla.CommonRoom, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF);

            if (position == Vector3i.ZERO_VECTOR)
            {
                // not found common room, stay at home
                BehaviorNursePatch.GoHome(__instance);
                return false;
            }

            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);
            __instance.SwitchState(NurseState.Commuting);

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to common room");

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "CheckNeeds")]
        public static bool CheckNeedsPrefix(AccessRights accessRights, bool surgeryScheduled, BehaviorNurse __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, this method should not be called!");

            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.GetReserved))]
        public static bool GetReservedPrefix(BehaviorNurse __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            __result = false;
            __result |= __instance.CurrentPatient != null;
            __result |= __instance.GetComponent<EmployeeComponent>().m_state.m_reservedByPatient != null;
            __result |= !String.IsNullOrEmpty(__instance.GetComponent<EmployeeComponent>().m_state.m_reservedForProcedureLocID);
            __result |= (__instance.m_state.m_nurseState == NurseState.OverridenReservedForProcedure);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.GoToWorkplace))]
        public static bool GoToWorkplacePrefix(BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            WalkComponent walkComponent = __instance.GetComponent<WalkComponent>(); ;
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            __instance.CancelBrowsing();

            if (employeeComponent.GetWorkChair() != null)
            {
                TileObject workChair = employeeComponent.GetWorkChair();
                if (!walkComponent.IsSittingOn(workChair) && !walkComponent.IsWalkingTo(workChair))
                {
                    walkComponent.GoSit(workChair, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to chair");

                    __instance.SwitchState(NurseState.GoingToWorkplace);
                }
            }
            else if (__instance.GetWorkDesk() != null)
            {
                Vector2f defaultUsePosition = __instance.GetWorkDesk().GetDefaultUsePosition();
                if (defaultUsePosition.subtract(walkComponent.m_state.m_currentPosition).length() > 0.25f)
                {
                    walkComponent.SetDestination(defaultUsePosition, __instance.GetWorkDesk().GetFloorIndex(), MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to work desk");

                    __instance.SwitchState(NurseState.GoingToWorkplace);
                }
            }
            else if ((employeeComponent.m_state.m_workPlacePosition != Vector2i.ZERO_VECTOR) && (employeeComponent.m_state.m_workPlacePosition != walkComponent.GetCurrentTile()))
            {
                walkComponent.SetDestination(employeeComponent.m_state.m_workPlacePosition, employeeComponent.m_state.m_workPlaceFloorIndex, MovementType.WALKING);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to work place position");

                __instance.SwitchState(NurseState.GoingToWorkplace);
            }
            else
            {
                // by default, go to common room
                Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypePreferDepartment(__instance, RoomTypes.Vanilla.CommonRoom, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF);

                if (position != Vector3i.ZERO_VECTOR)
                {
                    __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no workplace, going to common room, filling free time");

                    __instance.SwitchState(NurseState.FillingFreeTime);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.IsFree))]
        public static bool IsFree(BehaviorNurse __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            // nurse is possibly free when:
            // - is idle

            ProcedureComponent procedureComponent = __instance.GetComponent<ProcedureComponent>();
            WalkComponent walkComponent = __instance.GetComponent<WalkComponent>();

            // not doing any procedure
            __result = !procedureComponent.IsBusy();

            // not walking
            __result &= !walkComponent.IsBusy();

            // check needs (no critical need)
            foreach (var need in __instance.GetComponent<MoodComponent>().m_state.m_needs)
            {
                __result &= (need.m_currentValue < Tweakable.Mod.FulfillNeedsThresholdCritical());
            }

            switch (__instance.m_state.m_nurseState)
            {
                case NurseState.Idle:
                    {
                        __result = true;

                        // check if nurse have no patient
                        __result &= (__instance.CurrentPatient == null);

                        // check if nurse is not reserved by patient
                        __result &= (__instance.GetComponent<EmployeeComponent>().m_state.m_reservedByPatient == null);

                        // check if nurse is not reserved by procedure
                        __result &= String.IsNullOrEmpty(__instance.GetComponent<EmployeeComponent>().m_state.m_reservedForProcedureLocID);

                        return false;
                    }
                default:
                    break;
            }

            // default case
            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.IsHidden))]
        public static bool IsHiddenPrefix(BehaviorNurse __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            __result = (__instance.m_state.m_nurseState == NurseState.AtHome) || (__instance.m_state.m_nurseState == NurseState.FiredAtHome);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.SwitchState))]
        public static bool SwitchStatePrefix(NurseState state, BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            if (__instance.m_state.m_nurseState != state)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, switching state from {__instance.m_state.m_nurseState} to {state}");

                __instance.m_state.m_nurseState = state;
                __instance.m_state.m_timeInState = 0f;

                if (__instance.m_state.m_isBrowsing && __instance.IsAtWorkplace(__instance.GetComponent<EmployeeComponent>()))
                {
                    __instance.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitRelaxPcOut, false, true);
                }
                __instance.CancelBrowsing();
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.ReceiveMessage))]
        public static bool ReceiveMessagePrefix(Message message, BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, message received: {message.m_messageID}");

            if (message.m_messageID == Messages.OVERRIDE_BY_PROCEDURE_SCRIPT)
            {
                __instance.SwitchState(NurseState.OverridenByProcedureScript);
                return false;
            }

            if (message.m_messageID == Messages.CANCEL_OVERRIDE_BY_PROCEDURE_SCRIPT)
            {
                __instance.SwitchState(NurseState.Idle);
                return false;
            }

            if (message.m_messageID == Messages.OVERRIDE_BY_GOING_TO_STRETCHER)
            {
                __instance.SwitchState(NurseState.OverridenGoingToStretcher);
                return false;
            }

            if (message.m_messageID == Messages.OVERRIDE_BY_MOVING_STRETCHER)
            {
                __instance.SwitchState(NurseState.OverridenMovingStretcher);
                return false;
            }
            
            if (message.m_messageID == Messages.CANCEL_OVERRIDE_BY_MOVING_STRETCHER)
            {
                __instance.SwitchState(NurseState.Idle);
                return false;
            }

            return BehaviorPatch.ReceiveMessage(message, __instance, false);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "UpdateStateAtHome")]
        public static bool UpdateStateAtHomePrefix(BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
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

            if (employeeComponent.ShouldStartCommuting() && (!__instance.GetDepartment().IsClosed()))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, trying to find common room");

                Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypePreferDepartment(__instance, RoomTypes.Vanilla.CommonRoom, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF);

                if (position == Vector3i.ZERO_VECTOR)
                {
                    // not found common room, stay at home
                    return false;
                }

                __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);
                __instance.SwitchState(NurseState.Commuting);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to common room");
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "UpdateStateCommuting")]
        public static bool UpdateStateCommutingPrefix(BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            // "Commuting" state is only for arriving nurse to hospital (directly to common room)

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, arrived to hospital");

                employeeComponent.CheckChiefNodiagnoseDepartment(false);
                employeeComponent.CheckRoomSatisfactionBonuses();
                employeeComponent.CheckMoodModifiers(__instance.IsBookmarked());
                employeeComponent.CheckBossModifiers();
                employeeComponent.UpdateHomeRoom();
                __instance.GetComponent<AnimModelComponent>().RevertToDefaultClothes(false);

                __instance.m_state.m_hadLunch = false;
                __instance.m_state.m_isBrowsing = false;
                __instance.m_state.m_currentPatient = null;
                __instance.m_state.m_wheelchair = false;

                if (!BehaviorNursePatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                {
                    // default case
                    __instance.SwitchState(NurseState.FillingFreeTime);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "UpdateStateFillingFreeTime")]
        public static bool UpdateStateFillingFreeTimePrefix(float deltaTime, BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            if ((!__instance.GetComponent<ProcedureComponent>().IsBusy()) && (!__instance.GetComponent<WalkComponent>().IsBusy()))
            {
                // currently nurse is not doing anything
                if (!BehaviorNursePatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                {
                    if ((homeRoomType == null)
                        || ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace))
                        || ((homeRoomType != null) && (!homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace)) && (!BehaviorNursePatch.HandleGoHomeFulfillNeedsTraining(__instance))))
                    {
                        // nurse still don't have anything to do
                        // just stay in common room and fill free time
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, nothing to do");

                        // the nurse don't have to be in common room !
                        Room room = MapScriptInterface.Instance.GetRoomAt(__instance.m_entity.GetComponent<WalkComponent>());

                        if ((room != null) && (room.m_roomPersistentData.m_roomType.Entry.HasTag(Tags.Vanilla.CommonRoom)))
                        {
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, in common room, resting");

                            // did not find anything to do, just rest
                            GameDBProcedure staffFreeTimeProcedure = Database.Instance.GetEntry<GameDBProcedure>(Procedures.Vanilla.StaffFreeTime);
                            if (__instance.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(
                                staffFreeTimeProcedure, __instance.m_entity,
                                room.m_roomPersistentData.m_department.GetEntity(),
                                AccessRights.STAFF, EquipmentListRules.ONLY_FREE_SAME_FLOOR_PREFER_DPT) == ProcedureSceneAvailability.AVAILABLE)
                            {
                                __instance.GetComponent<ProcedureComponent>().StartProcedure(
                                    staffFreeTimeProcedure,
                                    __instance.m_entity,
                                    room.m_roomPersistentData.m_department.GetEntity(),
                                    AccessRights.STAFF,
                                    EquipmentListRules.ONLY_FREE_SAME_FLOOR_PREFER_DPT);

                                return false;
                            }
                        }
                        else
                        {
                            // find and go to common room
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, not in common room");

                            Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypePreferDepartment(__instance, RoomTypes.Vanilla.CommonRoom, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF);

                            if (position != Vector3i.ZERO_VECTOR)
                            {
                                __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to common room");
                            }
                        }
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "UpdateStateFulfillingNeeds")]
        public static bool UpdateStateFulfillingNeedsPrefix(BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            // need to fulfill some needs
            // check if not fulfilling

            if ((__instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript != null)
                && (__instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().m_stateData.m_state == ProcedureScriptPatch.STATE_RESERVED))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, planned procedure {__instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().m_stateData.m_scriptName}, activating");

                __instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().Activate();

                return false;
            }

            if (!__instance.GetComponent<ProcedureComponent>().IsBusy())
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, fulfilling need finished or not started yet");

                EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
                GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

                if (!__instance.GetComponent<ProcedureComponent>().IsBusy())
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, fulfilling need finished");

                    if (!BehaviorNursePatch.HandleGoHomeFulfillNeeds(__instance))
                    {
                        if ((homeRoomType == null)
                            || ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace) && (!BehaviorNursePatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance)))
                            || ((homeRoomType != null) && (!homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace)) && (!BehaviorNursePatch.HandleGoHomeFulfillNeedsTraining(__instance)) && (!BehaviorNursePatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))))
                        {
                            // by default, go to common room
                            Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypePreferDepartment(__instance, RoomTypes.Vanilla.CommonRoom, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF);

                            if (position != Vector3i.ZERO_VECTOR)
                            {
                                __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to common room");

                                __instance.SwitchState(NurseState.FillingFreeTime);
                            }
                            else
                            {
                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, common room not found");
                            }
                        }
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "UpdateStateGoingToWorkplace")]
        public static bool UpdateStateGoingToWorkplacePrefix(BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
                GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, arrived to workplace");

                employeeComponent.CheckChiefNodiagnoseDepartment(false);
                employeeComponent.CheckRoomSatisfactionBonuses();
                employeeComponent.CheckMoodModifiers(__instance.IsBookmarked());
                employeeComponent.CheckBossModifiers();
                employeeComponent.UpdateHomeRoom();
                __instance.CancelBrowsing();

                if (!__instance.GetComponent<WalkComponent>().IsOnBiohazard())
                {
                    __instance.GetComponent<AnimModelComponent>().RevertToDefaultClothes(false);
                }
                __instance.SwitchState(NurseState.Idle);

                if (!BehaviorNursePatch.HandleGoHomeFulfillNeedsTraining(__instance))
                {
                    if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace))
                    {
                        TileObject entity = employeeComponent.m_state.m_workDesk.GetEntity();
                        if (entity != null)
                        {
                            entity.SetLightEnabled(true);
                            entity.GetComponent<AnimatedObjectComponent>().ForceFrame(1);
                        }

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, nothing to do, filling free time");
                        __instance.SwitchState(NurseState.FillingFreeTime);
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "UpdateStateIdle")]
        public static bool UpdateStateIdlePrefix(float deltaTime, BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            if (deltaTime <= 0f)
            {
                return false;
            }

            if (!BehaviorNursePatch.HandleGoHomeFulfillNeedsTraining(__instance))
            {
                // nurse has nothing to do
                EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

                if ((__instance.m_state.m_timeInState > DayTime.Instance.IngameTimeHoursToRealTimeSeconds(5f / 60f)) && (!__instance.m_state.m_isBrowsing) && (employeeComponent.GetWorkChair() != null) && __instance.IsAtWorkplace(employeeComponent))
                {
                    __instance.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitRelaxPcIn, false, true);
                    __instance.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitRelaxPcIdle, true, false);
                    employeeComponent.m_state.m_workDesk.GetEntity().GetComponent<AnimatedObjectComponent>().ForceFrame(UnityEngine.Random.Range(2, 5));
                    __instance.m_state.m_isBrowsing = true;
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.UpdateTraining))]
        public static bool UpdateTrainingPrefix(BehaviorNurse __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            employeeComponent.CheckRoomSatisfactionBonuses();
            employeeComponent.CheckMoodModifiers(__instance.IsBookmarked());

            if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace))
            {
                if (employeeComponent.UpdateTraining(__instance.GetComponent<ProcedureComponent>()))
                {
                    // training was finished

                    if (!BehaviorNursePatch.HandleGoHomeFulfillNeedsTraining(__instance))
                    {
                        // default case, training is not possible more
                        __instance.SwitchState(NurseState.FillingFreeTime);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, filling free time");

                        Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypePreferDepartment(__instance, RoomTypes.Vanilla.CommonRoom, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF);

                        if (position != Vector3i.ZERO_VECTOR)
                        {
                            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to common room");
                        }

                        return false;
                    }
                }

                return false;
            }
            else
            {
                // regular nurse
                if (employeeComponent.UpdateTraining(__instance.GetComponent<ProcedureComponent>()))
                {
                    // training was finished

                    if (!BehaviorNursePatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                    {
                        // default case, training is not possible more
                        __instance.SwitchState(NurseState.FillingFreeTime);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, filling free time");

                        Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypePreferDepartment(__instance, RoomTypes.Vanilla.CommonRoom, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF);

                        if (position != Vector3i.ZERO_VECTOR)
                        {
                            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to common room");
                        }

                        return false;
                    }
                }
            }

            return false;
        }

        private static bool GoHome(BehaviorNurse instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            if (employeeComponent.IsFired() || instance.GetDepartment().IsClosed()
                || ((DayTime.Instance.GetShift() != employeeComponent.m_state.m_shift) && (Mathf.Abs(DayTime.Instance.GetDayTimeHours() - shift.StartTime) > 1)))
            {
                instance.GetComponent<WalkComponent>().SetDestination(MapScriptInterface.Instance.GetRandomSpawnPosition(), 0, MovementType.WALKING);
                instance.SwitchState(NurseState.GoingHome);

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

                return true;
            }

            return false;
        }

        private static bool IsNeededHandleGoHome(BehaviorNurse instance)
        {
            if (instance.GetReserved())
            {
                return false;
            }

            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            return (employeeComponent.IsFired() || instance.GetDepartment().IsClosed()
                || ((DayTime.Instance.GetShift() != employeeComponent.m_state.m_shift) && (Mathf.Abs(DayTime.Instance.GetDayTimeHours() - shift.StartTime) > 1)));
        }

        private static bool IsNeededHandleFullfillNeeds(BehaviorNurse instance)
        {
            if (instance.GetReserved())
            {
                return false;
            }

            if (instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript != null)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, planned procedure {instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().m_stateData.m_scriptName}");
                return true;
            }

            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();
            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            Need hunger = instance.GetComponent<MoodComponent>().GetNeed(Needs.Vanilla.HungerStaff);
            Need rest = instance.GetComponent<MoodComponent>().GetNeed(Needs.Vanilla.Rest);

            List<Need> needsSortedFromMostCritical = instance.GetComponent<MoodComponent>().GetNeedsSortedFromMostCritical();

            foreach (Need need in needsSortedFromMostCritical)
            {
                if ((need.m_currentValue > UnityEngine.Random.Range(Tweakable.Mod.FulfillNeedsThreshold(), Needs.NeedMaximum)) || (need.m_currentValue > Tweakable.Mod.FulfillNeedsThresholdCritical()))
                {
                    if ((need == rest)
                        && (need.m_currentValue <= Tweakable.Mod.FulfillNeedsThresholdCritical())
                        && instance.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.HardWorker))
                    {
                        // skip rest, hard worker does not take free time breaks
                        continue;
                    }
                    else if (need == hunger)
                    {
                        // check if nurse should go to lunch
                        if ((!instance.m_state.m_hadLunch)
                            && ((DayTime.Instance.IsScheduledActionTime(Schedules.Vanilla.SCHEDULE_STAFF_LUNCH, true) && (DayTime.Instance.GetShift() == Shift.DAY) && (employeeComponent.m_state.m_shift == Shift.DAY) && (DayTime.Instance.GetDayTimeHours() < shift.EndTime)) ||
                            (DayTime.Instance.IsScheduledActionTime(Schedules.Mod.SCHEDULE_STAFF_LUNCH_NIGHT, true) && (DayTime.Instance.GetShift() == Shift.NIGHT) && (employeeComponent.m_state.m_shift == Shift.NIGHT) && (DayTime.Instance.GetDayTimeHours() < shift.EndTime) && ViewSettingsPatch.m_staffLunchNight[SettingsManager.Instance.m_viewSettings].m_value)))
                        {
                            GameDBProcedure staffLunchProcedure = Database.Instance.GetEntry<GameDBProcedure>(Procedures.Vanilla.StaffLunch);

                            if (instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript == null)
                            {
                                if (instance.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(staffLunchProcedure, instance.m_entity, instance.GetDepartment(), AccessRights.STAFF_ONLY, EquipmentListRules.ONLY_FREE_SAME_FLOOR_PREFER_DPT) == ProcedureSceneAvailability.AVAILABLE)
                                {
                                    instance.GetComponent<ProcedureComponent>().StartProcedure(staffLunchProcedure, instance.m_entity, instance.GetDepartment(), AccessRights.STAFF_ONLY, EquipmentListRules.ONLY_FREE_SAME_FLOOR_PREFER_DPT);
                                    instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().SwitchState(ProcedureScriptPatch.STATE_RESERVED);

                                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, planning lunch");
                                    return true;
                                }
                            }
                        }
                    }

                    if (instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript == null)
                    {
                        if (instance.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(need.m_gameDBNeed.Entry.Procedure, instance.m_entity, instance.GetDepartment(), AccessRights.STAFF_ONLY, EquipmentListRules.ONLY_FREE_SAME_FLOOR_PREFER_DPT) == ProcedureSceneAvailability.AVAILABLE)
                        {
                            instance.GetComponent<ProcedureComponent>().StartProcedure(need.m_gameDBNeed.Entry.Procedure, instance.m_entity, instance.GetDepartment(), AccessRights.STAFF_ONLY, EquipmentListRules.ONLY_FREE_SAME_FLOOR_PREFER_DPT);
                            instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().SwitchState(ProcedureScriptPatch.STATE_RESERVED);

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, planning fulfilling need {need.m_gameDBNeed.Entry.DatabaseID}, need value {need.m_currentValue.ToString(CultureInfo.InvariantCulture)}");
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsNeededHandleTraining(BehaviorNurse instance)
        {
            if (instance.GetReserved())
            {
                return false;
            }

            return instance.GetComponent<EmployeeComponent>().ShouldGoToTraining();
        }

        public static bool HandleGoHomeFulfillNeeds(BehaviorNurse instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            if (instance.GetReserved())
            {
                return true;
            }

            // check if nurse needs to go home
            if (BehaviorNursePatch.GoHome(instance))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, going home");
                return true;
            }

            // check if nurse needs to fulfill his/her needs
            if (BehaviorNursePatch.IsNeededHandleFullfillNeeds(instance))
            {
                instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                instance.SwitchState(NurseState.FulfillingNeeds);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, fulfilling needs");
                return true;
            }

            return false;
        }

        public static bool HandleGoHomeFulfillNeedsGoToWorkplace(BehaviorNurse instance)
        {
            if (!BehaviorNursePatch.HandleGoHomeFulfillNeeds(instance))
            {
                EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

                GameDBRoomType homeRoomType = employeeComponent.GetHomeRoomType();
                WalkComponent walkComponent = instance.GetComponent<WalkComponent>();
                Entity oppositeShiftEmployee = employeeComponent.GetOppositeShiftEmployee();

                bool canGoToWorkplace = (oppositeShiftEmployee == null) ||
                    ((oppositeShiftEmployee != null) && ((oppositeShiftEmployee.GetComponent<BehaviorNurse>().m_state.m_nurseState == NurseState.AtHome)
                        || (oppositeShiftEmployee.GetComponent<BehaviorNurse>().m_state.m_nurseState == NurseState.GoingHome)
                        || (oppositeShiftEmployee.GetComponent<BehaviorNurse>().m_state.m_nurseState == NurseState.FiredAtHome)));

                canGoToWorkplace &= ((employeeComponent.GetWorkChair() != null)
                    || (instance.GetWorkDesk() != null)
                    || ((employeeComponent.m_state.m_workPlacePosition != Vector2i.ZERO_VECTOR) && (employeeComponent.m_state.m_workPlacePosition != walkComponent.GetCurrentTile())));

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, opposite shift employee: {oppositeShiftEmployee?.Name ?? "NULL"}, state: {oppositeShiftEmployee?.GetComponent<BehaviorNurse>().m_state.m_nurseState.ToString() ?? "NULL"}");

                if (canGoToWorkplace)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, can go to workplace");

                    if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, going to nurse training workplace");

                        instance.GoToWorkplace();
                        return true;
                    }

                    // regular nurse
                    instance.GoToWorkplace();
                    return true;
                }
                else
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, cannot go to workplace");
                }

                return false;
            }

            return true;
        }

        public static bool HandleGoHomeFulfillNeedsTraining(BehaviorNurse instance)
        {
            if (!BehaviorNursePatch.HandleGoHomeFulfillNeeds(instance))
            {
                EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();
                GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

                if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace))
                {
                    // it is nurse in training
                    // set him/her training flags

                    if (!employeeComponent.ShouldGoToTraining())
                    {
                        BehaviorPatch.ChooseSkillToTrainAndToggleTraining(instance);
                    }

                    // if possible, go to training
                    if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(instance.GetComponent<ProcedureComponent>()))
                    {
                        instance.SwitchState(NurseState.Training);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, training");
                        return true;
                    }
                }

                if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(instance.GetComponent<ProcedureComponent>()))
                {
                    instance.SwitchState(NurseState.Training);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, training");
                    return true;
                }

                return false;
            }

            return true;
        }

        
    }

    public static class BehaviorNurseExtensions
    {
        public static TileObject GetWorkDesk(this BehaviorNurse instance)
        {
            return MethodAccessHelper.CallMethod<TileObject>(instance, "GetWorkDesk");
        }
    }
}
