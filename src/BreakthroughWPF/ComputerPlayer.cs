using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Threading;

namespace BreakthroughWPF
{
    public class ComputerPlayer : Player
    {
        public int ComputerPlayerMinMoveTime = 0;

        public delegate void MoveToDel(Pair pr);
        public delegate void NoArgDel();

        public class ComputerPlayerInvalidChessBoardException : Exception { }

        public new VisualChessboard Gameboard;

        public ComputerPlayer(PiecesColor piecesColor) 
            : base(piecesColor) { }

        public override void SetGameboard(Chessboard ch)
        {
            Gameboard = ch as VisualChessboard;
            if (Gameboard == null) throw new ComputerPlayerInvalidChessBoardException();
        }

        public override Move NextMove()
        {
            Random rand = new Random();
            bool success = false;
            while (!success)
            {
                if (Terminate.Value)
                {
                    Terminated.Value = true;
                    return null;
                }
                if ((Gameboard.HostGame.State.Value & GameState.Paused) > 0) return null; 
                int x = rand.Next(Gameboard.NumberOfFields);
                int y = rand.Next(Gameboard.NumberOfFields);
                if (Gameboard.Pawns[x, y] != null)
                {
                    if (Gameboard.Pawns[x, y].Owner.PiecesColor == PiecesColor)
                    {
                        Gameboard.Pawns[x, y].CalculateAvailableFields();
                        if (Gameboard.Pawns[x, y].NumberOfAvailableFields > 0)
                        {
                            bool found = false;
                            int i = 0;
                            while (!found)
                            {
                                i = rand.Next(3);
                                if (Gameboard.Pawns[x, y].AvailableFields[i] != null)
                                    found = true;
                            }
                            Gameboard.BoardCanvas.Dispatcher.Invoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new NoArgDel(Gameboard.Pawns[x, y].DecoratePawn));
                            Gameboard.BoardCanvas.Dispatcher.Invoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new NoArgDel(Gameboard.Pawns[x, y].ShowAvailableFields));
                            Thread.Sleep(ComputerPlayerMinMoveTime);
                            Gameboard.BoardCanvas.Dispatcher.Invoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new NoArgDel(Gameboard.Pawns[x, y].UndecoratePawn));

                            if ((Gameboard.HostGame.State.Value & GameState.Paused) > 0) return null;
                            if (Gameboard.AnimatedMoves.Value)
                                Gameboard.BoardCanvas.Dispatcher.Invoke(
                                    System.Windows.Threading.DispatcherPriority.Normal,
                                    new MoveToDel(Gameboard.Pawns[x, y].AnimatedMoveTo),
                                    Gameboard.Pawns[x, y].AvailableFields[i]);
                            else 
                                Gameboard.BoardCanvas.Dispatcher.Invoke(
                                    System.Windows.Threading.DispatcherPriority.Normal,
                                    new MoveToDel(Gameboard.Pawns[x, y].MoveTo),
                                    Gameboard.Pawns[x, y].AvailableFields[i]);
                            success = true;
                        }
                    }
                }
            }
            return LastMove;
        }
    }
}
