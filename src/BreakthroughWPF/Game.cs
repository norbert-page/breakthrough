using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Threading;
using System.Windows;
using System.ServiceModel;

namespace BreakthroughWPF
{
    public class Game
    {
        public Object ButtonLocker = new Object(); // for thread-locking purposes

        public Window1 HostWindow;
        public VisualChessboard Gameboard;

        public Player PlayerWhite;
        public Player PlayerBlack;
        public Player Forced = null;
        public GameStory GameStory;
        public int DelayBetweenMovesMS = 300;

        public ReaderWriterCustomLock<GameState> State = new ReaderWriterCustomLock<GameState>();
        public ReaderWriterCustomLock<Player> Moves = new ReaderWriterCustomLock<Player>();

        public ReaderWriterCustomLock<bool> IsTerminatedGame = new ReaderWriterCustomLock<bool>(false);
        public ReaderWriterCustomLock<bool> IsNetworkGame = new ReaderWriterCustomLock<bool>(false);
        Timer pinger;

        public delegate void MovesDelegate();
        public delegate void ChangeStatBar(String str);

        public Game(Canvas c, Window1 hostWindow, bool WhiteHuman, bool BlackHuman, bool networkGame)
        {
            HostWindow = hostWindow;
            State.Value = GameState.Started;

            IsNetworkGame.Value = networkGame;

            HostWindow.resume.IsEnabled = false;
            HostWindow.undo.IsEnabled = false;
            GameStory = new GameStory(this, 
                                      HostWindow.movesExpanderPlayerWhite,
                                      HostWindow.movesExpanderPlayerBlack);

            if (!networkGame)
            {
                if (WhiteHuman) PlayerWhite = new HumanPlayer(PiecesColor.White);
                else PlayerWhite = new ComputerAIPlayer(PiecesColor.White);

                if (BlackHuman) PlayerBlack = new HumanPlayer(PiecesColor.Black);
                else PlayerBlack = new ComputerAIPlayer(PiecesColor.Black);

                HostWindow.expanderModify.IsEnabled = true;
                HostWindow.gameControlExpander.IsEnabled = true;
            }
            else
            {
                if (WhiteHuman)
                {
                    // Inviter
                    PlayerWhite = new HumanPlayer(PiecesColor.White);
                    PlayerBlack = new RemotePlayerEventDriven(PiecesColor.Black, hostWindow);

                    (PlayerWhite as HumanPlayer).MovePerformed += delegate(Object sender, EventArgs ea)
                    {
                        GameConnection.ComputerAIPlayerSMove csmove = new GameConnection.ComputerAIPlayerSMove();
                        csmove.sx = PlayerWhite.LastMove.Source.X;
                        csmove.sy = PlayerWhite.LastMove.Source.Y;
                        csmove.ex = PlayerWhite.LastMove.Destination.X;
                        csmove.ey = PlayerWhite.LastMove.Destination.Y;
                        try { HostWindow.GameConnectionClient.NextMove(csmove); }
                        catch (CommunicationException) { }
                    };
                }
                else
                {
                    // Invited
                    PlayerWhite = new RemotePlayerEventDriven(PiecesColor.White, hostWindow);
                    PlayerBlack = new HumanPlayer(PiecesColor.Black);

                    (PlayerBlack as HumanPlayer).MovePerformed += delegate(Object sender, EventArgs ea)
                    {
                        GameConnection.ComputerAIPlayerSMove csmove = new GameConnection.ComputerAIPlayerSMove();
                        csmove.sx = PlayerBlack.LastMove.Source.X;
                        csmove.sy = PlayerBlack.LastMove.Source.Y;
                        csmove.ex = PlayerBlack.LastMove.Destination.X;
                        csmove.ey = PlayerBlack.LastMove.Destination.Y;
                        HostWindow.GameConnectionClient.NextMove(csmove);
                    };
                }

                HostWindow.expanderModify.IsEnabled = false;
                HostWindow.gameControlExpander.IsEnabled = false;

                pinger = new Timer(new TimerCallback(isRemoteAlive));
                pinger.Change(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 4));
            }
            
            Moves.Value = PlayerWhite;
            Gameboard = new VisualChessboard(this, PlayerWhite, PlayerBlack, c);

            PlayerWhite.SetGameboard(Gameboard);
            PlayerBlack.SetGameboard(Gameboard);

            StartProcessingMoves();

            HostWindow.undo.IsEnabled = false;
        }

        private delegate void ChangeLog(string newText);
        public void isRemoteAlive(Object state)
        {
            Timer thisTimer = (state as Timer);
            try { HostWindow.GameConnectionClient.IsAlive(); }
            catch (CommunicationException)
            {
                if ((State.Value & GameState.Finished) == GameState.Finished)
                {
                    thisTimer.Dispose();
                    return;
                }
                MessageBox.Show("Remote player does not respond.", "Connection problem.", MessageBoxButton.OK, MessageBoxImage.Warning);
                HostWindow.Dispatcher.BeginInvoke(new ChangeLog(HostWindow.Log), "Connection lost.");
                State.Value |= GameState.Finished;
                State.Value |= GameState.Paused;
                thisTimer.Dispose();
            }
 
            return;
        }

        public void StartProcessingMoves()
        {
            State.Value &= ~GameState.Finished;
            MovesDelegate mover = new MovesDelegate(ProcessMoves);
            mover.BeginInvoke(null, null);
        }


        public void ModifyStatus(String str)
        {
            HostWindow.StatBar1.Content = str;
        }

        public void ModifyStatus2(String str)
        {
            HostWindow.StatBar2.Content = str;
        }

        public void SafeStatusSet(String str)
        {
            HostWindow.StatBar1.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new ChangeStatBar(ModifyStatus), str);
        }


        public void SafeStatusSet2(String str)
        {
            HostWindow.StatBar2.Dispatcher.BeginInvoke(
                System.Windows.Threading.DispatcherPriority.Send,
                new ChangeStatBar(ModifyStatus2), str);
        }

        public bool IsPaused()
        {
            return ((State.Value & GameState.Paused) > 0);
        }

        public bool IsBoardBeingModified()
        {
            return ((State.Value & GameState.BeingModified) > 0);
        }

        public void WaitIfPaused()
        {
             while (true)
            {
                if ((State.Value & GameState.Paused) > 0)
                {
                    if (Moves.Value.Terminate.Value == true) return;
                    Thread.Sleep(300);
                }
                else break;
            }
        }

        public Move PlayerMove(Player player)
        {
            Move lastMove = new Move();
            lastMove = player.NextMove();
            if (lastMove == null) return null;
            GameStory.AddMove(lastMove);
            return lastMove;
        }

        public Player AnotherPlayer(Player player)
        {
            if (player == PlayerWhite) return PlayerBlack;
            else return PlayerWhite;
        }

        public PiecesColor AnotherColor(PiecesColor color)
        {
           if (color == PiecesColor.Black) return PiecesColor.White;
            else return PiecesColor.Black;
        }

        public void ChangePlayer()
        {
            Moves.Value = AnotherPlayer(Moves.Value);
        }

        public bool IsWinningPosition()
        {
            int[] y = new int[2] { 0, Gameboard.NumberOfFields - 1 };

            for (int x = 0; x < Gameboard.NumberOfFields; ++x)
            {
                foreach(int i in y)
                {
                    if (Gameboard.Pawns[x, i] != null)
                    {
                        if ((i == 0 && Gameboard.Pawns[x, i].Owner.PiecesColor == PiecesColor.White)
                          ||(i != 0 && Gameboard.Pawns[x, i].Owner.PiecesColor == PiecesColor.Black))
                        {
                            State.Value |= GameState.Finished;
                            State.Value |= GameState.Paused;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public void Terminate()
        {
            PlayerWhite.Terminate.Value = true;
            PlayerBlack.Terminate.Value = true;
            PlayerWhite.Terminated.Value = true;
            PlayerBlack.Terminated.Value = true;
            IsTerminatedGame.Value = true;
        }

        public void Force(string color) 
        {
            if (color == "Black") Forced = PlayerBlack;
        }

        public bool isTerminated()
        {
            return (IsTerminatedGame.Value);
        }

        public void showHintExpander()
        {
            HostWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new VoidNoArgDelegate(HostWindow.showHintExpander));
        }

        public void hideHintExpander()
        {
            HostWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new VoidNoArgDelegate(HostWindow.hideHintExpander));
        }

        delegate void VoidNoArgDelegate();
        public void ProcessMoves()
        {

            Move lastMove = null;
            bool showedbox = false;
            while (true) 
            {
                if (Moves.Value.Terminate.Value)
                {
                    Terminate();
                    break;
                }

                if ((State.Value & GameState.Finished) == 0)
                {
                    if (!IsPaused())
                    {
                        if (Moves.Value.Terminate.Value) 
                        {
                            Terminate();
                            break; 
                        }
                        Thread.Sleep(DelayBetweenMovesMS);
                    }
                    WaitIfPaused();

                    if (!IsWinningPosition())
                    {
                        if (Moves.Value is HumanPlayer) showHintExpander();
                        else hideHintExpander();

                        lastMove = PlayerMove(Moves.Value);
                        if (GameStory.Count() > 0)
                            HostWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Window1.NoArgVoidDelegate(HostWindow.SetUndo));
                        else HostWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Window1.NoArgVoidDelegate(HostWindow.UnsetUndo));
                        if (!IsWinningPosition()) ChangePlayer();
                    }
                }
                else
                {
                    Thread.Sleep(200);
                    if (!showedbox)
                    {
                        MessageBox.Show(Moves.Value.PiecesColor.ToString() + " wins.");
                        showedbox = true;

                    }
                    else
                    {
                        if (GameStory.OffsetFromEndCount() > 0)
                        {
                            showedbox = false;
                            State.Value = State.Value & (~GameState.Finished);
                        }
                    }
                }
            }
            hideHintExpander();
            Terminate();
        }

        public delegate void LogDel(String str);
        public void LogErr(string p)
        {
            HostWindow.textBoxParagraph.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new LogDel(HostWindow.Log), "! " + p);
        }

        public void DeletePawn(int x, int y)
        {
            VisualChessboard.DeletePawnDelegate delete = delegate(int xx, int yy)
            {
                Gameboard.DeletePawn(xx, yy);
            };
            Gameboard.BoardCanvas.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                            delete, x, y);
        }

        public void PlacePawn(int x, int y, Player player)
        {

            VisualChessboard.PlacePawnDelegate place = delegate(int xx, int yy, Player pplayer)
            {
                Gameboard.PlacePawn(xx, yy, pplayer);
            };


            Gameboard.BoardCanvas.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                place, x, y, player);
        }

        public bool CanBeMoved(Pawn pawn)
        {
            if (pawn.Owner == Moves.Value) return true;
            else return false;
        }

        public void MakeMove()
        {
            throw new System.NotImplementedException();
        }

        public void Restart()
        {
            throw new System.NotImplementedException();
        }
    }
}
