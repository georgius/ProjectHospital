using Lopital;
using System;
using System.Globalization;

namespace ModAdvancedGameChanges 
{
    public static class Debug
    {
        public static void Log(System.Reflection.MethodBase method, string message)
        {
            int? day = DayTime.Instance?.GetDay();
            float? time = DayTime.Instance?.GetDayTimeHours();

            if (day.HasValue && time.HasValue)
            {
                UnityEngine.Debug.Log($"AGC: {method.DeclaringType.FullName}.{method.Name}(): Day: { day?.ToString(CultureInfo.InvariantCulture) ?? "NULL" } Time: { time?.ToString(CultureInfo.InvariantCulture) ?? "NULL"} {message}");
            }
            else
            {
                UnityEngine.Debug.Log($"AGC: {method.DeclaringType.FullName}.{method.Name}(): {message}");
            }
        }

        public static void LogError(System.Reflection.MethodBase method, string message, Exception exception)
        {
            int? day = DayTime.Instance?.GetDay();
            float? time = DayTime.Instance?.GetDayTimeHours();

            if (day.HasValue && time.HasValue)
            {
                UnityEngine.Debug.LogError($"AGC: {method.DeclaringType.FullName}.{method.Name}(): Day: { day?.ToString(CultureInfo.InvariantCulture) ?? "NULL" } Time: { time?.ToString(CultureInfo.InvariantCulture) ?? "NULL"} {message}\n{exception}");
            }
            else
            {
                UnityEngine.Debug.LogError($"AGC: {method.DeclaringType.FullName}.{method.Name}(): {message}\n{exception}");
            }
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
