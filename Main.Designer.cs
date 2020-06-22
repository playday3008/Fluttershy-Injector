namespace Injector
{
    partial class Main
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.ProcessList = new MetroFramework.Controls.MetroComboBox();
            this.Label1 = new MetroFramework.Controls.MetroLabel();
            this.RefreshButton = new MetroFramework.Controls.MetroButton();
            this.SelectedProcLabel = new MetroFramework.Controls.MetroLabel();
            this.SelectedPidLabel = new MetroFramework.Controls.MetroLabel();
            this.Label3 = new MetroFramework.Controls.MetroLabel();
            this.metroTile1 = new MetroFramework.Controls.MetroTile();
            this.Label2 = new MetroFramework.Controls.MetroLabel();
            this.DllPathTextBox = new MetroFramework.Controls.MetroTextBox();
            this.OpenFileButton = new MetroFramework.Controls.MetroButton();
            this.SelectedDllLabel = new MetroFramework.Controls.MetroLabel();
            this.InjectButton = new MetroFramework.Controls.MetroButton();
            this.VACBypassLabel = new MetroFramework.Controls.MetroLabel();
            this.metroProgressSpinner1 = new MetroFramework.Controls.MetroProgressSpinner();
            this.GitHubLink = new MetroFramework.Controls.MetroLink();
            this.MySiteLink = new MetroFramework.Controls.MetroLink();
            this.toolTip = new MetroFramework.Components.MetroToolTip();
            this.SuspendLayout();
            // 
            // ProcessList
            // 
            this.ProcessList.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ProcessList.FormattingEnabled = true;
            this.ProcessList.ItemHeight = 23;
            this.ProcessList.Location = new System.Drawing.Point(23, 82);
            this.ProcessList.Name = "ProcessList";
            this.ProcessList.Size = new System.Drawing.Size(354, 29);
            this.ProcessList.Sorted = true;
            this.ProcessList.Style = MetroFramework.MetroColorStyle.Purple;
            this.ProcessList.TabIndex = 0;
            this.ProcessList.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.ProcessList.UseSelectable = true;
            this.ProcessList.UseStyleColors = true;
            this.ProcessList.SelectedIndexChanged += new System.EventHandler(this.ProcessList_SelectedIndexChanged);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.Label1.Location = new System.Drawing.Point(19, 60);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(108, 19);
            this.Label1.Style = MetroFramework.MetroColorStyle.Purple;
            this.Label1.TabIndex = 1;
            this.Label1.Text = "Select process:";
            this.Label1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Label1.UseStyleColors = true;
            // 
            // RefreshButton
            // 
            this.RefreshButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.RefreshButton.Location = new System.Drawing.Point(302, 53);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(75, 23);
            this.RefreshButton.Style = MetroFramework.MetroColorStyle.Purple;
            this.RefreshButton.TabIndex = 2;
            this.RefreshButton.Text = "Refresh list";
            this.RefreshButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.RefreshButton.UseSelectable = true;
            this.RefreshButton.UseStyleColors = true;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // SelectedProcLabel
            // 
            this.SelectedProcLabel.AutoSize = true;
            this.SelectedProcLabel.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.SelectedProcLabel.Location = new System.Drawing.Point(19, 114);
            this.SelectedProcLabel.MaximumSize = new System.Drawing.Size(357, 0);
            this.SelectedProcLabel.Name = "SelectedProcLabel";
            this.SelectedProcLabel.Size = new System.Drawing.Size(68, 19);
            this.SelectedProcLabel.Style = MetroFramework.MetroColorStyle.Purple;
            this.SelectedProcLabel.TabIndex = 3;
            this.SelectedProcLabel.Text = "Process: ";
            this.SelectedProcLabel.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.SelectedProcLabel.UseStyleColors = true;
            // 
            // SelectedPidLabel
            // 
            this.SelectedPidLabel.AutoSize = true;
            this.SelectedPidLabel.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.SelectedPidLabel.Location = new System.Drawing.Point(19, 133);
            this.SelectedPidLabel.MaximumSize = new System.Drawing.Size(357, 0);
            this.SelectedPidLabel.Name = "SelectedPidLabel";
            this.SelectedPidLabel.Size = new System.Drawing.Size(40, 19);
            this.SelectedPidLabel.Style = MetroFramework.MetroColorStyle.Purple;
            this.SelectedPidLabel.TabIndex = 4;
            this.SelectedPidLabel.Text = "PID: ";
            this.SelectedPidLabel.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.SelectedPidLabel.UseStyleColors = true;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.Label3.Location = new System.Drawing.Point(301, 26);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(76, 19);
            this.Label3.Style = MetroFramework.MetroColorStyle.Purple;
            this.Label3.TabIndex = 5;
            this.Label3.Text = "(x32 only)";
            this.Label3.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Label3.UseStyleColors = true;
            // 
            // metroTile1
            // 
            this.metroTile1.ActiveControl = null;
            this.metroTile1.Location = new System.Drawing.Point(23, 155);
            this.metroTile1.Name = "metroTile1";
            this.metroTile1.Size = new System.Drawing.Size(354, 1);
            this.metroTile1.Style = MetroFramework.MetroColorStyle.Silver;
            this.metroTile1.TabIndex = 6;
            this.metroTile1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroTile1.UseSelectable = true;
            this.metroTile1.UseStyleColors = true;
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.Label2.Location = new System.Drawing.Point(19, 159);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(85, 19);
            this.Label2.Style = MetroFramework.MetroColorStyle.Purple;
            this.Label2.TabIndex = 7;
            this.Label2.Text = "Select DLL: ";
            this.Label2.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.Label2.UseStyleColors = true;
            // 
            // DllPathTextBox
            // 
            // 
            // 
            // 
            this.DllPathTextBox.CustomButton.Image = null;
            this.DllPathTextBox.CustomButton.Location = new System.Drawing.Point(251, 1);
            this.DllPathTextBox.CustomButton.Name = "";
            this.DllPathTextBox.CustomButton.Size = new System.Drawing.Size(21, 21);
            this.DllPathTextBox.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
            this.DllPathTextBox.CustomButton.TabIndex = 1;
            this.DllPathTextBox.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
            this.DllPathTextBox.CustomButton.UseSelectable = true;
            this.DllPathTextBox.CustomButton.Visible = false;
            this.DllPathTextBox.Lines = new string[0];
            this.DllPathTextBox.Location = new System.Drawing.Point(23, 181);
            this.DllPathTextBox.MaxLength = 32767;
            this.DllPathTextBox.Name = "DllPathTextBox";
            this.DllPathTextBox.PasswordChar = '\0';
            this.DllPathTextBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.DllPathTextBox.SelectedText = "";
            this.DllPathTextBox.SelectionLength = 0;
            this.DllPathTextBox.SelectionStart = 0;
            this.DllPathTextBox.ShortcutsEnabled = true;
            this.DllPathTextBox.Size = new System.Drawing.Size(273, 23);
            this.DllPathTextBox.Style = MetroFramework.MetroColorStyle.Purple;
            this.DllPathTextBox.TabIndex = 8;
            this.DllPathTextBox.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.DllPathTextBox.UseSelectable = true;
            this.DllPathTextBox.UseStyleColors = true;
            this.DllPathTextBox.WaterMark = "path to dll ex. \"C:\\lol\\fuck.dll\" (without quotes)";
            this.DllPathTextBox.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
            this.DllPathTextBox.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
            this.DllPathTextBox.TextChanged += new System.EventHandler(this.DllPathTextBox_TextChanged);
            // 
            // OpenFileButton
            // 
            this.OpenFileButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.OpenFileButton.Location = new System.Drawing.Point(302, 181);
            this.OpenFileButton.Name = "OpenFileButton";
            this.OpenFileButton.Size = new System.Drawing.Size(75, 23);
            this.OpenFileButton.Style = MetroFramework.MetroColorStyle.Purple;
            this.OpenFileButton.TabIndex = 9;
            this.OpenFileButton.Text = "Open file";
            this.OpenFileButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.OpenFileButton.UseSelectable = true;
            this.OpenFileButton.UseStyleColors = true;
            this.OpenFileButton.Click += new System.EventHandler(this.OpenFileButton_Click);
            // 
            // SelectedDllLabel
            // 
            this.SelectedDllLabel.AutoSize = true;
            this.SelectedDllLabel.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.SelectedDllLabel.Location = new System.Drawing.Point(19, 207);
            this.SelectedDllLabel.MaximumSize = new System.Drawing.Size(357, 0);
            this.SelectedDllLabel.Name = "SelectedDllLabel";
            this.SelectedDllLabel.Size = new System.Drawing.Size(41, 19);
            this.SelectedDllLabel.Style = MetroFramework.MetroColorStyle.Purple;
            this.SelectedDllLabel.TabIndex = 10;
            this.SelectedDllLabel.Text = "DLL: ";
            this.SelectedDllLabel.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.SelectedDllLabel.UseStyleColors = true;
            // 
            // InjectButton
            // 
            this.InjectButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.InjectButton.FontSize = MetroFramework.MetroButtonSize.Medium;
            this.InjectButton.Location = new System.Drawing.Point(296, 229);
            this.InjectButton.Name = "InjectButton";
            this.InjectButton.Size = new System.Drawing.Size(81, 32);
            this.InjectButton.Style = MetroFramework.MetroColorStyle.Purple;
            this.InjectButton.TabIndex = 11;
            this.InjectButton.Text = "Inject";
            this.InjectButton.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.InjectButton.UseSelectable = true;
            this.InjectButton.UseStyleColors = true;
            this.InjectButton.Click += new System.EventHandler(this.InjectButton_Click);
            // 
            // VACBypassLabel
            // 
            this.VACBypassLabel.AutoSize = true;
            this.VACBypassLabel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.VACBypassLabel.FontWeight = MetroFramework.MetroLabelWeight.Bold;
            this.VACBypassLabel.Location = new System.Drawing.Point(196, 235);
            this.VACBypassLabel.Name = "VACBypassLabel";
            this.VACBypassLabel.Size = new System.Drawing.Size(94, 19);
            this.VACBypassLabel.Style = MetroFramework.MetroColorStyle.Purple;
            this.VACBypassLabel.TabIndex = 12;
            this.VACBypassLabel.Text = "VAC-Bypass?";
            this.VACBypassLabel.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.VACBypassLabel.UseStyleColors = true;
            this.VACBypassLabel.Click += new System.EventHandler(this.VACBypassLabel_Click);
            // 
            // metroProgressSpinner1
            // 
            this.metroProgressSpinner1.Enabled = false;
            this.metroProgressSpinner1.Location = new System.Drawing.Point(158, 229);
            this.metroProgressSpinner1.Maximum = 100;
            this.metroProgressSpinner1.Name = "metroProgressSpinner1";
            this.metroProgressSpinner1.Size = new System.Drawing.Size(32, 32);
            this.metroProgressSpinner1.Style = MetroFramework.MetroColorStyle.Purple;
            this.metroProgressSpinner1.TabIndex = 13;
            this.metroProgressSpinner1.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.metroProgressSpinner1.UseSelectable = true;
            this.metroProgressSpinner1.UseStyleColors = true;
            this.metroProgressSpinner1.Visible = false;
            // 
            // GitHubLink
            // 
            this.GitHubLink.Cursor = System.Windows.Forms.Cursors.Hand;
            this.GitHubLink.Location = new System.Drawing.Point(23, 238);
            this.GitHubLink.Name = "GitHubLink";
            this.GitHubLink.Size = new System.Drawing.Size(71, 23);
            this.GitHubLink.Style = MetroFramework.MetroColorStyle.Purple;
            this.GitHubLink.TabIndex = 14;
            this.GitHubLink.Text = "My GitHub";
            this.GitHubLink.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.GitHubLink.UseSelectable = true;
            this.GitHubLink.UseStyleColors = true;
            this.GitHubLink.Click += new System.EventHandler(this.GitHubLink_Click);
            this.GitHubLink.MouseHover += new System.EventHandler(this.GitHubLink_MouseHover);
            // 
            // MySiteLink
            // 
            this.MySiteLink.AutoSize = true;
            this.MySiteLink.Cursor = System.Windows.Forms.Cursors.Hand;
            this.MySiteLink.Location = new System.Drawing.Point(100, 238);
            this.MySiteLink.Name = "MySiteLink";
            this.MySiteLink.Size = new System.Drawing.Size(52, 23);
            this.MySiteLink.Style = MetroFramework.MetroColorStyle.Purple;
            this.MySiteLink.TabIndex = 15;
            this.MySiteLink.Text = "My Site";
            this.MySiteLink.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.MySiteLink.UseSelectable = true;
            this.MySiteLink.UseStyleColors = true;
            this.MySiteLink.Click += new System.EventHandler(this.MySiteLink_Click);
            this.MySiteLink.MouseHover += new System.EventHandler(this.MySiteLink_MouseHover);
            // 
            // toolTip
            // 
            this.toolTip.Style = MetroFramework.MetroColorStyle.Blue;
            this.toolTip.StyleManager = null;
            this.toolTip.Theme = MetroFramework.MetroThemeStyle.Light;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 284);
            this.Controls.Add(this.MySiteLink);
            this.Controls.Add(this.GitHubLink);
            this.Controls.Add(this.metroProgressSpinner1);
            this.Controls.Add(this.VACBypassLabel);
            this.Controls.Add(this.InjectButton);
            this.Controls.Add(this.SelectedDllLabel);
            this.Controls.Add(this.OpenFileButton);
            this.Controls.Add(this.DllPathTextBox);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.metroTile1);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.SelectedPidLabel);
            this.Controls.Add(this.SelectedProcLabel);
            this.Controls.Add(this.RefreshButton);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.ProcessList);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Main";
            this.Resizable = false;
            this.Style = MetroFramework.MetroColorStyle.Purple;
            this.Text = "💜Fluttershy-Injector💜";
            this.Theme = MetroFramework.MetroThemeStyle.Dark;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MetroFramework.Controls.MetroComboBox ProcessList;
        private MetroFramework.Controls.MetroLabel Label1;
        private MetroFramework.Controls.MetroButton RefreshButton;
        private MetroFramework.Controls.MetroLabel SelectedProcLabel;
        private MetroFramework.Controls.MetroLabel SelectedPidLabel;
        private MetroFramework.Controls.MetroLabel Label3;
        private MetroFramework.Controls.MetroTile metroTile1;
        private MetroFramework.Controls.MetroLabel Label2;
        private MetroFramework.Controls.MetroTextBox DllPathTextBox;
        private MetroFramework.Controls.MetroButton OpenFileButton;
        private MetroFramework.Controls.MetroLabel SelectedDllLabel;
        private MetroFramework.Controls.MetroButton InjectButton;
        private MetroFramework.Controls.MetroLabel VACBypassLabel;
        private MetroFramework.Controls.MetroProgressSpinner metroProgressSpinner1;
        private MetroFramework.Controls.MetroLink GitHubLink;
        private MetroFramework.Controls.MetroLink MySiteLink;
        private MetroFramework.Components.MetroToolTip toolTip;
    }
}

