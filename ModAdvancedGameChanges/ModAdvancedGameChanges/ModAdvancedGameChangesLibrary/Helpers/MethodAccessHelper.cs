using System;
using System.Reflection;

namespace ModAdvancedGameChanges.Helpers
{
    public static class MethodAccessHelper
    {
        public static void CallMethod(this object instance, string methodName, params object[] parameters)
        {
            Type type = instance.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(instance, parameters);
        }

        public static TResult CallMethod<TResult>(this object instance, string methodName, params object[] parameters)
        {
            Type type = instance.GetType();
            MethodInfo methodInfo = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            return (TResult)methodInfo.Invoke(instance, parameters);
        }
    }
}
