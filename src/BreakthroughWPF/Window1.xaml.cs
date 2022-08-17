using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Net;

namespace BreakthroughWPF
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Game Game;

        public static RoutedCommand UndoCommand = new RoutedCommand();
        public static RoutedCommand RedoCommand = new RoutedCommand();
        public static RoutedCommand PauseCommand = new RoutedCommand();
        public static RoutedCommand ResumeCommand = new RoutedCommand();

        public static int WidthAdjustment = 188 + 20;
        public static int HeightAdjustment = 74 + 15;

        // for network connectivity:
        public ServiceHost GameConnectionHost;
        // service singleton instance
        public GameConnectionService GameConnectionServiceInstance;

        public GameConnection.GameConnectionServiceClient GameConnectionClient;
        public ReaderWriterCustomLock<string> endpointAddress;
        public string memberID;
        public string suffix;

        public ParticipantsWindow choosePlayerWindow;

        public Window1()
        {
            InitializeComponent();

            GameConnectionService.host = this;

            BoardCanvas.Width = BoardCanvas.Height = 
                Math.Min(MainWindow.Width - WidthAdjustment, MainWindow.Height - HeightAdjustment);

            Game = new Game(BoardCanvas, this, true, false, false);

            //InitializeGame();
            AddMainWindowResizeEventHandler();
            InitMovesExpander();
            InitHintExpander();
            InitCommands();

            Log("Ready.");
            //textBoxParagraph.ContentEnd.InsertTextInRun("Currently playing song: " + mediaElement1.Source.ToString());

            System.Windows.Application.Current.SessionEnding += new SessionEndingCancelEventHandler(Current_SessionEnding);
        }

        void Current_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            // Shutdown listener
            if (GameConnectionHost != null)
            {
                try { GameConnectionHost.Abort(); }
                catch (Exception) { }
            }

            // Shutdown client
            if (GameConnectionClient != null)
            {
                try { GameConnectionClient.Abort(); }
                catch (Exception) { }
            }
        }

        public void EnableButton(System.Windows.Controls.Button b)
        {
            lock (Game.ButtonLocker)
            {
                b.IsEnabled = true;
            }
        }

        public void DisableButton(System.Windows.Controls.Button b)
        {
            lock (Game.ButtonLocker)
            {
                b.IsEnabled = false;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        public void Resized()
        {
            Size sc = new Size(MainWindow.ActualWidth, MainWindow.ActualHeight);
            Resized(sc);
        }

        public void Resized(Size arg)
        {
            int WidthCorrection = (int)(grid.ColumnDefinitions.ElementAt(1).Width.Value - 180);
            int size = (int)Math.Min(arg.Width - WidthAdjustment - WidthCorrection, arg.Height - HeightAdjustment);

            BoardCanvas.Width = BoardCanvas.Height = Math.Max(size, 0);
            bool previous = Game.Gameboard.AnimatedMoves.Value;
            Game.Gameboard.AnimatedMoves.Value = false;
            Game.Gameboard.VisualUpdate();
            Game.Gameboard.AnimatedMoves.Value = previous;
        }

        private void AddMainWindowResizeEventHandler()
        {
            // Adjust canvas1 size on MainWindow resize:
            MainWindow.SizeChanged += delegate(Object sender, SizeChangedEventArgs arg)
            {
                Resized(arg.NewSize);
            };
        }

        private void InitHintExpander()
        {
            expanderHint.Expanded += new RoutedEventHandler(expanderHint_Expanded);
        }

        public void setHint(String str)
        {
            hintLabel.Content = str;
        }

        public void CalculateHint()
        {
            ComputerAIPlayer player = new ComputerAIPlayer(Game.Moves.Value.PiecesColor);
            player.SetGameboard(Game.Gameboard);

            ComputerAIPlayer.SMove smove = player.GenerateNextMove(Game.Moves.Value.PiecesColor);
            Move move = new Move();
            move.Source = new Pair(smove.sx, smove.sy);
            move.Destination = new Pair(smove.ex, smove.ey);
            move.NumberOfFields = 8;
            String str = move.ToString() + (smove.beaten ? " !" : "");

            hintLabel.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<String>(setHint), str);
            Game.Gameboard.BoardCanvas.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action<Pair>(Game.Gameboard.AddFieldDecoration2), new Pair(smove.ex, smove.ey));
            Game.Gameboard.BoardCanvas.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action<Pair>(Game.Gameboard.AddFieldDecoration2), new Pair(smove.sx, smove.sy));
        }

        void expanderHint_Expanded(object sender, RoutedEventArgs e)
        {
            hintLabel.Content = "Processing...";
            NoArgVoidDelegate hinter = new NoArgVoidDelegate(CalculateHint);
            hinter.BeginInvoke(null, null);
        }

        private void InitMovesExpander()
        {
            /*System.Windows.Controls.ListBox movesListBox = new System.Windows.Controls.ListBox();
            movesListBox.
            movesExpander.Content = movesListBox;*/
        }

        public void PauseGame()
        {
            lock (Game.ButtonLocker)
            {
                bool wasPaused = ((Game.State.Value & GameState.Paused) > 0);
                pause.IsEnabled = false;
                resume.IsEnabled = true;
                Game.State.Value |= GameState.Paused;
                Game.Gameboard.UnchoosePawn();
                Game.Gameboard.AnimatedMoves.Value = false;
                if (!wasPaused) Log("game paused");
            }
        }

        public void ResumeGame()
        {
            if ((Game.State.Value & GameState.Finished) == 0)
            lock (Game.ButtonLocker)
            {
                Game.State.Value &= ~GameState.Paused;
                pause.IsEnabled = true;
                resume.IsEnabled = false;
                undo.IsEnabled = (Game.GameStory.Count() > 0);
                redo.IsEnabled = false;
                Game.GameStory.TraversalFinished();
                Game.Gameboard.AnimatedMoves.Value = Game.Gameboard.AfterResumeAnimatedMoves.Value;
                Log("game resumed");
            }
        }

        private void pause_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
        }

        private void resume_Click(object sender, RoutedEventArgs e)
        {
            ResumeGame();
        }

        private void undo_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
            Game.GameStory.Undo();
        }

        private void redo_Click(object sender, RoutedEventArgs e)
        {
            PauseGame();
            Game.GameStory.Redo();
        }

        public void ExecutedUndoCommand(object sender, ExecutedRoutedEventArgs e)
        {
            bool ok = false;
            lock (Game.ButtonLocker)
            { ok = undo.IsEnabled; }
            if (ok) undo_Click(null, e);
        }
        public void ExecutedRedoCommand(object sender, ExecutedRoutedEventArgs e)
        {
            bool ok = false;
            lock (Game.ButtonLocker)
            { ok = redo.IsEnabled; }
            if (ok) redo_Click(null, e);
        }
        public void ExecutedResumeCommand(object sender, ExecutedRoutedEventArgs e)
        {
            bool ok = false;
            lock (Game.ButtonLocker)
            { ok = resume.IsEnabled; }
            if (ok) resume_Click(null, e);
        }
        public void ExecutedPauseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            bool ok = false;
            lock (Game.ButtonLocker)
            { ok = pause.IsEnabled; }
            if (ok) pause_Click(null, e);
        }

        public void CanExecuteCustomCommand(object sender,
                            CanExecuteRoutedEventArgs e)
        {
            System.Windows.Controls.Control target = e.Source as System.Windows.Controls.Control;

            if (target != null)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }

        public void InitCommands()
        {
            CommandBinding UndoCommandBinding = new CommandBinding(
            UndoCommand, ExecutedUndoCommand, CanExecuteCustomCommand);

            CommandBinding RedoCommandBinding = new CommandBinding(
            RedoCommand, ExecutedRedoCommand, CanExecuteCustomCommand);

            CommandBinding PauseCommandBinding = new CommandBinding(
            PauseCommand, ExecutedPauseCommand, CanExecuteCustomCommand);

            CommandBinding ResumeCommandBinding = new CommandBinding(
            ResumeCommand, ExecutedResumeCommand, CanExecuteCustomCommand);

            this.CommandBindings.Add(UndoCommandBinding);
            this.CommandBindings.Add(RedoCommandBinding);
            this.CommandBindings.Add(ResumeCommandBinding);
            this.CommandBindings.Add(PauseCommandBinding);

            KeyBinding UndoCmdKeyBinding = new KeyBinding(
                UndoCommand,
                Key.U,
                ModifierKeys.Alt);
            this.InputBindings.Add(UndoCmdKeyBinding);

            KeyBinding RedoCmdKeyBinding = new KeyBinding(
                RedoCommand,
                Key.R,
                ModifierKeys.Alt);
            this.InputBindings.Add(RedoCmdKeyBinding);

            KeyBinding PauseCmdKeyBinding = new KeyBinding(
                PauseCommand,
                Key.P,
                ModifierKeys.Alt);
            this.InputBindings.Add(PauseCmdKeyBinding);

            KeyBinding ResumeCmdKeyBinding = new KeyBinding(
                ResumeCommand,
                Key.E,
                ModifierKeys.Alt);
            this.InputBindings.Add(ResumeCmdKeyBinding);
        }

        private void rectangle1_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (grid.ColumnDefinitions.ElementAt(1).Width.Value > ((new GridLength(100)).Value))
                grid.ColumnDefinitions.ElementAt(1).Width = new GridLength(0, GridUnitType.Pixel);
            else
                grid.ColumnDefinitions.ElementAt(1).Width = new GridLength(180, GridUnitType.Pixel);
            Resized();
        }

        private void rectangle1_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rectangle1.Opacity = 1.0;
        }

        private void rectangle1_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            rectangle1.Opacity = 0.3;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            About about = new About();
            about.Owner = this;
            about.ShowDialog();
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Title = "Open saved game";
            openDialog.DefaultExt = "board";
            openDialog.Filter = "Board files (*.board)|*.board|All files|*.*";
            openDialog.FileName = "BreakthroughGame";
            openDialog.ShowDialog();

            openDialog.AddExtension = true;
            openDialog.AutoUpgradeEnabled = true;
            
            PauseGame();

            if (new FileInfo(openDialog.FileName).Exists) using (Stream rawFile = openDialog.OpenFile())
            {
                if (rawFile != null) using (StreamReader file = new StreamReader(rawFile, Encoding.Unicode))
                {
                    Game.GameStory.Clear();
                    for (int x = 0; x < Game.Gameboard.NumberOfFields; ++x)
                    {
                        for (int y = 0; y < Game.Gameboard.NumberOfFields; ++y)
                        {
                            Game.DeletePawn(x, y);
                        }
                    }

                    file.ReadLine();
                    file.ReadLine(); // read version

                    String player1 = file.ReadLine(),
                           player2 = file.ReadLine();

                    Game.PlayerWhite.Terminate.Value = true;
                    Game.PlayerBlack.Terminate.Value = true;
                    Game.PlayerWhite.MoveMade = true;
                    Game.PlayerBlack.MoveMade = true;

                    Thread.Sleep(200);

                    if (player1 == "Human") Game.PlayerWhite = new HumanPlayer(PiecesColor.White);
                    if (player1 == "Computer") Game.PlayerWhite = new ComputerAIPlayer(PiecesColor.White);

                    if (player2 == "Human") Game.PlayerBlack = new HumanPlayer(PiecesColor.Black);
                    if (player2 == "Computer") Game.PlayerBlack = new ComputerAIPlayer(PiecesColor.Black);

                    Game.PlayerWhite.SetGameboard(Game.Gameboard);
                    Game.PlayerBlack.SetGameboard(Game.Gameboard);

                    String moves = file.ReadLine();
                    if (moves == "White") Game.Moves.Value = Game.PlayerWhite;
                    else Game.Moves.Value = Game.PlayerBlack;

                    String temp;
                    for (int x = 0; x < Game.Gameboard.NumberOfFields; ++x)
                    {
                        for (int y = 0; y < Game.Gameboard.NumberOfFields; ++y)
                        {
                            temp = file.ReadLine();
                            if (temp == "White") Game.PlacePawn(x, y, Game.PlayerWhite);
                            else if (temp == "Black") Game.PlacePawn(x, y, Game.PlayerBlack);
                        }
                    }

                    Log("board setting loaded from file");

                    ResumeGame();
                    Game.State.Value &= ~GameState.Paused;
                    Game.State.Value &= ~GameState.Finished;

                    Game.Force(moves);
                    Game.StartProcessingMoves();
                }
            }
            ResumeGame();
        }

        private void GameMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Log(String str)
        {
            textBoxParagraph.ContentStart.InsertTextInRun("- " + str);
            textBoxParagraph.ContentStart.InsertLineBreak();
        }

        private void NewGameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            modificationOver(Game.Moves.Value);
            newGameWindow options = new newGameWindow(this);
            options.ShowDialog();
        }

        private void NewNetworkGameMenuItem_Click(object sender, RoutedEventArgs e)
        {
            modificationOver(Game.Moves.Value);
            NewNetworkGameWindow step1 = new NewNetworkGameWindow(this);
            step1.Owner = this;
            step1.ShowDialog();
        }

        private void modifyButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Game.State.Value & GameState.BeingModified) == 0){
                Log("Modification mode started.");
                Game.State.Value |= GameState.BeingModified;
                PauseGame();
                undo.IsEnabled = false;
                resume.IsEnabled = false;
                modifyButton.Visibility = Visibility.Hidden;
                startBlackButton.Visibility = Visibility.Visible;
                startWhiteButton.Visibility = Visibility.Visible;
                clearBoardButton.Visibility = Visibility.Visible;
                Game.GameStory.Clear();
            }
        }

        private void clearBoardButton_Click(object sender, RoutedEventArgs e)
        {
            for (int x = 0; x < Game.Gameboard.NumberOfFields; ++x)
            {
                for (int y = 0; y < Game.Gameboard.NumberOfFields; ++y)
                {
                    Game.DeletePawn(x, y);
                }
            }
        }

        private void modificationOver(Player player)
        {
            startBlackButton.Visibility = Visibility.Hidden;
            startWhiteButton.Visibility = Visibility.Hidden;
            clearBoardButton.Visibility = Visibility.Collapsed;
            modifyButton.Visibility = Visibility.Visible;
            Game.State.Value &= ~GameState.BeingModified;
            ResumeGame();
            Game.Moves.Value = player;
            Log("Modification over, gameplay mode started.");
        }

        private void startWhiteButton_Click(object sender, RoutedEventArgs e)
        {
            modificationOver(Game.PlayerWhite);
        }

        private void startBlackButton_Click(object sender, RoutedEventArgs e)
        {
            modificationOver(Game.PlayerBlack);
        }
        
        public delegate void NoArgVoidDelegate();
        public void SetUndo()
        {
            undo.IsEnabled = true;
        }

        public void UnsetUndo()
        {
            undo.IsEnabled = false;
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Title = "Save current game";
            saveDialog.DefaultExt = "board";
            saveDialog.Filter = "Board files (*.board)|*.board|All files|*.*";
            saveDialog.FileName = "BreakthroughGame";
            saveDialog.ShowDialog();
            PauseGame();

            using (Stream rawFile = saveDialog.OpenFile())
            {
                if (rawFile != null) using (StreamWriter file = new StreamWriter(rawFile, Encoding.Unicode))
                {
                    file.WriteLine("Breakthrough game state file.");
                    file.WriteLine(1.0D); // version

                    // Player white
                    if (Game.PlayerWhite is HumanPlayer) file.WriteLine("Human"); else file.WriteLine("Computer");
                    if (Game.PlayerBlack is HumanPlayer) file.WriteLine("Human"); else file.WriteLine("Computer");

                    if (Game.Moves.Value.PiecesColor == PiecesColor.White) file.WriteLine("White");
                    else file.WriteLine("Black");

                    for (int x = 0; x < Game.Gameboard.NumberOfFields; ++x)
                    {
                        for (int y = 0; y < Game.Gameboard.NumberOfFields; ++y)
                        {
                            if (Game.Gameboard.Pawns[x, y] == null) file.WriteLine("None");
                            else
                            {
                                if (Game.Gameboard.Pawns[x, y].Owner.PiecesColor == PiecesColor.White) file.WriteLine("White");
                                else file.WriteLine("Black");
                            }
                        }
                    }
                    Log("game saved to file");
                }
            }
            ResumeGame();
        }

        public void showHintExpander()
        {
            expanderHint.IsExpanded = false;
            expanderHint.Visibility = Visibility.Visible;
        }

        public void hideHintExpander()
        {
            expanderHint.IsExpanded = false;
            expanderHint.Visibility = Visibility.Collapsed;
        }
    }
}
