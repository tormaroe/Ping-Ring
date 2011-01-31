using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace PingLang.Editor
{
    public class New : EditorCommand
    {
        public override void Execute()
        {
            if (AbortDueToUnsavedChanges)
                return;

            SetPath(string.Empty);
            SetSource(string.Empty);
        }
    }
}
