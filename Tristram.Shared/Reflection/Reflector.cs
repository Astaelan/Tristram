using System;
using System.Collections.Generic;
using System.Reflection;

namespace Tristram.Shared.Reflection
{
    public static class Reflector
    {
        public static List<Tuple<T1, T2>> FindAllMethods<T1, T2>()
            where T1 : Attribute
            where T2 : class
        {
            if (!typeof(T2).IsSubclassOf(typeof(Delegate))) return null;
            List<Tuple<T1, T2>> results = new List<Tuple<T1, T2>>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GlobalAssemblyCache) continue;
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    MethodInfo[] methods = type.GetMethods();
                    foreach (MethodInfo method in methods)
                    {
                        T1 attribute = Attribute.GetCustomAttribute(method, typeof(T1), false) as T1;
                        if (attribute == null) continue;
                        T2 callback = Delegate.CreateDelegate(typeof(T2), method, false) as T2;
                        if (callback == null) continue;
                        results.Add(new Tuple<T1, T2>(attribute, callback));
                    }
                }
            }
            return results;
        }

        public static List<T> FindAllClasses<T>()
            where T : Attribute
        {
            List<T> results = new List<T>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GlobalAssemblyCache) continue;
                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    object[] attributes = type.GetCustomAttributes(typeof(T), false);
                    if (attributes.Length != 0)
                    {
                        T attribute = attributes[0] as T;
                        results.Add(attribute);
                    }
                }
            }
            return results;
        }
    }
}
