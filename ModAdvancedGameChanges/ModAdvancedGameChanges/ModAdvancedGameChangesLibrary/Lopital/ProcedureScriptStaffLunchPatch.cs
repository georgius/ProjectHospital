using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using UnityEngine;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptStaffLunch))]
    public static class ProcedureScriptStaffLunchPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptStaffLunch), nameof(ProcedureScriptStaffLunch.Activate))]
        public static bool ActivatePrefix(ProcedureScriptStaffLunch __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, activating script {__instance.m_stateData.m_scriptName}");

            __instance.SetParam(ProcedureScriptStaffLunchPatch.PARAM_EATING_TIME, DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Mod.NeedHungerLunchEatingTimeMinutes() / 60f));
            __instance.SetParam(ProcedureScriptStaffLunchPatch.PARAM_COFFEE_TIME, DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Mod.NeedHungerLunchCoffeeTimeMinutes() / 60f));
            __instance.SetParam(ProcedureScriptStaffLunchPatch.PARAM_PAY, 0f);

            if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.Hedonist))
            {
                __instance.SetParam(ProcedureScriptStaffLunchPatch.PARAM_EATING_TIME, __instance.GetParam(ProcedureScriptStaffLunchPatch.PARAM_EATING_TIME) * 2);
            }

            __instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_SEARCHING_ROOM);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptStaffLunch), nameof(ProcedureScriptStaffLunch.ScriptUpdate))]
        public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptStaffLunch __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __instance.m_stateData.m_timeInState += deltaTime;
            switch (__instance.m_stateData.m_state)
            {
                case ProcedureScriptStaffLunchPatch.STATE_SEARCHING_ROOM:
                    ProcedureScriptStaffLunchPatch.UpdateStateSearchingRoom(__instance);
                    break;

                // common room

                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_GOING_TO_ROOM:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomGoingToRoom(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_GOING_TO_FRIDGE:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomGoingToFridge(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_USING_FRIDGE:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomUsingFridge(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_GOING_TO_CHAIR:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomGoingToChair(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_EATING:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomEating(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_EATING_FINISHING:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomEatingFinishing(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_EATING_STANDING:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomEatingStanding(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_EATING_FINISHED:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomEatingFinished(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_GOING_TO_COFFEE_MAKER:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomGoingToCoffeeMaker(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_USING_COFFEE_MAKER:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomUsingCoffeeMaker(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_GOING_TO_SOFA:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonRoomGoingToSofa(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_DRINKING_COFFEE:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonDrinkingCoffee(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_COFFEE_FINISHED:
                    ProcedureScriptStaffLunchPatch.UpdateStateCommonCoffeeFinished(__instance);
                    break;

                // cafeteria

                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_ROOM:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaGoingToRoom(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_BOARD:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaGoingToBoard(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_USING_BOARD:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaUsingBoard(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_MEAL_COUNTER:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaGoingToMealCounter(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_USING_MEAL_COUNTER:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaUsingMealCounter(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_CHAIR:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaGoingToChair(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_EATING:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaEating(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_EATING_FINISHED:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaEatingFinished(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_SNACK:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaGoingToSnack(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_USING_SNACK:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaUsingSnack(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_FINISHED_SNACK:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaFinishedSnack(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_JUICE:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaGoingToJuice(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_USING_JUICE:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaUsingJuice(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_FINISHED_JUICE:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaFinishedJuice(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_COFFEE_MAKER:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaGoingToCoffeeMaker(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_USING_COFFEE_MAKER:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaUsingCoffeeMaker(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_SOFA:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaGoingToSofa(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_DRINKING_COFFEE:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaDrinkingCoffee(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_COFFEE_FINISHED:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaCoffeeFinished(__instance);
                    break;
                case ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_PAY:
                    ProcedureScriptStaffLunchPatch.UpdateStateCafeteriaPay(__instance);
                    break;
                default:
                    break;
            }

            return false;
        }

        public static void UpdateStateSearchingRoom(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            // there can be two options:
            // 1. lunch in common room
            // 2. lunch in cafeteria

            Room room = MapScriptInterface.Instance.GetRoomAt(instance.GetEquipment(0).GetDefaultUseTile(), instance.GetEquipment(0).GetFloorIndex());

            if ((room != null) && (room.m_roomPersistentData.m_roomType.Entry.HasTag(Tags.Vanilla.Cafeteria)))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, cafeteria");

                mainCharacter.GetComponent<WalkComponent>().SetDestination(instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_ROOM);
            }
            else
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, common room");

                mainCharacter.GetComponent<WalkComponent>().SetDestination(instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_GOING_TO_ROOM);
            }

            instance.SpeakCharacter(mainCharacter, null, Speeches.Vanilla.Food, 4f);
        }

        // common room

        public static void UpdateStateCommonRoomGoingToRoom(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (MapScriptInterfacePatch.IsInDestinationRoom(mainCharacter))
                {
                    mainCharacter.GetComponent<WalkComponent>().Stop();
                    mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
                }
            }
            else
            {
                if (instance.GetEquipment(0).User == null)
                {
                    mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));
                    instance.GetEquipment(0).User = mainCharacter;

                    instance.MoveCharacter(mainCharacter, instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex());
                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_GOING_TO_FRIDGE);
                }
            }
        }

        public static void UpdateStateCommonRoomGoingToFridge(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_USING_FRIDGE);
            }
        }

        public static void UpdateStateCommonRoomUsingFridge(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

                TileObject chair = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                    mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), currentRoom, 
                    Tags.Vanilla.Eating, AccessRights.STAFF_ONLY, false, null, false);

                if (chair != null)
                {
                    // free reserved object
                    instance.GetEquipment(0).User = null;

                    // reserve chair
                    instance.m_stateData.m_procedureScene.m_equipment[0] = chair;
                    instance.GetEquipment(0).User = mainCharacter;

                    mainCharacter.GetComponent<WalkComponent>().GoSit(chair, MovementType.WALKING);
                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_GOING_TO_CHAIR);
                }
                else
                {
                    mainCharacter.GetComponent<AnimModelComponent>().SetDirection(instance.GetEquipment(0).GetOppositeUseOrientation());
                    mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandTakeFromFridge, false);

                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_EATING_STANDING);
                }
            }
        }

        public static void UpdateStateCommonRoomGoingToChair(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitAndEatIn, false, true);
                mainCharacter.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitAndEatIdle, true, false);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_EATING);
            }
        }

        public static void UpdateStateCommonRoomEating(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (instance.m_stateData.m_timeInState > instance.GetParam(ProcedureScriptStaffLunchPatch.PARAM_EATING_TIME))
            {
                if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.Hedonist))
                {
                    mainCharacter.GetComponent<PerkComponent>().RevealPerk(Perks.Vanilla.Hedonist, mainCharacter.GetComponent<Behavior>().IsBookmarked());
                }

                mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.HUNGER_REDUCED, Tweakable.Mod.NeedHungerLunch()));
                mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.HadLunch);
                mainCharacter.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitAndEatOut, false, true);

                if (mainCharacter.GetComponent<BehaviorDoctor>() != null)
                {
                    mainCharacter.GetComponent<BehaviorDoctor>().m_state.m_hadLunch = true;
                }

                if (mainCharacter.GetComponent<BehaviorNurse>() != null)
                {
                    mainCharacter.GetComponent<BehaviorNurse>().m_state.m_hadLunch = true;
                }

                if (mainCharacter.GetComponent<BehaviorLabSpecialist>() != null)
                {
                    mainCharacter.GetComponent<BehaviorLabSpecialist>().m_state.m_hadLunch = true;
                }

                if (mainCharacter.GetComponent<BehaviorJanitor>() != null)
                {
                    mainCharacter.GetComponent<BehaviorJanitor>().m_state.m_hadLunch = true;
                }

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_EATING_FINISHING);
            }
        }

        public static void UpdateStateCommonRoomEatingFinishing(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (mainCharacter.GetComponent<AnimModelComponent>().IsIdle())
            {
                mainCharacter.GetComponent<WalkComponent>().StandUp();

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_EATING_FINISHED);
            }
        }

        public static void UpdateStateCommonRoomEatingStanding(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (mainCharacter.GetComponent<AnimModelComponent>().IsIdle())
            {
                // free reserved object (fridge)
                instance.GetEquipment(0).User = null;

                instance.m_stateData.m_procedureScene.m_equipment[0] = null;

                mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.HUNGER_REDUCED, 100f));
                mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.HadLunchStanding);

                if (mainCharacter.GetComponent<BehaviorDoctor>() != null)
                {
                    mainCharacter.GetComponent<BehaviorDoctor>().m_state.m_hadLunch = true;
                }

                if (mainCharacter.GetComponent<BehaviorNurse>() != null)
                {
                    mainCharacter.GetComponent<BehaviorNurse>().m_state.m_hadLunch = true;
                }

                if (mainCharacter.GetComponent<BehaviorLabSpecialist>() != null)
                {
                    mainCharacter.GetComponent<BehaviorLabSpecialist>().m_state.m_hadLunch = true;
                }

                if (mainCharacter.GetComponent<BehaviorJanitor>() != null)
                {
                    mainCharacter.GetComponent<BehaviorJanitor>().m_state.m_hadLunch = true;
                }

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_EATING_FINISHED);
            }
        }

        public static void UpdateStateCommonRoomEatingFinished(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (mainCharacter.GetComponent<AnimModelComponent>().IsIdle() && (!mainCharacter.GetComponent<WalkComponent>().IsBusy()))
            {
                // free reserved object (chair), if necessary
                if (instance.GetEquipment(0) != null)
                {
                    instance.GetEquipment(0).User = null;
                }

                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

                TileObject coffeeMachine = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                    mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), currentRoom, 
                    Tags.Vanilla.CoffeeMachine, AccessRights.STAFF_ONLY, false, null, false);

                if (coffeeMachine != null)
                {
                    instance.m_stateData.m_procedureScene.m_equipment[0] = coffeeMachine;

                    mainCharacter.GetComponent<UseComponent>().ReserveObject(coffeeMachine);
                    mainCharacter.GetComponent<WalkComponent>().SetDestination(coffeeMachine.GetDefaultUsePosition(), coffeeMachine.GetFloorIndex(), MovementType.WALKING);

                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_GOING_TO_COFFEE_MAKER);
                }
                else
                {
                    instance.SwitchState(ProcedureScriptStaffLunch.STATE_IDLE);
                }
            }
        }

        public static void UpdateStateCommonRoomGoingToCoffeeMaker(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_USING_COFFEE_MAKER);
            }
        }

        public static void UpdateStateCommonRoomUsingCoffeeMaker(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                // free reserved object (coffee maker)
                instance.GetEquipment(0).User = null;

                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

                TileObject restObject = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                    mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), currentRoom,
                    Tags.Vanilla.Rest, AccessRights.STAFF_ONLY, false, null, false);

                if (restObject != null)
                {
                    // reserve rest object (sofa)
                    instance.m_stateData.m_procedureScene.m_equipment[0] = restObject;
                    instance.GetEquipment(0).User = mainCharacter;

                    mainCharacter.GetComponent<UseComponent>().ReserveObject(restObject);
                    mainCharacter.GetComponent<WalkComponent>().GoSit(restObject, MovementType.WALKING);

                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_GOING_TO_SOFA);
                }
                else
                {
                    instance.SwitchState(ProcedureScriptStaffLunch.STATE_IDLE);
                }
            }
        }

        public static void UpdateStateCommonRoomGoingToSofa(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitIdleHoldPhone, false, true);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_DRINKING_COFFEE);
            }
        }

        public static void UpdateStateCommonDrinkingCoffee(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (instance.m_stateData.m_timeInState > instance.GetParam(ProcedureScriptStaffLunchPatch.PARAM_COFFEE_TIME))
            {
                if (mainCharacter.GetComponent<AnimModelComponent>().IsIdle())
                {
                    mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.REST_REDUCED, Tweakable.Mod.NeedRestPlayGame()));
                    mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.HadCoffee);

                    mainCharacter.GetComponent<WalkComponent>().StandUp();

                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_COMMON_ROOM_COFFEE_FINISHED);
                }
            }
            else
            {
                if (mainCharacter.GetComponent<AnimModelComponent>().IsIdle())
                {
                    mainCharacter.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitIdleHoldPhone, false, true);
                }
            }
        }

        public static void UpdateStateCommonCoffeeFinished(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                // free reserved object (sofa)
                instance.GetEquipment(0).User = null;

                instance.SwitchState(ProcedureScriptStaffLunch.STATE_IDLE);
            }
        }

        // cafeteria

        public static void UpdateStateCafeteriaGoingToRoom(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (MapScriptInterfacePatch.IsInDestinationRoom(mainCharacter))
                {
                    mainCharacter.GetComponent<WalkComponent>().Stop();
                    mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
                }
            }
            else
            {
                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

                TileObject cafeteriaBoard = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                    mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), currentRoom,
                    Tags.Vanilla.CafeteriaBoard, AccessRights.STAFF_ONLY, false, null, false);

                if (cafeteriaBoard != null)
                {
                    // reserve cafeteria board
                    instance.m_stateData.m_procedureScene.m_equipment[0] = cafeteriaBoard;
                    instance.GetEquipment(0).User = mainCharacter;

                    mainCharacter.GetComponent<UseComponent>().ReserveObject(cafeteriaBoard);
                    mainCharacter.GetComponent<WalkComponent>().SetDestination(instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);

                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_BOARD);
                }
            }
        }

        public static void UpdateStateCafeteriaGoingToBoard(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_USING_BOARD);
            }
        }

        public static void UpdateStateCafeteriaUsingBoard(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

                TileObject mealCounter = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                    mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), currentRoom,
                    Tags.Vanilla.MealCounter, AccessRights.STAFF_ONLY, false, null, false);

                if (mealCounter != null)
                {
                    // free reserved object (cafeteria board)
                    instance.GetEquipment(0).User = null;

                    // reserve meal counter
                    instance.m_stateData.m_procedureScene.m_equipment[0] = mealCounter;
                    instance.GetEquipment(0).User = mainCharacter;

                    mainCharacter.GetComponent<UseComponent>().ReserveObject(mealCounter);
                    mainCharacter.GetComponent<WalkComponent>().SetDestination(instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);

                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_MEAL_COUNTER);
                }
            }
        }

        public static void UpdateStateCafeteriaGoingToMealCounter(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_USING_MEAL_COUNTER);
            }
        }

        public static void UpdateStateCafeteriaUsingMealCounter(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                // free reserved object (meal counter)
                instance.GetEquipment(0).User = null;

                instance.SetParam(
                    ProcedureScriptStaffLunchPatch.PARAM_PAY,
                    instance.GetParam(ProcedureScriptStaffLunchPatch.PARAM_PAY) + UnityEngine.Random.Range(Tweakable.Mod.NeedHungerLunchPaymentMealMinimum(), Tweakable.Mod.NeedHungerLunchPaymentMealMaximum()));

                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

                TileObject chair = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                    mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), currentRoom,
                    Tags.Vanilla.Sitting, AccessRights.STAFF_ONLY, false, null, false);

                if (chair != null)
                {
                    // reserve chair
                    instance.m_stateData.m_procedureScene.m_equipment[0] = chair;
                    instance.GetEquipment(0).User = mainCharacter;

                    mainCharacter.GetComponent<WalkComponent>().GoSit(chair, MovementType.WALKING);

                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_CHAIR);
                }
            }
        }

        public static void UpdateStateCafeteriaGoingToChair(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitAndEatIn, false, true);
                mainCharacter.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitAndEatIdle, true, false);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_EATING);
            }
        }

        public static void UpdateStateCafeteriaEating(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (instance.m_stateData.m_timeInState > instance.GetParam(ProcedureScriptStaffLunchPatch.PARAM_EATING_TIME))
            {
                if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.Hedonist))
                {
                    mainCharacter.GetComponent<PerkComponent>().RevealPerk(Perks.Vanilla.Hedonist, mainCharacter.GetComponent<Behavior>().IsBookmarked());
                }

                mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.HUNGER_REDUCED, Tweakable.Mod.NeedHungerLunch()));
                mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.HadLunch);
                mainCharacter.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitAndEatOut, false, true);

                if (mainCharacter.GetComponent<BehaviorDoctor>() != null)
                {
                    mainCharacter.GetComponent<BehaviorDoctor>().m_state.m_hadLunch = true;
                }

                if (mainCharacter.GetComponent<BehaviorNurse>() != null)
                {
                    mainCharacter.GetComponent<BehaviorNurse>().m_state.m_hadLunch = true;
                }

                if (mainCharacter.GetComponent<BehaviorLabSpecialist>() != null)
                {
                    mainCharacter.GetComponent<BehaviorLabSpecialist>().m_state.m_hadLunch = true;
                }

                if (mainCharacter.GetComponent<BehaviorJanitor>() != null)
                {
                    mainCharacter.GetComponent<BehaviorJanitor>().m_state.m_hadLunch = true;
                }

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_EATING_FINISHED);
            }
        }

        public static void UpdateStateCafeteriaEatingFinished(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (mainCharacter.GetComponent<AnimModelComponent>().IsIdle() && (!mainCharacter.GetComponent<WalkComponent>().IsBusy()))
            {
                // free reserved object (chair)
                instance.GetEquipment(0).User = null;

                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

                TileObject snack = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                    mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), currentRoom,
                    (UnityEngine.Random.Range(0f, 2f) <= 1f) ? Tags.Vanilla.FruitSnack : Tags.Vanilla.DonutSnack, AccessRights.STAFF_ONLY, false, null, false);

                if (snack != null)
                {
                    // reserve snack
                    instance.m_stateData.m_procedureScene.m_equipment[0] = snack;
                    instance.GetEquipment(0).User = mainCharacter;

                    mainCharacter.GetComponent<UseComponent>().ReserveObject(snack);
                    mainCharacter.GetComponent<WalkComponent>().SetDestination(instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);

                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_SNACK);
                }
                else
                {
                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_FINISHED_SNACK);
                }
            }
        }

        public static void UpdateStateCafeteriaGoingToSnack(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_USING_SNACK);
            }
        }

        public static void UpdateStateCafeteriaUsingSnack(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                // free reserved object (snack)
                instance.GetEquipment(0).User = null;

                instance.SetParam(
                    ProcedureScriptStaffLunchPatch.PARAM_PAY,
                    instance.GetParam(ProcedureScriptStaffLunchPatch.PARAM_PAY) + UnityEngine.Random.Range(Tweakable.Mod.NeedHungerLunchPaymentSnackMinimum(), Tweakable.Mod.NeedHungerLunchPaymentSnackMaximum()));

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_FINISHED_SNACK);
            }
        }

        public static void UpdateStateCafeteriaFinishedSnack(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

            TileObject juice = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), currentRoom,
                Tags.Vanilla.Juice, AccessRights.STAFF_ONLY, false, null, false);

            if (juice != null)
            {
                // reserve juice
                instance.m_stateData.m_procedureScene.m_equipment[0] = juice;
                instance.GetEquipment(0).User = mainCharacter;

                mainCharacter.GetComponent<UseComponent>().ReserveObject(juice);
                mainCharacter.GetComponent<WalkComponent>().SetDestination(instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_JUICE);
            }
            else
            {
                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_FINISHED_JUICE);
            }
        }

        public static void UpdateStateCafeteriaGoingToJuice(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_USING_JUICE);
            }
        }

        public static void UpdateStateCafeteriaUsingJuice(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                // free reserved object (juice)
                instance.GetEquipment(0).User = null;

                instance.SetParam(
                    ProcedureScriptStaffLunchPatch.PARAM_PAY,
                    instance.GetParam(ProcedureScriptStaffLunchPatch.PARAM_PAY) + UnityEngine.Random.Range(Tweakable.Mod.NeedHungerLunchPaymentJuiceMinimum(), Tweakable.Mod.NeedHungerLunchPaymentJuiceMaximum()));

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_FINISHED_JUICE);
            }
        }

        public static void UpdateStateCafeteriaFinishedJuice(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

            TileObject coffeeMachine = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), currentRoom,
                Tags.Vanilla.CoffeeMachine, AccessRights.STAFF_ONLY, false, null, false);

            if (coffeeMachine != null)
            {
                instance.m_stateData.m_procedureScene.m_equipment[0] = coffeeMachine;

                mainCharacter.GetComponent<UseComponent>().ReserveObject(coffeeMachine);
                mainCharacter.GetComponent<WalkComponent>().SetDestination(coffeeMachine.GetDefaultUsePosition(), coffeeMachine.GetFloorIndex(), MovementType.WALKING);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_COFFEE_MAKER);
            }
            else
            {
                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_PAY);
            }
        }

        public static void UpdateStateCafeteriaGoingToCoffeeMaker(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_USING_COFFEE_MAKER);
            }
        }

        public static void UpdateStateCafeteriaUsingCoffeeMaker(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                // free reserved object (coffee maker)
                instance.GetEquipment(0).User = null;

                instance.SetParam(
                    ProcedureScriptStaffLunchPatch.PARAM_PAY, 
                    instance.GetParam(ProcedureScriptStaffLunchPatch.PARAM_PAY) + UnityEngine.Random.Range(Tweakable.Mod.NeedHungerLunchPaymentCoffeeMinimum(), Tweakable.Mod.NeedHungerLunchPaymentCoffeeMaximum()));

                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

                TileObject restObject = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                    mainCharacter, mainCharacter, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), currentRoom,
                    Tags.Vanilla.Rest, AccessRights.STAFF_ONLY, false, null, false);

                if (restObject != null)
                {
                    // reserve rest object (sofa)
                    instance.m_stateData.m_procedureScene.m_equipment[0] = restObject;
                    instance.GetEquipment(0).User = mainCharacter;

                    mainCharacter.GetComponent<UseComponent>().ReserveObject(restObject);
                    mainCharacter.GetComponent<WalkComponent>().GoSit(restObject, MovementType.WALKING);

                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_GOING_TO_SOFA);
                }
                else
                {
                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_PAY);
                }
            }
        }

        public static void UpdateStateCafeteriaGoingToSofa(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitIdleHoldPhone, false, true);

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_DRINKING_COFFEE);
            }
        }

        public static void UpdateStateCafeteriaDrinkingCoffee(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (instance.m_stateData.m_timeInState > instance.GetParam(ProcedureScriptStaffLunchPatch.PARAM_COFFEE_TIME))
            {
                if (mainCharacter.GetComponent<AnimModelComponent>().IsIdle())
                {
                    mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.REST_REDUCED, Tweakable.Mod.NeedRestPlayGame()));
                    mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.HadCoffee);

                    mainCharacter.GetComponent<WalkComponent>().StandUp();

                    instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_COFFEE_FINISHED);
                }
            }
            else
            {
                if (mainCharacter.GetComponent<AnimModelComponent>().IsIdle())
                {
                    mainCharacter.GetComponent<AnimModelComponent>().QueueAnimation(Animations.Vanilla.SitIdleHoldPhone, false, true);
                }
            }
        }

        public static void UpdateStateCafeteriaCoffeeFinished(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                // free reserved object (sofa)
                instance.GetEquipment(0).User = null;

                instance.SwitchState(ProcedureScriptStaffLunchPatch.STATE_CAFETERIA_PAY);
            }
        }

        public static void UpdateStateCafeteriaPay(ProcedureScriptStaffLunch instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            EmployeeComponent employee = mainCharacter.GetComponent<EmployeeComponent>();

            int payment = (int)instance.GetParam(ProcedureScriptStaffLunchPatch.PARAM_PAY);

            if (payment > 0)
            {
                if (employee != null)
                {
                    employee.m_state.m_department.GetEntity().Pay(payment, PaymentCategory.FOOD, mainCharacter);
                }

                if (SettingsManager.Instance.m_gameSettings.m_showPaymentsInGame.m_value)
                {
                    NotificationManager.GetInstance().AddFloatingIngameNotification(mainCharacter, "$" + payment, new Color(0.5f, 1f, 0.5f));
                }
            }

            instance.SwitchState(ProcedureScriptStaffLunch.STATE_IDLE);
        }



        public const string STATE_SEARCHING_ROOM = "STATE_SEARCHING_ROOM";

        // common room lunch

        public const string STATE_COMMON_ROOM_GOING_TO_ROOM = "STATE_COMMON_ROOM_GOING_TO_ROOM";
        public const string STATE_COMMON_ROOM_GOING_TO_FRIDGE = "STATE_COMMON_ROOM_GOING_TO_FRIDGE";
        public const string STATE_COMMON_ROOM_USING_FRIDGE = "STATE_COMMON_ROOM_USING_FRIDGE";
        public const string STATE_COMMON_ROOM_GOING_TO_CHAIR = "STATE_COMMON_ROOM_GOING_TO_CHAIR";
        public const string STATE_COMMON_ROOM_EATING = "STATE_COMMON_ROOM_EATING";
        public const string STATE_COMMON_ROOM_EATING_FINISHING = "STATE_COMMON_ROOM_EATING_FINISHING";
        public const string STATE_COMMON_ROOM_EATING_STANDING = "STATE_COMMON_ROOM_EATING_STANDING";
        public const string STATE_COMMON_ROOM_EATING_FINISHED = "STATE_COMMON_ROOM_EATING_FINISHED";
        public const string STATE_COMMON_ROOM_GOING_TO_COFFEE_MAKER = "STATE_COMMON_ROOM_GOING_TO_COFFEE_MAKER";
        public const string STATE_COMMON_ROOM_USING_COFFEE_MAKER = "STATE_COMMON_ROOM_USING_COFFEE_MAKER";
        public const string STATE_COMMON_ROOM_GOING_TO_SOFA = "STATE_COMMON_ROOM_GOING_TO_SOFA";
        public const string STATE_COMMON_ROOM_DRINKING_COFFEE = "STATE_COMMON_ROOM_DRINKING_COFFEE";
        public const string STATE_COMMON_ROOM_COFFEE_FINISHED = "STATE_COMMON_ROOM_COFFEE_FINISHED";

        // cafeteria lunch

        public const string STATE_CAFETERIA_GOING_TO_ROOM = "STATE_CAFETERIA_GOING_TO_ROOM";
        public const string STATE_CAFETERIA_GOING_TO_BOARD = "STATE_CAFETERIA_GOING_TO_BOARD";
        public const string STATE_CAFETERIA_USING_BOARD = "STATE_CAFETERIA_USING_BOARD";
        public const string STATE_CAFETERIA_GOING_TO_MEAL_COUNTER = "STATE_CAFETERIA_GOING_TO_MEAL_COUNTER";
        public const string STATE_CAFETERIA_USING_MEAL_COUNTER = "STATE_CAFETERIA_USING_MEAL_COUNTER";
        public const string STATE_CAFETERIA_GOING_TO_CHAIR = "STATE_CAFETERIA_GOING_TO_CHAIR";
        public const string STATE_CAFETERIA_EATING = "STATE_CAFETERIA_EATING";
        public const string STATE_CAFETERIA_EATING_FINISHED = "STATE_CAFETERIA_EATING_FINISHED";
        public const string STATE_CAFETERIA_GOING_TO_SNACK = "STATE_CAFETERIA_GOING_TO_SNACK";
        public const string STATE_CAFETERIA_USING_SNACK = "STATE_CAFETERIA_USING_SNACK";
        public const string STATE_CAFETERIA_FINISHED_SNACK = "STATE_CAFETERIA_FINISHED_SNACK";
        public const string STATE_CAFETERIA_GOING_TO_JUICE = "STATE_CAFETERIA_GOING_TO_JUICE";
        public const string STATE_CAFETERIA_USING_JUICE = "STATE_CAFETERIA_USING_JUICE";
        public const string STATE_CAFETERIA_FINISHED_JUICE = "STATE_CAFETERIA_FINISHED_JUICE";
        public const string STATE_CAFETERIA_GOING_TO_COFFEE_MAKER = "STATE_CAFETERIA_GOING_TO_COFFEE_MAKER";
        public const string STATE_CAFETERIA_USING_COFFEE_MAKER = "STATE_CAFETERIA_USING_COFFEE_MAKER";
        public const string STATE_CAFETERIA_GOING_TO_SOFA = "STATE_CAFETERIA_GOING_TO_SOFA";
        public const string STATE_CAFETERIA_DRINKING_COFFEE = "STATE_CAFETERIA_DRINKING_COFFEE";
        public const string STATE_CAFETERIA_COFFEE_FINISHED = "STATE_CAFETERIA_COFFEE_FINISHED";
        public const string STATE_CAFETERIA_PAY = "STATE_CAFETERIA_PAY";

        // parameters

        public const int PARAM_EATING_TIME = 0;
        public const int PARAM_COFFEE_TIME = 1;
        public const int PARAM_PAY = 2;
    }
}
