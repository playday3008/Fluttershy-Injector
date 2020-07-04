using System;

namespace DLLInjectionMarcin
{
    [Serializable]
    public class DLLInjectionFailedException : Exception
    {
        public DLLInjectionFailedException(string message) : base(message) { }
    }
}
