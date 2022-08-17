using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace BreakthroughWPF
{
    public class Move
    {
        public class NumberOfMovesNotSupported : ApplicationException { }

        public Pair Destination;
        public Pair Source;
        public VisualPawn Pawn;
        public VisualPawn TakenPawn;
        public ListBoxItem ListBoxItemHandle;
        public Player Player;
        public int NumberOfFields;
        public int NumberOfMove;

        public Move() { }
        public Move(Move move)
        {
            Destination = new Pair(move.Destination);
            Source = new Pair(move.Source);
            Pawn = move.Pawn;
            TakenPawn = move.TakenPawn;
            NumberOfMove = move.NumberOfMove;
            NumberOfFields = move.NumberOfFields;
            Player = move.Player;
            ListBoxItemHandle = move.ListBoxItemHandle;
        }

        public static String ChessPosField(int numberOfFields, Pair p)
        {
            Move tmp = new Move();
            tmp.NumberOfFields = numberOfFields;
            return tmp.ChessPos(p);
        }

        private String ChessPos(Pair pair)
        {
            char[] chars = new char[] 
                {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
                 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
                 'y', 'z'}; // 26

            if (pair.X >= chars.Length) throw new NumberOfMovesNotSupported();

            int n = NumberOfFields;
            return (chars[pair.X].ToString() + (n - pair.Y).ToString());
            
        }

        public override string ToString()
        {
            return ChessPos(Source) + "-" + ChessPos(Destination) + 
                ((TakenPawn != null)?" !":"") ;
            //return null;
        }
    }
}
