using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using ModAdvancedGameChanges.Helpers;
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
            Room room = MapScriptInterface.Instance.GetRoomAt(__instance.GetComponent<WalkComponent>());

            if ((room == null) || (room.m_roomPersistentData.m_roomType.Entry != waitingRoomType))
            {
                room = MapScriptInterface.Instance.FindLeastFullRoomAroundOutpatientOffice(waitingRoomType, __instance.m_state.m_department.GetEntity());
            }

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
        [HarmonyPatch(typeof(BehaviorPatient), "GetCalled")]
        public static bool GetCalledPrefix(Entity doctorEntity, BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Department doctorsDepartment = doctorEntity.GetComponent<EmployeeComponent>().m_state.m_department.GetEntity();

            __instance.m_state.m_continuousWaitingTime = 0f;
            int queueMachineCount = __instance.m_state.m_waitingRoom.GetEntity().GetObjectCountWithTag(Tags.Vanilla.Queue, __instance.GetComponent<WalkComponent>().Floor, true);
            int queueMonitorCount = __instance.m_state.m_waitingRoom.GetEntity().GetObjectCountWithTag(Tags.Vanilla.QueueMonitor, __instance.GetComponent<WalkComponent>().Floor, true);

            if ((queueMachineCount > 0) && (queueMonitorCount > 0))
            {
                GameDBProcedure procedure = Database.Instance.GetEntry<GameDBProcedure>(Procedures.Vanilla.CallPatientWithQueueMonitor);
                ProcedureSceneAvailability procedureAvailabilty = __instance.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(
                    procedure, __instance.m_entity, doctorEntity, AccessRights.PATIENT_PROCEDURE,
                    doctorEntity.GetComponent<EmployeeComponent>().m_state.m_homeRoom.GetEntity(), EquipmentListRules.ONLY_FREE);

                if (procedureAvailabilty == ProcedureSceneAvailability.AVAILABLE)
                {
                    BehaviorDoctor component = doctorEntity.GetComponent<BehaviorDoctor>();
                    component.CurrentPatient = __instance.m_entity;

                    __instance.GetComponent<ProcedureComponent>().StartProcedure(procedure, __instance.m_entity, doctorEntity, null, null, AccessRights.PATIENT_PROCEDURE, EquipmentListRules.ONLY_FREE);

                    // set doctor, because doctor is not set in start procedure
                    __instance.GetComponent<ProcedureComponent>().m_state.m_currentProcedureScript.GetEntity().m_stateData.m_procedureScene.Doctor = doctorEntity;
                    __instance.m_state.m_doctor.GetEntity().GetComponent<EmployeeComponent>().SetReserved(Procedures.Vanilla.CallPatientWithQueueMonitor, __instance.m_entity);

                    __instance.SwitchState(PatientState.WaitingBeingCalled);
                }
                else if (procedureAvailabilty == ProcedureSceneAvailability.STAFF_UNAVAILABLE)
                {
                    __instance.FreeWaitingRoom();
                    __instance.Leave(false, false, false);
                }
            }
            else
            {
                GameDBProcedure procedure = Database.Instance.GetEntry<GameDBProcedure>(Procedures.Vanilla.CallPatientWithoutQueueMonitor);
                ProcedureSceneAvailability procedureAvailabilty = __instance.GetComponent<ProcedureComponent>().GetProcedureAvailabilty(
                    procedure, __instance.m_entity, doctorEntity, AccessRights.PATIENT_PROCEDURE, 
                    doctorEntity.GetComponent<EmployeeComponent>().m_state.m_homeRoom.GetEntity(), EquipmentListRules.ONLY_FREE);

                if (procedureAvailabilty == ProcedureSceneAvailability.AVAILABLE)
                {
                    BehaviorDoctor component = doctorEntity.GetComponent<BehaviorDoctor>();
                    component.CurrentPatient = __instance.m_entity;

                    __instance.GetComponent<ProcedureComponent>().StartProcedure(procedure, __instance.m_entity, doctorEntity, null, null, AccessRights.PATIENT_PROCEDURE, EquipmentListRules.ONLY_FREE);
                    __instance.m_state.m_doctor.GetEntity().GetComponent<EmployeeComponent>().SetReserved(Procedures.Vanilla.CallPatientWithoutQueueMonitor, __instance.m_entity);

                    __instance.SwitchState(PatientState.WaitingBeingCalled);
                }
                else if (procedureAvailabilty == ProcedureSceneAvailability.STAFF_UNAVAILABLE)
                {
                    __instance.FreeWaitingRoom();
                    __instance.Leave(false, false, false);
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "GoToOffice")]
        public static bool GoToOffice(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __instance.FreeWaitingRoom();

            Entity doctor = __instance.m_state.m_doctor.GetEntity();
            EntityIDPointer<Room> homeRoom = doctor.GetComponent<EmployeeComponent>().m_state.m_homeRoom;
            Vector2i currentTile = doctor.GetComponent<WalkComponent>().GetCurrentTile();
            TileObject tileObject = null;

            if (homeRoom != null)
            {
                tileObject = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(__instance.m_entity, null, currentTile, homeRoom.GetEntity(), Tags.Vanilla.Sitting, AccessRights.PATIENT_PROCEDURE, false, null, false);
            }

            if (tileObject != null)
            {
                __instance.GetComponent<WalkComponent>().GoSit(tileObject, MovementType.WALKING);
            }
            else
            {
                __instance.GetComponent<WalkComponent>().SetDestination(doctor.GetComponent<Behavior>().GetInteractionPosition(), doctor.GetComponent<WalkComponent>().GetFloorIndex(), MovementType.WALKING);
            }

            __instance.FreeWaitingRoom();
            __instance.SwitchState(PatientState.GoingToDoctor);

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

            if (!__instance.EnsureWaitingRoom())
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no waiting room, leaving");

                __instance.ReportMissingWaitingRoom();
                __instance.Leave(true, false, false);

                return false;
            }

            // in __instance.m_state.m_waitingRoom is waiting room
            // choose some place, start walking and reset waiting room

            Room room = MapScriptInterface.Instance.GetRoomAt(__instance.GetComponent<WalkComponent>());

            if ((room != null) && (room == __instance.m_state.m_waitingRoom.GetEntity()))
            {
                // nothing to do, we are in waiting room
                __instance.SwitchState(PatientState.WaitingStandingIdle);
            }
            else
            {
                Vector2i position = MapScriptInterface.Instance.GetRandomFreePosition(__instance.m_state.m_waitingRoom.GetEntity(), AccessRights.PATIENT);

                if (position != Vector2i.ZERO_VECTOR)
                {
                    __instance.GetComponent<WalkComponent>().SetDestination(position, __instance.m_state.m_waitingRoom.GetEntity().GetFloorIndex(), MovementType.WALKING);
                    __instance.SwitchState(PatientState.GoingToWaitingRoom);

                    __instance.FreeWaitingRoom();
                }
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
            __instance.UnreserveEmployees(true, true, true);

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

            __instance.m_state.m_doctor = null;
            __instance.m_state.m_nurse = null;
            __instance.m_state.m_labSpecialist = null;

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
                __instance.m_state.m_playerControlwaitingTime = 0f;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "TryToSit")]
        public static bool TryToSitPrefix(TileObject chairObject, bool pharmacyChair, BehaviorPatient __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __result = (chairObject == null) ? false : __instance.GetComponent<WalkComponent>().IsSittingOn(chairObject);
            __result |= __instance.GetComponent<WalkComponent>().IsSitting();

            if (!__result)
            {
                chairObject = (chairObject == null) ? MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                    __instance.m_entity, null, __instance.GetComponent<WalkComponent>().GetCurrentTile(),
                    __instance.m_state.m_waitingRoom.GetEntity(), Tags.Vanilla.Sitting, AccessRights.PATIENT, false, null, false) : chairObject;

                if (chairObject != null)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, found place to sit");

                    __instance.GetComponent<WalkComponent>().GoSit(chairObject, MovementType.WALKING);

                    if (__instance.m_state.m_reservedWaitingRoomTile != Vector2i.ZERO_VECTOR)
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, freeing reserved tile");

                        MapScriptInterface.Instance.FreeTile(__instance.m_state.m_reservedWaitingRoomTile, __instance.GetComponent<WalkComponent>().GetFloorIndex());
                        __instance.m_state.m_reservedWaitingRoomTile = Vector2i.ZERO_VECTOR;
                    }

                    __result = true;
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "TryToStand")]
        public static bool TryToStandPrefix(bool tryAreasAround, bool onlyInFront, BehaviorPatient __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __result = (__instance.m_state.m_reservedWaitingRoomTile != Vector2i.ZERO_VECTOR);

            if (!__result)
            {
                if (__instance.m_state.m_waitingRoom != null)
                {
                    Vector2i position = (onlyInFront) 
                        ? MapScriptInterface.Instance.GetRandomFreeFrontPosition(__instance.m_state.m_waitingRoom.GetEntity(), __instance.GetAccessRights())
                        : MapScriptInterface.Instance.GetRandomFreePosition(__instance.m_state.m_waitingRoom.GetEntity(), __instance.GetAccessRights());

                    if (position != Vector2i.ZERO_VECTOR)
                    {
                        MapScriptInterface.Instance.ReserveTile(position, __instance.m_entity, __instance.m_state.m_waitingRoom.GetEntity().GetFloorIndex());
                        __instance.m_state.m_reservedWaitingRoomTile = position;
                        __instance.GetComponent<WalkComponent>().SetDestination(position, __instance.m_state.m_waitingRoom.GetEntity().GetFloorIndex(), MovementType.WALKING);

                        __result = true;
                    }
                    else if (tryAreasAround)
                    {
                        position = MapScriptInterface.Instance.FindClosestTileWithFree3x3Area(__instance.m_state.m_waitingRoom.GetEntity().GetCenter().ToVector2i(false), __instance.m_state.m_waitingRoom.GetEntity().GetFloorIndex());

                        if (position != Vector2i.ZERO_VECTOR)
                        {
                            MapScriptInterface.Instance.ReserveTile(position, __instance.m_entity, __instance.m_state.m_waitingRoom.GetEntity().GetFloorIndex());
                            __instance.m_state.m_reservedWaitingRoomTile = position;
                            __instance.GetComponent<WalkComponent>().SetDestination(position, __instance.m_state.m_waitingRoom.GetEntity().GetFloorIndex(), MovementType.WALKING);

                            __result = true;
                        }
                    }
                }
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

            var oddFrameHelper = new PrivateFieldAccessHelper<BehaviorPatient, bool>("m_oddFrame", __instance);

            oddFrameHelper.Field = !oddFrameHelper.Field;

            //if ((__instance.m_state.m_patientState == PatientState.WaitingSittingInPharmacy) 
            //    || (__instance.m_state.m_patientState == PatientState.WaitingStandingInPharmacy) 
            //    || (__instance.m_state.m_patientState == PatientState.BuyingMedicine))
            //{
            //    Hospital.Instance.m_currentHospitalStatus.m_patientsWaitingForPharmacy++;
            //}

            if ((!oddFrameHelper.Field) && SettingsManager.Instance.m_gameSettings.m_scatteredUpdate)
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
                        __instance.UpdateStateSpawned();
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
                    __instance.UpdateStateIdle();
                    break;
                case PatientState.GoingToReception:
                    __instance.UpdateStateGoingToReception();
                    break;
                case PatientState.GoingToReceptionist:
                    __instance.UpdateStateGoingToReceptionist();
                    break;
                case PatientState.ExaminedAtReception:
                    __instance.UpdateStateExaminedAtReception();
                    break;
                case PatientState.FindQueueMachine:
                    __instance.UpdateStateFindQueueMachine();
                    break;
                case PatientState.GoToQueueMachine:
                    __instance.UpdateStateGoingToQueueMachine();
                    break;
                case PatientState.UseQueueMachine:
                    __instance.UpdateStateUsingQueueMachine();
                    break;
                case PatientState.Wandering:
                    break;
                case PatientState.GoingToWaitingRoom:
                    __instance.UpdateStateGoingToWaitingRoom();
                    break;
                case PatientState.WaitingGoingToChair:
                    __instance.UpdateStateGoingToChair();
                    break;
                case PatientState.WaitingSitting:
                    __instance.UpdateStateWaitingStandingIdle(deltaTime);
                    break;
                case PatientState.WaitingStandingIdle:
                    __instance.UpdateStateWaitingStandingIdle(deltaTime);
                    break;
                case PatientState.WaitingBeingCalled:
                    __instance.UpdateStateBeingCalled();
                    break;
                case PatientState.FulfillingNeeds:
                    __instance.UpdateStateFulfillingNeeds();
                    break;
                case PatientState.GoingToDoctor:
                    __instance.UpdateStateGoingToDoctor(deltaTime);
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
                    __instance.UpdateStateLeaving();
                    break;
                case PatientState.Left:
                    {
                        __instance.UpdateStateLeft();
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
                    activePatient = false;
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
                __instance.EnsureDepartment();
                __instance.UnreserveEmployees(false, true, false);

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
        [HarmonyPatch(typeof(BehaviorPatient), nameof(BehaviorPatient.UpdateStateFindQueueMachine))]
        public static bool UpdateStateFindQueueMachinePrefix(BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (!BehaviorPatientPatch.HandleDiedSentHome(__instance))
            {
                if (MapScriptInterfacePatch.IsInDestinationRoom(__instance.m_entity))
                {
                    Room currentRoom = MapScriptInterface.Instance.GetRoomAt(__instance.GetComponent<WalkComponent>().GetDestinationTile(), __instance.GetComponent<WalkComponent>().GetFloorIndex());
                    TileObject queueMachine = MapScriptInterface.Instance.FindClosestFreeObjectWithTag(
                        __instance.m_entity, null, __instance.GetComponent<GridComponent>().GetGridPosition(),
                        currentRoom, Tags.Vanilla.Queue, AccessRights.PATIENT, false, null, false);

                    if (queueMachine != null)
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, found free queue machine");

                        __instance.m_entity.GetComponent<WalkComponent>().SetDestination(queueMachine.GetDefaultUsePosition(), queueMachine.GetFloorIndex(), MovementType.WALKING);
                        __instance.m_entity.GetComponent<UseComponent>().ReserveObject(queueMachine);

                        __instance.SwitchState(PatientState.GoToQueueMachine);

                        return false;
                    }
                }

                if (!__instance.GetComponent<WalkComponent>().IsBusy())
                {
                    // no free queue machine
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no free queue machine, going to waiting room");

                    __instance.m_state.m_usedQueueMachine = true;
                    __instance.SwitchState(PatientState.GoingToWaitingRoom);

                    return false;
                }
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

                    __instance.FreeWaitingRoom();
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
                            if (!__instance.m_state.m_finishedAtReception)
                            {
                                // nothing to do
                                __instance.SwitchState(PatientState.Idle);
                            }
                            else
                            {
                                BehaviorPatientPatch.UpdateWaitingTime(__instance, __instance.m_state.m_timeInState);

                                // go to waiting room
                                __instance.GoToWaitingRoom();
                            }
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

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, this method should not be called!");

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateGoingToDoctor")]
        public static bool UpdateStateGoingToDoctorPrefix(BehaviorPatient __instance, float deltaTime)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                __instance.CheckRoomSatisfactionBonuses();

                if (__instance.m_state.m_waitingForPlayer && __instance.GetControlMode() == PatientControlMode.PlayerControl)
                {
                    __instance.m_state.m_playerControlwaitingTime += deltaTime;
                    __instance.CheckPlayerControlTimes(__instance.m_state.m_playerControlwaitingTime);
                }

                if (!BehaviorPatientPatch.HandleDiedSentHome(__instance))
                {
                    if (!__instance.CheckDoctorValid(false))
                    {
                        __instance.UnreserveEmployees(true, false, false);
                        __instance.GoToWaitingRoom();

                        return false;
                    }

                    BehaviorDoctor component = __instance.m_state.m_doctor.GetEntity().GetComponent<BehaviorDoctor>();
                    if (!__instance.GetComponent<WalkComponent>().IsSitting())
                    {
                        __instance.GetComponent<AnimModelComponent>().SetDirection(component.GetInteractionOrientation(__instance.m_entity));
                    }

                    if (__instance.GetControlMode() == PatientControlMode.AI)
                    {
                        __instance.UpdateWaitingTimeModifiers();

                        

                        //this.SelectNextProcedure();
                    }
                }
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

                    __instance.GetComponent<ProcedureComponent>().StartExamination(examination, __instance.m_entity, null, __instance.m_state.m_nurse.GetEntity(), null, AccessRights.PATIENT_PROCEDURE, EquipmentListRules.ONLY_FREE);
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

                    __instance.UnreserveEmployees(false, true, false);
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

            if (!BehaviorPatientPatch.HandleDiedSentHome(__instance))
            {
                if (MapScriptInterfacePatch.IsInDestinationRoom(__instance.m_entity))
                {
                    if (__instance.m_state.m_usedQueueMachine)
                    {
                        Room currentRoom = MapScriptInterface.Instance.GetRoomAt(__instance.GetComponent<WalkComponent>());

                        __instance.m_state.m_waitingRoom = currentRoom;
                        __instance.m_state.m_waitingRoom.GetEntity().EnqueueCharacter(__instance.m_entity, true);
                        __instance.CheckRoomSatisfactionBonuses();

                        __instance.SwitchState(PatientState.WaitingStandingIdle);
                    }
                    else
                    {
                        __instance.SwitchState(PatientState.FindQueueMachine);
                    }
                }
            }

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
                            if ((!BehaviorPatientPatch.HandleDiedSentHomeFulfillNeeds(__instance))
                                && (!__instance.GetComponent<WalkComponent>().IsBusy()))
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

                                    if (!__instance.TryToSit(null, false))
                                    {
                                        __instance.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.CouldNotSit);

                                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no chair");

                                        if (!__instance.TryToStand(false, false))
                                        {
                                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no place to stand, search for waiting room");

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
                        // send patient to waiting room

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, done on reception, search for waiting room");

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
        [HarmonyPatch(typeof(BehaviorPatient), "UpdateStateWaitingSitting")]
        public static bool UpdateStateWaitingSittingPrefix(float deltaTime, BehaviorPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, this method should not be called!");
            __instance.Leave(false, false, false);

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

            BehaviorPatientPatch.UpdateWaitingTime(__instance, deltaTime);

            if (!BehaviorPatientPatch.HandleDiedSentHomeFulfillNeeds(__instance))
            {
                if (!__instance.GetComponent<WalkComponent>().IsBusy())
                {
                    if (!__instance.TryToSit(null, false))
                    {
                        __instance.GetComponent<MoodComponent>().AddSatisfactionModifier(SatisfationModifiers.Vanilla.CouldNotSit);

                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no chair");

                        if (!__instance.TryToStand(false, false))
                        {
                            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, no place to stand");

                            __instance.Leave(false, false, false);
                            return false;
                        }
                    }

                    if (__instance.GetComponent<AnimModelComponent>().IsIdle())
                    {
                        if (__instance.GetComponent<WalkComponent>().IsSitting())
                        {
                            __instance.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.SitIdle, true);
                        }
                        else
                        {
                            __instance.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
                        }
                    }

                    if (__instance.m_state.m_continuousWaitingTime > DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Vanilla.PatientMaxWaitTimeHours()))
                    {
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, Notifications.Vanilla.NOTIF_PATIENT_LEFT_AFTER_LONG_WAIT, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, null, null);

                        __instance.Leave(false, false, false);
                        return false;
                    }

                    if (__instance.m_state.m_fVisitDuration > DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Vanilla.PatientLeaveTimeHours()))
                    {
                        NotificationManager.GetInstance().AddMessage(__instance.m_entity, Notifications.Vanilla.NOTIF_PATIENT_LEFT_AFTER_LONG_VISIT, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, null, null);

                        __instance.Leave(false, false, false);
                        return false;
                    }

                    if (__instance.m_state.m_waitingRoom.GetEntity().IsCharactersTurn(__instance.m_entity))
                    {
                        // check if assigned doctor is free
                        if (__instance.m_state.m_doctor != null)
                        {
                            if (__instance.m_state.m_doctor.GetEntity().GetComponent<BehaviorDoctor>().IsFree(__instance.m_entity))
                            {
                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, assigned doctor {__instance.m_state.m_doctor.GetEntity().Name} is free");

                                // doctor is free
                                // patient is on turn

                                __instance.GetCalled(__instance.m_state.m_doctor.GetEntity());
                            }
                        }
                        else
                        {
                            // try to choose random free doctor

                            Entity doctor = MapScriptInterface.Instance.FindClosestFreeDoctorWithQualification(
                                null, __instance.m_state.m_department.GetEntity(), __instance.m_state.m_waitingRoom.GetEntity(),
                                __instance.GetComponent<WalkComponent>().GetCurrentTile(), __instance.GetComponent<WalkComponent>().GetFloorIndex(),
                                null, __instance.m_entity, Database.Instance.GetEntry<GameDBEmployeeRole>(EmployeeRoles.Vanilla.Diagnostics));

                            if (doctor != null)
                            {
                                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{__instance.m_entity.Name}, found free doctor {doctor.Name}");

                                // doctor is free
                                // patient is on turn
                                // assign doctor and start

                                __instance.SetDoctor(doctor);
                                __instance.GetCalled(doctor);
                            }
                        }
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
                instance.LeaveAfterClosingHours();

                return true;
            }

            if ((instance.m_state.m_department != null)  && (!instance.m_state.m_department.CheckEntity()))
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, is assigned to a deleted department");

                instance.m_state.m_department = null;

                if (!instance.EnsureDepartment())
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

                if (!instance.EnsureDepartment())
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, no working emergency, sending patient home");

                    instance.Leave(false, false, false);

                    return true;
                }
            }

            if ((instance.m_state.m_department != null)
                && (instance.m_state.m_department.GetEntity().GetDepartmentType() == Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Emergency))
                && (!instance.EnsureDepartment()))
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

        public static void UnreserveEmployees(this BehaviorPatient instance, bool doctor, bool nurse, bool labSpecialist)
        {
            if (doctor)
            {
                if (instance.m_state.m_doctor.GetEntity()?.GetComponent<BehaviorDoctor>().CurrentPatient == instance.m_entity)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, unreserved doctor {instance.m_state.m_doctor.GetEntity().Name}");

                    // unreserve doctor
                    instance.m_state.m_doctor.GetEntity().GetComponent<BehaviorDoctor>().CurrentPatient = null;
                }
                if (instance.m_state.m_doctor.GetEntity()?.GetComponent<EmployeeComponent>().m_state.m_reservedByPatient == instance.m_entity)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, unreserved doctor {instance.m_state.m_doctor.GetEntity().Name}");

                    // unreserve doctor
                    instance.m_state.m_doctor.GetEntity().GetComponent<EmployeeComponent>().m_state.m_reservedByPatient = null;
                }
            }

            if (nurse)
            {
                if (instance.m_state.m_nurse.GetEntity()?.GetComponent<BehaviorNurse>().CurrentPatient == instance.m_entity)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, unreserved nurse {instance.m_state.m_nurse.GetEntity().Name}");

                    // unreserve nurse
                    instance.m_state.m_nurse.GetEntity().GetComponent<BehaviorNurse>().CurrentPatient = null;
                }
                if (instance.m_state.m_nurse.GetEntity()?.GetComponent<EmployeeComponent>().m_state.m_reservedByPatient == instance.m_entity)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, unreserved nurse {instance.m_state.m_nurse.GetEntity().Name}");

                    // unreserve nurse
                    instance.m_state.m_nurse.GetEntity().GetComponent<EmployeeComponent>().m_state.m_reservedByPatient = null;
                }
            }

            if (labSpecialist)
            {
                if (instance.m_state.m_labSpecialist.GetEntity()?.GetComponent<BehaviorLabSpecialist>().CurrentPatient == instance.m_entity)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, unreserved lab specialist {instance.m_state.m_labSpecialist.GetEntity().Name}");

                    // unreserve lab specialist
                    instance.m_state.m_labSpecialist.GetEntity().GetComponent<BehaviorLabSpecialist>().CurrentPatient = null;
                }
                if (instance.m_state.m_labSpecialist.GetEntity()?.GetComponent<EmployeeComponent>().m_state.m_reservedByPatient == instance.m_entity)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, unreserved lab specialist {instance.m_state.m_labSpecialist.GetEntity().Name}");

                    // unreserve lab specialist
                    instance.m_state.m_labSpecialist.GetEntity().GetComponent<EmployeeComponent>().m_state.m_reservedByPatient = null;
                }
            }
        }

        private static void UpdateWaitingTime(BehaviorPatient instance, float deltaTime)
        {
            instance.m_state.m_waitingTime += deltaTime;

            float maxWaitingTime = DayTime.Instance.IngameTimeHoursToRealTimeSeconds(Tweakable.Vanilla.PatientMaxWaitTimeHours() - 1f);
            if ((instance.m_state.m_continuousWaitingTime < maxWaitingTime) && ((instance.m_state.m_continuousWaitingTime + deltaTime) >= maxWaitingTime))
            {
                NotificationManager.GetInstance().AddMessage(instance.m_entity, Notifications.Vanilla.NOTIF_PATIENT_LONG_WAIT, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, null, null);
            }

            instance.m_state.m_continuousWaitingTime += deltaTime;
        }
    }

    public static class BehaviorPatientExtensions
    {
        public static bool CheckDoctorValid(this BehaviorPatient instance, bool leavingHospitalizationPatient)
        {
            return MethodAccessHelper.CallMethod<bool>(instance, "CheckDoctorValid", leavingHospitalizationPatient);
        }

        public static void CheckRoomSatisfactionBonuses(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "CheckRoomSatisfactionBonuses");
        }

        public static bool EnsureDepartment(this BehaviorPatient instance)
        {
            return MethodAccessHelper.CallMethod<bool>(instance, "EnsureDepartment");
        }

        public static bool EnsureWaitingRoom(this BehaviorPatient instance)
        {
            return MethodAccessHelper.CallMethod<bool>(instance, "EnsureWaitingRoom");
        }

        public static void GetCalled(this BehaviorPatient instance, Entity doctorEntity)
        {
            MethodAccessHelper.CallMethod(instance, "GetCalled", doctorEntity);
        }

        public static void GoToEmergency(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "GoToEmergency");
        }

        public static void LeaveAfterClosingHours(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "LeaveAfterClosingHours");
        }

        public static void ReportMissingWaitingRoom(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "ReportMissingWaitingRoom");
        }

        public static bool TryToSit(this BehaviorPatient instance, TileObject chairObject, bool pharmacyChair)
        {
            return MethodAccessHelper.CallMethod<bool>(instance, "TryToSit", chairObject, pharmacyChair);
        }

        public static bool TryToStand(this BehaviorPatient instance, bool tryAreasAround, bool onlyInFront)
        {
            return MethodAccessHelper.CallMethod<bool>(instance, "TryToStand", tryAreasAround, onlyInFront);
        }

        public static void UpdateStateBeingCalled(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateBeingCalled");
        }

        public static void UpdateStateExaminedAtReception(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateExaminedAtReception");
        }

        public static void UpdateStateFulfillingNeeds(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateFulfillingNeeds");
        }

        public static void UpdateStateGoingToChair(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingToChair");
        }

        public static void UpdateStateGoingToDoctor(this BehaviorPatient instance, float deltaTime)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingToDoctor", deltaTime);
        }

        public static void UpdateStateGoingToQueueMachine(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingToQueueMachine");
        }

        public static void UpdateStateGoingToReception(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingToReception");
        }

        public static void UpdateStateGoingToReceptionist(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingToReceptionist");
        }

        public static void UpdateStateGoingToWaitingRoom(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateGoingToWaitingRoom");
        }

        public static void UpdateStateIdle(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateIdle");
        }

        public static void UpdateStateLeaving(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateLeaving");
        }

        public static void UpdateStateLeft(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateLeft");
        }

        public static void UpdateStateWaitingSitting(this BehaviorPatient instance, float deltaTime)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateWaitingSitting", deltaTime);
        }

        public static void UpdateStateWaitingStandingIdle(this BehaviorPatient instance, float deltaTime)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateWaitingStandingIdle", deltaTime);
        }

        public static void UpdateStateSpawned(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateSpawned");
        }

        public static void UpdateStateUsingQueueMachine(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateStateUsingQueueMachine");
        }

        public static void UpdateWaitingTimeModifiers(this BehaviorPatient instance)
        {
            MethodAccessHelper.CallMethod(instance, "UpdateWaitingTimeModifiers");
        }
    }
}
