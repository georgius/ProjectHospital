using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System.Globalization;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptDoctorFilingReports))]
    public static class ProcedureScriptDoctorFilingReportsPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptDoctorFilingReports), nameof(ProcedureScriptDoctorFilingReports.Activate))]
        public static bool ActivatePrefix(ProcedureScriptDoctorFilingReports __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;

            float actionTime = __instance.GetActionTime(
                mainCharacter,
                (int)DayTime.Instance.IngameTimeHoursToRealTimeSeconds(((float)Tweakable.Mod.DoctorFillingReportMinutes()) / 60f),
                mainCharacter.GetComponent<EmployeeComponent>().GetSkillLevel(Skills.Vanilla.SKILL_DOC_QUALIF_GENERAL_MEDICINE));

            __instance.SetParam(ProcedureScriptDoctorFilingReportsPatch.PARAM_WRITE_TIME, actionTime);

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, activating script {__instance.m_stateData.m_scriptName}, write time {actionTime.ToString(CultureInfo.InvariantCulture)}");

            __instance.SwitchState(ProcedureScriptDoctorFilingReportsPatch.STATE_SEARCH_WORKDESK);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptDoctorFilingReports), nameof(ProcedureScriptDoctorFilingReports.ScriptUpdate))]
        public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptDoctorFilingReports __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __instance.m_stateData.m_timeInState += deltaTime;
            switch (__instance.m_stateData.m_state)
            {
                case ProcedureScriptDoctorFilingReportsPatch.STATE_SEARCH_WORKDESK:
                    ProcedureScriptDoctorFilingReportsPatch.UpdateStateSearchWorkdesk(__instance);
                    break;
                case ProcedureScriptDoctorFilingReportsPatch.STATE_GOING_TO_WORKDESK:
                    ProcedureScriptDoctorFilingReportsPatch.UpdateStateGoingToWorkdesk(__instance);
                    break;
                case ProcedureScriptDoctorFilingReportsPatch.STATE_WRITING_REPORT:
                    ProcedureScriptDoctorFilingReportsPatch.UpdateStateWritingReport(__instance);
                    break;
                default:
                    break;
            }

            return false;
        }

        public static void UpdateStateSearchWorkdesk(ProcedureScriptDoctorFilingReports instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;
            EmployeeComponent employeeComponent = mainCharacter.GetComponent<EmployeeComponent>();
            WalkComponent walkComponent = mainCharacter.GetComponent<WalkComponent>();

            if (employeeComponent.GetWorkChair() != null)
            {
                

                TileObject workChair = employeeComponent.GetWorkChair();
                if (walkComponent.IsSittingOn(workChair))
                {
                    instance.SwitchState(ProcedureScriptDoctorFilingReportsPatch.STATE_GOING_TO_WORKDESK);
                    mainCharacter.GetComponent<BehaviorDoctor>().SwitchState(DoctorState.FilingReports);
                }
                else if (!walkComponent.IsWalkingTo(workChair))
                {
                    walkComponent.GoSit(workChair, MovementType.WALKING);

                    instance.SwitchState(ProcedureScriptDoctorFilingReportsPatch.STATE_GOING_TO_WORKDESK);
                    mainCharacter.GetComponent<BehaviorDoctor>().SwitchState(DoctorState.FilingReports);
                }
            }
            else
            {
                instance.SwitchState(ProcedureScriptDoctorFilingReports.STATE_IDLE);
            }
        }

        public static void UpdateStateGoingToWorkdesk(ProcedureScriptDoctorFilingReports instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                AnimModelComponent component = mainCharacter.GetComponent<AnimModelComponent>();

                component.QueueAnimation(Animations.Vanilla.SitTypeIn, false, true);
                component.QueueAnimation(Animations.Vanilla.SitTypeIdle, true, false);

                mainCharacter.GetComponent<SoundSourceComponent>().PlaySoundEvent(Sounds.Vanilla.ComputerTyping, SoundEventCategory.SFX, 1f, 0f, true, false);

                instance.SwitchState(ProcedureScriptDoctorFilingReportsPatch.STATE_WRITING_REPORT);
            }
        }

        public static void UpdateStateWritingReport(ProcedureScriptDoctorFilingReports instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (instance.m_stateData.m_timeInState > instance.GetParam(ProcedureScriptDoctorFilingReportsPatch.PARAM_WRITE_TIME))
            {
                mainCharacter.GetComponent<EmployeeComponent>().AddSkillPointsToMainQualification(Tweakable.Mod.DoctorFillingReportSkillPoints());

                AnimModelComponent component = mainCharacter.GetComponent<AnimModelComponent>();
                component.QueueAnimation(Animations.Vanilla.SitTypeOut, false, true);
                component.QueueAnimation(Animations.Vanilla.SitIdle, false, false);

                instance.SwitchState(ProcedureScriptDoctorFilingReports.STATE_IDLE);
                mainCharacter.GetComponent<BehaviorDoctor>().SwitchState(DoctorState.Idle);
            }
        }

        public const string STATE_SEARCH_WORKDESK = "STATE_SEARCH_WORKDESK";
        public const string STATE_GOING_TO_WORKDESK = "STATE_GOING_TO_WORKDESK";
        public const string STATE_WRITING_REPORT = "STATE_WRITING_REPORT";

        public const int PARAM_WRITE_TIME = 0;
    }
}
