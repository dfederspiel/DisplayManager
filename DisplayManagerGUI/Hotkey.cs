using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DisplayManagerGUI
{
    public class Hotkey
    {
        public bool Ctrl;

        public bool Alt;

        public bool Shift;

        public bool RemoveKey;

        public Keys Key;

        public string profileName;

        public Hotkey()
        {
            this.RemoveKey = false;
        }

        public void AssignFromKeyEventArgs(KeyEventArgs keyEvents)
        {
            this.Ctrl = keyEvents.Control;
            this.Alt = keyEvents.Alt;
            this.Shift = keyEvents.Shift;
            this.Key = keyEvents.KeyCode;
        }

        public override string ToString()
        {
            List<string> strs = new List<string>();
            if (this.Ctrl)
            {
                strs.Add("CTRL");
            }
            if (this.Alt)
            {
                strs.Add("ALT");
            }
            if (this.Shift)
            {
                strs.Add("SHIFT");
            }
            Keys key = this.Key;
            switch (key)
            {
                case Keys.ShiftKey:
                case Keys.ControlKey:
                case Keys.Menu:
                    {
                        return string.Join(" + ", strs);
                    }
                default:
                    {
                        if (key == Keys.Alt)
                        {
                            return string.Join(" + ", strs);
                        }
                        strs.Add(this.Key.ToString().Replace("Oem", string.Empty));
                        return string.Join(" + ", strs);
                    }
            }
        }
    }
}