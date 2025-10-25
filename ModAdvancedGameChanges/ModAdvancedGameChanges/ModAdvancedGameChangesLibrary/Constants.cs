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

            public static class Vanilla
            {
                public const string AdministrativeDepartment = "DLC_DPT_ADMINISTRATIVE";
            }
        }

        public static class Mood
        {
            public static class Vanilla
            {
                public const string Hangover = "SAT_MOD_HUNGOVER";
            }
        }

        public static class Perks
        {
            public static class Vanilla
            {
                public const string FastLearner = "PERK_FAST_LEARNER";
                public const string SlowLearner = "PERK_SLOW_LEARNER";
                public const string LongCommute = "PERK_LONG_COMMUTE";
                public const string Alcoholism = "PERK_ALCOHOLISM";
            }
        }

        public static class Procedures
        {
            public static class Vanilla
            {
                public const string StaffTraining = "DLC_ADMIN_STAFF_TRAINING";
            }
        }

        public static class Schedule
        {
            public static class Vanilla
            {
                public const string SCHEDULE_OPENING_HOURS_STAFF = "SCHEDULE_OPENING_HOURS_STAFF";
                public const string SCHEDULE_OPENING_HOURS_STAFF_NIGHT = "SCHEDULE_OPENING_HOURS_STAFF_NIGHT";

                public const string SCHEDULE_SHIFT_CHANGE_MORNING = "SCHEDULE_SHIFT_CHANGE_MORNING";
                public const string SCHEDULE_SHIFT_CHANGE_EVENING = "SCHEDULE_SHIFT_CHANGE_EVENING";
            }
        }

        public static class Skills
        {
            public static class Vanilla
            {
                public const string SKILL_DOC_QUALIF_GENERAL_MEDICINE = "SKILL_DOC_QUALIF_GENERAL_MEDICINE";
                public const string SKILL_DOC_QUALIF_DIAGNOSIS = "SKILL_DOC_QUALIF_DIAGNOSIS";

                public const string SKILL_NURSE_QUALIF_PATIENT_CARE = "SKILL_NURSE_QUALIF_PATIENT_CARE";

                public const string SKILL_NURSE_SPEC_RECEPTIONIST = "SKILL_NURSE_SPEC_RECEPTIONIST";
                public const string SKILL_NURSE_SPEC_MEDICAL_SURGERY = "SKILL_NURSE_SPEC_MEDICAL_SURGERY";
                public const string SKILL_NURSE_SPEC_CLINICAL_SPECIALIST = "SKILL_NURSE_SPEC_CLINICAL_SPECIALIST";

                public const string SKILL_LAB_SPECIALIST_QUALIF_SCIENCE_EDUCATION = "SKILL_LAB_SPECIALIST_QUALIF_SCIENCE_EDUCATION";

                public const string SKILL_LAB_SPECIALIST_SPEC_BIOCHEMISTRY = "SKILL_LAB_SPECIALIST_SPEC_BIOCHEMISTRY";
                public const string SKILL_LAB_SPECIALIST_SPEC_USG = "SKILL_LAB_SPECIALIST_SPEC_USG";
                public const string SKILL_LAB_SPECIALIST_SPEC_CARDIOLOGY = "SKILL_LAB_SPECIALIST_SPEC_CARDIOLOGY";
                public const string SKILL_LAB_SPECIALIST_SPEC_NEUROLOGY = "SKILL_LAB_SPECIALIST_SPEC_NEUROLOGY";

                public const string SKILL_JANITOR_QUALIF_EFFICIENCY = "SKILL_JANITOR_QUALIF_EFFICIENCY";
                public const string SKILL_JANITOR_QUALIF_DEXTERITY = "SKILL_JANITOR_QUALIF_DEXTERITY";

                public const string DLC_SKILL_JANITOR_SPEC_VENDOR = "DLC_SKILL_JANITOR_SPEC_VENDOR";
                public const string DLC_SKILL_JANITOR_SPEC_MANAGER = "DLC_SKILL_JANITOR_SPEC_MANAGER";
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

                public const string DLC_ADMIN_PATHOLOGY_ENABLED = "DLC_ADMIN_PATHOLOGY_ENABLED";
            }
        }

        public static class Tags
        {
            public static class Mod
            {
                public const string JanitorTrainingWorkspace = "janitor_training_workspace";
            }

            public static class Vanilla
            {
                public const string JanitorAdminWorkplace = "janitor_admin_workplace";
                public const string JanitorWorkspace = "janitor_workspace";
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
