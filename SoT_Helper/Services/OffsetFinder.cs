using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace SoT_Helper.Services
{
    public class OffsetFinder
    {
        static List<string> ArrayCounts;
        static List<string> ClassSize;
        public static async Task UpdateOffsets(string SDKPath = "h:\\Modding\\Sea of Thieves\\UnrealDumper-4.25-UnrealDumper-4.10\\UnrealDumper-4.25-UnrealDumper-4.10\\bin\\Debug\\Games\\SoTGame\\DUMP\\")
        {
            Dictionary<string, int>? newOffsets = new Dictionary<string, int>();
            ArrayCounts = new List<string>();
            ClassSize = new List<string>();
            var updates = new List<string>();


            SoT_DataManager.InfoLog += "\nScanning SDK in folder " + SDKPath;

            DirectoryInfo directoryInfo = new DirectoryInfo(SDKPath);
            var SDKfiles = directoryInfo.GetFiles().Where(f => f.Extension == ".h").ToList();

            if(SoT_Tool.offsets.IsEmpty)
            {
                MessageBox.Show("No offsets found. Is the offsets.json file in the SoT Helper app folder?");
                return;
            }

            if(SDKfiles.Count == 0)
            {
                MessageBox.Show("No SDK files found in folder " + SDKPath);
                return;
            }

            //To speed up the search, we will look in the main files first
            List<string> names = new List<string>() { "Athena_classes.h", "Athena_struct.h", "Engine_classes.h", "Engine_struct.h" };

            var mainFiles = SDKfiles.Where(f => names.Contains(f.Name)).ToList();
            SDKfiles.RemoveAll(f => mainFiles.Contains(f));
            SDKfiles.InsertRange(0, mainFiles);

            //Another speed up, we will read all the lines of the files into a single array to avoid having to search through the files multiple times
            List<string> fullSDKlist = new List<string>();
            SoT_DataManager.InfoLog += "\nReading SDK files into memory.";
            foreach (var f in SDKfiles)
            {
                fullSDKlist.AddRange(File.ReadAllLines(f.FullName));
            }
            string[] fullSDK = fullSDKlist.ToArray();
            fullSDKlist.Clear();

            SoT_DataManager.InfoLog += "\nSDK files stored in memory. Finding offsets...";

            int found = 0;
            int updatesFound = 0;
            foreach (var offsetName in SoT_Tool.offsets.Keys.ToList())
            {
                int oldOffset = SoT_Tool.offsets[offsetName];
                var offsetFound = FindSDKOffset(offsetName, fullSDK);
                //SoT_Tool.offsets[offset]
                if (offsetFound == -1 || oldOffset == offsetFound)
                {
                    if(!newOffsets.ContainsKey(offsetName))
                        newOffsets.Add(offsetName, oldOffset);

                    if (oldOffset == offsetFound)
                        found++;
                }
                else
                {
                    found++;
                    updatesFound++;
                    newOffsets.Add(offsetName, offsetFound);
                    updates.Add(offsetName);
                }
                if (ArrayCounts.Contains(offsetName))
                {
                    found++;
                    if (newOffsets.ContainsKey(offsetName + ".Count"))
                    {
                        newOffsets[offsetName + ".Count"] = offsetFound + 8;
                    }
                    else
                    {
                        newOffsets.Add(offsetName + ".Count", offsetFound + 8);
                    }
                }
            }
            SoT_DataManager.InfoLog += $"\nOffsets scanned. Found {found} offsets and updated {updatesFound} offsets.";
            //foreach(var c in updates)
            //{
            //    SoT_DataManager.InfoLog += $"\n{c}";
            //}
            foreach (var cSize in ClassSize)
            {
                var offsetFound = FindSDKOffset(cSize + ".Size", fullSDK);
                if (offsetFound != -1)
                {
                    found++;
                    if (newOffsets.ContainsKey(cSize + ".Size"))
                    {
                        newOffsets[cSize + ".Size"] = offsetFound;
                    }
                    else
                    {
                        newOffsets.Add(cSize + ".Size", offsetFound);
                    }
                }
            }

            // Convert the updated list to a JSON string and write it back to the file
            string updatedJson = JsonConvert.SerializeObject(newOffsets, Formatting.Indented);
            List<string> updatedOffsets = OrderJsonLines(updatedJson);
            string number = "";
            if(updatesFound > 0)
            {
                if(updatesFound == 1)
                    SoT_DataManager.InfoLog += $"\n{updates.First()} got a new offset from this SDK scan.";
                else
                    SoT_DataManager.InfoLog += $"\n{updates.Aggregate((a, b) => a + ", " + b)} got new offsets from this SDK scan.";

                if (File.Exists("offsets.json.bak"))
                {
                    while (true)
                    {
                        Random rnd = new Random();
                        number = "" + rnd.Next(1, 1000);
                        if (!File.Exists("offsets.json.bak" + number))
                            break;
                    }
                }
                if (File.Exists("offsets.json"))
                {
                    SoT_DataManager.InfoLog += "\nRenaming offsets.json to offsets.json.bak" + number;
                    File.Move("offsets.json", "offsets.json.bak" + number);
                }
                File.WriteAllLines("offsets.json", updatedOffsets);
                SoT_Tool.offsets = newOffsets.ToImmutableDictionary<string, int>();
                SoT_DataManager.InfoLog += "\nOffsets in memory and offsets.json updated.";
            }
            else
            {
                SoT_DataManager.InfoLog += "\nNo offsets were updated. No need to update offsets.json";
            }
            SoT_DataManager.InfoLog += "\nGarbage collecting to clean up SDK scan from memory.";
            GC.Collect(GC.GetGeneration(fullSDK));
            GC.Collect(GC.MaxGeneration);

            ConfigurationManager.AppSettings["SDKPath"] = SDKPath;
        }

        public static async Task UpdateOffsetsFromMemory()
        {
            SoT_DataManager.InfoLog += "\nSDK files stored in memory. Finding offsets...";
            Dictionary<string, int>? newOffsets = new Dictionary<string, int>();
            ArrayCounts = new List<string>();
            ClassSize = new List<string>();
            var updates = new List<string>();
            var notFound = new List<string>();
            int found = 0;
            int updatesFound = 0;
            foreach (var offsetName in SoT_Tool.offsets.Keys.ToList())
            {
                int oldOffset = SoT_Tool.offsets[offsetName];
                var offsetFound = SDKService.GetOffset(offsetName, true);
                //SoT_Tool.offsets[offset]
                if (offsetFound == -1 || oldOffset == offsetFound)
                {
                    if (!newOffsets.ContainsKey(offsetName))
                    {
                        notFound.Add(offsetName);
                        newOffsets.Add(offsetName, oldOffset);
                    }

                    if (oldOffset == offsetFound)
                        found++;
                }
                else
                {
                    found++;
                    updatesFound++;
                    newOffsets.Add(offsetName, (int)offsetFound);
                    updates.Add(offsetName);
                }
            }
            SoT_DataManager.InfoLog += $"\nOffsets scanned. Found {found} offsets and updated {updatesFound} offsets.";
            // Convert the updated list to a JSON string and write it back to the file
            string updatedJson = JsonConvert.SerializeObject(newOffsets, Formatting.Indented);
            List<string> updatedOffsets = OrderJsonLines(updatedJson);
            string number = "";
            if (updatesFound > 0)
            {
                if (updatesFound == 1)
                    SoT_DataManager.InfoLog += $"\n{updates.First()} got a new offset from this SDK scan.";
                else
                    SoT_DataManager.InfoLog += $"\n{updates.Aggregate((a, b) => a + ", " + b)} got new offsets from this SDK scan.";

                SoT_DataManager.InfoLog += $"\n{notFound.Aggregate((a, b) => a + ", " + b)} did not find offsets from this SDK scan and will keep the old offsets.";

                if (File.Exists("offsets.json.bak"))
                {
                    while (true)
                    {
                        Random rnd = new Random();
                        number = "" + rnd.Next(1, 1000);
                        if (!File.Exists("offsets.json.bak" + number))
                            break;
                    }
                }
                if (File.Exists("offsets.json"))
                {
                    SoT_DataManager.InfoLog += "\nRenaming offsets.json to offsets.json.bak" + number;
                    File.Move("offsets.json", "offsets.json.bak" + number);
                }
                File.WriteAllLines("offsets.json", updatedOffsets);
                SoT_Tool.offsets = newOffsets.ToImmutableDictionary<string, int>();
                SoT_DataManager.InfoLog += "\nOffsets in memory and offsets.json updated.";
            }
            else
            {
                SoT_DataManager.InfoLog += "\nNo offsets were updated. No need to update offsets.json";
            }
        }

        //Takes a json string, splits it into lines, orders the lines, and returns the lines as a list
        public static List<string> OrderJsonLines(string updatedJson)
        {
            List<string> updatedOffsets = updatedJson.Split("\r\n").ToList();
            if(updatedOffsets.Count() < 3)
            {
                return updatedOffsets;
            }
            //Add comma to last line
            updatedOffsets[updatedOffsets.Count()-2] = updatedOffsets[updatedOffsets.Count() - 2] + ",";
            var ordered = updatedOffsets.Skip(1).Take(updatedOffsets.Count() - 2).OrderBy(s => s).ToList();
            updatedOffsets.RemoveRange(1, updatedOffsets.Count() - 2);
            updatedOffsets.InsertRange(1, ordered);
            //Remove comma from last line
            updatedOffsets[updatedOffsets.Count() - 2] = updatedOffsets[updatedOffsets.Count() - 2].Replace(",", "");
            return updatedOffsets;
        }

        public static int FindSDKOffset(string ClassVariable, string[] fullSDK)
        {
            int offset = -1;

            var classname = ClassVariable.Substring(0, ClassVariable.IndexOf("."));
            var variable = ClassVariable.Substring(ClassVariable.IndexOf(".") + 1);
            if (variable.EndsWith("Count"))
                return -1;

            for (int i = 0; i < fullSDK.Length; i++)
            {
                if (fullSDK[i].EndsWith("." + classname) && fullSDK[i].StartsWith($"// "))
                {
                    var linefound = fullSDK[i];
                    if (variable == "Size")
                    {
                        var debugLine1 = fullSDK[i];
                        var debugLine2 = fullSDK[i + 1];
                        var offsetHex = fullSDK[i + 1].Substring(9, fullSDK[i + 1].IndexOf(" (") - 9);
                        offset = Convert.ToInt32(offsetHex, 16);
                        break;
                    }
                    else
                    {
                        for (int j = i; j < fullSDK.Length; j++)
                        {
                            if (fullSDK[j].Contains(" " + variable + ";"))
                            {
                                var line = fullSDK[j];
                                int start = fullSDK[j].IndexOf("; // ") + 5;
                                var end = fullSDK[j].IndexOf("(");
                                var offsetHex = fullSDK[j].Substring(start, end - start);

                                if(!offsetHex.StartsWith("0x"))
                                    continue;

                                offset = Convert.ToInt32(offsetHex, 16);

                                if (line.ToLower().Contains("tarray"))
                                {
                                    ArrayCounts.Add(ClassVariable);
                                    if (line.Contains("<struct"))
                                    {
                                        var classStart = line.IndexOf("<struct ") + 8;
                                        var classEnd = fullSDK[j].IndexOf(">");
                                        var classLength = classEnd - classStart;
                                        var className = line.Substring(classStart, classLength);
                                        ClassSize.Add(className);
                                    }
                                }
                                break;
                            }
                            if (fullSDK[j].Contains("};"))
                            {
                                break;
                            }
                        }
                    }
                }
            }
            return offset;
        }
    }
}
