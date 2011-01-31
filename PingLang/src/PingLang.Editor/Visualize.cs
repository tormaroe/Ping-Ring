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
            var tempFile = GetTempFileWithExtension("dot");
            File.WriteAllText(tempFile, dotSource);
            return tempFile;
        }

        private string GenerateImage(string dotFile)
        {
            var tempFile = GetTempFileWithExtension("png");
            using (var imageGeneratorProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "dot",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                Arguments = string.Format("-Tpng \"{0}\"", dotFile),
            }))
            using (var reader = imageGeneratorProcess.StandardOutput)
                File.WriteAllText(tempFile, reader.ReadToEnd(), reader.CurrentEncoding);
            return tempFile;
        }

        private string GetTempFileWithExtension(string ext)
        {
            return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "." + ext);            
        }
    }
}
