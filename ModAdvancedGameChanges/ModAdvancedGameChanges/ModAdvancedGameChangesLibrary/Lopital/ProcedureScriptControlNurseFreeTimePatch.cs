using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime))]
    public static class ProcedureScriptControlNurseFreeTimePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime), nameof(ProcedureScriptControlNurseFreeTime.Activate))]
        public static bool ActivatePrefix(ProcedureScriptControlNurseFreeTime __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, activating script {__instance.m_stateData.m_scriptName}");

            if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.Scholar))
            {
                __instance.SetParam(ProcedureScriptControlNurseFreeTimePatch.PARAM_EDUCATION_OBJECT_NOT_FOUND, 0f);
                ProcedureScriptControlNurseFreeTimePatch.ScholarChooseRoomAndEquipment(__instance);
            }
            else if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.Gamer))
            {
                ProcedureScriptControlNurseFreeTimePatch.GamerChooseRoomAndEquipment(__instance);
            }
            else
            {
                ProcedureScriptControlNurseFreeTimePatch.RestChooseRoomAndEquipment(__instance);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlNurseFreeTime), nameof(ProcedureScriptControlNurseFreeTime.ScriptUpdate))]
        public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptControlNurseFreeTime __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __instance.m_stateData.m_timeInState += deltaTime;

            switch (__instance.m_stateData.m_state)
            {
                // common rest
                case ProcedureScriptControlNurseFreeTimePatch.STATE_REST_GOING_TO_OBJECT:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateRestGoingToObject(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_REST_BLOCKED:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateRestBlocked(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_REST_RESTING:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateRestResting(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_REST_SIT_ON_REST_OBJECT:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateRestSitOnRestObject(__instance);
                    break;

                // scholar rest
                case ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_GOING_TO_EDUCATION_OBJECT:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateScholarGoingToEducationObject(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_USING_EDUCATION_OBJECT:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateScholarUsingEducationObject(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_GOING_TO_REST_OBJECT:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateScholarGoingToRestObject(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_SIT_ON_REST_OBJECT:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateScholarSitOnRestObject(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_RESTING:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateScholarResting(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_BLOCKED:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateScholarBlocked(__instance);
                    break;

                // gamer rest
                case ProcedureScriptControlNurseFreeTimePatch.STATE_GAMER_GOING_TO_OBJECT:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateGamerGoingToObject(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_GAMER_BLOCKED:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateGamerBlocked(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_GAMER_RESTING:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateGamerResting(__instance);
                    break;
                case ProcedureScriptControlNurseFreeTimePatch.STATE_GAMER_SIT_ON_REST_OBJECT:
                    ProcedureScriptControlNurseFreeTimePatch.UpdateStateGamerSitOnRestObject(__instance);
                    break;

                default:
                    break;
            }

            return false;
        }

        public static TileObject FindClosestFreeObject(ProcedureScriptControlNurseFreeTime instance, string tag)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Department department = mainCharacter.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();

            TileObject result = null;

            // try to find appropriate place in current room
            Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());
            if (currentRoom != null)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, searching in current room for '{tag}'");

                TileObject temp = MapScriptInterface.Instance.FindClosestFreeObjectWithTags(
                    mainCharacter, mainCharacter,
                    mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(),
                    currentRoom,
                    new string[] { tag }, AccessRights.STAFF, false, null, false);

                if (temp != null)
                {
                    Vector3i tempPosition = new Vector3i(temp.GetDefaultUseTile().m_x, temp.GetDefaultUseTile().m_y, temp.GetFloorIndex());
                    Vector3i resultPosition = (result == null) ? tempPosition : new Vector3i(result.GetDefaultUseTile().m_x, result.GetDefaultUseTile().m_y, result.GetFloorIndex());
                    Vector3i currentPosition = new Vector3i(mainCharacter.GetComponent<WalkComponent>().GetCurrentTile().m_x, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile().m_y, mainCharacter.GetComponent<WalkComponent>().GetFloorIndex());

                    if ((currentPosition - tempPosition).LengthSquared() <= (currentPosition - resultPosition).LengthSquared())
                    {
                        result = temp;
                    }
                }
            }

            // try to find appropriate place in home room
            if (mainCharacter.GetComponent<EmployeeComponent>().m_state.m_homeRoom != null)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, searching in home room for '{tag}'");

                TileObject temp = MapScriptInterface.Instance.FindClosestFreeObjectWithTags(
                    mainCharacter, mainCharacter,
                    mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(),
                    mainCharacter.GetComponent<EmployeeComponent>().m_state.m_homeRoom.GetEntity(),
                    new string[] { tag }, AccessRights.STAFF, false, null, false);

                if (temp != null)
                {
                    Vector3i tempPosition = new Vector3i(temp.GetDefaultUseTile().m_x, temp.GetDefaultUseTile().m_y, temp.GetFloorIndex());
                    Vector3i resultPosition = (result == null) ? tempPosition : new Vector3i(result.GetDefaultUseTile().m_x, result.GetDefaultUseTile().m_y, result.GetFloorIndex());
                    Vector3i currentPosition = new Vector3i(mainCharacter.GetComponent<WalkComponent>().GetCurrentTile().m_x, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile().m_y, mainCharacter.GetComponent<WalkComponent>().GetFloorIndex());

                    if ((currentPosition - tempPosition).LengthSquared() <= (currentPosition - resultPosition).LengthSquared())
                    {
                        result = temp;
                    }
                }
            }

            // try to find appropriate place in department in common room
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, searching in common room in department  for '{tag}'");

                TileObject temp = MapScriptInterface.Instance.FindClosestFreeObjectWithTags(
                    mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), mainCharacter.GetComponent<WalkComponent>().GetFloorIndex(),
                    department, new string[] { tag }, AccessRights.STAFF, Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.CommonRoom));

                if (temp != null)
                {
                    Vector3i tempPosition = new Vector3i(temp.GetDefaultUseTile().m_x, temp.GetDefaultUseTile().m_y, temp.GetFloorIndex());
                    Vector3i resultPosition = (result == null) ? tempPosition : new Vector3i(result.GetDefaultUseTile().m_x, result.GetDefaultUseTile().m_y, result.GetFloorIndex());
                    Vector3i currentPosition = new Vector3i(mainCharacter.GetComponent<WalkComponent>().GetCurrentTile().m_x, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile().m_y, mainCharacter.GetComponent<WalkComponent>().GetFloorIndex());

                    if ((currentPosition - tempPosition).LengthSquared() <= (currentPosition - resultPosition).LengthSquared())
                    {
                        result = temp;
                    }
                }
            }

            // try to find appropriate place in any common room
            if (result == null)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, searching in common room  for '{tag}'");

                TileObject temp = MapScriptInterface.Instance.FindClosestCenterObjectWithTagShortestPath(
                    mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), mainCharacter.GetComponent<WalkComponent>().GetFloorIndex(),
                    tag, AccessRights.STAFF, new string[] { RoomTypes.Vanilla.CommonRoom }, false, true, null, null);

                if (temp != null)
                {
                    Vector3i tempPosition = new Vector3i(temp.GetDefaultUseTile().m_x, temp.GetDefaultUseTile().m_y, temp.GetFloorIndex());
                    Vector3i resultPosition = (result == null) ? tempPosition : new Vector3i(result.GetDefaultUseTile().m_x, result.GetDefaultUseTile().m_y, result.GetFloorIndex());
                    Vector3i currentPosition = new Vector3i(mainCharacter.GetComponent<WalkComponent>().GetCurrentTile().m_x, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile().m_y, mainCharacter.GetComponent<WalkComponent>().GetFloorIndex());

                    if ((currentPosition - tempPosition).LengthSquared() <= (currentPosition - resultPosition).LengthSquared())
                    {
                        result = temp;
                    }
                }
            }

            return result;
        }

        public static void GamerChooseRoomAndEquipment(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Department department = mainCharacter.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();

            instance.m_stateData.m_procedureScene.m_equipment[0] = ProcedureScriptControlNurseFreeTimePatch.FindClosestFreeObject(instance, Tags.Vanilla.Rest);

            if (instance.m_stateData.m_procedureScene.m_equipment[0] != null)
            {
                instance.MoveCharacter(mainCharacter, instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex());
                instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_GAMER_GOING_TO_OBJECT);
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

                instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_GAMER_BLOCKED);
            }
        }

        public static void UpdateStateGamerBlocked(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Department department = mainCharacter.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();

            if (instance.m_stateData.m_timeInState < DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Mod.RestMinutes() / 60f))
            {
                ProcedureScriptControlNurseFreeTimePatch.RestChooseRoomAndEquipment(instance);
            }
            else
            {
                // not rested, finish procedure script
                instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_IDLE);
            }
        }

        public static void UpdateStateGamerGoingToObject(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (instance.GetEquipment(0).User == null)
                {
                    // object is not reserved
                    mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));

                    // sit on object
                    mainCharacter.GetComponent<WalkComponent>().GoSit(instance.GetEquipment(0), MovementType.WALKING);

                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_GAMER_SIT_ON_REST_OBJECT);
                }
                else
                {
                    mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Question, -1f);
                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_GAMER_BLOCKED);
                }
            }
        }

        public static void UpdateStateGamerResting(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Mod.RestMinutes() * UnityEngine.Random.Range(0.33f, 0.66f) / 60f) < instance.m_stateData.m_timeInState)
            {
                mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.REST_REDUCED, 1f));
                instance.m_stateData.m_timeInState = 0f;

                // free reserved object
                instance.GetEquipment(0).User = null;

                if (mainCharacter.GetComponent<MoodComponent>().HasSatisfactionModifier(SatisfationModifiers.Vanilla.Bored))
                {
                    mainCharacter.GetComponent<MoodComponent>().RemoveSatisfactionModifier(SatisfationModifiers.Vanilla.Bored);
                }
                mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.Entertained);

                if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.Spartan))
                {
                    mainCharacter.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Perks.Vanilla.Spartan);
                }

                mainCharacter.GetComponent<PropComponent>().m_state.m_prop = null;
                mainCharacter.GetComponent<SpeechComponent>().HideBubble();

                // we are done
                instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_IDLE);
            }
        }

        public static void UpdateStateGamerSitOnRestObject(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                mainCharacter.GetComponent<PropComponent>().m_state.m_prop = "phone";
                mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Game, -1f);

                mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.SitIdleHoldPhone, true);

                instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_GAMER_RESTING);
            }
        }

        public static void RestChooseRoomAndEquipment(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Department department = mainCharacter.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();

            instance.m_stateData.m_procedureScene.m_equipment[0] = ProcedureScriptControlNurseFreeTimePatch.FindClosestFreeObject(instance, Tags.Vanilla.Rest);

            if (instance.m_stateData.m_procedureScene.m_equipment[0] != null)
            {
                instance.MoveCharacter(mainCharacter, instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex());
                instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_REST_GOING_TO_OBJECT);
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

                instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_REST_BLOCKED);
            }
        }

        public static void UpdateStateRestBlocked(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Department department = mainCharacter.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();

            if (instance.m_stateData.m_timeInState < DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Mod.RestMinutes() / 60f))
            {
                ProcedureScriptControlNurseFreeTimePatch.RestChooseRoomAndEquipment(instance);
            }
            else
            {
                // not rested, finish procedure script
                instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_IDLE);
            }
        }

        public static void UpdateStateRestGoingToObject(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (instance.GetEquipment(0).User == null)
                {
                    // object is not reserved
                    mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));

                    // sit on object
                    mainCharacter.GetComponent<WalkComponent>().GoSit(instance.GetEquipment(0), MovementType.WALKING);
                    
                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_REST_SIT_ON_REST_OBJECT);
                }
                else
                {
                    mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Question, -1f);
                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_REST_BLOCKED);
                }
            }
        }

        public static void UpdateStateRestResting(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Mod.RestMinutes() / 60f) < instance.m_stateData.m_timeInState)
            {
                mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.REST_REDUCED, 1f));
                instance.m_stateData.m_timeInState = 0f;

                // free reserved object
                instance.GetEquipment(0).User = null;

                if (mainCharacter.GetComponent<MoodComponent>().HasSatisfactionModifier(SatisfationModifiers.Vanilla.Bored))
                {
                    mainCharacter.GetComponent<MoodComponent>().RemoveSatisfactionModifier(SatisfationModifiers.Vanilla.Bored);
                }
                mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.Entertained);

                if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.Spartan))
                {
                    mainCharacter.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Perks.Vanilla.Spartan);
                }

                // we are done
                instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_IDLE);
            }
        }

        public static void UpdateStateRestSitOnRestObject(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
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

                instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_REST_RESTING);
            }
        }

        public static void ScholarChooseRoomAndEquipment(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Department department = mainCharacter.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();

            instance.m_stateData.m_procedureScene.m_equipment[0] = ProcedureScriptControlNurseFreeTimePatch.FindClosestFreeObject(instance, Tags.Vanilla.Education);

            if (instance.m_stateData.m_procedureScene.m_equipment[0] != null)
            {
                instance.SetParam(ProcedureScriptControlNurseFreeTimePatch.PARAM_EDUCATION_OBJECT_NOT_FOUND, 0f);

                instance.MoveCharacter(mainCharacter, instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex());
                instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_GOING_TO_EDUCATION_OBJECT);
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

                // not found education object
                // just try to rest

                instance.SetParam(ProcedureScriptControlNurseFreeTimePatch.PARAM_EDUCATION_OBJECT_NOT_FOUND, 1f);

                instance.m_stateData.m_procedureScene.m_equipment[0] = ProcedureScriptControlNurseFreeTimePatch.FindClosestFreeObject(instance, Tags.Vanilla.Rest);

                if (instance.m_stateData.m_procedureScene.m_equipment[0] != null)
                {
                    instance.MoveCharacter(mainCharacter, instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex());
                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_GOING_TO_REST_OBJECT);
                }
                else
                {
                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_BLOCKED);
                }
            }
        }

        public static void UpdateStateScholarBlocked(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Department department = mainCharacter.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();

            if (instance.m_stateData.m_timeInState < DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Mod.RestMinutes() / 60f))
            {
                ProcedureScriptControlNurseFreeTimePatch.ScholarChooseRoomAndEquipment(instance);
            }
            else
            {
                // not rested, finish procedure script
                instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_IDLE);
            }
        }

        public static void UpdateStateScholarGoingToEducationObject(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (instance.GetEquipment(0).User == null)
                {
                    // object is not reserved
                    mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));

                    mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_USING_EDUCATION_OBJECT);
                }
                else
                {
                    mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Question, -1f);
                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_BLOCKED);
                }
            }
        }

        public static void UpdateStateScholarGoingToRestObject(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (instance.GetEquipment(0).User == null)
                {
                    // object is not reserved
                    mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));

                    // sit on object
                    mainCharacter.GetComponent<WalkComponent>().GoSit(instance.GetEquipment(0), MovementType.WALKING);

                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_SIT_ON_REST_OBJECT);
                }
                else
                {
                    mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Question, -1f);
                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_BLOCKED);
                }
            }
        }

        public static void UpdateStateScholarResting(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Mod.RestMinutes() / 60f) < instance.m_stateData.m_timeInState)
            {
                mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.REST_REDUCED, 1f));
                instance.m_stateData.m_timeInState = 0f;

                // free reserved object
                instance.GetEquipment(0).User = null;

                if (instance.GetParam(ProcedureScriptControlNurseFreeTimePatch.PARAM_EDUCATION_OBJECT_NOT_FOUND) == 0f)
                {
                    if (mainCharacter.GetComponent<MoodComponent>().HasSatisfactionModifier(SatisfationModifiers.Vanilla.Bored))
                    {
                        mainCharacter.GetComponent<MoodComponent>().RemoveSatisfactionModifier(SatisfationModifiers.Vanilla.Bored);
                    }
                    mainCharacter.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.Entertained);

                    int points = (int)((float)Tweakable.Vanilla.FreeTimeSkillPoints() * UnityEngine.Random.Range(0f, 1f));

                    if (mainCharacter.GetComponent<BehaviorNurse>() != null)
                    {
                        mainCharacter.GetComponent<EmployeeComponent>().AddSkillPoints(Skills.Vanilla.SKILL_NURSE_QUALIF_PATIENT_CARE, points, true);
                    }
                    else if (mainCharacter.GetComponent<BehaviorDoctor>() != null)
                    {
                        mainCharacter.GetComponent<EmployeeComponent>().AddSkillPoints(Skills.Vanilla.SKILL_DOC_QUALIF_GENERAL_MEDICINE, points, true);
                    }
                    else if (mainCharacter.GetComponent<BehaviorLabSpecialist>() != null)
                    {
                        mainCharacter.GetComponent<EmployeeComponent>().AddSkillPoints(Skills.Vanilla.SKILL_LAB_SPECIALIST_QUALIF_SCIENCE_EDUCATION, points, true);
                    }
                    else if (mainCharacter.GetComponent<BehaviorJanitor>() != null)
                    {
                        mainCharacter.GetComponent<EmployeeComponent>().AddSkillPoints(Skills.Vanilla.SKILL_JANITOR_QUALIF_EFFICIENCY, points, true);
                    }
                }

                if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.Spartan))
                {
                    mainCharacter.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Perks.Vanilla.Spartan);
                }

                // we are done
                instance.SwitchState(ProcedureScriptControlNurseFreeTime.STATE_IDLE);
            }
        }

        public static void UpdateStateScholarSitOnRestObject(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (instance.GetParam(ProcedureScriptControlNurseFreeTimePatch.PARAM_EDUCATION_OBJECT_NOT_FOUND) == 0f)
                {
                    mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.SitReadIdle, true);
                }
                else
                {
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
                }

                instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_RESTING);
            }
        }

        public static void UpdateStateScholarUsingEducationObject(ProcedureScriptControlNurseFreeTime instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                // free reserved object
                instance.GetEquipment(0).User = null;

                instance.m_stateData.m_procedureScene.m_equipment[0] = ProcedureScriptControlNurseFreeTimePatch.FindClosestFreeObject(instance, Tags.Vanilla.Rest);

                if (instance.m_stateData.m_procedureScene.m_equipment[0] != null)
                {
                    instance.MoveCharacter(mainCharacter, instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex());
                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_GOING_TO_REST_OBJECT);
                }
                else
                {
                    instance.SwitchState(ProcedureScriptControlNurseFreeTimePatch.STATE_SCHOLAR_BLOCKED);
                }
            }
        }

        public const string STATE_GAMER_GOING_TO_OBJECT = "STATE_GAMER_GOING_TO_OBJECT";
        public const string STATE_GAMER_BLOCKED = "STATE_GAMER_BLOCKED";
        public const string STATE_GAMER_RESTING = "STATE_GAMER_RESTING";
        public const string STATE_GAMER_SIT_ON_REST_OBJECT = "STATE_GAMER_SIT_ON_REST_OBJECT";

        public const string STATE_REST_GOING_TO_OBJECT = "STATE_REST_GOING_TO_OBJECT";
        public const string STATE_REST_BLOCKED = "STATE_REST_BLOCKED";
        public const string STATE_REST_RESTING = "STATE_REST_RESTING";
        public const string STATE_REST_SIT_ON_REST_OBJECT = "STATE_REST_SIT_ON_REST_OBJECT";

        public const string STATE_SCHOLAR_GOING_TO_EDUCATION_OBJECT = "STATE_SCHOLAR_GOING_TO_EDUCATION_OBJECT";
        public const string STATE_SCHOLAR_USING_EDUCATION_OBJECT = "STATE_SCHOLAR_USING_EDUCATION_OBJECT";
        public const string STATE_SCHOLAR_GOING_TO_REST_OBJECT = "STATE_SCHOLAR_GOING_TO_REST_OBJECT";
        public const string STATE_SCHOLAR_SIT_ON_REST_OBJECT = "STATE_SCHOLAR_SIT_ON_REST_OBJECT";
        public const string STATE_SCHOLAR_BLOCKED = "STATE_SCHOLAR_BLOCKED";
        public const string STATE_SCHOLAR_RESTING = "STATE_SCHOLAR_RESTING";

        public const int PARAM_EDUCATION_OBJECT_NOT_FOUND = 0;
    }
}
