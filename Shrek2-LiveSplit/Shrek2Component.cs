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

namespace Shrek2_LiveSplit
{
    class Shrek2Component : LogicComponent
    {
        public override string ComponentName
        {
            get { return "Shrek 2 (Improved)"; }
        }

        public Shrek2Settings Settings { get; set; }
        public bool Disposed { get; private set; }

        private TimerModel _timer;
        private GameMemory _gameMemory;
        private LiveSplitState _state;

        public Shrek2Component(LiveSplitState state)
        {
            bool debug = false;
#if DEBUG
            debug = true;
#endif
            Trace.WriteLine("[NoLoads] Using LiveSplit.Shrek2 component version " + Assembly.GetExecutingAssembly().GetName().Version + " " + ((debug) ? "Debug" : "Release") + " build");
            _state = state;

            this.Settings = new Shrek2Settings(state);

            _timer = new TimerModel { CurrentState = state };

            _gameMemory = new GameMemory();
            _gameMemory.OnMainMenuLoad += gameMemory_OnMainMenuLoad;
            _gameMemory.OnNewGame += gameMemory_OnNewGame;
            _gameMemory.OnSplitCompleted += gameMemory_OnSplitCompleted;
            _gameMemory.OnLoadStart += gameMemory_OnLoadStart;
            _gameMemory.OnLoadEnd += gameMemory_OnLoadEnd;
            state.OnStart += State_OnStart;
            _gameMemory.StartMonitoring();
        }

        public override void Dispose()
        {
            this.Disposed = true;

            _state.OnStart -= State_OnStart;

            if (_gameMemory != null)
            {
                _gameMemory.Stop();
            }
        }

        void State_OnStart(object sender, EventArgs e)
        {
            _gameMemory.resetSplitStates();
        }

        void gameMemory_OnMainMenuLoad(object sender, EventArgs e)
        {
            if ((_state.CurrentPhase == TimerPhase.Running || _state.CurrentPhase == TimerPhase.Ended) && this.Settings.AutoReset)
            {
                Trace.WriteLine(String.Format("[NoLoads] Reset - {0}", _gameMemory.frameCounter));
                _timer.Reset();
            }
        }

        void gameMemory_OnNewGame(object sender, EventArgs e)
        {
            if (_state.CurrentPhase == TimerPhase.NotRunning && this.Settings.AutoStart)
            {
                Trace.WriteLine(String.Format("[NoLoads] Start - {0}", _gameMemory.frameCounter));
                _timer.Start();
            }
        }

        void gameMemory_OnSplitCompleted(object sender, string split)
        {
            int index = GameMemory.splits.IndexOf(split);
            Debug.WriteLineIf(split != "", String.Format("[NoLoads] Trying to split {0}, State: {1} - {2}", split, _gameMemory.SplitStates[index], _gameMemory.frameCounter));

            if (Settings.Maps[split])
            {
                if (_state.CurrentPhase != TimerPhase.NotRunning && !_gameMemory.SplitStates[index])
                {
                    Trace.WriteLine(String.Format("[NoLoads] {0} Split - {1}", split, _gameMemory.frameCounter));
                    _timer.Split();
                    _gameMemory.SplitStates[index] = true;

                }
                else if (_state.CurrentPhase != TimerPhase.Running)
                    Debug.WriteLine(String.Format("[NoLoads] Didn't split. Reason: timer isn't running", _gameMemory.frameCounter));
            }
            else
                Debug.WriteLine("[NoLoads] Didn't split " + split + ". Reason: not enabled in the settings");
        }

        void gameMemory_OnLoadStart(object sender, EventArgs e)
        {
            _state.IsGameTimePaused = true;
        }

        void gameMemory_OnLoadEnd(object sender, EventArgs e)
        {
            _state.IsGameTimePaused = false;
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
