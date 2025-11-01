using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Reflection;

namespace ModAdvancedGameChanges
{
    [HarmonyPatch(typeof(LogisticsWorkspacePanelController))]
    public static class LogisticsWorkspacePanelControllerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LogisticsWorkspacePanelController), "OpenHiringCard")]
        public static bool OpenHiringCardPrefix(Shift shift, LogisticsWorkspacePanelController __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            switch (__instance.m_characterType)
            {
                case DisplayedCharacterType.DOCTOR:
                    break;
                case DisplayedCharacterType.NURSE:
                    break;
                case DisplayedCharacterType.LAB_SPECIALIST:
                    {
                        if (ViewSettingsPatch.m_labEmployeeBiochemistry[SettingsManager.Instance.m_viewSettings].m_value && (Hospital.Instance.m_activeDepartment.GetEntity().GetDepartmentType() == Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.MedicalLaboratories)))
                        {
                            HiringManager.Instance.m_workspace = LogisticsWorkspacePanelControllerPatch.GetWorkspaceMod(__instance);

                            MapEditorUIController.Instance.m_hiringPanel.transform.parent.gameObject.SetActive(true);
                            MapEditorUIController.Instance.m_hiringPanel.GetComponent<HiringPanelController>().UpdateShift(shift, true);
                            MapEditorUIController.Instance.m_hiringPanel.SetActive(true);
                            MapEditorUIController.Instance.m_hiringPanel.GetComponent<HiringPanelController>().SortHiringSpecializationsButtons(true);

                            MapEditorUIController.Instance.m_hiringPanel.GetComponent<HiringPanelController>().SetCharacterType(
                                LopitalTypes.CharacterAdvancedBiochemist,
                                Icons.ICON_BIOCHEMISTRY_MANAGEMENT - 4,
                                StringTable.GetInstance().GetLocalizedText(Skills.Vanilla.SKILL_LAB_SPECIALIST_SPEC_BIOCHEMISTRY, new string[0]));

                            __instance.SetEmployeeNumbers(shift, true);
                            UISoundManager.sm_instance.PlaySoundEvent("SFX_UI_WINDOW_OPEN", 1f);
                            MapEditorUIController.Instance.m_hiringPanel.GetComponent<HiringPanelController>().Update();
                            GlobalEventManager.Instance.OnGlobalEvent(new GlobalEvent
                            {
                                m_eventType = GlobalEventType.GAME_MODE_HIRING_CARD_OPENED
                            });
                            UIManager.FixMacTexts();

                            return false;
                        }

                        if (ViewSettingsPatch.m_enabledTrainingDepartment && (Hospital.Instance.m_activeDepartment.GetEntity().GetDepartmentType() == Database.Instance.GetEntry<GameDBDepartment>(Departments.Mod.TrainingDepartment)))
                        {
                            HiringManager.Instance.m_workspace = LogisticsWorkspacePanelControllerPatch.GetWorkspaceMod(__instance);

                            MapEditorUIController.Instance.m_hiringPanel.transform.parent.gameObject.SetActive(true);
                            MapEditorUIController.Instance.m_hiringPanel.GetComponent<HiringPanelController>().UpdateShift(shift, true);
                            MapEditorUIController.Instance.m_hiringPanel.SetActive(true);
                            MapEditorUIController.Instance.m_hiringPanel.GetComponent<HiringPanelController>().SortHiringSpecializationsButtons(true);

                            MapEditorUIController.Instance.m_hiringPanel.GetComponent<HiringPanelController>().SetCharacterType(
                                LopitalTypes.CharacterTechnologist,
                                IconManager.ICON_TECHNOLOGIST_MANAGEMENT - 4,
                                StringTable.GetInstance().GetLocalizedText(Localizations.Vanilla.LAB_TECHNOLOGIST, new string[0]));

                            __instance.SetEmployeeNumbers(shift, true);
                            UISoundManager.sm_instance.PlaySoundEvent("SFX_UI_WINDOW_OPEN", 1f);
                            MapEditorUIController.Instance.m_hiringPanel.GetComponent<HiringPanelController>().Update();
                            GlobalEventManager.Instance.OnGlobalEvent(new GlobalEvent
                            {
                                m_eventType = GlobalEventType.GAME_MODE_HIRING_CARD_OPENED
                            });
                            UIManager.FixMacTexts();

                            return false;
                        }
                    }
                    break;
                case DisplayedCharacterType.JANITOR:
                    break;
                //case DisplayedCharacterType.PARAMEDIC:
                //    break;
                //case DisplayedCharacterType.DOCTORS_NURSES:
                //    break;
                //case DisplayedCharacterType.DOCTOR_CLINIC:
                //    break;
                //case DisplayedCharacterType.DOCTOR_HOSPITALIZATION:
                //    break;
                //case DisplayedCharacterType.PATIENT:
                //    break;
                //case DisplayedCharacterType.ALL_EMPLOYEES:
                //    break;
                default:
                    break;
            }

            return true;
        }

        private static EntityIDPointer<TileObject> GetWorkspaceMod(LogisticsWorkspacePanelController instance)
        {
            // get the Type of the class
            Type type = typeof(LogisticsWorkspacePanelController);

            // get the private field using BindingFlags
            FieldInfo m_workspaceFieldInfo = type.GetField("m_workspace", BindingFlags.NonPublic | BindingFlags.Instance);

            // get object
            return (EntityIDPointer<TileObject>)m_workspaceFieldInfo.GetValue(instance);
        }
    }
}
