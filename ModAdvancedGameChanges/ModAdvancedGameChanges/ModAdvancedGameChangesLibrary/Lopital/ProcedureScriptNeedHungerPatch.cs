using GLib;
using HarmonyLib;
using Lopital;
using ModAdvancedGameChanges.Constants;
using UnityEngine;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(ProcedureScriptNeedHunger))]
    public static class ProcedureScriptNeedHungerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptNeedHunger), nameof(ProcedureScriptNeedHunger.Activate))]
        public static bool ActivatePreifx(ProcedureScriptNeedHunger __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            Entity mainCharacter = __instance.m_stateData.m_procedureScene.MainCharacter;
            Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, activating script {__instance.m_stateData.m_scriptName}");

            mainCharacter.GetComponent<WalkComponent>().SetDestination(__instance.GetEquipment(0).GetDefaultUsePosition(), __instance.GetEquipment(0).GetFloorIndex(), MovementType.WALKING);

            __instance.SwitchState(ProcedureScriptNeedHungerPatch.STATE_GOING_TO_PLACE);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProcedureScriptNeedHunger), nameof(ProcedureScriptNeedHunger.ScriptUpdate))]
        public static bool ScriptUpdatePrefix(float deltaTime, ProcedureScriptNeedHunger __instance)
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            __instance.m_stateData.m_timeInState += deltaTime;
            switch (__instance.m_stateData.m_state)
            {
                case ProcedureScriptNeedHungerPatch.STATE_GOING_TO_PLACE:
                    ProcedureScriptNeedHungerPatch.UpdateStateGoingToPlace(__instance);
                    break;
                case ProcedureScriptNeedHungerPatch.STATE_GOING_TO_OBJECT:
                    ProcedureScriptNeedHungerPatch.UpdateStateGoingToObject(__instance);
                    break;
                case ProcedureScriptNeedHungerPatch.STATE_USING_OBJECT:
                    ProcedureScriptNeedHungerPatch.UpdateStateUsingObject(__instance);
                    break;
                default:
                    break;
            }

            return false;
        }

        public static void UpdateStateGoingToPlace(ProcedureScriptNeedHunger instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (mainCharacter.GetComponent<WalkComponent>().m_state.m_walkState == WalkState.NoPath)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, no path");

                    instance.SwitchState(ProcedureScriptNeedHunger.STATE_IDLE);
                }
            }

            if (instance.GetEquipment(0).GetFloorIndex() == mainCharacter.GetComponent<WalkComponent>().GetFloorIndex())
            {
                // character is on same floor
                // we can check distance

                Vector2i objectPosition = instance.GetEquipment(0).GetDefaultUseTile();
                Vector2i characterPosition = mainCharacter.GetComponent<WalkComponent>().GetCurrentTile();

                if ((objectPosition - characterPosition).LengthSquared() <= 9)
                {
                    // we are closing to destination
                    // check if we can reserve object

                    if (instance.GetEquipment(0).User == null)
                    {
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, object is not reserved");

                        // object is free, reserve object and continue
                        mainCharacter.GetComponent<UseComponent>().ReserveObject(instance.GetEquipment(0));

                        instance.SpeakCharacter(mainCharacter, null, Speeches.Vanilla.Food, 4f);

                        instance.SwitchState(ProcedureScriptNeedHungerPatch.STATE_GOING_TO_OBJECT);
                    }
                    else
                    {
                        // object is used
                        Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, object is reserved");

                        mainCharacter.GetComponent<WalkComponent>().Stop();
                        mainCharacter.GetComponent<AnimModelComponent>().PlayAnimation(Animations.Vanilla.StandIdle, false);

                        instance.SwitchState(ProcedureScriptNeedHunger.STATE_IDLE);
                    }
                }
            }
        }

        public static void UpdateStateGoingToObject(ProcedureScriptNeedHunger instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<WalkComponent>().IsBusy())
            {
                if (mainCharacter.GetComponent<WalkComponent>().m_state.m_walkState == WalkState.NoPath)
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, no path");

                    instance.SwitchState(ProcedureScriptNeedHunger.STATE_IDLE);
                }
                else
                {
                    Debug.LogDebug(System.Reflection.MethodBase.GetCurrentMethod(), $"{mainCharacter?.Name ?? "NULL"}, using object");

                    instance.SwitchState(ProcedureScriptNeedHungerPatch.STATE_USING_OBJECT);
                    mainCharacter.GetComponent<UseComponent>().Activate(UseComponentMode.SINGLE_USE);
                }
            }
        }

        public static void UpdateStateUsingObject(ProcedureScriptNeedHunger instance)
        {
            Entity mainCharacter = instance.m_stateData.m_procedureScene.MainCharacter;

            if (!mainCharacter.GetComponent<UseComponent>().IsBusy())
            {
                // payment
                if (instance.GetEquipment(0).HasTag(Tags.Vanilla.Vending))
                {
                    BehaviorPatient patient = mainCharacter.GetComponent<BehaviorPatient>();
                    EmployeeComponent employee = mainCharacter.GetComponent<EmployeeComponent>();

                    int payment = (patient != null)
                        ? UnityEngine.Random.Range(Tweakable.Mod.PatientVendingPaymentMinimum(), Tweakable.Mod.PatientVendingPaymentMaximum())
                        : UnityEngine.Random.Range(Tweakable.Mod.EmployeeVendingPaymentMinimum(), Tweakable.Mod.EmployeeVendingPaymentMaximum());

                    if (payment > 0)
                    {
                        if ((patient != null) && patient.ShouldPatientPay())
                        {
                            patient.m_state.m_moneySpent += payment;
                            patient.m_state.m_department.GetEntity().Pay(payment, PaymentCategory.FOOD, mainCharacter);
                        }

                        if (employee != null)
                        {
                            employee.m_state.m_department.GetEntity().Pay(payment, PaymentCategory.FOOD, mainCharacter);
                        }

                        if (SettingsManager.Instance.m_gameSettings.m_showPaymentsInGame.m_value)
                        {
                            NotificationManager.GetInstance().AddFloatingIngameNotification(mainCharacter, "$" + payment, new Color(0.5f, 1f, 0.5f));
                        }
                    }
                }

                // free reserved object
                if (instance.GetEquipment(0).User == instance)
                {
                    mainCharacter.GetComponent<UseComponent>().Deactivate();
                    instance.GetEquipment(0).User = null;
                }

                if (mainCharacter.GetComponent<PerkComponent>().m_perkSet.HasHiddenPerk(Perks.Vanilla.Spartan))
                {
                    mainCharacter.GetComponent<PerkComponent>().m_perkSet.RevealPerk(Perks.Vanilla.Spartan);
                }

                mainCharacter.GetComponent<Behavior>().ReceiveMessage(new Message(Messages.HUNGER_REDUCED, 1f));

                instance.SwitchState(ProcedureScriptNeedHunger.STATE_IDLE);
            }
        }

        public const string STATE_GOING_TO_PLACE = "STATE_GOING_TO_PLACE";
        public const string STATE_GOING_TO_OBJECT = "STATE_GOING_TO_OBJECT";
        public const string STATE_USING_OBJECT = "STATE_USING_WC";
    }
}
