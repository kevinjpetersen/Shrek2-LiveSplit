using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using Shrek2_LiveSplit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Shrek2_LiveSplit.Data
{
    public class Shrek2Component : LogicComponent
    {
        public override string ComponentName
        {
            get { return Shrek2Variables.ComponentName; }
        }

        public Shrek2Settings Settings { get; set; }

        private Shrek2AutoSplitter AutoSplitter { get; set; }
        private TimerModel Timer { get; set; }
        private LiveSplitState LSS { get; set; }

        public Shrek2Component(LiveSplitState lss)
        {
            LSS = lss;
            

            Settings = new Shrek2Settings(lss);
            Timer = new TimerModel { CurrentState = lss };

            AutoSplitter = new Shrek2AutoSplitter();
            AutoSplitter.OnMainMenuLoad = OnMainMenuLoad;
            AutoSplitter.OnNewGame = OnNewGame;
            AutoSplitter.OnSplit = OnSplit;
            AutoSplitter.OnPause = OnPause;
            AutoSplitter.OnResume = OnResume;
            LSS.OnStart += OnStart;
            AutoSplitter.Start();
        }

        public override void Dispose()
        {
            LSS.OnStart -= OnStart;
            if (AutoSplitter != null) AutoSplitter.Stop();
        }

        void OnStart(object sender, EventArgs e)
        {

        }

        void OnMainMenuLoad()
        {
            if ((LSS.CurrentPhase == TimerPhase.Running || LSS.CurrentPhase == TimerPhase.Ended || LSS.CurrentPhase == TimerPhase.Paused) && Settings.AutoReset)
            {
                Timer.Reset();
            }
        }

        void OnNewGame()
        {
            if (LSS.CurrentPhase == TimerPhase.NotRunning && Settings.AutoStart)
            {
                Timer.Start();
            }
        }

        void OnSplit()
        {
            if (LSS.CurrentPhase == TimerPhase.NotRunning) return;
            if (LSS.CurrentPhase == TimerPhase.Ended) return;
            OnResume();
            Timer.Split();

            if (LSS.CurrentPhase == TimerPhase.Ended) return;
            OnPause();
        }

        void OnPause()
        {
            if (LSS.CurrentPhase == TimerPhase.Ended) return;
            if (LSS.CurrentPhase == TimerPhase.Paused) return;
            Timer.CurrentState.TimePausedAt = Timer.CurrentState.CurrentTime.RealTime.Value;
            Timer.CurrentState.CurrentPhase = TimerPhase.Paused;
        }

        void OnResume()
        {
            if (LSS.CurrentPhase == TimerPhase.Ended) return;
            if (LSS.CurrentPhase != TimerPhase.Paused) return;
            Timer.CurrentState.AdjustedStartTime = TimeStamp.Now - Timer.CurrentState.TimePausedAt;
            Timer.CurrentState.CurrentPhase = TimerPhase.Running;
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

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode) 
        {
            
        }
    }
}
