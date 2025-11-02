namespace ModAdvancedGameChanges.Constants
{
    public static class Departments
    {
        public static class Mod
        {
            public const string TrainingDepartment = "AGC_DPT_TRAINING";
        }

        public static class Vanilla
        {
            /// <summary>
            /// This "default" department is sometimes mentioned in code,
            /// but it is some strange department, because in game it does not exist.
            /// </summary>
            public const string Default = "DPT_DEFAULT";

            public const string Emergency = "DPT_EMERGENCY";
            public const string Radiology = "DPT_RADIOLOGY";
            public const string MedicalLaboratories = "DPT_LAB";
            public const string IntensiveCareUnit = "DPT_ICU";
            public const string AdministrativeDepartment = "DLC_DPT_ADMINISTRATIVE";
            public const string Pathology = "DLC_DPT_PATHOLOGY";
        }
    }
}
