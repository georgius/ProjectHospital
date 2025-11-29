using ModAdvancedGameChanges.Constants;
using System;
using System.Globalization;

namespace ModAdvancedGameChanges
{
    public sealed class TweakableValidation
    {
        public Action Validate;

        public TweakableValidation(Action validate)
        {
            this.Validate = validate;
        }

        public static T EnsureExists<T>(string tweakable) where T : DatabaseEntry
        {
            var entry = Tweakable.GetTweakable<T>(tweakable);
            if (entry == null)
            {
                throw new Exception($"The tweakable '{tweakable}' does not exist or is not of '{typeof(T).Name}' type or has incorrent value for '{typeof(T).Name}' type.");
            }

            return entry;
        }

        public static TweakableValidation[] Validations =
        {
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL}' must be greater than zero.");
                }
                if (value.Value > 5)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL}' must be lower than six.");
                }
            }),

            new TweakableValidation(() =>
            {
                // doctors
                TweakableValidation.CheckEmployeeLevelPoints(5, Tweakables.Mod.AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_FORMAT);              
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_MINIMUM);
                if (value.Value < 1f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_MINIMUM}' must be greater than or equal to 1.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_MINIMUM}' must be less than or equal to 100.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MINIMUM);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MINIMUM}' must be greater than or equal to zero.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MINIMUM}' must be less than or equal to 100.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MAXIMUM);
                if (value.Value < Tweakable.Mod.EfficiencyGoodBossMinimum())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.EfficiencyGoodBossMinimum().ToString(CultureInfo.InvariantCulture)}.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MAXIMUM}' must be less than or equal to 100.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SATISFACTION_MINIMUM);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SATISFACTION_MINIMUM}' must be greater than or equal to zero.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SATISFACTION_MINIMUM}' must be less than or equal to 100.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SATISFACTION_MAXIMUM);
                if (value.Value < Tweakable.Mod.EfficiencySatisfactionMinimum())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SATISFACTION_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.EfficiencySatisfactionMinimum().ToString(CultureInfo.InvariantCulture)}.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SATISFACTION_MAXIMUM}' must be less than or equal to 100.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SHIFT_PREFERENCE_MINIMUM);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SHIFT_PREFERENCE_MINIMUM}' must be greater than or equal to zero.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SHIFT_PREFERENCE_MINIMUM}' must be less than or equal to 100.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SHIFT_PREFERENCE_MAXIMUM);
                if (value.Value < Tweakable.Mod.EfficiencyShiftPreferenceMinimum())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SHIFT_PREFERENCE_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.EfficiencyShiftPreferenceMinimum().ToString(CultureInfo.InvariantCulture)}.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SHIFT_PREFERENCE_MAXIMUM}' must be less than or equal to 100.");
                }
            }),

            new TweakableValidation(() =>
            {
                // the AGC_TWEAKABLE_ENABLE_EXTRA_LEVELING_PERCENT value is not relevant, we test it against 1
                TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_ENABLE_EXTRA_LEVELING_PERCENT);
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD}' must be greater than or equal to zero.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD}' must be less than or equal to 100.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD_CRITICAL);
                if (value.Value < Tweakable.Mod.FulfillNeedsThreshold())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD_CRITICAL}' must be greater than or equal to {Tweakable.Mod.FulfillNeedsThreshold().ToString(CultureInfo.InvariantCulture)}.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD_CRITICAL}' must be less than or equal to 100.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_MINIMUM);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_MINIMUM}' must be greater than or equal to zero.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_MINIMUM}' must be less than or equal to 100.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_MAXIMUM);
                if (value.Value < Tweakable.Mod.FulfillNeedsReductionMinimum())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.FulfillNeedsReductionMinimum().ToString(CultureInfo.InvariantCulture)}.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_MAXIMUM}' must be less than or equal to 100.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_BLADDER);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_BLADDER}' must be greater than or equal to zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_BOREDOM_USING_OBJECT);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_BOREDOM_USING_OBJECT}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_BOREDOM_USING_PHONE);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_BOREDOM_USING_PHONE}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_BOREDOM_YAWNING);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_BOREDOM_YAWNING}' must be greater than or equal to zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_EATING_TIME_MINUTES);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_EATING_TIME_MINUTES}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_COFFEE_TIME_MINUTES);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_COFFEE_TIME_MINUTES}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_MEAL_MINIMUM);
                if (value.Value < 0)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_MEAL_MINIMUM}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_MEAL_MAXIMUM);
                if (value.Value < Tweakable.Mod.NeedHungerLunchPaymentMealMinimum())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_MEAL_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.NeedHungerLunchPaymentMealMinimum().ToString(CultureInfo.InvariantCulture)}.");
                }
            }),            
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_SNACK_MINIMUM);
                if (value.Value < 0)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_SNACK_MINIMUM}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_SNACK_MAXIMUM);
                if (value.Value < Tweakable.Mod.NeedHungerLunchPaymentSnackMinimum())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_SNACK_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.NeedHungerLunchPaymentSnackMinimum().ToString(CultureInfo.InvariantCulture)}.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_JUICE_MINIMUM);
                if (value.Value < 0)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_JUICE_MINIMUM}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_JUICE_MAXIMUM);
                if (value.Value < Tweakable.Mod.NeedHungerLunchPaymentJuiceMinimum())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_JUICE_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.NeedHungerLunchPaymentJuiceMinimum().ToString(CultureInfo.InvariantCulture)}.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_COFFEE_MINIMUM);
                if (value.Value < 0)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_COFFEE_MINIMUM}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_COFFEE_MAXIMUM);
                if (value.Value < Tweakable.Mod.NeedHungerLunchPaymentCoffeeMinimum())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_COFFEE_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.NeedHungerLunchPaymentCoffeeMinimum().ToString(CultureInfo.InvariantCulture)}.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_MINUTES);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_MINUTES}' must be greater than zero or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_PLAY_GAME);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_PLAY_GAME}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_SIT);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_SIT}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_STUDY);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_STUDY}' must be greater than or equal to zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                // nurses
                TweakableValidation.CheckEmployeeLevelPoints(5, Tweakables.Mod.AGC_TWEAKABLE_NURSE_LEVEL_POINTS_FORMAT);
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_QUESTION_MINUTES);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_QUESTION_MINUTES}' must be greater than zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_DECISION_MINUTES);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_DECISION_MINUTES}' must be greater than zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_QUESTION_SKILL_POINTS);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_QUESTION_SKILL_POINTS}' must be greater than zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_NEXT_QUESTION_SKILL_POINTS);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_NEXT_QUESTION_SKILL_POINTS}' must be greater than zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                // lab specialists
                TweakableValidation.CheckEmployeeLevelPoints(5, Tweakables.Mod.AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_FORMAT);
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_JANITOR_CLEANING_TIME_BLOOD);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_JANITOR_CLEANING_TIME_BLOOD}' must be greater than zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_JANITOR_CLEANING_TIME_DIRT);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_JANITOR_CLEANING_TIME_DIRT}' must be greater than zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                // janitors
                TweakableValidation.CheckEmployeeLevelPoints(5, Tweakables.Mod.AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_FORMAT);
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_SKILL_LEVELS);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_SKILL_LEVELS}' must be greater than zero.");
                }

                for (int i = 0; i < value.Value; i++)
                {
                    var name = String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_SKILL_POINTS_FORMAT, i);
                    var skillPoints = TweakableValidation.EnsureExists<GameDBTweakableInt>(name);

                    if (skillPoints.Value < 1)
                    {
                        throw new Exception($"The tweakable '{name}' must be greater than zero.");
                    }
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_SKILL_TIME_REDUCTION_MINIMUM);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_SKILL_TIME_REDUCTION_MINIMUM}' must be greater than or equal to zero.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_SKILL_TIME_REDUCTION_MINIMUM}' must be less than or equal to 100.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_SKILL_TIME_REDUCTION_MAXIMUM);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_SKILL_TIME_REDUCTION_MAXIMUM}' must be greater than or equal to zero.");
                }
                if (value.Value > 100f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_SKILL_TIME_REDUCTION_MAXIMUM}' must be less than or equal to 100.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_TRAINING_HOUR_SKILL_POINTS);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_TRAINING_HOUR_SKILL_POINTS}' must be greater than zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_EMPLOYEE_MINIMUM);
                if (value.Value < 0)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_EMPLOYEE_MINIMUM}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_EMPLOYEE_MAXIMUM);
                if (value.Value < Tweakable.Mod.VendingPaymentEmployeeMinimum())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_EMPLOYEE_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.VendingPaymentEmployeeMinimum().ToString(CultureInfo.InvariantCulture)}.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_PATIENT_MINIMUM);
                if (value.Value < 0)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_PATIENT_MINIMUM}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_PATIENT_MAXIMUM);
                if (value.Value < Tweakable.Mod.VendingPaymentPatientMinimum())
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_PATIENT_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.VendingPaymentPatientMinimum().ToString(CultureInfo.InvariantCulture)}.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS_HIGH_PRIORITY_MULTIPLIER);
                if (value.Value <= 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS_HIGH_PRIORITY_MULTIPLIER}' must be greater than zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS_MEDIUM_PRIORITY_MULTIPLIER);
                if (value.Value <= 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS_MEDIUM_PRIORITY_MULTIPLIER}' must be greater than zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS_LOW_PRIORITY_MULTIPLIER);
                if (value.Value <= 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS_LOW_PRIORITY_MULTIPLIER}' must be greater than zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_AMBIGUOUS_LEAVE_MINUTES);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_AMBIGUOUS_LEAVE_MINUTES}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_COMPLICATED_DECISION_MINUTES);
                if (value.Value < 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_COMPLICATED_DECISION_MINUTES}' must be greater than or equal to zero.");
                }
            }),

            // vanilla checks

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_MAIN_SKILL_POINTS);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_MAIN_SKILL_POINTS}' must be greater than zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_CORRECT_DIAGNOSE_PERK_SKILL_POINTS);
                if (value.Value < 0)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_CORRECT_DIAGNOSE_PERK_SKILL_POINTS}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_CORRECT_DIAGNOSE_SKILL_POINTS);
                if (value.Value < 0)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_CORRECT_DIAGNOSE_SKILL_POINTS}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_INCORRECT_DIAGNOSE_PERK_SKILL_POINTS);
                if (value.Value < 0)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_INCORRECT_DIAGNOSE_PERK_SKILL_POINTS}' must be greater than or equal to zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_INCORRECT_DIAGNOSE_SKILL_POINTS);
                if (value.Value < 0)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_INCORRECT_DIAGNOSE_SKILL_POINTS}' must be greater than or equal to zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_JANITOR_MANAGER_CLEANING_BONUS_PERCENT);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_JANITOR_MANAGER_CLEANING_BONUS_PERCENT}' must be greater than zero.");
                }
                if (value.Value > 100)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_JANITOR_MANAGER_CLEANING_BONUS_PERCENT}' must be less than or equal to 100.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_JANITOR_DEXTERITY_SKILL_POINTS);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_JANITOR_DEXTERITY_SKILL_POINTS}' must be greater than zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_TIME_HOURS);
                if (value.Value <= 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_TIME_HOURS}' must be greater than zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_WARNING_HOURS);
                if (value.Value <= 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_WARNING_HOURS}' must be greater than zero.");
                }
            }),
            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableFloat>(Tweakables.Vanilla.TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS);
                if (value.Value <= 0f)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS}' must be greater than zero.");
                }
            }),

            new TweakableValidation(() =>
            {
                var value = TweakableValidation.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_REPEAT_ACTION_FREE_TIME_SKILL_POINTS);
                if (value.Value < 1)
                {
                    throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_REPEAT_ACTION_FREE_TIME_SKILL_POINTS}' must be greater than zero.");
                }
            })
        };

        private static void CheckEmployeeLevelPoints(int levels, string format)
        {
            for (int i = 1; i < levels; i++)
            {
                var name = String.Format(CultureInfo.InvariantCulture, format, i);
                var levelPoints = TweakableValidation.EnsureExists<GameDBTweakableInt>(name);
                if (levelPoints.Value < 1)
                {
                    throw new Exception($"The tweakable '{name}' must be greater than zero.");
                }
            }
        }
    }
}