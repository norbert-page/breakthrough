using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BreakthroughWPF
{
    public abstract class Player
    {
        public PiecesColor PiecesColor;
        public Move LastMove;

        public ReaderWriterCustomLock<bool> Terminate = new ReaderWriterCustomLock<bool>(false);
        public ReaderWriterCustomLock<bool> Terminated = new ReaderWriterCustomLock<bool>(false);

        private bool m_MoveMade;
        public bool MoveMade
        {
            get
            {
                try
                {
                    MoveMadeMutex.WaitOne();
                    return m_MoveMade;
                }
                finally
                {
                    MoveMadeMutex.ReleaseMutex();
                }
            }
            set
            {
                try
                {
                    MoveMadeMutex.WaitOne();
                    m_MoveMade = value;
                }
                finally
                {
                    MoveMadeMutex.ReleaseMutex();
                }
            }
        }
        private Mutex MoveMadeMutex;
        public Chessboard Gameboard;

        public string TerminatedStr()
        {
            return "! " + PiecesColor.ToString() + " terminated";
        }

        public Player(PiecesColor piecesColor)
        {
            PiecesColor = piecesColor;
            LastMove = null;
            MoveMadeMutex = new Mutex();
        }

        public virtual void SetGameboard(Chessboard ch) {}

        public abstract Move NextMove();
    }
}
