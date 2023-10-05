using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoT_Helper.Models.SDKClasses.Offsets;

namespace SoT_Helper.Models.SDKClasses
{
    public class Offsets
    {
        public FNameEntry fNameEntry { get; set; }
        public UObject uObject { get; set; }
        public UField uField { get; set; }
        public UStruct uStruct { get; set; }
        public UEnum uEnum { get; set; }
        public UFunction uFunction { get; set; }
        public UProperty uProperty { get; set; }

        public Offsets()
        {
            fNameEntry = new FNameEntry();
            uObject = new UObject();
            uField = new UField();
            uStruct = new UStruct();
            uEnum = new UEnum();
            uFunction = new UFunction();
            uProperty = new UProperty();
        }

        public class FNameEntry
        {
            public ushort HeaderSize { get; set; } = 0;
        }

        public class UObject
        {
            public ushort Index { get; set; } = 0;
            public ushort Class { get; set; } = 0;
            public ushort Name { get; set; } = 0;
            public ushort Outer { get; set; } = 0;
        }

        public class UField
        {
            public ushort Next { get; set; } = 0;
        }

        public class UStruct
        {
            public ushort SuperStruct { get; set; } = 0;
            public ushort Children { get; set; } = 0;
            public ushort PropertiesSize { get; set; } = 0;
        }

        public class UEnum
        {
            public ushort Names { get; set; } = 0;
            public ushort NamesElementSize { get; set; } = 0;
        }

        public class UFunction
        {
            public ushort FunctionFlags { get; set; } = 0;
            public ushort Func { get; set; } = 0;
        }

        public class UProperty
        {
            public ushort ArrayDim { get; set; } = 0;
            public ushort ElementSize { get; set; } = 0;
            public ushort PropertyFlags { get; set; } = 0;
            public ushort Offset { get; set; } = 0;
            public ushort Size { get; set; } = 0;
        }
    }

    public class NewOffsetDef
    {
        public FNameEntry FNameEntry { get; set; }
        public UObject UObject { get; set; }
        public UField UField { get; set; }
        public UStruct UStruct { get; set; }
        public UEnum UEnum { get; set; }
        public UFunction UFunction { get; set; }
        public UProperty UProperty { get; set; }

        public NewOffsetDef()
        {
            FNameEntry = new FNameEntry() { HeaderSize = 0x10 };
            UObject = new UObject() { Index = 0xC, Class = 0x10, Name = 0x18, Outer = 0x20 };
            UField = new UField() { Next = 0x28 };
            UStruct = new UStruct() { SuperStruct = 0x30, Children = 0x38, PropertiesSize = 0x40 };
            UEnum = new UEnum() { Names = 0x40, NamesElementSize = 0xC };
            UFunction = new UFunction() { FunctionFlags = 0x88, Func = 0xB0 };
            UProperty = new UProperty() { ArrayDim = 0x30, ElementSize = 0x34, PropertyFlags = 0x38, Offset = 0x4C, Size = 0x70 };
        }

        // ... other classes are defined as before
    }


}
