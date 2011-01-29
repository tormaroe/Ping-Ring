using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PingLang.Core.Actors
{
    public class ActorObject
    {
        private Queue<EventState> _mailbox = new Queue<EventState>();
        private readonly string _name;
        private readonly World _world;

        internal ActorObject(string name, World world)
        {
            _world = world;
            _name = name;
            OnStart = new EventAction<EventState>();
            OnPing = new EventAction<EventState>();
        }
        
        public EventAction<EventState> OnPing { get; private set; }
        public EventAction<EventState> OnStart { get; private set; }
        
        internal void Ping(EventState p)
        {
            _mailbox.Enqueue(p);
        }

        internal void Start()
        {
            new Thread(() => 
            {
                Console.WriteLine(_name + " starting..");
                
                if (OnStart != null)
                    OnStart.Invoke(new EventState { World = _world });

                while (true)
                {
                    if (_mailbox.Count > 0)
                    {
                        OnPing.Invoke(_mailbox.Dequeue());
                    }
                    Thread.Sleep(_world.Frequenzy);
                }
            }).Start();
        }
    }
}
