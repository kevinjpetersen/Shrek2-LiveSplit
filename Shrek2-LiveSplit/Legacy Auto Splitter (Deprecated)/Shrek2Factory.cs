using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using LiveSplit.Model;

namespace Shrek2_LiveSplit
{
    public class Shrek2Factory : IComponentFactory
    {
        public string ComponentName
        {
            get { return Shrek2Variables.ComponentName; }
        }

        public string Description
        {
            get { return $"LiveSplit Auto Splitter for {Shrek2Variables.ComponentName} (PC)"; }
        }

        public ComponentCategory Category
        {
            get { return ComponentCategory.Control; }
        }

        public IComponent Create(LiveSplitState state)
        {
            return new Shrek2Component(state);
        }

        public string UpdateName
        {
            get { return this.ComponentName; }
        }

        public string UpdateURL
        {
            get { return Shrek2Variables.UpdateUrl; }
        }

        public Version Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public string XMLURL
        {
            get { return this.UpdateURL + "Components/update.Shrek2-LiveSplit.xml"; }
        }
    }
}
