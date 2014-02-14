using DisplayManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace DisplayManagerGUI
{
    public class MonitorSwitcherGUI : Form
    {
        private NotifyIcon trayIcon;

        private ContextMenuStrip trayMenu;

        private string settingsDirectory;

        private string settingsDirectoryProfiles;

        private List<Hotkey> Hotkeys;

        private GlobalKeyboardHook KeyHook;

        public MonitorSwitcherGUI()
        {
            this.settingsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MonitorSwitcher");
            this.settingsDirectoryProfiles = Path.Combine(this.settingsDirectory, "Profiles");
            if (!Directory.Exists(this.settingsDirectory))
            {
                Directory.CreateDirectory(this.settingsDirectory);
            }
            if (!Directory.Exists(this.settingsDirectoryProfiles))
            {
                Directory.CreateDirectory(this.settingsDirectoryProfiles);
            }
            this.Hotkeys = new List<Hotkey>();
            this.LoadSettings();
            this.KeyHook = new GlobalKeyboardHook();
            this.KeyHook.KeyDown += new KeyEventHandler(this.KeyHook_KeyDown);
            this.KeyHook.KeyUp += new KeyEventHandler(this.KeyHook_KeyUp);
            this.KeyHooksRefresh();
            this.trayMenu = new ContextMenuStrip()
            {
                ImageList = new ImageList()
            };
            this.trayMenu.ImageList.Images.Add(new Icon(base.GetType(), "MainIcon.ico"));
            this.trayMenu.ImageList.Images.Add(new Icon(base.GetType(), "DeleteProfile.ico"));
            this.trayMenu.ImageList.Images.Add(new Icon(base.GetType(), "Exit.ico"));
            this.trayMenu.ImageList.Images.Add(new Icon(base.GetType(), "Profile.ico"));
            this.trayMenu.ImageList.Images.Add(new Icon(base.GetType(), "SaveProfile.ico"));
            this.trayMenu.ImageList.Images.Add(new Icon(base.GetType(), "NewProfile.ico"));
            this.trayMenu.ImageList.Images.Add(new Icon(base.GetType(), "About.ico"));
            this.trayMenu.ImageList.Images.Add(new Icon(base.GetType(), "Hotkey.ico"));
            this.BuildTrayMenu();
            this.trayIcon = new NotifyIcon()
            {
                Text = "Monitor Profile Switcher",
                Icon = new Icon(base.GetType(), "MainIcon.ico"),
                ContextMenuStrip = this.trayMenu,
                Visible = true
            };
            this.trayIcon.MouseUp += new MouseEventHandler(this.OnTrayClick);
        }

        public void BuildTrayMenu()
        {
            ToolStripItem toolStripItem;
            this.trayMenu.Items.Clear();
            this.trayMenu.Items.Add("Load Profile").Enabled = false;
            this.trayMenu.Items.Add("-");
            string[] files = Directory.GetFiles(this.settingsDirectoryProfiles, "*.xml");
            string[] strArrays = files;
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(strArrays[i]);
                toolStripItem = this.trayMenu.Items.Add(fileNameWithoutExtension);
                toolStripItem.Click += new EventHandler(this.OnMenuLoad);
                toolStripItem.ImageIndex = 3;
            }
            this.trayMenu.Items.Add("-");
            ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem("Save Profile")
            {
                ImageIndex = 4,
                DropDown = new ToolStripDropDownMenu()
                {
                    ImageList = this.trayMenu.ImageList
                }
            };
            this.trayMenu.Items.Add(toolStripMenuItem);
            toolStripItem = toolStripMenuItem.DropDownItems.Add("New Profile...");
            toolStripItem.Click += new EventHandler(this.OnMenuSaveAs);
            toolStripItem.ImageIndex = 5;
            toolStripMenuItem.DropDownItems.Add("-");
            ToolStripMenuItem toolStripMenuItem1 = new ToolStripMenuItem("Delete Profile")
            {
                ImageIndex = 1,
                DropDown = new ToolStripDropDownMenu()
                {
                    ImageList = this.trayMenu.ImageList
                }
            };
            this.trayMenu.Items.Add(toolStripMenuItem1);
            ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem("Set Hotkeys")
            {
                ImageIndex = 7,
                DropDown = new ToolStripDropDownMenu()
                {
                    ImageList = this.trayMenu.ImageList
                }
            };
            this.trayMenu.Items.Add(toolStripMenuItem2);
            string[] strArrays1 = files;
            for (int j = 0; j < (int)strArrays1.Length; j++)
            {
                string str = strArrays1[j];
                string fileNameWithoutExtension1 = Path.GetFileNameWithoutExtension(str);
                toolStripItem = toolStripMenuItem.DropDownItems.Add(fileNameWithoutExtension1);
                toolStripItem.Click += new EventHandler(this.OnMenuSave);
                toolStripItem.ImageIndex = 3;
                toolStripItem = toolStripMenuItem1.DropDownItems.Add(fileNameWithoutExtension1);
                toolStripItem.Click += new EventHandler(this.OnMenuDelete);
                toolStripItem.ImageIndex = 3;
                string str1 = "(No Hotkey)";
                Hotkey hotkey = this.FindHotkey(Path.GetFileNameWithoutExtension(str));
                if (hotkey != null)
                {
                    str1 = string.Concat("(", hotkey.ToString(), ")");
                }
                toolStripItem = toolStripMenuItem2.DropDownItems.Add(string.Concat(fileNameWithoutExtension1, " ", str1));
                toolStripItem.Tag = fileNameWithoutExtension1;
                toolStripItem.Click += new EventHandler(this.OnHotkeySet);
                toolStripItem.ImageIndex = 3;
            }
            this.trayMenu.Items.Add("-");
            toolStripItem = this.trayMenu.Items.Add("About");
            toolStripItem.Click += new EventHandler(this.OnMenuAbout);
            toolStripItem.ImageIndex = 6;
            toolStripItem = this.trayMenu.Items.Add("Exit");
            toolStripItem.Click += new EventHandler(this.OnMenuExit);
            toolStripItem.ImageIndex = 2;
        }

        private static void buttonClear_Click(object sender, EventArgs e)
        {
            TextBox tag = (sender as Button).Tag as TextBox;
            if (tag.Tag != null)
            {
                (tag.Tag as Hotkey).RemoveKey = true;
            }
            tag.Clear();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.trayIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }

        public Hotkey FindHotkey(string name)
        {
            Hotkey hotkey;
            List<Hotkey>.Enumerator enumerator = this.Hotkeys.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Hotkey current = enumerator.Current;
                    if (current.profileName.CompareTo(name) != 0)
                    {
                        continue;
                    }
                    hotkey = current;
                    return hotkey;
                }
                return null;
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            return hotkey;
        }

        public static DialogResult HotkeySetting(string title, string promptText, ref Hotkey value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button button = new Button();
            Button button1 = new Button();
            Button button2 = new Button();
            form.Text = title;
            label.Text = "Press hotkey combination or click 'Clear Hotkey' to remove the current hotkey";
            if (value != null)
            {
                textBox.Text = value.ToString();
            }
            textBox.Tag = value;
            button2.Text = "Clear Hotkey";
            button.Text = "OK";
            button1.Text = "Cancel";
            button.DialogResult = DialogResult.OK;
            button1.DialogResult = DialogResult.Cancel;
            label.SetBounds(9, 10, 372, 13);
            textBox.SetBounds(12, 36, 289, 20);
            button.SetBounds(228, 72, 75, 23);
            button1.SetBounds(309, 72, 75, 23);
            button2.SetBounds(309, 35, 75, 23);
            button2.Tag = textBox;
            button2.Click += new EventHandler(MonitorSwitcherGUI.buttonClear_Click);
            textBox.KeyDown += new KeyEventHandler(MonitorSwitcherGUI.textBox_KeyDown);
            textBox.KeyUp += new KeyEventHandler(MonitorSwitcherGUI.textBox_KeyUp);
            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            form.ClientSize = new Size(396, 107);
            Control.ControlCollection controls = form.Controls;
            Control[] controlArray = new Control[] { label, textBox, button, button1, button2 };
            controls.AddRange(controlArray);
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = button;
            form.CancelButton = button1;
            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Tag as Hotkey;
            return dialogResult;
        }

        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button button = new Button();
            Button button1 = new Button();
            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;
            button.Text = "OK";
            button1.Text = "Cancel";
            button.DialogResult = DialogResult.OK;
            button1.DialogResult = DialogResult.Cancel;
            label.SetBounds(9, 10, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            button.SetBounds(228, 72, 75, 23);
            button1.SetBounds(309, 72, 75, 23);
            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            form.ClientSize = new Size(396, 107);
            Control.ControlCollection controls = form.Controls;
            Control[] controlArray = new Control[] { label, textBox, button, button1 };
            controls.AddRange(controlArray);
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = button;
            form.CancelButton = button1;
            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private void KeyHook_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void KeyHook_KeyUp(object sender, KeyEventArgs e)
        {
            this.LoadProfile((sender as Hotkey).profileName);
            e.Handled = true;
        }

        private void KeyHooksRefresh()
        {
            this.KeyHook.unhook();
            this.KeyHook.HookedKeys.Clear();
            foreach (Hotkey hotkey in this.Hotkeys)
            {
                this.KeyHook.HookedKeys.Add(hotkey);
            }
            this.KeyHook.hook();
        }

        public void LoadProfile(string name)
        {
            if (!MonitorSwitcher.LoadDisplaySettings(this.ProfileFileFromName(name)))
            {
                this.trayIcon.BalloonTipTitle = "Failed to load Multi Monitor profile";
                this.trayIcon.BalloonTipText = string.Concat("MonitorSwitcher was unable to load the previously saved profile \"", name, "\"");
                this.trayIcon.BalloonTipIcon = ToolTipIcon.Error;
                this.trayIcon.ShowBalloonTip(5000);
            }
        }

        public void LoadSettings()
        {
            this.Hotkeys.Clear();
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Hotkey));
            if (!File.Exists(this.SettingsFileFromName("Hotkeys")))
            {
                return;
            }
            try
            {
                XmlReader xmlReader = XmlReader.Create(this.SettingsFileFromName("Hotkeys"));
                xmlReader.Read();
                do
                {
                Label0:
                    if (xmlReader.Name.CompareTo("Hotkey") != 0 || !xmlReader.IsStartElement())
                    {
                        continue;
                    }
                    Hotkey hotkey = (Hotkey)xmlSerializer.Deserialize(xmlReader);
                    this.Hotkeys.Add(hotkey);
                    goto Label0;
                }
                while (xmlReader.Read());
            }
            catch
            {
            }
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MonitorSwitcherGUI());
        }

        public void OnHotkeySet(object sender, EventArgs e)
        {
            this.KeyHook.unhook();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(((ToolStripMenuItem)sender).Tag as string);
            Hotkey hotkey = this.FindHotkey(fileNameWithoutExtension);
            bool flag = false;
            if (hotkey == null)
            {
                flag = true;
            }
            if (MonitorSwitcherGUI.HotkeySetting(string.Concat("Set Hotkey for Monitor Profile '", fileNameWithoutExtension, "'"), "Enter name of new profile", ref hotkey) == DialogResult.OK)
            {
                if (flag && hotkey != null)
                {
                    if (!hotkey.RemoveKey)
                    {
                        hotkey.profileName = fileNameWithoutExtension;
                        this.Hotkeys.Add(hotkey);
                    }
                }
                else if (hotkey != null && hotkey.RemoveKey)
                {
                    this.Hotkeys.Remove(hotkey);
                }
                this.SaveSettings();
            }
            this.KeyHook.hook();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.Visible = false;
            base.ShowInTaskbar = false;
            base.OnLoad(e);
        }

        public void OnMenuAbout(object sender, EventArgs e)
        {
            MessageBox.Show("Monitor Profile Switcher by Martin Krämer \n(MartinKraemer84@gmail.com)\nVersion 0.1.0.0\nCopyright 2013 \n\nhttps://sourceforge.net/projects/monitorswitcher/", "About Monitor Profile Switcher", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        public void OnMenuDelete(object sender, EventArgs e)
        {
            File.Delete(this.ProfileFileFromName(((ToolStripMenuItem)sender).Text));
        }

        private void OnMenuExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void OnMenuLoad(object sender, EventArgs e)
        {
            this.LoadProfile(((ToolStripMenuItem)sender).Text);
        }

        public void OnMenuSave(object sender, EventArgs e)
        {
            if (!MonitorSwitcher.SaveDisplaySettings(this.ProfileFileFromName(((ToolStripMenuItem)sender).Text)))
            {
                this.trayIcon.BalloonTipTitle = "Failed to save Multi Monitor profile";
                this.trayIcon.BalloonTipText = string.Concat("MonitorSwitcher was unable to save the current profile to name\"", ((ToolStripMenuItem)sender).Text, "\"");
                this.trayIcon.BalloonTipIcon = ToolTipIcon.Error;
                this.trayIcon.ShowBalloonTip(5000);
            }
        }

        public void OnMenuSaveAs(object sender, EventArgs e)
        {
            string str = "New Profile";
            this.KeyHook.unhook();
            if (MonitorSwitcherGUI.InputBox("Save as new profile", "Enter name of new profile", ref str) == DialogResult.OK)
            {
                string str1 = string.Concat(new string(Path.GetInvalidFileNameChars()), new string(Path.GetInvalidPathChars()));
                string str2 = str1;
                for (int i = 0; i < str2.Length; i++)
                {
                    char chr = str2[i];
                    str = str.Replace(chr.ToString(), "");
                }
                if (str.Trim().Length > 0 && !MonitorSwitcher.SaveDisplaySettings(this.ProfileFileFromName(str)))
                {
                    this.trayIcon.BalloonTipTitle = "Failed to save Multi Monitor profile";
                    this.trayIcon.BalloonTipText = string.Concat("MonitorSwitcher was unable to save the current profile to a new profile with name\"", str, "\"");
                    this.trayIcon.BalloonTipIcon = ToolTipIcon.Error;
                    this.trayIcon.ShowBalloonTip(5000);
                }
            }
            this.KeyHook.hook();
        }

        public void OnTrayClick(object sender, MouseEventArgs e)
        {
            this.BuildTrayMenu();
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo method = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                method.Invoke(this.trayIcon, null);
            }
        }

        public string ProfileFileFromName(string name)
        {
            string str = string.Concat(name, ".xml");
            return Path.Combine(this.settingsDirectoryProfiles, str);
        }

        public void SaveSettings()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Hotkey));
            XmlWriterSettings xmlWriterSetting = new XmlWriterSettings()
            {
                CloseOutput = true
            };
            try
            {
                using (FileStream fileStream = new FileStream(this.SettingsFileFromName("Hotkeys"), FileMode.Create))
                {
                    XmlWriter xmlWriter = XmlWriter.Create(fileStream, xmlWriterSetting);
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("hotkeys");
                    foreach (Hotkey hotkey in this.Hotkeys)
                    {
                        xmlSerializer.Serialize(xmlWriter, hotkey);
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                    fileStream.Close();
                }
            }
            catch
            {
            }
        }

        public string SettingsFileFromName(string name)
        {
            string str = string.Concat(name, ".xml");
            return Path.Combine(this.settingsDirectory, str);
        }

        private static void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox str = sender as TextBox;
            Hotkey tag = str.Tag as Hotkey ?? new Hotkey();
            tag.AssignFromKeyEventArgs(e);
            e.Handled = true;
            e.SuppressKeyPress = true;
            str.Text = tag.ToString();
            str.Tag = tag;
        }

        private static void textBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Tag != null)
            {
                Hotkey tag = textBox.Tag as Hotkey;
                if (tag.Key < Keys.D0 || !tag.Alt && !tag.Ctrl && !tag.Shift)
                {
                    textBox.Text = "";
                }
            }
        }
    }
}
