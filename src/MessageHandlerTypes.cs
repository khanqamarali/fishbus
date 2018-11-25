using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Thon.Hotels.FishBus
{
    public static class MessageHandlerTypes
    {
        public static IEnumerable<Type> GetAll(Assembly assembly) =>
            GetAllTypesImplementingOpenGenericType(typeof(IHandleMessage<>), assembly);

        private static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(Type openGenericType, Assembly assembly)
        {
            return from x in assembly.GetTypes()
                   from z in x.GetInterfaces()
                   let y = x.BaseType
                   where
                   (y != null && y.IsGenericType &&
                   openGenericType.IsAssignableFrom(y.GetGenericTypeDefinition())) ||
                   (z.IsGenericType &&
                   openGenericType.IsAssignableFrom(z.GetGenericTypeDefinition()))
                   select x;
        }
    }
}
