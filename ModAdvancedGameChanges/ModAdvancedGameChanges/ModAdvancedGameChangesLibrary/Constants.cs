namespace ModAdvancedGameChanges
{
    public static class Constants
    {
        public static class Departments
        {
            public static class Mod
            {
                public const string TrainingDepartment = "AGC_DPT_TRAINING";
            }
        }

        public static class Perks
        {
            public static class Vanilla
            {
                public const string FastLearner = "PERK_FAST_LEARNER";
                public const string SlowLearner = "PERK_SLOW_LEARNER";
            }
        }

        public static class Skills
        {
            public static class Vanilla
            {
                public const string SKILL_DOC_QUALIF_GENERAL_MEDICINE = "SKILL_DOC_QUALIF_GENERAL_MEDICINE";
                public const string SKILL_DOC_QUALIF_DIAGNOSIS = "SKILL_DOC_QUALIF_DIAGNOSIS";

                public const string SKILL_NURSE_QUALIF_PATIENT_CARE = "SKILL_NURSE_QUALIF_PATIENT_CARE";

                public const string SKILL_LAB_SPECIALIST_QUALIF_SCIENCE_EDUCATION = "SKILL_LAB_SPECIALIST_QUALIF_SCIENCE_EDUCATION";

                public const string SKILL_JANITOR_QUALIF_EFFICIENCY = "SKILL_JANITOR_QUALIF_EFFICIENCY";
                public const string SKILL_JANITOR_QUALIF_DEXTERITY = "SKILL_JANITOR_QUALIF_DEXTERITY";
            }
        }

        public static class Tweakables
        {
            public static class Mod
            {
                public const string HospitalServicesEnabled = "DLC_ADMIN_PATHOLOGY_ENABLED";

                public const string AGC_TWEAKABLE_SKILL_LEVELS = "AGC_TWEAKABLE_SKILL_LEVELS";

                public const string AGC_TWEAKABLE_SKILL_POINTS_FORMAT = "AGC_TWEAKABLE_SKILL_POINTS_{0}";


                public const string AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_FORMAT = "AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_{0}";

                public const string AGC_TWEAKABLE_NURSE_LEVEL_POINTS_FORMAT = "AGC_TWEAKABLE_NURSE_LEVEL_POINTS_{0}";

                public const string AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_FORMAT = "AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_{0}";

                public const string AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_FORMAT = "AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_{0}";


                public const string AGC_TWEAKABLE_ENABLE_EXTRA_LEVELING_PERCENT = "AGC_TWEAKABLE_ENABLE_EXTRA_LEVELING_PERCENT";

                public const string AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL = "AGC_TWEAKABLE_ALLOWED_CLINIC_DOCTORS_LEVEL";
            }

            public static class Vanilla
            {
                public const string LevelingRatePercent = "LEVELING_RATE_PERCENT";
            }
        }

        public static class Tags
        {
            public static class Mod
            {
                public const string JanitorTrainingWorkspace = "janitor_training_workspace";
            }
        }

        public static class Localizations
        {
            public static class Mod
            {
                public const string AGC_DOCTOR_DESCRIPTION = "AGC_DOCTOR_DESCRIPTION";
                public const string AGC_NURSE_DESCRIPTION = "AGC_NURSE_DESCRIPTION";
                public const string AGC_LAB_SPECIALIST_DESCRIPTION = "AGC_LAB_SPECIALIST_DESCRIPTION";
                public const string AGC_JANITOR_DESCRIPTION = "AGC_JANITOR_DESCRIPTION";
            }

            public static class Vanilla
            {
                public const string DOCTOR = "DOCTOR";
                public const string DOCTORS = "DOCTORS";
                public const string NURSE = "NURSE";
                public const string NURSES = "NURSES";
                public const string LAB_TECHNOLOGIST = "LAB_TECHNOLOGIST";
                public const string LAB_TECHNOLOGISTS = "LAB_TECHNOLOGISTS";
                public const string JANITOR = "JANITOR";
                public const string JANITORS = "JANITORS";
            }
        }
    }
}
