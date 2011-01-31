using System;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using System.Drawing;

namespace PingLang.Editor
{
    public partial class EditorForm : Form
    {
        private PingScript script;
        private TextEditorControl _editorControl;
        private CodeCompletionWindow _codeCompletionWindow;
                
        public EditorForm()
        {
            InitializeEditor();
            InitializeComponent();            
            script = new PingScript();
        }

        private void InitializeEditor()
        {
            HighlightingManager.Manager.AddSyntaxModeFileProvider(new FileSyntaxModeProvider(@"syntax"));
            _editorControl = new TextEditorControl 
            { 
                Dock = DockStyle.Fill, 
                Font = new Font("Consolas", 10.0f), 
                TabIndent = 2 
            };
            _editorControl.SetHighlighting("PingLang");
            _editorControl.ActiveTextAreaControl.TextArea.KeyDown += (sender, e) =>
            {
                if (e.Control == false)
                    return;
                if (e.KeyCode != Keys.Space)
                    return;
                e.SuppressKeyPress = true;
                ShowIntellisense((char)e.KeyValue);
            };
            Controls.Add(_editorControl);            
        }

        private void ShowIntellisense(char value)
        {
            ICompletionDataProvider completeionDataProvider = new PingLangCodeCompletionProvider(intellisenseImages);
            _codeCompletionWindow = CodeCompletionWindow.ShowCompletionWindow(this, _editorControl, "", completeionDataProvider, value);
        }


        public string Path
        {
            set
            {
                Text = string.IsNullOrEmpty(value)
                    ? "PingLang: (new file)"
                    : "PingLang: " + System.IO.Path.GetFileName(value);
            }
        }

        public string Source
        {
            get
            {
                return _editorControl.Text;
            }
            set
            {
                _editorControl.Text = value;
                _editorControl.Refresh();
            }
        }

        private void Execute<COMMAND>() where COMMAND : EditorCommand, new()
        {            
            new COMMAND() { View = this, Script = script }.Execute();
        }

        private void NewButton_Click(object sender, EventArgs e) { Execute<New>(); }
        private void OpenButton_Click(object sender, EventArgs e) { Execute<Open>(); }
        private void SaveButton_Click(object sender, EventArgs e) { Execute<Save>(); }
        private void RunButton_Click(object sender, EventArgs e) { Execute<Run>(); }
        private void VisualizeButton_Click(object sender, EventArgs e) { Execute<Visualize>(); }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.N: Execute<New>(); break;
                    case Keys.O: Execute<Open>(); break;
                    case Keys.S: Execute<Save>(); break;
                    case Keys.R: Execute<Run>(); break;
                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                Execute<Run>();
            }
        }

        
    }
}
