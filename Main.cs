using DLLInjectionMarcin;
using ManualMapInjection.Injection;
using MetroFramework;
using MetroFramework.Forms;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Injector
{
    public partial class Main : MetroForm
    {
        private int SelectedProcessId = 0; // process id
        private bool x64 = false; // x64 = true - x64bit process, x64 = false - x32bit process
        private bool? x32 = false; // x32 = true - x64bit DLL, x32 = false - x32bit DLL, x32 = null - bloken DLL
        DialogResult archDll = DialogResult.None; // Yes - try to inject broken dll, No - skip

        #region DllImport

        /// <summary>
        /// External C++ function (BLLI)
        /// </summary>
        /// <param name="pid">Process ID</param>
        [DllImport("BLLI.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void BypassLLI(int pid);

        /// <summary>
        /// External C++ function (Inflame)
        /// </summary>
        /// <param name="dllName">Path to DLL (.ToCharArray())</param>
        /// <param name="PID">Prcess ID</param>
        [DllImport("I.dll", CallingConvention = CallingConvention.Cdecl)]
        #pragma warning disable IDE1006, CA1401 // Стили именования // P/Invokes should not be visible
        public static extern void manualMap(char[] dllName, int PID);
        #pragma warning restore IDE1006, CA1401 // Стили именования // P/Invokes should not be visible

        [DllImport("kernel32")]
        #pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern IntPtr CreateRemoteThread(
          IntPtr hProcess,
          IntPtr lpThreadAttributes,
          uint dwStackSize,
          UIntPtr lpStartAddress, // raw Pointer into remote process  
          IntPtr lpParameter,
          uint dwCreationFlags,
          out IntPtr lpThreadId
        );

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
            UInt32 dwDesiredAccess,
            Int32 bInheritHandle,
            Int32 dwProcessId
        );

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool VirtualFreeEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            UIntPtr dwSize,
            uint dwFreeType
        );

        #pragma warning disable CA2101 // Specify marshaling for P/Invoke string arguments
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern UIntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(
            IntPtr hProcess,
            IntPtr lpAddress,
            uint dwSize,
            uint flAllocationType,
            uint flProtect
        );

        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            string lpBuffer,
            UIntPtr nSize,
            out IntPtr lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        #pragma warning restore CA2101 // Specify marshaling for P/Invoke string arguments
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        #pragma warning restore CA1401 // P/Invokes should not be visible

        [DllImport("kernel32", SetLastError = true, ExactSpelling = true)]
        internal static extern Int32 WaitForSingleObject(IntPtr handle, Int32 milliseconds);

        #endregion

        public Main()
        {
            InitializeComponent();

            RefreshButton_Click(null, EventArgs.Empty); // automatically refresh process list
            InjectMethodCB.SelectedIndex = 0; // set to default element "ManualMap (C#) (x32 only)"
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ProcessList.Items.Clear();// clear process list
            Process[] MyProcess = Process.GetProcesses(); // get process list
            for (int i = 0; i < MyProcess.Length; i++)// add processes to ProcessList (step-by-step)
                ProcessList.Items.Add(MyProcess[i].ProcessName + ".exe - " + MyProcess[i].Id);
            int maxWidth = 0, temp;
            foreach (var obj in ProcessList.Items)
            {// function to set DropDownWidth dynamically
                temp = TextRenderer.MeasureText(obj.ToString(), ProcessList.Font).Width;
                if (temp > maxWidth)
                    maxWidth = temp;
            }
            ProcessList.DropDownWidth = Convert.ToInt32(maxWidth * 1.35);
        }

        private void OpenFileButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                openFileDialog.Filter = "DLL files (*.dll)|*.dll"; // show only .dll files
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)// if dll selected
                {
                    x32 = UnmanagedDllIs64Bit(openFileDialog.FileName);// check dll arch

                    if (x32 == null)// if can't detect dll arch
                        archDll = MetroMessageBox.Show(this, "Your dll is broken, or someting  else, do you want to continue?", "Fluttershy-Injector", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, 120);
                    if (archDll == DialogResult.Yes | archDll == DialogResult.None) // else LOL
                    {
                        DllPathTextBox.Text = openFileDialog.FileName; // print path to dll to textbox
                        if (archDll == DialogResult.None) // if arch detected
                            SelectedDllLabel.Text = "DLL: " + openFileDialog.SafeFileName + ((bool)x32 ? " (x64)" : " (x32)");// show dll arch
                        else if (archDll == DialogResult.Yes) // else LOL
                            SelectedDllLabel.Text = "DLL: " + openFileDialog.SafeFileName + "(WTF dude)";// show dll arch
                    }
                }
            }
        }

        private void InjectButton_Click(object sender, EventArgs e)
        {
            if (ProcessList.SelectedItem != null & !string.IsNullOrEmpty(DllPathTextBox.Text) & File.Exists(DllPathTextBox.Text) & Process.GetProcesses().Any(x => x.Id == SelectedProcessId))
            { // if item selected and textbox !empty and file exist and process exist
                SwitchUI(true); // disable UI controls
                if (InjectMethodCB.SelectedIndex == 0) // if selected "ManualMap (C#) (x32 only)"
                    ManualMapCSx32(SelectedProcessId, DllPathTextBox.Text); // run injection
                else if (InjectMethodCB.SelectedIndex == 1) // if selected "ManualMap (Inflame) (x32 only)"
                    Inflame(SelectedProcessId, DllPathTextBox.Text); // run injection
                else if (InjectMethodCB.SelectedIndex == 2) // if selected "CreateRemoteThread (C#) (x32 only)"
                {
                    BypassLLIRun(SelectedProcessId); // Bypass LoadLibrary injection if csgo.exe detected
                    DllInjectorMarcin(SelectedProcessId, DllPathTextBox.Text, InjectionMethod.CREATE_REMOTE_THREAD); // run injection
                }
                else if (InjectMethodCB.SelectedIndex == 3) // if selected "LoadLibraryA (C#/C++) (x32 only)
                {
                    BypassLLIRun(SelectedProcessId); // Bypass LoadLibrary injection if csgo.exe detected
                    XenforoInj(SelectedProcessId, DllPathTextBox.Text); // run injection
                }
                SwitchUI(false); // release UI controls
            }
            else if (string.IsNullOrEmpty(DllPathTextBox.Text) | !File.Exists(DllPathTextBox.Text)) // if textbox empty or file !exist
                MetroMessageBox.Show(this, "INCORRECT DLL PATH", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (ProcessList.SelectedItem == null) // if process not selected
                MetroMessageBox.Show(this, "SELCET PROCESS FIRST", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (!Process.GetProcesses().Any(x => x.Id == SelectedProcessId)) // if process !exist
                MetroMessageBox.Show(this, "Please refresh process list", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
        }

        private void VACBypassLabel_Click(object sender, EventArgs e)
        {
            DialogResult vac = MetroMessageBox.Show(this, "Do you want start VAC-Bypass?", "Fluttershy-Injector", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, 120);
            if (vac == DialogResult.Yes)
            {// if clicked "Yes"
                SwitchUI(true); // disable UI controls
                VACBypassRun(); // run VACBypass function
                SwitchUI(false); // release UI controls
            }
        }

        private void DllPathTextBox_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(DllPathTextBox.Text)) // if file exist
            { // then check file extension
                if (Path.GetExtension(DllPathTextBox.Text) == ".dll") // if extension .dll
                { // then check dll arch
                    x32 = UnmanagedDllIs64Bit(DllPathTextBox.Text); // x32 = true - x64bit DLL, x32 = false - x32bit DLL, x32 = null - bloken DLL

                    if (x32 != null) // if arch detected
                        SelectedDllLabel.Text = "DLL: " + Path.GetFileName(DllPathTextBox.Text) + ((bool)x32 ? " (x64)" : " (x32)"); // show arch
                    else
                        SelectedDllLabel.Text = "DLL: " + Path.GetFileName(DllPathTextBox.Text) + "(WTF dude)";
                }
                else
                    SelectedDllLabel.Text = "DLL: ";
            }
            else
                SelectedDllLabel.Text = "DLL: ";
        }

        private void ProcessList_SelectedIndexChanged(object sender, EventArgs e)
        {
            // parse process id from selected process
            SelectedProcessId = int.Parse(ProcessList.SelectedItem.ToString().Substring(ProcessList.SelectedItem.ToString().IndexOf('-') + 2));
            if (Process.GetProcesses().Any(x => x.Id == SelectedProcessId))
            {// if process exist
                try
                {// try detect process arch
                    if (IsWin64Emulator(Process.GetProcessById(SelectedProcessId)))
                        x64 = false; // 32bit proc
                    else
                        x64 = true; // 64bit proc
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode != 0x00000005) // if SYSTEM process, then cacth exception
                    {
                        throw;
                    }
                }
                SelectedProcLabel.Text = "Process: " + Process.GetProcessById(SelectedProcessId).ProcessName + ".exe" + (x64 ? " (x64)" : " (x32)"); // show process arch
                SelectedPidLabel.Text = "PID: " + SelectedProcessId; // show process id
            }
            else // if process !exist
                MetroMessageBox.Show(this, "Please refresh process list", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
        }

        private void CerditsButton_Click(object sender, EventArgs e)
        {
            var creditsForm = new Credits(); // init credits window
            if (!Application.OpenForms.OfType<Credits>().Any()) // If form !opened
                creditsForm.Show(); // show form
        }

        #region Injection Methods

        /// <summary>
        /// ManualMap injection on C# using WinAPI functions
        /// </summary>
        /// <param name="pid">Process ID</param>
        /// <param name="dllPath">Path to DLL</param>
        private void ManualMapCSx32(int pid, string dllPath)
        {
            if (!x64 & x32 == false)
            {// if proc and dll is x32bit
                var injector = new ManualMapInjector(Process.GetProcessById(pid)) { AsyncInjection = true };// init injector
                var injected = $"hmodule = 0x{injector.Inject(dllPath).ToInt64():x8}";// inject dll
                MetroMessageBox.Show(this, "Inject result:" + Environment.NewLine + injected, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Information, 160); // show inject result
            }
            else if (x64)// if proc x64bit
                MetroMessageBox.Show(this, "ONLY x32 PROCESSES", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (x32 != false)// if dll !x32bit
                MetroMessageBox.Show(this, "ONLY x32 DLL'S", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
        }

        /// <summary>
        /// ManualMap injection on C++ (using DllImport) (Inflame)
        /// </summary>
        /// <param name="pid">Process ID</param>
        /// <param name="dllPath">Path to DLL</param>
        private async void Inflame(int pid, string dllPath)
        {
            if (!x64 & x32 == false)
            {// if proc and dll is x32bit
                bool locked = false; // need to avoid IOException
                await Task.Run(() =>
                {// run code async to avoid UI lags
                    string Inflame = "I.dll"; // name of Inflame dll
                    if (IsFileLocked(new FileInfo(Inflame))) // if file locked
                        locked = true; // change to true
                    if (!locked) // if file unlocked
                        File.Delete(Inflame); // delete file
                    if (!File.Exists(Inflame)) // if file not exists
                    {
                        File.WriteAllBytes(Inflame, Properties.Resources.Inflame); // Create Inflame dll from exe memory
                        File.SetAttributes(Inflame, FileAttributes.Hidden); // hide dll from user
                        File.SetAttributes(Inflame, FileAttributes.ReadOnly); // dll read-only
                        File.SetAttributes(Inflame, FileAttributes.Temporary); // dll temporary
                    }
                    if (File.Exists(Inflame)) // if dll exist
                        manualMap(dllPath.ToCharArray(), pid); // inject dll
                    else // if file !exist, throw exception
                        throw new FileNotFoundException("Can't load dll, try add:\n" + Path.GetDirectoryName(Application.ExecutablePath) + "\nfolder to antivirus exceptions");
                }).ConfigureAwait(false);
            }
            else if (x64)// if proc x64bit
                MetroMessageBox.Show(this, "ONLY x32 PROCESSES", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (x32 != false)// if dll !x32
                MetroMessageBox.Show(this, "ONLY x32 DLL'S", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);

        }

        /// <summary>
        /// ManualMap injection on C# using WinAPI functions
        /// </summary>
        /// <param name="pid">Process ID</param>
        /// <param name="dllPath">Path to DLL</param>
        /// <param name="injectionMethod">InjectionMethod.CREATE_REMOTE_THREAD or InjectionMethod.NT_CREATE_THREAD_EX</param>
        private async void DllInjectorMarcin(int pid, string dllPath, InjectionMethod injectionMethod)
        {
            try
            { // try to inject
                var injector = new DLLInjector(injectionMethod); // init inject
                await Task.Run(() => injector.Inject(pid, dllPath)).ConfigureAwait(false); // run inject async to avoid UI lags
            }
            #pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                MetroMessageBox.Show(this, ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error, 150);
            }
            #pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// CopyPaste from XenforoLoader
        /// </summary>
        /// <param name="pid">Process ID</param>
        /// <param name="dllPath">Path to DLL</param>
        private async void XenforoInj(int pid, String dllPath)
        {
            IntPtr hProcess = OpenProcess(0x1F0FFF, 1, pid); // try to open process
            if (hProcess == null)
            {
                MetroMessageBox.Show(this, "OpenProcess() Failed!");
                return;
            }
            else
                await Task.Run(() =>
                    {
                        // Length of string containing the DLL file name +1 byte padding  
                        Int32 LenWrite = dllPath.Length + 1;
                        // Allocate memory within the virtual address space of the target process  
                        IntPtr AllocMem = (IntPtr)VirtualAllocEx(hProcess, (IntPtr)null, (uint)LenWrite, 0x1000, 0x40); //allocation pour WriteProcessMemory  

                        // Write DLL file name to allocated memory in target process  
                        WriteProcessMemory(hProcess, AllocMem, dllPath, (UIntPtr)LenWrite, out _);
                        // Function pointer "Injector"  
                        UIntPtr Injector = (UIntPtr)GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                        if (Injector == null)
                        {
                            MessageBox.Show(" Injector Error! \n ");
                            // return failed  
                            return;
                        }

                        // Create thread in target process, and store handle in hThread  
                        IntPtr hThread = (IntPtr)CreateRemoteThread(hProcess, (IntPtr)null, 0, Injector, AllocMem, 0, out _);
                        // Make sure thread handle is valid  
                        if (hThread == null)
                        {
                            //incorrect thread handle ... return failed  
                            MessageBox.Show(" hThread [ 1 ] Error! \n ");
                            return;
                        }
                        // Time-out is 10 seconds...  
                        int Result = WaitForSingleObject(hThread, 10 * 1000);
                        // Check whether thread timed out...  
                        #pragma warning disable CS0652 // Сравнение с константой интеграции бесполезно: константа находится за пределами диапазона типа
                        if (Result == 0x00000080L || Result == 0x00000102L || Result == 0xFFFFFFFF)
                        #pragma warning restore CS0652 // Сравнение с константой интеграции бесполезно: константа находится за пределами диапазона типа
                        {
                            /* Thread timed out... */
                            MessageBox.Show(" hThread [ 2 ] Error! \n ");
                            // Make sure thread handle is valid before closing... prevents crashes.  
                            if (hThread != null)
                            {
                                //Close thread in target process  
                                CloseHandle(hThread);
                            }
                            return;
                        }
                        // Sleep thread for 1 second  
                        Thread.Sleep(1000);
                        // Clear up allocated space ( Allocmem )  
                        VirtualFreeEx(hProcess, AllocMem, (UIntPtr)0, 0x8000);
                        // Make sure thread handle is valid before closing... prevents crashes.  
                        if (hThread != null)
                        {
                            //Close thread in target process  
                            CloseHandle(hThread);
                        }
                        // return succeeded  
                        return;
                    }).ConfigureAwait(false);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Run LoadLibrary bypass function
        /// </summary>
        /// <param name="pid">Process ID</param>
        private async void BypassLLIRun(int pid)
        {
            if (Path.GetFileName(Process.GetProcessById(pid).MainModule.FileName) == "csgo.exe")// if process name is "csgo.exe"
            {// then
                bool locked = false; // need to avoid IOException
                await Task.Run(() =>
                {// run code async to avoid UI lags
                    var BLLI = "BLLI.dll"; // name of BLLI dll
                    if (IsFileLocked(new FileInfo(BLLI))) // if file locked
                        locked = true; // change to true
                    if (!locked) // if file unlocked
                        File.Delete(BLLI); // delete file
                    if (!File.Exists(BLLI)) // if file not exists
                    {
                        File.WriteAllBytes(BLLI, Properties.Resources.BypassLLI); // Create BLLI dll from exe memory
                        File.SetAttributes(BLLI, FileAttributes.Hidden); // hide dll from user
                        File.SetAttributes(BLLI, FileAttributes.ReadOnly); // dll read-only
                        File.SetAttributes(BLLI, FileAttributes.Temporary); // dll temporary
                    }
                    if (File.Exists(BLLI)) // if dll exist
                        BypassLLI(pid); // load BypassLLI function
                    else
                        throw new FileNotFoundException("Can't load dll, try add:\n" + Path.GetDirectoryName(Application.ExecutablePath) + "\nfolder to antivirus exceptions");
                }).ConfigureAwait(false);
            }
        }

        private async void VACBypassRun()// Bypass Loader code
        {
            string VACBypass = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + "VBL.exe";// VBL.exe location + FileName
            await Task.Run(() =>// run async to avoid  UI lags
            {
                if (File.Exists(VACBypass))// if "VBL.exe" exists
                    File.Delete(VACBypass);// then del "VBL.exe"
                File.WriteAllBytes(VACBypass, Properties.Resources.VACBypassLoader);// Create "VBL.exe" from exe memory
                if (File.Exists(VACBypass))// check if "VBL.exe" exists
                {// if exists, then
                    File.SetAttributes(VACBypass, FileAttributes.Hidden);// hide file from user
                    File.SetAttributes(VACBypass, FileAttributes.ReadOnly);// make it read-only
                    File.SetAttributes(VACBypass, FileAttributes.Temporary);// make file temporary
                    var proc = Process.Start(VACBypass); // Start VBL.exe
                    proc.WaitForExit();// wait for process stop
                    File.Delete(VACBypass);// delete VBL.exe
                }
                else
                    throw new FileNotFoundException("Can't load VACBypass module, try add:\n" + Path.GetDirectoryName(Application.ExecutablePath) + "\nfolder to antivirus exceptions");
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Disable/Enable UI controls
        /// </summary>
        /// <param name="sw">true = off, false = on</param>
        private void SwitchUI(bool sw)// on/off UI controls
        {
            if (sw)// if "sw" true
            {// then off
                InjectButton.Enabled = false;
                ProcessList.Enabled = false;
                OpenFileButton.Enabled = false;
                RefreshButton.Enabled = false;
                DllPathTextBox.Enabled = false;
                InjectMethodCB.Enabled = false;
            }
            else if (!sw)// else "sw" false
            {//then on
                InjectButton.Enabled = true;
                ProcessList.Enabled = true;
                OpenFileButton.Enabled = true;
                RefreshButton.Enabled = true;
                DllPathTextBox.Enabled = true;
                InjectMethodCB.Enabled = true;
            };
        }

        /// <summary>
        /// Check if process x32/x64 bit
        /// </summary>
        /// <param name="process">Process handle</param>
        /// <returns>true = x64 proc, false = x32 proc</returns>
        private static bool IsWin64Emulator(Process process)
        {
            if ((Environment.OSVersion.Version.Major > 5)
                || ((Environment.OSVersion.Version.Major == 5) && (Environment.OSVersion.Version.Minor >= 1)))
                return NativeMethods.IsWow64Process(process.Handle, out bool retVal) && retVal;

            return false; // not on 64-bit Windows Emulator
        }

        internal static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);
        }

        /// <summary>
        /// Get Dll arch
        /// </summary>
        /// <param name="dllPath">Path to DLL</param>
        /// <returns>MachineType</returns>
        private static MachineType GetDllMachineType(string dllPath)
        {
            //see http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
            //offset to PE header is always at 0x3C
            //PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00
            //followed by 2-byte machine type field (see document above for enum)
            FileStream fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            fs.Seek(0x3c, SeekOrigin.Begin);
            Int32 peOffset = br.ReadInt32();
            fs.Seek(peOffset, SeekOrigin.Begin);
            UInt32 peHead = br.ReadUInt32();
            if (peHead != 0x00004550) // "PE\0\0", little-endian
                throw new Exception("Can't find PE header");
            MachineType machineType = (MachineType)br.ReadUInt16();
            br.Close();
            fs.Close();
            return machineType;
        }

        private enum MachineType : ushort
        {
            IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
            IMAGE_FILE_MACHINE_AM33 = 0x1d3,
            IMAGE_FILE_MACHINE_AMD64 = 0x8664,
            IMAGE_FILE_MACHINE_ARM = 0x1c0,
            IMAGE_FILE_MACHINE_EBC = 0xebc,
            IMAGE_FILE_MACHINE_I386 = 0x14c,
            IMAGE_FILE_MACHINE_IA64 = 0x200,
            IMAGE_FILE_MACHINE_M32R = 0x9041,
            IMAGE_FILE_MACHINE_MIPS16 = 0x266,
            IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
            IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
            IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
            IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
            IMAGE_FILE_MACHINE_R4000 = 0x166,
            IMAGE_FILE_MACHINE_SH3 = 0x1a2,
            IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
            IMAGE_FILE_MACHINE_SH4 = 0x1a6,
            IMAGE_FILE_MACHINE_SH5 = 0x1a8,
            IMAGE_FILE_MACHINE_THUMB = 0x1c2,
            IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169,
        }

        /// <summary>
        /// Check dll arch x64/x32/WTF
        /// </summary>
        /// <param name="dllPath">Path to DLL</param>
        /// <returns>x32 = true - x64bit DLL, x32 = false - x32bit DLL, x32 = null - bloken DLL</returns>
        private static bool? UnmanagedDllIs64Bit(string dllPath) // returns true if the dll is 64-bit, false if 32-bit, and null if unknown
        {
            switch (GetDllMachineType(dllPath))
            {
                case MachineType.IMAGE_FILE_MACHINE_AMD64:
                case MachineType.IMAGE_FILE_MACHINE_IA64:
                    return true;
                case MachineType.IMAGE_FILE_MACHINE_I386:
                    return false;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Check if file locked by process
        /// </summary>
        /// <param name="file">FileInfo "filename"</param>
        /// <returns>true = locked, false = unlocked</returns>
        protected virtual bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                    stream.Close();
            }
            catch (Exception ex) when (ex is IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }
        #endregion
    }
}
