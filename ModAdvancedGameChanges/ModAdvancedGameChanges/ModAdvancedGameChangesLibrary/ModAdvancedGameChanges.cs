using System;
using System.Reflection;
using HarmonyLib;
using ModAdvancedGameChanges;

// Note: .NET framework version needs to be set to 3.5
[assembly: AssemblyTitle("ModAdvancedGameChanges ")] // ENTER MOD TITLE
[assembly: AssemblyFileVersion("1.0.0.0")] // MOD VERSION
[assembly: AssemblyVersion("1.2.0.0")] // GAME VERSION

// Parent HospitalMod class is inherited also from Mono Behavior, so this gets automatically added as a component
// on its own game object in Unity as soon as the main menu loads
public class AdvancedGameChanges : HospitalMod 
{
    internal static bool m_enabled = false;

    public AdvancedGameChanges() 
        : base()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Start");

        try
        {
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Booting up Harmony");
            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Patching " + typeof(AdvancedGameChanges).Assembly.FullName);

            Harmony harmony = new Harmony(GetType().FullName);
            harmony.PatchAll(typeof(AdvancedGameChanges).Assembly);

            Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Booted up Harmony");

            AdvancedGameChanges.m_enabled = true;
        }
        catch (Exception ex)
        {
            Debug.LogError(System.Reflection.MethodBase.GetCurrentMethod(), "Harmony patch fail", ex);
        }

        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "End");
    }

    void Start()
    {
        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Mod init start");

        Debug.Log(System.Reflection.MethodBase.GetCurrentMethod(), "Mod init end");
    } 
}