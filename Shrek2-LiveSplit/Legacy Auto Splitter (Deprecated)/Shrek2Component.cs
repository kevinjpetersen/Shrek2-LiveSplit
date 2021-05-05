using LiveSplit.Model;
using LiveSplit.TimeFormatters;
using LiveSplit.UI.Components;
using LiveSplit.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Linq;

namespace Shrek2_LiveSplit
{
    class Shrek2Component : LogicComponent
    {
        public override string ComponentName
        {
            get { return Shrek2Variables.ComponentName; }
        }

        public Shrek2Settings Settings { get; set; }
        private bool IsDisposed { get; set; }
        private bool IsDebug { get; set; }

        private TimerModel Timer { get; set; }
        private Shrek2GameLogic GameLogic { get; set; }
        private LiveSplitState LSS { get; set; }


        public Shrek2Component(LiveSplitState lss)
        {
            #if DEBUG
                IsDebug = true;
            #endif

            Trace.WriteLine($"[NoLoads] Using {Shrek2Variables.ComponentName} component version " + Assembly.GetExecutingAssembly().GetName().Version + " " + (IsDebug ? "Debug" : "Release") + " build");
            LSS = lss;

            Settings = new Shrek2Settings(lss);
            Timer = new TimerModel { CurrentState = lss };

            GameLogic = new Shrek2GameLogic();
            GameLogic.OnMainMenuLoad += GameLogic_OnMainMenuLoad;
            GameLogic.OnNewGame += GameLogic_OnNewGame;
            GameLogic.OnSplitCompleted += GameLogic_OnSplitCompleted;
            GameLogic.OnLoadStart += GameLogic_OnLoadStart;
            GameLogic.OnLoadEnd += GameLogic_OnLoadEnd;
            lss.OnStart += State_OnStart;
            GameLogic.StartMonitoring();
        }

        public override void Dispose()
        {
            this.IsDisposed = true;

            LSS.OnStart -= State_OnStart;

            if (GameLogic != null)
            {
                GameLogic.Stop();
            }
        }

        void State_OnStart(object sender, EventArgs e)
        {
            Timer.InitializeGameTime();
            GameLogic.ResetSplitStates();
        }

        void GameLogic_OnMainMenuLoad(object sender, EventArgs e)
        {
            if ((LSS.CurrentPhase == TimerPhase.Running || LSS.CurrentPhase == TimerPhase.Ended) && Settings.AutoReset)
            {
                Trace.WriteLine(String.Format("[NoLoads] Reset - {0}", GameLogic.FrameCounter));
                Timer.Reset();
            }
        }

        void GameLogic_OnNewGame(object sender, EventArgs e)
        {
            if (LSS.CurrentPhase == TimerPhase.NotRunning && Settings.AutoStart)
            {
                Trace.WriteLine(String.Format("[NoLoads] Start - {0}", GameLogic.FrameCounter));
                Timer.InitializeGameTime();

                Timer.Start();
            Timer.InitializeGameTime();

            }
        }

        void GameLogic_OnSplitCompleted(object sender, string split)
        {
            int index = Shrek2Splits.Splits.Select(p => p.ID).ToList().IndexOf(split);
            Debug.WriteLineIf(split != "", String.Format("[NoLoads] Trying to split {0}, State: {1} - {2}", split, GameLogic.SplitStates[index], GameLogic.FrameCounter));

            if (Settings.Maps[split])
            {
                if (LSS.CurrentPhase != TimerPhase.NotRunning && !GameLogic.SplitStates[index])
                {
                    Trace.WriteLine(String.Format("[NoLoads] {0} Split - {1}", split, GameLogic.FrameCounter));
                    Timer.Split();
                    GameLogic.SplitStates[index] = true;

                }
                else if (LSS.CurrentPhase != TimerPhase.Running)
                    Debug.WriteLine(String.Format("[NoLoads] Didn't split. Reason: timer isn't running", GameLogic.FrameCounter));
            }
            else
                Debug.WriteLine("[NoLoads] Didn't split " + split + ". Reason: not enabled in the settings");
        }

        void GameLogic_OnLoadStart(object sender, EventArgs e)
        {
            LSS.IsGameTimePaused = true;
        }

        void GameLogic_OnLoadEnd(object sender, EventArgs e)
        {
            LSS.IsGameTimePaused = false;
        }

        public override XmlNode GetSettings(XmlDocument document)
        {
            return this.Settings.GetSettings(document);
        }

        public override Control GetSettingsControl(LayoutMode mode)
        {
            return this.Settings;
        }

        public override void SetSettings(XmlNode settings)
        {
            this.Settings.SetSettings(settings);
        }

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode) { }
    }
}
