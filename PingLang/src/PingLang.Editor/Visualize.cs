using System.Windows.Forms;
using System.Diagnostics;

namespace PingLang.Editor
{
    public class Visualize : Save
    {
        public override void Execute()
        {
            base.Execute(); // Save before run..

            if (string.IsNullOrEmpty(Script.Path))
            {
                MessageBox.Show(
                    "No script to visualize",
                    "Can't do that",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
                return;
            }
            // parse, get AST
            // analyse AST to produce dot file
            // save dot file in temp folder

            // run dot tool on dot file, produce png
            // execute png to open it
        }
    }
}
