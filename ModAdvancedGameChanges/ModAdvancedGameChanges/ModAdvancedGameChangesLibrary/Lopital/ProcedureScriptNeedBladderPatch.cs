using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Linq;
using System.Reflection;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptNeedBladder))]
    public static class ProcedureScriptNeedBladderPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptNeedBladder), nameof(ProcedureScriptNeedBladder.Activate))]
        public static bool ActivatePrefix(ProcedureScriptNeedBladder __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, activating script {__instance.m_stateData.m_scriptName}");

            __instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_SEARCHING_ROOM);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptNeedBladder), nameof(ProcedureScriptNeedBladder.ScriptUpdate))]
        public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptNeedBladder __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            __instance.m_stateData.m_timeInState += deltaTime;
            switch (__instance.m_stateData.m_state)
            {
                case ProcedureScriptNeedBladderPatch.STATE_SEARCHING_ROOM:
                    ProcedureScriptNeedBladderPatch.UpdateStateSearchingRoom(__instance);
                    break;
                case ProcedureScriptNeedBladderPatch.STATE_GOING_TO_ROOM:
                    ProcedureScriptNeedBladderPatch.UpdateStateGoingToRoom(__instance);
                    break;
                case ProcedureScriptNeedBladderPatch.STATE_GOING_TO_WC:
                    ProcedureScriptNeedBladderPatch.UpdateStateGoingToWC(__instance);
                    break;
                case ProcedureScriptNeedBladderPatch.STATE_USING_WC:
                    ProcedureScriptNeedBladderPatch.UpdateStateUsingWC(__instance);
                    break;
                case ProcedureScriptNeedBladderPatch.STATE_GOING_TO_SINK:
                    ProcedureScriptNeedBladderPatch.UpdateStateGoingToSink(__instance);
                    break;
                case ProcedureScriptNeedBladderPatch.STATE_USING_SINK:
                    ProcedureScriptNeedBladderPatch.UpdateStateUsingSink(__instance);
                    break;
                case ProcedureScriptNeedBladderPatch.STATE_GOING_TO_DRYER:
                    ProcedureScriptNeedBladderPatch.UpdateStateGoingToDryer(__instance);
                    break;
                case ProcedureScriptNeedBladderPatch.STATE_USING_DRYER:
                    ProcedureScriptNeedBladderPatch.UpdateStateUsingDryer(__instance);
                    break;
                default:
                    break;
            }

            return false;
        }

        public static void UpdateStateSearchingRoom(ProcedureScriptNeedBladder instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            EmployeeComponent employee = mainCharacter.GetComponent<EmployeeComponent>();
            BehaviorPatient patient = mainCharacter.GetComponent<BehaviorPatient>();

            Department department = (employee != null) ? employee.m_state.m_department.GetEntity() : patient.GetDepartment();

            if (department != null)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, department {department.m_departmentPersistentData.m_departmentType.Entry.DatabaseID}");

                GameDBProcedure procedure = (mainCharacter.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().m_stateData.m_procedure.Entry != null)
                    ? mainCharacter.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().m_stateData.m_procedure.Entry
                    : null;

                TileObject result = MapScriptInterface.Instance.FindClosestFreeObjectWithTagsAndRoomTags(
                    mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(),
                    mainCharacter.GetComponent<WalkComponent>().GetFloorIndex(),
                    department,
                    procedure?.RequiredEquipment?.Select(eq => eq.Tag).ToArray() ?? new string[] { },
                    (patient != null) ? AccessRights.PATIENT : AccessRights.STAFF_ONLY,
                    procedure?.RequiredRoomTags ?? new string[] { });

                if (result == null)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, no free toilet in department");

                    foreach (var dpt in Hospital.Instance.m_departments)
                    {
                        if (!dpt.IsClosed())
                        {
                            TileObject toilet = MapScriptInterface.Instance.FindClosestFreeObjectWithTagsAndRoomTags(
                                mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(),
                                mainCharacter.GetComponent<WalkComponent>().GetFloorIndex(),
                                dpt,
                                procedure?.RequiredEquipment?.Select(eq => eq.Tag).ToArray() ?? new string[] { },
                                (patient != null) ? AccessRights.PATIENT : AccessRights.STAFF_ONLY,
                                procedure?.RequiredRoomTags ?? new string[] { });

                            if (toilet != null)
                            {
                                Vector3i toiletPosition = new Vector3i(toilet.GetDefaultUseTile().m_x, toilet.GetDefaultUseTile().m_y, toilet.GetFloorIndex());
                                Vector3i resultPosition = (result == null) ? toiletPosition : new Vector3i(result.GetDefaultUseTile().m_x, result.GetDefaultUseTile().m_y, result.GetFloorIndex());
                                Vector3i currentPosition = new Vector3i(mainCharacter.GetComponent<WalkComponent>().GetCurrentTile().m_x, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile().m_y, mainCharacter.GetComponent<WalkComponent>().GetFloorIndex());

                                if ((currentPosition - toiletPosition).LengthSquaredWithPenalty() <= (currentPosition - resultPosition).LengthSquaredWithPenalty())
                                {
                                    result = toilet;
                                }
                            }
                        }
                    }
                }

                if (result == null)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, no free toilet in hospital");
                }
                else
                {
                    // do not reserve object (wc), just go to room
                    Room room = MapScriptInterface.Instance.GetRoomAt(result);
                    Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(room, (patient != null) ? AccessRights.PATIENT : AccessRights.STAFF_ONLY);

                    instance.MoveCharacter(mainCharacter, position, room.GetFloorIndex());
                    instance.SpeakCharacter(mainCharacter, null, Speeches.Vanilla.WC, 4f);

                    instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_GOING_TO_ROOM);
                }
            }
            else
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, no department");

                instance.SwitchState(ProcedureScriptNeedBladder.STATE_IDLE);
            }
        }

        public static void UpdateStateGoingToRoom(ProcedureScriptNeedBladder instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (mainCharacter.GetComponent<WalkComponent>().m_state.m_walkState == WalkState.NoPath)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, no path");

                    instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_SEARCHING_ROOM);
                }
                else
                {
                    // get current room
                    Room room = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), mainCharacter.GetComponent<WalkComponent>().GetFloorIndex());

                    if (room == null)
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, no room");

                        instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_SEARCHING_ROOM);
                    }
                    else
                    {
                        BehaviorPatient patient = mainCharacter.GetComponent<BehaviorPatient>();

                        GameDBProcedure procedure = (mainCharacter.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().m_stateData.m_procedure.Entry != null)
                            ? mainCharacter.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().m_stateData.m_procedure.Entry
                            : null;

                        // find free toilet
                        instance.m_stateData.m_procedureScene.m_equipment[0] = MapScriptInterface.Instance.FindClosestFreeObjectWithTags(
                            mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(),
                            room, 
                            procedure?.RequiredEquipment?.Select(eq => eq.Tag).ToArray() ?? new string[] { }, 
                            (patient != null) ? AccessRights.PATIENT : AccessRights.STAFF_ONLY,
                            false, null, false);

                        if (instance.m_stateData.m_procedureScene.m_equipment[0] == null)
                        {
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, no free toilet");

                            instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_SEARCHING_ROOM);
                        }
                        else
                        {
                            // reserve toilet
                            mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));

                            instance.MoveCharacter(mainCharacter, instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex());
                            instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_GOING_TO_WC);
                        }
                    }
                }
            }
        }

        public static void UpdateStateGoingToWC(ProcedureScriptNeedBladder instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_USING_WC);
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
            }
        }

        public static void UpdateStateUsingWC(ProcedureScriptNeedBladder instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.BLADDER_REDUCED, Tweakable.Mod.NeedBladder()));

                if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.Spartan))
                {
                    if (instance.MainCharacterBookmarkedInternal())
                    {
                        mainCharacter.GetComponent<PerkComponent>().RevealPerk(Perks.Vanilla.Spartan, true);
                    }
                    else
                    {
                        mainCharacter.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Perks.Vanilla.Spartan);
                    }
                }

                // free reserved object
                instance.GetEquipment(0).User = null;

                // try to find sink
                Room room = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), mainCharacter.GetComponent<WalkComponent>().GetFloorIndex());
                BehaviorPatient patient = mainCharacter.GetComponent<BehaviorPatient>();

                TileObject sink = MapScriptInterface.Instance.FindClosestFreeObjectWithTags(
                    mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(),
                    room,
                    new string[] { Tags.Vanilla.Washing },
                    (patient != null) ? AccessRights.PATIENT : AccessRights.STAFF_ONLY,
                    false, null, false);

                if (sink != null)
                {
                    // reserve sink
                    instance.m_stateData.m_procedureScene.m_equipment[0] = sink;
                    mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));

                    instance.SetParam(ProcedureScriptNeedBladderPatch.PARAM_WASHING_CYCLES, 1f);

                    if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.Germaphobe))
                    {
                        if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.Germaphobe))
                        {
                            if (instance.MainCharacterBookmarkedInternal())
                            {
                                mainCharacter.GetComponent<PerkComponent>().RevealPerk(Perks.Vanilla.Germaphobe, true);
                            }
                            else
                            {
                                mainCharacter.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Perks.Vanilla.Germaphobe);
                            }
                        }

                        instance.SetParam(ProcedureScriptNeedBladderPatch.PARAM_WASHING_CYCLES, 3f);
                    }
                    
                    instance.MoveCharacter(mainCharacter, instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex());
                    instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_GOING_TO_SINK);
                }
                else
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, no free sink");

                    mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.CouldNotWashHands);
                    instance.SwitchState(ProcedureScriptNeedBladder.STATE_IDLE);
                }
            }
        }

        public static void UpdateStateGoingToSink(ProcedureScriptNeedBladder instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_USING_SINK);
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
                instance.GetEquipment(0).GetComponent<AnimatedObjectComponent>().ForceFrame(1);
            }
        }

        public static void UpdateStateUsingSink(ProcedureScriptNeedBladder instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if ((instance.m_stateData.m_timeInState > 1f) && (instance.GetEquipment(0) != null))
            {
                instance.GetEquipment(0).GetComponent<AnimatedObjectComponent>().ForceFrame(0);
            }

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                // free reserved object
                instance.GetEquipment(0).User = null;

                if (mainCharacter.GetComponent<MoodComponent>().HasSatisfactionModifier(SatisfationModifiers.Vanilla.CouldNotWashHands))
                {
                    mainCharacter.GetComponent<MoodComponent>().RemoveSatisfactionModifier(SatisfationModifiers.Vanilla.CouldNotWashHands);
                }

                // check washing cycles
                instance.SetParam(ProcedureScriptNeedBladderPatch.PARAM_WASHING_CYCLES, instance.GetParam(ProcedureScriptNeedBladderPatch.PARAM_WASHING_CYCLES) - 1);

                if (instance.GetParam(ProcedureScriptNeedBladderPatch.PARAM_WASHING_CYCLES) > 0f)
                {
                    // germaphobe, wash hands again
                    mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));
                    instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_GOING_TO_SINK);
                }
                else
                {
                    // try to find dryer
                    Room room = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), mainCharacter.GetComponent<WalkComponent>().GetFloorIndex());
                    BehaviorPatient patient = mainCharacter.GetComponent<BehaviorPatient>();

                    TileObject dryer = MapScriptInterface.Instance.FindClosestFreeObjectWithTags(
                        mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(),
                        room,
                        new string[] { Tags.Vanilla.Dryer },
                        (patient != null) ? AccessRights.PATIENT : AccessRights.STAFF_ONLY,
                        false, null, false);

                    if (dryer != null)
                    {
                        // reserve dryer
                        instance.m_stateData.m_procedureScene.m_equipment[0] = dryer;
                        mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));

                        instance.MoveCharacter(mainCharacter, instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex());
                        instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_GOING_TO_DRYER);
                    }
                    else
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, no free dryer");

                        mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.CouldNotDryHands);
                        instance.SwitchState(ProcedureScriptNeedBladder.STATE_IDLE);
                    }
                }
            }
        }

        public static void UpdateStateGoingToDryer(ProcedureScriptNeedBladder instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                instance.SwitchState(ProcedureScriptNeedBladderPatch.STATE_USING_DRYER);
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
            }
        }

        public static void UpdateStateUsingDryer(ProcedureScriptNeedBladder instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                // free reserved object
                instance.GetEquipment(0).User = null;

                if (mainCharacter.GetComponent<MoodComponent>().HasSatisfactionModifier(SatisfationModifiers.Vanilla.CouldNotDryHands))
                {
                    mainCharacter.GetComponent<MoodComponent>().RemoveSatisfactionModifier(SatisfationModifiers.Vanilla.CouldNotDryHands);
                }

                instance.SwitchState(ProcedureScriptNeedBladder.STATE_IDLE);
            }
        }

        private static bool MainCharacterBookmarkedInternal(this ProcedureScriptNeedBladder instance)
        {
            Type type = typeof(ProcedureScriptNeedBladder);
            MethodInfo methodInfo = type.GetMethod("MainCharacterBookmarked", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        public const string STATE_SEARCHING_ROOM = "STATE_SEARCHING_ROOM";
        public const string STATE_GOING_TO_ROOM = "STATE_GOING_TO_ROOM";
        public const string STATE_GOING_TO_WC = "STATE_GOING_TO_WC";
        public const string STATE_USING_WC = "STATE_USING_WC";
        public const string STATE_GOING_TO_SINK = "STATE_GOING_TO_SINK";
        public const string STATE_USING_SINK = "STATE_USING_SINK";
        public const string STATE_GOING_TO_DRYER = "STATE_GOING_TO_DRYER";
        public const string STATE_USING_DRYER = "STATE_USING_DRYER";

        private const int PARAM_WASHING_CYCLES = 0;
    }
}
