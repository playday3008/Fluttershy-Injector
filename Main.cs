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
        private int SelectedProcessId = 0;
        private bool x64 = false;
        private bool? x32 = false;
        DialogResult archDll = DialogResult.None;

        #region DllImport

        [DllImport("BLLI.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void BypassLLI(int pid);

        [DllImport("I.dll", CallingConvention = CallingConvention.Cdecl)]
        #pragma warning disable IDE1006 // Стили именования
        #pragma warning disable CA1401 // P/Invokes should not be visible
        public static extern void manualMap(char[] dllName, int PID);
        #pragma warning restore CA1401 // P/Invokes should not be visible
        #pragma warning restore IDE1006 // Стили именования


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

            RefreshButton_Click(null, EventArgs.Empty);
            InjectMethodCB.SelectedIndex = 0;
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            ProcessList.Items.Clear();
            Process[] MyProcess = Process.GetProcesses();
            for (int i = 0; i < MyProcess.Length; i++)
                ProcessList.Items.Add(MyProcess[i].ProcessName + ".exe - " + MyProcess[i].Id);
            int maxWidth = 0, temp;
            foreach (var obj in ProcessList.Items)
            {
                temp = TextRenderer.MeasureText(obj.ToString(), ProcessList.Font).Width;
                if (temp > maxWidth)
                {
                    maxWidth = temp;
                }
            }
            ProcessList.DropDownWidth = Convert.ToInt32(maxWidth * 1.35);
        }

        private void OpenFileButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                openFileDialog.Filter = Properties.Resources.OpenFileFilter;
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    x32 = UnmanagedDllIs64Bit(openFileDialog.FileName);

                    if (x32 == null)
                        archDll = MetroMessageBox.Show(this, Properties.Resources.DllBroken, "Fluttershy-Injector", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, 120);
                    if (archDll == DialogResult.Yes | archDll == DialogResult.None)
                    {
                        DllPathTextBox.Text = openFileDialog.FileName;
                        SelectedDllLabel.Text = Properties.Resources.DllLabel + openFileDialog.SafeFileName + ((bool)x32 ? " (x64)" : " (x32)");
                    }
                }
            }
        }

        private void InjectButton_Click(object sender, EventArgs e)
        {
            if (ProcessList.SelectedItem != null & !string.IsNullOrEmpty(DllPathTextBox.Text) & File.Exists(DllPathTextBox.Text))
            {
                SwitchUI(true);
                if (InjectMethodCB.SelectedIndex == 0)
                    ManualMapCSx32(SelectedProcessId, DllPathTextBox.Text);
                else if (InjectMethodCB.SelectedIndex == 1)
                    Inflame(SelectedProcessId, DllPathTextBox.Text);
                else if (InjectMethodCB.SelectedIndex == 2)
                {
                    BypassLLIRun(SelectedProcessId);
                    DllInjectorMarcin(SelectedProcessId, DllPathTextBox.Text, InjectionMethod.CREATE_REMOTE_THREAD);
                }
                else if (InjectMethodCB.SelectedIndex == 3)
                {
                    BypassLLIRun(SelectedProcessId);
                    XenforoInj(SelectedProcessId, DllPathTextBox.Text);
                }
                SwitchUI(false);
            }
            else if (string.IsNullOrEmpty(DllPathTextBox.Text) | !File.Exists(DllPathTextBox.Text))
                MetroMessageBox.Show(this, Properties.Resources.IncDllPath, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (ProcessList.SelectedItem == null)
                MetroMessageBox.Show(this, Properties.Resources.SelProcFirst, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
        }

        private void VACBypassLabel_Click(object sender, EventArgs e)
        {
            DialogResult vac = MetroMessageBox.Show(this, Properties.Resources.VACquestion, "Fluttershy-Injector", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, 120);
            if (vac == DialogResult.Yes)
            {
                SwitchUI(true);
                VACBypassRun();
                SwitchUI(false);
            }
        }

        private void DllPathTextBox_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(DllPathTextBox.Text))
            {
                if (Path.GetExtension(DllPathTextBox.Text) == ".dll")
                {
                    x32 = UnmanagedDllIs64Bit(DllPathTextBox.Text);

                    if (x32 != null)
                        SelectedDllLabel.Text = Properties.Resources.DllLabel + Path.GetFileName(DllPathTextBox.Text) + ((bool)x32 ? " (x64)" : " (x32)");
                }
                else
                    SelectedDllLabel.Text = Properties.Resources.DllLabel;
            }
            else
                SelectedDllLabel.Text = Properties.Resources.DllLabel;
        }

        private void ProcessList_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedProcessId = int.Parse(ProcessList.SelectedItem.ToString().Substring(ProcessList.SelectedItem.ToString().IndexOf('-') + 2));
            if (Process.GetProcesses().Any(x => x.Id == SelectedProcessId))
            {
                try
                {
                    if (IsWin64Emulator(Process.GetProcessById(SelectedProcessId)))
                        x64 = false;
                    else
                        x64 = true;
                }
                catch (Win32Exception ex)
                {
                    if (ex.NativeErrorCode != 0x00000005)
                    {
                        throw;
                    }
                }
                SelectedProcLabel.Text = Properties.Resources.ProcessLabel + Process.GetProcessById(SelectedProcessId).ProcessName + ".exe" + (x64 ? " (x64)" : " (x32)");
                SelectedPidLabel.Text = Properties.Resources.PidLabel + SelectedProcessId;
            }
            else
                MetroMessageBox.Show(this, Properties.Resources.RefreshList, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
        }

        private void CerditsButton_Click(object sender, EventArgs e)
        {
            var creditsForm = new Credits();
            if (!Application.OpenForms.OfType<Credits>().Any())
                creditsForm.Show();
        }

        #region Injection Methods

        private void ManualMapCSx32(int pid, string dllPath)
        {
            if (!x64 & x32 == false)
            {
                var injector = new ManualMapInjector(Process.GetProcessById(pid)) { AsyncInjection = true };
                var injected = $"hmodule = 0x{injector.Inject(dllPath).ToInt64():x8}";
                MetroMessageBox.Show(this, Properties.Resources.InjResult + Environment.NewLine + injected, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Information, 160);
            }
            else if (x64)
                MetroMessageBox.Show(this, Properties.Resources.OnlyX32Proc, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (x32 == true)
                MetroMessageBox.Show(this, Properties.Resources.OnlyX32Dll, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
        }

        private async void Inflame(int pid, string dllPath)
        {
            if (!x64 & x32 == false)
            {
                bool locked = false;
                await Task.Run(() =>
                {
                    string I = "I.dll";
                    if (IsFileLocked(new FileInfo(I)))
                        locked = true;
                    if (!locked)
                        File.Delete(I);
                    if (!File.Exists(I))
                    {
                        File.WriteAllBytes(I, Properties.Resources.Inflame);
                        File.SetAttributes(I, FileAttributes.Hidden);
                        File.SetAttributes(I, FileAttributes.ReadOnly);
                        File.SetAttributes(I, FileAttributes.Temporary);
                    }
                    if (File.Exists(I))
                        manualMap(dllPath.ToCharArray(), pid);
                    else
                        throw new FileNotFoundException("Can't load dll, try add:\n" + Path.GetDirectoryName(Application.ExecutablePath) + "\nfolder to antivirus exceptions");
                });
            }
            else if (x64)
                MetroMessageBox.Show(this, Properties.Resources.OnlyX32Proc, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (x32 == true)
                MetroMessageBox.Show(this, Properties.Resources.OnlyX32Dll, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);

        }

        private async void DllInjectorMarcin(int pid, string dllPath, InjectionMethod injectionMethod)
        {
            try
            {
                var injector = new DLLInjector(injectionMethod);
                await Task.Run(() => injector.Inject(pid, dllPath));
            }
            #pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                MetroMessageBox.Show(this, ex.Message, ex.GetType().Name, MessageBoxButtons.OK, MessageBoxIcon.Error, 150);
            }
            #pragma warning restore CA1031 // Do not catch general exception types
        }

        private async void XenforoInj(int pid, String dllPath)
        {
            Int32 ProcID = pid;
            if (ProcID >= 0)
            {
                IntPtr hProcess = OpenProcess(0x1F0FFF, 1, ProcID);
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
                    });

            }
        }

        #endregion

        #region Functions
        private async void BypassLLIRun(int pid)
        {
            if (Path.GetFileName(Process.GetProcessById(pid).MainModule.FileName) == "csgo.exe")
            {
                bool locked = false;
                await Task.Run(() =>
                {
                    var BLLI = "BLLI.dll";
                    if (IsFileLocked(new FileInfo(BLLI)))
                        locked = true;
                    if (!locked)
                        File.Delete(BLLI);
                    if (!File.Exists(BLLI))
                    {
                        File.WriteAllBytes(BLLI, Properties.Resources.BypassLLI);
                        File.SetAttributes(BLLI, FileAttributes.Hidden);
                        File.SetAttributes(BLLI, FileAttributes.ReadOnly);
                        File.SetAttributes(BLLI, FileAttributes.Temporary);
                    }
                    if (File.Exists(BLLI))
                        BypassLLI(pid);
                    else
                        throw new FileNotFoundException("Can't load dll, try add:\n" + Path.GetDirectoryName(Application.ExecutablePath) + "\nfolder to antivirus exceptions");
                }).ConfigureAwait(false);
            }
        }

        private async void VACBypassRun()// Bypass Loader code
        {
            string VACBypass = Path.GetDirectoryName(Application.ExecutablePath) + "\\" + Properties.Resources.VBLfile;
            await Task.Run(() =>
            {
                if (File.Exists(VACBypass))
                    File.Delete(VACBypass);
                File.WriteAllBytes(VACBypass, Properties.Resources.VACBypassLoader);
                if (File.Exists(VACBypass))
                {
                    File.SetAttributes(VACBypass, FileAttributes.Hidden);
                    File.SetAttributes(VACBypass, FileAttributes.ReadOnly);
                    File.SetAttributes(VACBypass, FileAttributes.Temporary);
                    var proc = Process.Start(VACBypass);
                    proc.WaitForExit();
                    File.Delete(VACBypass);
                }
                else
                    throw new FileNotFoundException("Can't load VACBypass module, try add:\n" + Path.GetDirectoryName(Application.ExecutablePath) + "\nfolder to antivirus exceptions");
            }).ConfigureAwait(false);
        }

        private void SwitchUI(bool sw)
        {
            if (sw)
            {
                InjectButton.Enabled = false;
                ProcessList.Enabled = false;
                OpenFileButton.Enabled = false;
                RefreshButton.Enabled = false;
                DllPathTextBox.Enabled = false;
                InjectMethodCB.Enabled = false;
            }
            else if (!sw)
            {
                InjectButton.Enabled = true;
                ProcessList.Enabled = true;
                OpenFileButton.Enabled = true;
                RefreshButton.Enabled = true;
                DllPathTextBox.Enabled = true;
                InjectMethodCB.Enabled = true;
            }
        }

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
                throw new Exception(Properties.Resources.GetDllArchErr);
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
