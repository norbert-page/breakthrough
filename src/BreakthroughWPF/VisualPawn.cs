using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Threading;
using System.Diagnostics;
using System.Windows.Threading;

namespace BreakthroughWPF
{
    public class VisualPawn : Pawn
    {
        public Canvas HostCanvas;
        public VisualChessboard HostChessboard;
        public Shape Pawn;
        public ReaderWriterCustomLock<bool> BeingMoved = new ReaderWriterCustomLock<bool>();

        public Pair[] AvailableFields;
        public int NumberOfAvailableFields;
        public int AnimatedMoveStepTimeMS = 30;
        public static LinearGradientBrush WhiteGradient = new LinearGradientBrush(Colors.SkyBlue, Colors.SteelBlue, 45);
        public static LinearGradientBrush BlackGradient = new LinearGradientBrush(Colors.Silver, Colors.SlateGray, 45);

        public double PawnSize;

        public VisualPawn(Player owner, Pair boardPosition, Canvas canvas, double pawnSize, VisualChessboard host)
            : base(owner, boardPosition)
        {
         
   
            HostCanvas = canvas;
            HostChessboard = host;

            PawnSize = pawnSize;
            BeingMoved.Value = false;

            AvailableFields = new Pair[3];
            NumberOfAvailableFields = 0;

            WhiteGradient.Freeze();
            BlackGradient.Freeze();

            Pawn = new Ellipse();
            Pawn.VerticalAlignment = VerticalAlignment.Center;
            Pawn.HorizontalAlignment = HorizontalAlignment.Center;
            Pawn.Width = Pawn.Height = PawnSize;

            if (owner.PiecesColor == PiecesColor.Black) Pawn.Fill = BlackGradient;
            else Pawn.Fill = WhiteGradient;

            DropShadowBitmapEffect effect = new DropShadowBitmapEffect();

            effect.Color = Colors.Black;
            effect.Direction = 320;
            effect.ShadowDepth = HostChessboard.BoardMargin * HostCanvas.Width / 3.0;
            effect.Softness = 0.8;
            effect.Opacity = 0.4;
            Pawn.BitmapEffect = effect;

            UpdateShapePosition(); // set pawn position on hostcanvas
            DoubleAnimation MouseOverAnimation = new DoubleAnimation();
            MouseOverAnimation.From = 1;
            MouseOverAnimation.To = 0.4;
            MouseOverAnimation.AutoReverse = true;
            MouseOverAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(250));

            Pawn.IsMouseDirectlyOverChanged += delegate(Object sender, DependencyPropertyChangedEventArgs arg)
            {
                if (HostChessboard.HostGame.IsPaused()) return;
                if (HostChessboard.HostGame.Moves.Value != Owner || (HostChessboard.HostGame.Moves.Value is ComputerAIPlayer)) return;
                if (((Shape)sender).IsMouseDirectlyOver)
                {
                    Pawn.BeginAnimation(Shape.OpacityProperty, MouseOverAnimation);
                }
            };

            Pawn.PreviewMouseRightButtonDown += delegate(Object sender, MouseButtonEventArgs arg)
            {
                if (HostChessboard.HostGame.IsBoardBeingModified())
                {
                    Pair brdPosition = HostChessboard.CanvasToBoardPosition(arg.GetPosition(HostChessboard.BoardCanvas));
                    HostChessboard.HostGame.PlacePawn(brdPosition.X, brdPosition.Y, HostChessboard.HostGame.PlayerBlack);
                }
            };

            Pawn.PreviewMouseLeftButtonDown += delegate(Object sender, MouseButtonEventArgs arg)
            {
                if (HostChessboard.HostGame.IsBoardBeingModified())
                {
                    Pair brdPosition = HostChessboard.CanvasToBoardPosition(arg.GetPosition(HostChessboard.BoardCanvas));
                    HostChessboard.HostGame.PlacePawn(brdPosition.X, brdPosition.Y, HostChessboard.HostGame.PlayerWhite);
                }

                if (HostChessboard.HostGame.IsPaused()) return;
                if (HostChessboard.HostGame.Moves.Value is ComputerAIPlayer) return;

                if (HostChessboard.PieceChosen && HostChessboard.PawnTakenOK(sender)) return;
                if (!HostChessboard.HostGame.CanBeMoved(this)) return;
                if (HostChessboard.PieceChosen)
                {
                    HostChessboard.UnchoosePawn();
                    UndecoratePawn();

                    ChooseThisPawn();
                    DecoratePawn();
                    ShowAvailableFields();
                }
                else if (HostChessboard.HostGame.CanBeMoved(this))
                {
                    ChooseThisPawn();
                    DecoratePawn();
                    ShowAvailableFields();
                }
            };

            HostCanvas.Children.Add(Pawn);
        }

        public void DecoratePawn()
        {
            Pawn.StrokeThickness = 2;
            Pawn.Stroke = Brushes.Navy;
        }

        public void UndecoratePawn()
        {
            Pawn.StrokeThickness = 0;
        }

        public void ChooseThisPawn()
        {
            BeingMoved.Value = true;
            HostChessboard.PieceChosen = true;
            HostChessboard.ChosenPawn = this;
        }

        public void ShowAvailableFields() 
        {
            CalculateAvailableFields();
            for (int n = 0; n < 3; ++n)
            {
                if (AvailableFields[n] != null)
                    HostChessboard.AddFieldDecoration(AvailableFields[n]);
            }
        }

        public void CalculateAvailableFields()
        {
            AvailableFields = new Pair[3];
            NumberOfAvailableFields = 0;

            Pair[] list;
            if (Owner.PiecesColor == PiecesColor.White)
                list = new Pair[] { new Pair(-1, -1), new Pair(0, -1), new Pair(1, -1) };
            else list = new Pair[] { new Pair(-1, 1), new Pair(0, 1), new Pair(1, 1) };

            for (int n = 0; n < 3; ++n)
            {
                int x = BoardPosition.X + list[n].X;
                int y = BoardPosition.Y + list[n].Y;

                if (x < 0 || y < 0) continue;
                if (x >= HostChessboard.NumberOfFields || y >= HostChessboard.NumberOfFields) continue;

                if (n == 1 && HostChessboard.Pawns[x, y] == null) AvailableFields[1] = new Pair(x, y);
                if (n == 0 || n == 2)
                {
                    if (HostChessboard.Pawns[x, y] == null) AvailableFields[n] = new Pair(x, y);
                    else
                    {
                        if (HostChessboard.Pawns[x, y].Owner.PiecesColor != Owner.PiecesColor)
                            AvailableFields[n] = new Pair(x, y);
                    }
                }
                if (AvailableFields[n] != null) ++NumberOfAvailableFields;
            }
        }

        public delegate void SetPos(Shape sh, Double db);
        public void UpdateShapePosition()
        {
            Point pixeled = new Point();
            pixeled = HostChessboard.BoardToCanvasPosition(BoardPosition);
            HostCanvas.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new SetPos(Canvas.SetLeft),
                Pawn,
                pixeled.X - PawnSize / 2);
            HostCanvas.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new SetPos(Canvas.SetTop),
                Pawn,
                pixeled.Y - PawnSize / 2);
        }

        public void VisualUpdate()
        {
            PawnSize = HostChessboard.PawnSize;
            Pawn.Width = Pawn.Height = PawnSize;

            if (!HostChessboard.AnimatedMoves.Value) UpdateShapePosition();
            else if (!BeingMoved.Value && HostChessboard.AnimatedMoves.Value)
            {
                Pair BoardPosition2 = HostChessboard.CanvasToBoardPosition(
                        new Point(Canvas.GetLeft(Pawn), Canvas.GetTop(Pawn)));
                if (BoardPosition.X != BoardPosition2.X
                    || BoardPosition.Y != BoardPosition2.Y) UpdateShapePosition();
            }
            else if ((HostChessboard.HostGame.State.Value & GameState.Paused) > 0) UpdateShapePosition();
        }

        public override void MoveTo(Pair dst)
        {
            if (HostChessboard.HostGame.IsPaused()) return;
            Move lastMove = new Move();
            lastMove.Pawn = HostChessboard.Pawns[BoardPosition.X, BoardPosition.Y];
            lastMove.Player = Owner;
            lastMove.Source = BoardPosition;
            lastMove.Destination = dst;
            lastMove.TakenPawn = null;
            lastMove.NumberOfFields = HostChessboard.NumberOfFields;

            if (HostChessboard.Pawns[dst.X, dst.Y] != null)
            {
                lastMove.TakenPawn = HostChessboard.Pawns[dst.X, dst.Y];
                HostCanvas.Children.Remove(HostChessboard.Pawns[dst.X, dst.Y].Pawn);
            }
            HostChessboard.Pawns[dst.X, dst.Y] = HostChessboard.Pawns[BoardPosition.X, BoardPosition.Y];
            HostChessboard.Pawns[BoardPosition.X, BoardPosition.Y] = null;
            HostChessboard.Pawns[dst.X, dst.Y].BoardPosition = dst;

            VisualUpdate();

            HostChessboard.Pawns[dst.X, dst.Y].Owner.MoveMade = true;
            Owner.LastMove = lastMove;
        }

        public void AnimatedMoveTo(Pair dst)
        {
            CustomMoveDelegate mover = new CustomMoveDelegate(BackgroundShapeMover);
            Point DstPoint = HostChessboard.BoardToCanvasPosition(dst);
            DstPoint.X -= PawnSize / 2.0;
            DstPoint.Y -= PawnSize / 2.0;
            BeingMoved.Value = true;
            mover.BeginInvoke(
                new Point(Canvas.GetLeft(Pawn), Canvas.GetTop(Pawn)),
                DstPoint,
                Pawn, AnimatedMoveStepTimeMS, null, null);
            MoveTo(dst);
            BeingMoved.Value = false;
        }

        public delegate void CustomMoveDelegate(Point start, Point end, Shape sh, int timeWait);
        public delegate void ShapeMoveDelegate(Point start, Point end, Shape sh, double x_s);

        public void BackgroundShapeMover(Point start, Point end, Shape sh, int timeWait)
        {

            Stopwatch stopper = Stopwatch.StartNew();
            long last_time;
            double dist = Math.Sqrt(Math.Pow(Math.Abs(end.Y - start.Y), 2) +
                                    Math.Pow(Math.Abs(end.X - start.X), 2));
            last_time = stopper.ElapsedMilliseconds;
            int time_wait = timeWait; //ms
            double x_s;

            for (x_s = 0; x_s <= 1.0;)
            {
                Thread.Sleep(time_wait);

                HostCanvas.Dispatcher.Invoke(DispatcherPriority.Render,
                    new ShapeMoveDelegate(UpdateShapePosition), start, end, sh, x_s);

                x_s += (1.0 / dist) * 5 * (stopper.ElapsedMilliseconds / time_wait);
                last_time = stopper.ElapsedMilliseconds;
            }

            x_s = 1.0;
            HostCanvas.Dispatcher.Invoke(DispatcherPriority.Render,
                    new ShapeMoveDelegate(UpdateShapePosition), start, end, sh, x_s);
        }

        public void UpdateShapePosition(Point start, Point end, Shape sh, double x_s)
        {
            Canvas.SetLeft(sh, start.X +
                ((end.X > start.X) ? ((end.X - start.X) * x_s) : ((end.X - start.X) * x_s)));
            Canvas.SetTop(sh, start.Y +
                ((end.Y > start.Y) ? ((end.Y - start.Y) * x_s) : ((end.Y - start.Y) * x_s)));
        }

        public void Dispose()
        {
            HostCanvas.Children.Remove(Pawn);
        }
    }
}
