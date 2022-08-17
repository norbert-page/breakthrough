using System;
using System.Collections.Generic;
using System.Text;

namespace BreakthroughWPF
{
    [Flags]
    public enum GameState
    {
        None = 0,
        Started = 1,
        Paused = 2,
        Finished = 4,
        AwaitingMove = 8,
        BeingModified = 16,
        StartedAndPaused = Started | Paused,
        StartedAndAwaitingMove = Started | AwaitingMove
    }
}
