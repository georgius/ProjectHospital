using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEngine;

namespace ModAdvancedGameChanges .Lopital
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

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, added to hospital, trying to find common room");

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

            employeeComponent.m_state.m_department = MapScriptInterface.Instance.GetActiveDepartment();
            employeeComponent.m_state.m_startDay = DayTime.Instance.GetDay();

            Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

            if (position == Vector3i.ZERO_VECTOR)
            {
                // not found common room, stay at home
                BehaviorJanitorPatch.GoHome(__instance);
                return false;
            }

            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);
            __instance.SwitchState(BehaviorJanitorState.Walking);

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "CheckNeeds")]
        public static bool CheckNeedsPrefix(BehaviorJanitor __instance, ref bool __result)
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

            // check if janitor should go to lunch
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
                    __instance.CancelUsingComputer();
                    __instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                    __instance.SwitchState(BehaviorJanitorState.FulfillingNeeds);

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
        [HarmonyPatch(typeof(BehaviorJanitor), nameof(BehaviorJanitor.GoToWorkPlace))]
        public static bool GoToWorkPlacePrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            WalkComponent walkComponent = __instance.GetComponent<WalkComponent>(); ;
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            if (employeeComponent.GetWorkChair() != null)
            {
                TileObject workChair = employeeComponent.GetWorkChair();
                if (!walkComponent.IsSittingOn(workChair) && !walkComponent.IsWalkingTo(workChair))
                {
                    walkComponent.GoSit(workChair, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to chair");

                    __instance.m_state.m_usingComputer = false;
                    __instance.SwitchState(BehaviorJanitorState.GoingToWorkplace);
                }
            }
            else if (BehaviorJanitorPatch.GetWorkDeskMod(__instance) != null)
            {
                Vector2f defaultUsePosition = BehaviorJanitorPatch.GetWorkDeskMod(__instance).GetDefaultUsePosition();
                if (defaultUsePosition.subtract(walkComponent.m_state.m_currentPosition).length() > 0.25f)
                {
                    walkComponent.SetDestination(defaultUsePosition, BehaviorJanitorPatch.GetWorkDeskMod(__instance).GetFloorIndex(), MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to work desk");

                    __instance.m_state.m_usingComputer = false;
                    __instance.SwitchState(BehaviorJanitorState.GoingToWorkplace);
                }
            }
            else if (employeeComponent.m_state.m_workPlacePosition != walkComponent.GetCurrentTile())
            {
                walkComponent.SetDestination(employeeComponent.m_state.m_workPlacePosition, employeeComponent.m_state.m_workPlaceFloorIndex, MovementType.WALKING);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to work place position");

                __instance.m_state.m_usingComputer = false;
                __instance.SwitchState(BehaviorJanitorState.GoingToWorkplace);
            }
            else
            {
                // by default, go to common room
                Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

                if (position != Vector3i.ZERO_VECTOR)
                {
                    __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");

                    __instance.m_state.m_usingComputer = false;
                    __instance.SwitchState(BehaviorJanitorState.GoingToWorkplace);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "GoReturnCart")]
        public static bool GoReturnCartPrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

            if (__instance.m_state.m_cart != null && __instance.m_state.m_cart.GetEntity() != null && __instance.m_state.m_cart.GetEntity().m_state.m_position != Vector2i.ZERO_VECTOR)
            {
                __instance.GetComponent<WalkComponent>().SetDestination(__instance.m_state.m_cart.GetEntity().m_state.m_position, __instance.m_state.m_cart.GetEntity().GetFloorIndex(), MovementType.WALKING);
                __instance.SwitchState(BehaviorJanitorState.GoingToReturnCart);
            }
            else
            {
                if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsTraining(__instance))
                {
                    if (BehaviorJanitorPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to workplace");

                        __instance.GoToWorkPlace();
                        __instance.SwitchState(BehaviorJanitorState.GoingToWorkplace);
                    }
                    else
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, nothing to do");

                        __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), nameof(BehaviorJanitor.ReceiveMessage))]
        public static bool ReceiveMessagePrefix(Message message, BehaviorJanitor __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} Message received: {message.m_messageID}");

            if (message.m_messageID == Messages.REST_REDUCED)
            {
                __instance.GetComponent<MoodComponent>().GetNeed(Needs.Vanilla.Rest).ReduceRandomized(message.m_mainParameter, __instance.GetComponent<MoodComponent>());

                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "SelectNextAction")]
        public static bool SelectNextActionPrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            if ((homeRoomType != null) && homeRoomType.HasAnyTag(new string[] { Tags.Vanilla.JanitorAdminWorkplace, Tags.Mod.JanitorTrainingWorkspace }))
            {
                BehaviorJanitorPatch.FreeObjectMod(__instance);
                BehaviorJanitorPatch.FreeTileMod(__instance);
                BehaviorJanitorPatch.FreeRoomMod(__instance);
                BehaviorJanitorPatch.GoReturnCartMod(__instance);
            }
            else
            {
                if (__instance.m_state.m_cleaningTime <= -1f)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, starting cleaning tile");

                    __instance.GetComponent<AnimModelComponent>().PlayAnimation("stand_mop", true);

                    BehaviorJanitorPatch.UpdateCleaningTimeMod(__instance);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, cleaning time {__instance.m_state.m_cleaningTime.ToString(CultureInfo.InvariantCulture)}");

                    PlayerStatistics.Instance.IncrementStatistic("STAT_TILES_CLEAN", 1);
                    MapScriptInterface.Instance.CleanTile(__instance.GetComponent<WalkComponent>().GetCurrentTile(), __instance.GetComponent<WalkComponent>().GetFloorIndex());
                }
                else if (__instance.m_state.m_cleaningTime <= 0f)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, cleaning tile over");

                    __instance.GetComponent<AnimModelComponent>().PlayAnimation("stand_idle", true);

                    employeeComponent.AddSkillPoints(Skills.Vanilla.SKILL_JANITOR_QUALIF_DEXTERITY, Tweakable.Vanilla.JanitorDexteritySkillPoints(), true);

                    __instance.m_state.m_cleaningTime = -1f;

                    if (!BehaviorJanitorPatch.TryToSelectTileInCurrentRoomMod(__instance))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, dirty tile in current room not found");

                        if (!BehaviorJanitorPatch.TryToSelectTileInARoomMod(__instance))
                        {
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, dirty tile in room not found");

                            if (!BehaviorJanitorPatch.TryToSelectIndoorTileMod(__instance, BehaviorJanitorPatch.DirtinessThreshold))
                            {
                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, dirty tile not found");

                                BehaviorJanitorPatch.FreeObjectMod(__instance);
                                BehaviorJanitorPatch.FreeTileMod(__instance);
                                BehaviorJanitorPatch.FreeRoomMod(__instance);
                                BehaviorJanitorPatch.GoReturnCartMod(__instance);
                            }
                        }
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "TryToSelectTileInARoom")]
        public static bool TryToSelectTileInARoomPrefix(BehaviorJanitor __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            WalkComponent walkComponent = __instance.GetComponent<WalkComponent>();

            Vector3i dirtyTileFloor = MapScriptInterface.Instance.FindDirtiestTileInRoomWithMatchingAssignmentAnyFloor(__instance, __instance.GetDepartment(), BehaviorJanitorPatch.DirtinessThreshold);
            if (dirtyTileFloor.m_x == 0 && dirtyTileFloor.m_y == 0)
            {
                dirtyTileFloor = MapScriptInterface.Instance.FindDirtiestTileInAnyUnreservedRoomAnyFloor(__instance.GetDepartment(), BehaviorJanitorPatch.DirtinessThreshold);
            }
            if (dirtyTileFloor.m_x == 0 && dirtyTileFloor.m_y == 0)
            {
                __result = false;
                return false;
            }

            Vector2i dirtyTile = new Vector2i(dirtyTileFloor.m_x, dirtyTileFloor.m_y);

            if (dirtyTile != Vector2i.ZERO_VECTOR)
            {
                BehaviorJanitor janitorManager = null;
                if (employeeComponent.m_state.m_supervisor != null)
                {
                    janitorManager = employeeComponent.m_state.m_supervisor.GetEntity()?.GetComponent<BehaviorJanitor>();
                }

                float skillPoints = (float)Tweakable.Vanilla.MainSkillPoints() * UnityEngine.Random.Range(0.25f, 0.75f);

                if ((dirtyTile != Vector2i.ZERO_VECTOR) && (janitorManager != null))
                {
                    float points = UnityEngine.Random.Range(0f, 1f) * skillPoints;

                    janitorManager.GetComponent<EmployeeComponent>().AddSkillPoints(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_MANAGER, (int)points, false);
                }

                employeeComponent.AddSkillPoints(Skills.Vanilla.SKILL_JANITOR_QUALIF_EFFICIENCY, (int)skillPoints, true);

                __instance.GetComponent<WalkComponent>().SetDestination(dirtyTile, dirtyTileFloor.m_z, MovementType.WALKING);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, dirty tile [{dirtyTile.m_x.ToString(CultureInfo.InvariantCulture)}, {dirtyTile.m_y.ToString(CultureInfo.InvariantCulture)}], current tile [{walkComponent.GetCurrentTile().m_x.ToString(CultureInfo.InvariantCulture)}, {walkComponent.GetCurrentTile().m_y.ToString(CultureInfo.InvariantCulture)}]");

                __result = true;
                return false;
            }

            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "TryToSelectTileInCurrentRoom")]
        public static bool TryToSelectTileInCurrentRoomPrefix(BehaviorJanitor __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            WalkComponent walkComponent = __instance.GetComponent<WalkComponent>();

            if (__instance.m_state.m_room == null)
            {
                __result = false;
                return false;
            }

            if (__instance.m_state.m_room.GetEntity() == null)
            {
                __result = false;
                return false;
            }

            BehaviorJanitor janitorManager = null;
            if (employeeComponent.m_state.m_supervisor != null)
            {
                janitorManager = employeeComponent.m_state.m_supervisor.GetEntity()?.GetComponent<BehaviorJanitor>();
            }

            float skillLevel = employeeComponent.GetSkillLevel(Skills.Vanilla.SKILL_JANITOR_QUALIF_EFFICIENCY);

            if (UnityEngine.Random.Range(Skills.SkillLevelMinimum, Skills.SkillLevelMaximum) < skillLevel)
            {
                Vector2i dirtyTile = Vector2i.ZERO_VECTOR;

                if (UnityEngine.Random.Range(Skills.SkillLevelMinimum, Skills.SkillLevelMaximum) < skillLevel)
                {
                    dirtyTile = MapScriptInterface.Instance.FindClosestDirtyTileInARoom(__instance.m_state.m_room.GetEntity(), walkComponent.GetCurrentTile());

                    if ((dirtyTile != Vector2i.ZERO_VECTOR) && (janitorManager != null))
                    {
                        float points = UnityEngine.Random.Range(0f, 1f) * (float)Tweakable.Vanilla.MainSkillPoints();

                        janitorManager.GetComponent<EmployeeComponent>().AddSkillPoints(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_MANAGER, (int)points, false);
                    }
                }
                else
                {
                    dirtyTile = MapScriptInterface.Instance.FindDirtiestTileInARoom(__instance.m_state.m_room.GetEntity(), false, BehaviorJanitorPatch.DirtinessThreshold);

                    if ((dirtyTile != Vector2i.ZERO_VECTOR) && (janitorManager != null))
                    {
                        float points = UnityEngine.Random.Range(0f, 0.5f) * (float)Tweakable.Vanilla.MainSkillPoints();

                        janitorManager.GetComponent<EmployeeComponent>().AddSkillPoints(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_MANAGER, (int)points, false);
                    }
                }

                if (dirtyTile != Vector2i.ZERO_VECTOR)
                {
                    employeeComponent.AddSkillPoints(Skills.Vanilla.SKILL_JANITOR_QUALIF_EFFICIENCY, Tweakable.Vanilla.MainSkillPoints(), true);

                    __instance.GetComponent<WalkComponent>().SetDestination(dirtyTile, __instance.m_state.m_room.GetEntity().GetFloorIndex(), MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, dirty tile [{dirtyTile.m_x.ToString(CultureInfo.InvariantCulture)}, {dirtyTile.m_y.ToString(CultureInfo.InvariantCulture)}], current tile [{walkComponent.GetCurrentTile().m_x.ToString(CultureInfo.InvariantCulture)}, {walkComponent.GetCurrentTile().m_y.ToString(CultureInfo.InvariantCulture)}]");

                    __result = true;
                    return false;
                }
                else
                {
                    __result = false;
                    return false;
                }
            }

            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "TryToSelectIndoorTile")]
        public static bool TryToSelectIndoorTilePrefix(int threshold, BehaviorJanitor __instance, ref bool __result)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            WalkComponent walkComponent = __instance.GetComponent<WalkComponent>();

            Vector2i dirtyTile = MapScriptInterface.Instance.FindClosestDirtyIndoorsTile(walkComponent.GetCurrentTile(), walkComponent.GetFloorIndex(), threshold);
            if (dirtyTile == Vector2i.ZERO_VECTOR)
            {
                __result = false;
                return false;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, dirty tile [{dirtyTile.m_x.ToString(CultureInfo.InvariantCulture)}, {dirtyTile.m_y.ToString(CultureInfo.InvariantCulture)}], current tile [{walkComponent.GetCurrentTile().m_x.ToString(CultureInfo.InvariantCulture)}, {walkComponent.GetCurrentTile().m_y.ToString(CultureInfo.InvariantCulture)}]");

            walkComponent.SetDestination(dirtyTile, walkComponent.GetFloorIndex(), MovementType.WALKING);

            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateCleaningTime")]
        public static bool UpdateCleaningTimePrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            WalkComponent walkComponent = __instance.GetComponent<WalkComponent>();
            PerkComponent perkComponent = __instance.GetComponent<PerkComponent>();

            Floor floor = Hospital.Instance.m_floors[walkComponent.GetFloorIndex()];
            DirtType dirtType = floor.m_mapPersistentData.m_tiles[walkComponent.GetCurrentTile().m_x, walkComponent.GetCurrentTile().m_y].m_dirtType;
            float dirtLevel = floor.m_mapPersistentData.m_tiles[walkComponent.GetCurrentTile().m_x, walkComponent.GetCurrentTile().m_y].m_dirtLevel;

            float cleaningTime = (dirtType == DirtType.DIRT) ? Tweakable.Mod.CleaningTimeDirt() : Tweakable.Mod.CleaningTimeBlood();
            float skillLevel = employeeComponent.GetSkillLevel(Skills.Vanilla.SKILL_JANITOR_QUALIF_DEXTERITY);

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, dirt {dirtType}, dirt level {dirtLevel.ToString(CultureInfo.InvariantCulture)}, cleaning time {cleaningTime.ToString(CultureInfo.InvariantCulture)}, skill level {skillLevel.ToString(CultureInfo.InvariantCulture)}");

            cleaningTime *= dirtLevel / skillLevel;

            if (perkComponent.m_perkSet.HasPerk(Perks.Vanilla.Chemist))
            {
                if (perkComponent.m_perkSet.HasHiddenPerk(Perks.Vanilla.Chemist))
                {
                    perkComponent.RevealPerk(Perks.Vanilla.Chemist, __instance.m_state.m_bookmarked);
                }

                cleaningTime *= UnityEngine.Random.Range(0.5f, 1f);
            }

            BehaviorJanitor janitorManager = null;
            float penalty = 1f;
            float bonus = 0f;

            if (employeeComponent.m_state.m_supervisor != null)
            {
                janitorManager = employeeComponent.m_state.m_supervisor.GetEntity()?.GetComponent<BehaviorJanitor>();

                if (janitorManager != null)
                {
                    penalty = UnityEngine.Random.Range(Skills.SkillLevelMinimum, Skills.SkillLevelMaximum + Skills.SkillLevelMinimum - janitorManager.GetComponent<EmployeeComponent>().m_state.m_skillSet.GetSkillLevel(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_MANAGER));
                    bonus = (float)Tweakable.Vanilla.JanitorManagerCleaningBonusPercent() / (100f * penalty);
                }
            }

            bonus += __instance.m_state.m_cartAvailable ? UnityEngine.Random.Range(0f, 0.2f) : 0f;

            cleaningTime *= Mathf.Max(0f, Mathf.Min(1f, (1 - bonus)));

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, chemist {perkComponent.m_perkSet.HasPerk(Perks.Vanilla.Chemist)}, penalty {penalty.ToString(CultureInfo.InvariantCulture)}, bonus {bonus.ToString(CultureInfo.InvariantCulture)}, cart {__instance.m_state.m_cartAvailable}");

            __instance.m_state.m_cleaningTime = cleaningTime;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateAdminIdle")]
        public static bool UpdateStateAdminIdlePrefix(float deltaTime, BehaviorJanitor __instance)
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

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
            GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

            __instance.m_state.m_idleTime += deltaTime;

            if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsTraining(__instance))
            {
                employeeComponent.FindWorkplace();
                if (!__instance.IsAtWorkplace(employeeComponent))
                {
                    __instance.GoToWorkPlace();
                    return false;
                }

                if (__instance.m_state.m_timeInState > 2f)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, managing janitors");

                    if (!__instance.m_state.m_usingComputer && employeeComponent.m_state.m_workDesk.GetEntity() != null)
                    {
                        employeeComponent.m_state.m_workDesk.GetEntity().GetComponent<AnimatedObjectComponent>().ForceFrame(1);
                    }

                    if (!__instance.m_state.m_usingComputer && employeeComponent.GetWorkChair() != null && __instance.IsAtWorkplace(employeeComponent))
                    {
                        __instance.GetComponent<AnimModelComponent>().QueueAnimation("sit_relax_pc_in", false, true);
                        __instance.GetComponent<AnimModelComponent>().QueueAnimation("sit_relax_pc_idle", true, false);
                        __instance.m_state.m_usingComputer = true;
                    }

                    __instance.m_state.m_timeInState = 0f;
                }
            }
            else
            {
                __instance.CancelUsingComputer();
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateAtHome")]
        public static bool UpdateStateAtHomePrefix(BehaviorJanitor __instance)
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

            // janitors don't have commuting state as doctors, nurses and lab specialists
            // we just keep them at home longer time

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
                __instance.SwitchState(BehaviorJanitorState.Walking);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateCleaning")]
        public static bool UpdateStateCleaningPrefix(float deltaTime, BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

                if (BehaviorJanitorPatch.IsNeededHandleGoHome(__instance) || BehaviorJanitorPatch.IsNeededHandleFullfillNeeds(__instance) || BehaviorJanitorPatch.IsNeededHandleTraining(__instance))
                {
                    BehaviorJanitorPatch.FreeObjectMod(__instance);
                    BehaviorJanitorPatch.FreeTileMod(__instance);
                    BehaviorJanitorPatch.FreeRoomMod(__instance);
                    BehaviorJanitorPatch.GoReturnCartMod(__instance);
                }
                else
                {
                    BehaviorJanitorPatch.SelectNextActionMod(__instance);
                }

                __instance.m_state.m_workingTime += deltaTime;
                if (__instance.m_state.m_cleaningTime >= 0f)
                {
                    __instance.m_state.m_cleaningTime -= deltaTime;
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

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
                GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, arrived to workplace");

                employeeComponent.CheckRoomSatisfactionBonuses();
                employeeComponent.CheckMoodModifiers(__instance.IsBookmarked());

                if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsTraining(__instance))
                {
                    if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
                    {
                        TileObject entity = employeeComponent.m_state.m_workDesk.GetEntity();
                        if (entity != null)
                        {
                            entity.SetLightEnabled(true);
                            entity.GetComponent<AnimatedObjectComponent>().ForceFrame(1);
                        }

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, nothing to do, filling free time");
                        __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);
                    }
                    else if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Vanilla.JanitorAdminWorkplace))
                    {
                        TileObject entity = employeeComponent.m_state.m_workDesk.GetEntity();
                        if (entity != null)
                        {
                            entity.SetLightEnabled(true);
                            entity.GetComponent<AnimatedObjectComponent>().ForceFrame(1);
                        }

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, nothing to do, managing janitors");
                        __instance.SwitchState(BehaviorJanitorState.AdminIdle);
                    }
                    else
                    {
                        if (__instance.m_state.m_cart == null)
                        {
                            BehaviorJanitorPatch.FindCartMod(__instance);
                        }

                        if (__instance.m_state.m_cart != null)
                        {
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to cart");

                            __instance.GetComponent<WalkComponent>().SetDestination(__instance.m_state.m_cart.GetEntity().GetDefaultUsePosition(), __instance.m_state.m_cart.GetEntity().GetFloorIndex(), MovementType.WALKING);
                            __instance.SwitchState(BehaviorJanitorState.WalkingToCart);
                        }
                        else
                        {
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, no cart, cleaning");

                            __instance.m_state.m_cartAvailable = false;
                            __instance.SwitchState(BehaviorJanitorState.Cleaning);
                        }
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateFillingFreeTime")]
        public static bool UpdateStateFillingFreeTimePrefix(float deltaTime, BehaviorJanitor __instance)
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
                // currently janitor is not doing anything
                if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                {
                    if ((homeRoomType == null) 
                        || ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
                        || ((homeRoomType != null) && (!homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace)) && (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsTraining(__instance))))
                    {
                        // janitor still don't have anything to do
                        // just stay in common room and fill free time
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, nothing to do");

                        // the janitor don't have to be in common room !
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

                                __instance.m_state.m_hadBreak = true;
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

            if (!__instance.GetComponent<ProcedureComponent>().IsBusy())
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, fulfilling need finished");

                if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeeds(__instance))
                {
                    if ((homeRoomType == null)
                        || ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace) && (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance)))
                        || ((homeRoomType != null) && (!homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace)) && (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsTraining(__instance))))
                    {
                        // by default, go to common room
                        Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

                        if (position != Vector3i.ZERO_VECTOR)
                        {
                            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");

                            __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);
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

            employeeComponent.CheckRoomSatisfactionBonuses();
            employeeComponent.CheckMoodModifiers(__instance.IsBookmarked());

            if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
            {
                if (employeeComponent.UpdateTraining(__instance.GetComponent<ProcedureComponent>()))
                {
                    // training was finished

                    if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsTraining(__instance))
                    {
                        // default case, training is not possible more
                        __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);

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
                // janitor manager or regular janitor
                if (employeeComponent.UpdateTraining(__instance.GetComponent<ProcedureComponent>()))
                {
                    // training was finished

                    if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                    {
                        // default case, training is not possible more
                        __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);

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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateReturningCart")]
        public static bool UpdateStateReturningCartPrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, returned cart");

                __instance.m_state.m_cart.GetEntity().User = null;
                __instance.m_state.m_cart.GetEntity().SetAttachedToCharacter(false);
                __instance.m_state.m_cart.GetEntity().StopSounds();
                __instance.m_state.m_cartAvailable = false;

                if (MapScriptInterface.Instance.MoveObject(__instance.m_state.m_cart.GetEntity(), __instance.m_state.m_cartHomeTile))
                {
                    __instance.m_state.m_cart.GetEntity().Orientation = __instance.m_state.m_cartHomeOrientation;
                }
                else
                {
                    Hospital.Instance.m_floors[__instance.m_state.m_cart.GetEntity().GetFloorIndex()].m_movingObjects.Remove(__instance.m_state.m_cart.GetEntity());
                    __instance.m_state.m_cart.GetEntity().m_state.m_department.GetEntity().RemoveObject(__instance.m_state.m_cart.GetEntity());
                    __instance.m_state.m_cart.GetEntity().Destroy();
                }
                __instance.m_state.m_cart.GetEntity().StopSounds();
                __instance.m_state.m_cart = null;

                if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsTraining(__instance))
                {
                    if (BehaviorJanitorPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to workplace");

                        __instance.GoToWorkPlace();
                        __instance.SwitchState(BehaviorJanitorState.GoingToWorkplace);
                    }
                    else
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, nothing to do");

                        __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateWalking")]
        public static bool UpdateStateWalkingPrefix(float deltaTime, BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            // "Walking" state is only for arriving janitor to hospital (directly to common room)

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, arrived to hospital");

                employeeComponent.CheckChiefNodiagnoseDepartment(true);
                employeeComponent.CheckRoomSatisfactionBonuses();
                employeeComponent.CheckMoodModifiers(__instance.IsBookmarked());
                employeeComponent.CheckBossModifiers();
                employeeComponent.UpdateHomeRoom();
                __instance.GetComponent<AnimModelComponent>().RevertToDefaultClothes(false);

                __instance.m_state.m_hadBreak = false;
                __instance.m_state.m_hadLunch = false;
                __instance.m_state.m_usingComputer = false;
                __instance.m_state.m_idleTime = 0f;
                __instance.m_state.m_cleaningTime = 0f;

                if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeedsGoToWorkplace(__instance))
                {
                    // default case
                    __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateWalkingToCartToNextRoom")]
        public static bool UpdateStateWalkingToCartToNextRoomPrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, arrived to cart");

                EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

                if (__instance.m_state.m_cart != null)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, picking up cart");

                    MapScriptInterface.Instance.PickUpObject(__instance.m_state.m_cart.GetEntity());
                    __instance.m_state.m_cart.GetEntity().SetAttachedToCharacter(true);
                    __instance.m_state.m_cartAvailable = true;
                }

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, cleaning");

                __instance.SwitchState(BehaviorJanitorState.Cleaning);
            }

            return false;
        }

        private static bool IsNeededHandleGoHome(BehaviorJanitor instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            return (employeeComponent.IsFired() || instance.GetDepartment().IsClosed()
                || ((DayTime.Instance.GetShift() != employeeComponent.m_state.m_shift) && (Mathf.Abs(DayTime.Instance.GetDayTimeHours() - shift.StartTime) > 1)));
        }

        private static bool IsNeededHandleFullfillNeeds(BehaviorJanitor instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();
            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            // check if janitor should go to lunch
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

        private static bool IsNeededHandleTraining(BehaviorJanitor instance)
        {
            return instance.GetComponent<EmployeeComponent>().ShouldGoToTraining();
        }

        public static bool HandleGoHomeFulfillNeeds(BehaviorJanitor instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            // check if janitor needs to go home
            if (BehaviorJanitorPatch.GoHome(instance))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, going home");
                return true;
            }

            // check if janitor needs to fulfill his/her needs
            if (BehaviorJanitorPatch.CheckNeedsMod(instance))
            {
                instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                instance.SwitchState(BehaviorJanitorState.FulfillingNeeds);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, fulfilling needs");
                return true;
            }

            return false;
        }

        public static bool HandleGoHomeFulfillNeedsGoToWorkplace(BehaviorJanitor instance)
        {
            if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeeds(instance))
            {
                EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();
                WalkComponent walkComponent = instance.GetComponent<WalkComponent>();
                GameDBRoomType homeRoomType = employeeComponent.GetHomeRoomType();
                Entity oppositeShiftEmployee = employeeComponent.GetOppositeShiftEmployee();

                bool canGoToWorkplace = (oppositeShiftEmployee == null) ||
                    ((oppositeShiftEmployee != null) && ((oppositeShiftEmployee.GetComponent<BehaviorJanitor>().m_state.m_janitorState == BehaviorJanitorState.AtHome)
                        || (oppositeShiftEmployee.GetComponent<BehaviorJanitor>().m_state.m_janitorState == BehaviorJanitorState.GoingHome)
                        || (oppositeShiftEmployee.GetComponent<BehaviorJanitor>().m_state.m_janitorState == BehaviorJanitorState.FiredAtHome)));

                canGoToWorkplace &= ((employeeComponent.GetWorkChair() != null)
                    || (BehaviorJanitorPatch.GetWorkDeskMod(instance) != null)
                    || ((employeeComponent.m_state.m_workPlacePosition != Vector2i.ZERO_VECTOR) && (employeeComponent.m_state.m_workPlacePosition != walkComponent.GetCurrentTile())));

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, opposite shift employee: {oppositeShiftEmployee?.Name ?? "NULL"}, state: {oppositeShiftEmployee?.GetComponent<BehaviorJanitor>().m_state.m_janitorState.ToString() ?? "NULL"}");

                if (canGoToWorkplace)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, can go to workplace");

                    if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, going to janitor training workplace");

                        instance.GoToWorkPlace();
                        return true;
                    }

                    if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Vanilla.JanitorAdminWorkplace))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, going to janitor admin workplace");

                        instance.GoToWorkPlace();
                        return true;
                    }

                    // regular janitor
                    if (BehaviorJanitorPatch.FindAnyDirtyTile(instance) != Vector3i.ZERO_VECTOR)
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, going to janitor workplace");

                        instance.GoToWorkPlace();
                        return true;
                    }
                }
                else
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, cannot go to workplace");
                }

                return false;
            }

            return true;
        }

        public static bool HandleGoHomeFulfillNeedsTraining(BehaviorJanitor instance)
        {
            if (!BehaviorJanitorPatch.HandleGoHomeFulfillNeeds(instance))
            {
                EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();
                GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

                if ((homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
                {
                    // it is janitor in training
                    // set him/her training flags

                    if (!employeeComponent.ShouldGoToTraining())
                    {
                        BehaviorPatch.ChooseSkillToTrainAndToggleTraining(instance);
                    }

                    // if possible, go to training
                    if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(instance.GetComponent<ProcedureComponent>()))
                    {
                        instance.SwitchState(BehaviorJanitorState.Training);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, training");
                        return true;
                    }
                }

                if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(instance.GetComponent<ProcedureComponent>()))
                {
                    instance.SwitchState(BehaviorJanitorState.Training);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, training");
                    return true;
                }

                return false;
            }

            return true;
        }

        private static bool CheckNeedsMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("CheckNeeds", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static void FindCartMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("FindCart", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void FreeObjectMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("FreeObject", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void FreeRoomMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("FreeRoom", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void FreeTileMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("FreeTile", BindingFlags.NonPublic | BindingFlags.Instance);
            
            methodInfo.Invoke(instance, null);
        }

        private static void GoReturnCartMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("GoReturnCart", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static TileObject GetWorkDeskMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("GetWorkDesk", BindingFlags.NonPublic | BindingFlags.Instance);

            return (TileObject)methodInfo.Invoke(instance, null);
        }

        private static bool GoHome(BehaviorJanitor instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            GameDBSchedule shift = (employeeComponent.m_state.m_shift == Shift.DAY) ?
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF) :
                Database.Instance.GetEntry<GameDBSchedule>(Schedules.Vanilla.SCHEDULE_OPENING_HOURS_STAFF_NIGHT);

            if (employeeComponent.IsFired() || instance.GetDepartment().IsClosed() 
                || ((DayTime.Instance.GetShift() != employeeComponent.m_state.m_shift) && (Mathf.Abs(DayTime.Instance.GetDayTimeHours() - shift.StartTime) > 1)))
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

                return true;
            }

            return false;
        }

        private static bool TryToSelectIndoorTileMod(BehaviorJanitor instance, int threshold)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("TryToSelectIndoorTile", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, new object[] { threshold });
        }

        private static bool TryToSelectTileInARoomMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("TryToSelectTileInARoom", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static bool TryToSelectTileInCurrentRoomMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("TryToSelectTileInCurrentRoom", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static void SelectNextActionMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("SelectNextAction", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void UpdateCleaningTimeMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("UpdateCleaningTime", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static Vector3i FindAnyDirtyTile(BehaviorJanitor instance)
        {
            Vector2i position = instance.GetComponent<WalkComponent>().GetCurrentTile();
            int floorIndex = instance.GetComponent<WalkComponent>().GetFloorIndex();

            Vector2i tile = MapScriptInterface.Instance.FindClosestDirtyIndoorsTile(position, floorIndex, BehaviorJanitorPatch.DirtinessThreshold);

            return new Vector3i(tile.m_x, tile.m_y, (tile == Vector2i.ZERO_VECTOR) ? 0 : floorIndex);
        }

        public const int DirtinessThreshold = 12;
    }
}
