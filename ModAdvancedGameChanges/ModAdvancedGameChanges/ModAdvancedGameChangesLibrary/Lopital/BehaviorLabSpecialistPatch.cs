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
    [HarmonyPatch(typeof(BehaviorLabSpecialist))]
    public static class BehaviorLabSpecialistPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.AddToHospital))]
        public static bool AddToHospitalPrefix(BehaviorLabSpecialist __instance)
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
                BehaviorLabSpecialistPatch.GoHome(__instance);
                return false;
            }

            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);
            __instance.SwitchState(LabSpecialistState.Commuting);

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to common room");

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "CheckNeeds")]
        public static bool CheckNeedsPrefix(AccessRights accessRights, BehaviorLabSpecialist __instance, ref bool __result)
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
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.GetReserved))]
        public static bool GetReservedPrefix(BehaviorLabSpecialist __instance, ref bool __result)
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
            __result |= (__instance.m_state.m_labSpecialistState == LabSpecialistState.OverridenReservedForProcedure);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.GoToWorkplace))]
        public static bool GoToWorkplacePrefix(BehaviorLabSpecialist __instance)
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

                    __instance.SwitchState(LabSpecialistState.GoingToWorkplace);
                }
            }
            else if (__instance.GetWorkDesk() != null)
            {
                Vector2f defaultUsePosition = __instance.GetWorkDesk().GetDefaultUsePosition();
                if (defaultUsePosition.subtract(walkComponent.m_state.m_currentPosition).length() > 0.25f)
                {
                    walkComponent.SetDestination(defaultUsePosition, __instance.GetWorkDesk().GetFloorIndex(), MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to work desk");

                    __instance.SwitchState(LabSpecialistState.GoingToWorkplace);
                }
            }
            else if ((employeeComponent.m_state.m_workPlacePosition != Vector2i.ZERO_VECTOR) && (employeeComponent.m_state.m_workPlacePosition != walkComponent.GetCurrentTile()))
            {
                walkComponent.SetDestination(employeeComponent.m_state.m_workPlacePosition, employeeComponent.m_state.m_workPlaceFloorIndex, MovementType.WALKING);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to work place position");

                __instance.SwitchState(LabSpecialistState.GoingToWorkplace);
            }
            else
            {
                // by default, go to common room
                Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypePreferDepartment(__instance, RoomTypes.Vanilla.CommonRoom, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF);

                if (position != Vector3i.ZERO_VECTOR)
                {
                    __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no workplace, going to common room, filling free time");

                    __instance.SwitchState(LabSpecialistState.FillingFreeTime);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.IsFree))]
        [HarmonyPatch(new Type[] { })]
        public static bool IsFreePrefix1(BehaviorLabSpecialist __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

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

            switch (__instance.m_state.m_labSpecialistState)
            {
                case LabSpecialistState.Idle:
                    {
                        // check if lab specialist is sitting
                        __result &= walkComponent.IsSitting();

                        // check if lab specialist have no patient
                        __result &= (__instance.CurrentPatient == null);

                        // check if lab specialist is not reserved by patient
                        __result &= (__instance.GetComponent<EmployeeComponent>().m_state.m_reservedByPatient == null);

                        // check if lab specialist is not reserved by procedure
                        __result &= String.IsNullOrEmpty(__instance.GetComponent<EmployeeComponent>().m_state.m_reservedForProcedureLocID);

                        return false;
                    }
                default:
                    // default case
                    __result = false;
                    break;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.IsFree))]
        [HarmonyPatch(new Type[] { typeof(Entity) })]
        public static bool IsFreePrefix2(Entity patient, BehaviorLabSpecialist __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            __result = false;

            // check if lab specialist have patient as current patient
            __result |= (__instance.CurrentPatient == patient);

            // check if lab specialist is reserved by patient
            __result |= (__instance.GetComponent<EmployeeComponent>().m_state.m_reservedByPatient == patient);

            if (__result)
            {
                return false;
            }

            return BehaviorLabSpecialistPatch.IsFreePrefix1(__instance, ref __result);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.IsHidden))]
        public static bool IsHiddenPrefix(BehaviorLabSpecialist __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            __result = (__instance.m_state.m_labSpecialistState == LabSpecialistState.AtHome) || (__instance.m_state.m_labSpecialistState == LabSpecialistState.FiredAtHome);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.SwitchState))]
        public static bool SwitchStatePrefix(LabSpecialistState state, BehaviorLabSpecialist __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            if (__instance.m_state.m_labSpecialistState != state)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, switching state from {__instance.m_state.m_labSpecialistState} to {state}");

                __instance.m_state.m_labSpecialistState = state;
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
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.ReceiveMessage))]
        public static bool ReceiveMessagePrefix(Message message, BehaviorLabSpecialist __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, message received: {message.m_messageID}");

            if (message.m_messageID == Messages.OVERRIDE_BY_PROCEDURE_SCRIPT)
            {
                __instance.SwitchState(LabSpecialistState.OverridenByProcedureScript);
                return false;
            }

            if (message.m_messageID == Messages.CANCEL_OVERRIDE_BY_PROCEDURE_SCRIPT)
            {
                __instance.SwitchState(LabSpecialistState.Idle);
                return false;
            }

            return BehaviorPatch.ReceiveMessage(message, __instance, false);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.SetReserved))]
        public static bool SetReservedPrefix(bool reserved, string procedureLocID, Entity patient, BehaviorLabSpecialist __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, procedure {procedureLocID ?? "NULL"}, patient {patient?.Name ?? "NULL"}, reserved {reserved} ");

            __instance.SwitchState(reserved ? LabSpecialistState.OverridenReservedForProcedure : LabSpecialistState.Idle);
            __instance.GetComponent<EmployeeComponent>().SetReserved(procedureLocID, patient);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.Update))]
        public static bool UpdatePrefix(float deltaTime, BehaviorLabSpecialist __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            __instance.m_state.m_timeInState += deltaTime;

            if ((__instance.m_state.m_currentPatient != null) 
                && (__instance.m_state.m_currentPatient.GetEntity() == null))
            {
                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, cleaned up deleted patient {__instance.m_state.m_currentPatient.m_entityID}");
                __instance.m_state.m_currentPatient = null;
            }

            if ((__instance.m_state.m_currentPatient != null) 
                && (__instance.m_state.m_currentPatient.GetEntity() != null))
            {
                BehaviorPatient patient = __instance.m_state.m_currentPatient.GetEntity().GetComponent<BehaviorPatient>();

                if ((patient != null)
                    && patient.m_state.m_patientState == PatientState.Left)
                {
                    Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, cleaned up deleted patient {__instance.m_state.m_currentPatient.m_entityID}");
                    __instance.m_state.m_currentPatient = null;
                }
            }

            bool activeCharacter = true;

            switch (__instance.m_state.m_labSpecialistState)
            {
                case LabSpecialistState.Idle:
                    __instance.UpdateStateIdle(deltaTime);
                    break;
                case LabSpecialistState.GoingToWorkplace:
                    __instance.UpdateStateGoingToWorkplace();
                    break;
                case LabSpecialistState.FulfillingNeeds:
                    __instance.UpdateStateFulfillingNeeds();
                    break;
                case LabSpecialistState.FillingFreeTime:
                    __instance.UpdateStateFillingFreeTime(deltaTime);
                    break;
                case LabSpecialistState.OverridenByProcedureScript:
                    __instance.UpdeteSafetyReleaseCheck();
                    break;
                case LabSpecialistState.GoingHome:
                    __instance.UpdateStateGoingHome();
                    break;
                case LabSpecialistState.AtHome:
                    {
                        __instance.UpdateStateAtHome();
                        activeCharacter = false;
                    }
                    break;
                case LabSpecialistState.Commuting:
                    __instance.UpdateStateCommuting();
                    break;
                case LabSpecialistState.FiredAtHome:
                    activeCharacter = false;
                    break;
                case LabSpecialistState.OverridenReservedForProcedure:
                    {
                        if ((__instance.GetComponent<EmployeeComponent>().m_state.m_reservedByPatient != null) 
                            && (__instance.GetComponent<EmployeeComponent>().m_state.m_reservedByPatient.GetEntity() == null))
                        {
                            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, reserved by a patient who left, cleared!");
                            __instance.SwitchState(LabSpecialistState.Idle);
                        }
                    }
                    break;
                case LabSpecialistState.GoingForSample:
                    __instance.UpdateStateGoingForSample();
                    break;
                case LabSpecialistState.GoingForSampleFromHospitalizedPatient:
                    __instance.UpdateStateGoingForSampleFromHospitalizedPatient();
                    break;
                case LabSpecialistState.TakingSample:
                    __instance.UpdateStateTakingSample();
                    break;
                case LabSpecialistState.ProcessingSampleFromPatient:
                    __instance.UpdateStateProcessingSampleFromPatient();
                    break;
                case LabSpecialistState.GoingToStoreSampleFromPatient:
                    __instance.UpdateStateGoingToStoreSampleFromPatient();
                    break;
                case LabSpecialistState.StoringSampleFromPatient:
                    __instance.UpdateStateStoringSampleFromPatient();
                    break;
                case LabSpecialistState.SampleFromPatientStored:
                    __instance.UpdateStateSampleFromPatientStored();
                    break;
                case LabSpecialistState.ReturningWithSample:
                    __instance.UpdateStateReturningWithSample();
                    break;
                case LabSpecialistState.StartWritingNotes:
                    __instance.UpdateStateWritingNotes();
                    break;
                case LabSpecialistState.StopWritingNotes:
                    __instance.UpdateStopWritingNotes();
                    break;
                case LabSpecialistState.GoingToEquipment:
                    __instance.UpdateStateGoingToEquipment();
                    break;
                case LabSpecialistState.UsingEquipment:
                    __instance.UpdateStateUsingEquipment();
                    break;
                case LabSpecialistState.StoppedUsingEquipment:
                    __instance.UpdateStateStoppedUsingEquipment();
                    break;
                case LabSpecialistState.FinishedProcedure:
                    __instance.UpdateFinishedProcedure();
                    break;
                case LabSpecialistState.Training:
                    __instance.UpdateTraining();
                    break;
                default:
                    break;
            }

            if (activeCharacter)
            {
                Hospital.Instance.m_currentHospitalStatus.m_hadAnyActiveCharactersThisFrame = true;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateAtHome")]
        public static bool UpdateStateAtHomePrefix(BehaviorLabSpecialist __instance)
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
                __instance.SwitchState(LabSpecialistState.Commuting);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to common room");
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateCommuting")]
        public static bool UpdateStateCommutingPrefix(BehaviorLabSpecialist __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            // "Commuting" state is only for arriving lab specialist to hospital (directly to common room)

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
                __instance.m_state.m_idleTime = 0f;
                __instance.m_state.m_isBrowsing = false;
                __instance.m_state.m_equipmentUseTime = 0f;

                if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                {
                    // default case
                    __instance.SwitchState(LabSpecialistState.FillingFreeTime);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateFillingFreeTime")]
        public static bool UpdateStateFillingFreeTimePrefix(float deltaTime, BehaviorLabSpecialist __instance)
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
                // currently lab specialist is not doing anything
                if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                {
                    if ((homeRoomType == null)
                        || ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace))
                        || ((homeRoomType != null) && (!homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace)) && (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsTraining(__instance))))
                    {
                        // lab specialist still don't have anything to do
                        // just stay in common room and fill free time
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, nothing to do");

                        // the lab specialist don't have to be in common room !
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

                                __instance.m_state.m_idleTime = 0f;

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
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateFulfillingNeeds")]
        public static bool UpdateStateFulfillingNeedsPrefix(BehaviorLabSpecialist __instance)
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

                    if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeeds(__instance))
                    {
                        if ((homeRoomType == null)
                            || ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace) && (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance)))
                            || ((homeRoomType != null) && (!homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace)) && (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsTraining(__instance))))
                        {
                            // by default, go to common room
                            Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypePreferDepartment(__instance, RoomTypes.Vanilla.CommonRoom, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF);

                            if (position != Vector3i.ZERO_VECTOR)
                            {
                                __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to common room");

                                __instance.SwitchState(LabSpecialistState.FillingFreeTime);
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
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateGoingToWorkplace")]
        public static bool UpdateStateGoingToWorkplacePrefix(BehaviorLabSpecialist __instance)
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

                employeeComponent.CheckRoomSatisfactionBonuses();
                employeeComponent.CheckMoodModifiers(__instance.IsBookmarked());
                __instance.CancelBrowsing();

                if (!__instance.GetComponent<WalkComponent>().IsOnBiohazard())
                {
                    __instance.GetComponent<AnimModelComponent>().RevertToDefaultClothes(false);
                }
                __instance.SwitchState(LabSpecialistState.Idle);

                if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsTraining(__instance))
                {
                    if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace))
                    {
                        TileObject entity = employeeComponent.m_state.m_workDesk.GetEntity();
                        if (entity != null)
                        {
                            entity.SetLightEnabled(true);
                            entity.GetComponent<AnimatedObjectComponent>().ForceFrame(1);
                        }

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, nothing to do, filling free time");
                        __instance.SwitchState(LabSpecialistState.FillingFreeTime);
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateIdle")]
        public static bool UpdateStateIdlePrefix(float deltaTime, BehaviorLabSpecialist __instance)
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
            __instance.m_state.m_idleTime += deltaTime;

            if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsTraining(__instance))
            {
                // lab specialist has nothing to do
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
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.UpdateTraining))]
        public static bool UpdateTrainingPrefix(BehaviorLabSpecialist __instance)
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

            if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace))
            {
                if (employeeComponent.UpdateTraining(__instance.GetComponent<ProcedureComponent>()))
                {
                    // training was finished

                    if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsTraining(__instance))
                    {
                        // default case, training is not possible more
                        __instance.SwitchState(LabSpecialistState.FillingFreeTime);

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
                // regular lab specialist
                if (employeeComponent.UpdateTraining(__instance.GetComponent<ProcedureComponent>()))
                {
                    // training was finished

                    if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                    {
                        // default case, training is not possible more
                        __instance.SwitchState(LabSpecialistState.FillingFreeTime);

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

        private static bool GoHome(BehaviorLabSpecialist instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            if (employeeComponent.IsFired() || instance.GetDepartment().IsClosed()
                || ((DayTime.Instance.GetShift() != employeeComponent.m_state.m_shift) && (Mathf.Abs(DayTime.Instance.GetDayTimeHours() - shift.StartTime) > 1)))
            {
                instance.GetComponent<WalkComponent>().SetDestination(MapScriptInterface.Instance.GetRandomSpawnPosition(), 0, MovementType.WALKING);
                instance.SwitchState(LabSpecialistState.GoingHome);

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

        private static bool IsNeededHandleGoHome(BehaviorLabSpecialist instance)
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

        private static bool IsNeededHandleFullfillNeeds(BehaviorLabSpecialist instance)
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
                        // check if lab specialist should go to lunch
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

        private static bool IsNeededHandleTraining(BehaviorLabSpecialist instance)
        {
            if (instance.GetReserved())
            {
                return false;
            }

            return instance.GetComponent<EmployeeComponent>().ShouldGoToTraining();
        }

        public static bool HandleGoHomeFulfillNeeds(BehaviorLabSpecialist instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            if (instance.GetReserved())
            {
                return true;
            }

            // check if lab specialist needs to go home
            if (BehaviorLabSpecialistPatch.GoHome(instance))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, going home");
                return true;
            }

            // check if lab specialist needs to fulfill his/her needs
            if (BehaviorLabSpecialistPatch.IsNeededHandleFullfillNeeds(instance))
            {
                instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                instance.SwitchState(LabSpecialistState.FulfillingNeeds);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, fulfilling needs");
                return true;
            }

            return false;
        }

        public static bool HandleGoHomeFulfillNeedsGoToWorkplace(BehaviorLabSpecialist instance)
        {
            if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeeds(instance))
            {
                EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

                GameDBRoomType homeRoomType = employeeComponent.GetHomeRoomType();
                WalkComponent walkComponent = instance.GetComponent<WalkComponent>();
                Entity oppositeShiftEmployee = employeeComponent.GetOppositeShiftEmployee();

                bool canGoToWorkplace = (oppositeShiftEmployee == null) ||
                    ((oppositeShiftEmployee != null) && ((oppositeShiftEmployee.GetComponent<BehaviorLabSpecialist>().m_state.m_labSpecialistState == LabSpecialistState.AtHome)
                        || (oppositeShiftEmployee.GetComponent<BehaviorLabSpecialist>().m_state.m_labSpecialistState == LabSpecialistState.GoingHome)
                        || (oppositeShiftEmployee.GetComponent<BehaviorLabSpecialist>().m_state.m_labSpecialistState == LabSpecialistState.FiredAtHome)));

                canGoToWorkplace &= ((employeeComponent.GetWorkChair() != null)
                    || (instance.GetWorkDesk() != null)
                    || ((employeeComponent.m_state.m_workPlacePosition != Vector2i.ZERO_VECTOR) && (employeeComponent.m_state.m_workPlacePosition != walkComponent.GetCurrentTile())));

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, opposite shift employee: {oppositeShiftEmployee?.Name ?? "NULL"}, state: {oppositeShiftEmployee?.GetComponent<BehaviorLabSpecialist>().m_state.m_labSpecialistState.ToString() ?? "NULL"}");

                if (canGoToWorkplace)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, can go to workplace");

                    if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, going to lab specialist training workplace");

                        instance.GoToWorkplace();
                        return true;
                    }

                    // regular lab specialist
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

        public static bool HandleGoHomeFulfillNeedsTraining(BehaviorLabSpecialist instance)
        {
            if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeeds(instance))
            {
                EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();
                GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

                if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace))
                {
                    // it is lab specialist in training
                    // set him/her training flags

                    if (!employeeComponent.ShouldGoToTraining())
                    {
                        BehaviorPatch.ChooseSkillToTrainAndToggleTraining(instance);
                    }

                    // if possible, go to training
                    if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(instance.GetComponent<ProcedureComponent>()))
                    {
                        instance.SwitchState(LabSpecialistState.Training);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, training");
                        return true;
                    }
                }

                if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(instance.GetComponent<ProcedureComponent>()))
                {
                    instance.SwitchState(LabSpecialistState.Training);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, training");
                    return true;
                }

                return false;
            }

            return true;
        }


    }

    public static class BehaviorLabSpecialistExtensions
    {
        public static TileObject GetWorkDesk(this BehaviorLabSpecialist instance)
        {
            return MethodAccessHelper.CallMethod<TileObject>(instance, "GetWorkDesk");
        }

        public static void UpdeteSafetyReleaseCheck(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdeteSafetyReleaseCheck");
        }

        public static void UpdateStateAtHome(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateAtHome");
        }

        public static void UpdateStateCommuting(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateCommuting");
        }

        public static void UpdateFinishedProcedure(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateFinishedProcedure");
        }

        public static void UpdateStateFillingFreeTime(this BehaviorLabSpecialist instance, float deltaTime)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateFillingFreeTime", deltaTime);
        }

        public static void UpdateStateFulfillingNeeds(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateFulfillingNeeds");
        }

        public static void UpdateStateGoingForSample(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingForSample");
        }

        public static void UpdateStateGoingForSampleFromHospitalizedPatient(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingForSampleFromHospitalizedPatient");
        }

        public static void UpdateStateGoingHome(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingHome");
        }

        public static void UpdateStateGoingToEquipment(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingToEquipment");
        }

        public static void UpdateStateGoingToStoreSampleFromPatient(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingToStoreSampleFromPatient");
        }

        public static void UpdateStateGoingToWorkplace(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingToWorkplace");
        }

        public static void UpdateStateIdle(this BehaviorLabSpecialist instance, float deltaTime)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateIdle", deltaTime);
        }

        public static void UpdateTraining(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateTraining");
        }

        public static void UpdateStateProcessingSampleFromPatient(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateProcessingSampleFromPatient");
        }

        public static void UpdateStateReturningWithSample(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateReturningWithSample");
        }

        public static void UpdateStateSampleFromPatientStored(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateSampleFromPatientStored");
        }

        public static void UpdateStateStoppedUsingEquipment(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateStoppedUsingEquipment");
        }

        public static void UpdateStopWritingNotes(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStopWritingNotes");
        }

        public static void UpdateStateStoringSampleFromPatient(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateStoringSampleFromPatient");
        }

        public static void UpdateStateTakingSample(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateTakingSample");
        }

        public static void UpdateStateUsingEquipment(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateUsingEquipment");
        }

        public static void UpdateStateWritingNotes(this BehaviorLabSpecialist instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateWritingNotes");
        }
    }
}
