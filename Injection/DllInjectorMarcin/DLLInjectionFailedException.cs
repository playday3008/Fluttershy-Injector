using System;

namespace DLLInjectionMarcin
{
    [Serializable]
#pragma warning disable CA2229, CA1032 // Implement serialization constructors // Implement standard exception constructors
    public class DLLInjectionFailedException : Exception
#pragma warning restore CA1032, CA2229 // Implement standard exception constructors // Implement serialization constructors
    {
        public DLLInjectionFailedException(string message) : base(message) { }
    }
}
