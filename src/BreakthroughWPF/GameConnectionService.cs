using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Threading;
using BreakthroughWPF.GameConnection;

namespace BreakthroughWPF
{

    public class NewMoveEventArgs : EventArgs
    {
        private ComputerAIPlayer.SMove moveDescription;
        public NewMoveEventArgs(ComputerAIPlayer.SMove move)
        {
            moveDescription = move;
        }

        public ComputerAIPlayer.SMove MoveDescription
        {
            get { return moveDescription; }
            set { moveDescription = value; }
        }
    }

    // NOTE: If you change the class name "GameConnectionService" here, you must also update the reference to "GameConnectionService" in App.config.
    public class GameConnectionService : IGameConnectionService
    {
        private static Object lockObject = new Object();

        public event EventHandler<NewMoveEventArgs> NewMoveReceived;
        public event EventHandler InvitationAccepted;
        public event EventHandler GameFinished;
        public event EventHandler ConnectionLost;

        public static Window1 host;

        private static ReaderWriterCustomLock<bool> isGameOngoing = new ReaderWriterCustomLock<bool>(false);
        public static bool IsGameOngoing
        {
            get { return isGameOngoing.Value; }
            set { isGameOngoing.Value = value; }
        }

        public string InviteWelcome(string nickName, string endpoint)
        {
            host.GameConnectionServiceInstance = this;//TODO
            return "";
        }

        public bool IsAlive()
        {
            return true;
        }

        public bool Invite(string nickName, string endpoint)
        {
            MessageBoxResult mbr = MessageBox.Show(String.Format("Player \"{0}\" would like to play with you.\nDo you accept this invitation?", nickName),
                "Invitation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.Yes);

            if (mbr == MessageBoxResult.Yes)
            {
                bool proceed = false;

                // avoid race condition
                lock (lockObject)
                {
                    if (!IsGameOngoing)
                    {
                        IsGameOngoing = true;
                        proceed = true;
                    }
                }
                if (!proceed) return false;

                host.GameConnectionServiceInstance = this;

                NetTcpBinding binding = new NetTcpBinding();
                binding.PortSharingEnabled = true;

                try { host.GameConnectionClient = new GameConnectionServiceClient(binding, new EndpointAddress(endpoint)); }
                catch (CommunicationException ce)
                {
                    host.GameConnectionClient.Abort();
                    MessageBox.Show(ce.Message, "Communication error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    throw;
                }

                host.GameConnectionClient.Open();
                host.GameConnectionClient.InviteWelcome(host.memberID, host.endpointAddress.Value);

                // connection with the client was successfull and the invitation was accepted
                host.pause.IsEnabled = true;
                host.resume.IsEnabled = false;
                host.undo.IsEnabled = false;
                host.redo.IsEnabled = false;
                host.Game.State.Value |= GameState.Paused;
                host.movesExpanderPlayerWhite.Items.Clear();
                host.movesExpanderPlayerBlack.Items.Clear();
                host.BoardCanvas.Children.Clear();
                host.Game.PlayerBlack.Terminate.Value = true;
                host.Game.PlayerWhite.Terminate.Value = true;
                host.Game = new Game(host.BoardCanvas, host, false, false, true);
                if (host.Game != null) host.Log("New network game started.");
                else host.Log("New network game cannot be started.");
                if (host.choosePlayerWindow != null) host.choosePlayerWindow.Close();

                EventHandler handler = InvitationAccepted;
                if (handler != null) handler(this, null);
                return true;
            }
            else return false;
        }

        public ComputerAIPlayer.SMove NextMove(ComputerAIPlayer.SMove opponentMove)
        {
            if (host.GameConnectionServiceInstance == null) host.GameConnectionServiceInstance = this;

            EventHandler<NewMoveEventArgs> handle = NewMoveReceived;
            if (handle != null) handle(this, new NewMoveEventArgs(opponentMove));
            // white moves TO DO

            //(host.Game.PlayerBlack as HumanPlayer).moveWait.WaitOne();

            /*while (!host.Game.PlayerBlack.MoveMade) Thread.Sleep(200);*/
            //Thread.Sleep(4000);
            /*opponentMove.sx = host.Game.PlayerBlack.LastMove.Source.X;
            opponentMove.sy = host.Game.PlayerBlack.LastMove.Source.Y;
            opponentMove.ex = host.Game.PlayerBlack.LastMove.Destination.X;
            opponentMove.ey = host.Game.PlayerBlack.LastMove.Destination.Y;*/

            opponentMove.sy = 7 - opponentMove.sy;
            opponentMove.ey = 7 - opponentMove.ey;

            return opponentMove;
        }
    }
}
