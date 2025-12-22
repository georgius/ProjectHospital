using ModAdvancedGameChanges.Constants;
using System;
using System.Globalization;

namespace ModAdvancedGameChanges 
{
    public static class Tweakable
    {
        public static class Mod
        {
            public static int AmbiguousLeaveMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_AMBIGUOUS_LEAVE_MINUTES).Value;
            }

            public static int AllowedClinicDoctorsLevel()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL).Value;
            }

            public static int ComplicatedDecisionMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_COMPLICATED_DECISION_MINUTES).Value;
            }

            public static int DoctorLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int DoctorFillingReportMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_DOCTOR_FILLING_REPORT_MINUTES).Value;
            }
            public static int DoctorFillingReportSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_DOCTOR_FILLING_REPORT_SKILL_POINTS).Value;
            }

            public static float EfficiencyMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_MINIMUM).Value;
            }

            public static float EfficiencyGoodBossMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MINIMUM).Value;
            }

            public static float EfficiencyGoodBossMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MAXIMUM).Value;
            }

            public static float EfficiencySatisfactionMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SATISFACTION_MINIMUM).Value;
            }

            public static float EfficiencySatisfactionMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SATISFACTION_MAXIMUM).Value;
            }

            public static float EfficiencyShiftPreferenceMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SHIFT_PREFERENCE_MINIMUM).Value;
            }

            public static float EfficiencyShiftPreferenceMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_SHIFT_PREFERENCE_MAXIMUM).Value;
            }

            public static float EfficiencyNeedBladderPenalty()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_NEED_BLADDER_PENALTY).Value;
            }

            public static float EfficiencyNeedHungerPenalty()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_NEED_HUNGER_PENALTY).Value;
            }

            public static float EfficiencyNeedRestPenalty()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_EFFICIENCY_NEED_REST_PENALTY).Value;
            }

            public static bool EnableExtraLevelingPercent()
            {
                return (Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_ENABLE_EXTRA_LEVELING_PERCENT).Value == 1);
            }

            public static float FulfillNeedsThreshold()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD).Value;
            }

            public static float FulfillNeedsThresholdCritical()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD_CRITICAL).Value;
            }

            public static float FulfillNeedsReductionMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_MINIMUM).Value;
            }

            public static float FulfillNeedsReductionMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_MAXIMUM).Value;
            }

            public static int JanitorCleaningTimeBloodMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_JANITOR_CLEANING_TIME_BLOOD_MINUTES).Value;
            }

            public static int JanitorCleaningTimeDirtMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_JANITOR_CLEANING_TIME_DIRT_MINUTES).Value;
            }

            public static int JanitorLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int LabSpecialistLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static float NeedBladder()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_BLADDER).Value;
            }

            public static float NeedBoredomUsingObject()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_BOREDOM_USING_OBJECT).Value;
            }

            public static float NeedBoredomUsingPhone()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_BOREDOM_USING_PHONE).Value;
            }

            public static float NeedBoredomYawning()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_BOREDOM_YAWNING).Value;
            }

            public static float NeedHunger()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER).Value;
            }

            public static float NeedHungerLunch()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH).Value;
            }

            public static float NeedHungerLunchEatingTimeMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_EATING_TIME_MINUTES).Value;
            }

            public static float NeedHungerLunchCoffeeTimeMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_COFFEE_TIME_MINUTES).Value;
            }

            public static int NeedHungerLunchPaymentMealMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_MEAL_MINIMUM).Value;
            }
            public static int NeedHungerLunchPaymentMealMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_MEAL_MAXIMUM).Value;
            }
            public static int NeedHungerLunchPaymentSnackMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_SNACK_MINIMUM).Value;
            }
            public static int NeedHungerLunchPaymentSnackMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_SNACK_MAXIMUM).Value;
            }
            public static int NeedHungerLunchPaymentJuiceMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_JUICE_MINIMUM).Value;
            }
            public static int NeedHungerLunchPaymentJuiceMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_JUICE_MAXIMUM).Value;
            }
            public static int NeedHungerLunchPaymentCoffeeMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_COFFEE_MINIMUM).Value;
            }
            public static int NeedHungerLunchPaymentCoffeeMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NEED_HUNGER_LUNCH_PAYMENT_COFFEE_MAXIMUM).Value;
            }

            public static float NeedRestMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_MINUTES).Value;
            }

            public static float NeedRestPlayGame()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_PLAY_GAME).Value;
            }

            public static float NeedRestSit()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_SIT).Value;
            }

            public static float NeedRestStudy()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_NEED_REST_STUDY).Value;
            }

            public static int NurseLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_NURSE_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int NurseReceptionQuestionMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_QUESTION_MINUTES).Value;
            }

            public static int NurseReceptionDecisionMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_DECISION_MINUTES).Value;
            }

            public static int NurseReceptionQuestionSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_QUESTION_SKILL_POINTS).Value;
            }

            public static int NurseReceptionNextQuestionSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_NURSE_RECEPTION_NEXT_QUESTION_SKILL_POINTS).Value;
            }

            public static float PatientMaxWaitTimeHoursHighPriorityMultiplier()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS_HIGH_PRIORITY_MULTIPLIER).Value;
            }

            public static float PatientMaxWaitTimeHoursMediumPriorityMultiplier()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS_MEDIUM_PRIORITY_MULTIPLIER).Value;
            }

            public static float PatientMaxWaitTimeHoursLowPriorityMultiplier()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS_LOW_PRIORITY_MULTIPLIER).Value;
            }

            public static int PedestrianCount()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PEDESTRIAN_COUNT).Value;
            }

            public static float PedestrianPharmacyProbabilityDayPercent()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_PROBABILITY_DAY_PERCENT).Value;
            }

            public static float PedestrianPharmacyProbabilityDayNightPercent()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_PROBABILITY_NIGHT_PERCENT).Value;
            }

            public static int PharmacyCustomerMaximumWaitingTimeMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PHARMACY_CUSTOMER_MAXIMUM_WAITING_TIME_MINUTES).Value;
            }

            public static int PharmacyPharmacistQuestionMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PHARMACY_PHARMACIST_QUESTION_MINUTES).Value;
            }

            public static int PharmacyPharmacistSearchDrugMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PHARMACY_PHARMACIST_SEARCH_DRUG_MINUTES).Value;
            }

            public static int PharmacyPharmacistSearchDrugSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PHARMACY_PHARMACIST_SEARCH_DRUG_SKILL_POINTS).Value;
            }

            public static int PharmacyNonRestrictedDrugsPaymentMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PHARMACY_NON_RESTRICTED_DRUGS_PAYMENT_MINIMUM).Value;
            }
            public static int PharmacyNonRestrictedDrugsPaymentMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PHARMACY_NON_RESTRICTED_DRUGS_PAYMENT_MAXIMUM).Value;
            }

            public static int SkillLevels()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_SKILL_LEVELS).Value;
            }

            public static int SkillPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_SKILL_POINTS_FORMAT, index)).Value;
            }

            public static float SkillTimeReductionMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_SKILL_TIME_REDUCTION_MINIMUM).Value;
            }

            public static float SkillTimeReductionMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_SKILL_TIME_REDUCTION_MAXIMUM).Value;
            }

            public static int TrainingHourSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_TRAINING_HOUR_SKILL_POINTS).Value;
            }

            public static int VendingPaymentEmployeeMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_EMPLOYEE_MINIMUM).Value;
            }

            public static int VendingPaymentEmployeeMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_EMPLOYEE_MAXIMUM).Value;
            }

            public static int VendingPaymentPatientMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_PATIENT_MINIMUM).Value;
            }

            public static int VendingPaymentPatientMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_VENDING_PAYMENT_PATIENT_MAXIMUM).Value;
            }
        }

        public static class Vanilla
        {
            public static float LevelingRatePercent()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Vanilla.LEVELING_RATE_PERCENT).Value;
            }

            public static bool DlcHospitalServicesEnabled()
            {
                GameDBTweakableInt dlcHospitalServices = Database.Instance.GetEntry<GameDBTweakableInt>(Tweakables.Vanilla.DLC_ADMIN_PATHOLOGY_ENABLED);

                return ((dlcHospitalServices != null) && (dlcHospitalServices.Value == 1));
            }

            public static int CorrectDiagnosePerkSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_CORRECT_DIAGNOSE_PERK_SKILL_POINTS).Value;
            }

            public static int CorrectDiagnoseSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_CORRECT_DIAGNOSE_SKILL_POINTS).Value;
            }

            public static int IncorrectDiagnosePerkSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_INCORRECT_DIAGNOSE_PERK_SKILL_POINTS).Value;
            }

            public static int IncorrectDiagnoseSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_INCORRECT_DIAGNOSE_SKILL_POINTS).Value;
            }

            public static int JanitorDexteritySkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_JANITOR_DEXTERITY_SKILL_POINTS).Value;
            }

            public static int JanitorManagerCleaningBonusPercent()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_JANITOR_MANAGER_CLEANING_BONUS_PERCENT).Value;
            }

            public static int MainSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_MAIN_SKILL_POINTS).Value;
            }

            public static float PatientLeaveTimeHours()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_TIME_HOURS).Value;
            }

            public static float PatientLeaveWarningHours()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_WARNING_HOURS).Value;
            }

            public static float PatientMaxWaitTimeHours()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Vanilla.TWEAKABLE_PATIENT_MAX_WAIT_TIME_HOURS).Value;
            }

            public static int RepeatActionFreeTimeSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_REPEAT_ACTION_FREE_TIME_SKILL_POINTS).Value;
            }
        }

        public static T GetTweakable<T>(string tweakable) where T : DatabaseEntry
        {
            return Database.Instance.GetEntry<T>(tweakable);
        }

        public static void CheckConfiguration()
        {
            foreach (var tweakableValidation in TweakableValidation.Validations)
            {
                tweakableValidation.Validate();
            }
        }
    }
}