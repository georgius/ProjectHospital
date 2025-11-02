using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Reflection;
using UnityEngine;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(BehaviorPatient))]
    public static class BehaviorPatientPatch
    {
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(BehaviorPatient), "LeaveAfterClosingHours")]
        //public static bool LeaveAfterClosingHours(BehaviorPatient __instance)
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
        [HarmonyPatch(typeof(BehaviorPatient), nameof(BehaviorPatient.Update))]
        public static bool Update(float deltaTime, BehaviorPatient __instance)
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

                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patient {__instance.m_entity.Name}, Error: reset a collapse still triggered with no critical symptoms left");
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

            if ((__instance.m_state.m_patientState == PatientState.WaitingSittingInPharmacy) 
                || (__instance.m_state.m_patientState == PatientState.WaitingStandingInPharmacy) 
                || (__instance.m_state.m_patientState == PatientState.BuyingMedicine))
            {
                Hospital.Instance.m_currentHospitalStatus.m_patientsWaitingForPharmacy++;
            }

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
                    break;
                case PatientState.GoingToReception:
                    break;
                case PatientState.GoingToReceptionist:
                    break;
                case PatientState.ExaminedAtReception:
                    break;
                case PatientState.FindQueueMachine:
                    break;
                case PatientState.GoToQueueMachine:
                    break;
                case PatientState.UseQueueMachine:
                    break;
                case PatientState.Wandering:
                    break;
                case PatientState.GoingToWaitingRoom:
                    break;
                case PatientState.WaitingGoingToChair:
                    break;
                case PatientState.WaitingSitting:
                    break;
                case PatientState.WaitingStandingIdle:
                    break;
                case PatientState.WaitingBeingCalled:
                    break;
                case PatientState.FulfillingNeeds:
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
                    break;
                case PatientState.Left:
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
                InsuranceCompany insuranceCompany = InsuranceManager.Instance.GetInsuranceCompany(__instance.GetComponent<CharacterPersonalInfoComponent>().m_personalInfo.m_insuranceCompany.Entry);
                if ((!__instance.m_state.m_fromLevel) && (!insuranceCompany.m_isContracted))
                {
                    __instance.SwitchState(PatientState.SpawnedFromDisabledInsurance);
                    return false;
                }

                if (__instance.m_state.m_deathTriggered)
                {
                    Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patient {__instance.m_entity.Name} is dead!");
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

                if (!__instance.m_state.m_department.CheckEntity())
                {
                    Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patient {__instance.m_entity.Name} is assigned to a deleted department");

                    __instance.m_state.m_department = null;

                    if (!__instance.EnsureDepartmentInternal())
                    {
                        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patient {__instance.m_entity.Name} no working emergency");
                        __instance.Leave(false, false, false);
                    }
                }

                if (__instance.m_state.m_department.GetEntity().GetDepartmentType() == Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Default))
                {
                    if (Application.isEditor)
                    {
                        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patient {__instance.m_entity.Name} is assigned to a {Departments.Vanilla.Default} department");
                    }

                    __instance.m_state.m_department.GetEntity().RemovePatient(__instance.m_entity);
                    __instance.m_state.m_department = null;

                    if (!__instance.EnsureDepartmentInternal())
                    {
                        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patient {__instance.m_entity.Name} no working emergency");
                        __instance.Leave(false, false, false);
                    }
                }

                if ((__instance.m_state.m_medicalCondition.m_wrongDiagnoses != null) && (__instance.m_state.m_medicalCondition.m_wrongDiagnoses.Count > 0))
                {
                    NotificationManager.GetInstance().AddMessage(__instance.m_entity, Notifications.Vanilla.NOTIF_WRONG_DIAGNOSIS_RETURNS, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, null, null);
                }

                if ((__instance.m_state.m_department != null) 
                    && (__instance.m_state.m_department.GetEntity().GetDepartmentType() == Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Emergency)) 
                    && (!__instance.EnsureDepartmentInternal()))
                {
                    Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patient {__instance.m_entity.Name} no working emergency");
                    __instance.Leave(false, false, false);
                }

                __instance.m_state.m_reservedWaitingRoomTile = Vector2i.ZERO_VECTOR;
                __instance.m_state.m_medicalCondition.ResetCollapseTimes(__instance);

                if (__instance.m_state.m_medicalCondition.HasImmobileSymptom())
                {
                    if ((!__instance.m_state.m_fromLevel) && ((!insuranceCompany.m_isContracted) || (insuranceCompany.m_immobileSpawnedToday == 0)))
                    {
                        __instance.m_state.m_sentAway = true;
                        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patient {__instance.m_entity.Name}, map contains an immobile patient that doesn't match insurance!");
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
                    __instance.m_state.m_sentAway = false;
                    __instance.m_state.m_waitingRoom = null;
                    __instance.GetComponent<AnimModelComponent>().FadeIn(1f);
                    insuranceCompany.OnPatientSpawned(__instance.m_entity);
                    __instance.GetComponent<ProcedureComponent>().CheckLabProcedures();
                    __instance.SwitchState(PatientState.Idle);
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

        private static bool EnsureDepartmentInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("EnsureDepartment", BindingFlags.NonPublic | BindingFlags.Instance);

            return (bool)methodInfo.Invoke(instance, null);
        }

        private static void LeaveAfterClosingHoursInternal(this BehaviorPatient instance)
        {
            Type type = typeof(BehaviorPatient);
            MethodInfo methodInfo = type.GetMethod("LeaveAfterClosingHours", BindingFlags.NonPublic | BindingFlags.Instance);

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
