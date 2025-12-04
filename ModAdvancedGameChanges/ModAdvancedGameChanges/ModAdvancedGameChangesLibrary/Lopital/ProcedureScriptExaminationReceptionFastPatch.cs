using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System.Globalization;
using System.Linq;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptExaminationReceptionFast))]
    public static class ProcedureScriptExaminationReceptionFastPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptExaminationReceptionFast), nameof(ProcedureScriptExaminationReceptionFast.Activate))]
        public static bool ActivatePrefix(ProcedureScriptExaminationReceptionFast __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            Entity nurse = __instance.m_stateData.m_procedureScene.Nurse.GetEntity();
            Entity patient = __instance.m_stateData.m_procedureScene.m_patient.GetEntity();

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"nurse {nurse?.Name ?? "NULL"}, patient {patient?.Name ?? "NULL"}, activating script {__instance.m_stateData.m_scriptName}");

            nurse.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.OVERRIDE_BY_PROCEDURE_SCRIPT));

            if (nurse.GetComponent<BehaviorNurse>().IsAtWorkplace(nurse.GetComponent<EmployeeComponent>()))
            {
                ProcedureScriptExaminationReceptionFastPatch.NurseStartTalking(__instance);
            }
            else
            {
                patient.GetComponent<AnimModelComponent>().SetDirection(nurse.GetComponent<BehaviorNurse>().GetInteractionOrientation(patient));
                patient.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);
                
                __instance.SwitchState(ProcedureScriptExaminationReceptionFastPatch.STATE_GOING_TO_WORKPLACE);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptExaminationReceptionFast), nameof(ProcedureScriptExaminationReceptionFast.ScriptUpdate))]
        public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptExaminationReceptionFast __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // allow original method to run
                return true;
            }

            __instance.m_stateData.m_timeInState += deltaTime;

            switch (__instance.m_stateData.m_state)
            {
                case ProcedureScriptExaminationReceptionFastPatch.STATE_GOING_TO_WORKPLACE:
                    ProcedureScriptExaminationReceptionFastPatch.UpdateStateNurseGoingToWorkplace(__instance);
                    break;
                case ProcedureScriptExaminationReceptionFastPatch.STATE_NURSE_DECIDING:
                    ProcedureScriptExaminationReceptionFastPatch.UpdateStateNurseDeciding(__instance);
                    break;
                case ProcedureScriptExaminationReceptionFastPatch.STATE_NURSE_TALKING:
                    ProcedureScriptExaminationReceptionFastPatch.UpdateStateNurseTalking(__instance);
                    break;
                case ProcedureScriptExaminationReceptionFastPatch.STATE_PATIENT_TALKING:
                    ProcedureScriptExaminationReceptionFastPatch.UpdateStatePatientTalking(__instance);
                    break;
                default:
                    break;
            }

            return false;
        }

        private static void NurseStartTalking(ProcedureScriptExaminationReceptionFast instance)
        {
            Entity nurse = instance.m_stateData.m_procedureScene.Nurse.GetEntity();
            Entity patient = instance.m_stateData.m_procedureScene.m_patient.GetEntity();

            float actionTime = instance.GetActionTime(
                nurse,
                (int)DayTime.Instance.IngameTimeHoursToRealTimeSeconds(((float)Tweakable.Mod.NurseReceptionQuestionMinutes()) / 60f),
                nurse.GetComponent<EmployeeComponent>().GetSkillLevel(Skills.Vanilla.SKILL_NURSE_SPEC_RECEPTIONIST));

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"nurse {nurse?.Name ?? "NULL"}, question time {actionTime.ToString(CultureInfo.InvariantCulture)}");

            instance.SetParam(ProcedureScriptExaminationReceptionFastPatch.PARAM_QUESTION_TIME, actionTime);

            patient.GetComponent<AnimModelComponent>().SetDirection(nurse.GetComponent<BehaviorNurse>().GetInteractionOrientation(patient));
            nurse.GetComponent<AnimModelComponent>().SetDirection(nurse.GetComponent<BehaviorNurse>().GetInteractionOrientationSelf(patient));

            nurse.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Question, -1f);
            nurse.GetComponent<SpeechComponent>().PlayDialogue(Dialogues.Vanilla.StaffStatement);

            patient.GetComponent<SpeechComponent>().HideBubble();

            instance.SwitchState(ProcedureScriptExaminationReceptionFastPatch.STATE_NURSE_TALKING);
        }

        private static void UpdateStateNurseDeciding(ProcedureScriptExaminationReceptionFast instance)
        {
            Entity nurse = instance.m_stateData.m_procedureScene.Nurse.GetEntity();
            Entity patient = instance.m_stateData.m_procedureScene.m_patient.GetEntity();

            if (instance.m_stateData.m_timeInState > instance.GetParam(ProcedureScriptExaminationReceptionFastPatch.PARAM_DECISION_TIME))
            {
                bool continueAsking = false;

                if (instance.GetParam(ProcedureScriptExaminationReceptionFastPatch.PARAM_SYMPTOM_REPORTED) != 0f)
                {
                    // some symptom is reported
                    continueAsking = UnityEngine.Random.Range(Skills.SkillLevelMinimum, Skills.SkillLevelMaximum) < nurse.GetComponent<EmployeeComponent>().GetSkillLevel(Skills.Vanilla.SKILL_NURSE_QUALIF_PATIENT_CARE);
                }

                if (continueAsking)
                {
                    nurse.GetComponent<EmployeeComponent>().AddSkillPoints(Skills.Vanilla.SKILL_NURSE_QUALIF_PATIENT_CARE, Tweakable.Mod.NurseReceptionNextQuestionSkillPoints(), true);

                    float actionTime = instance.GetActionTime(
                        nurse,
                        (int)DayTime.Instance.IngameTimeHoursToRealTimeSeconds(((float)Tweakable.Mod.NurseReceptionQuestionMinutes()) / 60f),
                        nurse.GetComponent<EmployeeComponent>().GetSkillLevel(Skills.Vanilla.SKILL_NURSE_SPEC_RECEPTIONIST));

                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"nurse {nurse?.Name ?? "NULL"}, question time {actionTime.ToString(CultureInfo.InvariantCulture)}");

                    instance.SetParam(ProcedureScriptExaminationReceptionFastPatch.PARAM_QUESTION_TIME, actionTime);

                    nurse.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Question, -1f);
                    nurse.GetComponent<SpeechComponent>().PlayDialogue(Dialogues.Vanilla.StaffStatement);

                    patient.GetComponent<SpeechComponent>().HideBubble();

                    instance.SwitchState(ProcedureScriptExaminationReceptionFastPatch.STATE_NURSE_TALKING);
                }
                else
                {
                    // no other symptom will be reported by patient
                    BehaviorPatient component = patient.GetComponent<BehaviorPatient>();
                    component.m_state.m_priority = component.GetWorstKnownHazard();
                    component.m_state.m_finishedAtReception = true;

                    component.ScheduleExamination(Database.Instance.GetEntry<GameDBExamination>(Examinations.Vanilla.Interview));

                    patient.GetComponent<SpeechComponent>().HideBubble();
                    nurse.GetComponent<SpeechComponent>().HideBubble();

                    nurse.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.CANCEL_OVERRIDE_BY_PROCEDURE_SCRIPT));
                    instance.CheckPerkModifiers(patient, nurse);

                    instance.SwitchState(ProcedureScriptExaminationReceptionFast.STATE_IDLE);
                }
            }
        }

        private static void UpdateStateNurseGoingToWorkplace(ProcedureScriptExaminationReceptionFast instance)
        {
            Entity nurse = instance.m_stateData.m_procedureScene.Nurse.GetEntity();

            if (!nurse.GetComponent<WalkComponent>().IsBusy())
            {
                if (nurse.GetComponent<BehaviorNurse>().IsAtWorkplace(nurse.GetComponent<EmployeeComponent>()))
                {
                    ProcedureScriptExaminationReceptionFastPatch.NurseStartTalking(instance);
                }
                else if (nurse.GetComponent<BehaviorNurse>().m_state.m_nurseState != NurseState.GoingToWorkplace)
                {
                    nurse.GetComponent<BehaviorNurse>().GoToWorkplace();
                }
            }
        }

        private static void UpdateStateNurseTalking(ProcedureScriptExaminationReceptionFast instance)
        {
            Entity nurse = instance.m_stateData.m_procedureScene.Nurse.GetEntity();
            Entity patient = instance.m_stateData.m_procedureScene.m_patient.GetEntity();

            if (instance.m_stateData.m_timeInState > instance.GetParam(ProcedureScriptExaminationReceptionFastPatch.PARAM_QUESTION_TIME))
            {
                if (nurse.GetComponent<PerkComponent>().m_perkSet.HasPerk(Perks.Vanilla.PeoplePerson) && (patient.GetComponent<PerkComponent>().m_perkSet.GetHiddenPerkCount() > 0))
                {
                    if (nurse.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.PeoplePerson))
                    {
                        nurse.GetComponent<PerkComponent>().RevealPerk(Perks.Vanilla.PeoplePerson, true);
                    }

                    patient.GetComponent<PerkComponent>().RevealAllPerks(patient.GetComponent<BehaviorPatient>().IsBookmarked());
                }

                patient.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Answer, -1f);
                patient.GetComponent<SpeechComponent>().PlayDialogue(Dialogues.Vanilla.PatientStatement);
                nurse.GetComponent<SpeechComponent>().HideBubble();
                nurse.GetComponent<EmployeeComponent>().AddSkillPoints(Skills.Vanilla.SKILL_NURSE_SPEC_RECEPTIONIST, Tweakable.Mod.NurseReceptionQuestionSkillPoints(), true);

                instance.SwitchState(ProcedureScriptExaminationReceptionFastPatch.STATE_PATIENT_TALKING);
            }
        }

        private static void UpdateStatePatientTalking(ProcedureScriptExaminationReceptionFast instance)
        {
            Entity nurse = instance.m_stateData.m_procedureScene.Nurse.GetEntity();
            Entity patient = instance.m_stateData.m_procedureScene.m_patient.GetEntity();

            if (instance.m_stateData.m_timeInState > 2f)
            {
                instance.SetParam(ProcedureScriptExaminationReceptionFastPatch.PARAM_SYMPTOM_REPORTED, ProcedureScriptExaminationReceptionFastPatch.ReportedPatientSomeSymptom(instance) ? 1f : 0f);

                float actionTime = instance.GetActionTime(
                    nurse, 
                    (int)DayTime.Instance.IngameTimeHoursToRealTimeSeconds(((float)Tweakable.Mod.NurseReceptionDecisionMinutes()) / 60f),
                    nurse.GetComponent<EmployeeComponent>().GetSkillLevel(Skills.Vanilla.SKILL_NURSE_QUALIF_PATIENT_CARE));

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"nurse {nurse?.Name ?? "NULL"}, decide time {actionTime.ToString(CultureInfo.InvariantCulture)}");

                instance.SetParam(ProcedureScriptExaminationReceptionFastPatch.PARAM_DECISION_TIME, actionTime);

                instance.SwitchState(ProcedureScriptExaminationReceptionFastPatch.STATE_NURSE_DECIDING);

                patient.GetComponent<SpeechComponent>().HideBubble();
                nurse.GetComponent<SpeechComponent>().SetBubble(Speeches.Vanilla.Exclamation, -1f);
                nurse.GetComponent<SpeechComponent>().PlayDialogue(Dialogues.Vanilla.StaffStatement);
            }
        }

        private static bool ReportedPatientSomeSymptom(ProcedureScriptExaminationReceptionFast instance)
        {
            Entity patient = instance.m_stateData.m_procedureScene.m_patient.GetEntity();
            BehaviorPatient component = patient.GetComponent<BehaviorPatient>();

            Symptom symptom = component.m_state.m_medicalCondition.m_symptoms
                .Where(s => (s.m_hidden && s.m_patientKnowsAndComplains))
                .OrderByDescending(s => s.m_symptom.Entry.Hazard)
                .FirstOrDefault();

            if (symptom != null)
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"patient {patient?.Name ?? "NULL"}, reporting symptom {symptom.m_symptom.Entry.DatabaseID}");

                symptom.m_hidden = false;
                return true;
            }

            // default case, no other symptom to be reported
            return false;
        }

        public const string STATE_GOING_TO_WORKPLACE = "STATE_GOING_TO_WORKPLACE";
        public const string STATE_NURSE_DECIDING = "STATE_NURSE_DECIDING";
        public const string STATE_NURSE_TALKING = "STATE_NURSE_TALKING";
        public const string STATE_PATIENT_TALKING = "STATE_PATIENT_TALKING";

        public const int PARAM_QUESTION_TIME = 0;
        public const int PARAM_SYMPTOM_REPORTED = 1;
        public const int PARAM_DECISION_TIME = 2;
    }
}
