using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System.Collections.Generic;
using System.Linq;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(DLCProcedureScriptPharmacy))]
    public static class DLCProcedureScriptPharmacyPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DLCProcedureScriptPharmacy), nameof(DLCProcedureScriptPharmacy.Activate))]
        public static bool ActivatePrefix(DLCProcedureScriptPharmacy __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            // main character can be patient or pedestrian

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, activating script {__instance.m_stateData.m_scriptName}");

            // reset assigned lab specialist, not needed anymore
            __instance.m_stateData.m_procedureScene.m_labSpecialist = null;

            __instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_SEARCHING_ROOM);

            __instance.SetParam(DLCProcedureScriptPharmacyPatch.PARAM_MAX_WAIT_TIME, DayTime.Instance.IngameTimeHoursToRealTimeSeconds(2f));

            if (__instance.IsPatient())
            {
                __instance.SetParam(DLCProcedureScriptPharmacyPatch.PARAM_PAY,
                    (float)mainCharacter.GetComponent<ProcedureComponent>().m_state.m_procedureQueue.m_activeTreatmentStates
                        .Where(ats => ats.m_treatment.Entry.PharmacyPickup)
                        .Sum(ats => ats.m_treatment.Entry.Cost));

                __instance.SetParam(DLCProcedureScriptPharmacyPatch.PARAM_BUY_RESTRICTED_ITEMS,
                    (float)mainCharacter.GetComponent<ProcedureComponent>().m_state.m_procedureQueue.m_activeTreatmentStates.Count(ats => ats.m_treatment.Entry.PharmacyPickup));

                var treatments = mainCharacter.GetComponent<BehaviorPatient>().m_state.m_medicalCondition.m_symptoms
                    .Where(s => s.m_active)
                    .SelectMany(s => s.m_symptom.Entry?.Treatments ?? new DatabaseEntryRef<GameDBTreatment>[0])
                    .Select(rt => rt.Entry)
                    .Where(t => t != null)
                    .Distinct()
                    .Where(t => t.PharmacyPickup);

                __instance.SetParam(DLCProcedureScriptPharmacyPatch.PARAM_BUY_ITEMS, (float)treatments.Count());
            }
            else
            {
                __instance.SetParam(DLCProcedureScriptPharmacyPatch.PARAM_PAY, 0f);
                __instance.SetParam(DLCProcedureScriptPharmacyPatch.PARAM_BUY_ITEMS, (float)UnityEngine.Random.Range(1, 5));
                __instance.SetParam(DLCProcedureScriptPharmacyPatch.PARAM_BUY_RESTRICTED_ITEMS, (float)UnityEngine.Random.Range(0, 3));
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DLCProcedureScriptPharmacy), nameof(DLCProcedureScriptPharmacy.ScriptUpdate))]
        public static bool ScriptUpdatePrefix(float deltaTime, DLCProcedureScriptPharmacy __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            __instance.m_stateData.m_timeInState += deltaTime;

            switch (__instance.m_stateData.m_state)
            {
                case DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_SEARCHING_ROOM:
                    DLCProcedureScriptPharmacyPatch.UpdateStateCustomerSearchingRoom(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_GOING_TO_ROOM:
                    DLCProcedureScriptPharmacyPatch.UpdateStateCustomerGoingToRoom(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_SEARCHING_DRUG_SHELF:
                    DLCProcedureScriptPharmacyPatch.UpdateStateCustomerSearchingDrugShelf(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_GOING_TO_DRUG_SHELF:
                    DLCProcedureScriptPharmacyPatch.UpdateStateCustomerGoingToDrugShelf(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_USING_DRUG_SHELF:
                    DLCProcedureScriptPharmacyPatch.UpdateStateCustomerUsingDrugShelf(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_GOING_TO_WAIT:
                    DLCProcedureScriptPharmacyPatch.UpdateStateCustomerGoingToWait(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_WAITING:
                    DLCProcedureScriptPharmacyPatch.UpdateStateCustomerWaiting(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_GOING_TO_PHARMACIST:
                    DLCProcedureScriptPharmacyPatch.UpdateStateCustomerGoingToPharmacist(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_QUESTION:
                    DLCProcedureScriptPharmacyPatch.UpdateStatePharmacistQuestion(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_WISH:
                    DLCProcedureScriptPharmacyPatch.UpdateStateCustomerWish(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_GOING_TO_DRUG_SHELF:
                    DLCProcedureScriptPharmacyPatch.UpdateStatePharmacistGoingToDrugShelf(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_SEARCHING_DRUG_SHELF:
                    DLCProcedureScriptPharmacyPatch.UpdateStatePharmacistSearchingDrugShelf(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_USING_DRUG_SHELF:
                    DLCProcedureScriptPharmacyPatch.UpdateStatePharmacistUsingDrugShelf(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_GOING_TO_WORKPLACE:
                    DLCProcedureScriptPharmacyPatch.UpdateStatePharmacistGoingToWorkplace(__instance);
                    break;
                case DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_PAY_FOR_DRUGS:
                    DLCProcedureScriptPharmacyPatch.UpdateStateCustomerPayForDrugs(__instance);
                    break;
                default:
                    break;
            }

            return false;
        }
        public static void UpdateStateCustomerSearchingRoom(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Department administrativeDepartment = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.AdministrativeDepartment));

            Vector3i position = MapScriptInterfacePatch.GetRandomFreePlaceInRoomTypeInDepartment(mainCharacter.GetComponent<WalkComponent>(), RoomTypes.Vanilla.Pharmacy, administrativeDepartment, AccessRights.PATIENT);

            if (position != Vector3i.ZERO_VECTOR)
            {
                mainCharacter.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);
                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_GOING_TO_ROOM);
            }
            else
            {
                instance.Leave(false);
            }
        }

        public static void UpdateStateCustomerGoingToRoom(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if ((!mainCharacter.GetComponent<WalkComponent>().IsBusy())
                || MapScriptInterfacePatch.IsInDestinationRoom(mainCharacter))
            {
                mainCharacter.GetComponent<WalkComponent>().Stop();
                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_SEARCHING_DRUG_SHELF);
            }
        }

        public static void UpdateStateCustomerSearchingDrugShelf(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (instance.GetParam(DLCProcedureScriptPharmacyPatch.PARAM_BUY_ITEMS) > 0f)
            {
                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

                var freeDrugShelfs = MapScriptInterface.Instance.FindAllObjectWithTags(currentRoom, new string[] { Tags.Vanilla.DrugShelfCustomer }, AccessRights.PATIENT, false)
                    .Where(ds => ds.m_state.m_user == null);

                if (freeDrugShelfs.Any())
                {
                    TileObject drugShelf = freeDrugShelfs.ElementAt(UnityEngine.Random.Range(0, freeDrugShelfs.Count()));

                    instance.m_stateData.m_procedureScene.m_equipment[0] = drugShelf;
                    mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));

                    mainCharacter.GetComponent<WalkComponent>().SetDestination(instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);
                    instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_GOING_TO_DRUG_SHELF);

                    instance.SetParam(DLCProcedureScriptPharmacyPatch.PARAM_BUY_ITEMS, instance.GetParam(DLCProcedureScriptPharmacyPatch.PARAM_BUY_ITEMS) - 1f);
                }
                else
                {
                    instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_GOING_TO_WAIT);
                }
            }
            else
            {
                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_GOING_TO_WAIT);
            }
        }

        public static void UpdateStateCustomerGoingToDrugShelf(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_USING_DRUG_SHELF);
                mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
            }
        }

        public static void UpdateStateCustomerUsingDrugShelf(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                // free reserved object
                instance.GetEquipment(0).User = null;
                mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);

                if (instance.IsPatient())
                {
                    var treatment = mainCharacter.GetComponent<BehaviorPatient>().m_state.m_medicalCondition.m_symptoms
                        .Where(s => s.m_active)
                        .SelectMany(s => s.m_symptom.Entry?.Treatments ?? new DatabaseEntryRef<GameDBTreatment>[0])
                        .Select(rt => rt.Entry)
                        .Where(t => t != null)
                        .Distinct()
                        .FirstOrDefault(t => t.PharmacyPickup);

                    if (treatment != null)
                    {
                        instance.SetParam(DLCProcedureScriptPharmacyPatch.PARAM_PAY, instance.GetParam(DLCProcedureScriptPharmacyPatch.PARAM_PAY) + treatment.Cost);
                        mainCharacter.GetComponent<ProcedureComponent>().SuppressSymptoms(treatment);
                    }
                }
                else
                {
                    instance.SetParam(
                        DLCProcedureScriptPharmacyPatch.PARAM_PAY,
                        instance.GetParam(DLCProcedureScriptPharmacyPatch.PARAM_PAY)
                        + UnityEngine.Random.Range(
                            (float)Tweakable.Mod.PharmacyNonRestrictedDrugsPaymentMinimum(),
                            (float)Tweakable.Mod.PharmacyNonRestrictedDrugsPaymentMaximum()));
                }

                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_SEARCHING_DRUG_SHELF);
            }
        }

        public static void UpdateStateCustomerGoingToWait(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

            currentRoom.EnqueueCharacter(mainCharacter, false);
            instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_WAITING);
        }

        public static void UpdateStateCustomerWaiting(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (instance.m_stateData.m_timeInState > instance.GetParam(DLCProcedureScriptPharmacyPatch.PARAM_MAX_WAIT_TIME))
                {
                    // leave, too long waiting
                    instance.Leave(false);
                }
                else
                {
                    Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());
                    List<Entity> pharmacists = MapScriptInterfacePatch.FindLabSpecialistsAssignedToRoom(
                        currentRoom, false, currentRoom.m_roomPersistentData.m_department.GetEntity(),
                        Database.Instance.GetEntry<GameDBEmployeeRole>(EmployeeRoles.Vanilla.Pharmacist),
                        Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.DLC_SKILL_LAB_SPECIALIST_SPEC_PHARMACOLOGY));

                    if (pharmacists.Count > 0)
                    {
                        if (currentRoom.IsCharactersTurn(mainCharacter))
                        {
                            Entity pharmacist = pharmacists
                                .Where(r => r.GetComponent<BehaviorLabSpecialist>().IsFree())
                                .OrderBy(r =>
                                {
                                    Vector2i pharmacistPosition = r.GetComponent<BehaviorLabSpecialist>().GetInteractionPosition();
                                    Vector2i entityPosition = mainCharacter.GetComponent<WalkComponent>().GetCurrentTile();

                                    return (pharmacistPosition - entityPosition).LengthSquared();
                                }).
                                FirstOrDefault();

                            if (pharmacist != null)
                            {
                                Vector2i interactionPosition = pharmacist.GetComponent<BehaviorLabSpecialist>().GetInteractionPosition();

                                mainCharacter.GetComponent<WalkComponent>().SetDestination(interactionPosition, mainCharacter.GetComponent<WalkComponent>().GetFloorIndex(), MovementType.WALKING);

                                // remove character from queue
                                currentRoom.DequeueCharacter(mainCharacter);
                                instance.FreeReservedTile();

                                // reserve lab specialist by character
                                pharmacist.GetComponent<BehaviorLabSpecialist>().CurrentPatient = mainCharacter;
                                pharmacist.GetComponent<EmployeeComponent>().SetReserved(Procedures.Vanilla.Pharmacy, mainCharacter);
                                pharmacist.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.OVERRIDE_BY_PROCEDURE_SCRIPT));

                                if (instance.IsPatient())
                                {
                                    mainCharacter.GetComponent<BehaviorPatient>().SwitchState(PatientState.BuyingMedicine);
                                }

                                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_GOING_TO_PHARMACIST);
                            }
                        }

                        if (instance.m_stateData.m_state == DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_WAITING)
                        {
                            if (!instance.TryToSit())
                            {
                                instance.TryToStand();
                            }
                        }
                    }
                    else
                    {
                        // leave, no pharmacist
                        instance.Leave(false);
                    }
                }
            }
        }

        public static void UpdateStateCustomerGoingToPharmacist(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                Entity pharmacist = mainCharacter.GetAssignedPharmacist();

                mainCharacter.GetComponent<AnimModelComponent>().SetDirection(pharmacist.GetComponent<BehaviorLabSpecialist>().GetInteractionOrientation(mainCharacter));
                pharmacist.GetComponent<AnimModelComponent>().SetDirection(pharmacist.GetComponent<BehaviorLabSpecialist>().GetInteractionOrientationSelf(mainCharacter));
                pharmacist.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Question, -1f);
                pharmacist.GetComponent<SpeechComponent>().PlayDialogue(Dialogues.Vanilla.StaffStatement);

                mainCharacter.GetComponent<SpeechComponent>().HideBubble();
                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_QUESTION);
            }
        }

        public static void UpdateStatePharmacistQuestion(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity pharmacist = mainCharacter.GetAssignedPharmacist();

            float actionTime = instance.GetActionTime(
                pharmacist,
                (int)DayTime.Instance.IngameTimeHoursToRealTimeSeconds(((float)Tweakable.Mod.PharmacistQuestionMinutes()) / 60f),
                pharmacist.GetComponent<EmployeeComponent>().GetSkillLevel(Skills.Vanilla.DLC_SKILL_LAB_SPECIALIST_SPEC_PHARMACOLOGY));

            if (instance.m_stateData.m_timeInState > actionTime)
            {
                if (pharmacist.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.PeoplePerson))
                {
                    if (pharmacist.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.PeoplePerson))
                    {
                        pharmacist.GetComponent<PerkComponent>().RevealPerk(Perks.Vanilla.PeoplePerson, true);
                    }

                    if (instance.IsPatient())
                    {
                        mainCharacter.GetComponent<PerkComponent>().RevealAllPerks(mainCharacter.GetComponent<BehaviorPatient>().IsBookmarked());
                    }
                }

                mainCharacter.GetComponent<SpeechComponent>().PlayDialogue(Dialogues.Vanilla.PatientStatement);
                pharmacist.GetComponent<SpeechComponent>().HideBubble();

                if (instance.GetParam(DLCProcedureScriptPharmacyPatch.PARAM_BUY_RESTRICTED_ITEMS) > 0f)
                {
                    mainCharacter.GetComponent<SpeechComponent>().SetBubble(
                        instance.IsPatient() 
                            ? Speeches.Vanilla.Prescription 
                            : (UnityEngine.Random.Range(0, 100) > 50) 
                                ? Speeches.Vanilla.Pills 
                                : Speeches.Vanilla.Prescription, 
                        -1f);
                    instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_WISH);
                }
                else
                {
                    mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Answer, -1f);
                    instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_CUSTOMER_PAY_FOR_DRUGS);
                }
            }
        }

        public static void UpdateStateCustomerWish(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity pharmacist = mainCharacter.GetAssignedPharmacist();

            if (instance.m_stateData.m_timeInState > 2f)
            {
                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(pharmacist.GetComponent<WalkComponent>());

                var freeDrugShelfs = MapScriptInterface.Instance.FindAllObjectWithTags(currentRoom, new string[] { Tags.Vanilla.DrugShelfPharmacist }, AccessRights.STAFF, false)
                    .Where(ds => ds.m_state.m_user == null);

                if (freeDrugShelfs.Any())
                {
                    TileObject drugShelf = freeDrugShelfs.ElementAt(UnityEngine.Random.Range(0, freeDrugShelfs.Count()));

                    instance.m_stateData.m_procedureScene.m_equipment[0] = drugShelf;
                    pharmacist.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));
                    pharmacist.GetComponent<WalkComponent>().SetDestination(instance.GetEquipment(0).GetDefaultUsePosition(), instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);

                    mainCharacter.GetComponent<SpeechComponent>().HideBubble();
                    instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_GOING_TO_DRUG_SHELF);
                }
            }
        }

        public static void UpdateStatePharmacistGoingToDrugShelf(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity pharmacist = mainCharacter.GetAssignedPharmacist();

            if (!pharmacist.GetComponent<WalkComponent>().IsBusy())
            {
                pharmacist.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Question, -1f);
                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_SEARCHING_DRUG_SHELF);
            }
        }

        public static void UpdateStatePharmacistSearchingDrugShelf(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity pharmacist = mainCharacter.GetAssignedPharmacist();

            float actionTime = instance.GetActionTime(
                pharmacist,
                (int)DayTime.Instance.IngameTimeHoursToRealTimeSeconds(((float)Tweakable.Mod.PharmacistSearchDrugMinutes()) / 60f),
                pharmacist.GetComponent<EmployeeComponent>().GetSkillLevel(Skills.Vanilla.DLC_SKILL_LAB_SPECIALIST_SPEC_PHARMACOLOGY));

            if (instance.m_stateData.m_timeInState > actionTime)
            {
                pharmacist.GetComponent<SpeechComponent>().HideBubble();
                pharmacist.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_USING_DRUG_SHELF);
            }
        }

        public static void UpdateStatePharmacistUsingDrugShelf(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity pharmacist = mainCharacter.GetAssignedPharmacist();

            if (!pharmacist.GetComponent<UseComponent>().IsBusy())
            {
                // free reserved object
                instance.GetEquipment(0).User = null;

                pharmacist.GetComponent<EmployeeComponent>().AddSkillPoints(Skills.Vanilla.DLC_SKILL_LAB_SPECIALIST_SPEC_PHARMACOLOGY, Tweakable.Mod.PharmacistSearchDrugSkillPoints(), true);

                TileObject workChair = pharmacist.GetComponent<EmployeeComponent>().GetWorkChair();
                pharmacist.GetComponent<WalkComponent>().GoSit(workChair, MovementType.WALKING);

                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_GOING_TO_WORKPLACE);
            }
        }

        public static void UpdateStatePharmacistGoingToWorkplace(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity pharmacist = mainCharacter.GetAssignedPharmacist();

            if (!pharmacist.GetComponent<WalkComponent>().IsBusy())
            {
                pharmacist.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Happy, 5f);
                pharmacist.GetComponent<SpeechComponent>().PlayDialogue(Dialogues.Vanilla.StaffStatement);

                mainCharacter.GetComponent<SpeechComponent>().HideBubble();
                mainCharacter.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Happy, 5f);

                instance.SetParam(DLCProcedureScriptPharmacyPatch.PARAM_BUY_RESTRICTED_ITEMS, instance.GetParam(DLCProcedureScriptPharmacyPatch.PARAM_BUY_RESTRICTED_ITEMS) - 1f);
                
                instance.SwitchState(DLCProcedureScriptPharmacyPatch.STATE_PHARMACIST_QUESTION);
            }
        }

        public static void UpdateStateCustomerPayForDrugs(DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity pharmacist = mainCharacter.GetAssignedPharmacist();

            float actionTime = instance.GetActionTime(
                pharmacist,
                (int)DayTime.Instance.IngameTimeHoursToRealTimeSeconds(((float)Tweakable.Mod.PharmacistQuestionMinutes()) / 60f),
                pharmacist.GetComponent<EmployeeComponent>().GetSkillLevel(Skills.Vanilla.DLC_SKILL_LAB_SPECIALIST_SPEC_PHARMACOLOGY));

            if (instance.m_stateData.m_timeInState > actionTime)
            {
                instance.Leave(true);
            }
        }

        public static Entity GetAssignedPharmacist(this Entity mainCharacter)
        {
            Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());
            return currentRoom?.m_roomPersistentData.m_department.GetEntity().m_departmentPersistentData.m_labSpecialists
                .FirstOrDefault(l => l.GetEntity().GetComponent<BehaviorLabSpecialist>().CurrentPatient == mainCharacter).GetEntity();
        }

        public static Vector2i GetReservedTile(this DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

            if (currentRoom != null)
            {
                Floor floor = Hospital.Instance.m_floors[currentRoom.GetFloorIndex()];

                for (int i = currentRoom.m_roomPersistentData.m_positionBottom.m_x; i <= currentRoom.m_roomPersistentData.m_positionTop.m_x; i++)
                {
                    for (int j = currentRoom.m_roomPersistentData.m_positionBottom.m_y; j <= currentRoom.m_roomPersistentData.m_positionTop.m_y; j++)
                    {
                        Vector2i position = new Vector2i(i, j);

                        if (currentRoom.IsPositionInRoom(position)
                            && (MapScriptInterface.Instance.GetTileReservedBy(position, currentRoom.GetFloorIndex()) == mainCharacter))
                        {
                            return position;
                        }
                    }
                }
            }

            return Vector2i.ZERO_VECTOR;
        }

        public static void FreeReservedTile(this DLCProcedureScriptPharmacy instance)
        {
            Vector2i reserved = instance.GetReservedTile();

            if (reserved != Vector2i.ZERO_VECTOR)
            {
                Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

                MapScriptInterface.Instance.FreeTile(reserved, mainCharacter.GetComponent<WalkComponent>().GetFloorIndex());
            }
        }

        public static bool IsPatient(this DLCProcedureScriptPharmacy instance)
        {
            return instance.m_stateData.m_procedureScene.MainCharacter.GetComponent<BehaviorPatient>() != null;
        }

        public static void Leave(this DLCProcedureScriptPharmacy instance, bool pay)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

            if (pay)
            {
                int amount = (int)instance.GetParam(DLCProcedureScriptPharmacyPatch.PARAM_PAY);

                Department administrativeDepartment = MapScriptInterface.Instance.GetDepartmentOfType(Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.AdministrativeDepartment));

                administrativeDepartment.Pay(amount, PaymentCategory.INSURANCE_CLINIC, mainCharacter);
                if (SettingsManager.Instance.m_gameSettings.m_showPaymentsInGame.m_value)
                {
                    NotificationManager.GetInstance().AddFloatingIngameNotification(mainCharacter, "$" + amount, new UnityEngine.Color(0.5f, 1f, 0.5f));
                }
            }

            Entity pharmacist = mainCharacter.GetAssignedPharmacist();
            if (pharmacist != null)
            {
                pharmacist.GetComponent<BehaviorLabSpecialist>().CurrentPatient = null;
                pharmacist.GetComponent<EmployeeComponent>().SetReserved(string.Empty, null);
                pharmacist.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.CANCEL_OVERRIDE_BY_PROCEDURE_SCRIPT));
            }

            currentRoom?.DequeueCharacter(mainCharacter);
            instance.FreeReservedTile();

            instance.SwitchState(DLCProcedureScriptPharmacy.STATE_IDLE);
            mainCharacter.GetComponent<SpeechComponent>().HideBubble();

            if (instance.IsPatient())
            {
                mainCharacter.GetComponent<WalkComponent>().SetDestination(MapScriptInterface.Instance.GetRandomSpawnPosition(), 0, MovementType.WALKING);
                mainCharacter.GetComponent<BehaviorPatient>().SwitchState(PatientState.Leaving);
            }
            else
            {
                mainCharacter.GetComponent<WalkComponent>().SetDestination(MapScriptInterface.Instance.DEBUG_GetPedestiranSpawnPosition(), 0, MovementType.WALKING);
                mainCharacter.GetComponent<BehaviorPedestrian>().SwitchState(BehaviorPedestrianState.WalkingBack);
            }
        }

        public static bool TryToSit(this DLCProcedureScriptPharmacy instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

            bool result = false;
            result |= mainCharacter.GetComponent<WalkComponent>().IsSitting();

            if (!result)
            {
                TileObject chairObject = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                    mainCharacter, null, mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(),
                    currentRoom, Tags.Vanilla.Sitting, AccessRights.PATIENT, false, null, false);

                if (chairObject != null)
                {
                    instance.FreeReservedTile();
                    mainCharacter.GetComponent<WalkComponent>().GoSit(chairObject, MovementType.WALKING);

                    if (instance.IsPatient())
                    {
                        mainCharacter.GetComponent<BehaviorPatient>().SwitchState(PatientState.WaitingSittingInPharmacy);
                    }

                    return true;
                }
            }

            return result;
        }

        public static bool TryToStand(this DLCProcedureScriptPharmacy instance)
        {
            Vector2i reserved = instance.GetReservedTile();

            if (reserved != Vector2i.ZERO_VECTOR)
            {
                return true;
            }
            else
            {
                Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(mainCharacter.GetComponent<WalkComponent>());

                Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(currentRoom, AccessRights.PATIENT);

                if (position != Vector2i.ZERO_VECTOR)
                {
                    mainCharacter.GetComponent<WalkComponent>().SetDestination(position, currentRoom.GetFloorIndex(), MovementType.WALKING);

                    MapScriptInterface.Instance.ReserveTile(position, mainCharacter, currentRoom.GetFloorIndex());

                    if (instance.IsPatient())
                    {
                        mainCharacter.GetComponent<BehaviorPatient>().SwitchState(PatientState.WaitingStandingInPharmacy);
                    }

                    return true;
                }

                return false;
            }
        }

        public const string STATE_CUSTOMER_SEARCHING_ROOM = "STATE_CUSTOMER_SEARCHING_ROOM";
        public const string STATE_CUSTOMER_GOING_TO_ROOM = "STATE_CUSTOMER_GOING_TO_ROOM";

        public const string STATE_CUSTOMER_SEARCHING_DRUG_SHELF = "STATE_CUSTOMER_SEARCHING_DRUG_SHELF";
        public const string STATE_CUSTOMER_GOING_TO_DRUG_SHELF = "STATE_CUSTOMER_GOING_TO_DRUG_SHELF";
        public const string STATE_CUSTOMER_USING_DRUG_SHELF = "STATE_CUSTOMER_USING_DRUG_SHELF";

        public const string STATE_CUSTOMER_GOING_TO_WAIT = "STATE_CUSTOMER_GOING_TO_WAIT";
        public const string STATE_CUSTOMER_WAITING = "STATE_CUSTOMER_WAITING";

        public const string STATE_CUSTOMER_GOING_TO_PHARMACIST = "STATE_CUSTOMER_GOING_TO_PHARMACIST";
        public const string STATE_PHARMACIST_QUESTION = "STATE_PHARMACIST_QUESTION";
        public const string STATE_CUSTOMER_WISH = "STATE_CUSTOMER_WISH";
        public const string STATE_PHARMACIST_GOING_TO_DRUG_SHELF = "STATE_PHARMACIST_GOING_TO_DRUG_SHELF";
        public const string STATE_PHARMACIST_SEARCHING_DRUG_SHELF = "STATE_PHARMACIST_SEARCHING_DRUG_SHELF";
        public const string STATE_PHARMACIST_USING_DRUG_SHELF = "STATE_PHARMACIST_USING_DRUG_SHELF";
        public const string STATE_PHARMACIST_GOING_TO_WORKPLACE = "STATE_PHARMACIST_GOING_TO_WORKPLACE";
        public const string STATE_CUSTOMER_PAY_FOR_DRUGS = "STATE_CUSTOMER_PAY_FOR_DRUGS";

        public const int PARAM_PAY = 0;
        public const int PARAM_BUY_ITEMS = 1;
        public const int PARAM_MAX_WAIT_TIME = 2;
        public const int PARAM_BUY_RESTRICTED_ITEMS = 3;
    }
}
