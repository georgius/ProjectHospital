using HarmonyLib;
using Lopital;
using System;
using System.Reflection;
using UnityEngine;
using System.Linq;

namespace ModAdvancedGameChanges
{
    [HarmonyPatch(typeof(DepartmentManagementLogisticsHybridController))]
    public static class DepartmentManagementLogisticsHybridControllerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DepartmentManagementLogisticsHybridController), "UpdateClinicStaffSegments")]
        public static bool UpdateClinicStaffSegmentsPrefix(Department department, DepartmentManagementLogisticsHybridController __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            GameDBDepartment trainingDepartment = Database.Instance.GetEntry<GameDBDepartment>(Constants.Departments.Mod.TrainingDepartment);
            if ((trainingDepartment != null) && (department.m_departmentPersistentData.m_departmentType.Entry == trainingDepartment))
            {
                // Get the Type of the class
                Type type = __instance.GetType();

                // Get the private field using BindingFlags
                FieldInfo m_staffClinicSegmentDoctorsFieldInfo = type.GetField("m_staffClinicSegmentDoctors", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo m_staffClinicSegmentReceptionistFieldInfo = type.GetField("m_staffClinicSegmentReceptionist", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo m_staffClinicSegmentLabFieldInfo = type.GetField("m_staffClinicSegmentLab", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo m_staffClinicSegmentJanitorFieldInfo = type.GetField("m_staffClinicSegmentJanitor", BindingFlags.NonPublic | BindingFlags.Instance);

                // get objects
                GameObject m_staffClinicSegmentDoctors = (GameObject)m_staffClinicSegmentDoctorsFieldInfo.GetValue(__instance);
                GameObject m_staffClinicSegmentReceptionist = (GameObject)m_staffClinicSegmentReceptionistFieldInfo.GetValue(__instance);
                GameObject m_staffClinicSegmentLab = (GameObject)m_staffClinicSegmentLabFieldInfo.GetValue(__instance);
                GameObject m_staffClinicSegmentJanitor = (GameObject)m_staffClinicSegmentJanitorFieldInfo.GetValue(__instance);

                m_staffClinicSegmentDoctors.SetActive(true);
                m_staffClinicSegmentReceptionist.SetActive(true);
                m_staffClinicSegmentLab.SetActive(true);
                m_staffClinicSegmentJanitor.SetActive(true);

                m_staffClinicSegmentDoctors.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -15f, 0f);
                m_staffClinicSegmentReceptionist.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -40f, 0f);
                m_staffClinicSegmentLab.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -65f, 0f);
                m_staffClinicSegmentJanitor.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -90f, 0f);

                m_staffClinicSegmentDoctors
                    .GetComponent<SegmentItemIconTextTextIconController>()
                    .UpdateData(
                        StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.DOCTORS, new string[0]),
                        department.m_departmentPersistentData.m_doctors.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.DAY).Count().ToString(),
                        IconManager.ICON_DOCTOR_MANAGEMENT, 
                        UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                if (m_staffClinicSegmentDoctors.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                {
                    TooltipManager.Instance
                        .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.DOCTOR, new string[0]),
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_DOCTOR_DESCRIPTION, new string[0]),
                            IconManager.ICON_DOCTOR_MANAGEMENT - 5, 
                            TextAnchor.UpperLeft);
                }

                m_staffClinicSegmentReceptionist
                    .GetComponent<SegmentItemIconTextTextIconController>()
                    .UpdateData(
                        StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.NURSES, new string[0]),
                        department.m_departmentPersistentData.m_nurses.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.DAY).Count().ToString(), 
                        IconManager.ICON_NURSE_MANAGEMENT, 
                        UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                if (m_staffClinicSegmentReceptionist.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                {
                    TooltipManager.Instance
                        .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.NURSE, new string[0]),
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_NURSE_DESCRIPTION, new string[0]),
                            IconManager.ICON_NURSE_MANAGEMENT - 5, 
                            TextAnchor.UpperLeft);
                }

                m_staffClinicSegmentLab
                    .GetComponent<SegmentItemIconTextTextIconController>()
                    .UpdateData(
                        StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_TECHNOLOGISTS, new string[0]),
                        department.m_departmentPersistentData.m_labSpecialists.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.DAY).Count().ToString(),
                        IconManager.ICON_TECHNOLOGIST_MANAGEMENT,
                        UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                if (m_staffClinicSegmentLab.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                {
                    TooltipManager.Instance
                        .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_TECHNOLOGIST, new string[0]),
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_LAB_SPECIALIST_DESCRIPTION, new string[0]),
                            IconManager.Instance.GetIcon(IconManager.ICON_TECHNOLOGIST_MANAGEMENT - 5), 
                            TextAnchor.UpperLeft);
                }

                m_staffClinicSegmentJanitor
                    .GetComponent<SegmentItemIconTextTextIconController>()
                    .UpdateData(
                        StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITORS, new string[0]),
                        department.m_departmentPersistentData.m_civilianEmployees.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.DAY).Count().ToString(),
                        IconManager.ICON_JANITOR_MANAGEMENT, 
                        UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                if (m_staffClinicSegmentJanitor.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
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

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DepartmentManagementLogisticsHybridController), "UpdateClinicStaffSegmentsNight")]
        public static bool UpdateClinicStaffSegmentsNightPrefix(Department department, DepartmentManagementLogisticsHybridController __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            GameDBDepartment trainingDepartment = Database.Instance.GetEntry<GameDBDepartment>(Constants.Departments.Mod.TrainingDepartment);
            if ((trainingDepartment != null) && (department.m_departmentPersistentData.m_departmentType.Entry == trainingDepartment))
            {
                // Get the Type of the class
                Type type = __instance.GetType();

                // Get the private field using BindingFlags
                FieldInfo m_staffClinicSegmentDoctorsNightFieldInfo = type.GetField("m_staffClinicSegmentDoctorsNight", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo m_staffClinicSegmentReceptionistNightFieldInfo = type.GetField("m_staffClinicSegmentReceptionistNight", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo m_staffClinicSegmentLabNightFieldInfo = type.GetField("m_staffClinicSegmentLabNight", BindingFlags.NonPublic | BindingFlags.Instance);
                FieldInfo m_staffClinicSegmentJanitorNightFieldInfo = type.GetField("m_staffClinicSegmentJanitorNight", BindingFlags.NonPublic | BindingFlags.Instance);

                // get objects
                GameObject m_staffClinicSegmentDoctorsNight = (GameObject)m_staffClinicSegmentDoctorsNightFieldInfo.GetValue(__instance);
                GameObject m_staffClinicSegmentReceptionistNight = (GameObject)m_staffClinicSegmentReceptionistNightFieldInfo.GetValue(__instance);
                GameObject m_staffClinicSegmentLabNight = (GameObject)m_staffClinicSegmentLabNightFieldInfo.GetValue(__instance);
                GameObject m_staffClinicSegmentJanitorNight = (GameObject)m_staffClinicSegmentJanitorNightFieldInfo.GetValue(__instance);

                m_staffClinicSegmentDoctorsNight.SetActive(true);
                m_staffClinicSegmentReceptionistNight.SetActive(true);
                m_staffClinicSegmentLabNight.SetActive(true);
                m_staffClinicSegmentJanitorNight.SetActive(true);

                m_staffClinicSegmentDoctorsNight.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -15f, 0f);
                m_staffClinicSegmentReceptionistNight.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -40f, 0f);
                m_staffClinicSegmentLabNight.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -65f, 0f);
                m_staffClinicSegmentJanitorNight.GetComponent<RectTransform>().anchoredPosition = new Vector3(40f, -90f, 0f);

                m_staffClinicSegmentDoctorsNight
                    .GetComponent<SegmentItemIconTextTextIconController>()
                    .UpdateData(
                        StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.DOCTORS, new string[0]),
                        department.m_departmentPersistentData.m_doctors.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT).Count().ToString(),
                        IconManager.ICON_DOCTOR_MANAGEMENT,
                        UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                if (m_staffClinicSegmentDoctorsNight.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                {
                    TooltipManager.Instance
                        .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.DOCTOR, new string[0]),
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_DOCTOR_DESCRIPTION, new string[0]),
                            IconManager.ICON_DOCTOR_MANAGEMENT - 5,
                            TextAnchor.UpperLeft);
                }

                m_staffClinicSegmentReceptionistNight
                    .GetComponent<SegmentItemIconTextTextIconController>()
                    .UpdateData(
                        StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.NURSES, new string[0]),
                        department.m_departmentPersistentData.m_nurses.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT).Count().ToString(),
                        IconManager.ICON_NURSE_MANAGEMENT,
                        UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                if (m_staffClinicSegmentReceptionistNight.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                {
                    TooltipManager.Instance
                        .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.NURSE, new string[0]),
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_NURSE_DESCRIPTION, new string[0]),
                            IconManager.ICON_NURSE_MANAGEMENT - 5,
                            TextAnchor.UpperLeft);
                }

                m_staffClinicSegmentLabNight
                    .GetComponent<SegmentItemIconTextTextIconController>()
                    .UpdateData(
                        StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_TECHNOLOGISTS, new string[0]),
                        department.m_departmentPersistentData.m_labSpecialists.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT).Count().ToString(),
                        IconManager.ICON_TECHNOLOGIST_MANAGEMENT,
                        UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                if (m_staffClinicSegmentLabNight.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
                {
                    TooltipManager.Instance
                        .GetTooltipComponent<TooltipHeadingIconSmallTextController>()
                        .UpdateData(
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.LAB_TECHNOLOGIST, new string[0]),
                            StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Mod.AGC_LAB_SPECIALIST_DESCRIPTION, new string[0]),
                            IconManager.Instance.GetIcon(IconManager.ICON_TECHNOLOGIST_MANAGEMENT - 5),
                            TextAnchor.UpperLeft);
                }

                m_staffClinicSegmentJanitorNight
                    .GetComponent<SegmentItemIconTextTextIconController>()
                    .UpdateData(
                        StringTable.GetInstance().GetLocalizedText(Constants.Localizations.Vanilla.JANITORS, new string[0]),
                        department.m_departmentPersistentData.m_civilianEmployees.Where(n => n.GetEntity().GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT).Count().ToString(),
                        IconManager.ICON_JANITOR_MANAGEMENT,
                        UISettings.Instance.EXAMINATION_COLOR_SUCCESSFUL);
                if (m_staffClinicSegmentJanitorNight.GetComponentInChildren<HoverTooltipDelay>().ShouldShowToolTip())
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

            return true;
        }
    }
}
