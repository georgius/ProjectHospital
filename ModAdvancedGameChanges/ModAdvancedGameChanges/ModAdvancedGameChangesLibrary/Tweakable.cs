using ModAdvancedGameChanges;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ModGameChanges
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
            Tweakable.CheckEmployeeLevelPoints(5, Constants.Tweakables.Mod.AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_FORMAT);              // doctors
            Tweakable.CheckEmployeeLevelPoints(3, Constants.Tweakables.Mod.AGC_TWEAKABLE_NURSE_LEVEL_POINTS_FORMAT);               // nurses
            Tweakable.CheckEmployeeLevelPoints(3, Constants.Tweakables.Mod.AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_FORMAT);      // lab specialists
            Tweakable.CheckEmployeeLevelPoints(3, Constants.Tweakables.Mod.AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_FORMAT);             // janitors
        }

        public static void CheckNonLinearSkillLevelingConfiguration()
        {
            var skillLevels = Tweakable.EnsureExists<GameDBTweakableInt>(Constants.Tweakables.Mod.AGC_TWEAKABLE_SKILL_LEVELS);
            if (skillLevels.Value < 1)
            {
                throw new Exception($"The tweakable '{Constants.Tweakables.Mod.AGC_TWEAKABLE_SKILL_LEVELS}' must be greater than zero.");
            }

            for (int i = 0; i < skillLevels.Value; i++)
            {
                var name = String.Format(CultureInfo.InvariantCulture, Constants.Tweakables.Mod.AGC_TWEAKABLE_SKILL_POINTS_FORMAT, i);
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
            Tweakable.EnsureExists<GameDBTweakableInt>(Constants.Tweakables.Mod.AGC_TWEAKABLE_ENABLE_EXTRA_LEVELING_PERCENT);
        }

        public static void CheckClinicDoctorsMaxLevel()
        {
            var allowedLevel = Tweakable.EnsureExists<GameDBTweakableInt>(Constants.Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL);
            if (allowedLevel.Value < 1)
            {
                throw new Exception($"The tweakable '{Constants.Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL}' must be greater than zero.");
            }
            if (allowedLevel.Value > 5)
            {
                throw new Exception($"The tweakable '{Constants.Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL}' must be lower than six.");
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
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Constants.Tweakables.Mod.AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int NurseLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Constants.Tweakables.Mod.AGC_TWEAKABLE_NURSE_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int LabSpecialistLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Constants.Tweakables.Mod.AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int JanitorLevelPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Constants.Tweakables.Mod.AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_FORMAT, index)).Value;
            }

            public static int SkillLevels()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Constants.Tweakables.Mod.AGC_TWEAKABLE_SKILL_LEVELS).Value;
            }

            public static int SkillPoints(int index)
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(String.Format(CultureInfo.InvariantCulture, Constants.Tweakables.Mod.AGC_TWEAKABLE_SKILL_POINTS_FORMAT, index)).Value;
            }

            public static bool EnableExtraLevelingPercent()
            {
                return (Tweakable.GetTweakable<GameDBTweakableInt>(Constants.Tweakables.Mod.AGC_TWEAKABLE_ENABLE_EXTRA_LEVELING_PERCENT).Value == 1);
            }

            public static int AllowedClinicDoctorsLevel()
            {
                return Tweakable.GetTweakable<GameDBTweakableInt>(Constants.Tweakables.Mod.AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL).Value;
            }
        }

        public static class Vanilla
        {
            public static float LevelingRatePercent()
            {
                return Tweakable.GetTweakable<GameDBTweakableFloat>(Constants.Tweakables.Vanilla.LevelingRatePercent).Value;
            }

            public static bool DlcHospitalServicesEnabled()
            {
                GameDBTweakableInt dlcHospitalServices = Database.Instance.GetEntry<GameDBTweakableInt>(Constants.Tweakables.Vanilla.DLC_ADMIN_PATHOLOGY_ENABLED);

                return ((dlcHospitalServices != null) && (dlcHospitalServices.Value == 1));
            }
        }
    }
}