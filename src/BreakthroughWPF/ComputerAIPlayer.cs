using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Threading;

namespace BreakthroughWPF
{
    public class ComputerAIPlayer : Player
    {
        public int ComputerPlayerMinMoveTime = 0;

        // copied board, white = 1, black -1, none 0
        protected PiecesColor[,] Board = new PiecesColor[8, 8];

        private Random randgen = new Random();

        private int[,] paths = new int[8, 8];
        
        private int maxDepth = 4;
        protected SMove bestMove;

        public delegate void MoveToDel(Pair pr);
        public delegate void NoArgDel();

        public class ComputerPlayerInvalidChessBoardException : Exception { }

        public new VisualChessboard Gameboard;

        public ComputerAIPlayer(PiecesColor piecesColor) 
            : base(piecesColor) { }

        public override void SetGameboard(Chessboard ch)
        {
            Gameboard = ch as VisualChessboard;
            if (Gameboard == null) throw new ComputerPlayerInvalidChessBoardException();
        }

        public double WhiteHeuristicEvaluation()
        {
            double sum = 0;

            // take the number of figures into consideration
            for(int y = 0; y < 8; ++y)
                for (int x = 0; x < 8; ++x)
                {
                    if (Board[x, y] == PiecesColor.White)
                    {
                        sum += 1.0;
                        if (y < 3) sum += 1.0 / 3.0;
                    }
                    if (Board[x, y] == PiecesColor.Black)
                    {
                        sum -= 1.0;
                        if (y >= 5) sum -= 1.0 / 3.0;
                    }
                }


            /*foreach (PiecesColor color in Board)
            {
                if (color == PiecesColor.White) sum += 1.0 / 16.0;
                else if (color == PiecesColor.Black) sum -= 1.0 / 16.0;
            }*/

            for (int x = 0; x < 8; ++x)
            {
                if (Board[x, 0] == PiecesColor.White) sum += 100.0;
                if (Board[x, 7] == PiecesColor.Black) sum += -100.0;
            }

            for (int x = 0; x < 8; ++x)
                for (int y = 0; y < 8; ++y) paths[x, y] = 1;

            for (int y = 1; y < 8; ++y)
            {
                for (int x = 0; x < 8; ++x)
                {
                    if (Board[x, y] != PiecesColor.None) { paths[x, y] = 0; continue; }
                    if (x == 0)
                    {
                        if (Board[x + 1, y - 1] == PiecesColor.Black)
                            { paths[x, y] = 0; continue; }
                        paths[x, y] = Math.Max(paths[x, y - 1], paths[x + 1, y - 1]);
                    }
                    else if (x == 7)
                    {
                        if (Board[x - 1, y - 1] == PiecesColor.Black) { paths[x, y] = 0; continue; }
                        paths[x, y] = Math.Max(paths[x, y - 1], paths[x - 1, y - 1]);
                    }
                    else
                    {
                        if (Board[x - 1, y - 1] == PiecesColor.Black || Board[x + 1, y - 1] == PiecesColor.Black)
                        { paths[x, y] = 0; continue; }

                        paths[x, y] = Math.Max(paths[x - 1, y - 1],
                            Math.Max(paths[x, y - 1], paths[x + 1, y - 1]));
                    }

                    if (paths[x, y] == 1 && Board[x, y] == PiecesColor.White)
                    {
                        sum += 10.0 / (y*y*y);
                    }
                }
            }


            for (int x = 0; x < 8; ++x)
                for (int y = 0; y < 8; ++y) paths[x, y] = 1;

            for (int y = 6; y >= 0; --y)
            {
                for (int x = 0; x < 8; ++x)
                {
                    if (Board[x, y] != PiecesColor.None) { paths[x, y] = 0; continue; }
                    if (x == 0)
                    {
                        if (Board[x + 1, y + 1] == PiecesColor.White)
                        { paths[x, y] = 0; continue; }
                        paths[x, y] = Math.Max(paths[x, y + 1], paths[x + 1, y + 1]);
                    }
                    else if (x == 7)
                    {
                        if (Board[x - 1, y + 1] == PiecesColor.White) { paths[x, y] = 0; continue; }
                        paths[x, y] = Math.Max(paths[x, y + 1], paths[x - 1, y + 1]);
                    }
                    else
                    {
                        if (Board[x - 1, y + 1] == PiecesColor.White || Board[x + 1, y + 1] == PiecesColor.White)
                        { paths[x, y] = 0; continue; }

                        paths[x, y] = Math.Max(paths[x - 1, y + 1],
                            Math.Max(paths[x, y + 1], paths[x + 1, y + 1]));
                    }

                    if (paths[x, y] == 1 && Board[x, y] == PiecesColor.Black)
                    {
                        sum -= 10.0 / ((7 - y)*(7 - y)*(7 - y));
                    }
                }
            }

            return sum + (randgen.NextDouble()/10000.0 - 1.0/20000.0);
        }

        [Serializable()]
        public struct SMove
        {
            public int sx, sy, ex, ey;
            public bool beaten;
            public PiecesColor player;

            public SMove(int xs, int ys, int xe, int ye, bool beatenn, PiecesColor playerr)
            {
                sx = xs;
                sy = ys;
                ex = xe;
                ey = ye;
                beaten = beatenn;
                player = playerr;
            }
        }

        private void PerformMove(SMove move)
        {
            Board[move.sx, move.sy] = PiecesColor.None;
            Board[move.ex, move.ey] = move.player;
        }

        private void UnperformMove(SMove move)
        {
            Board[move.sx, move.sy] = move.player;
            if (move.beaten) Board[move.ex, move.ey] = Gameboard.HostGame.AnotherColor(move.player);
            else Board[move.ex, move.ey] = PiecesColor.None;
        }

        public double minmax(int depth, PiecesColor whoMoves)
        {
            if (Terminate.Value)
            {
                Terminated.Value = true;
                return 0.0;
            }
            SMove bestMove = new SMove();
            double bestMoveScore;
            if (whoMoves == PiecesColor.White) bestMoveScore = double.MinValue;
            else bestMoveScore = double.MaxValue;

            if (depth == maxDepth) return WhiteHeuristicEvaluation();
            else
            {
                List<SMove> moves = GeneratePossibleMoves(whoMoves);
                foreach (SMove move in moves)
                {
                    PerformMove(move);
                    double moveScore = minmax(depth + 1, Gameboard.HostGame.AnotherColor(whoMoves));
                    if ((whoMoves == PiecesColor.White && moveScore > bestMoveScore)
                        || (whoMoves == PiecesColor.Black && moveScore < bestMoveScore))
                    {
                        bestMove = move;
                        bestMoveScore = moveScore;
                    }
                    UnperformMove(move);
                }
            }

            this.bestMove = bestMove;
            return bestMoveScore;
        }

        private List<SMove> GeneratePossibleMoves(PiecesColor color)
        {
            List<SMove> Moves = new List<SMove>();
            
            int offset_y;
            if (color == PiecesColor.White) offset_y = -1; else offset_y = 1;

            for (int y = 0; y < 8; ++y)
            {
                if (color == PiecesColor.White && y == 0) continue;
                else if (color == PiecesColor.Black && y == 7) continue;

                for (int x = 0; x < 8; ++x)
                {
                    if (Board[x, y] != color) continue;

                    if (x != 0)
                    {
                        if (Board[x - 1, y + offset_y] == PiecesColor.None)
                            Moves.Add(new SMove(x, y, x - 1, y + offset_y, false, color));
                        else if (Board[x - 1, y + offset_y] != color)
                            Moves.Add(new SMove(x, y, x - 1, y + offset_y, true, color));
                    }

                    if (x != 7)
                    {
                        if (Board[x + 1, y + offset_y] == PiecesColor.None)
                            Moves.Add(new SMove(x, y, x + 1, y + offset_y, false, color));
                        else if (Board[x + 1, y + offset_y] != color)
                            Moves.Add(new SMove(x, y, x + 1, y + offset_y, true, color));
                    }

                    if (Board[x, y + offset_y] == PiecesColor.None)
                        Moves.Add(new SMove(x, y, x, y + offset_y, false, color));
                }
            }
            return Moves;
        }

        public override Move NextMove()
        {
            GenerateNextMove(PiecesColor);

            //MessageBox.Show(bestMove.sx + ", " + bestMove.sy + " > " + bestMove.ex + ", " + bestMove.ey);
            if (Terminate.Value)
            {
                Terminated.Value = true;
                return null;
            }

            Gameboard.BoardCanvas.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new NoArgDel(Gameboard.Pawns[bestMove.sx, bestMove.sy].DecoratePawn));
            Gameboard.BoardCanvas.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new NoArgDel(Gameboard.Pawns[bestMove.sx, bestMove.sy].ShowAvailableFields));
            Thread.Sleep(ComputerPlayerMinMoveTime);
            Gameboard.BoardCanvas.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new NoArgDel(Gameboard.Pawns[bestMove.sx, bestMove.sy].UndecoratePawn));

            if ((Gameboard.HostGame.State.Value & GameState.Paused) > 0) return null;
            if (Gameboard.AnimatedMoves.Value)
                Gameboard.BoardCanvas.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new MoveToDel(Gameboard.Pawns[bestMove.sx, bestMove.sy].AnimatedMoveTo),
                    new Pair(bestMove.ex, bestMove.ey));
            else
                Gameboard.BoardCanvas.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new MoveToDel(Gameboard.Pawns[bestMove.sx, bestMove.sy].MoveTo),
                    new Pair(bestMove.ex, bestMove.ey));

            return LastMove;
        }

        public virtual SMove GenerateNextMove(PiecesColor player)
        {
            Gameboard.HostGame.SafeStatusSet2("Calculating position for \"" + player + "\"...");
            // set up board
            for (int y = 0; y < 8; ++y)
                for (int x = 0; x < 8; ++x)
                    Board[x, y] = (Gameboard.Pawns[x, y] == null ? PiecesColor.None : Gameboard.Pawns[x, y].Owner.PiecesColor);

            //MessageBox.Show(GeneratePossibleMoves(PiecesColor).Count.ToString());

            minmax(0, player);
            Gameboard.HostGame.SafeStatusSet2("done");
            return bestMove;
        }
    }
}
