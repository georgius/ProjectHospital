using System;

namespace ModGameChanges
{
    public static class Debug
    {
        public static void Log(System.Reflection.MethodBase method, string message)
        {
            UnityEngine.Debug.Log("AGC: " + method.DeclaringType.FullName + "." + method.Name + "(): " + message);
        }

        public static void LogError(System.Reflection.MethodBase method, string message, Exception exception)
        {
            UnityEngine.Debug.LogError("AGC: " + method.DeclaringType.FullName + "." + method.Name  + "(): " + message + "\n" + exception.ToString());
        }

        public static void LogDebug(System.Reflection.MethodBase method, string message)
        {
            if (ViewSettingsPatch.m_enabled && ViewSettingsPatch.m_debug[SettingsManager.Instance.m_viewSettings].m_value)
            {
                Debug.Log(method, message);
            }
        }
    }
}
