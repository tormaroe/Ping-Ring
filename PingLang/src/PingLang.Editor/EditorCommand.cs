using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace PingLang.Editor
{
    public abstract class EditorCommand
    {
        public const string PingFileFilter = "PingLang Script (*.ping)|*.ping";
        public PingScript Script;
        public EditorForm View;
        public abstract void Execute();

        public bool AbortDueToUnsavedChanges
        {
            get
            {
                return UnsavedChanges && MindUnsavedChanges("Are you sure you want to discard changes and open a new service?");
            }
        }

        private bool UnsavedChanges
        {
            get
            {
                return !View.Source.Equals(Script.SavedSource);
            }
        }

        private bool MindUnsavedChanges(string question)
        {
            return MessageBox.Show(
                "You have unsaved changes. " + question,
                "Sure?",
                MessageBoxButtons.YesNo) == DialogResult.No;
        }

        public void SetSource(string value)
        {
            View.Source = value;
            Script.SavedSource = value;
        }

        public void SetPath(string value)
        {
            Script.Path = value;
            View.Path = value;
        }
    }


}
