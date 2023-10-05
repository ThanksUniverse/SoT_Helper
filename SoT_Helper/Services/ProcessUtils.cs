using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Reflection;
using System.Numerics;
using System.Net.NetworkInformation;
using System.Configuration;

namespace SoT_Helper.Services
{
    public static class ProcessUtils
    {
        private static Process _process;
        private static IntPtr _windowHandle;

        public static int Width { get; private set; }
        public static int Height { get; private set; }
        public static int X { get; private set; }
        public static int Y { get; private set; }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,

            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static bool TryGetProcess(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                return false;
            }
            return true;
        }

        //public static bool IsValid(long addr)
        //{
        //    VirtualQueryEx(_process, (IntPtr)addr, out var lpBuffer, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
        //    return lpBuffer.State == 4096;
        //}

        //public static bool IsValid(IntPtr process, long addr)
        //{
        //    VirtualQueryEx(process, (IntPtr)addr, out var lpBuffer, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
        //    return lpBuffer.State == 4096;
        //}

        public static bool GetProcessMainWindowRect(out int width, out int height, out int x, out int y)
        {
            var mainWindowHandle = _windowHandle;
            width = 2560;
            height = 1440;
            x = 0;
            y = 0;
            RECT rect = new RECT();

            if (!GetWindowRect(mainWindowHandle, out rect))
            {
                return false;
            }

            var scaling = getScalingFactor();
            if (scaling == 1)
            {
                scaling = GetDisplayScaleFactor(mainWindowHandle);
            }

            if (scaling != 1)
            {
                width = (int)(((float)rect.Right - (float)rect.Left) / scaling);
                height = (int)(((float)rect.Bottom - (float)rect.Top) / scaling);
                x = (int)(rect.Left / scaling);
                y = (int)(rect.Top / scaling);
            }
            else
            {
                width = rect.Right - rect.Left;
                height = rect.Bottom - rect.Top;
                x = rect.Left;
                y = rect.Top;
            }

            if (width == 0 || height == 0)
            {
                width = 2560;
                height = 1440;
            }
            return true;
        }

        public static Vector2 GetScreenCenter()
        {
            return new Vector2(Width / 2, Height / 2);
        }

        public static Vector2 GetProcessWindowPosition()
        {
            return new Vector2(X, Y);
        }

        public static bool TryGetProcessMainWindow(string processName, out IntPtr windowHandle, out int width, out int height, out int x, out int y)
        {
            windowHandle = IntPtr.Zero;
            width = 2560;
            height = 1440;
            x = 0;
            y = 0;

            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                return false;
            }
            _process = processes[0];
            IntPtr mainWindowHandle = processes[0].MainWindowHandle;
            var p = processes[0];
            var id = p.Id;
            RECT rect = new RECT();

            if (mainWindowHandle == IntPtr.Zero)
            {
                var windows = GetProcessWindows(processes[0].Id);
                if (windows.Length > 1)
                {
                    foreach(var window in windows)
                    {
                        if (window != IntPtr.Zero)
                        {
                            mainWindowHandle = window;
                            if (GetWindowRect(mainWindowHandle, out rect) && rect.Right>0)
                                break;
                        }
                    }
                    mainWindowHandle = windows[1];
                }
                
                if(rect.Right == 0)
                {
                    mainWindowHandle = FindWindow(null, "Sea of Thieves");
                    if (mainWindowHandle == IntPtr.Zero)
                        mainWindowHandle = FindWindow(null, "Sea of Thieves Insider");

                }
                if (!GetWindowRect(mainWindowHandle, out rect) || rect.Right == 0)
                    return false;
            }

            if (!GetWindowRect(mainWindowHandle, out rect))
            {
                return false;
            }
            var rect2 = rect;
            if (!GetClientRect(mainWindowHandle, out rect))
            {
                return false;
            }
            else
            {
                width = rect.Right - rect.Left;
                height = rect.Bottom - rect.Top;

                Point topLeft = new Point(rect.Left, rect.Top);
                ClientToScreen(mainWindowHandle, ref topLeft);

                Point bottomRight = new Point(rect.Right, rect.Bottom);
                ClientToScreen(mainWindowHandle, ref bottomRight);
                rect.Left = topLeft.X;
                rect.Top = topLeft.Y;
                rect.Right = rect2.Right;
                rect.Bottom = rect2.Bottom;
            }

            var scaling = getScalingFactor();
            if(scaling == 1)
            {
                scaling = GetDisplayScaleFactor(mainWindowHandle);
            }

            if (scaling != 1 && !bool.Parse(ConfigurationManager.AppSettings["FormsOverlay"]))
            {
                width = (int)(((float)rect.Right - (float)rect.Left) / scaling);
                height = (int)(((float)rect.Bottom - (float)rect.Top) / scaling);
                x = (int)(rect.Left / scaling);
                y = (int)(rect.Top / scaling);
            }
            else
            {
                width = rect.Right - rect.Left;
                height = rect.Bottom - rect.Top;
                x = rect.Left;
                y = rect.Top;
            }

            if(width == 0 || height == 0)
            {
                width = 2560;
                height = 1440;
            }
            _windowHandle = mainWindowHandle;
            X = x; Y = y; Width = width; Height = height;
            return true;
        }

        public static float GetDisplayScaleFactor(IntPtr windowHandle)
        {
            try
            {
                return GetDpiForWindow(windowHandle) / 96f;
            }
            catch
            {
                // Or fallback to GDI solutions above
                return 1;
            }
        }

        [DllImport("user32.dll")]
        static extern int GetDpiForWindow(IntPtr hWnd);

        public static IntPtr FindWindow(string windowName)
        {
            return FindWindow(null, windowName);
        }

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr parentWindow, IntPtr previousChildWindow, string windowClass, string windowTitle);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr window, out int process);

        public static IntPtr[] GetProcessWindows(int process)
        {
            IntPtr[] apRet = (new IntPtr[256]);
            int iCount = 0;
            IntPtr pLast = IntPtr.Zero;
            do
            {
                pLast = FindWindowEx(IntPtr.Zero, pLast, null, null);
                int iProcess_;
                GetWindowThreadProcessId(pLast, out iProcess_);
                if (iProcess_ == process) apRet[iCount++] = pLast;
            } while (pLast != IntPtr.Zero);
            System.Array.Resize(ref apRet, iCount);
            return apRet;
        }

        public static float getScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }

        public static ProcessModule GetModuleBaseAddress(Process process)
        {
            ProcessModuleCollection modules = process.Modules;
            foreach (ProcessModule module in modules)
            {
                if (module.ModuleName == "SoT_Game.exe")
                {
                    return module;
                }
            }
            return null;
        }

        public static bool IsForegroundWindow()
        {
            return GetForegroundWindow() == _windowHandle;
        }

    }
    /*
     * https://stackoverflow.com/questions/51913872/c-sharp-calling-a-function-from-its-memory-address
    Find target function address (for example using GetProcAddress)
    Define a delegate for that function
    Use Marshal.GetDelegateForFunctionPointer and get delegate for target function so you call it inside your hook function
    Define your hook function (I mean the function that will called instead of target)
    Use Marshal.GetFunctionPointerForDelegate and get function pointer for your hook function
    (Note: assign the delegate to a static field or use GCHandle.Alloc to prevent GC from collecting it which leads to crash)
    Now use Hook class
    This technique isn't thread safe (more info on it), I recommend you to use EasyHook.

    */
    // Author: Moien007
    public unsafe class Hook
    {
        const string KERNEL32 = "kernel32.dll";

        [DllImport(KERNEL32)]
        static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, VirtualProtectionType flNewProtect, out VirtualProtectionType lpflOldProtect);

        private enum VirtualProtectionType : uint
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            Readonly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        private byte[] m_OriginalBytes;

        public IntPtr TargetAddress { get; }
        public IntPtr HookAddress { get; }

        public Hook(IntPtr target, IntPtr hook)
        {
            if (Environment.Is64BitProcess)
                throw new NotSupportedException("X64 not supported, TODO");

            TargetAddress = target;
            HookAddress = hook;

            m_OriginalBytes = new byte[5];
            fixed (byte* p = m_OriginalBytes)
            {
                ProtectionSafeMemoryCopy(new IntPtr(p), target, m_OriginalBytes.Length);
            }
        }

        public void Install()
        {
            var jmp = CreateJMP(TargetAddress, HookAddress);
            fixed (byte* p = jmp)
            {
                ProtectionSafeMemoryCopy(TargetAddress, new IntPtr(p), jmp.Length);
            }
        }

        public void Uninstall()
        {
            fixed (byte* p = m_OriginalBytes)
            {
                ProtectionSafeMemoryCopy(TargetAddress, new IntPtr(p), m_OriginalBytes.Length);
            }
        }

        static void ProtectionSafeMemoryCopy(IntPtr dest, IntPtr source, int count)
        {
            // UIntPtr = size_t
            var bufferSize = new UIntPtr((uint)count);
            VirtualProtectionType oldProtection, temp;

            // unprotect memory to copy buffer
            if (!VirtualProtect(dest, bufferSize, VirtualProtectionType.ExecuteReadWrite, out oldProtection))
                throw new Exception("Failed to unprotect memory.");

            byte* pDest = (byte*)dest;
            byte* pSrc = (byte*)source;

            // copy buffer to address
            for (int i = 0; i < count; i++)
            {
                *(pDest + i) = *(pSrc + i);
            }

            // protect back
            if (!VirtualProtect(dest, bufferSize, oldProtection, out temp))
                throw new Exception("Failed to protect memory.");
        }

        static byte[] CreateJMP(IntPtr from, IntPtr to)
        {
            return CreateJMP(new IntPtr(to.ToInt32() - from.ToInt32() - 5));
        }

        static byte[] CreateJMP(IntPtr relAddr)
        {
            var list = new List<byte>();
            // get bytes of function address
            var funcAddr32 = BitConverter.GetBytes(relAddr.ToInt32());

            // jmp [relative addr] (http://ref.x86asm.net/coder32.html#xE9)
            list.Add(0xE9); // jmp
            list.AddRange(funcAddr32); // func addr

            return list.ToArray();
        }
    }
}
