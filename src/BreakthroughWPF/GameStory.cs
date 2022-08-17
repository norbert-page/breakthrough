using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Reflection;
using System.Windows.Input;

namespace BreakthroughWPF
{
    public class GameStory
    {
        private Object GameStoryLocker = new Object();
        private Game HostGame;
        private ListBox PlayerWhite, PlayerBlack;
        private LinkedList<Move> Story = new LinkedList<Move>();
        private int OffsetFromEnd = 0;
        private bool VisualTraverse;
        private LinkedListNode<Move> OffsetNode;

        public int Count()
        {
            return Story.Count;
        }

        public int OffsetFromEndCount()
        {
            return OffsetFromEnd;
        }

        public GameStory(Game hostGame, ListBox pw, ListBox pb)
        {
            OffsetNode = Story.Last;
            HostGame = hostGame;
            PlayerWhite = pw;
            PlayerBlack = pb;
            VisualTraverse = true;
        }

        public void lbi_PreviewMouseLeftButtonDown(Object sender, MouseButtonEventArgs ar)
        {
            if (sender == null) return;
            if (HostGame.IsNetworkGame.Value)
            {
                MessageBox.Show("You cannot traverse game history during network game with remote player.", "Not available for network game", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            HostGame.HostWindow.PauseGame();
            if (PlayerWhite.Items.Contains(sender as ListBoxItem))
            {
                int n = PlayerWhite.Items.IndexOf(sender as ListBoxItem);
                int offset = 2*(n + 1) - 2;
                int current_offset = Story.Count - OffsetFromEnd;

                lock (GameStoryLocker)
                {
                    VisualTraverse = false;
                    for (int i = current_offset; i < offset - 1; ++i) Redo();
                    for (int i = current_offset; i > offset + 1; --i) Undo();
                    VisualTraverse = true;
                    if (current_offset < offset) Redo();
                    if (current_offset > offset) Undo();
                }
            }
            else
            {
                int n = PlayerBlack.Items.IndexOf(sender as ListBoxItem);
                int offset = 2 * (n + 1) - 2 + 1;
                int current_offset = Story.Count - OffsetFromEnd;

                lock (GameStoryLocker)
                {
                    VisualTraverse = false;
                    for (int i = current_offset; i < offset - 1; ++i) Redo();
                    for (int i = current_offset; i > offset + 1; --i) Undo();
                    VisualTraverse = true;
                    if (current_offset < offset) Redo();
                    if (current_offset > offset) Undo();
                }
            }
        }

        public delegate void NewItemDelegate(String str, PiecesColor color);
        public void NewItem(String str, PiecesColor color)
        {
            ListBoxItem lbi = new ListBoxItem();
            OffsetNode.Value.ListBoxItemHandle = lbi;

            lbi.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(lbi_PreviewMouseLeftButtonDown);

            lbi.Content = ((Story.Count + 1)/2).ToString() + ": " + str;
            if (color == PiecesColor.White)
            {
                PlayerWhite.Items.Add(lbi);
                PlayerWhite.ScrollIntoView(lbi);
                PlayerWhite.SelectedItem = lbi;
                PlayerBlack.UnselectAll();
            }
            else 
            {
                PlayerBlack.Items.Add(lbi);
                PlayerBlack.ScrollIntoView(lbi);
                PlayerBlack.SelectedItem = lbi;
                PlayerWhite.UnselectAll();
            }
        }

        public void RemoveLastItem()
        {
            if (Story.Count == 0) return;
            if (Story.Last.Value.Pawn.Owner.PiecesColor == PiecesColor.White)
            {

                PlayerWhite.Items.Remove(Story.Last.Value.ListBoxItemHandle);
                if (VisualTraverse && Story.Count >= 2)
                {
                    PlayerBlack.ScrollIntoView(Story.Last.Previous.Value.ListBoxItemHandle);
                    PlayerBlack.SelectedItem = Story.Last.Previous.Value.ListBoxItemHandle;
                    PlayerWhite.UnselectAll();
                }
            }
            else
            {
                PlayerBlack.Items.Remove(Story.Last.Value.ListBoxItemHandle);
                if (VisualTraverse && Story.Count >= 2)
                {
                    PlayerWhite.ScrollIntoView(Story.Last.Previous.Value.ListBoxItemHandle);
                    PlayerWhite.SelectedItem = Story.Last.Previous.Value.ListBoxItemHandle;
                    PlayerBlack.UnselectAll();
                }
            }
            
        }

        public void VisualUpdate(Move move)
        {
            HostGame.HostWindow.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new NewItemDelegate(NewItem),
                move.ToString(), move.Pawn.Owner.PiecesColor);
        }

        public void AddMove(Move move)
        {
            lock (GameStoryLocker)
            {
                Move localmove = new Move(move);
                move.NumberOfMove = (Story.Count + 1) / 2;
                Story.AddLast(localmove);
                OffsetNode = Story.Last;
                OffsetFromEnd = 0;
                VisualUpdate(localmove);
            }
        }

        public delegate void NoArgDelegate();
        public void RemoveLastMove()
        {
            lock (GameStoryLocker)
            {
                    HostGame.HostWindow.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                        new NoArgDelegate(RemoveLastItem));
                Story.RemoveLast();
                --OffsetFromEnd;
            }
        }

        public void SelectItem(PiecesColor color, ListBoxItem lbi, VisualPawn pawn)
        {
            if (!VisualTraverse) return;
            if (lbi == null)
            {
                PlayerWhite.UnselectAll();
                PlayerBlack.UnselectAll();
                HostGame.Gameboard.UnchoosePawn();
                return;
            }

            if (color == PiecesColor.White)
            {
                HostGame.Gameboard.UnchoosePawn();
                pawn.ChooseThisPawn();
                pawn.DecoratePawn();
                PlayerWhite.SelectedItem = lbi;
                PlayerWhite.ScrollIntoView(lbi);
                PlayerBlack.UnselectAll();
            }
            else
            {
                HostGame.Gameboard.UnchoosePawn();
                pawn.ChooseThisPawn();
                pawn.DecoratePawn();
                PlayerBlack.SelectedItem = lbi;
                PlayerBlack.ScrollIntoView(lbi);
                PlayerWhite.UnselectAll();
            }
        }

        public delegate void ButtonDelegate(System.Windows.Controls.Button b);
        public delegate int AddDelegate(Shape sh);
        public delegate void SelectDelegate(PiecesColor color, ListBoxItem lbi, VisualPawn pawn);
        public void Undo()
        {
            lock (GameStoryLocker)
            {
                if (Story.Count == 0 || OffsetFromEnd == Story.Count) return;

                // cofnij ruch OffsetNode

                HostGame.Gameboard.Pawns[
                    OffsetNode.Value.Source.X,
                    OffsetNode.Value.Source.Y] = OffsetNode.Value.Pawn;
                HostGame.Gameboard.Pawns[
                        OffsetNode.Value.Destination.X,
                        OffsetNode.Value.Destination.Y] = null;

                OffsetNode.Value.Pawn.BoardPosition = OffsetNode.Value.Source;
                OffsetNode.Value.Pawn.BeingMoved.Value = false;
                OffsetNode.Value.Pawn.VisualUpdate();

                if (OffsetNode.Value.TakenPawn != null)
                {
                    HostGame.Gameboard.Pawns[
                        OffsetNode.Value.Destination.X,
                        OffsetNode.Value.Destination.Y] = OffsetNode.Value.TakenPawn;
                    OffsetNode.Value.TakenPawn.BoardPosition = OffsetNode.Value.Destination;
                    OffsetNode.Value.TakenPawn.BeingMoved.Value = false;

                    HostGame.Gameboard.BoardCanvas.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new AddDelegate(HostGame.Gameboard.BoardCanvas.Children.Add),
                        ((OffsetNode.Value.TakenPawn).Pawn));

                    OffsetNode.Value.TakenPawn.VisualUpdate();
                }

                if (OffsetNode.Previous != null) 
                    HostGame.HostWindow.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                        new SelectDelegate(SelectItem),
                        HostGame.AnotherColor(OffsetNode.Value.Pawn.Owner.PiecesColor),
                        OffsetNode.Previous.Value.ListBoxItemHandle,
                        OffsetNode.Previous.Value.Pawn);
                else
                    HostGame.HostWindow.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                        new SelectDelegate(SelectItem),
                        HostGame.AnotherColor(OffsetNode.Value.Pawn.Owner.PiecesColor),
                        null,
                        null);

                ++OffsetFromEnd;
                OffsetNode = OffsetNode.Previous;

                HostGame.HostWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new ButtonDelegate(HostGame.HostWindow.EnableButton),
                    HostGame.HostWindow.redo);

                if (OffsetNode == null)
                    HostGame.HostWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new ButtonDelegate(HostGame.HostWindow.DisableButton),
                    HostGame.HostWindow.undo);
            }
        }

        public delegate void RemoveDelegate(Shape sh);
        public void Redo()
        {
            lock (GameStoryLocker)
            {
                if (OffsetFromEnd == 0) return;

                // wykonaj ruch OffsetNode.Next
                --OffsetFromEnd;
                if (OffsetNode != null) OffsetNode = OffsetNode.Next;
                else OffsetNode = Story.First;

                if (HostGame.Gameboard.Pawns[
                        OffsetNode.Value.Destination.X,
                        OffsetNode.Value.Destination.Y] != null)
                    HostGame.Gameboard.BoardCanvas.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new RemoveDelegate(HostGame.Gameboard.BoardCanvas.Children.Remove),
                        HostGame.Gameboard.Pawns[
                        OffsetNode.Value.Destination.X,
                        OffsetNode.Value.Destination.Y].Pawn);
                HostGame.Gameboard.Pawns[
                        OffsetNode.Value.Destination.X,
                        OffsetNode.Value.Destination.Y] = OffsetNode.Value.Pawn;
                HostGame.Gameboard.Pawns[
                    OffsetNode.Value.Source.X,
                    OffsetNode.Value.Source.Y] = null;

                OffsetNode.Value.Pawn.BoardPosition = OffsetNode.Value.Destination;
                OffsetNode.Value.Pawn.BeingMoved.Value = false;
                OffsetNode.Value.Pawn.VisualUpdate();

                if (OffsetNode != null)
                    HostGame.HostWindow.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                        new SelectDelegate(SelectItem),
                        OffsetNode.Value.Pawn.Owner.PiecesColor,
                        OffsetNode.Value.ListBoxItemHandle,
                        OffsetNode.Value.Pawn);

                HostGame.HostWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new ButtonDelegate(HostGame.HostWindow.EnableButton),
                    HostGame.HostWindow.undo);

                if (OffsetFromEnd == 0)
                    HostGame.HostWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new ButtonDelegate(HostGame.HostWindow.DisableButton),
                    HostGame.HostWindow.redo);
            }
        }

        public void TraversalFinished()
        {
            lock (GameStoryLocker)
            {
                VisualTraverse = false;
                while (OffsetFromEnd > 1) RemoveLastMove();
                VisualTraverse = true;
                if (OffsetFromEnd > 0) RemoveLastMove();

                OffsetNode = Story.Last;
                if (OffsetNode != null)
                    HostGame.Moves.Value = HostGame.AnotherPlayer(OffsetNode.Value.Pawn.Owner);
                else HostGame.Moves.Value = HostGame.PlayerWhite;
            }
        }

        public void Clear()
        {
            lock (GameStoryLocker) 
            {
                OffsetFromEnd = 0;
                OffsetNode = null;
                Story = new LinkedList<Move>();
                VisualTraverse = true;
                PlayerWhite.Items.Clear();
                PlayerBlack.Items.Clear();
            }
        }
    }
}
