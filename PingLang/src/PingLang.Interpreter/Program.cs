using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingLang.Core.Actors;
using PingLang.Core.Parsing;
using PingLang.Core.Lexing;
using System.IO;

namespace PingLang.Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("pinglang interpreter");
            Console.WriteLine("--------------------");
            Console.WriteLine();

            try
            {
                var path = string.Join(" ", args);
                Console.WriteLine("* Reading source file " + Path.GetFileName(path));
                Console.WriteLine("  from folder " + Path.GetDirectoryName(path));
                var source = File.ReadAllText(path);

                Console.WriteLine();
                Console.WriteLine("* Parsing..");
                var parser = new Parser(new Lexer(Tokens.All));
                parser.ConstructTree(source);

                Console.WriteLine("AST:" + TreeFormatter.ToString(parser.AST));

                Console.WriteLine();
                Console.WriteLine("* Building actors..");
                var world = new World(100);
                var builder = new WorldBuilder(world);
                builder.Build(parser.AST);

                Console.WriteLine();
                Console.WriteLine("* Starting actors, press Ctrl+C to exit!");
                Console.WriteLine();

                world.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Press ENTER to exit!");
                Console.ReadLine();
            }
        }
    }
}
