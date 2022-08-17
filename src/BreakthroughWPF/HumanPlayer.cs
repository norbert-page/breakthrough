using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Input;
using System.Windows;

namespace BreakthroughWPF
{
    public class HumanPlayer :Player
    {
        public event EventHandler MovePerformed;

        public class HumanPlayerInvalidChessBoardException : Exception { }

        public new VisualChessboard Gameboard;

        public HumanPlayer(PiecesColor piecesColor) 
            : base(piecesColor) { }

        public override void SetGameboard(Chessboard ch)
        {
            Gameboard = ch as VisualChessboard;
            if (Gameboard == null) throw new HumanPlayerInvalidChessBoardException();
        }

        public override Move NextMove()
        {
            //MessageBox.Show(Gameboard.HostGame.Moves.Value.PiecesColor.ToString());
            MoveMade = false;
            while (!MoveMade && !((GameState.Paused & Gameboard.HostGame.State.Value) > 0)) 
            {
                if (Terminate.Value)
                {
                    Terminated.Value = true;
                    return null;
                }
                Thread.Sleep(200);
                if (Gameboard.HostGame.Forced != null && Gameboard.HostGame.Forced != this)
                {
                    Gameboard.HostGame.Forced = null;
                    return null;
                }
            }
            if (Terminate.Value)
            {
                Terminated.Value = true;
                return null;
            }
            MoveMade = false;
            EventHandler eh = MovePerformed;
            if (eh != null) eh(this, null);
            //waitForConfirmation.WaitOne();
            if ((GameState.Paused & Gameboard.HostGame.State.Value) > 0) return null;
            else return LastMove;
        }
    }
}
