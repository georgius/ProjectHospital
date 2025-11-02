using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Collections.Generic;
using System.Reflection;
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
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, added to hospital, trying to find common room");

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

            employeeComponent.m_state.m_department = MapScriptInterface.Instance.GetActiveDepartment();
            employeeComponent.m_state.m_startDay = DayTime.Instance.GetDay();

            Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

            if (position == Vector3i.ZERO_VECTOR)
            {
                // not found common room, stay at home
                BehaviorNursePatch.GoHome(__instance);
                return false;
            }

            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);
            __instance.SwitchState(NurseState.Commuting);

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "CheckNeeds")]
        public static bool CheckNeedsPrefix(AccessRights accessRights, bool surgeryScheduled, BehaviorNurse __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            // check if nurse should go to lunch
            if ((!__instance.m_state.m_hadLunch) &&
                ((DayTime.Instance.IsScheduledActionTime(Schedules.Vanilla.SCHEDULE_STAFF_LUNCH, true) && (DayTime.Instance.GetShift() == Shift.DAY) && (employeeComponent.m_state.m_shift == Shift.DAY) && (DayTime.Instance.GetDayTimeHours() < shift.EndTime)) ||
                (DayTime.Instance.IsScheduledActionTime(Schedules.Mod.SCHEDULE_STAFF_LUNCH_NIGHT, true) && (DayTime.Instance.GetShift() == Shift.NIGHT) && (employeeComponent.m_state.m_shift == Shift.NIGHT) && (DayTime.Instance.GetDayTimeHours() < shift.EndTime) && ViewSettingsPatch.m_staffLunchNight[SettingsManager.Instance.m_viewSettings].m_value)))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, can go to lunch");

                GameDBProcedure staffLunchProcedure = Database.Instance.GetEntry<GameDBProcedure>(Procedures.Vanilla.StaffLunch);

                if (__instance.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(
                    staffLunchProcedure, __instance.m_entity, __instance.GetDepartment(), AccessRights.STAFF, EquipmentListRules.ONLY_FREE_SAME_FLOOR_PREFER_DPT) == ProcedureSceneAvailability.AVAILABLE)
                {
                    __instance.GetComponent<ProcedureComponent>().StartProcedure(staffLunchProcedure, __instance.m_entity, __instance.GetDepartment(), AccessRights.STAFF, EquipmentListRules.ONLY_FREE_SAME_FLOOR_PREFER_DPT);
                    __instance.m_state.m_hadLunch = true;
                    __instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                    __instance.SwitchState(NurseState.FulfillingNeeds);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to lunch");

                    __result = true;
                    return false;
                }
            }

            List<Need> needsSortedFromMostCritical = __instance.GetComponent<MoodComponent>().GetNeedsSortedFromMostCritical();
            foreach (Need need in needsSortedFromMostCritical)
            {
                if (need.m_currentValue > 50f && __instance.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(need.m_gameDBNeed.Entry.Procedure, __instance.m_entity, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF_ONLY, EquipmentListRules.ONLY_FREE_SAME_FLOOR) == ProcedureSceneAvailability.AVAILABLE)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, fulfilling need {need.m_gameDBNeed.Entry.DatabaseID}");

                    __instance.GetComponent<ProcedureComponent>().StartProcedure(need.m_gameDBNeed.Entry.Procedure, __instance.m_entity, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF_ONLY, EquipmentListRules.ONLY_FREE_SAME_FLOOR);

                    __result = true;
                    return false;
                }
            }

            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.GoToWorkplace))]
        public static bool GoToWorkplacePrefix(BehaviorNurse __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
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

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to chair");

                    __instance.SwitchState(NurseState.GoingToWorkplace);
                }
            }
            else if (BehaviorNursePatch.GetWorkDeskInternal(__instance) != null)
            {
                Vector2f defaultUsePosition = BehaviorNursePatch.GetWorkDeskInternal(__instance).GetDefaultUsePosition();
                if (defaultUsePosition.subtract(walkComponent.m_state.m_currentPosition).length() > 0.25f)
                {
                    walkComponent.SetDestination(defaultUsePosition, BehaviorNursePatch.GetWorkDeskInternal(__instance).GetFloorIndex(), MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to work desk");

                    __instance.SwitchState(NurseState.GoingToWorkplace);
                }
            }
            else if ((employeeComponent.m_state.m_workPlacePosition != Vector2i.ZERO_VECTOR) && (employeeComponent.m_state.m_workPlacePosition != walkComponent.GetCurrentTile()))
            {
                walkComponent.SetDestination(employeeComponent.m_state.m_workPlacePosition, employeeComponent.m_state.m_workPlaceFloorIndex, MovementType.WALKING);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to work place position");

                __instance.SwitchState(NurseState.GoingToWorkplace);
            }
            else
            {
                // by default, go to common room
                Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

                if (position != Vector3i.ZERO_VECTOR)
                {
                    __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, no workplace, going to common room, filling free time");

                    __instance.SwitchState(NurseState.FillingFreeTime);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.IsHidden))]
        public static bool IsHidden(BehaviorNurse __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            __result = (__instance.m_state.m_nurseState == NurseState.AtHome) || (__instance.m_state.m_nurseState == NurseState.FiredAtHome);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "UpdateStateAtHome")]
        public static bool UpdateStateAtHomePrefix(BehaviorNurse __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
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

            if (employeeComponent.ShouldStartCommuting() && (!__instance.GetDepartment().IsClosed()))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, trying to find common room");

                Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

                if (position == Vector3i.ZERO_VECTOR)
                {
                    // not found common room, stay at home
                    return false;
                }

                __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);
                __instance.SwitchState(NurseState.Commuting);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "UpdateStateCommuting")]
        public static bool UpdateStateCommutingPrefix(BehaviorNurse __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            // "Commuting" state is only for arriving nurse to hospital (directly to common room)

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, arrived to hospital");

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
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
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
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, nothing to do");

                        // the nurse don't have to be in common room !
                        Room room = MapScriptInterface.Instance.GetRoomAt(__instance.m_entity.GetComponent<WalkComponent>());

                        if ((room != null) && (room.m_roomPersistentData.m_roomType.Entry.HasTag(Tags.Vanilla.CommonRoom)))
                        {
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, in common room, resting");

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
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, not in common room");

                            Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

                            if (position != Vector3i.ZERO_VECTOR)
                            {
                                __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
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
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            if (!__instance.GetComponent<ProcedureComponent>().IsBusy())
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, fulfilling need finished");

                if (!BehaviorNursePatch.HandleGoHomeFulfillNeeds(__instance))
                {
                    if ((homeRoomType == null)
                        || ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace) && (!BehaviorNursePatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance)))
                        || ((homeRoomType != null) && (!homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace)) && (!BehaviorNursePatch.HandleGoHomeFulfillNeedsTraining(__instance))))
                    {
                        // by default, go to common room
                        Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

                        if (position != Vector3i.ZERO_VECTOR)
                        {
                            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");

                            __instance.SwitchState(NurseState.FillingFreeTime);
                        }
                        else
                        {
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, common room room not found");
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
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
                GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, arrived to workplace");

                employeeComponent.CheckRoomSatisfactionBonuses();
                employeeComponent.CheckMoodModifiers(__instance.IsBookmarked());
                __instance.CancelBrowsing();

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

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, nothing to do, filling free time");
                        __instance.SwitchState(NurseState.FillingFreeTime);
                    }
                    else
                    {
                        if (!__instance.GetComponent<WalkComponent>().IsOnBiohazard())
                        {
                            __instance.GetComponent<AnimModelComponent>().RevertToDefaultClothes(false);
                        }
                        __instance.SwitchState(NurseState.Idle);
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), "UpdateStateIdle")]
        public static bool UpdateStateIdlePrefix(float deltaTime, BehaviorNurse __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            if (deltaTime <= 0f)
            {
                return false;
            }

            if (!BehaviorNursePatch.HandleGoHomeFulfillNeedsTraining(__instance))
            {
                // nurse has nothing to do
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorNurse), nameof(BehaviorNurse.UpdateTraining))]
        public static bool UpdateTrainingPrefix(BehaviorNurse __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
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

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, filling free time");

                        Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

                        if (position != Vector3i.ZERO_VECTOR)
                        {
                            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
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

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, filling free time");

                        Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

                        if (position != Vector3i.ZERO_VECTOR)
                        {
                            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
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
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            return (employeeComponent.IsFired() || instance.GetDepartment().IsClosed()
                || ((DayTime.Instance.GetShift() != employeeComponent.m_state.m_shift) && (Mathf.Abs(DayTime.Instance.GetDayTimeHours() - shift.StartTime) > 1)));
        }

        private static bool IsNeededHandleFullfillNeeds(BehaviorNurse instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();
            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            // check if nurse should go to lunch
            if ((!instance.m_state.m_hadLunch) &&
                ((DayTime.Instance.IsScheduledActionTime(Schedules.Vanilla.SCHEDULE_STAFF_LUNCH, true) && (DayTime.Instance.GetShift() == Shift.DAY) && (employeeComponent.m_state.m_shift == Shift.DAY) && (DayTime.Instance.GetDayTimeHours() < shift.EndTime)) ||
                (DayTime.Instance.IsScheduledActionTime(Schedules.Mod.SCHEDULE_STAFF_LUNCH_NIGHT, true) && (DayTime.Instance.GetShift() == Shift.NIGHT) && (employeeComponent.m_state.m_shift == Shift.NIGHT) && (DayTime.Instance.GetDayTimeHours() < shift.EndTime) && ViewSettingsPatch.m_staffLunchNight[SettingsManager.Instance.m_viewSettings].m_value)))
            {
                return true;
            }

            List<Need> needsSortedFromMostCritical = instance.GetComponent<MoodComponent>().GetNeedsSortedFromMostCritical();
            foreach (Need need in needsSortedFromMostCritical)
            {
                if ((need.m_currentValue > 50f) && instance.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(need.m_gameDBNeed.Entry.Procedure, instance.m_entity, employeeComponent.m_state.m_department.GetEntity(), AccessRights.STAFF_ONLY, EquipmentListRules.ONLY_FREE_SAME_FLOOR) == ProcedureSceneAvailability.AVAILABLE)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsNeededHandleTraining(BehaviorNurse instance)
        {
            return instance.GetComponent<EmployeeComponent>().ShouldGoToTraining();
        }

        public static bool HandleGoHomeFulfillNeeds(BehaviorNurse instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            // check if nurse needs to go home
            if (BehaviorNursePatch.GoHome(instance))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, going home");
                return true;
            }

            // check if nurse needs to fulfill his/her needs
            if (BehaviorNursePatch.CheckNeedsInternal(instance))
            {
                instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                instance.SwitchState(NurseState.FulfillingNeeds);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, fulfilling needs");
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
                    || (BehaviorNursePatch.GetWorkDeskInternal(instance) != null)
                    || ((employeeComponent.m_state.m_workPlacePosition != Vector2i.ZERO_VECTOR) && (employeeComponent.m_state.m_workPlacePosition != walkComponent.GetCurrentTile())));

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, opposite shift employee: {oppositeShiftEmployee?.Name ?? "NULL"}, state: {oppositeShiftEmployee?.GetComponent<BehaviorNurse>().m_state.m_nurseState.ToString() ?? "NULL"}");

                if (canGoToWorkplace)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, can go to workplace");

                    if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.NurseTrainingWorkspace))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, going to nurse training workplace");

                        instance.GoToWorkplace();
                        return true;
                    }

                    // regular nurse
                    instance.GoToWorkplace();
                    return true;
                }
                else
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, cannot go to workplace");
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

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, training");
                        return true;
                    }
                }

                if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(instance.GetComponent<ProcedureComponent>()))
                {
                    instance.SwitchState(NurseState.Training);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, training");
                    return true;
                }

                return false;
            }

            return true;
        }

        private static bool CheckNeedsInternal(BehaviorNurse instance)
        {
            Type type = typeof(BehaviorNurse);
            MethodInfo methodInfo = type.GetMethod("CheckNeeds", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, new object[] { AccessRights.STAFF, false });
        }

        private static TileObject GetWorkDeskInternal(BehaviorNurse instance)
        {
            Type type = typeof(BehaviorNurse);
            MethodInfo methodInfo = type.GetMethod("GetWorkDesk", BindingFlags.NonPublic | BindingFlags.Instance);

            return (TileObject)methodInfo.Invoke(instance, null);
        }
    }
}
