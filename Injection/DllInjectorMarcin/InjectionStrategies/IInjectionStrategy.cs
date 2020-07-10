using System;

namespace DLLInjectionMarcin.InjectionStrategies
{
    interface IInjectionStrategy
    {
        /// <summary>
        /// Inject DLL into process.
        /// </summary>
        /// <param name="processHandle"></param>
        /// <param name="dllPath"></param>
        /// <returns>
        /// Handle to newly created thread.
        /// </returns>
        IntPtr Inject(IntPtr processHandle, string dllPath);
    }
}
