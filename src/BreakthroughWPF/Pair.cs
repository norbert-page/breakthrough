using System;
using System.Collections.Generic;
using System.Text;

namespace BreakthroughWPF
{
    public class Pair
    {
        public int X;
        public int Y;

        public Pair(Pair p)
        {
            X = p.X;
            Y = p.Y;
        }

        public Pair(int x, int y)
        {
            X = x; Y = y;
        }
    }
}
