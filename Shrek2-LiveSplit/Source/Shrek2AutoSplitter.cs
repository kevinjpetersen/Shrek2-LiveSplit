using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shrek2_LiveSplit.Data;
using Shrek2_LiveSplit.Game;

namespace Shrek2_LiveSplit
{
    public class Shrek2AutoSplitter
    {
        // Main
        private Shrek2Game Game { get; set; }
        private Task AutoSplitterThread { get; set; }
        private CancellationTokenSource AutoSplitterCancellationToken { get; set; }
        private SynchronizationContext UIThread { get; set; }

        // Events
        public Action OnMainMenuLoad;
        public Action OnNewGame;
        public Action OnSplit;
        public Action OnPause;
        public Action OnResume;

        public bool IsTimerGoing { get; set; } = false;

        public Shrek2AutoSplitter()
        {
            Game = new Shrek2Game();
        }

        public void Start()
        {
            if (AutoSplitterThread != null && AutoSplitterThread.Status == TaskStatus.Running) return;
            if (!(SynchronizationContext.Current is WindowsFormsSynchronizationContext)) throw new InvalidOperationException("SynchronizationContext.Current is not a UI thread.");

            UIThread = SynchronizationContext.Current;
            AutoSplitterCancellationToken = new CancellationTokenSource();
            AutoSplitterThread = Task.Factory.StartNew(AutoSplitterLogic);
        }

        public void Stop()
        {
            if (AutoSplitterCancellationToken == null || AutoSplitterThread == null || AutoSplitterThread.Status != TaskStatus.Running) return;

            AutoSplitterCancellationToken.Cancel();
            AutoSplitterThread.Wait();
        }

        private void AutoSplitterLogic()
        {
            while (!AutoSplitterCancellationToken.IsCancellationRequested)
            {
                while ((Game.GameProcess = Shrek2Utils.GetGameProcess()) == null)
                {
                    Task.Delay(250);
                    if (AutoSplitterCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }

                if (Game.GameProcess == null || Game.GameProcess.HasExited)
                {
                    Task.Delay(Shrek2Variables.SleepTime);
                    continue;
                }

                var gameState = Game.GetGameState();

                if(gameState == Shrek2Game.GameState.NewGame)
                {
                    IsTimerGoing = true;
                    UIThread.Post(d => {
                        OnNewGame();
                    }, null);
                } 
                else if(gameState == Shrek2Game.GameState.Reset)
                {
                    IsTimerGoing = false;
                    UIThread.Post(d => {
                        OnMainMenuLoad();
                    }, null);
                } 
                else if(gameState == Shrek2Game.GameState.Split)
                {
                    UIThread.Post(d => {
                        OnSplit();
                    }, null);
                }

                if (!IsTimerGoing) continue;

                var logLines = Game.GetCurrentCutLogLines();
                if(logLines == null || logLines.Count <= 0)
                {
                    Task.Delay(Shrek2Variables.SleepTime);
                    continue;
                }

                foreach(var line in logLines)
                {
                    var loadlessState = Game.GetLoadlessState(line);

                    if(loadlessState == Shrek2Game.GameState.Pause)
                    {
                        UIThread.Post(d => {
                            OnPause();
                        }, null);
                    }
                    else if(loadlessState == Shrek2Game.GameState.Resume)
                    {
                        UIThread.Post(d => {
                            OnResume();
                        }, null);
                    }
                    else if(loadlessState == Shrek2Game.GameState.Split)
                    {
                        UIThread.Post(d => {
                            OnSplit();
                        }, null);
                    }
                }

                Task.Delay(Shrek2Variables.SleepTime);
            }
        }
    }
}
