using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace PingLang.Editor
{
    public class Save : EditorCommand
    {
        public override void Execute()
        {
            if (string.IsNullOrEmpty(Script.Path))
                using (var saveFileDialog = new SaveFileDialog
                {
                    AddExtension = true,
                    CreatePrompt = false,
                    DefaultExt = "ping",
                    Filter = PingFileFilter,
                    Title = "Save PingLang Service",
                })
                {
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        SetPath(saveFileDialog.FileName);
                        PerformSave();
                    }
                }
            else
                PerformSave();
        }

        private void PerformSave()
        {
            using (var writer = System.IO.File.CreateText(Script.Path))
                writer.Write(View.Source);

            Script.SavedSource = View.Source;
        }
    }
}
