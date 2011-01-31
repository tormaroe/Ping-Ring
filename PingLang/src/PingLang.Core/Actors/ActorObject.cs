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
        private int _countInterval;
        
        internal ActorObject(string name, World world)
        {
            _world = world;
            _name = name;
            OnStart = new EventAction<EventState>();
            OnPing = new EventAction<EventState>();
        }
        
        public EventAction<EventState> OnPing { get; private set; }
        
        public EventAction<EventState> OnStart { get; private set; }

        public Predicate<int> WhenCounter { get; set; }
        public EventAction<EventState> OnCounter { get; private set; }

        public void CountEvery(int countInMilliseconds)
        {
            _countInterval = countInMilliseconds;            
        }

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

                int tick = 0;
                int counter = 0;
                int nextCount = _countInterval;
                while (true)
                {
                    if (_countInterval > 0 && (tick * _world.Frequenzy) > nextCount)
                    {
                        counter++;
                        
                        if (WhenCounter(counter))
                            OnCounter.Invoke(new EventState { World = _world });

                        nextCount += _countInterval;
                    }

                    if (_mailbox.Count > 0)
                    {
                        OnPing.Invoke(_mailbox.Dequeue());
                    }
                    
                    tick++;
                    Thread.Sleep(_world.Frequenzy);
                }
            }).Start();
        }
    }
}
