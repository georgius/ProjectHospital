using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System.Linq;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptControlDoctorCallsPatient))]
    public static class ProcedureScriptControlDoctorCallsPatientPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlDoctorCallsPatient), nameof(ProcedureScriptControlDoctorCallsPatient.Activate))]
        public static bool ActivatePrefix(ProcedureScriptControlDoctorCallsPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, activating script {__instance.m_stateData.m_scriptName}");

            Entity doctor = __instance.m_stateData.m_procedureScene.Doctor.GetEntity();
            Entity patient = __instance.m_stateData.m_procedureScene.m_patient.GetEntity();

            __instance.SendMessage(doctor, new Message(Messages.OVERRIDE_BY_PROCEDURE_SCRIPT));
            doctor.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Answer, -1f);

            doctor.GetComponent<WalkComponent>().SetDestination(patient.GetComponent<GridComponent>().GetGridPosition(), patient.GetComponent<WalkComponent>().GetFloorIndex(), MovementType.WALKING);

            __instance.SwitchState(ProcedureScriptControlDoctorCallsPatientPatch.STATE_DOCTOR_GOING_OUT);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlDoctorCallsPatient), nameof(ProcedureScriptControlDoctorCallsPatient.ScriptUpdate))]
        public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptControlDoctorCallsPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __instance.m_stateData.m_timeInState += deltaTime;

            switch (__instance.m_stateData.m_state)
            {
                case ProcedureScriptControlDoctorCallsPatientPatch.STATE_DOCTOR_GOING_OUT:
                    ProcedureScriptControlDoctorCallsPatientPatch.UpdateStateDoctorGoingOut(__instance);
                    break;
                case ProcedureScriptControlDoctorCallsPatientPatch.STATE_DOCTOR_CALLING_PATIENT:
                    ProcedureScriptControlDoctorCallsPatientPatch.UpdateStateDoctorCallingPatient(__instance);
                    break;
                case ProcedureScriptControlDoctorCallsPatientPatch.STATE_PATIENT_RESPONDING:
                    ProcedureScriptControlDoctorCallsPatientPatch.UpdateStatePatientResponding(__instance);
                    break;
                case ProcedureScriptControlDoctorCallsPatientPatch.STATE_DOCTOR_RETURNING:
                    ProcedureScriptControlDoctorCallsPatientPatch.UpdateStateDoctorReturning(__instance);
                    break;
                default:
                    break;
            }

            return false;
        }

        public static void UpdateStateDoctorGoingOut(ProcedureScriptControlDoctorCallsPatient instance)
        {
            Entity patient = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity doctor = instance.m_stateData.m_procedureScene.Doctor.GetEntity();

            Room currentRoom = MapScriptInterface.Instance.GetRoomAt(doctor.GetComponent<WalkComponent>());
            Room homeRoom = doctor.GetComponent<EmployeeComponent>().m_state.m_homeRoom.GetEntity();

            if ((homeRoom != currentRoom) || (!doctor.GetComponent<WalkComponent>().IsBusy()))
            {
                doctor.GetComponent<WalkComponent>().Stop();
                doctor.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
                doctor.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.NextPatient, -1f);
                doctor.GetComponent<SpeechComponent>().PlayDialogue(Dialogues.Vanilla.StaffNext);

                instance.SwitchState(ProcedureScriptControlDoctorCallsPatientPatch.STATE_DOCTOR_CALLING_PATIENT);
            }
        }

        public static void UpdateStateDoctorCallingPatient(ProcedureScriptControlDoctorCallsPatient instance)
        {
            Entity patient = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity doctor = instance.m_stateData.m_procedureScene.Doctor.GetEntity();

            if (instance.m_stateData.m_timeInState > 1f)
            {
                instance.SpeakCharacter(patient, Dialogues.Vanilla.PatientStatement, Speeches.Vanilla.NextPatient, -1f);
                doctor.GetComponent<SpeechComponent>().HideBubble();

                instance.SwitchState(ProcedureScriptControlDoctorCallsPatientPatch.STATE_PATIENT_RESPONDING);
            }
        }

        public static void UpdateStatePatientResponding(ProcedureScriptControlDoctorCallsPatient instance)
        {
            Entity patient = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity doctor = instance.m_stateData.m_procedureScene.Doctor.GetEntity();

            if (instance.m_stateData.m_timeInState > 1f)
            {
                instance.SendMessage(doctor, new Message(Messages.CANCEL_OVERRIDE_BY_PROCEDURE_SCRIPT));
                doctor.GetComponent<BehaviorDoctor>().GoToWorkPlace();
                
                patient.GetComponent<SpeechComponent>().HideBubble();
                instance.SwitchState(ProcedureScriptControlDoctorCallsPatientPatch.STATE_DOCTOR_RETURNING);
            }
        }

        public static void UpdateStateDoctorReturning(ProcedureScriptControlDoctorCallsPatient instance)
        {
            Entity patient = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity doctor = instance.m_stateData.m_procedureScene.Doctor.GetEntity();

            if (!doctor.GetComponent<WalkComponent>().IsBusy())
            {
                instance.SwitchState(ProcedureScriptControlDoctorCallsPatient.STATE_IDLE);
            }
        }

        public const string STATE_DOCTOR_GOING_OUT = "STATE_DOCTOR_GOING_OUT";
        public const string STATE_DOCTOR_CALLING_PATIENT = "STATE_DOCTOR_CALLING_PATIENT";
        public const string STATE_PATIENT_RESPONDING = "STATE_PATIENT_RESPONDING";
        public const string STATE_DOCTOR_RETURNING = "STATE_DOCTOR_RETURNING";
    }
}
