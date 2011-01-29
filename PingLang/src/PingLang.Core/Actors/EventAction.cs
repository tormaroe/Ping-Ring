using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PingLang.Core.Actors
{
    public class EventAction<T>
    {
        private List<Action<T>> _block = new List<Action<T>>();

        public void Invoke(T arg)
        {
            _block.ForEach(a => a.Invoke(arg));
        }

        public void Add(Action<T> a)
        {
            _block.Add(a);
        }
    }
}
