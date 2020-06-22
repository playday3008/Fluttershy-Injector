﻿using ManualMapInjection.Injection;
using MetroFramework;
using MetroFramework.Forms;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Injector
{
    public partial class Main : MetroForm
    {
        #region Functions

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

        private int SelectedProcessId = 0;
        bool x64 = false;
        bool? x32 = false;
        DialogResult archDll = DialogResult.None;

        public Main()
        {
            InitializeComponent();

            RefreshButton_Click(null, EventArgs.Empty);
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
                SelectedProcLabel.Text = "Process: " + Process.GetProcessById(SelectedProcessId).ProcessName + ".exe" + (x64 ? " (x64)" : " (x32)");
                SelectedPidLabel.Text = "PID: " + SelectedProcessId;
            }
            else
                MetroMessageBox.Show(this, "Please refresh list", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
        }

        private void OpenFileButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = Path.GetDirectoryName(Application.ExecutablePath);
                openFileDialog.Filter = "DLL files (*.dll)|*.dll";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    x32 = UnmanagedDllIs64Bit(openFileDialog.FileName);

                    if (x32 == null)
                        archDll = MetroMessageBox.Show(this, "Your dll is broken, or someting  else, do you want to continue?", "Fluttershy-Injector", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, 120);
                    if (archDll == DialogResult.Yes | archDll == DialogResult.None)
                    {
                        DllPathTextBox.Text = openFileDialog.FileName;
                        SelectedDllLabel.Text = "DLL: " + openFileDialog.SafeFileName + ((bool)x32 ? " (x64)" : " (x32)");
                    }
                }
            }
        }

        private void InjectButton_Click(object sender, EventArgs e)
        {
            if (!x64 & x32 == false & ProcessList.SelectedItem != null & !string.IsNullOrEmpty(DllPathTextBox.Text) & File.Exists(DllPathTextBox.Text))
            {
                var injector = new ManualMapInjector(Process.GetProcessById(SelectedProcessId)) { AsyncInjection = true };
                MetroMessageBox.Show(this, "Inject result:" + Environment.NewLine + $"hmodule = 0x{injector.Inject(DllPathTextBox.Text).ToInt64():x8}", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Information, 150);
            }
            else if (ProcessList.SelectedItem == null)
                MetroMessageBox.Show(this, "SELCET PROCESS FIRST", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (x64)
                MetroMessageBox.Show(this, "ONLY x32 PROCESSES", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (string.IsNullOrEmpty(DllPathTextBox.Text) | !File.Exists(DllPathTextBox.Text))
                MetroMessageBox.Show(this, "INCORRECT DLL PATH", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
            else if (x32 == true)
                MetroMessageBox.Show(this, "ONLY x32 DLL'S", "Fluttershy-Injector", MessageBoxButtons.OK, MessageBoxIcon.Error, 120);
        }

        private void DllPathTextBox_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(DllPathTextBox.Text))
            {
                if (Path.GetExtension(DllPathTextBox.Text) == ".dll")
                {
                    x32 = UnmanagedDllIs64Bit(DllPathTextBox.Text);

                    if (x32 != null)
                        SelectedDllLabel.Text = "DLL: " + Path.GetFileName(DllPathTextBox.Text) + ((bool)x32 ? " (x64)" : " (x32)");
                }
                else
                    SelectedDllLabel.Text = "DLL: ";
            }
            else
                SelectedDllLabel.Text = "DLL: ";
        }
    }
}