using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static SoT_Helper.Services.InputHelper;

namespace SoT_Helper.Services
{
    public class InputHelper
    {
        #region Imports/Structs/Enums
        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public HardwareInput hi;
        }

        public struct Input
        {
            public int type;
            public InputUnion u;
        }

        [Flags]
        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum KeyEventF
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008
        }

        [Flags]
        public enum MouseEventF
        {
            Absolute = 0x8000,
            HWheel = 0x01000,
            Move = 0x0001,
            MoveNoCoalesce = 0x2000,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            VirtualDesk = 0x4000,
            Wheel = 0x0800,
            XDown = 0x0080,
            XUp = 0x0100
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);


        //public enum KeyList
        //{
        //    W = 0x11, // 0x11 = what int 17 is in hex
        //    A = 0x1E, // 0x1E = what int 30 is in hex
        //    S = 0x1F, 
        //    D = 0x20,
        //    E = 0x12,
        //    space = 0x39, 
        //    F = 0x21,
        //    mouseLeft = 0x01,
        //    mouseRight = 0x02,
        //    None = 0x00
        //}

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public uint Type;
            public MOUSEINPUT Data;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public int dwExtraInfo;
        }

        private const int INPUT_MOUSE = 0;
        private const int MOUSEEVENTF_MOVE = 0x0001;

        public static void MoveMouse(int dx, int dy)
        {
            //INPUT[] inputs = new INPUT[]
            //{
            //new INPUT
            //{
            //    Type = INPUT_MOUSE,
            //    Data = new MOUSEINPUT
            //    {
            //        dx = dx,
            //        dy = dy,
            //        dwFlags = MOUSEEVENTF_MOVE
            //    }
            //}
            //};

            //SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            mouse_event((uint)MouseEventF.Move, (uint)dx, (uint)dy, 0, 0);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        #endregion

        #region Wrapper Methods
        public static POINT GetCursorPosition()
        {
            GetCursorPos(out POINT point);
            return point;
        }

        public static void SetCursorPosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

        //public static void MoveMouse(int x, int y)
        //{
        //    SetCursorPos(x, y);
        //}

        public static Vector2 GetCursorPositionVector2()
        {
            GetCursorPos(out POINT point);
            return new Vector2(point.X, point.Y);
        }

        public static void MoveMouseRelative(int dx, int dy)
        {
            POINT point;
            GetCursorPos(out point);

            SetCursorPos(point.X + dx, point.Y + dy);
        }

        //public static void SendKeyboardInput(KeyboardInput[] kbInputs)
        //{
        //    Input[] inputs = new Input[kbInputs.Length];

        //    for (int i = 0; i < kbInputs.Length; i++)
        //    {
        //        inputs[i] = new Input
        //        {
        //            type = (int)InputType.Keyboard,
        //            u = new InputUnion
        //            {
        //                ki = kbInputs[i]
        //            }
        //        };
        //    }
        //}

        //public static Keys PressedKey;

        //public static void ClickKey(Keys key)
        //{
        //    //ClickKey((ushort)key);
        //    if(PressedKey == null)
        //    {
        //        PressedKey = key;
        //    }
        //    var inputs = new KeyboardInput[]
        //    {
        //        new KeyboardInput
        //        {
        //            wScan = (ushort)PressedKey,
        //            dwFlags = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode),
        //            dwExtraInfo = GetMessageExtraInfo()
        //        },
        //        new KeyboardInput
        //        {
        //            wScan = (ushort)key,
        //            dwFlags = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode),
        //            dwExtraInfo = GetMessageExtraInfo()
        //        }
        //    };
        //    SendKeyboardInput(inputs);

        //}

        //public static void ClickKey(ushort scanCode)
        //{
        //    var inputs = new KeyboardInput[]
        //    {
        //    new KeyboardInput
        //    {
        //        wScan = scanCode,
        //        dwFlags = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode),
        //        dwExtraInfo = GetMessageExtraInfo()
        //    },
        //    new KeyboardInput
        //    {
        //        wScan = scanCode,
        //        dwFlags = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode),
        //        dwExtraInfo = GetMessageExtraInfo()
        //    }
        //    };
        //    SendKeyboardInput(inputs);
        //}

        public static void PressMouseKey(MouseEventF key = MouseEventF.RightDown)
        {
            mouse_event((uint)key, 0, 0, 0, 0);
        }

        //public static void PressAndHoldMouseKey(MouseEventF key, int durationMilliseconds)
        //{
        //    mouse_event((uint)key, 0, 0, 0, 0);
        //    Thread.Sleep(durationMilliseconds); // Wait with the key down
        //    if(key == MouseEventF.LeftDown)
        //    {
        //        mouse_event((uint)MouseEventF.LeftUp, 0, 0, 0, 0);
        //    }
        //    else if(key == MouseEventF.RightDown)
        //    {
        //        mouse_event((uint)MouseEventF.RightUp, 0, 0, 0, 0);
        //    }
        //    else if(key == MouseEventF.MiddleDown)
        //    {
        //        mouse_event((uint)MouseEventF.MiddleUp, 0, 0, 0, 0);
        //    }
        //    else if(key == MouseEventF.XDown)
        //    {
        //        mouse_event((uint)MouseEventF.XUp, 0, 0, 0, 0);
        //    }
        //}

        [DllImport("user32.dll")]
        static extern void keybd_event(ushort bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        const int KEYEVENTF_KEYUP = 0x0002;
        const byte VK_A = 0x41;

        //public static void PressAndHoldKey(byte virtualKeyCode, int durationMilliseconds)
        //{
        //    // Simulate key down
        //    keybd_event(virtualKeyCode, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);

        //    Thread.Sleep(durationMilliseconds); // Wait with the key down

        //    // Simulate key up
        //    keybd_event(virtualKeyCode, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
        //}

        public static void PressKey(Keys key)
        {
            // Simulate key down
            keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
        }

        public static void PressKey(byte virtualKeyCode)
        {
            // Simulate key down
            keybd_event(virtualKeyCode, 0, KEYEVENTF_EXTENDEDKEY, UIntPtr.Zero);
        }

        public static void ReleaseKey(Keys key)
        {
            // Simulate key up
            keybd_event((byte)key, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public static void ReleaseKey(byte virtualKeyCode)
        {
            // Simulate key up
            keybd_event(virtualKeyCode, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        public static void ReleaseKeys()
        {
            InputHelper.ReleaseKey((byte)Keys.W);
            InputHelper.ReleaseKey((byte)Keys.A);
            InputHelper.ReleaseKey((byte)Keys.S);
            InputHelper.ReleaseKey((byte)Keys.D);
            InputHelper.ReleaseKey((byte)Keys.X);
            InputHelper.ReleaseKey((byte)Keys.K);
            InputHelper.ReleaseKey((byte)Keys.R);
            InputHelper.ReleaseKey((byte)Keys.F);
            InputHelper.ReleaseKey((byte)Keys.D3);
            InputHelper.ReleaseKey((byte)Keys.ControlKey);
            InputHelper.PressMouseKey(InputHelper.MouseEventF.LeftUp);
            InputHelper.PressMouseKey(InputHelper.MouseEventF.RightUp);
        }

        //public static void SendMouseInput(MouseInput[] mInputs)
        //{
        //    Input[] inputs = new Input[mInputs.Length];

        //    for (int i = 0; i < mInputs.Length; i++)
        //    {
        //        inputs[i] = new Input
        //        {
        //            type = (int)InputType.Mouse,
        //            u = new InputUnion
        //            {
        //                mi = mInputs[i]
        //            }
        //        };
        //    }

        //    SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
        //}
        #endregion

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);
    }
}
