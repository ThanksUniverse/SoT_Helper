using SoT_Helper.Models.SDKClasses;
using SoT_Helper.Models.SDKHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SoT_Helper.Services
{
    public class SDKService
    {
        static public Dictionary<string, Dictionary<string,SDK_Class>> fullSDKDetailed = new Dictionary<string, Dictionary<string,SDK_Class>>();
        static public Dictionary<string, SDK_Enum> SDK_Enums = new Dictionary<string, SDK_Enum>();
        static public Dictionary<string, int> classSizeMap = new Dictionary<string, int>();

        public static async Task ScanSDK(string SDKPath = "h:\\Modding\\Sea of Thieves\\UnrealDumper-4.25-UnrealDumper-4.10\\UnrealDumper-4.25-UnrealDumper-4.10\\bin\\Debug\\Games\\SoTGame\\DUMP\\")
        {
            if(fullSDKDetailed.Any()) 
            {
                SoT_DataManager.InfoLog += "\nSDK already scanned";
                return;
            }

            SoT_DataManager.InfoLog += "\nScanning SDK in folder " + SDKPath;

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            DirectoryInfo directoryInfo = new DirectoryInfo(SDKPath);
            var SDKfiles = directoryInfo.GetFiles().Where(f => f.Extension == ".h").ToList();

            if (SDKfiles.Count == 0)
            {
                SoT_DataManager.InfoLog += "\nNo SDK files found in folder " + SDKPath;
                return;
            }

            //To speed up the search, we will look in the main files first
            List<string> names = new List<string>() { "Athena_classes.h", "Athena_struct.h", "Engine_classes.h", "Engine_struct.h" };
            // We will ignore these files as they are not needed and cause errors
            List<string> namesToIgnore = new List<string>() { "BP_CliffGenerator_classes.h" };
            var mainFiles = SDKfiles.Where(f => names.Contains(f.Name)).ToList();
            SDKfiles.RemoveAll(f => mainFiles.Contains(f));
            SDKfiles.RemoveAll(f => namesToIgnore.Contains(f.Name));
            SDKfiles.InsertRange(0, mainFiles);

            Dictionary<string, List<string>> fullSDK = new Dictionary<string, List<string>>();

            //Another speed up, we will read all the lines of the files into a single array to avoid having to search through the files multiple times
            //SoT_DataManager.InfoLog += "\nReading SDK files into memory.";
            foreach (var f in SDKfiles)
            {
                fullSDK.Add(f.Name, File.ReadAllLines(f.FullName).ToList());
            }

            foreach(var file in fullSDK) 
            {
                //SoT_DataManager.InfoLog += "\nReading SDK file " + file.Key;

                Dictionary<string,SDK_Class> classes = new Dictionary<string, SDK_Class>();
                for(int i = 0; i< file.Value.Count; i++)
                {
                    
                    var className = file.Value[i].Split('.')[1];
                    var classLength = GetClassLength(file.Value.Skip(i).ToArray());
                    if (classLength < 1)
                    {
                        //SoT_DataManager.InfoLog += "\nError reading SDK file " + file.Key + " Class " + className;
                        break;
                    }
                    var classText = file.Value.Skip(i).Take(classLength).ToArray();
                    if (file.Value[i].Split('.')[0].Contains("Enum"))
                    {
                        var enumClass = new SDK_Enum();
                        enumClass.Name = className;
                        enumClass.Values = GetEnumValues(classText);
                        SDK_Enums.Add(enumClass.Name, enumClass);
                        i += classLength;
                        continue;
                    }
                    SDK_Class sdkclass = new SDK_Class();
                    sdkclass.Name = className;
                    sdkclass.CodeText = classText;
                    sdkclass.Size = GetClassSize(sdkclass.CodeText);
                    classSizeMap.Add(className, sdkclass.Size);
                    classes.Add(sdkclass.Name, sdkclass);
                    i += classLength;
                }
                fullSDKDetailed.Add(file.Key, classes);
                //SoT_DataManager.InfoLog += "\nRead SDK file " + file.Key;
            }
            fullSDKDetailed.SelectMany(s => s.Value.Values).ToList().
                ForEach(c => c.Update());

            // HelperClasses, define unknown properties
            {
                //List<SDK_Class> classes = new List<SDK_Class>();
                //var ObjectClass = new SDK_Class()
                //{
                //    Name = "Object",
                //    Size = 40,
                //    IsUpdated = true,
                //};
                //ObjectClass.Properties.Add(0, new SDK_Property() { Name = "ActorId", Offset = 24, Size = 8, TypeName = "Int" });
                //ObjectClass.Properties.Add(1, new SDK_Property() { Name = "Rawname", Offset = 24, Size = 8, TypeName = "Name" });
                //fullSDKDetailed.Add("Other", classes);

                var ObjectClass = fullSDKDetailed.SelectMany(s => s.Value.Values).Where(c => c.Name == "Object").First();
                //ObjectClass.Update();
                ObjectClass.Properties.Add(ObjectClass.Properties.Count, 
                    new SDK_Property() { Name = "ActorId", Offset = 24, Size = 4, TypeName = "int32_t", IsSimpleType = true });
                ObjectClass.Properties.Add(ObjectClass.Properties.Count, 
                    new SDK_Property() { Name = "Rawname", Offset = 24, Size = 4, TypeName = "Name" });

                var SceneComponentClass = fullSDKDetailed.SelectMany(s => s.Value.Values).Where(c => c.Name == "SceneComponent").First();
                //ObjectClass.Update();
                var key = SceneComponentClass.Properties.Where(p => p.Value.Offset == 300).First().Key;
                SceneComponentClass.Properties.Remove(key);

                SceneComponentClass.Properties[key] = new SDK_Property() { Name = "Bounds", Offset = 300, Size = 28, TypeName = "BoxSphereBounds" };

                SceneComponentClass.Properties.Add(SceneComponentClass.Properties.Count,
                    new SDK_Property() { Name = "ComponentToWorld", Offset = 336, Size = 48, TypeName = "Transform" }); // 0x30 = 48

                var FishingFloatNameplateComponent = fullSDKDetailed.SelectMany(s => s.Value.Values).Where(c => c.Name == "FishingFloatNameplateComponent").First();
                //"FishingFloatNameplateComponent.FishName"

                FishingFloatNameplateComponent.Properties.Add(FishingFloatNameplateComponent.Properties.Count,
                    new SDK_Property() { Name = "FishName", Offset = 856, Size = 12, TypeName = "String" }); // 0x30 = 48
            }

            //SoT_DataManager.InfoLog += "\nSDK files stored in memory.";
            ConfigurationManager.AppSettings["SDKPath"]= SDKPath;
            stopwatch.Stop();
            SoT_DataManager.InfoLog += "\nSDK scanned in " + stopwatch.ElapsedMilliseconds + "ms";
            SoT_DataManager.InfoLog += "\nSDK found " + fullSDKDetailed.SelectMany(s => s.Value).Count() + " classes";

            var enumClasses = fullSDKDetailed.SelectMany(s => s.Value.Values).Where(c => c.Properties.Any(p => p.Value.SDK_Enum != null)).ToArray();
            var enumProperties = fullSDKDetailed.SelectMany(s => s.Value.Values).SelectMany(c => c.Properties).Where(p => p.Value.SDK_Enum != null).ToList();
                //ToDictionary(p => p.Value.Name, p => p.Value).ToArray();
        }

        private static Dictionary<int, string> GetEnumValues(string[] classText)
        {
            Dictionary<int, string> values = new Dictionary<int, string>();
            for (int j = 2; j < classText.Count() - 1; j++)
            {
                var line = classText[j];
                if (line.Contains("};"))
                    break;
                var name = line.Split(',')[0].Trim();
                //var name = line.Split(',')[1].Trim();
                values.Add(j-2, name);
            };
            return values;
        }

        public static int GetClassLength(string[] sdklines)
        {
            for(int i = 0; i< sdklines.Length; i++) 
            {
                if (sdklines[i].Contains("};"))
                {
                    return i+1;
                }
            }
            return -1;
        }

        public static SDK_Class GetPropertyClass(string className, string propertyName)
        {
            var c = GetClassFromName(className);
            if (c.Properties == null)
                c.Update();
            if(c != null)
            {
                var p = c.Properties.Where(p => p.Value.Name == propertyName);
                if(p.Any())
                {
                    var property = p.First().Value;
                    var propertyclass = GetClassFromName(property.TypeName);
                    property.TypeClass = propertyclass;
                    return propertyclass;
                }
            }
            return null;
        }

        public static SDK_Class GetClassFromName(string className)
        {
            if(string.IsNullOrEmpty(className))
                return null;
            var c = fullSDKDetailed.SelectMany(s => s.Value.Values).Where(c => c.Name.ToLower() == className.ToLower());
            if (c.Any())
            {
                return c.First();
            }
            var test = fullSDKDetailed.SelectMany(s => s.Value.Values).Where(c => c.Name.Contains(className));

            return null;
        }

        public static int FindSDKOffset(string ClassVariable)
        {
            int offset = -1;

            var classname = ClassVariable.Substring(0, ClassVariable.IndexOf("."));
            var variable = ClassVariable.Substring(ClassVariable.IndexOf(".") + 1);
            offset = fullSDKDetailed.Values.SelectMany(v => v.Select(v => v.Value))
                .Where(c => c.Name == classname)
                .Select(c => c.Properties.Where(p => p.Value.Name == variable)
                .Select(p => p.Value.Offset).FirstOrDefault()).FirstOrDefault();

            return offset;
        }

        public static SDK_Class ReadClass(string[] lines) 
        {
            SDK_Class classInfo = new SDK_Class();

            int propertyCount = 0;

            int bit = 0;
            int bitOffset = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                var debugLine = lines[i];

                if (lines[i].Contains(@"};"))
                {
                    break;
                }
                if (lines[i].Length < 3)
                {
                    continue;
                }
                if (lines[i].StartsWith(@"//") && !lines[i].StartsWith("// Size"))
                {
                    classInfo.Name = lines[i].Split('.')[1];
                }
                else if (lines[i].StartsWith("// Size"))
                {
                    var offsetHex = lines[i].Substring(9, lines[i].IndexOf(" (") - 9);
                    classInfo.Size = Convert.ToInt32(offsetHex, 16);
                    if(lines[i].Contains("Inherited"))
                    {
                        var inheritedHex = lines[i].Split("Inherited: ")[1].Replace(")","").Trim();
                        classInfo.InheritedSize = Convert.ToInt32(inheritedHex, 16);
                    }
                }
                else if (lines[i].EndsWith(@"{"))
                {
                    if (lines[i].Contains(":"))
                        classInfo.ParentClassName = lines[i].Split(':')[1].Replace("{", "").Trim().Substring(1);
                    else
                        continue;
                } // ```
                else if (lines[i].EndsWith(@")"))
                {
                    SDK_Property property = new SDK_Property();
                    var offsetHex = lines[i].Split("; // ")[1].Split("(")[0].Trim();
                    var offset = Convert.ToInt32(offsetHex, 16);

                    property.Offset = offset;

                    var propertySizeHex = lines[i].Split("(")[1].Replace(")", "").Trim();
                    var propertySize = Convert.ToInt32(propertySizeHex, 16);

                    // Helps enums to be read correctly, it might be changed later if this property is not an enum
                    property.Name = lines[i].Split(";")[0].Split(" ").Last();
                    // Add support for properties like this:
                    // class                                                        ProjectileClass;                                   // 0x5b0(0x8)
                    property.Size= propertySize;
                    property.IsPointer = lines[i].Contains("*");
                    lines[i] = lines[i].Replace("*", "");
                    var propertyType = lines[i].Substring(1).Split(" ")[0];
                    if (propertyType == "struct")
                    {
                        property.Name = lines[i].Split(";")[0].Split(" ").Last();
                        var line = lines[i].Substring(1).Split(" ")[1];

                        if (line == "UClass")
                        {
                            property.TypeName = property.Name.Replace("Class", "");
                            property.IsSimpleType = false;
                            //var propertyInfo = GetClassFromName(property.TypeName);
                            //propertyInfo.Update();
                            //property.TypeClass = propertyInfo;
                            //property.Size = propertyInfo.Size;
                        }
                        else if (line.Contains("TArray"))
                        {
                            string arrayInfo = lines[i].Split("<")[1].Split(">")[0];
                            property.IsArray = true;
                            if (arrayInfo.Contains("struct"))
                            {
                                property.TypeName = arrayInfo.Split(" ")[1].Substring(1);
                                property.IsSimpleType = false;
                                if (classSizeMap.ContainsKey(property.TypeName))
                                    propertySize = classSizeMap[property.TypeName];
                                else
                                    propertySize = 0;
                                property.Size = propertySize;
                            }
                            else
                            {
                                property.TypeName = arrayInfo;
                                property.IsSimpleType = true;
                            }
                        }
                        else
                        {
                            property.TypeName = line.Substring(1);
                            property.IsSimpleType = false;
                        }
                    }
                    else if (propertyType == "char" && propertySize == 1 && !lines[i].Contains(":") && property.Name != "Id" && !property.Name.Contains("UnknownData") && SDK_Enums.Keys.Any(k => k.Contains(property.Name)))
                    {
                        var proptext = lines[i];
                        var classname = classInfo.Name;
                        var enums = SDK_Enums.Where(k => k.Key.Contains(property.Name)).Select(e => e.Value).ToList();
                        var names = enums.Select(e => e.Name).ToList();
                        var enumName = enums.First().Name;
                        if(property.Name == "DisplayPriority")
                        {

                        }
                        if (enums.Count > 1)
                        {
                            if (enums.Count(e => e.Name == "E" + property.Name) == 1)
                            {
                                property.SDK_Enum = enums.First(e => e.Name == "E" + property.Name);
                                property.TypeName = property.SDK_Enum.Name;
                                property.IsSimpleType = true;
                            }
                            else if (enums.Count(e => e.Name.Contains(classInfo.Name)) == 1)
                            {
                                property.SDK_Enum = enums.First(e => e.Name.Contains(classInfo.Name));
                                property.TypeName = property.SDK_Enum.Name;
                                property.IsSimpleType = true;
                            }
                            else
                            {
                                BuildStandardProperty(lines, ref bit, ref bitOffset, i, property, propertyType);
                            }
                        }
                        else
                        {
                            property.TypeName = enums[0].Name;
                            property.SDK_Enum = enums[0];
                            property.IsSimpleType = true;
                        }
                    }
                    else
                    {
                        BuildStandardProperty(lines, ref bit, ref bitOffset, i, property, propertyType);
                    }
                    classInfo.Properties.Add(propertyCount, property);
                    propertyCount++;
                }
                else if (lines[i].Contains(@"// Function"))
                {
                    SDK_Function function = new SDK_Function();
                    function.Returntype = lines[i].Substring(1).Split(" ")[0];
                    function.Name = lines[i].Substring(1).Split(" ")[1].Split("(")[0];
                    classInfo.Functions.Add(function);
                }
            }

            return classInfo;
        }

        private static void BuildStandardProperty(string[] lines, ref int bit, ref int bitOffset, int i, SDK_Property property, string propertyType)
        {
            property.TypeName = propertyType;
            string name = lines[i].Split(" ")[1].Split(";")[0];
            if (lines[i].Contains(":"))
            {
                property.Name = lines[i].Split(" ")[1].Split(" : ")[0];
                property.IsBitSize = true;
                if (bitOffset == property.Offset)
                {
                    property.BitNumber = bit;
                    bit += Convert.ToInt32(lines[i].Split(" : ")[1].Split(";")[0]);
                }
                else
                {
                    property.BitNumber = 0;
                    bit = Convert.ToInt32(lines[i].Split(" : ")[1].Split(";")[0]);
                }
                bitOffset = property.Offset;
                name = name.Split(":")[0].Trim();
            }
            property.IsSimpleType = true;
            property.Name = name;
        }

        private static int GetClassSize(string[] codetext)
        {
            //var c = fullSDKDetailed.SelectMany(s => s.Value).Where(c => c.Name.ToLower() == typeName.ToLower());
            if (codetext.Any(l => l.StartsWith("// Size")))
            {
                var line = codetext.First(l => l.StartsWith("// Size"));
                var offsetHex = line.Substring(9, line.IndexOf(" (") - 9);
                var size = Convert.ToInt32(offsetHex, 16);

                return size;
            }
            return 0;
        }

        private static int GetClassSizeFromName(string typeName)
        {
            var c = fullSDKDetailed.SelectMany(s => s.Value.Values).Where(c => c.Name.ToLower() == typeName.ToLower());
            if (c.Any() && c.First().CodeText.Any(l => l.StartsWith("// Size")))
            {
                var line = c.First().CodeText.First(l => l.StartsWith("// Size"));
                var offsetHex = line.Substring(9, line.IndexOf(" (") - 9);
                var size = Convert.ToInt32(offsetHex, 16);

                return size;
            }
            return 0;
        }

        //Memory SDK methods
        //static List<ulong> uobjects = new List<ulong>();
        static Dictionary<string, int> uobjectsClassesDict = new Dictionary<string, int>();
        static Dictionary<string, int> offsets = new Dictionary<string, int>();
        static MemoryReader mem;

        static List<string> hiddenProperties = new List<string>
        {
            "Actor.actorId",
            "SceneComponent.ComponentToWorld",
            "SceneComponent.Bounds",
            "Player.PlayerController",
            "FishingFloatNameplateComponent.FishName",
            "SceneComponent.ActorCoordinates",
            "SceneComponent.ActorRotation"
        };

        public static uint GetOffset(string offsetName, bool forceMemory = false)
        {
            //if(offsetName == "Actor.actorId")
            //    return 24;

            if (!bool.Parse(ConfigurationManager.AppSettings["ReadOffsetsFromMemory"]) && !forceMemory)
                return (uint)SoT_Tool.offsets[offsetName];

            if (offsets.ContainsKey(offsetName))
            {
                var offset = offsets[offsetName];
                return (uint)offset;
            }
            else
            {
                var classname = offsetName.Split('.').First();

                GetOffsets(classname);
                if (offsets.ContainsKey(offsetName))
                {
                    var offset = offsets[offsetName];
                    return (uint)offset;
                }
                else if(offsetName.EndsWith(".Count") && offsets.ContainsKey(offsetName.Replace(".Count", "")))
                {
                    var offset = offsets[offsetName.Replace(".Count", "")] + 8;
                    offsets[offsetName] = offset;
                    return (uint)offset;
                }
                else if(SoT_Tool.offsets.ContainsKey(offsetName))
                {
                    if(hiddenProperties.Contains(offsetName)) //KeyValuePair`2 "[HullDamage.ActiveHullDamageZones, 1056]"
                        offsets[offsetName] = SoT_Tool.offsets[offsetName]; 
                    return (uint)SoT_Tool.offsets[offsetName];
                }
                else
                    return 0;
            }
        }

        private static Offsets SoTOffsets = new Offsets
        {
            fNameEntry = new Offsets.FNameEntry { HeaderSize = 0x10 },
            uObject = new Offsets.UObject { Index = 0xC, Class = 0x10, Name = 0x18, Outer = 0x20 },
            uField = new Offsets.UField { Next = 0x28 },
            uStruct = new Offsets.UStruct { SuperStruct = 0x30, Children = 0x38, PropertiesSize = 0x40 },
            uEnum = new Offsets.UEnum { Names = 0x40, NamesElementSize = 0xC },
            uFunction = new Offsets.UFunction { FunctionFlags = 0x88, Func = 0xB0 },
            uProperty = new Offsets.UProperty { ArrayDim = 0x30, ElementSize = 0x34, PropertyFlags = 0x38, Offset = 0x4C, Size = 0x70 }
        };

        static bool isReading = false;

        public static void ReadGObjects()
        {
            if (isReading)
                return;
            isReading = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var gobjects = SoT_Tool.g_objects;
            mem = SoT_Tool.mem;

            var objectsPtr = mem.ReadULong(gobjects);
            //var preallocated = mem.ReadInt(gobjects + 4);
            var maxElements = mem.ReadInt(gobjects + 4 + 4);
            var numElements = mem.ReadInt(gobjects + 4 + 4 + 4);
            //var maxChunks = mem.ReadInt(gobjects + 4+4+4+4);
            //var numChunks = mem.ReadInt(gobjects + 4+4+4+4+4);
            //var numChunks = mem.ReadByte(gobjects + 4);
            //var maxChunks = mem.ReadByte(gobjects + 4 + 1);
            //var test2 = mem.ReadByte(gobjects + 4 + 2);
            //var test3 = mem.ReadByte(gobjects + 4 + 3);

            for (uint i = 0; i < numElements; i++)
            {
                try
                {
                    var obj = mem.ReadULong(objectsPtr + (ulong)i * 24);
                    //GetObjectPtr(i);
                    if (obj == 0)
                    {
                        continue;
                    }
                    //uobjects.Add(obj);
                    //uobjects.Add(obj.Value);
                    //Console.WriteLine($"Found Object Pointer [{obj}]");

                    string fullname = "";
                    //index = uobjects.IndexOf(o);
                    var className = mem.ReadRawname(mem.ReadULong(obj + (ulong)SoTOffsets.uObject.Class));

                    if (className != "Class" && className != "ScriptStruct" && className != "BlueprintGeneratedClass")
                        continue;
                    //if(className.Contains('/'))
                    var objActorId = SoT_Tool.GetActorId(obj);
                    var objName = mem.ReadGname(objActorId);
                    if (objName == "NoStringFound")// || !offsetClassNames.Contains(objName))
                        continue;

                    objName = objName.Split('/').Last();
                    //objName = objName.Split('_').Last();

                    //var outer = mem.ReadULong(obj + (ulong)SoTOffsets.uObject.Outer);
                    //string outerName = "";
                    //if (outer != 0)
                    //{
                    //    outerName = mem.ReadRawname(outer);
                    //    if (outerName == "NoStringFound")
                    //    {
                    //        continue;
                    //    }
                    //    outerName = outerName.Split('/').Last() + ".";
                    //    fullname = $"{className} {outerName}{objName}";
                    //}
                    //else
                    //{
                    //    fullname = $"{className} {objName}";
                    //}
                    uobjectsClassesDict[objName] = (int)i;//.TryAdd(objName, (int)i);
                }
                catch (Exception e)
                {
                    //Console.WriteLine(e.Message);
                }
            }
            int totaltime = stopwatch.Elapsed.Milliseconds;
            stopwatch.Reset();
            isReading = false;
        }

        private static void GetOffsets(string className)
        {
            if(!uobjectsClassesDict.ContainsKey(className))
            {
                ReadGObjects();
            }
            if (!uobjectsClassesDict.ContainsKey(className))
                return;
            var gobjects = SoT_Tool.g_objects;
            mem = SoT_Tool.mem;
            var objectsPtr = mem.ReadULong(gobjects);
            var gobjectId = uobjectsClassesDict[className];
            var gobject = mem.ReadULong(objectsPtr + (ulong)gobjectId * 24);

            var children = mem.ReadULong(gobject + (ulong)SoTOffsets.uStruct.Children);
            var objActorId = SoT_Tool.GetActorId(gobject);
            var objName = mem.ReadGname(objActorId);
            var size = mem.ReadInt(gobject + (ulong)SoTOffsets.uStruct.PropertiesSize);
            if (size > 0)
                offsets.TryAdd($"{objName}.Size", size);
            //var childrenCount = mem.ReadInt(o + (ulong)SoTOffsets.uStruct.Children + 8);
            //var properties = mem.ReadULong(o + (ulong)SoTOffsets.uStruct.PropertiesSize);
            for (ulong child = children; child > 0; child = mem.ReadULong(child + (ulong)SoTOffsets.uField.Next))
            {
                int actorId = 0;
                string propname = "";
                int offset = 0;
                try
                {
                    actorId = SoT_Tool.GetActorId(child);
                    if (actorId == 0)
                        continue;
                    propname = mem.ReadGname(actorId);
                    offset = mem.ReadInt(child + (ulong)SoTOffsets.uProperty.Offset);
                    
                    offsets.TryAdd($"{objName}.{propname}", offset);

                    //var super = mem.ReadULong(child + (ulong)SoTOffsets.uStruct.SuperStruct);
                    //var superName = mem.ReadRawname(super);
                    //var inner = mem.ReadULong(child + (ulong)SoTOffsets.uProperty.Size);
                    //var innerName = mem.ReadRawname(inner);
                    //var outerName = mem.ReadRawname(mem.ReadULong(inner + (ulong)SoTOffsets.uObject.Outer));
                    //var inner2 = mem.ReadULong(inner + (ulong)SoTOffsets.uProperty.Size);
                    //var inner2Name = mem.ReadRawname(inner2);
                    //var outerName2 = mem.ReadRawname(mem.ReadULong(inner2 + (ulong)SoTOffsets.uObject.Outer));

                    ////var inner3 = mem.ReadULong(inner2 + (ulong)SoTOffsets.uProperty.Size);
                    ////var inner3Name = mem.ReadRawname(inner3);
                    ////var offset2 = mem.ReadInt(inner + (ulong)SoTOffsets.uProperty.Offset);

                    //if (innerName.ToLower().Contains("array"))
                    //{
                    //    offsets.TryAdd($"{objName}.{propname}.Count", offset+8);
                    //}
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        //var outerName = mem.ReadRawname(outer);

        //if(className == "Class")
        //{
        //    fullname = $"{className} {objName}";
        //    var propertiesSize = mem.ReadInt(o + (ulong)SoTOffsets.uStruct.PropertiesSize);
        //    var super = mem.ReadULong(o + (ulong)SoTOffsets.uStruct.SuperStruct);
        //    var children = mem.ReadULong(o + (ulong)SoTOffsets.uStruct.Children);
        //    var childrenCount = mem.ReadInt(o + (ulong)SoTOffsets.uStruct.Children + 8);
        //    var properties = mem.ReadULong(o + (ulong)SoTOffsets.uStruct.PropertiesSize);
        //    for (ulong child = children; child > 0; child= mem.ReadULong(child + (ulong)SoTOffsets.uField.Next))
        //    {
        //        var actorId = SoT_Tool.GetActorId(child);
        //        if(actorId == 0)
        //            continue;
        //        var propname = mem.ReadGname(actorId);
        //        var offset = mem.ReadInt(child + (ulong)SoTOffsets.uProperty.Offset);
        //        offsets.Add($"{objName}.{propname}", offset);
        //    }
        //}
        //else if (className == "Struct")
        //{
        //    fullname = $"{className} {objName}";
        //}
        //else if (className == "Package")
        //{
        //    fullname = $"{className} {objName}";
        //}
        //else if (className == "Enum")
        //{
        //    fullname = $"{className} {objName}";
        //    //var enumNames = mem.ReadULong(o + (ulong)SoTOffsets.uEnum.Names);
        //    //var enumNamesCount = mem.ReadInt(o + (ulong)SoTOffsets.uEnum.Names + 8);
        //    //for(int i = 0; i < enumNamesCount; i++)
        //    //{
        //    //    var enumName = mem.ReadRawname(enumNames + (ulong)i * (ulong)SoTOffsets.uEnum.NamesElementSize);
        //    //    //fullname += $" {enumName}";
        //    //}
        //}
        //else if (className.Contains("Array"))
        //{
        //    var inner = mem.ReadULong(o + (ulong)SoTOffsets.uProperty.Size);
        //    var offset = mem.ReadInt(o + (ulong)SoTOffsets.uProperty.Offset);
        //    var innerName = mem.ReadRawname(inner);
        //    var inner2 = mem.ReadULong(inner + (ulong)SoTOffsets.uProperty.Size);
        //    var inner2Name = mem.ReadRawname(inner2);
        //    var inner3 = mem.ReadULong(inner2 + (ulong)SoTOffsets.uProperty.Size);
        //    var inner3Name = mem.ReadRawname(inner3);
        //    var offset2 = mem.ReadInt(inner + (ulong)SoTOffsets.uProperty.Offset);

        //    fullname = $"{className} {outerName}{objName}";
        //}
        //else if(Enum.IsDefined(typeof(PropertyType), className))
        //{
        //    fullname = $"{className} {outerName}{objName}";
        //}
        //else // unknown
        //{
        //    fullname = $"{className} {outerName}{objName}";
        //}

        public static List<string> ReadGnames()
        {
            List<string> list = new List<string>();
            //var g_objects_offset = mem.ReadUInt((UIntPtr)(g_object_base.ToUInt64() + 2));
            //var g_objects = g_object_base.ToUInt64() + g_objects_offset + 22;
            //var g_objects_ptr = mem.ReadUInt((UIntPtr)g_objects);
            //var g_objects_count = mem.ReadUInt((UIntPtr)g_objects +8);

            //var g_name = g_name_start_address;
            //var g_name_address = mem.ReadULong((UIntPtr)g_name);
            //var g_name_count = mem.ReadUInt((UIntPtr)g_name + 8);
            try
            {
                for (int i = 0; i < 500000; i++)
                {
                    //var next_g_name_address = mem.ReadULong((UIntPtr)g_name_address + i * 8);
                    var g_name_string = mem.ReadGname(i);
                    list.Add(g_name_string);
                }
            }
            catch (Exception e)
            {
                SoT_DataManager.InfoLog += $"Error reading gnames: {e.Message}\n";
            }
            return list;
        }

    }

    public enum PropertyType
    {
        Unknown,
	    StructProperty,
	    ObjectProperty,
	    SoftObjectProperty,
	    FloatProperty,
	    ByteProperty,
	    BoolProperty,
	    IntProperty,
	    Int8Property,
	    Int16Property,
	    Int64Property,
	    UInt16Property,
	    UInt32Property,
	    UInt64Property,
	    NameProperty,
	    DelegateProperty,
	    SetProperty,
	    ArrayProperty,
	    WeakObjectProperty,
	    StrProperty,
	    TextProperty,
	    MulticastSparseDelegateProperty,
	    EnumProperty,
	    DoubleProperty,
	    MulticastDelegateProperty,
	    ClassProperty,
	    MulticastInlineDelegateProperty,
	    MapProperty,
	    InterfaceProperty,
        NumericProperty
    };

    public enum EFunctionFlags : uint
    {
        // Function flags.
        FUNC_None = 0x00000000,

        FUNC_Final = 0x00000001,    // Function is final (prebindable, non-overridable function).
        FUNC_RequiredAPI = 0x00000002,  // Indicates this function is DLL exported/imported.
        FUNC_BlueprintAuthorityOnly = 0x00000004,   // Function will only run if the object has network authority
        FUNC_BlueprintCosmetic = 0x00000008,   // Function is cosmetic in nature and should not be invoked on dedicated servers
                                               // FUNC_				= 0x00000010,   // unused.
                                               // FUNC_				= 0x00000020,   // unused.
        FUNC_Net = 0x00000040,   // Function is network-replicated.
        FUNC_NetReliable = 0x00000080,   // Function should be sent reliably on the network.
        FUNC_NetRequest = 0x00000100,   // Function is sent to a net service
        FUNC_Exec = 0x00000200, // Executable from command line.
        FUNC_Native = 0x00000400,   // Native function.
        FUNC_Event = 0x00000800,   // Event function.
        FUNC_NetResponse = 0x00001000,   // Function response from a net service
        FUNC_Static = 0x00002000,   // Static function.
        FUNC_NetMulticast = 0x00004000, // Function is networked multicast Server -> All Clients
        FUNC_UbergraphFunction = 0x00008000,   // Function is used as the merge 'ubergraph' for a blueprint, only assigned when using the persistent 'ubergraph' frame
        FUNC_MulticastDelegate = 0x00010000,    // Function is a multi-cast delegate signature (also requires FUNC_Delegate to be set!)
        FUNC_Public = 0x00020000,   // Function is accessible in all classes (if overridden, parameters must remain unchanged).
        FUNC_Private = 0x00040000,  // Function is accessible only in the class it is defined in (cannot be overridden, but function name may be reused in subclasses.  IOW: if overridden, parameters don't need to match, and Super.Func() cannot be accessed since it's private.)
        FUNC_Protected = 0x00080000,    // Function is accessible only in the class it is defined in and subclasses (if overridden, parameters much remain unchanged).
        FUNC_Delegate = 0x00100000, // Function is delegate signature (either single-cast or multi-cast, depending on whether FUNC_MulticastDelegate is set.)
        FUNC_NetServer = 0x00200000,    // Function is executed on servers (set by replication code if passes check)
        FUNC_HasOutParms = 0x00400000,  // function has out (pass by reference) parameters
        FUNC_HasDefaults = 0x00800000,  // function has structs that contain defaults
        FUNC_NetClient = 0x01000000,    // function is executed on clients
        FUNC_DLLImport = 0x02000000,    // function is imported from a DLL
        FUNC_BlueprintCallable = 0x04000000,    // function can be called from blueprint code
        FUNC_BlueprintEvent = 0x08000000,   // function can be overridden/implemented from a blueprint
        FUNC_BlueprintPure = 0x10000000,    // function can be called from blueprint code, and is also pure (produces no side effects). If you set this, you should set FUNC_BlueprintCallable as well.
        FUNC_EditorOnly = 0x20000000,   // function can only be called from an editor scrippt.
        FUNC_Const = 0x40000000,    // function can be called from blueprint code, and only reads state (never writes state)
        FUNC_NetValidate = 0x80000000,  // function must supply a _Validate implementation

        FUNC_AllFlags = 0xFFFFFFFF,
    };
}
