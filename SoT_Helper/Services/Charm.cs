using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using Charms.Properties;

namespace SoT_Helper.Services
{

    public class Charm
    {
        public enum CharmResult
        {
            CHARM_SUCCESS,
            CHARM_PROCESS_NONE,
            CHARM_PROCESS_MANY,
            CHARM_NATIVE_NONE,
            CHARM_WINDOW_NONE
        }

        [Flags]
        public enum CharmSettings
        {
            CHARM_REQUIRE_FOREGROUND = 0x1,
            CHARM_DRAW_FPS = 0x2,
            CHARM_VSYNC = 0x4,
            CHARM_FONT_CALIBRI = 0x8,
            CHARM_FONT_ARIAL = 0x10,
            CHARM_FONT_COURIER = 0x20,
            CHARM_FONT_GABRIOLA = 0x40,
            CHARM_FONT_IMPACT = 0x80,
            WPM_WRITE_DIRTY = 0x100
        }

        private delegate void CallbackUnmanaged(int width, int height);

        public delegate void CallbackManaged(RPM rpm, Renderer renderer, int width, int height);

        public class RPM
        {
            private struct MEMORY_BASIC_INFORMATION
            {
                public ulong BaseAddress;

                public ulong AllocationBase;

                public int AllocationProtect;

                public ulong RegionSize;

                public int State;

                public ulong Protect;

                public ulong Type;
            }

            private bool writeFastMode;

            private IntPtr hProc = IntPtr.Zero;

            [DllImport("kernel32.dll")]
            private static extern IntPtr OpenProcess(uint dwAccess, bool inherit, int pid);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flNewProtect, out uint lpflOldProtect);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool ReadProcessMemory(IntPtr hProcess, long lpBaseAddress, [In][Out] byte[] lpBuffer, ulong dwSize, out IntPtr lpNumberOfBytesRead);

            [DllImport("kernel32.dll")]
            private static extern bool WriteProcessMemory(IntPtr hProcess, long lpBaseAddress, [In][Out] byte[] lpBuffer, ulong dwSize, out IntPtr lpNumberOfBytesWritten);

            [DllImport("ntdll.dll")]
            private static extern bool NtWriteVirtualMemory(IntPtr hProcess, long lpBaseAddress, [In][Out] byte[] lpBuffer, ulong dwSize, out IntPtr lpNumberOfBytesWritten);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

            public RPM(int pId, bool fastmode)
            {
                writeFastMode = fastmode;
                hProc = OpenProcess(56u, inherit: false, pId);
            }

            public T ReadStruct<T>(long addr)
            {
                byte[] array = new byte[Marshal.SizeOf(typeof(T))];
                ReadProcessMemory(hProc, addr, array, (ulong)Marshal.SizeOf(typeof(T)), out var _);
                IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)));
                Marshal.Copy(array, 0, intPtr, Marshal.SizeOf(typeof(T)));
                T? result = Marshal.PtrToStructure<T>(intPtr);
                Marshal.FreeHGlobal(intPtr);
                return result;
            }

            public long ReadInt64(long addr)
            {
                byte[] array = new byte[8];
                ReadProcessMemory(hProc, addr, array, 8uL, out var _);
                return BitConverter.ToInt64(array, 0);
            }

            public int ReadInt32(long addr)
            {
                byte[] array = new byte[4];
                ReadProcessMemory(hProc, addr, array, 4uL, out var _);
                return BitConverter.ToInt32(array, 0);
            }

            public float ReadFloat(long addr)
            {
                byte[] array = new byte[4];
                ReadProcessMemory(hProc, addr, array, 4uL, out var _);
                return BitConverter.ToSingle(array, 0);
            }

            public bool WriteMemory(long addr, byte[] Buffer)
            {
                IntPtr lpNumberOfBytesWritten;
                if (!writeFastMode)
                {
                    WriteProcessMemory(hProc, addr, Buffer, (uint)Buffer.Length, out lpNumberOfBytesWritten);
                }
                else
                {
                    NtWriteVirtualMemory(hProc, addr, Buffer, (uint)Buffer.Length, out lpNumberOfBytesWritten);
                }

                return (IntPtr)Buffer.Length == lpNumberOfBytesWritten;
            }

            public bool WriteFloat(long addr, float _Value)
            {
                byte[] bytes = BitConverter.GetBytes(_Value);
                return WriteMemory(addr, bytes);
            }

            public bool WriteInt32(long addr, int _Value)
            {
                byte[] bytes = BitConverter.GetBytes(_Value);
                return WriteMemory(addr, bytes);
            }

            public bool WriteInt64(long addr, long _Value)
            {
                byte[] bytes = BitConverter.GetBytes(_Value);
                return WriteMemory(addr, bytes);
            }

            public bool WriteString(long addr, string _Value)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(_Value);
                return WriteMemory(addr, bytes);
            }

            public bool WriteByte(long addr, byte _Value)
            {
                byte[] bytes = BitConverter.GetBytes((short)_Value);
                return WriteMemory(addr, bytes);
            }

            public byte ReadByte(long addr)
            {
                byte[] array = new byte[1];
                ReadProcessMemory(hProc, addr, array, 1uL, out var _);
                return array[0];
            }

            public string ReadString(long addr)
            {
                byte[] array = new byte[512];
                ReadProcessMemory(hProc, addr, array, 512uL, out var _);
                return Encoding.ASCII.GetString(array).Split(new char[1])[0];
            }

            public Vector3 ReadVector3(long addr)
            {
                Vector3 result = default(Vector3);
                byte[] array = new byte[12];
                ReadProcessMemory(hProc, addr, array, 12uL, out var _);
                result.X = BitConverter.ToSingle(array, 0);
                result.Y = BitConverter.ToSingle(array, 4);
                result.Z = BitConverter.ToSingle(array, 8);
                return result;
            }

            public Matrix4x4 ReadMatrix(long addr)
            {
                Matrix4x4 result = default(Matrix4x4);
                byte[] array = new byte[64];
                ReadProcessMemory(hProc, addr, array, 64uL, out var _);
                result.M11 = BitConverter.ToSingle(array, 0);
                result.M12 = BitConverter.ToSingle(array, 4);
                result.M13 = BitConverter.ToSingle(array, 8);
                result.M14 = BitConverter.ToSingle(array, 12);
                result.M21 = BitConverter.ToSingle(array, 16);
                result.M22 = BitConverter.ToSingle(array, 20);
                result.M23 = BitConverter.ToSingle(array, 24);
                result.M24 = BitConverter.ToSingle(array, 28);
                result.M31 = BitConverter.ToSingle(array, 32);
                result.M32 = BitConverter.ToSingle(array, 36);
                result.M33 = BitConverter.ToSingle(array, 40);
                result.M34 = BitConverter.ToSingle(array, 44);
                result.M41 = BitConverter.ToSingle(array, 48);
                result.M42 = BitConverter.ToSingle(array, 52);
                result.M43 = BitConverter.ToSingle(array, 56);
                result.M44 = BitConverter.ToSingle(array, 60);
                return result;
            }

            public bool IsValid(long addr)
            {
                VirtualQueryEx(hProc, (IntPtr)addr, out var lpBuffer, (uint)Marshal.SizeOf(typeof(MEMORY_BASIC_INFORMATION)));
                return lpBuffer.State == 4096;
            }
        }

        public class Renderer
        {
            private int width;

            private int height;

            private Matrix4x4 pViewProj;

            [DllImport("Charm_native.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern void DrawLine(float x1, float y1, float x2, float y2, float thickness, float r, float g, float b, float a = 1f);

            [DllImport("Charm_native.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern void DrawBox(float x, float y, float width, float height, float thickness, float r, float g, float b, float a, bool filled);

            [DllImport("Charm_native.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern void DrawCircle(float x, float y, float radius, float thickness, float r, float g, float b, float a, bool filled);

            [DllImport("Charm_native.dll", CallingConvention = CallingConvention.Cdecl)]
            private static extern void DrawEllipse(float x, float y, float width, float height, float thickness, float r, float g, float b, float a, bool filled);

            [DllImport("Charm_native.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
            private static extern void DrawString(string str, float fontSize, float x, float y, float r, float g, float b, float a = 1f);

            private float ByteToFloat(byte b)
            {
                return (float)(int)b / 255f;
            }

            public void SetViewProjection(Matrix4x4 m_ViewProj)
            {
                pViewProj = m_ViewProj;
            }

            public void SetWorldToScreenSize(int m_width, int m_height)
            {
                width = m_width;
                height = m_height;
            }

            public void DrawLine(float x1, float y1, float x2, float y2, float thickness, Color color)
            {
                DrawLine(x1, y1, x2, y2, thickness, ByteToFloat(color.R), ByteToFloat(color.G), ByteToFloat(color.B));
            }

            public void DrawBox(float x, float y, float width, float height, float thickness, Color color, bool filled)
            {
                DrawBox(x, y, width, height, thickness, ByteToFloat(color.R), ByteToFloat(color.G), ByteToFloat(color.B), ByteToFloat(color.A), filled);
            }

            public void DrawCircle(float x, float y, float radius, float thickness, Color color, bool filled)
            {
                DrawCircle(x, y, radius, thickness, ByteToFloat(color.R), ByteToFloat(color.G), ByteToFloat(color.B), ByteToFloat(color.A), filled);
            }

            public void DrawEllipse(float x, float y, float width, float height, float thickness, Color color, bool filled)
            {
                DrawEllipse(x, y, width, height, thickness, ByteToFloat(color.R), ByteToFloat(color.G), ByteToFloat(color.B), ByteToFloat(color.A), filled);
            }

            public void DrawString(float x, float y, string text, Color color, float fontsize = 24f)
            {
                DrawString(text, fontsize, x, y, ByteToFloat(color.R), ByteToFloat(color.G), ByteToFloat(color.B));
            }

            public bool WorldToScreen(Vector3 m_World, out Vector3 m_Screen)
            {
                m_Screen = new Vector3(0f, 0f, 0f);
                float num = pViewProj.M14 * m_World.X + pViewProj.M24 * m_World.Y + (pViewProj.M34 * m_World.Z + pViewProj.M44);
                if (num < 0.0001f)
                {
                    return false;
                }

                float num2 = pViewProj.M11 * m_World.X + pViewProj.M21 * m_World.Y + (pViewProj.M31 * m_World.Z + pViewProj.M41);
                float num3 = pViewProj.M12 * m_World.X + pViewProj.M22 * m_World.Y + (pViewProj.M32 * m_World.Z + pViewProj.M42);
                m_Screen.X = (float)(width / 2) + (float)(width / 2) * num2 / num;
                m_Screen.Y = (float)(height / 2) - (float)(height / 2) * num3 / num;
                m_Screen.Z = num;
                return true;
            }
        }

        public const string UnmanagedFileName = "Charm_native.dll";

        private RPM rpm;

        private bool fastWrite;

        private Renderer renderer;

        private CallbackManaged callbackManaged;

        private CallbackUnmanaged callbackUnmanaged;

        private static CallbackUnmanaged StaticCallbackUnmanaged;

        private static CallbackManaged StaticCallbackManaged;

        [DllImport("Charm_native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DirectOverlaySetup([MarshalAs(UnmanagedType.FunctionPtr)] CallbackUnmanaged callBackUnmanaged, IntPtr hWnd);

        [DllImport("Charm_native.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DirectOverlaySetOption(CharmSettings option);

        public Charm()
        {
            //if (!File.Exists(Directory.GetCurrentDirectory() + "Charm_native.dll"))
            //{
            //    File.WriteAllBytes("Charm_native.dll", Resources.Charm_native);
            //}
        }

        public void CharmSetOptions(CharmSettings settings)
        {
            if ((settings & CharmSettings.WPM_WRITE_DIRTY) == CharmSettings.WPM_WRITE_DIRTY)
            {
                fastWrite = true;
            }
            else
            {
                DirectOverlaySetOption(settings);
            }
        }

        public CharmResult CharmInit(CallbackManaged m_callbackManaged, string processName)
        {
            Process[] processesByName = Process.GetProcessesByName(processName);
            if (processesByName.Length < 1)
            {
                return CharmResult.CHARM_NATIVE_NONE;
            }

            if (processesByName.Length > 1)
            {
                return CharmResult.CHARM_PROCESS_MANY;
            }

            Process process = processesByName[0];
            IntPtr mainWindowHandle = process.MainWindowHandle;
            if (mainWindowHandle == IntPtr.Zero)
            {
                mainWindowHandle = ProcessUtils.FindWindow("Sea of Thieves");
                if (mainWindowHandle == IntPtr.Zero)
                    mainWindowHandle = ProcessUtils.FindWindow("Sea of Thieves Insider");
                if (mainWindowHandle == IntPtr.Zero)
                    return CharmResult.CHARM_WINDOW_NONE;
            }

            callbackManaged = (StaticCallbackManaged = m_callbackManaged);
            rpm = new RPM(process.Id, fastWrite);
            renderer = new Renderer();
            callbackUnmanaged = (StaticCallbackUnmanaged = CharmCallback);
            DirectOverlaySetup(callbackUnmanaged, mainWindowHandle);
            return CharmResult.CHARM_SUCCESS;
        }

        private void CharmCallback(int width, int height)
        {
            renderer.SetWorldToScreenSize(width, height);
            callbackManaged(rpm, renderer, width, height);
        }
    }
}
