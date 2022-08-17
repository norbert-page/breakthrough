using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;

namespace BreakthroughWPF
{
    public class Pawn
    {
        public Pair BoardPosition;
        public Player Owner;

        public Pawn(Player owner, Pair boardPosition)
        {
            Owner = owner;
            BoardPosition = boardPosition;
        }

        public virtual void MoveTo(Pair dest)
        {
            // implement moving and animation
        }
    }
}
