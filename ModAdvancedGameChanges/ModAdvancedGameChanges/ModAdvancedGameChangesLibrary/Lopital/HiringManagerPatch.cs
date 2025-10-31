using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;

namespace ModAdvancedGameChanges .Lopital
{
    [HarmonyPatch(typeof(HiringManager))]
    public static class HiringManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HiringManager), nameof(HiringManager.GenerateAvailableCharacters))]
        public static bool GenerateAvailableCharactersPrefix(AvailableCharacters availableCharacters, GameDBDepartment gameDBDepartment, LopitalTypes characterType, bool forceAllcharacters, HiringManager __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_forceEmployeeLowestHireLevel[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            int numberOfCharactersForHire = SettingsManager.Instance.m_gameSettings.m_numberOfCharactersForHire;

            if (forceAllcharacters || characterType == LopitalTypes.CharacterDoctor)
            {
                for (int i = availableCharacters.m_availableNormalDoctors.Count; i < numberOfCharactersForHire; i++)
                {
                    Entity entity = LopitalEntityFactory.CreateCharacterDoctor(gameDBDepartment, null, Vector2i.ZERO_VECTOR, Levels.Doctors.Intern, Levels.Doctors.Intern, null, null);
                    entity.GetComponent<EmployeeComponent>().m_state.m_hiredForDepartment = gameDBDepartment;
                    entity.GetComponent<EmployeeComponent>().m_state.m_hiredLevel = entity.GetComponent<EmployeeComponent>().m_state.m_level;
                    entity.GetComponent<EmployeeComponent>().m_state.m_hiredSalaryRandomization = UnityEngine.Random.Range(0f, 1f);
                    availableCharacters.m_availableNormalDoctors.Add(entity);
                    PortraitManager.Instance.CreatePortraitSlot(entity);
                    entity.GetComponent<EmployeeComponent>().m_state.m_employeeType = LopitalTypes.CharacterDoctor;
                }
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterDoctorWithInterns)
            {
                for (int i = availableCharacters.m_availableDoctorsWithInterns.Count; i < numberOfCharactersForHire; i++)
                {
                    Entity entity = LopitalEntityFactory.CreateCharacterDoctor(gameDBDepartment, null, Vector2i.ZERO_VECTOR, Levels.Doctors.Intern, Levels.Doctors.Intern, null, null);
                    entity.GetComponent<EmployeeComponent>().m_state.m_hiredForDepartment = gameDBDepartment;
                    entity.GetComponent<EmployeeComponent>().m_state.m_hiredLevel = entity.GetComponent<EmployeeComponent>().m_state.m_level;
                    entity.GetComponent<EmployeeComponent>().m_state.m_hiredSalaryRandomization = UnityEngine.Random.Range(0f, 1f);
                    availableCharacters.m_availableDoctorsWithInterns.Add(entity);
                    PortraitManager.Instance.CreatePortraitSlot(entity);
                    entity.GetComponent<EmployeeComponent>().m_state.m_employeeType = LopitalTypes.CharacterDoctorWithInterns;
                }
            }

            if (gameDBDepartment.SecondDoctorSpecializations != null)
            {
                if (forceAllcharacters || characterType == LopitalTypes.CharacterSurgeon)
                {
                    availableCharacters.m_availableSurgeons?.Clear();
                }

                if (forceAllcharacters || characterType == LopitalTypes.CharacterAnesteziologist)
                {
                    availableCharacters.m_availableAnesteziologists?.Clear();
                }

                if (forceAllcharacters || characterType == LopitalTypes.CharacterAdvancedDiagnoses)
                {
                    availableCharacters.m_availableAdvancedDiagnoses?.Clear();
                }
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterNurse)
            {
                for (int i = availableCharacters.m_availableNormalNurses.Count; i < numberOfCharactersForHire; i++)
                {
                    Entity entity = LopitalEntityFactory.CreateCharacterNurse(null, Vector2i.ZERO_VECTOR, Levels.Nurses.NursingIntern, Levels.Nurses.NursingIntern, null, null);
                    entity.GetComponent<EmployeeComponent>().m_state.m_hiredForDepartment = gameDBDepartment;
                    entity.GetComponent<EmployeeComponent>().m_state.m_hiredLevel = entity.GetComponent<EmployeeComponent>().m_state.m_level;
                    entity.GetComponent<EmployeeComponent>().m_state.m_hiredSalaryRandomization = UnityEngine.Random.Range(0f, 1f);
                    availableCharacters.m_availableNormalNurses.Add(entity);
                    PortraitManager.Instance.CreatePortraitSlot(entity);
                    entity.GetComponent<EmployeeComponent>().m_state.m_employeeType = LopitalTypes.CharacterNurse;
                }
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterReceptionist)
            {
                availableCharacters.m_availableReceptionists?.Clear();
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterSurgeryNurse)
            {
                availableCharacters.m_availableSurgeryNurses?.Clear();
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterSpecialistNurse)
            {
                availableCharacters.m_availableSpecialistNurses?.Clear();
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterTechnologist)
            {
                for (int i = availableCharacters.m_availableTechnologists.Count; i < numberOfCharactersForHire; i++)
                {
                    Entity entity10 = LopitalEntityFactory.CreateCharacterLabSpecialist(gameDBDepartment, null, Vector2i.ZERO_VECTOR, Levels.LabSpecialists.JuniorScientist, Levels.LabSpecialists.JuniorScientist, null, null);
                    entity10.GetComponent<EmployeeComponent>().m_state.m_hiredForDepartment = gameDBDepartment;
                    entity10.GetComponent<EmployeeComponent>().m_state.m_hiredLevel = entity10.GetComponent<EmployeeComponent>().m_state.m_level;
                    entity10.GetComponent<EmployeeComponent>().m_state.m_hiredSalaryRandomization = UnityEngine.Random.Range(0f, 1f);
                    availableCharacters.m_availableTechnologists.Add(entity10);
                    PortraitManager.Instance.CreatePortraitSlot(entity10);
                    entity10.GetComponent<EmployeeComponent>().m_state.m_employeeType = LopitalTypes.CharacterTechnologist;
                }
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterAdvancedBiochemist)
            {
                availableCharacters.m_availableAdvancedBiochemists?.Clear();
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterRadiolog)
            {
                availableCharacters.m_availableRadiologists?.Clear();
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterPharmacologist)
            {
                availableCharacters.m_availablePharmacologists?.Clear();
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterVendorJanitor)
            {
                availableCharacters.m_availableVendorJanitors?.Clear();
                
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterManagerJanitor)
            {
                availableCharacters.m_availableManagerJanitors?.Clear();
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterUSGTechnologist)
            {
                availableCharacters.m_availableUSGTechnologists?.Clear();
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterNeurolog)
            {
                availableCharacters.m_availableNeurologists?.Clear();
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterCardiolog)
            {
                availableCharacters.m_availableCardiologists?.Clear();
            }

            if (forceAllcharacters || characterType == LopitalTypes.CharacterJanitor)
            {
                for (int i = availableCharacters.m_availableNormalJanitors.Count; i < numberOfCharactersForHire; i++)
                {
                    Entity entity19 = LopitalEntityFactory.CreateCharacterJanitor(null, Vector2i.ZERO_VECTOR, Levels.Janitors.Janitor, Levels.Janitors.Janitor, null, null);
                    entity19.GetComponent<EmployeeComponent>().m_state.m_hiredForDepartment = gameDBDepartment;
                    entity19.GetComponent<EmployeeComponent>().m_state.m_hiredLevel = entity19.GetComponent<EmployeeComponent>().m_state.m_level;
                    entity19.GetComponent<EmployeeComponent>().m_state.m_hiredSalaryRandomization = UnityEngine.Random.Range(0f, 1f);
                    availableCharacters.m_availableNormalJanitors.Add(entity19);
                    PortraitManager.Instance.CreatePortraitSlot(entity19);
                    entity19.GetComponent<EmployeeComponent>().m_state.m_employeeType = LopitalTypes.CharacterJanitor;
                }
            }

            return false;
        }
    }
}
