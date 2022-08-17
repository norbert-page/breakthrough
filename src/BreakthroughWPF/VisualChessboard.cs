using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Effects;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace BreakthroughWPF
{
    public class VisualChessboard:Chessboard
    {
        public Rectangle[,]  Fields;
        public new VisualPawn[,] Pawns;
        //public List<Pawn> PawnsWhite, PawnsBlack; TODO

        public Canvas BoardCanvas;
        public Rectangle ShadowGenerator;
        public DropShadowBitmapEffect ShadowGeneratorEffect;

        public double FieldSize = 40;
        public double PawnSizeToFieldSizeFactor = 0.6;
        public ReaderWriterCustomLock<bool> AfterResumeAnimatedMoves = new ReaderWriterCustomLock<bool>(true);
        public ReaderWriterCustomLock<bool> AnimatedMoves = new ReaderWriterCustomLock<bool>(true);
        public double PawnSize;
        public double BoardMargin = 0.05;
        public bool PieceChosen;
        public VisualPawn ChosenPawn;

        public VisualChessboard(Game hostGame, Player playerWhite, Player playerBlack, Canvas canvas)
            : base()
        {
            HostGame = hostGame;
            BoardCanvas = canvas;
            PieceChosen = false;
            ChosenPawn = null;
            FieldSize = (int)(BoardCanvas.Width * (1.0 - 2 * BoardMargin) / NumberOfFields);
            PawnSize = FieldSize * PawnSizeToFieldSizeFactor;

            ShadowGenerator = new Rectangle();
            ShadowGenerator.Fill = Brushes.White;
            DropShadowBitmapEffect ShadowGeneratorEffect = new DropShadowBitmapEffect();
            ShadowGeneratorEffect.Color = Colors.Silver;
            ShadowGeneratorEffect.Direction = 300;
            ShadowGeneratorEffect.ShadowDepth = BoardCanvas.Height * BoardMargin;
            ShadowGeneratorEffect.Softness = 0.9;
            ShadowGeneratorEffect.Opacity = 1;
            ShadowGenerator.BitmapEffect = ShadowGeneratorEffect;
            ShadowGenerator.Width = ShadowGenerator.Height = FieldSize * NumberOfFields + 2;
            ShadowGenerator.StrokeThickness = 1;
            ShadowGenerator.Stroke = Brushes.Gray;
            Canvas.SetTop(ShadowGenerator, BoardCanvas.Height * BoardMargin - 1);
            Canvas.SetLeft(ShadowGenerator, BoardCanvas.Width * BoardMargin - 1);

            BoardCanvas.Children.Add(ShadowGenerator);

            Fields = new Rectangle[NumberOfFields, NumberOfFields];

            for (int x = 0; x < NumberOfFields; ++x)
            {
                for (int y = 0; y < NumberOfFields; ++y)
                {
                    Fields[x, y] = new Rectangle();
                    if ((x + y) % 2 == 0) Fields[x, y].Fill = Brushes.Silver;
                    else Fields[x, y].Fill = Brushes.White;
                    Fields[x, y].Fill.Freeze();
                    
                    Fields[x, y].Width = Fields[x, y].Height = FieldSize;
                    Canvas.SetTop(Fields[x, y], BoardCanvas.Height * BoardMargin + FieldSize * y);
                    Canvas.SetLeft(Fields[x, y], BoardCanvas.Width * BoardMargin + FieldSize * x);
                    
                    BoardCanvas.Children.Add(Fields[x, y]);

                    // Add event handlers for Fields.
                    Fields[x, y].MouseLeftButtonDown += delegate(Object sender, MouseButtonEventArgs arg)
                    {
                        if (HostGame.IsBoardBeingModified())
                        {
                            Pair brdPosition = CanvasToBoardPosition(arg.GetPosition(BoardCanvas));
                            HostGame.PlacePawn(brdPosition.X, brdPosition.Y, playerWhite);
                        }
                        else
                        {
                            if (HostGame.IsPaused())
                            {
                                UnchoosePawn();
                            }
                            if (sender == null) return;
                            PawnTakenOK(sender, arg.GetPosition(BoardCanvas));
                        }
                    };

                    Fields[x, y].MouseRightButtonDown += delegate(Object sender, MouseButtonEventArgs arg)
                    {
                        if (HostGame.IsBoardBeingModified())
                        {
                            Pair brdPosition = CanvasToBoardPosition(arg.GetPosition(BoardCanvas));
                            HostGame.PlacePawn(brdPosition.X, brdPosition.Y, playerBlack);
                        }
                    };

                    // Add event handler for DEBUG position calc
                    Fields[x, y].MouseEnter += delegate(Object sender, MouseEventArgs arg)
                    {
                        Pair pos = CanvasToBoardPosition(arg.GetPosition(BoardCanvas));
                        if (!PieceChosen)
                            HostGame.SafeStatusSet("Move: " + Move.ChessPosField(NumberOfFields, pos));
                        else if (Pawns[pos.X, pos.Y] == null)
                            HostGame.SafeStatusSet("Move: " + Move.ChessPosField(NumberOfFields, ChosenPawn.BoardPosition) +"-" + Move.ChessPosField(NumberOfFields, pos));
                        else if (Pawns[pos.X, pos.Y].Owner.PiecesColor != ChosenPawn.Owner.PiecesColor)
                            HostGame.SafeStatusSet("Move: " + Move.ChessPosField(NumberOfFields, ChosenPawn.BoardPosition) + "-" + Move.ChessPosField(NumberOfFields, pos) + " !");
                    };
                }
            }   

            Pawns = new VisualPawn[NumberOfFields, NumberOfFields];
            for (int y = NumberOfFields - 2; y < NumberOfFields; ++y)
            {
                for (int x = 0; x < NumberOfFields; ++x)
                {
                    Pawns[x, y] = 
                        new VisualPawn(playerWhite, new Pair(x, y), BoardCanvas, PawnSize, this);
                    Pawns[x, NumberOfFields - y - 1] = 
                        new VisualPawn(playerBlack, new Pair(x, NumberOfFields - y - 1), BoardCanvas, PawnSize, this);
                }
            }
        }

        public void UnchoosePawn()
        {
            if (!PieceChosen || ChosenPawn == null) return;
            ChosenPawn.BeingMoved.Value = false;
            ChosenPawn.Pawn.StrokeThickness = 0;
            PieceChosen = false;
            ChosenPawn = null;
        }

        public void AddFieldDecoration(Pair brd)
        {
            DoubleAnimation effect = new DoubleAnimation();
            effect.From = 0;
            effect.To = 1;
            effect.AutoReverse = true;
            effect.RepeatBehavior = new RepeatBehavior(1);
            effect.Duration = new Duration(TimeSpan.FromMilliseconds(500));


            Fields[(int)brd.X, (int)brd.Y].Stroke = Brushes.Black;
            Fields[(int)brd.X, (int)brd.Y].BeginAnimation(Rectangle.StrokeThicknessProperty, effect);
        }

        public void AddFieldDecoration2(Pair brd)
        {
            DoubleAnimation effect = new DoubleAnimation();
            effect.From = 0;
            effect.To = 2.0;
            effect.AutoReverse = true;
            effect.RepeatBehavior = new RepeatBehavior(6);
            effect.Duration = new Duration(TimeSpan.FromMilliseconds(200));


            Fields[(int)brd.X, (int)brd.Y].Stroke = Brushes.Indigo;
            Fields[(int)brd.X, (int)brd.Y].BeginAnimation(Rectangle.StrokeThicknessProperty, effect);
        }

        // centered coordinate of position
        public Point BoardToCanvasPosition(Pair brd)
        {
            return new Point(
                brd.X * FieldSize + BoardCanvas.Width * BoardMargin + FieldSize / 2,
                brd.Y * FieldSize + BoardCanvas.Height * BoardMargin + FieldSize / 2);
        }

        public Pair CanvasToBoardPosition(Point cnv)
        {
            return new Pair(
                (int) Math.Min(((cnv.X - BoardCanvas.Width * BoardMargin) / FieldSize), 7.0),
                (int) Math.Min(((cnv.Y - BoardCanvas.Height * BoardMargin) / FieldSize), 7.0));
        }

        public bool PawnTakenOK(Object sender) { return PawnTakenOK(sender, new Point(-100.5, -100.5)); }
        public bool PawnTakenOK(Object sender, Point pos)
        {
            bool taken = false;
            if (PieceChosen)
            {
                Pair brdPosition;
                if (!(pos.X <= -100)) brdPosition = CanvasToBoardPosition(pos);
                else
                {
                    if (sender is Rectangle)
                        brdPosition = CanvasToBoardPosition(new Point(
                        Canvas.GetLeft((Rectangle)sender),
                        Canvas.GetTop((Rectangle)sender)));
                    else if (sender is Ellipse)
                    {
                        brdPosition = CanvasToBoardPosition(new Point(
                        Canvas.GetLeft((Ellipse)sender),
                        Canvas.GetTop((Ellipse)sender)));
                    }
                    else throw new ApplicationException("Unsupported sender: " + sender.ToString());
                }

                ChosenPawn.CalculateAvailableFields();
                for (int n = 0; n < 3; ++n)
                {
                    if (ChosenPawn.AvailableFields[n] == null) continue;
                    if (ChosenPawn.AvailableFields[n].X == brdPosition.X
                     && ChosenPawn.AvailableFields[n].Y == brdPosition.Y)
                    {
                        if (AnimatedMoves.Value)
                            BoardCanvas.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ComputerPlayer.MoveToDel(ChosenPawn.AnimatedMoveTo),
                                brdPosition);
                        else
                            BoardCanvas.Dispatcher.BeginInvoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new ComputerPlayer.MoveToDel(ChosenPawn.MoveTo),
                                brdPosition);


                        PieceChosen = false;
                        ChosenPawn.BeingMoved.Value = false;
                        ChosenPawn.UndecoratePawn();
                        ChosenPawn = null;
                        taken = true;
                        break;
                    }
                }
            }
            return taken;
        }

        public void VisualUpdate()
        {
            bool previousAnimatedMoves = AnimatedMoves.Value;
            AnimatedMoves.Value = false;
            FieldSize = (int)(BoardCanvas.Width * (1.0 - 2 * BoardMargin) / NumberOfFields);
            PawnSize = FieldSize * PawnSizeToFieldSizeFactor;

            ShadowGenerator.Width = ShadowGenerator.Height = FieldSize * NumberOfFields + 2;

            DropShadowBitmapEffect ShadowGeneratorEffect = new DropShadowBitmapEffect();
            ShadowGeneratorEffect.Color = Colors.Silver;
            ShadowGeneratorEffect.Direction = 300;
            ShadowGeneratorEffect.ShadowDepth = BoardCanvas.Height * BoardMargin / 3.0;
            ShadowGeneratorEffect.Softness = 0.9;
            ShadowGeneratorEffect.Opacity = 0.7;
            ShadowGenerator.BitmapEffect = ShadowGeneratorEffect;
            Canvas.SetTop(ShadowGenerator, BoardCanvas.Height * BoardMargin - 1);
            Canvas.SetLeft(ShadowGenerator, BoardCanvas.Width * BoardMargin - 1);

            // update fields and pawns
            for (int x = 0; x < NumberOfFields; ++x)
            {
                for (int y = 0; y < NumberOfFields; ++y)
                {
                    Fields[x, y].Width = Fields[x, y].Height = FieldSize;
                    Canvas.SetTop(Fields[x, y], BoardCanvas.Height * BoardMargin + FieldSize * y);
                    Canvas.SetLeft(Fields[x, y], BoardCanvas.Width * BoardMargin + FieldSize * x);
                    if (Pawns[x, y] != null) Pawns[x, y].VisualUpdate();
                }
            }
            AnimatedMoves.Value = previousAnimatedMoves;
        }

        public override void MoveUpdate(Move move)
        {
            throw new System.NotImplementedException();
        }

        public delegate void DeletePawnDelegate(int x, int y);
        public void DeletePawn(int x, int y)
        {
            if (Pawns[x, y] == null) return;
            Pawns[x, y].Dispose();
            Pawns[x, y] = null;
        }

        public delegate void PlacePawnDelegate(int x, int y, Player player);
        public void PlacePawn(int x, int y, Player player)
        {
            bool add = (Pawns[x, y] == null) || ((Pawns[x, y] != null) && (Pawns[x, y].Owner != player));
            DeletePawn(x, y);
            if (add)
            {
                Pawns[x, y] = new VisualPawn(player, new Pair(x, y), BoardCanvas, PawnSize, this);
                Pawns[x, y].VisualUpdate();
            }
        }
    }
}
