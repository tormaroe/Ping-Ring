using System;
using System.Windows.Forms;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace PingLang.Editor
{
    public class PingLangCodeCompletionProvider : CodeCompletionProvider
    {
        public PingLangCodeCompletionProvider(ImageList imageList)
            : base(imageList)
        {

        }

        public override ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
        {
            return new ICompletionData[] 
            {
                new DefaultCompletionData("when message", "", 6),
                new DefaultCompletionData("when pinged", "", 2),
                new DefaultCompletionData("when counter", "", 4),
                new DefaultCompletionData("when starting", "", 11),
                new DefaultCompletionData("listen on port", "", 10),
                new DefaultCompletionData("print", "", 9),
                new DefaultCompletionData("ping", "", 2),
                new DefaultCompletionData("send", "", 7),
                new DefaultCompletionData("wait", "", 3),
                new DefaultCompletionData("when error", "", 5),
                new DefaultCompletionData("count every", "", 8),
                new DefaultCompletionData("reset counter", "", 1),
            };
        }
    }
}
