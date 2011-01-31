using System.Windows.Forms;
using System.Diagnostics;
using PingLang.Core.Parsing;
using PingLang.Core.Lexing;
using System.IO;
using System;

namespace PingLang.Editor
{
    public class Visualize : Save
    {
        public override void Execute()
        {
            base.Execute(); // Save before run..

            if (string.IsNullOrEmpty(Script.Path))
            {
                MessageBox.Show(
                    "No script to visualize",
                    "Can't do that",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
                return;
            }

            try
            {
                var tree = ParseScript();
                var dotSource = new DotFileCreator().ToDot(tree);
                var dotFile = SaveDotFileInTempFolder(dotSource);
                var imageFile = GenerateImage(dotFile);
                Process.Start(imageFile);

                // delete png and dot file when done?
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private AST ParseScript()
        {
            var parser = new Parser(new Lexer(Tokens.All));
            parser.ConstructTree(File.ReadAllText(Script.Path));
            return parser.AST;
        }

        private string SaveDotFileInTempFolder(string dotSource)
        {
            return DoWithTempFile("dot", temp => File.WriteAllText(temp, dotSource));
        }

        private string GenerateImage(string dotFile)
        {
            return DoWithTempFile("png", temp =>
            {
                using (var imageGeneratorProcess = Process.Start(new ProcessStartInfo
                {
                    FileName = "dot",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    Arguments = string.Format("-Tpng \"{0}\"", dotFile),
                }))
                using (var reader = imageGeneratorProcess.StandardOutput)
                    File.WriteAllText(temp, reader.ReadToEnd(), reader.CurrentEncoding);
            });
        }

        private string DoWithTempFile(string ext, Action<string> block)
        {
            var fileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "." + ext);
            block.Invoke(fileName);
            return fileName;
        }
    }
}
