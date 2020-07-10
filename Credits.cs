using MetroFramework.Forms;
using System;
using System.Diagnostics;

namespace Injector
{
    public partial class Credits : MetroForm
    {
        public Credits()
        {
            InitializeComponent();
        }

        private void MetroLink1_Click(object sender, EventArgs e)
        {
            Process.Start(Properties.Resources.GitHubLink);
        }

        private void MetroLink2_Click(object sender, EventArgs e)
        {
            Process.Start(Properties.Resources.MySiteLink);
        }

        private void MetroLink3_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/danielkrupinski");
        }

        private void MetroLink4_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/ThaisenPM");
        }

        private void MetroLink5_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/marcin-chwedczuk");
        }
    }
}
