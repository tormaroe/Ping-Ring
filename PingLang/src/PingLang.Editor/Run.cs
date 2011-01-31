using System.Windows.Forms;
using System.Diagnostics;

namespace PingLang.Editor
{
    public class Run : Save
    {
        public override void Execute()
        {
            base.Execute(); // Save before run..

            if (!string.IsNullOrEmpty(Script.Path))
                Process.Start("pinglang.exe", Script.Path);
            else
                MessageBox.Show(
                    "No script to run",
                    "Can't do that",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
        }
    }
}
