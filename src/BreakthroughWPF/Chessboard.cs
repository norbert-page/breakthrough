using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace BreakthroughWPF
{
    public class Chessboard
    {
        public Pawn[,] Pawns;
        public Game HostGame;
        public int NumberOfFields = 8;
        protected Chessboard() { }

        public Chessboard(Player playerWhite, Player playerBlack)
        {
            Pawns = new Pawn[NumberOfFields, NumberOfFields];
            for (int y = NumberOfFields - 2; y < NumberOfFields; ++y)
            {
                for (int x = 0; x < NumberOfFields; ++x)
                {
                    Pawns[x, y] = new Pawn(playerWhite, new Pair(x, y));
                    Pawns[x, NumberOfFields - y - 1] = 
                        new Pawn(playerBlack, new Pair(x, NumberOfFields - y - 1));
                }
            }
        }

        public virtual void MoveUpdate(Move move)
        {

        }
    }
}
