using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System.Collections.Generic;
using System.Linq;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptControlCallPatient))]
    public static class ProcedureScriptControlCallPatientPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlCallPatient), nameof(ProcedureScriptControlCallPatient.Activate))]
        public static bool ActivatePrefix(ProcedureScriptControlCallPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, activating script {__instance.m_stateData.m_scriptName}");

            __instance.SwitchState(ProcedureScriptControlCallPatientPatch.STATE_WAITING_FOR_DOCTOR);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptControlCallPatient), nameof(ProcedureScriptControlCallPatient.ScriptUpdate))]
        public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptControlCallPatient __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            __instance.m_stateData.m_timeInState += deltaTime;

            switch (__instance.m_stateData.m_state)
            {
                case ProcedureScriptControlCallPatientPatch.STATE_WAITING_FOR_DOCTOR:
                    ProcedureScriptControlCallPatientPatch.UpdateStateWaitingForDoctor(__instance);
                    break;
                case ProcedureScriptControlCallPatientPatch.STATE_DOCTOR_CALLING_PATIENT:
                    ProcedureScriptControlCallPatientPatch.UpdateStateDoctorCallingPatient(__instance);
                    break;
                case ProcedureScriptControlCallPatientPatch.STATE_PATIENT_RESPONDING:
                    ProcedureScriptControlCallPatientPatch.UpdateStatePatientResponding(__instance);
                    break;
                default:
                    break;
            }

            return false;
        }

        public static void UpdateStateWaitingForDoctor(ProcedureScriptControlCallPatient instance)
        {
            if (instance.m_stateData.m_procedureScene.Doctor != null)
            {
                Entity doctor = instance.m_stateData.m_procedureScene.Doctor.GetEntity();

                instance.SendMessage(doctor, new Message(Messages.OVERRIDE_BY_PROCEDURE_SCRIPT));

                instance.SwitchState(ProcedureScriptControlCallPatientPatch.STATE_DOCTOR_CALLING_PATIENT);
            }
        }

        public static void UpdateStateDoctorCallingPatient(ProcedureScriptControlCallPatient instance)
        {
            Entity patient = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity doctor = instance.m_stateData.m_procedureScene.Doctor.GetEntity();

            List<TileObject> queueMonitors = MapScriptInterface.Instance.FindAllObjectWithTags(
                patient.GetComponent<BehaviorPatient>().m_state.m_waitingRoom.GetEntity(), 
                new string[] { Tags.Vanilla.QueueMonitor }, AccessRights.PATIENT_PROCEDURE, false);

            foreach (TileObject queueMonitor in queueMonitors)
            {
                queueMonitor.GetComponent<AnimatedObjectComponent>().ForceFrame(2);
            }

            queueMonitors.FirstOrDefault()?.PlayStartUseSound();

            doctor.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.NextPatient, -1f);
            instance.SwitchState(ProcedureScriptControlCallPatientPatch.STATE_PATIENT_RESPONDING);
        }

        public static void UpdateStatePatientResponding(ProcedureScriptControlCallPatient instance)
        {
            Entity patient = instance.m_stateData.m_procedureScene.MainCharacter;
            Entity doctor = instance.m_stateData.m_procedureScene.Doctor.GetEntity();

            if (instance.m_stateData.m_timeInState > 2.5f)
            {
                patient.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.NextPatient, 2.5f);
                patient.GetComponent<UseComponent>().Interrupt();

                List<TileObject> queueMonitors = MapScriptInterface.Instance.FindAllObjectWithTags(
                    patient.GetComponent<BehaviorPatient>().m_state.m_waitingRoom.GetEntity(),
                    new string[] { Tags.Vanilla.QueueMonitor }, AccessRights.PATIENT_PROCEDURE, false);

                foreach (TileObject queueMonitor in queueMonitors)
                {
                    queueMonitor.GetComponent<AnimatedObjectComponent>().ForceFrame(1);
                }

                doctor.GetComponent<SpeechComponent>().HideBubble();
                instance.SendMessage(doctor, new Message(Messages.CANCEL_OVERRIDE_BY_PROCEDURE_SCRIPT));
                instance.SwitchState(ProcedureScriptControlCallPatient.STATE_IDLE);
            }
        }

        public const string STATE_WAITING_FOR_DOCTOR = "STATE_WAITING_FOR_DOCTOR";
        public const string STATE_DOCTOR_CALLING_PATIENT = "STATE_DOCTOR_CALLING_PATIENT";
        public const string STATE_PATIENT_RESPONDING = "STATE_PATIENT_RESPONDING";
    }
}
