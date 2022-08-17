using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace BreakthroughWPF
{
    class RemotePlayerEventDriven : ComputerAIPlayer
    {
        public Window1 host;
        private SMove moveGlobal;
        private EventWaitHandle wh = new EventWaitHandle(false, EventResetMode.AutoReset);

        public RemotePlayerEventDriven(PiecesColor piecesColor, Window1 host)
            : base(piecesColor) 
        {
            this.host = host;
            host.GameConnectionServiceInstance.NewMoveReceived += delegate(Object sender, NewMoveEventArgs args)
            {
                ComputerAIPlayer.SMove newMove = args.MoveDescription;
                moveGlobal = new SMove(newMove.sx, newMove.sy, newMove.ex, newMove.ey, newMove.beaten, PiecesColor.White);
                wh.Set();
            };
        }

        public override SMove GenerateNextMove(PiecesColor player)
        {
            Gameboard.HostGame.SafeStatusSet2("Awaiting \"" + player + "\" player move...");
            // set up board
            for (int y = 0; y < 8; ++y)
                for (int x = 0; x < 8; ++x)
                    Board[x, y] = (Gameboard.Pawns[x, y] == null ? PiecesColor.None : Gameboard.Pawns[x, y].Owner.PiecesColor);

            wh.WaitOne();
            bestMove = moveGlobal;
            Gameboard.HostGame.SafeStatusSet2("done");
            return bestMove;
        }
    }
}
