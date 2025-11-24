using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using ModAdvancedGameChanges.Helpers;
using System.Linq;
using UnityEngine;

namespace ModAdvancedGameChanges
{
    [HarmonyPatch(typeof(DepartmentManagementLogisticsHybridController))]
    public static class DepartmentManagementLogisticsHybridControllerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DepartmentManagementLogisticsHybridController), "UpdateClinicStaffSegments")]
        public static bool UpdateClinicStaffSegmentsPrefix(Department department, DepartmentManagementLogisticsHybridController __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (ViewSettingsPatch.m_enabledTrainingDepartment)
            {
                GameDBDepartment trainingDepartment = Database.Instance.GetEntry<GameDBDepartment>(Constants.Departments.Mod.TrainingDepartment);
                if ((trainingDepartment != null) && (department.m_departmentPersistentData.m_departmentType.Entry == trainingDepartment))
                {
                    var m_staffClinicSegmentDoctorsHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentDoctors", __instance);
                    var m_staffClinicSegmentReceptionistHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentReceptionist", __instance);
                    var m_staffClinicSegmentLabHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentLab", __instance);
                    var m_staffClinicSegmentJanitorHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentJanitor", __instance);

                    m_staffClinicSegmentDoctorsHelper.Field.SetActive(true);
                    m_staffClinicSegmentReceptionistHelper.Field.SetActive(true);
                    m_staffClinicSegmentLabHelper.Field.SetActive(true);
                    m_staffClinicSegmentJanitorHelper.Field.SetActive(true);

                    m_staffClinicSegmentDoctorsHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -15f, 0f);
                    m_staffClinicSegmentReceptionistHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -40f, 0f);
                    m_staffClinicSegmentLabHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -65f, 0f);
                    m_staffClinicSegmentJanitorHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -90f, 0f);

                    m_staffClinicSegmentDoctorsHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.DOCTORS, new string[0]),
                            department.m_departmentPersistentData.m_doctors.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.DAY).Count().ToString(),
                            IconManager.ICON_DOCTOR_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentDoctorsHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.DOCTOR, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_DOCTOR_DESCRIPTION, new string[0]),
                                IconManager.ICON_DOCTOR_MANAGEMENT - 5,
                                TextAnchor.UpperLeft);
                    }

                    m_staffClinicSegmentReceptionistHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.NURSES, new string[0]),
                            department.m_departmentPersistentData.m_nurses.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.DAY).Count().ToString(),
                            IconManager.ICON_NURSE_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentReceptionistHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.NURSE, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_NURSE_DESCRIPTION, new string[0]),
                                IconManager.ICON_NURSE_MANAGEMENT - 5,
                                TextAnchor.UpperLeft);
                    }

                    m_staffClinicSegmentLabHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_TECHNOLOGISTS, new string[0]),
                            department.m_departmentPersistentData.m_labSpecialists.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.DAY).Count().ToString(),
                            IconManager.ICON_TECHNOLOGIST_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentLabHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_TECHNOLOGIST, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_LAB_SPECIALIST_DESCRIPTION, new string[0]),
                                IconManager.Instance.GetIcon(IconManager.ICON_TECHNOLOGIST_MANAGEMENT - 5),
                                TextAnchor.UpperLeft);
                    }

                    m_staffClinicSegmentJanitorHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITORS, new string[0]),
                            department.m_departmentPersistentData.m_civilianEmployees.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.DAY).Count().ToString(),
                            IconManager.ICON_JANITOR_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentJanitorHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITOR, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_JANITOR_DESCRIPTION, new string[0]),
                                IconManager.ICON_JANITOR_MANAGEMENT - 5,
                                TextAnchor.UpperLeft);
                    }

                    return false;
                }
            }

            if (ViewSettingsPatch.m_labEmployeeBiochemistry[SettingsManager.Instance.m_viewSettings].m_value)
            {
                GameDBDepartment mediacalLaboratoriesDepartment = Database.Instance.GetEntry<GameDBDepartment>(Constants.Departments.Vanilla.MedicalLaboratories);
                if ((mediacalLaboratoriesDepartment != null) && (department.m_departmentPersistentData.m_departmentType.Entry == mediacalLaboratoriesDepartment))
                {
                    var m_staffClinicSegmentDoctorsHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentDoctors", __instance);
                    var m_staffClinicSegmentReceptionistHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentReceptionist", __instance);
                    var m_staffClinicSegmentLabHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentLab", __instance);
                    var m_staffClinicSegmentJanitorHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentJanitor", __instance);

                    m_staffClinicSegmentDoctorsHelper.Field.SetActive(false);
                    m_staffClinicSegmentReceptionistHelper.Field.SetActive(false);
                    m_staffClinicSegmentLabHelper.Field.SetActive(true);
                    m_staffClinicSegmentJanitorHelper.Field.SetActive(true);

                    //m_staffClinicSegmentDoctorsHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -15f, 0f);
                    //m_staffClinicSegmentReceptionistHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -40f, 0f);
                    m_staffClinicSegmentLabHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -15f, 0f);
                    m_staffClinicSegmentJanitorHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -40f, 0f);

                    m_staffClinicSegmentLabHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_BIOCHEMISTRY, new string[0]),
                            department.m_departmentPersistentData.m_labSpecialists.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.DAY).Count().ToString(),
                            Icons.ICON_BIOCHEMISTRY_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentLabHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_BIOCHEMISTRY, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_BIOCHEMISTRY_DESCRIPTION, new string[0]),
                                IconManager.Instance.GetIcon(Icons.ICON_BIOCHEMISTRY_MANAGEMENT - 5),
                                TextAnchor.UpperLeft);
                    }

                    m_staffClinicSegmentJanitorHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITORS, new string[0]),
                            department.m_departmentPersistentData.m_civilianEmployees.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.DAY).Count().ToString(),
                            IconManager.ICON_JANITOR_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentJanitorHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITOR, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITOR_DESCRIPTION, new string[0]),
                                IconManager.ICON_JANITOR_MANAGEMENT - 5,
                                TextAnchor.UpperLeft);
                    }

                    return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DepartmentManagementLogisticsHybridController), "UpdateClinicStaffSegmentsNight")]
        public static bool UpdateClinicStaffSegmentsNightPrefix(Department department, DepartmentManagementLogisticsHybridController __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (ViewSettingsPatch.m_enabledTrainingDepartment)
            {
                GameDBDepartment trainingDepartment = Database.Instance.GetEntry<GameDBDepartment>(Constants.Departments.Mod.TrainingDepartment);
                if ((trainingDepartment != null) && (department.m_departmentPersistentData.m_departmentType.Entry == trainingDepartment))
                {
                    var m_staffClinicSegmentDoctorsNightHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentDoctorsNight", __instance);
                    var m_staffClinicSegmentReceptionistNightHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentReceptionistNight", __instance);
                    var m_staffClinicSegmentLabNightHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentLabNight", __instance);
                    var m_staffClinicSegmentJanitorNightHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentJanitorNight", __instance);

                    m_staffClinicSegmentDoctorsNightHelper.Field.SetActive(true);
                    m_staffClinicSegmentReceptionistNightHelper.Field.SetActive(true);
                    m_staffClinicSegmentLabNightHelper.Field.SetActive(true);
                    m_staffClinicSegmentJanitorNightHelper.Field.SetActive(true);

                    m_staffClinicSegmentDoctorsNightHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -15f, 0f);
                    m_staffClinicSegmentReceptionistNightHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -40f, 0f);
                    m_staffClinicSegmentLabNightHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -65f, 0f);
                    m_staffClinicSegmentJanitorNightHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -90f, 0f);

                    m_staffClinicSegmentDoctorsNightHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.DOCTORS, new string[0]),
                            department.m_departmentPersistentData.m_doctors.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT).Count().ToString(),
                            IconManager.ICON_DOCTOR_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentDoctorsNightHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.DOCTOR, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_DOCTOR_DESCRIPTION, new string[0]),
                                IconManager.ICON_DOCTOR_MANAGEMENT - 5,
                                TextAnchor.UpperLeft);
                    }

                    m_staffClinicSegmentReceptionistNightHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.NURSES, new string[0]),
                            department.m_departmentPersistentData.m_nurses.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT).Count().ToString(),
                            IconManager.ICON_NURSE_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentReceptionistNightHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.NURSE, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_NURSE_DESCRIPTION, new string[0]),
                                IconManager.ICON_NURSE_MANAGEMENT - 5,
                                TextAnchor.UpperLeft);
                    }

                    m_staffClinicSegmentLabNightHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_TECHNOLOGISTS, new string[0]),
                            department.m_departmentPersistentData.m_labSpecialists.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT).Count().ToString(),
                            IconManager.ICON_TECHNOLOGIST_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentLabNightHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_TECHNOLOGIST, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_LAB_SPECIALIST_DESCRIPTION, new string[0]),
                                IconManager.Instance.GetIcon(IconManager.ICON_TECHNOLOGIST_MANAGEMENT - 5),
                                TextAnchor.UpperLeft);
                    }

                    m_staffClinicSegmentJanitorNightHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITORS, new string[0]),
                            department.m_departmentPersistentData.m_civilianEmployees.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT).Count().ToString(),
                            IconManager.ICON_JANITOR_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentJanitorNightHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITOR, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_JANITOR_DESCRIPTION, new string[0]),
                                IconManager.ICON_JANITOR_MANAGEMENT - 5,
                                TextAnchor.UpperLeft);
                    }

                    return false;
                }
            }

            if (ViewSettingsPatch.m_labEmployeeBiochemistry[SettingsManager.Instance.m_viewSettings].m_value)
            {
                GameDBDepartment mediacalLaboratoriesDepartment = Database.Instance.GetEntry<GameDBDepartment>(Constants.Departments.Vanilla.MedicalLaboratories);
                if ((mediacalLaboratoriesDepartment != null) && (department.m_departmentPersistentData.m_departmentType.Entry == mediacalLaboratoriesDepartment))
                {
                    var m_staffClinicSegmentDoctorsNightHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentDoctorsNight", __instance);
                    var m_staffClinicSegmentReceptionistNightHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentReceptionistNight", __instance);
                    var m_staffClinicSegmentLabNightHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentLabNight", __instance);
                    var m_staffClinicSegmentJanitorNightHelper = new PrivateFieldAccessHelper<DepartmentManagementLogisticsHybridController, GameObject>("m_staffClinicSegmentJanitorNight", __instance);

                    m_staffClinicSegmentDoctorsNightHelper.Field.SetActive(false);
                    m_staffClinicSegmentReceptionistNightHelper.Field.SetActive(false);
                    m_staffClinicSegmentLabNightHelper.Field.SetActive(true);
                    m_staffClinicSegmentJanitorNightHelper.Field.SetActive(true);

                    //m_staffClinicSegmentDoctorsNightHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -15f, 0f);
                    //m_staffClinicSegmentReceptionistNightHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -40f, 0f);
                    m_staffClinicSegmentLabNightHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -15f, 0f);
                    m_staffClinicSegmentJanitorNightHelper.Field.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -40f, 0f);

                    m_staffClinicSegmentLabNightHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_BIOCHEMISTRY, new string[0]),
                            department.m_departmentPersistentData.m_labSpecialists.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT).Count().ToString(),
                            Icons.ICON_BIOCHEMISTRY_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentLabNightHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_BIOCHEMISTRY, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_BIOCHEMISTRY_DESCRIPTION, new string[0]),
                                IconManager.Instance.GetIcon(Icons.ICON_BIOCHEMISTRY_MANAGEMENT - 5),
                                TextAnchor.UpperLeft);
                    }

                    m_staffClinicSegmentJanitorNightHelper.Field
                        .GetComponent<SegmentItemIconTextTextIconController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITORS, new string[0]),
                            department.m_departmentPersistentData.m_civilianEmployees.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT).Count().ToString(),
                            IconManager.ICON_JANITOR_MANAGEMENT,
                            UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                    if (m_staffClinicSegmentJanitorNightHelper.Field.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                    {
                        TooltipManager.Instance
                            .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                            .UpdateData(
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITOR, new string[0]),
                                StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITOR_DESCRIPTION, new string[0]),
                                IconManager.ICON_JANITOR_MANAGEMENT - 5,
                                TextAnchor.UpperLeft);
                    }

                    return false;
                }
            }

            return true;
        }
    }
}
