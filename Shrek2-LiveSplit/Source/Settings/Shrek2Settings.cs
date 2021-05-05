using LiveSplit.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace Shrek2_LiveSplit
{
    public partial class Shrek2Settings : UserControl
    {
        public bool AutoStart { get; set; }
        public bool AutoReset { get; set; }

        public const string MAP_PREFIX = "map_";

        public Dictionary<string, bool> Maps { get; set; }

        private const bool DEFAULT_AUTOSTART = true;
        private const bool DEFAULT_AUTORESET = true;
        private const bool DEFAULT_MAP = true;

        private LiveSplitState _state;
        private bool isCheckingAll;

        public Shrek2Settings(LiveSplitState state)
        {
            InitializeComponent();

            this._state = state;

            this.Load += Settings_OnLoad;

            this.chkAutoStart.DataBindings.Add("Checked", this, "AutoStart", false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkAutoReset.DataBindings.Add("Checked", this, "AutoReset", false, DataSourceUpdateMode.OnPropertyChanged);

            // defaults
            this.AutoStart = DEFAULT_AUTOSTART;
            this.AutoReset = DEFAULT_AUTORESET;
        }

        private void Settings_OnLoad(object sender, EventArgs e)
        {

        }

        public XmlNode GetSettings(XmlDocument doc)
        {
            XmlElement settingsNode = doc.CreateElement("Settings");

            settingsNode.AppendChild(ToElement(doc, "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));
            settingsNode.AppendChild(ToElement(doc, "AutoStart", this.AutoStart));
            settingsNode.AppendChild(ToElement(doc, "AutoReset", this.AutoReset));

            return settingsNode;
        }

        public void SetSettings(XmlNode settings)
        {
            var element = (XmlElement)settings;

            this.AutoStart = ParseBool(settings, "AutoStart", DEFAULT_AUTOSTART);
            this.AutoReset = ParseBool(settings, "AutoReset", DEFAULT_AUTOSTART);
        }

        static bool ParseBool(XmlNode settings, string setting, bool default_ = false)
        {
            bool val;
            return settings[setting] != null ?
                (Boolean.TryParse(settings[setting].InnerText, out val) ? val : default_)
                : default_;
        }

        static XmlElement ToElement<T>(XmlDocument document, string name, T value)
        {
            XmlElement str = document.CreateElement(name);
            str.InnerText = value.ToString();
            return str;
        }

        private bool AreAllEnabledCheckboxChecked(ControlCollection collection, bool state = true)
        {
            foreach (Control c in collection)
            {
                if (c.GetType().Equals(typeof(CheckBox)))
                {
                    CheckBox checkBox = (CheckBox)c;
                    if (checkBox.Checked != state && checkBox.Enabled)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
