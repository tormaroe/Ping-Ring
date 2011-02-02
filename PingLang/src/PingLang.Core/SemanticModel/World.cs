using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using PingLang.Core.Parsing;

namespace PingLang.Core.Actors
{
    public class World
    {
        private Dictionary<string, ActorObject> _actors = new Dictionary<string, ActorObject>();
        internal readonly int Frequenzy;

        public World(int frequenzy)
        {
            Frequenzy = frequenzy;
        }

        public ActorObject CreateActor(string name)
        {
            var actorObject = new ActorObject(name, this);
            _actors.Add(name, actorObject);
            return actorObject;
        }

        public void Start()
        {
            foreach (var actor in _actors.Values)
                actor.Start();
        }

        public void Ping(string actorName)
        {
            _actors[actorName].Ping(new EventState { World = this });
        }
    }
}
