using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace PingLang.Editor
{
    public class EditorController
    {
        #region Misc
        private const string PingFileFilter = "PingLang Script (*.ping)|*.ping";
        private string _savedSource;
        private string _path;
        private readonly EditorForm _view;

        public EditorController(EditorForm view)
        {
            _view = view;
            Path = string.Empty;
            _savedSource = string.Empty;
        }

        private string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                _view.Text = string.IsNullOrEmpty(_path)
                    ? "PingLang: (new file)"
                    : "PingLang: " + System.IO.Path.GetFileName(_path);
            }
        }

        public void SetSource(string value)
        {
            _view.Source = value;
            _savedSource = value;
        }

        private bool UnsavedChanges
        {
            get
            {
                return !_view.Source.Equals(_savedSource);
            }
        }

        private bool MindUnsavedChanges(string question)
        {
            return MessageBox.Show(
                "You have unsaved changes. " + question,
                "Sure?",
                MessageBoxButtons.YesNo) == DialogResult.No;
        }
        #endregion

        #region New
        public void NewRequest()
        {
            if (UnsavedChanges && MindUnsavedChanges("Are you sure you want to discard changes and open a new service?"))
                return; // ABORT!

            Path = string.Empty;
            SetSource(string.Empty);
        }
        #endregion

        #region Open
        public void OpenRequest()
        {
            if (UnsavedChanges && MindUnsavedChanges("Are you sure you want to discard changes and open a new service?"))
                return; // ABORT!

            using (var openFileDialog = new OpenFileDialog
            {
                Filter = PingFileFilter,
                Title = "Open PingLang Script",
            })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Path = openFileDialog.FileName;
                    SetSource(System.IO.File.ReadAllText(openFileDialog.FileName));
                }
            }
        }
        #endregion

        #region Save
        public void SaveRequest()
        {
            if (string.IsNullOrEmpty(Path))
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
                        Path = saveFileDialog.FileName;
                        PerformSave();
                    }
                }
            else
                PerformSave();
        }

        public void PerformSave()
        {
            using (var writer = System.IO.File.CreateText(Path))
                writer.Write(_view.Source);

            _savedSource = _view.Source;
        }
        #endregion

        #region Run
        public void RunRequest()
        {
            SaveRequest();

            if (!string.IsNullOrEmpty(Path))
                Process.Start("pinglang.exe", Path);
            else
                MessageBox.Show(
                    "No script to run", 
                    "Can't do that", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Asterisk);
        }
        #endregion
    }
}
