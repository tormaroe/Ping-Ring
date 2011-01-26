using System;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace PingLang.Editor
{
    public partial class EditorForm : Form
    {
        private EditorController _controller;
        private TextEditorControl _editorControl;
        private CodeCompletionWindow _codeCompletionWindow;
                
        public EditorForm()
        {
            InitializeEditor();
            InitializeComponent();            
            _controller = new EditorController(this);
        }

        private void InitializeEditor()
        {
            HighlightingManager.Manager.AddSyntaxModeFileProvider(new FileSyntaxModeProvider(@"syntax"));
            _editorControl = new TextEditorControl { Dock = DockStyle.Fill };
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

        private void NewButton_Click(object sender, EventArgs e)
        {
            _controller.NewRequest();
        }
        private void OpenButton_Click(object sender, EventArgs e)
        {
            _controller.OpenRequest();
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            _controller.SaveRequest();
        }
        private void RunButton_Click(object sender, EventArgs e)
        {
            _controller.RunRequest();
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.N: _controller.NewRequest(); break;
                    case Keys.O: _controller.OpenRequest(); break;
                    case Keys.S: _controller.SaveRequest(); break;
                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                _controller.RunRequest();
            }
        }
    }
}
