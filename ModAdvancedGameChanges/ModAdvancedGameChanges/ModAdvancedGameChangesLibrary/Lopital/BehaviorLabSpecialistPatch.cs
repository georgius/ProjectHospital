using GLib;
using HarmonyLib;
using Lopital;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(BehaviorLabSpecialist))]
    public static class BehaviorLabSpecialistPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorLabSpecialist), nameof(BehaviorLabSpecialist.AddToHospital))]
        public static bool AddToHospitalPrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, added to hospital, trying to find common room");

            EmployeeComponent employeeComponent = __instance.GetComponent<EmployeeComponent>();

            employeeComponent.m_state.m_department = MapScriptInterface.Instance.GetActiveDepartment();
            employeeComponent.m_state.m_startDay = DayTime.Instance.GetDay();

            Vector3i position = BehaviorPatch.GetCommonRoomFreePlace(__instance);

            if (position == Vector3i.ZERO_VECTOR)
            {
                // not found common room, stay at home
                //BehaviorJanitorPatch.GoHomeMod(__instance);
                return false;
            }

            __instance.GetComponent<WalkComponent>().SetDestination(new Vector2i(position.m_x, position.m_y), position.m_z, MovementType.WALKING);
            __instance.SwitchState(BehaviorJanitorState.Walking);

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name}, going to common room");

            return false;
        }
    }
}
