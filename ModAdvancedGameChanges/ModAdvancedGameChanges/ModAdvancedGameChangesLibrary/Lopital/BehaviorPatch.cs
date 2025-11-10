using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System.Collections.Generic;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(Behavior))]
    public static class BehaviorPatch
    {
        public static void ChooseSkillToTrainAndToggleTraining(Behavior instance)
        {
            EmployeeComponent employeeComponent = instance.GetComponent<EmployeeComponent>();

            List<Skill> skills = new List<Skill>();

            if (employeeComponent.m_state.m_skillSet.m_qualifications != null)
            {
                foreach (var skill in employeeComponent.m_state.m_skillSet.m_qualifications)
                {
                    if (skill.m_level < Skills.SkillLevelMaximum)
                    {
                        skills.Add(skill);
                    }
                }
            }
            if (employeeComponent.m_state.m_skillSet.m_specialization1 != null)
            {
                if (employeeComponent.m_state.m_skillSet.m_specialization1.m_level < Skills.SkillLevelMaximum)
                {
                    skills.Add(employeeComponent.m_state.m_skillSet.m_specialization1);
                }
            }
            if (employeeComponent.m_state.m_skillSet.m_specialization2 != null)
            {
                if (employeeComponent.m_state.m_skillSet.m_specialization2.m_level < Skills.SkillLevelMaximum)
                {
                    skills.Add(employeeComponent.m_state.m_skillSet.m_specialization2);
                }
            }

            if (skills.Count != 0)
            {
                Skill skillToTrain = skills[UnityEngine.Random.Range(0, skills.Count)];

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{instance.m_entity.Name}, training skill {skillToTrain.m_gameDBSkill.Entry.DatabaseID}");
                employeeComponent.ToggleTraining(skillToTrain);
            }
        }


        public static bool ReceiveMessage(Message message, Behavior instance, bool patient)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            if (message.m_messageID == Messages.BLADDER_REDUCED)
            {
                instance.GetComponent<MoodComponent>().GetNeed(Needs.Vanilla.Bladder).ReduceRandomized(message.m_mainParameter, instance.GetComponent<MoodComponent>());

                return false;
            }

            if (message.m_messageID == Messages.HUNGER_REDUCED)
            {
                instance.GetComponent<MoodComponent>().GetNeed(patient ? Needs.Vanilla.HungerPatient : Needs.Vanilla.HungerStaff).ReduceRandomized(message.m_mainParameter, instance.GetComponent<MoodComponent>());

                return false;
            }

            if (message.m_messageID == Messages.BOREDOM_REDUCED)
            {
                instance.GetComponent<MoodComponent>().GetNeed(Needs.Vanilla.Boredom).ReduceRandomized(message.m_mainParameter, instance.GetComponent<MoodComponent>());

                return false;
            }

            if (message.m_messageID == Messages.REST_REDUCED)
            {
                instance.GetComponent<MoodComponent>().GetNeed(Needs.Vanilla.Rest).ReduceRandomized(message.m_mainParameter, instance.GetComponent<MoodComponent>());

                return false;
            }

            return true;
        }
    }
}
