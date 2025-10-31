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
    [HarmonyPatch(typeof(BehaviorLabSpecialist))]
    public static class BehaviorLabSpecialistPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.AddToHospital))]
        public static bool AddToHospitalPrefix(BehaviorLabSpecialist __instance)
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
                BehaviorLabSpecialistPatch.GoHomeMod(__instance);
                return false;
            }

            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);
            __instance.SwitchState(LabSpecialistState.Commuting);

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "CheckNeeds")]
        public static bool CheckNeedsPrefix(AccessRights accessRights, BehaviorLabSpecialist __instance, ref bool __result)
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

            // check if lab specialist should go to lunch
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
                    __instance.SwitchState(LabSpecialistState.FulfillingNeeds);

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
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.GoToWorkplace))]
        public static bool GoToWorkplacePrefix(BehaviorLabSpecialist __instance)
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

                    __instance.SwitchState(LabSpecialistState.GoingToWorkplace);
                }
            }
            else if (BehaviorLabSpecialistPatch.GetWorkDeskMod(__instance) != null)
            {
                Vector2f defaultUsePosition = BehaviorLabSpecialistPatch.GetWorkDeskMod(__instance).GetDefaultUsePosition();
                if (defaultUsePosition.subtract(walkComponent.m_state.m_currentPosition).length() > 0.25f)
                {
                    walkComponent.SetDestination(defaultUsePosition, BehaviorLabSpecialistPatch.GetWorkDeskMod(__instance).GetFloorIndex(), MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to work desk");

                    __instance.SwitchState(LabSpecialistState.GoingToWorkplace);
                }
            }
            else if (employeeComponent.m_state.m_workPlacePosition != walkComponent.GetCurrentTile())
            {
                walkComponent.SetDestination(employeeComponent.m_state.m_workPlacePosition, employeeComponent.m_state.m_workPlaceFloorIndex, MovementType.WALKING);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to work place position");

                __instance.SwitchState(LabSpecialistState.GoingToWorkplace);
            }
            else
            {
                // by default, go to common room
                Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

                if (position != Vector3i.ZERO_VECTOR)
                {
                    __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");

                    __instance.SwitchState(LabSpecialistState.GoingToWorkplace);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.IsHidden))]
        public static bool IsHidden(BehaviorLabSpecialist __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            __result = __instance.m_state.m_labSpecialistState == LabSpecialistState.AtHome || __instance.m_state.m_labSpecialistState == LabSpecialistState.FiredAtHome;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateAtHome")]
        public static bool UpdateStateAtHomePrefix(BehaviorLabSpecialist __instance)
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
                __instance.SwitchState(LabSpecialistState.Commuting);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateCommuting")]
        public static bool UpdateStateCommutingPrefix(BehaviorLabSpecialist __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            // "Commuting" state is only for arriving lab specialist to hospital (directly to common room)

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
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
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
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, nothing to do");

                        // the lab specialist don't have to be in common room !
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

                                __instance.m_state.m_idleTime = 0f;

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
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateFulfillingNeeds")]
        public static bool UpdateStateFulfillingNeedsPrefix(BehaviorLabSpecialist __instance)
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

                if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeeds(__instance))
                {
                    if ((homeRoomType == null)
                        || ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace) && (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance)))
                        || ((homeRoomType != null) && (!homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace)) && (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsTraining(__instance))))
                    {
                        // by default, go to common room
                        Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

                        if (position != Vector3i.ZERO_VECTOR)
                        {
                            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");

                            __instance.SwitchState(LabSpecialistState.FillingFreeTime);
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
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateGoingToWorkplace")]
        public static bool UpdateStateGoingToWorkplacePrefix(BehaviorLabSpecialist __instance)
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

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, nothing to do, filling free time");
                        __instance.SwitchState(LabSpecialistState.FillingFreeTime);
                    }
                    else
                    {
                        if (!__instance.GetComponent<WalkComponent>().IsOnBiohazard())
                        {
                            __instance.GetComponent<AnimModelComponent>().RevertToDefaultClothes(false);
                        }
                        __instance.SwitchState(LabSpecialistState.Idle);
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), "UpdateStateIdle")]
        public static bool UpdateStateIdlePrefix(float deltaTime, BehaviorLabSpecialist __instance)
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
            __instance.m_state.m_idleTime += deltaTime;

            if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsTraining(__instance))
            {
                // lab specialist has nothing to do
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.UpdateTraining))]
        public static bool UpdateTrainingPrefix(BehaviorLabSpecialist __instance)
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

            if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace))
            {
                if (employeeComponent.UpdateTraining(__instance.GetComponent<ProcedureComponent>()))
                {
                    // training was finished

                    if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsTraining(__instance))
                    {
                        // default case, training is not possible more
                        __instance.SwitchState(LabSpecialistState.FillingFreeTime);

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
                // regular janitor lab specialist
                if (employeeComponent.UpdateTraining(__instance.GetComponent<ProcedureComponent>()))
                {
                    // training was finished

                    if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                    {
                        // default case, training is not possible more
                        __instance.SwitchState(LabSpecialistState.FillingFreeTime);

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

        private static bool GoHomeMod(BehaviorLabSpecialist instance)
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
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            return (employeeComponent.IsFired() || instance.GetDepartment().IsClosed()
                || ((DayTime.Instance.GetShift() != employeeComponent.m_state.m_shift) && (Mathf.Abs(DayTime.Instance.GetDayTimeHours() - shift.StartTime) > 1)));
        }

        private static bool IsNeededHandleFullfillNeeds(BehaviorLabSpecialist instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();
            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            // check if lab specialist should go to lunch
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

        private static bool IsNeededHandleTraining(BehaviorLabSpecialist instance)
        {
            return instance.GetComponent<EmployeeComponent>().ShouldGoToTraining();
        }

        public static bool HandleGoHomeFulfillNeeds(BehaviorLabSpecialist instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            // check if lab specialist needs to go home
            if (BehaviorLabSpecialistPatch.GoHomeMod(instance))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, going home");
                return true;
            }

            // check if lab specialist needs to fulfill his/her needs
            if (BehaviorLabSpecialistPatch.CheckNeedsMod(instance))
            {
                instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                instance.SwitchState(LabSpecialistState.FulfillingNeeds);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, fulfilling needs");
                return true;
            }

            return false;
        }

        public static bool HandleGoHomeFulfillNeedsGoToWorkplace(BehaviorLabSpecialist instance)
        {
            if (!BehaviorLabSpecialistPatch.HandleGoHomeFulfillNeeds(instance))
            {
                EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

                GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();
                Entity oppositeShiftEmployee = employeeComponent.GetOppositeShiftEmployee();

                bool canGoToWorkplace = (oppositeShiftEmployee == null) ||
                    ((oppositeShiftEmployee != null) && ((oppositeShiftEmployee.GetComponent<BehaviorLabSpecialist>().m_state.m_labSpecialistState == LabSpecialistState.AtHome)
                        || (oppositeShiftEmployee.GetComponent<BehaviorLabSpecialist>().m_state.m_labSpecialistState == LabSpecialistState.GoingHome)));

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, opposite shift employee: {oppositeShiftEmployee?.Name ?? "NULL"}, state: {oppositeShiftEmployee?.GetComponent<BehaviorLabSpecialist>().m_state.m_labSpecialistState.ToString() ?? "NULL"}");

                if (canGoToWorkplace)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, can go to workplace");

                    if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.LabSpecialistTrainingWorkspace))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, going to lab specialist training workplace");

                        instance.GoToWorkplace();
                        return true;
                    }

                    // regular lab specialist
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

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, training");
                        return true;
                    }
                }

                if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(instance.GetComponent<ProcedureComponent>()))
                {
                    instance.SwitchState(LabSpecialistState.Training);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, training");
                    return true;
                }

                return false;
            }

            return true;
        }

        private static bool CheckNeedsMod(BehaviorLabSpecialist instance)
        {
            Type type = typeof(BehaviorLabSpecialist);
            MethodInfo methodInfo = type.GetMethod("CheckNeeds", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, new object[] { AccessRights.STAFF });
        }

        private static TileObject GetWorkDeskMod(BehaviorLabSpecialist instance)
        {
            Type type = typeof(BehaviorLabSpecialist);
            MethodInfo methodInfo = type.GetMethod("GetWorkDesk", BindingFlags.NonPublic | BindingFlags.Instance);

            return (TileObject)methodInfo.Invoke(instance, null);
        }
    }
}
