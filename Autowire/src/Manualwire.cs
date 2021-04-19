using System;

namespace Macros.Autowire
{
    public static class Manualwire
    {
        public static void BindSingleton(object obj, string name = null)
        {
            Storage.Current.Bind(obj.GetType(), () => obj, name);
        }
        public static void BindSingleton<T>(object obj, string name = null)
        {
            Storage.Current.Bind(typeof(T), () => obj, name);
        }
        public static void BindRelative(Func<object> instanceGenerator, string name = null)
        {
            Storage.Current.Bind(instanceGenerator.Method.ReturnType, instanceGenerator, name);
        }
        public static void BindRelative<T>(Func<object> instanceGenerator, string name = null)
        {
            Storage.Current.Bind(typeof(T), instanceGenerator, name);
        }
        public static void BindRelative<T>(string name = null) where T : new()
        {
            Storage.Current.Bind(typeof(T), () => new T(), name);
        }
        public static void BindRelative<T, R>(string name = null) where T : new()
        {
            Storage.Current.Bind(typeof(R), () => new T(), name);
        }
    }
}