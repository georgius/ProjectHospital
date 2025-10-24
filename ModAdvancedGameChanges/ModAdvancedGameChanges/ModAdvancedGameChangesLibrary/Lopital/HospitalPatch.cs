using HarmonyLib;
using Lopital;
using ModGameChanges;
using System;
using System.Collections;
using System.Reflection;

namespace ModAdvancedGameChanges.Lopital
{
    [HarmonyPatch(typeof(Hospital))]
    public static class HospitalPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Hospital), nameof(Hospital.CreateNew))]
        public static bool CreateNewPrefix()
        {
            if (!ViewSettingsPatch.m_enabled)
            {
                // Allow original method to run
                return true;
            }

            bool dlcHospitalServices = Tweakable.Vanilla.DlcHospitalServicesEnabled();

            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Patching hospital");
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"DLC Hospital services present - {dlcHospitalServices}");

            if (!dlcHospitalServices)
            {
                ViewSettingsPatch.m_enabledTrainingDepartment = false;

                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Disabling training department");

                // Get the Type of the class
                Type type = typeof(Database);

                // Get the private field using BindingFlags
                FieldInfo fieldInfo = type.GetField("tables", BindingFlags.NonPublic | BindingFlags.Instance);

                // cast to IDictionary since we can’t use the internal generic type
                var tables = (IDictionary)fieldInfo.GetValue(Database.Instance);

                foreach (DictionaryEntry tableEntry in tables)
                {
                    if (tableEntry.Key == typeof(GameDBDepartment))
                    {
                        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Found game departments");

                        // tableEntry.Value is a DatabaseTable (internal) — treat it as IDictionary
                        var dbTable = (IDictionary)tableEntry.Value;

                        foreach (DictionaryEntry entry in dbTable)
                        {
                            if (entry.Key.ToString() == Constants.Departments.Mod.TrainingDepartment)
                            {
                                Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), $"Found training department ({Constants.Departments.Mod.TrainingDepartment}), removing");

                                dbTable.Remove(entry.Key);
                                break;
                            }
                        }
                        break;
                    }
                }
            }

            return true;
        }
    }
}
