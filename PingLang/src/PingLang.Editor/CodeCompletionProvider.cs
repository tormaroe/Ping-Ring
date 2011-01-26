using System;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace PingLang.Editor
{
    public abstract class CodeCompletionProvider : ICompletionDataProvider
    {
        private readonly ImageList _imageList;

        public CodeCompletionProvider(ImageList imageList)
        {
            _imageList = imageList;
        }

        #region ICompletionDataProvider Members

        public int DefaultIndex
        {
            get { return -1; }
        }

        public ImageList ImageList
        {
            get { return _imageList; }
        }

        public string PreSelection
        {
            get { return null; }
        }

        public CompletionDataProviderKeyResult ProcessKey(char key)
        {
            if (char.IsLetterOrDigit(key) || key == '_')
                return CompletionDataProviderKeyResult.NormalKey;

            return CompletionDataProviderKeyResult.InsertionKey;
        }

        public bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
        {
            textArea.Caret.Position = textArea.Document.OffsetToPosition(
                Math.Min(insertionOffset, textArea.Document.TextLength));

            return data.InsertAction(textArea, key);
        }


        public abstract ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped);
        #endregion
    }
}
