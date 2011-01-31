using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace PingLang.Editor
{
    public class Open : EditorCommand
    {
        public override void Execute()
        {
            if (AbortDueToUnsavedChanges) return;

            using (var openFileDialog = new OpenFileDialog
            {
                Filter = PingFileFilter,
                Title = "Open PingLang Script",
            })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SetPath(openFileDialog.FileName);
                    SetSource(System.IO.File.ReadAllText(openFileDialog.FileName));
                }
            }
        }
    }
}
