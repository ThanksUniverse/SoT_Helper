using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SoT_Helper.Services;
using SoT_Helper;
using System.Reflection;

public class InterceptKeys
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;

    private static Keys LastKey = Keys.None;
    public static bool Active = false;

    public static void RunKeyInterception()
    {
        _hookID = SetHook(_proc);
        Active = true;
        //Application.Run();
        //UnhookWindowsHookEx(_hookID);
    }

    public static void Unload()
    {
        UnhookWindowsHookEx(_hookID);
        Active = false;
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(
        int nCode, IntPtr wParam, IntPtr lParam);

    static long delay = 0;

    private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (!Active) return CallNextHookEx(_hookID, nCode, wParam, lParam);

        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            //Console.WriteLine((Keys)vkCode);
            if (LastKey != (Keys)vkCode)
            {
                if (delay > DateTime.UtcNow.Ticks)
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);

                //SoTHelper.DebugTextBox.Text = $"Key Pressed:{(Keys)vkCode}";
                //SoT_DataManager.InfoLog += $"\nKey Pressed:{(Keys)vkCode}";
                LastKey = (Keys)vkCode;

                if (SoT_DataManager.KeyBindings.Any(b => b.Key == (Keys)vkCode))
                {
                    delay = DateTime.UtcNow.Ticks + TimeSpan.TicksPerMillisecond * 300;
                    LastKey = Keys.None;
                    var keyBinding = SoT_DataManager.KeyBindings.First(b => b.Key == (Keys)vkCode);
                    foreach (var action in keyBinding.Delegates)
                    {
                        action();
                    }
                }
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
}