using System;
using System.Linq;
using System.Reflection;

namespace GameTac.Net.Client.Core
{
    public class Reflection
    {
        public static Type[] GetChilds<T>()
        {
            var baseType = typeof(T);
            var assembly = Assembly.GetAssembly(baseType);
            if (assembly is null) return new Type[] { };

            Type[] types = assembly.GetTypes().Where(type => type != baseType && baseType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract).ToArray();

            return types;
        }
    }
}