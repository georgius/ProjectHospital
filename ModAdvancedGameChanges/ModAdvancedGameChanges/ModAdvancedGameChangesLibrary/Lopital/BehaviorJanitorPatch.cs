using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to training workplace");

                employeeComponent.CheckChiefNodiagnoseDepartment(true);

                __instance.GetComponent<WalkComponent>().GoSit(employeeComponent.GetWorkChair(), MovementType.WALKING);
                __instance.SwitchState(BehaviorJanitorState.GoingToWorkplace);

                __instance.GetComponent<WalkComponent>().Floor = Hospital.Instance.GetCurrentFloor();

                return false;
            }

            return true;
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
                __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);
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

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(BehaviorJanitor), "TryToSelectTileInARoom")]
        //public static bool TryToSelectTileInARoomPrefix(BehaviorJanitor __instance, ref bool __result)
        //{
        //    return true;
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(BehaviorJanitor), "TryToSelectTileInCurrentRoom")]
        //public static bool TryToSelectTileInCurrentRoomPrefix(BehaviorJanitor __instance, ref bool __result)
        //{
        //    return true;
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(BehaviorJanitor), "TryToSelectIndoorTile")]
        //public static bool TryToSelectIndoorTilePrefix(BehaviorJanitor __instance, ref bool __result)
        //{
        //    return true;
        //}

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(BehaviorJanitor), "SelectNextAction")]
        //public static bool SelectNextActionPrefix(BehaviorJanitor __instance)
        //{
        //    if (!ViewSettingsPatch.m_enabled)
        //    {
        //        // Allow original method to run
        //        return true;
        //    }

        //    EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();
        //    GameDBRoomType homeRoomType = employeeComponent?.GetHomeRoomType();

        //    if (employeeComponent.m_state.m_homeRoom == null)
        //    {
        //        BehaviorJanitorPatch.GoReturnCartPrefix(__instance);
        //        return false;
        //    }

        //    if (employeeComponent.IsFired() || __instance.GetDepartment().IsClosed())
        //    {
        //        BehaviorJanitorPatch.GoReturnCartPrefix(__instance);
        //        return false;
        //    }

        //    if (ViewSettingsPatch.m_enabledTrainingDepartment && (homeRoomType != null) && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
        //    {
        //        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to workplace");

        //        BehaviorJanitorPatch.GoReturnCartPrefix(__instance);
        //        return false;
        //    }

        //    if (homeRoomType != null && homeRoomType.HasTag(Tags.Vanilla.JanitorAdminWorkplace))
        //    {
        //        BehaviorJanitorPatch.GoReturnCartPrefix(__instance);
        //        return false;
        //    }

        //    if (__instance.m_state.m_object != null)
        //    {
        //        __instance.m_state.m_object.GetEntity().Repair();
        //        BehaviorJanitorPatch.FreeObjectMod(__instance);
        //    }
        //    else
        //    {
        //        PlayerStatistics.Instance.IncrementStatistic("STAT_TILES_CLEAN", 1);
        //        MapScriptInterface.Instance.CleanTile(__instance.GetComponent<WalkComponent>().GetCurrentTile(), __instance.GetComponent<WalkComponent>().GetFloorIndex());
        //    }

        //    if (DayTime.Instance.GetShift() != employeeComponent.m_state.m_shift)
        //    {
        //        BehaviorJanitorPatch.FreeObjectMod(__instance);
        //        BehaviorJanitorPatch.FreeRoomMod(__instance);
        //        BehaviorJanitorPatch.FreeTileMod(__instance);
        //        BehaviorJanitorPatch.GoReturnCartPrefix(__instance);

        //        __instance.m_state.m_hadLunch = false;
        //        __instance.m_state.m_hadBreak = false;
        //    }
        //    else
        //    {
        //        BehaviorJanitorPatch.FreeTileMod(__instance);

        //        if (BehaviorJanitorPatch.CheckNeedsMod(__instance))
        //        {
        //            //__instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
        //            //__instance.SwitchState(BehaviorJanitorState.FulfillingNeeds);

        //            BehaviorJanitorPatch.FreeObjectMod(__instance);
        //            BehaviorJanitorPatch.FreeRoomMod(__instance);
        //            BehaviorJanitorPatch.FreeTileMod(__instance);
        //            BehaviorJanitorPatch.GoReturnCartPrefix(__instance);

        //            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, fulfilling needs");
        //        }
        //        else if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(__instance.GetComponent<ProcedureComponent>()))
        //        {
        //            //__instance.SwitchState(BehaviorJanitorState.Training);

        //            BehaviorJanitorPatch.FreeObjectMod(__instance);
        //            BehaviorJanitorPatch.FreeRoomMod(__instance);
        //            BehaviorJanitorPatch.FreeTileMod(__instance);
        //            BehaviorJanitorPatch.GoReturnCartPrefix(__instance);

        //            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, training");
        //        }

        //        if (__instance.m_state.m_cart == null)
        //        {
        //            BehaviorJanitorPatch.FindCartMod(__instance);
        //        }

        //        if (!BehaviorJanitorPatch.TryToSelectTileInCurrentRoomMod(__instance))
        //        {
        //            if (!BehaviorJanitorPatch.TryToSelectTileInARoomMod(__instance))
        //            {
        //                if (!BehaviorJanitorPatch.TryToSelectIndoorTileMod(__instance, 10))
        //                {
        //                    BehaviorJanitorPatch.FreeObjectMod(__instance);
        //                    BehaviorJanitorPatch.FreeRoomMod(__instance);
        //                    BehaviorJanitorPatch.FreeTileMod(__instance);
        //                    BehaviorJanitorPatch.GoReturnCartPrefix(__instance);
        //                }
        //            }
        //        }
        //    }

        //    return false;
        //}

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

            if (employeeComponent.m_state.m_shift == Shift.NIGHT)
            {
                __instance.m_state.m_hadLunch = true;
            }

            // janitors don't have commuting state as doctors, nurses and lab specialists
            // we just keep them at home longer time

            if (employeeComponent.ShouldStartCommuting() && (!__instance.GetDepartment().IsClosed()))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, trying to find common room");

                var commonRoom = BehaviorJanitorPatch.GetCommonRoom(__instance);

                if (commonRoom == null)
                {
                    // not found common room, stay at home
                    return false;
                }

                __instance.GetComponent<WalkComponent>().SetDestination(commonRoom.GetDefaultUsePosition(), commonRoom.GetFloorIndex(), MovementType.WALKING);
                __instance.SwitchState(BehaviorJanitorState.GoingToWorkplace);

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
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

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, arrived to common room");

                employeeComponent.CheckChiefNodiagnoseDepartment(true);
                employeeComponent.CheckRoomSatisfactionBonuses();
                employeeComponent.CheckMoodModifiers(__instance.IsBookmarked());
                employeeComponent.CheckBossModifiers();
                employeeComponent.UpdateHomeRoom();
                __instance.GetComponent<AnimModelComponent>().RevertToDefaultClothes(false);

                __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);
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
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, fulfilling needs");

                // check if janitor has to go home
                if (BehaviorJanitorPatch.GoHomeMod(__instance))
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going home");
                    return false;
                }

                // check if janitor needs to fulfill his/her needs
                if (BehaviorJanitorPatch.CheckNeedsMod(__instance))
                {
                    __instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                    __instance.SwitchState(BehaviorJanitorState.FulfillingNeeds);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, fulfilling needs");
                    return false;
                }

                if (homeRoomType != null && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
                {
                    // it is janitor in training
                    // set him/her training flags

                    if (!employeeComponent.ShouldGoToTraining())
                    {
                        BehaviorJanitorPatch.ChooseSkillToTrainAndToggleTraining(__instance);
                    }

                    // if possible, go to training
                    if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(__instance.GetComponent<ProcedureComponent>()))
                    {
                        __instance.SwitchState(BehaviorJanitorState.Training);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, training");
                        return false;
                    }
                }

                // by default, go to common room
                var commonRoom = BehaviorJanitorPatch.GetCommonRoom(__instance);

                if (commonRoom != null)
                {
                    __instance.GetComponent<WalkComponent>().SetDestination(commonRoom.GetDefaultUsePosition(), commonRoom.GetFloorIndex(), MovementType.WALKING);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");

                    __instance.SwitchState(BehaviorJanitorState.GoingToWorkplace);
                }
                else
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, common room room not found");
                }
            }

            return false;
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

            if ((!__instance.GetComponent<ProcedureComponent>().IsBusy()) && (!__instance.GetComponent<WalkComponent>().IsBusy()))
            {
                // currently janitor is not doing anything

                // check if janitor has to go home
                if (BehaviorJanitorPatch.GoHomeMod(__instance))
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going home");
                    return false;
                }

                // check if janitor needs to fulfill his/her needs
                if (BehaviorJanitorPatch.CheckNeedsMod(__instance))
                {
                    __instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                    __instance.SwitchState(BehaviorJanitorState.FulfillingNeeds);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, fulfilling needs");
                    return false;
                }

                if (homeRoomType != null && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
                {
                    // it is janitor in training
                    // set him/her training flags

                    if (!employeeComponent.ShouldGoToTraining())
                    {
                        BehaviorJanitorPatch.ChooseSkillToTrainAndToggleTraining(__instance);
                    }

                    // if possible, go to training
                    if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(__instance.GetComponent<ProcedureComponent>()))
                    {
                        __instance.SwitchState(BehaviorJanitorState.Training);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, training");
                        return false;
                    }
                }

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

                    var commonRoom = BehaviorJanitorPatch.GetCommonRoom(__instance);

                    if (commonRoom != null)
                    {
                        __instance.GetComponent<WalkComponent>().SetDestination(commonRoom.GetDefaultUsePosition(), commonRoom.GetFloorIndex(), MovementType.WALKING);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");
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

            if (homeRoomType != null && homeRoomType.HasTag(Tags.Mod.JanitorTrainingWorkspace))
            {
                if (employeeComponent.UpdateTraining(__instance.GetComponent<ProcedureComponent>()))
                {
                    // training was finished

                    // check if janitor has to go home
                    if (BehaviorJanitorPatch.GoHomeMod(__instance))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going home");
                        return false;
                    }

                    // check if janitor needs to fulfill his/her needs
                    if (BehaviorJanitorPatch.CheckNeedsMod(__instance))
                    {
                        __instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                        __instance.SwitchState(BehaviorJanitorState.FulfillingNeeds);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, fulfilling needs");
                        return false;
                    }

                    if (!employeeComponent.ShouldGoToTraining())
                    {
                        BehaviorJanitorPatch.ChooseSkillToTrainAndToggleTraining(__instance);
                    }

                    // if possible, go to training
                    if (employeeComponent.ShouldGoToTraining() && employeeComponent.GoToTraining(__instance.GetComponent<ProcedureComponent>()))
                    {
                        __instance.SwitchState(BehaviorJanitorState.Training);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, training");
                        return false;
                    }

                    // default case, training is not possible more
                    __instance.SwitchState(BehaviorJanitorState.FillingFreeTime);

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, filling free time");
                    return false;
                }

                return false;
            }

            // missing janitor admin training

            // missing reular janitor training

            return true;
        }

        public static void ChooseSkillToTrainAndToggleTraining(BehaviorJanitor instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            List<Skill> skills = new List<Skill>();

            if (employeeComponent.m_state.m_skillSet.m_qualifications != null)
            {
                foreach (var skill in employeeComponent.m_state.m_skillSet.m_qualifications)
                {
                    if (skill.m_level < Skills.SkillLevelMaximum)
                    {
                        skills.Add(skill);
                    }
                }
            }
            if (employeeComponent.m_state.m_skillSet.m_specialization1 != null)
            {
                if (employeeComponent.m_state.m_skillSet.m_specialization1.m_level < Skills.SkillLevelMaximum)
                {
                    skills.Add(employeeComponent.m_state.m_skillSet.m_specialization1);
                }
            }
            if (employeeComponent.m_state.m_skillSet.m_specialization2 != null)
            {
                if (employeeComponent.m_state.m_skillSet.m_specialization2.m_level < Skills.SkillLevelMaximum)
                {
                    skills.Add(employeeComponent.m_state.m_skillSet.m_specialization2);
                }
            }

            if (skills.Count != 0)
            {
                Skill skillToTrain = skills[UnityEngine.Random.Range(0, skills.Count)];

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {instance.m_entity.Name}, training skill {skillToTrain.m_gameDBSkill.Entry.DatabaseID}");
                employeeComponent.ToggleTraining(skillToTrain);
            }
        }

        private static bool CheckNeedsMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("CheckNeeds", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static bool FindCartMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("FindCart", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static bool FreeObjectMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("FreeObject", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static bool FreeRoomMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("FreeRoom", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static bool FreeTileMod(BehaviorJanitor instance)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("FreeTile", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static bool GoHomeMod(BehaviorJanitor instance)
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

        private static bool TryToSelectIndoorTileMod(BehaviorJanitor instance, int threshold)
        {
            Type type = typeof(BehaviorJanitor);
            MethodInfo methodInfo = type.GetMethod("TryToSelectIndoorTile", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, new object[] { threshold });
        }

        private static TileObject GetCommonRoom(BehaviorJanitor instance)
        {
            var result = MapScriptInterface.Instance.FindClosestObjectWithTag(
                instance.GetComponent<WalkComponent>().GetCurrentTile(),
                instance.GetComponent<WalkComponent>().GetFloorIndex(),
                instance.GetDepartment(),
                Tags.Vanilla.Rest,
                AccessRights.STAFF,
                new string[] { Tags.Vanilla.CommonRoom },
                false,
                false,
                false,
                false);

            if (result != null)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"02: Employee: {instance.m_entity.Name} found common room in {instance.GetDepartment()?.m_departmentPersistentData.m_departmentType.m_id.ToString() ?? "NULL"}");

                return result;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"03: Employee: {instance.m_entity.Name} not found common room in {instance.GetDepartment()?.m_departmentPersistentData.m_departmentType.m_id.ToString() ?? "NULL"}");

            foreach (var department in Hospital.Instance.m_departments)
            {
                result = MapScriptInterface.Instance.FindClosestObjectWithTag(
                    instance.GetComponent<WalkComponent>().GetCurrentTile(),
                    instance.GetComponent<WalkComponent>().GetFloorIndex(),
                    department,
                    Tags.Vanilla.Rest,
                    AccessRights.STAFF,
                    new string[] { Tags.Vanilla.CommonRoom },
                    false,
                    false,
                    false,
                    false);

                if (result != null)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"04: Employee: {instance.m_entity.Name} found common room in {department.m_departmentPersistentData.m_departmentType.m_id.ToString() ?? "NULL"}");

                    break;
                }

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"05: Employee: {instance.m_entity.Name} not found common room in {department.m_departmentPersistentData.m_departmentType.m_id.ToString() ?? "NULL"}");
            }

            if (result != null)
            {
                return result;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"06: Employee: {instance.m_entity.Name} not found common room");

            return null;
        }
    }
}
