using System;
using System.Linq;
using System.Reflection;

namespace Macros.Autowire
{
    public static class AutowireExtensions
    {
        static AutowireExtensions()
        {
            var assembly = Assembly.GetEntryAssembly();
            var referencedAssemblies = assembly.GetReferencedAssemblies().SelectMany(x => Assembly.Load(x.FullName).ExportedTypes);
            var exportedTypes = assembly.GetExportedTypes().Concat(referencedAssemblies);
            foreach (var i in exportedTypes)
            {
                BindAttribute bindAttribute = i.GetCustomAttribute<BindAttribute>();
                if (bindAttribute != null)
                {
                    Storage.Current.Bind(i, bindAttribute);
                }
            }
        }
        public static void EnableAutowiring(this object obj, params Type[] exclusions)
        {
            Type typeOfObject = obj.GetType();
            var autowireTypes = Storage.Current.GetOrSetType(typeOfObject);
            foreach (var autowireType in autowireTypes)
            {
                autowireType.Instantiate(obj);
            }
        }
        public static void EnableAutowiring(this object obj, Scope scope, params Type[] exclusions)
        {
            Type typeOfObject = obj.GetType();
            var autowireTypes = Storage.Current.GetOrSetType(typeOfObject);
            foreach (var autowireType in autowireTypes)
            {
                autowireType.Instantiate(obj, scope);
            }
        }
    }
}