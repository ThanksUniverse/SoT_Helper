using Microsoft.VisualBasic;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoT_Helper.Models.SDKHelper
{
    public class SDK_Function
    {
        public string Name { get; set; }
        public string Returntype { get; set; }
        public List<SDK_Property> Parameters { get; set; }
    }

    public class SDK_Enum
    {
        public string Name { get; set; }
        public Dictionary<int, string> Values { get; set; }
    }

    public class SDK_Property
    {
        public string Name { get; set; }
        public string TypeName { get; set; }
        public SDK_Enum SDK_Enum { get; set; }
        public SDK_Class TypeClass { get; set; }
        public int Size { get; set; }
        public int Offset { get; set; }
        public bool IsPointer { get; set; }
        public bool IsSimpleType { get; set; }
        public bool IsArray { get; set; }
        public bool IsBitSize { get; set; }
        public int BitNumber { get; set; }

        public string GetPropertyText()
        {
            string text = $"{TypeName}";
            if (IsPointer)
            {
                text += "*";
            }
            if (IsArray)
            {
                text += "[]";
            }
            text += $" {Name}";
            
            if (IsBitSize)
            {
                text += " : " + BitNumber;
            }
            text+= $"; {Offset}({Size})" ;
            return text;
        }
    }

    public class SDK_Class
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public int InheritedSize { get; set; }
        public string[] CodeText { get; set; }
        public SDK_Class Parent { get; set; }
        public List<SDK_Class> Children { get; set; }
        public string ParentClassName { get; set; }
        public Dictionary<int, SDK_Property> Properties { get; set; }
        //public Dictionary<string, object> PropertyValues { get; set; }
        public List<SDK_Function> Functions { get; set; }
        public bool IsUpdated { get; set; }

        public SDK_Class()
        {
            Children = new List<SDK_Class>();
            Properties = new Dictionary<int, SDK_Property>();
            Functions = new List<SDK_Function>();
        }

        public void Update()
        {
            if(!IsUpdated)
            {
                try
                {
                    var newClass = SDKService.ReadClass(CodeText);
                    Update(newClass);
                }
                catch (Exception e)
                {
                    SoT_DataManager.InfoLog += $"Error updating class {Name}: {e.Message}\n";
                }
            }
        }

        public void Update(SDK_Class newClass)
        {
            Name = newClass.Name;
            Size = newClass.Size;
            InheritedSize = newClass.InheritedSize;
            ParentClassName = newClass.ParentClassName;
            Properties = newClass.Properties;
            Functions = newClass.Functions;
            IsUpdated = true;
        }
    }
}
