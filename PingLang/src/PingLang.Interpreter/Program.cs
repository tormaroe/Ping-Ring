using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PingLang.Core.Actors;

namespace PingLang.Interpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press Ctrl+C to exit!");
            
            var world = new World(100);

            var a1 = world.CreateActor("A1");
            a1.OnStart = () => world.Ping("A2");
            a1.OnPing = ping => world.Ping("A2");

            var a2 = world.CreateActor("A2");
            a2.OnPing = ping =>
            {
                Console.WriteLine("A2 was pinged");
                world.Ping("A1");
            };

            world.Start();

            

        }
    }
}
