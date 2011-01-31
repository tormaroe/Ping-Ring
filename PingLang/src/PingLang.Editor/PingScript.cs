using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace PingLang.Editor
{
    public class PingScript
    {        
        public PingScript()
        {
            Path = string.Empty;
            SavedSource = string.Empty;
        }

        public string Path { get; set; }
        public string SavedSource { get; set; }
    }
}