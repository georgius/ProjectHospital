using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges;
using System;

namespace ModGameChanges.Lopital
{
    [HarmonyPatch(typeof(BehaviorJanitor))]
    public static class BehaviorJanitorPatch
    {
        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(BehaviorJanitor), "TryToSelectTileInCurrentRoom")]
        //public static bool TryToSelectTileInCurrentRoomPrefix(BehaviorJanitor __instance)
        //{
        //    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), "Employee: " + __instance.m_entity.Name);

        //    __instance.GetComponent<EmployeeComponent>().AddSkillPoints("SKILL_JANITOR_QUALIF_EFFICIENCY", Database.Instance.GetEntry<GameDBTweakableInt>("TWEAKABLE_MAIN_SKILL_POINTS").Value, true);
        //    __instance.GetComponent<EmployeeComponent>().AddSkillPoints("SKILL_JANITOR_QUALIF_DEXTERITY", Database.Instance.GetEntry<GameDBTweakableInt>("TWEAKABLE_JANITOR_DEXTERITY_SKILL_POINTS").Value, true);

        //    return true;
        //}

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateAtHome")]
        public static bool UpdateStateAtHomePrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            __instance.GetComponent<EmployeeComponent>().CheckNoWorkplaceAtHome();

			if (DayTime.Instance.GetShift() == __instance.GetComponent<EmployeeComponent>().m_state.m_shift && !__instance.GetDepartment().IsClosed())
			{
				if (__instance.GetComponent<EmployeeComponent>().m_state.m_shift == Shift.NIGHT)
				{
					__instance.m_state.m_hadLunch = true;
				}

				GameDBRoomType homeRoomType = __instance.GetComponent<EmployeeComponent>().GetHomeRoomType();
				if (homeRoomType != null && homeRoomType.HasTag(Constants.Tags.Mod.JanitorTrainingWorkspace))
				{
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - going to training workplace");

                    __instance.GoToWorkPlace();
					return false;
				}
			}

			return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BehaviorJanitor), "UpdateStateGoingToWorkplace")]
        public static bool UpdateStateGoingToWorkplacePrefix(BehaviorJanitor __instance)
        {
            if ((!ViewSettingsPatch.m_enabled) || (!ViewSettingsPatch.m_enabledTrainingDepartment))
            {
                // Allow original method to run
                return true;
            }

            if (!__instance.GetComponent<WalkComponent>().IsBusy())
            {
                GameDBRoomType homeRoomType = __instance.GetComponent<EmployeeComponent>().GetHomeRoomType();

                if (homeRoomType != null && homeRoomType.HasTag(Constants.Tags.Mod.JanitorTrainingWorkspace))
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"Employee: {__instance.m_entity.Name} - arrived to training workplace");

                    TileObject entity = __instance.GetComponent<EmployeeComponent>().m_state.m_workDesk.GetEntity();
                    if (entity != null)
                    {
                        entity.SetLightEnabled(true);
                        entity.GetComponent<AnimatedObjectComponent>().ForceFrame(1);
                    }

                    EmployeeComponent component = __instance.GetComponent<EmployeeComponent>();
                    component.CheckChiefNodiagnoseDepartment(true);
                    component.CheckRoomSatisfactionBonuses();
                    component.CheckMoodModifiers(__instance.IsBookmarked());
                    component.CheckBossModifiers();
                    component.UpdateHomeRoom();

                    if (component.m_state.m_skillSet.m_qualifications != null)
                    {
                        foreach (var skill in component.m_state.m_skillSet.m_qualifications)
                        {
                            component.ToggleTraining(skill);
                        }
                    }
                    if (component.m_state.m_skillSet.m_specialization1 != null)
                    {
                        component.ToggleTraining(component.m_state.m_skillSet.m_specialization1);
                    }
                    if (component.m_state.m_skillSet.m_specialization2 != null)
                    {
                        component.ToggleTraining(component.m_state.m_skillSet.m_specialization2);
                    }

                    __instance.SwitchState(BehaviorJanitorState.AdminIdle);

                    return false;
                }
            }

            return true;
        }
    }
}
