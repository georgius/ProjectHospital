using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(BehaviorPatient))]
    public static class BehaviorPatientPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorDoctor), "CheckNeeds")]
        public static bool CheckNeedsPrefix(AccessRights accessRights, BehaviorPatient __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, this method should not be called!");

            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "CheckReception")]
        public static bool CheckReceptionPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            //Department departmentOfType = MapScriptInterface.Instance.GetDepartmentOfType(__instance.m_entity.GetComponent<BehaviorPatient>().BelongsToDepartment());

            ////Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, department {departmentOfType.m_departmentPersistentData.m_departmentType.m_id}, referal {__instance.m_state.m_fromReferral}");

            //if (__instance.m_state.m_fromReferral && (departmentOfType != null) && (departmentOfType.GetDepartmentType() != Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Emergency)))
            //{
            ////    Entity nurseOnDepartment = MapScriptInterface.Instance.FindNurseAssignedToARoomType(
            ////        Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.Reception), false, departmentOfType, 
            ////        Database.Instance.GetEntry<GameDBEmployeeRole>(EmployeeRoles.Vanilla.Receptionist),
            ////        Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_RECEPTIONIST));

            ////    if (nurseOnDepartment != null)
            ////    {
            //        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, changing to department {departmentOfType.m_departmentPersistentData.m_departmentType.m_id}");

            //        __instance.ChangeDepartment(departmentOfType, false, false, HospitalizationLevel.NONE);
            ////    }
            //}

            // if activated option AGC_OPTION_PATIENTS_ONLY_EMERGENCY (patients go through emergency) then department will be DPT_EMERGENCY
            // otherwise it can be any department in hospital
            //Entity receptionist = 
            //    MapScriptInterface.Instance.FindNurseAssignedToARoomType(
            //        Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.Reception), false, __instance.m_state.m_department.GetEntity(),
            //        Database.Instance.GetEntry<GameDBEmployeeRole>(EmployeeRoles.Vanilla.Receptionist),
            //        Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_RECEPTIONIST));

            //if (receptionist != null)
            //{
            //    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, receptionist {receptionist.Name}");

            //    if ((__instance.m_state.m_waitingRoom == null) || (__instance.m_state.m_waitingRoom.GetEntity() == null))
            //    {
            //        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no reception set");

            //        __instance.m_state.m_waitingRoom = receptionist.GetComponent<EmployeeComponent>().m_state.m_homeRoom;
            //    }

            //    if (!__instance.IsInWaitingRoom())
            //    {
            //        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, is not in reception");

            //        if ((__instance.m_state.m_reservedWaitingRoomTile == Vector2i.ZERO_VECTOR) && __instance.TryToStandInternal(true, true))
            //        {
            //            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to reception");

            //            __instance.SwitchState(PatientState.GoingToReception);
            //            return false;
            //        }

            //        //if (!receptionist.GetComponent<WalkComponent>().IsBusy())
            //        //{
            //        //    __instance.m_state.m_reservedWaitingRoomTile = Vector2i.ZERO_VECTOR;
            //        //    return false;
            //        //}
            //    }

            //    if (__instance.IsInWaitingRoom())
            //    {
            //        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, in reception");

            //        __instance.m_state.m_waitingRoom.GetEntity().EnqueueCharacter(__instance.m_entity, false);
            //    }

            //    if (__instance.IsInWaitingRoom() && receptionist.GetComponent<BehaviorNurse>().IsFree() && __instance.m_state.m_waitingRoom.GetEntity().IsCharactersTurn(__instance.m_entity))
            //    {
            //        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, on turn");

            //        Vector2i interactionPosition = receptionist.GetComponent<BehaviorNurse>().GetInteractionPosition();
            //        TileObject centerObjectAt = MapScriptInterface.Instance.GetCenterObjectAt(interactionPosition, receptionist.GetComponent<WalkComponent>().GetFloorIndex());

            //        if ((centerObjectAt != null) && centerObjectAt.HasTag(Tags.Vanilla.Sitting))
            //        {
            //            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to sit");

            //            __instance.GetComponent<WalkComponent>().GoSit(centerObjectAt, MovementType.WALKING);
            //            __instance.m_state.m_chair = centerObjectAt;
            //        }
            //        else
            //        {
            //            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to receptionist");

            //            __instance.GetComponent<WalkComponent>().SetDestination(interactionPosition, receptionist.GetComponent<WalkComponent>().GetFloorIndex(), MovementType.WALKING);
            //        }

            //        receptionist.GetComponent<BehaviorNurse>().CurrentPatient = __instance.m_entity;
            //        __instance.m_state.m_nurse = receptionist;

            //        __instance.FreeWaitingRoom();
            //        __instance.m_state.m_chair = null;
            //        __instance.SwitchState(PatientState.GoingToReceptionist);

            //        return false;
            //    }

            //    if (__instance.IsInWaitingRoom() && (__instance.m_state.m_chair == null))
            //    {
            //        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, is in waiting room, no chair");

            //        if (__instance.m_state.m_reservedWaitingRoomTile != Vector2i.ZERO_VECTOR)
            //        {
            //            MapScriptInterface.Instance.FreeTile(__instance.m_state.m_reservedWaitingRoomTile, receptionist.GetComponent<WalkComponent>().GetFloorIndex());
            //            __instance.m_state.m_reservedWaitingRoomTile = Vector2i.ZERO_VECTOR;
            //        }

            //        TileObject chairObject = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(__instance.m_entity, null, __instance.GetComponent<GridComponent>().GetGridPosition(), __instance.m_state.m_waitingRoom.GetEntity(), Tags.Vanilla.Sitting, AccessRights.PATIENT, false, null, false);
            //        if (__instance.m_state.m_triedToSitAndFailed || (!__instance.TryToSitInternal(chairObject, false)))
            //        {
            //            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no free seats");

            //            if (__instance.TryToStandInternal(true, true))
            //            {
            //                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, found place to stand");

            //                __instance.m_state.m_triedToSitAndFailed = false;
            //            }

            //            //if (__instance.m_state.m_waitingRoom != null)
            //            //{
            //            //    __instance.m_state.m_waitingRoom.GetEntity().DequeueCharacter(__instance.m_entity);
            //            //}

            //            //__instance.m_state.m_nurse = null;
            //            //__instance.m_state.m_waitingRoom = null;
            //            //__instance.m_state.m_chair = null;
            //            //__instance.GoToEmergencyInternal();
            //        }

            //        return false;
            //    }
            //}
            //else
            //{
            //    if (__instance.m_state.m_waitingRoom != null)
            //    {
            //        __instance.m_state.m_waitingRoom.GetEntity().DequeueCharacter(__instance.m_entity);
            //    }
            //    __instance.m_state.m_nurse = null;
            //    __instance.m_state.m_waitingRoom = null;
            //    __instance.m_state.m_chair = null;
            //    __instance.GoToEmergencyInternal();
            //}

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, this method should not be called!");

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "EnsureWaitingRoom")]
        private static bool EnsureWaitingRoomPrefix(BehaviorPatient __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (__instance.m_state.m_department == null)
            {
                __result = false;
                return false;
            }

            if (__instance.m_state.m_waitingRoom != null)
            {
                __result = true;
                return false;
            }

            GameDBRoomType waitingRoomType = Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.WaitingRoom);
            Room room = MapScriptInterface.Instance.FindLeastFullRoomAroundOutpatientOffice(waitingRoomType, __instance.m_state.m_department.GetEntity());

            if (room != null)
            {
                __instance.m_state.m_waitingRoom = room;
                //room.EnqueueCharacter(__instance.m_entity, true);

                __result = true;
                return false;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), nameof(BehaviorPatient.GoToWaitingRoom))]
        public static bool GoToWaitingRoomPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.EnsureWaitingRoomInternal())
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no waiting room, leaving");

                __instance.ReportMissingWaitingRoomInternal();
                __instance.Leave(true, false, false);

                return false;
            }

            // in __instance.m_state.m_waitingRoom is waiting room
            // choose some place, start walking and reset waiting room

            Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(__instance.m_state.m_waitingRoom.GetEntity(), AccessRights.PATIENT);

            if (position != Vector2i.ZERO_VECTOR)
            {
                __instance.GetComponent<WalkComponent>().SetDestination(position, __instance.m_state.m_waitingRoom.GetEntity().GetFloorIndex(), MovementType.WALKING);
                __instance.SwitchState(PatientState.GoingToWaitingRoom);

                __instance.FreeWaitingRoom();
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), nameof(BehaviorPatient.FreeWaitingRoom))]
        public static bool FreeWaitingRoomPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (__instance.m_state.m_waitingRoom != null)
            {
                __instance.m_state.m_waitingRoom.GetEntity().DequeueCharacter(__instance.m_entity);
                __instance.m_state.m_waitingRoom = null;
            }

            if (__instance.m_state.m_reservedWaitingRoomTile != Vector2i.ZERO_VECTOR)
            {
                MapScriptInterface.Instance.FreeTile(__instance.m_state.m_reservedWaitingRoomTile, __instance.GetComponent<WalkComponent>().GetFloorIndex());
                __instance.m_state.m_reservedWaitingRoomTile = Vector2i.ZERO_VECTOR;
            }

            __instance.m_state.m_chair = null;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), nameof(BehaviorPatient.Leave))]
        public static bool LeavePrefix(bool pay, bool leaveAfterHours, bool leavingHospitalizationPatient, BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            foreach (EntityIDPointer<Entity> entityIDPointer in __instance.m_state.m_pastDoctors)
            {
                if (entityIDPointer.GetEntity() != null)
                {
                    entityIDPointer.GetEntity().GetComponent<BehaviorDoctor>().AddPatientScore(
                        Math.Min(100, __instance.GetComponent<MoodComponent>().GetTotalSatisfaction()), 
                        Math.Min(100, __instance.GetComponent<MoodComponent>().GetTotalDiscomfort()), 
                        __instance.m_state.m_collapseCount, 
                        (__instance.m_state.m_medicalCondition.m_wrongDiagnoses != null) ? __instance.m_state.m_medicalCondition.m_wrongDiagnoses.Count : 0, 
                        false);
                }
            }

            __instance.FreeWaitingRoom();

            if (__instance.m_state.m_department != null)
            {
                __instance.m_state.m_department.GetEntity().RemovePatient(__instance.m_entity);
            }

            if (__instance.m_state.m_sentHome)
            {
                __instance.m_entity.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Home, 10f);

                if (__instance.HasBeenTreated())
                {
                    if (!__instance.m_state.m_treatedCounted)
                    {
                        __instance.CountTreatedPatient();
                    }
                }
                else if (!__instance.m_state.m_untreated)
                {
                    __instance.m_state.m_department.GetEntity().m_departmentPersistentData.m_todaysStatistics.m_untreatedPatients++;
                    __instance.m_state.m_department.GetEntity().m_departmentPersistentData.m_todaysStatistics.m_clinicUntreated++;

                    if (__instance.m_state.m_doctor.GetEntity() != null)
                    {
                        BehaviorDoctor doctor = __instance.m_state.m_doctor.GetEntity().GetComponent<BehaviorDoctor>();

                        doctor.m_state.m_todaysStatistics.m_untreated++;
                        doctor.m_state.m_allTimeStatisics.m_untreated++;
                    }
                    __instance.m_state.m_untreated = true;
                }
            }
            else if (!leaveAfterHours)
            {
                // very strange original code
                int num = System.Math.Min(4, __instance.m_entity.GetComponent<MoodComponent>().GetTotalSatisfaction() / 20);
                __instance.m_entity.GetComponent<SpeechComponent>().SetBubble(20 - num, 10f);
            }
            else
            {
                __instance.m_entity.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Unhappy, 10f);
            }
            
            __instance.GetComponent<WalkComponent>().SetDestination(MapScriptInterface.Instance.GetRandomSpawnPosition(), 0, MovementType.WALKING);

            if (__instance.HasBeenTreated() && pay && (!leaveAfterHours))
            {
                if (__instance.ShouldPatientPay())
                {
                    __instance.m_state.m_department.GetEntity().Pay(__instance.GetInsurancePayment(true), PaymentCategory.INSURANCE_CLINIC, __instance.m_entity);
                    __instance.GetComponent<SoundSourceComponent>().PlaySoundEvent(Sounds.Vanilla.PatientPays, SoundEventCategory.SFX, 1f, 0f, true, false);

                    if (SettingsManager.Instance.m_gameSettings.m_showPaymentsInGame.m_value)
                    {
                        NotificationManager.GetInstance().AddFloatingIngameNotification(__instance.m_entity, "$" + __instance.GetInsurancePayment(false), new Color(0.5f, 1f, 0.5f));
                    }
                }
                PlayerStatistics.Instance.IncrementStatistic(Statistics.Vanilla.Treated, 1);
            }

            InsuranceManager.Instance.UpdateInsuranceCompanyRequirementsAndObjectives(InsuranceCheckMode.IMMEDIATE);

            __instance.m_state.m_bookmarked = false;
            BookmarkedCharacterManager.Instance.RemoveCharacter(__instance.m_entity);
            
            __instance.SwitchState(PatientState.Leaving);

            return false;
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(BehaviorPatient), "LeaveAfterClosingHours")]
        //public static bool LeaveAfterClosingHoursPrefix(BehaviorPatient __instance)
        //{
        //    if (!ViewSettingsPatch.m_enabled)
        //    {
        //        // Allow original method to run
        //        return true;
        //    }

        //    __instance.m_state.m_chair = null;
        //    if ((__instance.m_state.m_waitingRoom != null) && (__instance.m_state.m_waitingRoom.GetEntity() != null))
        //    {
        //        __instance.m_state.m_waitingRoom.GetEntity().DequeueCharacter(__instance.m_entity);
        //    }

        //    __instance.GetComponent<MoodComponent>().AddSatisfactionModifier(Moods.Vanilla.Untreated);

        //    __instance.Leave(false, true, false);

        //    if (!this.m_state.m_sentHome)
        //    {
        //        if (this.m_state.m_department.GetEntity() != null)
        //        {
        //            this.m_state.m_department.GetEntity().m_departmentPersistentData.m_todaysStatistics.m_untreatedPatients++;
        //            this.m_state.m_department.GetEntity().m_departmentPersistentData.m_todaysStatistics.m_clinicUntreated++;
        //        }
        //        if (this.m_state.m_doctor.GetEntity() != null)
        //        {
        //            BehaviorDoctor component = this.m_state.m_doctor.GetEntity().GetComponent<BehaviorDoctor>();
        //            BehaviorDoctorPrototypeData state = component.m_state;
        //            state.m_todaysStatistics.m_untreated = state.m_todaysStatistics.m_untreated + 1;
        //            BehaviorDoctorPrototypeData state2 = component.m_state;
        //            state2.m_allTimeStatisics.m_untreated = state2.m_allTimeStatisics.m_untreated + 1;
        //        }
        //    }
        //    this.m_state.m_untreated = true;

        //    return false;
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), nameof(BehaviorPatient.ReceiveMessage))]
        public static bool ReceiveMessagePrefix(Message message, BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name} Message received: {message.m_messageID}");

            return BehaviorPatch.ReceiveMessage(message, __instance, true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "ReportMissingWaitingRoom")]
        private static bool ReportMissingWaitingRoomPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            string department = StringTable.GetInstance().GetLocalizedText(__instance.m_state.m_department.GetEntity().m_departmentPersistentData.m_departmentType.Entry);
            NotificationManager.GetInstance().AddMessage(__instance.m_entity, Notifications.Vanilla.NOTIF_NO_WAITING_ROOM, department, string.Empty, string.Empty, 0, 0, 0, 0, null, null);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), nameof(BehaviorPatient.SwitchState))]
        public static bool SwitchStatePrefix(PatientState state, BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (__instance.m_state.m_patientState != state)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, switching state from {__instance.m_state.m_patientState} to {state}");

                __instance.m_state.m_patientState = state;
                __instance.m_state.m_timeInState = 0f;
                __instance.m_state.m_continuousWaitingTime = 0f;
                __instance.m_state.m_playerControlwaitingTime = 0f;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), nameof(BehaviorPatient.Update))]
        public static bool UpdatePrefix(float deltaTime, BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (deltaTime <= 0f)
            {
                return false;
            }

            if (__instance.m_state.m_fromLevelNoCollapse)
            {
                __instance.m_state.m_medicalCondition.ResetCollapseTimes(__instance);
                __instance.m_state.m_collapseProcedure = null;
                __instance.m_state.m_collapseSymptom = null;
            }

            if (((__instance.m_state.m_collapseSymptom != null) || (__instance.m_state.m_collapseProcedure != null)) && (!__instance.m_state.m_medicalCondition.HasActiveHazardSymptom()))
            {
                __instance.m_state.m_collapseProcedure = null;
                __instance.m_state.m_collapseSymptom = null;

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, error: reset a collapse still triggered with no critical symptoms left");
            }

            if ((__instance.m_state.m_patientState != PatientState.Spawned) && (__instance.m_state.m_patientState != PatientState.ReservingAmbulance))
            {
                MedicalConditionChange medicalConditionChange = __instance.m_state.m_medicalCondition.Update(deltaTime, __instance.m_entity);
                if ((medicalConditionChange == MedicalConditionChange.SymptomActivated) && __instance.GetComponent<HospitalizationComponent>().IsHospitalized())
                {
                    __instance.GetComponent<HospitalizationComponent>().ActivatedSymptomCheck(deltaTime);
                }
            }

            __instance.m_state.m_timeInState += deltaTime;

            //Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, state {__instance.m_state.m_patientState}, visit time {__instance.m_state.m_fVisitTime.ToString(CultureInfo.InvariantCulture)}");

            // check patient waiting time
            switch (__instance.m_state.m_patientState)
            {
                case PatientState.Spawned:
                case PatientState.SpawnedFromDisabledInsurance:
                case PatientState.GoingToDoctor:
                case PatientState.BeingExamined:
                case PatientState.GoingToTreatment:
                case PatientState.BeingTreated:
                case PatientState.BlockedByNoTreatment:
                case PatientState.GoingToPharmacy:
                case PatientState.GoingToPharmacyChair:
                case PatientState.WaitingSittingInPharmacy:
                case PatientState.WaitingStandingInPharmacy:
                case PatientState.BuyingMedicine:
                case PatientState.Leaving:
                case PatientState.Left:
                case PatientState.OverriddenByHospitalization:
                case PatientState.ReservingAmbulance:
                case PatientState.WaitingForAmbulance:
                case PatientState.DeliveredByAmbulance:
                case PatientState.GoingToCollapse:
                case PatientState.Collapsing:
                case PatientState.Dead:
                    // do not check patient waititng time
                    break;

                case PatientState.Idle:
                case PatientState.GoingToReception:
                case PatientState.GoingToReceptionist:
                case PatientState.ExaminedAtReception:
                case PatientState.FindQueueMachine:
                case PatientState.GoToQueueMachine:
                case PatientState.UseQueueMachine:
                case PatientState.Wandering:
                case PatientState.GoingToWaitingRoom:
                case PatientState.WaitingGoingToChair:
                case PatientState.WaitingSitting:
                case PatientState.WaitingStandingIdle:
                case PatientState.WaitingBeingCalled:
                case PatientState.FulfillingNeeds:
                case PatientState.BlockedByAmbiguousResults:
                case PatientState.BlockedByComplicatedDiagnosis:
                case PatientState.GoingToStatLab:
                default:
                    {
                        // check patient waiting time
                        float secondsToLeave = DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Vanilla.PatientLeaveTimeHours() - Tweakable.Vanilla.PatientLeaveWarningHours());

                        if ((__instance.m_state.m_fVisitDuration < secondsToLeave) && ((__instance.m_state.m_fVisitDuration + deltaTime) >= secondsToLeave))
                        {
                            NotificationManager.GetInstance().AddMessage(__instance.m_entity, Notifications.Vanilla.NOTIF_PATIENT_LONG_VISIT, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                        }

                        __instance.m_state.m_fVisitDuration += deltaTime;
                    }
                    break;
            }

            __instance.SetOddFrame(!__instance.GetOddFrame());

            //if ((__instance.m_state.m_patientState == PatientState.WaitingSittingInPharmacy) 
            //    || (__instance.m_state.m_patientState == PatientState.WaitingStandingInPharmacy) 
            //    || (__instance.m_state.m_patientState == PatientState.BuyingMedicine))
            //{
            //    Hospital.Instance.m_currentHospitalStatus.m_patientsWaitingForPharmacy++;
            //}

            if ((!__instance.GetOddFrame()) && SettingsManager.Instance.m_gameSettings.m_scatteredUpdate)
            {
                return false;
            }

            if (__instance.m_examinationsDirty)
            {
                __instance.GetComponent<ProcedureComponent>().UpdateAllExaminationsForMedicalCondition(__instance.m_state.m_medicalCondition, -1, false);
                __instance.m_examinationsDirty = false;
            }

            bool activePatient = true;

            switch (__instance.m_state.m_patientState)
            {
                case PatientState.Spawned:
                    {
                        __instance.UpdateStateSpawnedInternal();
                        activePatient = false;
                    }
                    break;
                case PatientState.SpawnedFromDisabledInsurance:
                    {
                        activePatient = false;
                        __instance.m_state.m_hidden = true;
                    }
                    break;
                case PatientState.Idle:
                    {
                        __instance.UpdateStateIdleInternal();
                    }
                    break;
                case PatientState.GoingToReception:
                    {
                        __instance.UpdateStateGoingToReceptionInternal();
                    }
                    break;
                case PatientState.GoingToReceptionist:
                    {
                        __instance.UpdateStateGoingToReceptionistInternal();
                    }
                    break;
                case PatientState.ExaminedAtReception:
                    {
                        __instance.UpdateStateExaminedAtReceptionInternal();
                    }
                    break;
                case PatientState.FindQueueMachine:
                    {
                        __instance.Leave(false, false, false);
                    }
                    break;
                case PatientState.GoToQueueMachine:
                    break;
                case PatientState.UseQueueMachine:
                    break;
                case PatientState.Wandering:
                    break;
                case PatientState.GoingToWaitingRoom:
                    {
                        __instance.UpdateStateGoingToWaitingRoomInternal();
                    }
                    break;
                case PatientState.WaitingGoingToChair:
                    {
                        __instance.UpdateStateGoingToChairInternal();
                    }
                    break;
                case PatientState.WaitingSitting:
                    break;
                case PatientState.WaitingStandingIdle:
                    break;
                case PatientState.WaitingBeingCalled:
                    break;
                case PatientState.FulfillingNeeds:
                    {
                        __instance.UpdateStateFulfillingNeedsInternal();
                    }
                    break;
                case PatientState.GoingToDoctor:
                    break;
                case PatientState.BeingExamined:
                    break;
                case PatientState.GoingToTreatment:
                    break;
                case PatientState.BeingTreated:
                    break;
                case PatientState.BlockedByAmbiguousResults:
                    break;
                case PatientState.BlockedByComplicatedDiagnosis:
                    break;
                case PatientState.BlockedByNoTreatment:
                    break;
                case PatientState.GoingToPharmacy:
                    break;
                case PatientState.GoingToPharmacyChair:
                    break;
                case PatientState.WaitingSittingInPharmacy:
                    break;
                case PatientState.WaitingStandingInPharmacy:
                    break;
                case PatientState.BuyingMedicine:
                    break;
                case PatientState.Leaving:
                    {
                        __instance.UpdateStateLeavingInternal();
                    }
                    break;
                case PatientState.Left:
                    {
                        __instance.UpdateStateLeftInternal();
                        activePatient = false;
                    }
                    break;
                case PatientState.OverriddenByHospitalization:
                    break;
                case PatientState.GoingToStatLab:
                    break;
                case PatientState.ReservingAmbulance:
                    break;
                case PatientState.WaitingForAmbulance:
                    break;
                case PatientState.DeliveredByAmbulance:
                    break;
                case PatientState.GoingToCollapse:
                    break;
                case PatientState.Collapsing:
                    break;
                case PatientState.Dead:
                    break;
                default:
                    {
                        activePatient = false;
                    }
                    break;
            }

            if (activePatient)
            {
                Hospital.Instance.m_currentHospitalStatus.m_hadAnyActiveCharactersThisFrame = true;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateExaminedAtReception")]
        public static bool UpdateStateExaminedAtReceptionPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.GetComponent<ProcedureComponent>().IsBusy())
            {
                __instance.EnsureDepartmentInternal();

                if ((__instance.m_state.m_nurse != null) && (__instance.m_state.m_nurse.GetEntity() != null) && (__instance.m_state.m_nurse.GetEntity().GetComponent<BehaviorNurse>().CurrentPatient == __instance.m_entity))
                {
                    // free nurse
                    __instance.m_state.m_nurse.GetEntity().GetComponent<BehaviorNurse>().CurrentPatient = null;
                }

                __instance.m_state.m_nurse = null;
                __instance.m_state.m_finishedAtReception = true;
                
                if (__instance.m_state.m_department.GetEntity().m_departmentPersistentData.m_allPatientsControlled)
                {
                    __instance.SetPlayerControl();
                }

                __instance.SwitchState(PatientState.Idle);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateFulfillingNeeds")]
        public static bool UpdateStateFulfillingNeedsPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            // need to fulfill some needs
            // check if not fulfilling

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
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

                    if (!__instance.GetComponent<ProcedureComponent>().IsBusy())
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, fulfilling need finished");

                        if (!BehaviorPatientPatch.HandleDiedSentHomeFulfillNeeds(__instance))
                        {
                            // nothing to do
                            __instance.SwitchState(PatientState.Idle);
                        }
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateGoingToChair")]
        public static bool UpdateStateGoingToChairPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                Room currentRoom = MapScriptInterface.Instance.GetRoomAt(__instance.GetComponent<WalkComponent>());

                if (currentRoom.m_roomPersistentData.m_roomType == Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.Reception))
                {
                    __instance.SwitchState(PatientState.Idle);
                }

                // ???
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateGoingToReception")]
        public static bool UpdateStateGoingToReceptionPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if ((!__instance.GetComponent<WalkComponent>().IsBusy()) || MapScriptInterfacePatch.IsInDestinationRoom(__instance.m_entity))
            {
                __instance.GetComponent<WalkComponent>().Stop();
                __instance.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
                __instance.SwitchState(PatientState.Idle);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateGoingToReceptionist")]
        public static bool UpdateStateGoingToReceptionistPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                if ((__instance.m_state.m_nurse == null) || (__instance.m_state.m_nurse.GetEntity() == null))
                {
                    __instance.SwitchState(PatientState.Idle);
                }
                else if (__instance.m_state.m_nurse.GetEntity().GetComponent<BehaviorNurse>().m_state.m_nurseState == NurseState.Idle)
                {
                    // nurse is "idle"
                    GameDBExamination examination = Database.Instance.GetEntry<GameDBExamination>(Examinations.Vanilla.Reception);

                    __instance.GetComponent<ProcedureComponent>().StartExamination(
                        examination, __instance.m_entity, null, __instance.m_state.m_nurse.GetEntity(), null, AccessRights.PATIENT_PROCEDURE, EquipmentListRules.ONLY_FREE);
                    __instance.m_state.m_nurse.GetEntity().GetComponent<EmployeeComponent>().SetReserved(Examinations.Vanilla.Reception, __instance.m_entity);
                    __instance.SwitchState(PatientState.ExaminedAtReception);
                }
                else if (__instance.m_state.m_nurse.GetEntity().GetComponent<BehaviorNurse>().m_state.m_nurseState == NurseState.GoingToWorkplace)
                {
                    // nurse is going to workplace
                    // just wait
                }
                else
                {
                    // nurse is doing something else
                    // release nurse and return to queue

                    if ((__instance.m_state.m_nurse != null) && (__instance.m_state.m_nurse.GetEntity() != null) && (__instance.m_state.m_nurse.GetEntity().GetComponent<BehaviorNurse>().CurrentPatient == __instance.m_entity))
                    {
                        // free nurse
                        __instance.m_state.m_nurse.GetEntity().GetComponent<BehaviorNurse>().CurrentPatient = null;
                    }

                    __instance.m_state.m_nurse = null;

                    __instance.SwitchState(PatientState.Idle);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateGoingToWaitingRoom")]
        public static bool UpdateStateGoingToWaitingRoomPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            //if (__instance.GetComponent<WalkComponent>().IsBusy())
            //{
            //    Room room = MapScriptInterface.Instance.GetRoomAt(__instance.GetComponent<WalkComponent>().GetDestinationTile(), __instance.GetComponent<WalkComponent>().GetFloorIndex());
                
            //    if ((room == null) || ((room.m_roomPersistentData.m_valid & RoomValidity.INACCESSIBLE_PATIENTS) != 0))
            //    {
            //        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, destination waiting room not accessible");

            //        __instance.FreeWaitingRoom();
            //        __instance.SwitchState(PatientState.Idle);

            //        return false;
            //    }
            //}

            //if (!__instance.GetComponent<WalkComponent>().IsBusy())
            //{
            //    if (!BehaviorPatientPatch.HandleDiedSentHomeFulfillNeeds(__instance))
            //    {
            //        if (__instance.m_state.m_waitingRoom == null)
            //        {
            //            __instance.m_state.m_waitingRoom = MapScriptInterface.Instance.GetRoomAt(__instance.GetComponent<WalkComponent>());
            //            __instance.m_state.m_waitingRoom.GetEntity().EnqueueCharacter(__instance.m_entity, true);
            //        }
                    
            //        __instance.CheckRoomSatisfactionBonusesInternal();

            //        if ((__instance.m_state.m_waitingRoom != null) && (__instance.m_state.m_reservedWaitingRoomTile == null))
            //        {
            //            // reserve some tile to stand and move
            //            Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(__instance.m_state.m_waitingRoom.GetEntity(), AccessRights.PATIENT);

            //            if (position != Vector2i.ZERO_VECTOR)
            //            {
            //                __instance.m_state.m_reservedWaitingRoomTile = position;
            //                __instance.GetComponent<WalkComponent>().SetDestination(position, __instance.GetComponent<WalkComponent>().GetFloorIndex(), MovementType.WALKING);
            //                MapScriptInterface.Instance.ReserveTile(position, __instance.m_entity, __instance.GetComponent<WalkComponent>().GetFloorIndex());
            //            }
            //        }

            //        if ((__instance.m_state.m_waitingRoom != null) && (__instance.m_state.m_reservedWaitingRoomTile != null))
            //        {
            //            // finally on standing place
            //            __instance.SwitchState(PatientState.WaitingStandingIdle);
            //        }
            //    }
            //}

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateIdle")]
        private static bool UpdateStateIdlePrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            // "Idle" state can be:
            // - after spawning patient
            // - after coming to reception
            // - waiting in reception

            if (!BehaviorPatientPatch.HandleDiedSentHome(__instance))
            {
                if (!__instance.GetComponent<WalkComponent>().IsBusy())
                {
                    if (!__instance.m_state.m_finishedAtReception)
                    {
                        // patient is standing somewhere
                        // it can be after spawning or in reception

                        Room currentRoom = MapScriptInterface.Instance.GetRoomAt(__instance.GetComponent<WalkComponent>());

                        if ((currentRoom == null)
                            || (currentRoom.m_roomPersistentData.m_roomType != Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.Reception)))
                        {
                            // we are spawned
                            // go to reception or waiting room

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, department {__instance.GetDepartment().m_departmentPersistentData.m_departmentType.m_id}");

                            Room room = MapScriptInterface.Instance.FindValidRoomWithType(Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.Reception), __instance.GetDepartment());

                            if (room != null)
                            {
                                Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(room, __instance.GetAccessRights());

                                if (position != Vector2i.ZERO_VECTOR)
                                {
                                    __instance.GetComponent<WalkComponent>().SetDestination(position, room.GetFloorIndex(), MovementType.WALKING);

                                    __instance.SwitchState(PatientState.GoingToReception);
                                }
                            }
                            else
                            {
                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no reception");

                                room = MapScriptInterface.Instance.FindValidRoomWithType(Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.WaitingRoom), __instance.GetDepartment());

                                if (room != null)
                                {
                                    Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(room, __instance.GetAccessRights());

                                    if (position != Vector2i.ZERO_VECTOR)
                                    {
                                        __instance.GetComponent<WalkComponent>().SetDestination(position, room.GetFloorIndex(), MovementType.WALKING);

                                        __instance.SwitchState(PatientState.GoingToWaitingRoom);
                                    }
                                }
                                else
                                {
                                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no waiting room");

                                    __instance.Leave(false, false, false);
                                }
                            }

                            return false;
                        }
                        else
                        {
                            if (!BehaviorPatientPatch.HandleDiedSentHomeFulfillNeeds(__instance))
                            {
                                // we are on reception
                                // check if there is receptionist

                                List<Entity> receptionists =
                                    MapScriptInterfacePatch.FindNursesAssignedToRoom(
                                        currentRoom, false,
                                        __instance.m_state.m_department.GetEntity(),
                                        Database.Instance.GetEntry<GameDBEmployeeRole>(EmployeeRoles.Vanilla.Receptionist),
                                        Database.Instance.GetEntry<GameDBSkill>(Skills.Vanilla.SKILL_NURSE_SPEC_RECEPTIONIST));

                                if (receptionists.Count != 0)
                                {
                                    if ((__instance.m_state.m_waitingRoom == null) || (__instance.m_state.m_waitingRoom.GetEntity() == null))
                                    {
                                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no reception set");

                                        __instance.m_state.m_waitingRoom = currentRoom;
                                        __instance.m_state.m_waitingRoom.GetEntity().EnqueueCharacter(__instance.m_entity, true);
                                    }

                                    if (__instance.GetComponent<WalkComponent>().IsSitting())
                                    {
                                        __instance.m_state.m_chair = __instance.GetComponent<WalkComponent>().m_state.m_objectSittingOn;
                                    }

                                    if (__instance.m_state.m_waitingRoom.GetEntity().IsCharactersTurn(__instance.m_entity))
                                    {
                                        Entity receptionist = receptionists
                                            .Where(r => r.GetComponent<BehaviorNurse>().IsFree())
                                            .OrderBy(r =>
                                            {
                                                Vector2i receptionistPosition = r.GetComponent<BehaviorNurse>().GetInteractionPosition();
                                                Vector2i entityPosition = __instance.GetComponent<WalkComponent>().GetCurrentTile();

                                                return (receptionistPosition - entityPosition).LengthSquared();
                                            }).
                                            FirstOrDefault();

                                        if (receptionist != null)
                                        {
                                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, on turn, going to receptionist {receptionist.Name}");

                                            Vector2i interactionPosition = receptionist.GetComponent<BehaviorNurse>().GetInteractionPosition();

                                            __instance.GetComponent<WalkComponent>().SetDestination(interactionPosition, receptionist.GetComponent<WalkComponent>().GetFloorIndex(), MovementType.WALKING);

                                            __instance.FreeWaitingRoom();

                                            __instance.m_state.m_nurse = receptionist;

                                            // reserve nurse by patient
                                            receptionist.GetComponent<BehaviorNurse>().CurrentPatient = __instance.m_entity;

                                            __instance.SwitchState(PatientState.GoingToReceptionist);
                                            return false;
                                        }
                                    }

                                    if (__instance.m_state.m_chair == null)
                                    {
                                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no chair");

                                        TileObject chairObject = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                                            __instance.m_entity, null, __instance.GetComponent<WalkComponent>().GetCurrentTile(),
                                            __instance.m_state.m_waitingRoom.GetEntity(), Tags.Vanilla.Sitting, AccessRights.PATIENT, false, null, false);

                                        if (__instance.TryToSitInternal(chairObject, false))
                                        {
                                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, found place to sit");

                                            if (__instance.m_state.m_reservedWaitingRoomTile != Vector2i.ZERO_VECTOR)
                                            {
                                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, freeing reserved stand tile");

                                                MapScriptInterface.Instance.FreeTile(__instance.m_state.m_reservedWaitingRoomTile, __instance.GetComponent<WalkComponent>().GetFloorIndex());
                                                __instance.m_state.m_reservedWaitingRoomTile = Vector2i.ZERO_VECTOR;
                                            }
                                        }
                                        else if (__instance.m_state.m_reservedWaitingRoomTile == Vector2i.ZERO_VECTOR)
                                        {
                                            // no reserved stand tile and no chair

                                            if (!__instance.TryToStandInternal(false, false))
                                            {
                                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, not found place to stand");

                                                if (__instance.m_state.m_waitingRoom != null)
                                                {
                                                    __instance.m_state.m_waitingRoom.GetEntity().DequeueCharacter(__instance.m_entity);
                                                }

                                                Room room = MapScriptInterface.Instance.FindValidRoomWithType(Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.WaitingRoom), __instance.GetDepartment());

                                                if (room != null)
                                                {
                                                    Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(room, __instance.GetAccessRights());

                                                    if (position != Vector2i.ZERO_VECTOR)
                                                    {
                                                        __instance.GetComponent<WalkComponent>().SetDestination(position, room.GetFloorIndex(), MovementType.WALKING);

                                                        __instance.SwitchState(PatientState.GoingToWaitingRoom);
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no waiting room");

                                                    __instance.Leave(false, false, false);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no receptionist, search for waiting room");

                                    Room room = MapScriptInterface.Instance.FindValidRoomWithType(Database.Instance.GetEntry<GameDBRoomType>(RoomTypes.Vanilla.WaitingRoom), __instance.GetDepartment());

                                    if (room != null)
                                    {
                                        Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(room, __instance.GetAccessRights());

                                        if (position != Vector2i.ZERO_VECTOR)
                                        {
                                            __instance.GetComponent<WalkComponent>().SetDestination(position, room.GetFloorIndex(), MovementType.WALKING);

                                            __instance.SwitchState(PatientState.GoingToWaitingRoom);
                                        }
                                    }
                                    else
                                    {
                                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no waiting room");

                                        __instance.Leave(false, false, false);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // we are done on reception
                        // send patient to search queue machine

                        __instance.SwitchState(PatientState.FindQueueMachine);
                    }
                }


                //    if (!__instance.IsInWaitingRoom())
                //    {
                //        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, is not in reception");

                //        if ((__instance.m_state.m_reservedWaitingRoomTile == Vector2i.ZERO_VECTOR) && __instance.TryToStandInternal(true, true))
                //        {
                //            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, going to reception");

                //            __instance.SwitchState(PatientState.GoingToReception);
                //            return false;
                //        }

                //        //if (!receptionist.GetComponent<WalkComponent>().IsBusy())


                //__instance.GoToWaitingRoom();

                //if (!__instance.m_state.m_finishedAtReception)
                //{
                //    __instance.CheckReceptionInternal();
                //}
                //else if (__instance.m_state.m_usedQueueMachine)
                //{
                //    __instance.GoToWaitingRoom();
                //}
                //else
                //{
                //    __instance.SwitchState(PatientState.FindQueueMachine);
                //}
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateLeaving")]
        public static bool UpdateStateLeavingPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                __instance.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
                __instance.GetComponent<SpeechComponent>().HideBubble();
                __instance.m_state.m_hidden = true;
                __instance.m_state.m_eventState = PatientEventState.HOSPITAL_FINISHED;
                __instance.SwitchState(PatientState.Left);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateSpawned")]
        public static bool UpdateStateSpawnedPrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __instance.m_state.m_hidden = true;

            if ((DayTime.Instance.GetDayTimeHours() > __instance.m_state.m_fVisitTime) && (DayTime.Instance.GetDayTimeHours() < (__instance.m_state.m_fVisitTime + 0.1f)))
            {
                if (!BehaviorPatientPatch.HandleDiedSentHome(__instance))
                {
                    InsuranceCompany insuranceCompany = InsuranceManager.Instance.GetInsuranceCompany(__instance.GetComponent<CharacterPersonalInfoComponent>().m_personalInfo.m_insuranceCompany.Entry);
                    if ((!__instance.m_state.m_fromLevel) && (!insuranceCompany.m_isContracted))
                    {
                        __instance.SwitchState(PatientState.SpawnedFromDisabledInsurance);
                        return false;
                    }

                    __instance.m_state.m_hidden = false;
                    __instance.m_state.m_sentHome = false;
                    __instance.m_state.m_fVisitDuration = 0f;

                    __instance.GetComponent<WalkComponent>().Floor = Hospital.Instance.GetGroundFloor();
                    Vector2i randomSpawnPosition = MapScriptInterface.Instance.GetRandomSpawnPosition();
                    __instance.GetComponent<WalkComponent>().ForceWorldPosition((float)randomSpawnPosition.m_x, (float)randomSpawnPosition.m_y);
                    __instance.GetComponent<MoodComponent>().m_state.m_needs = NeedsFactory.CreatePatientNeeds();
                    __instance.GetComponent<MoodComponent>().ResetSatisfaction();
                    __instance.GetComponent<MoodComponent>().UpdateSymptomDiscomfortModifiers();
                    __instance.GetComponent<MoodComponent>().UpdateTotalSatisfaction();

                    if ((__instance.m_state.m_medicalCondition.m_wrongDiagnoses != null) && (__instance.m_state.m_medicalCondition.m_wrongDiagnoses.Count > 0))
                    {
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, Notifications.Vanilla.NOTIF_WRONG_DIAGNOSIS_RETURNS, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                    }

                    __instance.m_state.m_reservedWaitingRoomTile = Vector2i.ZERO_VECTOR;
                    __instance.m_state.m_medicalCondition.ResetCollapseTimes(__instance);

                    if (__instance.m_state.m_medicalCondition.HasImmobileSymptom())
                    {
                        if ((!__instance.m_state.m_fromLevel) && ((!insuranceCompany.m_isContracted) || (insuranceCompany.m_immobileSpawnedToday == 0)))
                        {
                            __instance.m_state.m_sentAway = true;

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, is immobile, sending home");

                            __instance.SwitchState(PatientState.SpawnedFromDisabledInsurance);
                            __instance.GetComponent<AnimModelComponent>().FadeOut(0f);

                            return false;
                        }

                        __instance.m_state.m_sentAway = false;
                        __instance.GetComponent<AnimModelComponent>().FadeIn(1f);
                        insuranceCompany.OnPatientSpawned(__instance.m_entity);
                        __instance.SwitchState(PatientState.ReservingAmbulance);
                    }
                    else
                    {
                        Department departmentOfType = MapScriptInterface.Instance.GetDepartmentOfType(__instance.m_entity.GetComponent<BehaviorPatient>().BelongsToDepartment());

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, department {departmentOfType.m_departmentPersistentData.m_departmentType.m_id}, referal {__instance.m_state.m_fromReferral}");

                        if (__instance.m_state.m_fromReferral && (departmentOfType != null) && (departmentOfType.GetDepartmentType() != Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Emergency)))
                        {
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, changing to department {departmentOfType.m_departmentPersistentData.m_departmentType.m_id}");

                            __instance.ChangeDepartment(departmentOfType, false, false, HospitalizationLevel.NONE);
                        }

                        __instance.m_state.m_sentAway = false;
                        __instance.m_state.m_waitingRoom = null;
                        __instance.m_state.m_finishedAtReception = false;
                        __instance.m_state.m_usedQueueMachine = false;
                        __instance.GetComponent<AnimModelComponent>().FadeIn(1f);
                        insuranceCompany.OnPatientSpawned(__instance.m_entity);
                        __instance.SwitchState(PatientState.Idle);
                    }
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateWaitingStandingIdle")]
        public static bool UpdateStateWaitingStandingIdlePrefix(float deltaTime, BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __instance.m_state.m_waitingTime += deltaTime;
            __instance.m_state.m_continuousWaitingTime += deltaTime;

            if (!BehaviorPatientPatch.HandleDiedSentHome(__instance))
            {
                if ((__instance.m_state.m_collapseProcedure != null) && __instance.CanCollapse() && __instance.TryToCollapse())
                {
                    return false;
                }

                if ((__instance.m_state.m_collapseSymptom != null) && __instance.SetupCollapseProcedure(true))
                {
                    return false;
                }

                if (!BehaviorPatientPatch.HandleDiedSentHomeFulfillNeeds(__instance))
                {
                    // do nothing at current momment

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, nothing to do");

                    if (__instance.GetComponent<AnimModelComponent>().IsIdle())
                    {
                        __instance.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, false);
                    }
                }
            }

            return false;
        }

        public static bool HandleDied(BehaviorPatient instance)
        {
            if (instance.m_state.m_deathTriggered)
            {
                instance.Die();

                return true;
            }

            return false;
        }

        public static bool HandleSentHome(BehaviorPatient instance)
        {
            if (instance.m_state.m_sentHome)
            {
                instance.Leave(instance.HasBeenTreated(), false, false);

                return true;
            }

            if ((!DayTime.Instance.IsOpenForPatients()) || (instance.GetDepartment() == null) || instance.GetDepartment().IsClosed())
            {
                instance.LeaveAfterClosingHoursInternal();

                return true;
            }

            if ((instance.m_state.m_department != null)  && (!instance.m_state.m_department.CheckEntity()))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, is assigned to a deleted department");

                instance.m_state.m_department = null;

                if (!instance.EnsureDepartmentInternal())
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, no working emergency, sending patient home");

                    instance.Leave(false, false, false);

                    return true;
                }
            }

            if (instance.m_state.m_department.GetEntity().GetDepartmentType() == Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Default))
            {
                if (Application.isEditor)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, is assigned to a {Departments.Vanilla.Default} department");
                }

                instance.m_state.m_department.GetEntity().RemovePatient(instance.m_entity);
                instance.m_state.m_department = null;

                if (!instance.EnsureDepartmentInternal())
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, no working emergency, sending patient home");

                    instance.Leave(false, false, false);

                    return true;
                }
            }

            if ((instance.m_state.m_department != null)
                && (instance.m_state.m_department.GetEntity().GetDepartmentType() == Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Emergency))
                && (!instance.EnsureDepartmentInternal()))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, no working emergency, sending patient home");

                instance.Leave(false, false, false);

                return true;
            }

            if ((instance.m_state.m_department == null) || (!instance.m_state.m_department.GetEntity().AcceptsOutpatients()))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, department {instance.m_state.m_department.GetEntity()?.Name ?? "NULL"} no working, sending patient home");

                NotificationManager.GetInstance().AddMessage(instance.m_entity, Notifications.Vanilla.NOTIF_DEPARTMENT_NOT_WORKING, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, null, null);

                instance.Leave(false, false, false);
                return true;
            }

            return false;
        }

        public static bool HandleDiedSentHome(BehaviorPatient instance)
        {
            if (!BehaviorPatientPatch.HandleDied(instance))
            {
                if (!BehaviorPatientPatch.HandleSentHome(instance))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool HandleDiedSentHomeFulfillNeeds(BehaviorPatient instance)
        {
            // check if patient died or needs to go home
            if (BehaviorPatientPatch.HandleDiedSentHome(instance))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, died or sent home");

                return true;
            }

            // check if patient needs to fulfill his/her needs
            if (BehaviorPatientPatch.IsNeededHandleFullfillNeeds(instance))
            {
                instance.FreeWaitingRoom();

                instance.m_entity.GetComponent<SpeechComponent>().HideBubble();
                instance.SwitchState(PatientState.FulfillingNeeds);

                return true;
            }

            return false;
        }

        private static bool IsNeededHandleFullfillNeeds(BehaviorPatient instance)
        {
            if (instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript != null)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, planned procedure {instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().m_stateData.m_scriptName}");
                return true;
            }

            List<Need> needsSortedFromMostCritical = instance.GetComponent<MoodComponent>().GetNeedsSortedFromMostCritical();
            foreach (Need need in needsSortedFromMostCritical)
            {
                if ((need.m_currentValue > UnityEngine.Random.Range(Tweakable.Mod.FulfillNeedsThreshold(), Needs.NeedMaximum)) || (need.m_currentValue > Tweakable.Mod.FulfillNeedsThresholdCritical()))
                {
                    if (instance.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(need.m_gameDBNeed.Entry.Procedure, instance.m_entity, instance.GetDepartment(), AccessRights.PATIENT, EquipmentListRules.ONLY_FREE_SAME_FLOOR_PREFER_DPT) == ProcedureSceneAvailability.AVAILABLE)
                    {
                        if (instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript == null)
                        {
                            instance.GetComponent<ProcedureComponent>().StartProcedure(need.m_gameDBNeed.Entry.Procedure, instance.m_entity, instance.GetDepartment(), AccessRights.PATIENT, EquipmentListRules.ONLY_FREE_SAME_FLOOR_PREFER_DPT);
                            instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().SwitchState(ProcedureScriptPatch.STATE_RESERVED);

                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, planning fulfilling need {need.m_gameDBNeed.Entry.DatabaseID}, need value {need.m_currentValue.ToString(CultureInfo.InvariantCulture)}");
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool GetOddFrame(this BehaviorPatient instance)
        {
            // Get the Type of the class
            Type type = typeof(BehaviorPatient);

            // Get the private field using BindingFlags
            FieldInfo m_oddFrameFieldInfo = type.GetField("m_oddFrame", BindingFlags.NonPublic | BindingFlags.Instance);

            // get objects
            return (bool)m_oddFrameFieldInfo.GetValue(instance);
        }

        private static void SetOddFrame(this BehaviorPatient instance, bool value)
        {
            // Get the Type of the class
            Type type = typeof(BehaviorPatient);

            // Get the private field using BindingFlags
            FieldInfo m_oddFrameFieldInfo = type.GetField("m_oddFrame", BindingFlags.NonPublic | BindingFlags.Instance);

            // set objects
            m_oddFrameFieldInfo.SetValue(instance, value);
        }

        private static void CheckRoomSatisfactionBonusesInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("CheckRoomSatisfactionBonuses", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static bool EnsureDepartmentInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("EnsureDepartment", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static bool EnsureWaitingRoomInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("EnsureWaitingRoom", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static void GoToEmergencyInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("GoToEmergency", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void LeaveAfterClosingHoursInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("LeaveAfterClosingHours", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void ReportMissingWaitingRoomInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("ReportMissingWaitingRoom", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static bool TryToSitInternal(this BehaviorPatient instance, TileObject chairObject, bool pharmacyChair)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("TryToSit", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, new object[] { chairObject, pharmacyChair });
        }

        private static bool TryToStandInternal(this BehaviorPatient instance, bool tryAreasAround, bool onlyInFront)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("TryToStand", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, new object[] { tryAreasAround, onlyInFront });
        }

        private static void UpdateStateExaminedAtReceptionInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("UpdateStateExaminedAtReception", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void UpdateStateFulfillingNeedsInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("UpdateStateFulfillingNeeds", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void UpdateStateGoingToChairInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("UpdateStateGoingToChair", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void UpdateStateGoingToReceptionInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("UpdateStateGoingToReception", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void UpdateStateGoingToReceptionistInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("UpdateStateGoingToReceptionist", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void UpdateStateGoingToWaitingRoomInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("UpdateStateGoingToWaitingRoom", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void UpdateStateIdleInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("UpdateStateIdle", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void UpdateStateLeavingInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("UpdateStateLeaving", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void UpdateStateLeftInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("UpdateStateLeft", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }

        private static void UpdateStateSpawnedInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("UpdateStateSpawned", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, null);
        }
    }
}
