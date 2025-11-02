using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System;
using System.Reflection;

namespace ModAdvancedGameChanges
{
    [HarmonyPatch(typeof(CharacterPanelController))]
    public static class CharacterPanelControllerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CharacterPanelController), nameof(CharacterPanelController.OnDepartmentSelected))]
        public static bool OnDepartmentSelectedPrefix(int index, CharacterPanelController __instance, ref bool __result)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Department department = Hospital.Instance.m_departments[index];
            Entity entity = (__instance.GetCharacter() != null) ? __instance.GetCharacter().GetEntity() : null;
            EmployeeComponent employeeComponent = entity?.GetComponent<EmployeeComponent>();
            BehaviorDoctor doctor = entity?.GetComponent<BehaviorDoctor>();
            BehaviorNurse nurse = entity?.GetComponent<BehaviorNurse>();
            BehaviorLabSpecialist labSpecialist = entity?.GetComponent<BehaviorLabSpecialist>();
            BehaviorJanitor janitor = entity?.GetComponent<BehaviorJanitor>();

            if (employeeComponent != null)
            {
                bool allowedSwitchDepartment = false;

                if (doctor != null)
                {
                    allowedSwitchDepartment |= (doctor.m_state.m_doctorState == DoctorState.AtHome);

                    allowedSwitchDepartment &= !(Tweakable.Vanilla.DlcHospitalServicesEnabled() && (department.m_departmentPersistentData.m_departmentType == Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.AdministrativeDepartment)));
                    allowedSwitchDepartment &= (department.m_departmentPersistentData.m_departmentType != Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Radiology));
                    allowedSwitchDepartment &= (department.m_departmentPersistentData.m_departmentType != Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.MedicalLaboratories));
                }
                else if (nurse != null)
                {
                    allowedSwitchDepartment |= (nurse.m_state.m_nurseState == NurseState.AtHome);

                    allowedSwitchDepartment &= !(Tweakable.Vanilla.DlcHospitalServicesEnabled() && (department.m_departmentPersistentData.m_departmentType == Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.AdministrativeDepartment)));
                    allowedSwitchDepartment &= (department.m_departmentPersistentData.m_departmentType != Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Radiology));
                    allowedSwitchDepartment &= (department.m_departmentPersistentData.m_departmentType != Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.MedicalLaboratories));
                }
                else if (labSpecialist != null)
                {
                    allowedSwitchDepartment |= (labSpecialist.m_state.m_labSpecialistState == LabSpecialistState.AtHome);

                    allowedSwitchDepartment &= !(Tweakable.Vanilla.DlcHospitalServicesEnabled() && (department.m_departmentPersistentData.m_departmentType == Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.Pathology)));
                    allowedSwitchDepartment &= (department.m_departmentPersistentData.m_departmentType != Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.IntensiveCareUnit));
                }
                else if (janitor != null)
                {
                    allowedSwitchDepartment |= (janitor.m_state.m_janitorState == BehaviorJanitorState.AtHome);
                }

                if (!allowedSwitchDepartment)
                {
                    // employee is doing something, we can't swith department

                    UISoundManager.sm_instance.PlaySoundEvent("SFX_UI_FORBIDDEN", 1f);

                    __result = false;
                    return false;
                }
                else
                {
                    UISoundManager.sm_instance.PlaySoundEvent("SFX_UI_BEEP", 1f);
                    employeeComponent.SwitchDepartment(department);
                    employeeComponent.m_state.m_homeRoom = null;

                    __result = true;
                    return false;
                }
            }

            return true;
        }

        private static EntityIDPointer<Entity> GetCharacter(this CharacterPanelController instance)
        {
            // Get the Type of the class
            Type type = typeof(CharacterPanelController);

            // Get the private field using BindingFlags
            FieldInfo m_characterFieldInfo = type.GetField("m_character", BindingFlags.NonPublic | BindingFlags.Instance);

            // get objects
            return (EntityIDPointer<Entity>)m_characterFieldInfo.GetValue(instance);
        }
    }
}
