using ModAdvancedGameChanges.Constants;
using System;
using System.Globalization;

namespace ModAdvancedGameChanges 
{
    public static class Tweakable
    {
        public static T GetTweakable<T>(string tweakable) where T : DatabaseEntry
        {
            return Database.Instance.GetEntry<T>(tweakable);
        }

        public static void CheckConfiguration()
        {
            Tweakable.CheckNonLinearSkillLevelingConfiguration();
            Tweakable.CheckEnableExtraLevelingPercent();
            Tweakable.CheckClinicDoctorsMaxLevel();
            Tweakable.CheckEmployeeLevelPoints(5, Tweakables.Mod.AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_FORMAT);              // doctors
            Tweakable.CheckEmployeeLevelPoints(5, Tweakables.Mod.AGC_TWEAKABLE_NURSE_LEVEL_POINTS_FORMAT);               // nurses
            Tweakable.CheckEmployeeLevelPoints(5, Tweakables.Mod.AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_FORMAT);      // lab specialists
            Tweakable.CheckEmployeeLevelPoints(5, Tweakables.Mod.AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_FORMAT);             // janitors
            Tweakable.CheckTrainingHourPoints();
            Tweakable.CheckMainSkillPoints();
            Tweakable.CheckJanitorDexteritySkillPoints();
            Tweakable.CheckCleaningTimeDirt();
            Tweakable.CheckCleaningTimeBlood();
            Tweakable.CheckJanitorManagerCleaningBonusPercent();
            Tweakable.CheckPatientLeaveTimeHours();
            Tweakable.CheckPatientLeaveWarningHours();
            Tweakable.CheckFulfillNeedsThreshold();
            Tweakable.CheckFulfillNeedsCriticalThreshold();
            Tweakable.CheckFulfillNeedsReductionMinimum();
            Tweakable.CheckFulfillNeedsReductionMaximum();
            Tweakable.CheckRestMinutes();
            Tweakable.CheckFreeTimeSkillPoints();
            Tweakable.CheckPatientVendingPaymentMinimum();
            Tweakable.CheckPatientVendingPaymentMaximum();
            Tweakable.CheckEmployeeVendingPaymentMinimum();
            Tweakable.CheckEmployeeVendingPaymentMaximum();
        }

        public static void CheckNonLinearSkillLevelingConfiguration()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_SKILL_LEVELS);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_SKILL_LEVELS}' must be greater than zero.");
            }

            for (int i = 0; i < value.Value; i++)
            {
                var name = String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_SKILL_POINTS_FORMAT, i);
                var skillPoints = Tweakable.EnsureExists<GameDBTweakableInt>(name);

                if (skillPoints.Value < 1)
                {
                    throw new Exception($"The tweakable '{name}' must be greater than zero.");
                }
            }
        }

        public static void CheckEnableExtraLevelingPercent()
        {
            // the AGC_TWEAKABLE_ENABLE_EXTRA_LEVELING_PERCENT value is not relevant, we test it against 1
            Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_ENABLE_EXTRA_LEVELING_PERCENT);
        }

        public static void CheckClinicDoctorsMaxLevel()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL}' must be greater than zero.");
            }
            if (value.Value > 5)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL}' must be lower than six.");
            }
        }

        public static void CheckEmployeeLevelPoints(int levels, string format)
        {
            for (int i = 1; i < levels ; i++)
            {
                var name = String.Format(CultureInfo.InvariantCulture, format, i);
                var levelPoints = Tweakable.EnsureExists<GameDBTweakableInt>(name);
                if (levelPoints.Value < 1)
                {
                    throw new Exception($"The tweakable '{name}' must be greater than zero.");
                }
            }
        }

        public static void CheckTrainingHourPoints()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_TRAINING_HOUR_POINTS);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_TRAINING_HOUR_POINTS}' must be greater than zero.");
            }
        }

        public static void CheckMainSkillPoints()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_MAIN_SKILL_POINTS);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_MAIN_SKILL_POINTS}' must be greater than zero.");
            }
        }

        public static void CheckJanitorDexteritySkillPoints()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_JANITOR_DEXTERITY_SKILL_POINTS);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_JANITOR_DEXTERITY_SKILL_POINTS}' must be greater than zero.");
            }
        }

        public static void CheckCleaningTimeDirt()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_CLEANING_TIME_DIRT);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_CLEANING_TIME_DIRT}' must be greater than zero.");
            }
        }

        public static void CheckCleaningTimeBlood()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_CLEANING_TIME_BLOOD);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_CLEANING_TIME_BLOOD}' must be greater than zero.");
            }
        }

        public static void CheckJanitorManagerCleaningBonusPercent()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_JANITOR_MANAGER_CLEANING_BONUS_PERCENT);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_JANITOR_MANAGER_CLEANING_BONUS_PERCENT}' must be greater than zero.");
            }
            if (value.Value > 100)
            {
                throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_JANITOR_MANAGER_CLEANING_BONUS_PERCENT}' must be less than or equal to 100.");
            }
        }

        public static void CheckPatientLeaveTimeHours()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableFloat>(Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_TIME_HOURS);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_TIME_HOURS}' must be greater than zero.");
            }
        }

        public static void CheckPatientLeaveWarningHours()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableFloat>(Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_WARNING_HOURS);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_WARNING_HOURS}' must be greater than zero.");
            }
        }

        public static void CheckFulfillNeedsThreshold()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD}' must be greater than zero.");
            }
            if (value.Value > 100)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD}' must be less than or equal to 100.");
            }
        }

        public static void CheckFulfillNeedsCriticalThreshold()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_CRITICAL_THRESHOLD);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_CRITICAL_THRESHOLD}' must be greater than zero.");
            }
            if (value.Value > 100)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_CRITICAL_THRESHOLD}' must be less than or equal to 100.");
            }
        }

        public static void CheckFulfillNeedsReductionMinimum()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_RANGE_MINIMUM);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_RANGE_MINIMUM}' must be greater than zero.");
            }
            if (value.Value > 100)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_RANGE_MINIMUM}' must be less than or equal to 100.");
            }
        }

        public static void CheckFulfillNeedsReductionMaximum()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_RANGE_MAXIMUM);
            if (value.Value < Tweakable.Mod.FulfillNeedsReductionMinimum())
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_RANGE_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.FulfillNeedsReductionMinimum().ToString(CultureInfo.InvariantCulture)}.");
            }
            if (value.Value > 100)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_RANGE_MAXIMUM}' must be less than or equal to 100.");
            }
        }

        public static void CheckRestMinutes()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_REST_MINUTES);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_REST_MINUTES}' must be greater than zero.");
            }
        }

        public static void CheckPatientVendingPaymentMinimum()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PATIENT_VENDING_PAYMENT_MINIMUM);
            if (value.Value < 0)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_PATIENT_VENDING_PAYMENT_MINIMUM}' must be greater than or equal to zero.");
            }
        }

        public static void CheckPatientVendingPaymentMaximum()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PATIENT_VENDING_PAYMENT_MAXIMUM);
            if (value.Value < Tweakable.Mod.PatientVendingPaymentMinimum())
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_PATIENT_VENDING_PAYMENT_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.PatientVendingPaymentMinimum().ToString(CultureInfo.InvariantCulture)}.");
            }
        }

        public static void CheckEmployeeVendingPaymentMinimum()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_EMPLOYEE_VENDING_PAYMENT_MINIMUM);
            if (value.Value < 0)
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EMPLOYEE_VENDING_PAYMENT_MINIMUM}' must be greater than or equal to zero.");
            }
        }

        public static void CheckEmployeeVendingPaymentMaximum()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_EMPLOYEE_VENDING_PAYMENT_MAXIMUM);
            if (value.Value < Tweakable.Mod.EmployeeVendingPaymentMinimum())
            {
                throw new Exception($"The tweakable '{Tweakables.Mod.AGC_TWEAKABLE_EMPLOYEE_VENDING_PAYMENT_MAXIMUM}' must be greater than or equal to {Tweakable.Mod.EmployeeVendingPaymentMinimum().ToString(CultureInfo.InvariantCulture)}.");
            }
        }

        public static void CheckFreeTimeSkillPoints()
        {
            var value = Tweakable.EnsureExists<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_REPEAT_ACTION_FREE_TIME_SKILL_POINTS);
            if (value.Value < 1)
            {
                throw new Exception($"The tweakable '{Tweakables.Vanilla.TWEAKABLE_REPEAT_ACTION_FREE_TIME_SKILL_POINTS}' must be greater than zero.");
            }
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

        public static class Mod
        {
            public static int DoctorLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int NurseLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_NURSE_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int LabSpecialistLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int JanitorLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int SkillLevels()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_SKILL_LEVELS).Value;
            }

            public static int SkillPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Tweakables.Mod.AGC_TWEAKABLE_SKILL_POINTS_FORMAT, index)).Value;
            }

            public static bool EnableExtraLevelingPercent()
            {
                return (Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_ENABLE_EXTRA_LEVELING_PERCENT).Value == 1);
            }

            public static int AllowedClinicDoctorsLevel()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL).Value;
            }

            public static int TrainingHourPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_TRAINING_HOUR_POINTS).Value;
            }

            public static int CleaningTimeDirt()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_CLEANING_TIME_DIRT).Value;
            }

            public static int CleaningTimeBlood()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_CLEANING_TIME_BLOOD).Value;
            }

            public static float FulfillNeedsThreshold()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_THRESHOLD).Value;
            }

            public static float FulfillNeedsCriticalThreshold()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_CRITICAL_THRESHOLD).Value;
            }

            public static float FulfillNeedsReductionMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_RANGE_MINIMUM).Value;
            }

            public static float FulfillNeedsReductionMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_FULFILL_NEEDS_REDUCTION_RANGE_MAXIMUM).Value;
            }

            public static float RestMinutes()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Mod.AGC_TWEAKABLE_REST_MINUTES).Value;
            }

            public static int PatientVendingPaymentMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PATIENT_VENDING_PAYMENT_MINIMUM).Value;
            }

            public static int PatientVendingPaymentMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_PATIENT_VENDING_PAYMENT_MAXIMUM).Value;
            }

            public static int EmployeeVendingPaymentMinimum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_EMPLOYEE_VENDING_PAYMENT_MINIMUM).Value;
            }

            public static int EmployeeVendingPaymentMaximum()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Mod.AGC_TWEAKABLE_EMPLOYEE_VENDING_PAYMENT_MAXIMUM).Value;
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

            public static int MainSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_MAIN_SKILL_POINTS).Value;
            }

            public static int JanitorDexteritySkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_JANITOR_DEXTERITY_SKILL_POINTS).Value;
            }

            public static int JanitorManagerCleaningBonusPercent()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_JANITOR_MANAGER_CLEANING_BONUS_PERCENT).Value;
            }

            public static float PatientLeaveTimeHours()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_TIME_HOURS).Value;
            }

            public static float PatientLeaveWarningHours()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Tweakables.Vanilla.TWEAKABLE_PATIENT_LEAVE_WARNING_HOURS).Value;
            }

            public static int FreeTimeSkillPoints()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Tweakables.Vanilla.TWEAKABLE_REPEAT_ACTION_FREE_TIME_SKILL_POINTS).Value;
            }
        }
    }
}