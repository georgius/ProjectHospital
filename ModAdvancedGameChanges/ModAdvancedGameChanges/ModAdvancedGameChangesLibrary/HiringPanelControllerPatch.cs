using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using UnityEngine;

namespace ModAdvancedGameChanges
{
    [HarmonyPatch(typeof(HiringPanelController))]
    public static class HiringPanelControllerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HiringPanelController), nameof(HiringPanelController.SortTechnologistButtons))]
        public static bool SortTechnologistButtonsPrefix(bool clinic, HiringPanelController __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_labEmployeeBiochemistry[SettingsManager.Instance.m_viewSettings].m_value))
            {
                // Allow original method to run
                return true;
            }

            GameDBDepartment departmentType = Hospital.Instance.m_activeDepartment.GetEntity().GetDepartmentType();

            if (departmentType == Database.Instance.GetEntry<GameDBDepartment>(Departments.Vanilla.MedicalLaboratories))
            {
                for (int i = 0; i < __instance.m_technologistCharacters.Count; i++)
                {
                    __instance.m_technologistCharacters[i].SetActive(false);
                }

                __instance.m_technologistCharacters[1].SetActive(true);
                __instance.m_technologistCharacters[1].GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, 0f, 0f);

                return false;
            }

            return true;
        }
    }
}
