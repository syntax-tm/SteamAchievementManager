using System;

namespace SAM.Extensions;

public static class TypeExtensions
{
    extension(Type type)
    {
        public bool ImplementsInterface<T>()
        {
            var interfaceType = typeof(T);
            return type.ImplementsInterface(interfaceType);
        }

        public bool ImplementsInterface(Type interfaceType)
        {
            var match = type.GetInterface(interfaceType.Name);
            return match != null;
        }
    }
}
