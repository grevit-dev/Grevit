using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grevit.Reflection
{
    public class Utilities
    {
        /// <summary>
        /// Get Extension Methods
        /// </summary>
        /// <param name="assembly">Assembly to check (this)</param>
        /// <param name="extendedType">Class</param>
        /// <returns></returns>
        public static IEnumerable<System.Reflection.MethodInfo> GetExtensionMethods(System.Reflection.Assembly assembly, Type extendedType)
        {
            var query = from type in assembly.GetTypes()
                        where type.IsSealed && !type.IsGenericType && !type.IsNested
                        from method in type.GetMethods(System.Reflection.BindingFlags.Static
                            | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                        where method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false)
                        where method.GetParameters()[0].ParameterType == extendedType
                        select method;
            return query;
        }
    }
}
