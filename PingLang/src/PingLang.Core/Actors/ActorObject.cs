using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PingLang.Core.Actors
{
    public class ActorObject
    {
        private Queue<Ping> _mailbox = new Queue<Ping>();
        private readonly string _name;
        private readonly World _world;

        internal ActorObject(string name, World world)
        {
            _world = world;
            _name = name;            
        }
        
        public Action<Ping> OnPing { get; set; }
        public Action OnStart { get; set; }
        
        internal void Ping(Ping p)
        {
            _mailbox.Enqueue(p);
        }

        internal void Start()
        {
            new Thread(() => 
            {
                Console.WriteLine(_name + " starting..");
                
                if (OnStart != null)
                    OnStart.Invoke();

                while (true)
                {
                    if (OnPing != null && _mailbox.Count > 0)
                        OnPing(_mailbox.Dequeue());

                    Thread.Sleep(_world.Frequenzy);
                }
            }).Start();
        }
    }
}
