using GLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using System.Globalization;

namespace ModAdvancedGameChanges.Lopital
{
    public class ProcedureScriptJanitorCleaning : ProcedureScript
    {
        public ProcedureScriptJanitorCleaning() 
            : base()
        { 
        }

        public override void Activate()
        {
            Entity mainCharacter = this.m_stateData.m_procedureScene.MainCharacter;
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, activating script {this.m_stateData.m_scriptName}");

            this.SwitchState(ProcedureScriptJanitorCleaning.STATE_SEARCH_FOR_DIRTY_PLACE);
        }

        public override bool IsIdle()
        {
            return this.m_stateData.m_state == ProcedureScriptJanitorCleaning.STATE_IDLE;
        }

        public override void ScriptUpdate(float deltaTime)
        {
            this.m_stateData.m_timeInState += deltaTime;

            switch (this.m_stateData.m_state)
            {
                case ProcedureScriptJanitorCleaning.STATE_SEARCH_FOR_DIRTY_PLACE:
                    this.UpdateStateSearchForDirtyPlace();
                    break;
                case ProcedureScriptJanitorCleaning.STATE_GOING_TO_DIRTY_PLACE:
                    this.UpdateStateGoingToDirtyPlace();
                    break;
                case ProcedureScriptJanitorCleaning.STATE_CLEANING_DIRTY_PLACE:
                    this.UpdateStateCleaningDirtyPlace();
                    break;
                default:
                    break;
            }
        }

        public void UpdateStateSearchForDirtyPlace()
        {
            Entity mainCharacter = this.m_stateData.m_procedureScene.MainCharacter;
            BehaviorJanitor janitor = mainCharacter.GetComponent<BehaviorJanitor>();

            mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);

            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter.Name}, searching dirty place in current room");

            if (!janitor.TryToSelectTileInCurrentRoom())
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter.Name}, dirty tile in current room not found");

                if (!janitor.TryToSelectTileInARoom())
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter.Name}, dirty tile in room not found");

                    if (!janitor.TryToSelectIndoorTile(BehaviorJanitorPatch.DirtinessThreshold))
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter.Name}, dirty tile not found");

                        janitor.FreeObject();
                        janitor.FreeTile();
                        janitor.FreeRoom();
                        janitor.GoReturnCart();

                        this.SwitchState(ProcedureScriptJanitorCleaning.STATE_IDLE);
                        return;
                    }
                }
            }

            this.SwitchState(ProcedureScriptJanitorCleaning.STATE_GOING_TO_DIRTY_PLACE);
        }

        public void UpdateStateGoingToDirtyPlace()
        {
            Entity mainCharacter = this.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter.Name}, starting cleaning tile");

                mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.Mop, true);

                WalkComponent walkComponent = mainCharacter.GetComponent<WalkComponent>();
                PerkComponent perkComponent = mainCharacter.GetComponent<PerkComponent>();
                EmployeeComponent employeeComponent = mainCharacter.GetComponent<EmployeeComponent>();
                BehaviorJanitor janitor = mainCharacter.GetComponent<BehaviorJanitor>();

                Floor floor = Hospital.Instance.m_floors[walkComponent.GetFloorIndex()];
                DirtType dirtType = floor.m_mapPersistentData.m_tiles[walkComponent.GetCurrentTile().m_x, walkComponent.GetCurrentTile().m_y].m_dirtType;
                float dirtLevel = floor.m_mapPersistentData.m_tiles[walkComponent.GetCurrentTile().m_x, walkComponent.GetCurrentTile().m_y].m_dirtLevel;

                float cleaningTime = (dirtType == DirtType.DIRT) ? Tweakable.Mod.JanitorCleaningTimeDirtMinutes() : Tweakable.Mod.JanitorCleaningTimeBloodMinutes();

                float actionTime = this.GetActionTime(
                    mainCharacter,
                    (int)DayTime.Instance.IngameTimeHoursToRealTimeSeconds(cleaningTime / 60f),
                    mainCharacter.GetComponent<EmployeeComponent>().GetSkillLevel(Skills.Vanilla.SKILL_JANITOR_QUALIF_DEXTERITY));

                if (perkComponent.m_perkSet.HasPerk(Perks.Vanilla.Chemist))
                {
                    if (perkComponent.m_perkSet.HasHiddenPerk(Perks.Vanilla.Chemist))
                    {
                        perkComponent.RevealPerk(Perks.Vanilla.Chemist, janitor.m_state.m_bookmarked);
                    }

                    actionTime *= UnityEngine.Random.Range(0.5f, 1f);
                }

                float bonus = 0f;

                if (employeeComponent.m_state.m_supervisor != null)
                {
                    BehaviorJanitor janitorManager = employeeComponent.m_state.m_supervisor.GetEntity()?.GetComponent<BehaviorJanitor>();

                    if (janitorManager != null)
                    {
                        float penalty = UnityEngine.Random.Range(Skills.SkillLevelMinimum, Skills.SkillLevelMaximum + Skills.SkillLevelMinimum - janitorManager.GetComponent<EmployeeComponent>().m_state.m_skillSet.GetSkillLevel(Skills.Vanilla.DLC_SKILL_JANITOR_SPEC_MANAGER));
                        bonus = (float)Tweakable.Vanilla.JanitorManagerCleaningBonusPercent() / (100f * penalty);
                    }
                }

                bonus += janitor.m_state.m_cartAvailable ? UnityEngine.Random.Range(0f, 0.2f) : 0f;

                actionTime *= UnityEngine.Mathf.Max(0f, UnityEngine.Mathf.Min(1f, (1 - bonus)));

                Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter.Name}, cleaning time {actionTime.ToString(CultureInfo.InvariantCulture)}");

                this.SetParam(ProcedureScriptJanitorCleaning.PARAM_CLEANING_TIME, actionTime);

                this.SwitchState(ProcedureScriptJanitorCleaning.STATE_CLEANING_DIRTY_PLACE);
            }
        }

        public void UpdateStateCleaningDirtyPlace()
        {
            Entity mainCharacter = this.m_stateData.m_procedureScene.MainCharacter;
            EmployeeComponent employeeComponent = mainCharacter.GetComponent<EmployeeComponent>();

            if (this.m_stateData.m_timeInState > this.GetParam(ProcedureScriptJanitorCleaning.PARAM_CLEANING_TIME))
            {
                PlayerStatistics.Instance.IncrementStatistic(Statistics.Vanilla.TilesClean, 1);
                MapScriptInterface.Instance.CleanTile(mainCharacter.GetComponent<WalkComponent>().GetCurrentTile(), mainCharacter.GetComponent<WalkComponent>().GetFloorIndex());

                mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, true);

                employeeComponent.AddSkillPoints(Skills.Vanilla.SKILL_JANITOR_QUALIF_DEXTERITY, Tweakable.Vanilla.JanitorDexteritySkillPoints(), true);

                this.SwitchState(ProcedureScriptJanitorCleaning.STATE_IDLE);
            }
        }

        // states

        public const string STATE_SEARCH_FOR_DIRTY_PLACE = "STATE_SEARCH_FOR_DIRTY_PLACE";
        public const string STATE_GOING_TO_DIRTY_PLACE = "STATE_GOING_TO_DIRTY_PLACE";
        public const string STATE_CLEANING_DIRTY_PLACE = "STATE_CLEANING_DIRTY_PLACE";

        public const string STATE_IDLE = "IDLE";

        // parameters

        public const int PARAM_CLEANING_TIME = 0;
    }
}
