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
            this.flp_AnyPercentSplits.Controls.Remove(chkExemple);
            this.lWarningNbrAutoSplit.Visible = false;
            Maps = new Dictionary<string, bool>();
            AddMapCheckBoxes();

            this.chkAutoStart.DataBindings.Add("Checked", this, "AutoStart", false, DataSourceUpdateMode.OnPropertyChanged);
            this.chkAutoReset.DataBindings.Add("Checked", this, "AutoReset", false, DataSourceUpdateMode.OnPropertyChanged);

            // defaults
            this.AutoStart = DEFAULT_AUTOSTART;
            this.AutoReset = DEFAULT_AUTORESET;
            foreach (string map in GameMemory.splits)
            {
                Maps[map] = DEFAULT_MAP;
                CheckBox checkbox = this.flp_AnyPercentSplits.Controls.Find("chk_" + map, true)[0] as CheckBox;
                checkbox.Checked = Maps[map];
            }
        }

        private void Settings_OnLoad(object sender, EventArgs e)
        {
            CheckNumberAutoSplits();
        }

        private void AddMapCheckBoxes()
        {
            foreach (string map in GameMemory.splits)
            {
                Maps.Add(map, DEFAULT_MAP);
                CheckBox checkbox = new CheckBox();
                checkbox.AutoSize = true;
                checkbox.Dock = DockStyle.Top;
                checkbox.Location = new System.Drawing.Point(3, 3);
                checkbox.Margin = new Padding(3, 3, 50, 3);
                checkbox.Name = "chk_" + map;
                checkbox.Size = new System.Drawing.Size(78, 17);
                checkbox.Text = map.Replace('_', ' ');
                checkbox.UseVisualStyleBackColor = true;
                this.flp_AnyPercentSplits.Controls.Add(checkbox);

                checkbox.CheckedChanged += OnMapCheckBoxChange;
            }
        }

        public XmlNode GetSettings(XmlDocument doc)
        {
            XmlElement settingsNode = doc.CreateElement("Settings");

            settingsNode.AppendChild(ToElement(doc, "Version", Assembly.GetExecutingAssembly().GetName().Version.ToString(3)));
            settingsNode.AppendChild(ToElement(doc, "AutoStart", this.AutoStart));
            settingsNode.AppendChild(ToElement(doc, "AutoReset", this.AutoReset));

            XmlElement mapsNode = doc.CreateElement("Maps");

            foreach (string map in GameMemory.splits)
            {
                mapsNode.AppendChild(ToElement(doc, MAP_PREFIX + map, this.Maps[map]));
            }
            settingsNode.AppendChild(mapsNode);

            return settingsNode;
        }

        public void SetSettings(XmlNode settings)
        {
            var element = (XmlElement)settings;

            this.AutoStart = ParseBool(settings, "AutoStart", DEFAULT_AUTOSTART);
            this.AutoReset = ParseBool(settings, "AutoReset", DEFAULT_AUTOSTART);

            XmlElement mapsNode = settings["Maps"];
            foreach (string map in GameMemory.splits)
            {
                    Maps[map] = (mapsNode != null) ? ParseBool(mapsNode, MAP_PREFIX + map, DEFAULT_MAP) : DEFAULT_MAP;
                    CheckBox checkbox = this.flp_AnyPercentSplits.Controls.Find("chk_" + map, true)[0] as CheckBox;
                    checkbox.Checked = Maps[map];
            }
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

        private void llCheckAll_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!AreAllEnabledCheckboxChecked(flp_AnyPercentSplits.Controls))
            {
                CheckAll(flp_AnyPercentSplits.Controls);
            }
            else
            {
                CheckAll(flp_AnyPercentSplits.Controls, false);
            }
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

        private void CheckAll(ControlCollection collection, bool state = true)
        {
            isCheckingAll = true;
            foreach (Control c in collection)
            {
                if (c is CheckBox)
                {
                    CheckBox checkBox = (CheckBox)c;
                    if (checkBox.Checked != state && checkBox.Enabled)
                    {
                        checkBox.Checked = state;
                    }
                }
            }

            CheckNumberAutoSplits(); //disabled for CheckedChanged event for performance so manual check needed
            isCheckingAll = false;
        }

        private void CheckNumberAutoSplits()
        {

            uint checkedCount = 0;
            foreach (Control c in flp_AnyPercentSplits.Controls)
            {
                if (c is CheckBox)
                {
                    CheckBox cB = (CheckBox)c;
                    if (cB.Checked)
                        checkedCount++;
                }
            }

            if (checkedCount != 0 && _state.Run.Count != checkedCount)
            {
                this.lWarningNbrAutoSplit.Text = "The number of enabled autosplits and segments in your splits don't match!\nAutoSplits: " + checkedCount + " Segments: " + _state.Run.Count;
                this.lWarningNbrAutoSplit.Visible = true;
            }
            else
                this.lWarningNbrAutoSplit.Visible = false;
        }

        private void OnMapCheckBoxChange(object sender, EventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            Maps[checkbox.Name.Substring("chk_".Length)] = checkbox.Checked;

            if (!isCheckingAll) //checking all the checkboxes is very slow otherwise
                CheckNumberAutoSplits();
        }
    }
}
