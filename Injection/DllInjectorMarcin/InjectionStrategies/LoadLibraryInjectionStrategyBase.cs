﻿using System;
using System.Text;

namespace DLLInjectionMarcin.InjectionStrategies
{
    abstract class LoadLibraryInjectionStrategyBase : IInjectionStrategy
    {

        public IntPtr Inject(IntPtr processHandle, string dllPath)
        {
            if (processHandle == IntPtr.Zero)
                throw new ArgumentException("Invalid process handle", nameof(processHandle));

            if (string.IsNullOrWhiteSpace(dllPath))
                throw new ArgumentException("Invalid dll path", "pathToDll");

            byte[] pathBytes = Encoding.ASCII.GetBytes(dllPath + "\0");
            var addressOfDllPath = WinAPI.VirtualAllocEx(
                             processHandle,
                             IntPtr.Zero,
                             (uint)pathBytes.Length,
                             WinAPI.AllocationType.Reserve | WinAPI.AllocationType.Commit,
                             WinAPI.MemoryProtection.ExecuteReadWrite);

            Utils.CheckForFailure(addressOfDllPath == IntPtr.Zero, "Cannot allocate memory in process");


            bool success = WinAPI.WriteProcessMemory(
                processHandle,
                addressOfDllPath,
                pathBytes,
                pathBytes.Length,
                #pragma warning disable IDE0059 // Ненужное присваивание значения
                out IntPtr tmp);
                #pragma warning restore IDE0059 // Ненужное присваивание значения

            Utils.CheckForFailure(!success, "Cannot write to process memory");


            IntPtr kernel32Module = WinAPI.GetModuleHandle(WinAPI.KERNEL32_DLL);
            Utils.CheckForFailure(kernel32Module == IntPtr.Zero, "Cannot get handle to kernel32 module");


            IntPtr loadLibraryAddress = WinAPI.GetProcAddress(kernel32Module, WinAPI.LOAD_LIBRARY_PROC);
            Utils.CheckForFailure(loadLibraryAddress == IntPtr.Zero, "Cannot get address of LoadLibrary function");


            IntPtr remoteThreadHandle = this.Inject(processHandle, loadLibraryAddress, addressOfDllPath);
            Utils.CheckForFailure(remoteThreadHandle == IntPtr.Zero, "Cannot create remote thread using {0} method.", this.GetType().Name);

            return remoteThreadHandle;
        }

        protected abstract IntPtr Inject(IntPtr processHandle, IntPtr loadLibraryAddress, IntPtr addressOfDllPath);
    }
}
