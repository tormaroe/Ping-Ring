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
        private int _counter;
        private int _countInterval;
        
        internal ActorObject(string name, World world)
        {
            _world = world;
            _name = name;
            OnStart = new EventAction<EventState>();
            OnPing = new EventAction<EventState>();
            OnCounter = new EventAction<EventState>();
        }

        public Predicate<int> WhenCounter { get; set; }        
        public EventAction<EventState> OnPing { get; private set; }        
        public EventAction<EventState> OnStart { get; private set; }
        public EventAction<EventState> OnCounter { get; private set; }

        public int Counter
        {
            get
            {
                return _counter;
            }
        }

        public void CountEvery(int countInMilliseconds)
        {
            _countInterval = countInMilliseconds;            
        }

        internal void Ping(EventState p)
        {
            p.Self = this;
            _mailbox.Enqueue(p);
        }

        public void ResetCounter()
        {
            _counter = 0;
        }

        internal void Start()
        {
            new Thread(() => 
            {
                Console.WriteLine(_name + " starting..");
                
                if (OnStart != null)
                    OnStart.Invoke(new EventState { World = _world, Self = this });

                int tick = 0;
                _counter = 0;
                int nextCount = _countInterval;
                while (true)
                {
                    if (_countInterval > 0 && (tick * _world.Frequenzy) > nextCount)
                    {                        
                        _counter++;

                        if (WhenCounter != null && WhenCounter(_counter))
                            OnCounter.Invoke(new EventState { World = _world, Self = this });

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
