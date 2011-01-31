using System;

namespace PingLang.Editor
{
    public class IntGenerator
    {
        private int i;

        public int Next()
        {
            return ++i;
        }
    }
}
