using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace SoT_Helper.Services
{
    public struct MEMORY_BASIC_INFORMATION
    {
        public int BaseAddress;
        public int AllocationBase;
        public int AllocationProtect;
        public int RegionSize;   // size of the region allocated by the program
        public int State;   // check if allocated (MEM_COMMIT)
        public int Protect; // page protection (must be PAGE_READWRITE)
        public int lType;
    }

    public struct MEMORY_BASIC_INFORMATION64
    {
        public ulong BaseAddress;
        public ulong AllocationBase;
        public uint AllocationProtect;
        public uint __alignment1;
        public ulong RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
        public uint __alignment2;
    }

    [Flags]
    public enum AllocationType
    {
        MEM_COMMIT = 0x00001000,
        MEM_RESERVE = 0x00002000,
        MEM_RESET = 0x00080000,
        MEM_RESET_UNDO = 0x1000000,
        MEM_LARGE_PAGES = 0x20000000,
        MEM_PHYSICAL = 0x00400000,
        MEM_TOP_DOWN = 0x00100000
    }

    [Flags]
    public enum MemoryProtection
    {
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x4,
        PAGE_WRITECOPY = 0x8,
        PAGE_TARGETS_INVALID = 0x40000000,
        PAGE_TARGETS_NO_UPDATE = 0x40000000,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400
    }

    [Flags]
    public enum ProcessAccessFlags : uint
    {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VMOperation = 0x00000008,
        VMRead = 0x00000010,
        VMWrite = 0x00000020,
        DupHandle = 0x00000040,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        Synchronize = 0x00100000
    }

    public class MemoryReader //: IDisposable
    {
        private readonly IntPtr _handle;

        //private static readonly Dictionary<IntPtr, int> _allocations = new Dictionary<IntPtr, int>();

        private readonly System.Diagnostics.Process _process;

        //int MAX_PATH = 260;
        //int MAX_MODULE_NAME32 = 255;
        //int TH32CS_SNAPMODULE = 0x00000008;
        //int TH32CS_SNAPMODULE32 = 0x00000010;
        //int PROCESS_QUERY_INFORMATION = 0x0400;
        //int PROCESS_VM_READ = 0x0010;

        ////extra added by Caldor
        //int PROCESS_VM_OPERATION = 0x0008;
        //int PROCESS_VM_WRITE = 0x0020;
        //int PROCESS_ALL_ACCESS = 0x1f0fff;
        public bool ThrowExceptions { get; set; } = false;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern Int32 CloseHandle(IntPtr hProcess);

        public MemoryReader(System.Diagnostics.Process process)
        {
            _process = process;
            //_handle = process.Handle;
            _handle = OpenProcess(ProcessAccessFlags.VMRead, false, process.Id);
        }

        public MemoryReader(IntPtr processHandle)
        {
            _handle = processHandle;
        }

        public void WriteBytes(ulong address, byte[] bytes)
        {
            var hProc = OpenProcess(ProcessAccessFlags.All, false, _process.Id);
            int wtf = 0;
            WriteProcessMemory(hProc, new IntPtr((long)address), bytes, (uint)bytes.Length, out wtf);

            CloseHandle(hProc);
        }

        public void WriteBool(ulong address, bool v)
        {
            WriteBool((long)address, v);
        }

        public void WriteByte(ulong address, byte b)
        {
            var hProc = OpenProcess(ProcessAccessFlags.All, false, _process.Id);
            //var val = new byte[] { (byte)v };
            var val = new byte[] { b };

            int wtf = 0;
            WriteProcessMemory(hProc, new IntPtr((long)address), val, 1, out wtf);

            CloseHandle(hProc);
        }

        public void WriteBool(long address, bool v)
        {
            var hProc = OpenProcess(ProcessAccessFlags.All, false, _process.Id);
            //var val = new byte[] { (byte)v };
            var val = new byte[] { (byte)(v ? 1 : 0) };

            int wtf = 0;
            WriteProcessMemory(hProc, new IntPtr(address), val, 1, out wtf);

            CloseHandle(hProc);
        }

        public static IntPtr GetEndAddress(System.Diagnostics.Process process, ProcessModule module)
        {
            return module.BaseAddress + module.ModuleMemorySize;
        }

        public string ReadRawname(ulong address)
        {
            var actorId =ReadInt((IntPtr)address + SoT_Tool.offsets["Actor.actorId"]);
            if(actorId < 1 || actorId > 500000)
                return "";
            var name = ReadGname(actorId);
            return name;
        }

        public string ReadGname(int actorId)
        {
            if(actorId < 1 || actorId > 500000)
                return "None";

            if(SoT_DataManager.Actor_name_map.TryGetValue(actorId, out string name))
                return name;

            ulong gname = ReadULong(SoT_Tool.g_name_start_address + (ulong)(actorId / 0x4000) * 0x8); // 0x4000 = 16384 and 0x8 = 8
            var nameAddress = gname + (ulong)(0x8 * (actorId % 0x4000));
            ulong nameptr = ReadULong(nameAddress);
            //int len = ReadInt(nameAddress + 8);

            //name = ReadNameString(nameptr + 0x10, 64);
            //name = ReadString2(nameptr + 0x10, 64);

            name = ReadString(nameptr + 0x10, 64);
            SoT_DataManager.Actor_name_map.TryAdd(actorId, name);
            return name;
            //return ReadString(nameptr + 0x10, 64);
        }

        public string ReadFString(ulong stringptr)
        {
            var stringAddress = ReadULong(stringptr);
            var charcount = ReadInt((IntPtr)stringptr + 8);
            var text = ReadString(stringAddress, charcount);
            return text;
        }

        public string ReadFName(ulong address)
        {
            int comparisonIndex = ReadInt((IntPtr)address);
            //int number = ReadInt((IntPtr)address + 4);

            var name = ReadGname(comparisonIndex);
            return name;
        }

        public string ReadString2(ulong address, int byteCount = 256)
        {
            // Read bytes directly into span to avoid array copy
            Span<byte> buffer = stackalloc byte[byteCount * 2]; // Allocate double for potential UTF-16 encoding
            ReadBytes((UIntPtr)address, buffer);

            // Check the encoding and read accordingly
            string result;
            if (buffer[1] == 0) // Likely UTF-16
            {
                int nullIndex = buffer.IndexOf(stackalloc byte[] { 0, 0 });
                result = Encoding.Unicode.GetString(buffer.Slice(0, nullIndex));
            }
            else // Likely ASCII or UTF-8
            {
                int nullIndex = buffer.IndexOf((byte)0);
                result = Encoding.ASCII.GetString(buffer.Slice(0, nullIndex));
            }

            if (string.IsNullOrWhiteSpace(result) || result.Length == 1)
            {
                return "NoStringFound";
            }

            return result;
        }

        public string ReadString(ulong address, int byteCount = 256)
        {
            int skip = 0;
            if (ReadBytes((IntPtr)address + 1, 1).First() == 0)
                skip = 1;
            else
            {

            }
            // Read the bytes from the specified address
            List<byte> buffer = new List<byte>();

            for (int i = 0; i < byteCount * 2; i += skip + 1)
            {
                var b = ReadBytes((UIntPtr)address + (uint)i, 1).First();
                if (b == 0)
                    break;
                buffer.Add(b);
            }

            // Convert the bytes up until the first null byte to a string
            string result = Encoding.ASCII.GetString(buffer.ToArray());

            // Sometimes in SoT, strings are UTF-16 vs UTF-8, so we want to check and see if this string is UTF-16 and return that version if it is
            if (result.Length == 1)
            {
                string longerCheck = ReadNameString(address, byteCount * 2);
                if (Regex.IsMatch(longerCheck, "[A-Za-z0-9_/\"']"))
                {
                    result = longerCheck;
                }
            }

            if (result == "")
                return "NoStringFound";

            return result;
        }

        public string ReadStringOld(ulong address, int byteCount = 256)
        {
            // Read the bytes from the specified address
            byte[] buffer = ReadBytes((IntPtr)address, byteCount);

            // Find the index of the first null byte
            int nullByteIndex = Array.IndexOf(buffer, (byte)0);

            if (nullByteIndex == 1)
            {
                int zerosFound = 0;
                // Double the buffer because this nullByteIndex shows the string is probably half 0 bytes which usually means its UTF-16
                var newbuffer = buffer.ToList();
                newbuffer.AddRange(ReadBytes((IntPtr)address + byteCount, byteCount));
                buffer = newbuffer.ToArray();
                // Find the double 0 byte and use that as the nullByteIndex
                for (int i = 0; i < buffer.Length || zerosFound > 1; i++)
                {
                    if (buffer[i] == 0)
                    {
                        zerosFound++;
                        if(zerosFound == 2)
                        {
                            if(i > 0)
                                nullByteIndex = i - 1;
                            break;
                        }
                    }
                    else
                        zerosFound=0;
                }
                
                byte[] newBuffer2 = buffer.Take(nullByteIndex).Where(b => b > 0).ToArray();

                buffer = newBuffer2;
                nullByteIndex = buffer.Length;
            }

            if (nullByteIndex < 0)
                return "NoStringFound";

            // Convert the bytes up until the first null byte to a string
            string result = Encoding.ASCII.GetString(buffer, 0, nullByteIndex);

            // Sometimes in SoT, strings are UTF-16 vs UTF-8, so we want to check and see if this string is UTF-16 and return that version if it is
            if (result.Length == 1)
            {
                string longerCheck = ReadNameString(address, byteCount);
                if (Regex.IsMatch(longerCheck, "[A-Za-z0-9_/\"']"))
                {
                    result = longerCheck;
                }
            }

            return result;
        }

        public string ReadNameString(ulong address, int byteCount = 32)
        {
            byte[] buffer = ReadBytes((IntPtr)address, byteCount);
            int i = Array.IndexOf(buffer, (byte)0x00, 0);
            if (i >= 0 && i < byteCount - 1)
            {
                byte[] shorter = new byte[i + 1];
                Array.Copy(buffer, shorter, i + 1);
                try
                {
                    string joined = Encoding.Unicode.GetString(shorter).TrimEnd('\0');
                    return joined.Replace('’', '\'');
                }
                catch
                {
                    string joined = Encoding.UTF8.GetString(shorter).TrimEnd('\0');
                    return joined.Replace('’', '\'');
                }
            }
            else
            {
                return "";
            }
        }

        public string ReadString(IntPtr address, int length, Encoding encoding = null)
        {
            if (encoding == null) encoding = Encoding.Unicode;

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < length; i++)
            {
                byte read = ReadBytes(address + bytes.Count, 1)[0];

                //if (read == 0x00)
                //    break;

                bytes.Add(read);
            }

            return encoding.GetString(bytes.ToArray());
        }

        public string ReadUnicodeString(IntPtr address, int length)
        {
            return Encoding.Unicode.GetString(ReadBytes(address, length));
        }

        public string ReadUTF8String(IntPtr address, int length)
        {
            return Encoding.UTF8.GetString(ReadBytes(address, length));
        }

        public Vector2 ReadVector2(ulong address)
        {
            byte[] bytes = ReadBytes((IntPtr)address, 8);

            Vector2 vector2 = new Vector2();

            vector2.X = BitConverter.ToSingle(bytes, 0)/100;
            vector2.Y = BitConverter.ToSingle(bytes, 4)/100;
            return vector2;
        }

        public Vector3 ReadVector3(ulong address)
        {
            byte[] bytes = ReadBytes((IntPtr)address, 12);

            Vector3 vector3 = new Vector3();

            vector3.X = BitConverter.ToSingle(bytes, 0) /100;
            vector3.Y = BitConverter.ToSingle(bytes, 4) /100;
            vector3.Z = BitConverter.ToSingle(bytes, 8) /100;
            return vector3;
        }

        public Quaternion ReadQuaternion(ulong address)
        {
            byte[] bytes = ReadBytes((IntPtr)address, 16);

            Quaternion quart = new Quaternion();

            quart.X = BitConverter.ToSingle(bytes, 0);
            quart.Y = BitConverter.ToSingle(bytes, 4);
            quart.Z = BitConverter.ToSingle(bytes, 8);
            quart.W = BitConverter.ToSingle(bytes, 12);
            return quart;
        }

        public string ReadFText(ulong address)
        {
            var FTextPtr = ReadULong((ulong)address);
            //var FTextStringPtr = ReadULong((ulong)FTextPtr);
            //var FTextStringCharCount = ReadInt((IntPtr)FTextPtr+8);
            //var text = ReadString(FTextStringPtr, FTextStringCharCount);

            return ReadFString(FTextPtr);
        }

        public bool ReadBool(ulong address, int bit)
        {
            byte by = ReadBytes((IntPtr)address, 1).First();
            bool bo = GetBit(by, bit);
            return bo;
        }

        public bool ReadBool(ulong address)
        {
            return BitConverter.ToBoolean(ReadBytes((IntPtr)address, 1), 0);
        }

        public static bool GetBit(byte b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        public bool ReadBool(IntPtr address)
        {
            return BitConverter.ToBoolean(ReadBytes(address, 1), 0);
        }

        public byte ReadByte(ulong address)
        {
            return ReadBytes((UIntPtr)address, 1).First();
        }

        public short ReadShort(IntPtr address)
        {
            return BitConverter.ToInt16(ReadBytes(address, 2), 0);
        }

        public int ReadInt(IntPtr address)
        {
            return BitConverter.ToInt32(ReadBytes(address, 4), 0);
        }

        public int ReadInt(ulong address)
        {
            return BitConverter.ToInt32(ReadBytes((UIntPtr)address, 4), 0);
        }

        public uint ReadUInt(UIntPtr address)
        {
            return BitConverter.ToUInt32(ReadBytes(address, 4), 0);
        }

        public Guid ReadGuid(ulong address)
        {
            byte[] crewGuidRaw = ReadBytes((UIntPtr)address, 16);
            Guid guid = new Guid(crewGuidRaw);

            return guid;
        }

        public uint ReadUInt(ulong address)
        {
            return BitConverter.ToUInt32(ReadBytes((UIntPtr)address, 4), 0);
        }

        public float ReadFloat(ulong address)
        {
            return BitConverter.ToSingle(ReadBytes((UIntPtr)address, 4), 0);
        }

        public long ReadLong(IntPtr address)
        {
            return BitConverter.ToInt64(ReadBytes(address, 8), 0);
        }

        public long ReadLong(long address)
        {
            return BitConverter.ToInt64(ReadBytes((IntPtr)address, 8), 0);
        }

        public ulong ReadULong(ulong address)
        {
            return BitConverter.ToUInt64(ReadBytes((UIntPtr)address, 8), 0);
        }

        public ulong ReadULong(UIntPtr address)
        {
            return BitConverter.ToUInt64(ReadBytes(address, 8), 0);
        }

        public byte[] ReadBytes(UIntPtr address, int size)
        {
            byte[] bytes = new byte[size];

            var temp = address.ToUInt64();
            if (temp < SoT_Tool.minmemaddress || temp > SoT_Tool.maxmemaddress)
                return bytes;

            //VirtualQueryEx(_handle, (IntPtr)address.ToUInt64(), out var buffer, (uint)size);

            int bytesread = 0;

            var test1 = SoT_Tool.DebugRawName;
            var test2 = SoT_Tool.DebugActorId;
            if (!ReadProcessMemory(_handle, (IntPtr)address.ToUInt64(), bytes, size, out bytesread))
                throw new Exception("Failed to read process memory", new Win32Exception(Marshal.GetLastWin32Error()));

            return bytes;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern unsafe bool ReadProcessMemory
        (
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            void* lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
        );

        public void ReadBytes(UIntPtr address, Span<byte> buffer)
        {
            if (address.ToUInt64() < SoT_Tool.minmemaddress || address.ToUInt64() > SoT_Tool.maxmemaddress)
                return;

            unsafe
            {
                fixed (byte* b = &MemoryMarshal.GetReference(buffer))
                {
                    if (!ReadProcessMemory(_handle, (IntPtr)address.ToUInt64(), b, buffer.Length, out int bytesread))
                        throw new Exception("Failed to read process memory", new Win32Exception(Marshal.GetLastWin32Error()));
                }
            }
        }

        //[DllImport("kernel32.dll")]
        //public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out long lpNumberOfBytesRead);

        //[DllImport("kernel32.dll", SetLastError = true)]
        //public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        public byte[] ReadBytes(IntPtr address, int size)
        {
            byte[] bytes = new byte[size];

            var temp = (ulong)address.ToInt64();
            if (temp < SoT_Tool.minmemaddress || temp > SoT_Tool.maxmemaddress)
                return bytes;

            //70368744177688
            //236349177856
            //140702931032064

            int bytesRead = 0;

            for (int i = 0; i < size; i++)
            {
                byte[] b = new byte[1];
                var reading = address + i;

                var test1 = SoT_Tool.DebugRawName;
                var test2 = SoT_Tool.DebugActorId;

                if (!ReadProcessMemory(_handle, address + i, b, 1, out int byteread))
                    throw new Exception("Failed to read process memory", new Win32Exception(Marshal.GetLastWin32Error()));

                if (byteread != 1)
                {
                    Console.WriteLine($"Expected to read 1 byte at offset {i}, but read {byteread} bytes");
                }
                bytes[i] = b[0];
                byteread++;
                //try
                //{
                //    if (!ReadProcessMemory(_handle, address + i, b, 1, out int byteread))
                //        throw new Exception("Failed to read process memory", new Win32Exception(Marshal.GetLastWin32Error()));

                //    if (byteread != 1)
                //    {
                //        Console.WriteLine($"Expected to read 1 byte at offset {i}, but read {byteread} bytes");
                //    }
                //    bytes[i] = b[0];
                //}
                //catch (Exception ex)
                //{
                //    var test1 = SoT_Tool.DebugRawName;
                //    var test2 = SoT_Tool.DebugActorId;
                //    if (ThrowExceptions)
                //    {

                //        throw ex;
                //    }
                //    //return bytes;
                //    bytesRead++;
                //}
            }

            //if (!ReadProcessMemory(_handle, address, out bytes, size, out bytesread))
            //    throw new Exception("Failed to read process memory", new Win32Exception(Marshal.GetLastWin32Error()));

            if (bytesRead != size)
            {
                Console.WriteLine($"Expected to read {size} bytes, but read {bytesRead} bytes");
            }

            return bytes;
        }

        /// <summary>
        /// ReadProcessMemory
        /// 
        ///     API import definition for ReadProcessMemory.
        /// </summary>
        /// <param name="hProcess">Handle to the process we want to read from.</param>
        /// <param name="lpBaseAddress">The base address to start reading from.</param>
        /// <param name="lpBuffer">The return buffer to write the read data to.</param>
        /// <param name="dwSize">The size of data we wish to read.</param>
        /// <param name="lpNumberOfBytesRead">The number of bytes successfully read.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            [Out()] byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead
            );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        [Out()] byte lpBuffer,
        int dwSize,
        out int lpNumberOfBytesRead
        );

        //[DllImport("kernel32.dll")]
        //public static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out ulong lpNumberOfBytesRead);

        //[DllImport("kernel32.dll", SetLastError = true)]
        //static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION64 lpBuffer, uint dwLength);

        //public IntPtr AllocateAndWrite(byte[] data)
        //{
        //    IntPtr addr = Allocate(data.Length);
        //    Write(addr, data);
        //    return addr;
        //}

        //public IntPtr AllocateAndWrite(string data) => AllocateAndWrite(Encoding.UTF8.GetBytes(data));

        //public IntPtr AllocateAndWrite(int data) => AllocateAndWrite(BitConverter.GetBytes(data));

        //public IntPtr AllocateAndWrite(long data) => AllocateAndWrite(BitConverter.GetBytes(data));

        //public IntPtr Allocate(int size)
        //{
        //    IntPtr addr =
        //        VirtualAllocEx(_handle, IntPtr.Zero, size,
        //            AllocationType.MEM_COMMIT, MemoryProtection.PAGE_EXECUTE_READWRITE);

        //    if (addr == IntPtr.Zero)
        //        throw new Exception("Failed to allocate process memory", new Win32Exception(Marshal.GetLastWin32Error()));

        //    _allocations.Add(addr, size);
        //    return addr;
        //}

        //public void Write(IntPtr addr, byte[] data)
        //{
        //    if (!WriteProcessMemory(_handle, addr, data, data.Length))
        //        throw new Exception("Failed to write process memory", new Win32Exception(Marshal.GetLastWin32Error()));
        //}

        //public void Dispose()
        //{
        //    foreach (var kvp in _allocations)
        //        VirtualFreeEx(_handle, kvp.Key, kvp.Value, MemoryFreeType.MEM_DECOMMIT);
        //}
    }
}
