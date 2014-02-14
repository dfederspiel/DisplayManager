using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace DisplayManagerGUI
{
    internal class GlobalKeyboardHook
    {
        private const int WH_KEYBOARD_LL = 13;

        private const int WM_KEYDOWN = 256;

        private const int WM_KEYUP = 257;

        private const int WM_SYSKEYDOWN = 260;

        private const int WM_SYSKEYUP = 261;

        private const int VK_SHIFT = 16;

        private const int VK_CONTROL = 17;

        private const int VK_MENU = 18;

        private const int VK_CAPITAL = 20;

        public List<Hotkey> HookedKeys = new List<Hotkey>();

        private IntPtr hhook = IntPtr.Zero;

        public GlobalKeyboardHook()
        {
            this.hook();
        }

        private Keys AddModifiers(Keys key)
        {
            if ((GlobalKeyboardHook.GetKeyState(20) & 1) != 0)
            {
                key = key | Keys.Capital;
            }
            if ((GlobalKeyboardHook.GetKeyState(16) & 32768) != 0)
            {
                key = key | Keys.Shift;
            }
            if ((GlobalKeyboardHook.GetKeyState(17) & 32768) != 0)
            {
                key = key | Keys.Control;
            }
            if ((GlobalKeyboardHook.GetKeyState(18) & 32768) != 0)
            {
                key = key | Keys.Alt;
            }
            return key;
        }

        [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref GlobalKeyboardHook.keyboardHookStruct lParam);

        ~GlobalKeyboardHook()
        {
            this.unhook();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern short GetKeyState(int keyCode);

        public void hook()
        {
            IntPtr intPtr = GlobalKeyboardHook.LoadLibrary("User32");
            this.hhook = GlobalKeyboardHook.SetWindowsHookEx(13, new GlobalKeyboardHook.keyboardHookProc(this.hookProc), intPtr, 0);
        }

        public int hookProc(int code, int wParam, ref GlobalKeyboardHook.keyboardHookStruct lParam)
        {
            int num;
            if (code >= 0)
            {
                Keys key = (Keys)lParam.vkCode;
                List<Hotkey>.Enumerator enumerator = this.HookedKeys.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        Hotkey current = enumerator.Current;
                        if (current.Key != key)
                        {
                            continue;
                        }
                        key = this.AddModifiers(key);
                        KeyEventArgs keyEventArg = new KeyEventArgs(key);
                        if ((wParam == 257 || wParam == 261) && this.KeyUp != null && current.Alt == keyEventArg.Alt && current.Shift == keyEventArg.Shift && current.Ctrl == keyEventArg.Control)
                        {
                            this.KeyUp(current, keyEventArg);
                        }
                        if (!keyEventArg.Handled)
                        {
                            continue;
                        }
                        num = 1;
                        return num;
                    }
                    return GlobalKeyboardHook.CallNextHookEx(this.hhook, code, wParam, ref lParam);
                }
                finally
                {
                    ((IDisposable)enumerator).Dispose();
                }
                return num;
            }
            return GlobalKeyboardHook.CallNextHookEx(this.hhook, code, wParam, ref lParam);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern IntPtr SetWindowsHookEx(int idHook, GlobalKeyboardHook.keyboardHookProc callback, IntPtr hInstance, uint threadId);

        public void unhook()
        {
            GlobalKeyboardHook.UnhookWindowsHookEx(this.hhook);
        }

        [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        public event KeyEventHandler KeyDown;

        public event KeyEventHandler KeyUp;

        public delegate int keyboardHookProc(int code, int wParam, ref GlobalKeyboardHook.keyboardHookStruct lParam);

        public struct keyboardHookStruct
        {
            public int vkCode;

            public int scanCode;

            public int flags;

            public int time;

            public int dwExtraInfo;
        }
    }
}