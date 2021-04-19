using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Macros.Autowire
{
    internal class Storage
    {
        internal static Storage Current { get; set; } = new Storage();
        private ConcurrentDictionary<Type, List<AutowireContext>> registeredTypes;
        private ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object>>> instanceContext;
        private ConcurrentDictionary<Scope, ConcurrentDictionary<Type, ConcurrentDictionary<string, WeakReference<Func<object>>>>> scopedInstanced;
        private Storage()
        {
            registeredTypes = new ConcurrentDictionary<Type, List<AutowireContext>>();
            instanceContext = new ConcurrentDictionary<Type, ConcurrentDictionary<string, Func<object>>>();
            scopedInstanced = new ConcurrentDictionary<Scope, ConcurrentDictionary<Type, ConcurrentDictionary<string, WeakReference<Func<object>>>>>();
        }
        internal List<AutowireContext> GetOrSetType(Type type)
        {
            if (registeredTypes.ContainsKey(type))
            {
                return registeredTypes[type];
            }
            List<AutowireContext> autoWireProperties = new List<AutowireContext>();
            foreach (var property in type.GetProperties())
            {
                AutowiredAttribute autowireAttribute = property.GetCustomAttribute<AutowiredAttribute>();
                if (autowireAttribute != null)
                {
                    autoWireProperties.Add(new AutowireContext(autowireAttribute, property));
                }
            }
            registeredTypes.TryAdd(type, autoWireProperties);
            return autoWireProperties;
        }
        internal Func<object> GetInstance(Type type, string name, Scope scope = default)
        {
            if (scope.HasValue)
            {
                var instance = instanceContext[type][name ?? "default"]();
                if (scopedInstanced.ContainsKey(scope))
                {
                    if (scopedInstanced[scope].ContainsKey(type))
                    {
                        if (scopedInstanced[scope][type].ContainsKey(name ?? "default"))
                        {
                            if (scopedInstanced[scope][type][name ?? "default"].TryGetTarget(out Func<object> _value))
                            {
                                var tttt = _value();
                                return _value;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            scopedInstanced[scope][type].TryAdd(name ?? default, new WeakReference<Func<object>>(() => instance));
                        }
                    }
                    else
                    {
                        scopedInstanced[scope].TryAdd(type, new ConcurrentDictionary<string, WeakReference<Func<object>>>
                        {
                            [name ?? "default"] = new WeakReference<Func<object>>(() => instance)
                        });
                    }
                }
                else
                {
                    scopedInstanced.TryAdd(scope, new ConcurrentDictionary<Type, ConcurrentDictionary<string, WeakReference<Func<object>>>>
                    {
                        [type] = new ConcurrentDictionary<string, WeakReference<Func<object>>>
                        {
                            [name ?? "default"] = new WeakReference<Func<object>>(() => instance)
                        }
                    });
                }
                return (() => instance);
            }
            return instanceContext[type][name ?? "default"];
        }
        internal bool TryRemoveScope(Scope scope)
        {
            return scopedInstanced.TryRemove(scope, out ConcurrentDictionary<Type, ConcurrentDictionary<string, WeakReference<Func<object>>>> value);
        }
        internal void Bind(Type type, BindAttribute bindAttribute, Scope scope = default)
        {
            Func<object> instanceGenerator = null;
            switch (bindAttribute.BindingMethod)
            {
                case BindingMethods.SINGLETON:
                    {
                        var instance = Activator.CreateInstance(type);
                        instanceGenerator = () => instance;
                        break;
                    }
                case BindingMethods.RELATIVE:
                    {
                        instanceGenerator = () => Activator.CreateInstance(type);
                        break;
                    }
            }
            if (instanceContext.TryGetValue(bindAttribute.UnderlyingType, out ConcurrentDictionary<string, Func<object>> outValue))
            {
                outValue.TryAdd(bindAttribute.Name ?? "default", instanceGenerator);
            }
            else
            {
                instanceContext.TryAdd(bindAttribute.UnderlyingType, new ConcurrentDictionary<string, Func<object>>
                {
                    [bindAttribute.Name ?? "default"] = instanceGenerator
                });
            }
        }
        internal void Bind(Type type, Func<object> instanceGenerator, string name = null, Scope scope = default)
        {
            if (instanceContext.TryGetValue(type, out ConcurrentDictionary<string, Func<object>> outValue))
            {
                outValue.TryAdd(name ?? "default", instanceGenerator);
            }
            else
            {
                instanceContext.TryAdd(type, new ConcurrentDictionary<string, Func<object>>
                {
                    [name ?? "default"] = instanceGenerator
                });
            }
        }
    }
    internal readonly struct AutowireContext
    {
        private readonly AutowiredAttribute autowireAttribute;
        private readonly PropertyInfo propertyInfo;
        internal Type UnderlyingType => propertyInfo.PropertyType;
        internal AutowireContext(AutowiredAttribute autowireAttribute, PropertyInfo propertyInfo)
        {
            this.autowireAttribute = autowireAttribute;
            this.propertyInfo = propertyInfo;
        }
        internal void Instantiate(object obj, Scope scope = default)
        {
            propertyInfo.SetValue(obj, Storage.Current.GetInstance(autowireAttribute.UnderlyingType ?? propertyInfo.PropertyType, autowireAttribute.Name, scope)());
        }
    }
}