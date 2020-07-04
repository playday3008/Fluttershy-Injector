using ManualMapInjection.Injection;
using MetroFramework;
using MetroFramework.Forms;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        private class Inflame32
        {
            [DllImport("I.dll", CallingConvention = CallingConvention.Cdecl)]
            #pragma warning disable IDE1006 // Стили именования
            public static extern void manualMap(char[] dllName, int PID);
        }

        private class Inflame64
        {
            [DllImport("I64.dll", CallingConvention = CallingConvention.Cdecl)]
            public static extern void manualMap(char[] dllName, int PID);
            #pragma warning restore IDE1006 // Стили именования
        }
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
                SwitchUI(false);
                RefreshButton_Click(null, EventArgs.Empty);
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
                MetroMessageBox.Show(this, Properties.Resources.InjResult + Environment.NewLine + $"hmodule = 0x{injector.Inject(dllPath).ToInt64():x8}", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Information, 150);
            }
            else if (x64)
                MetroMessageBox.Show(this, Properties.Resources.OnlyX32Proc, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (x32 == true)
                MetroMessageBox.Show(this, Properties.Resources.OnlyX32Dll, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
        }

        private async void Inflame(int pid, string dllPath)
        {
            string dll = Path.GetRandomFileName().Replace(".", "") + ".dll";
            File.Copy(dllPath, dll, true);
            File.SetAttributes(dll, FileAttributes.Hidden);
            if (x64 & x32 == true)
            {
                if (!File.Exists("I64.dll"))
                    File.WriteAllBytes("I64.dll", Properties.Resources.Inflame64);
                if (File.Exists("I64.dll"))
                {
                    File.SetAttributes("I64.dll", FileAttributes.Hidden);
                    await Task.Run(() => Inflame64.manualMap(dll.ToCharArray(), pid));
                }
                else
                    throw new FileNotFoundException("Can't load dll, try add:\n" + Path.GetDirectoryName(Application.ExecutablePath) + "\nfolder to antivirus exceptions");
            }
            else if (!x64 & x32 == false)
            {
                if (!File.Exists("I.dll"))
                    File.WriteAllBytes("I.dll", Properties.Resources.Inflame);
                if (File.Exists("I.dll"))
                {
                    File.SetAttributes("I.dll", FileAttributes.Hidden);
                    await Task.Run(() => Inflame32.manualMap(dll.ToCharArray(), pid));
                }
                else
                    throw new FileNotFoundException("Can't load dll, try add:\n" + Path.GetDirectoryName(Application.ExecutablePath) + "\nfolder to antivirus exceptions");
            }
            else if (x64 & x32 == false)
                MetroMessageBox.Show(this, Properties.Resources.OnlyX32Proc, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (!x64 & x32 == true)
                MetroMessageBox.Show(this, Properties.Resources.OnlyX32Dll, "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);

        }

        #endregion

        #region Functions

        private async void BypassLLIRun(int pid)
        {
            await Task.Run(() =>
            {
                if (File.Exists("BLLI.dll"))
                    File.Delete("BLLI.dll");
                File.WriteAllBytes("BLLI.dll", Properties.Resources.BypassLLI);
                if (File.Exists("BLLI.dll"))
                {
                    File.SetAttributes("BLLI.dll", FileAttributes.Hidden);
                    BypassLLI(pid);
                }
            }).ConfigureAwait(false);
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
        #endregion
    }
}
