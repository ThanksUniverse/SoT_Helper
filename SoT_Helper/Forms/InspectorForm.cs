using Microsoft.VisualBasic;
using SoT_Helper.Models;
using SoT_Helper.Models.SDKHelper;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace SoT_Helper
{
    public partial class InspectorForm : Form
    {
        private List<TreeNode> nodeList = new List<TreeNode>();

        static TreeNode selectedNode;

        static private MemoryReader mem;

        public InspectorForm()
        {
            InitializeComponent();
            mem = SoT_Tool.mem;
            var actors = SoT_DataManager.Actors.Values.ToList();
            actors.Sort((a, b) => a.RawName.CompareTo(b.RawName));
            foreach (var actor in actors)
            {
                var node = new TreeNode(actor.ActorAddress.ToString("X") + " : " + actor.RawName);
                node.Tag = actor;
                treeView1.Nodes.Add(node);
                node.Name = actor.ActorAddress.ToString();
                nodeList.Add(node);
            }

            var list = SDKService.fullSDKDetailed.SelectMany(c => c.Value.Values.ToArray()).ToList();
            list.Sort((a, b) => a.Name.CompareTo(b.Name));
            foreach (var sdk in list)
            {
                //SDKClassComboBox.SelectedIndex = 0;
                SDKClassComboBox.Items.Add(sdk);
            }
            SDKClassComboBox.DisplayMember = "Name";

            //if (!SDKService.fullSDKDetailed.Any())
            //{
            //    FolderBrowserDialog folderDlg = new System.Windows.Forms.FolderBrowserDialog();
            //    folderDlg.ShowNewFolderButton = true;

            //    if (!Directory.Exists(ConfigurationManager.AppSettings["SDKPath"]))
            //    {
            //        // Show the FolderBrowserDialog.  
            //        DialogResult result = folderDlg.ShowDialog();
            //        if (result == DialogResult.OK)
            //        {
            //            Environment.SpecialFolder root = folderDlg.RootFolder;
            //            Task.Run(async () =>
            //            {
            //                await OffsetFinder.UpdateOffsets(folderDlg.SelectedPath);
            //            });
            //        }
            //    }
            //    else
            //    {
            //        var task = Task.Run(async () =>
            //        {
            //            await SDKService.ScanSDK(ConfigurationManager.AppSettings["SDKPath"]);
            //            //await OffsetFinder.UpdateOffsets(ConfigurationManager.AppSettings["SDKPath"]);
            //        });
            //        task.GetAwaiter().GetResult();
            //        var list = SDKService.fullSDKDetailed.SelectMany(c => c.Value.Values.ToArray()).ToList();
            //        list.Sort((a, b) => a.Name.CompareTo(b.Name));
            //        foreach (var sdk in list)
            //        {
            //            SDKClassComboBox.Items.Add(sdk);
            //        }
            //        SDKClassComboBox.SelectedIndex = 0;
            //        SDKClassComboBox.DisplayMember = "Name";
            //    }
            //}
            //else
            //{
            //    var list = SDKService.fullSDKDetailed.SelectMany(c => c.Value.Values.ToArray()).ToList();
            //    list.Sort((a, b) => a.Name.CompareTo(b.Name));
            //    foreach (var sdk in list)
            //    {
            //        //SDKClassComboBox.SelectedIndex = 0;
            //        SDKClassComboBox.Items.Add(sdk);
            //    }
            //    SDKClassComboBox.DisplayMember = "Name";
            //}

            timer1.Interval = 500; // 1000 / 60 = refresh 60 times per second
                                   //300000;//5 minutes

            timer1.Tick += new System.EventHandler(timer1_Tick);
            timer1.Start();

            // Create a new ContextMenuStrip
            ContextMenuStrip menu = new ContextMenuStrip();

            // Add menu items to it
            ToolStripMenuItem menuItem = new ToolStripMenuItem("Copy node hex address");
            menu.Items.Add(menuItem);
            ToolStripMenuItem menuItem4 = new ToolStripMenuItem("Copy node numeric address");
            menu.Items.Add(menuItem4);
            ToolStripMenuItem menuItem2 = new ToolStripMenuItem("Copy node text");
            menu.Items.Add(menuItem2);
            ToolStripMenuItem menuItem3 = new ToolStripMenuItem("Delete node");
            menu.Items.Add(menuItem3);
            ToolStripMenuItem menuItem6 = new ToolStripMenuItem("Delete nodes childnodes");
            menu.Items.Add(menuItem6);
            ToolStripMenuItem menuItem5 = new ToolStripMenuItem("Change value");
            menu.Items.Add(menuItem5);

            // Set the TreeView's ContextMenuStrip property
            AttachedTreeView.ContextMenuStrip = menu;

            // Add an event handler for the TreeView's NodeMouseClick event
            AttachedTreeView.NodeMouseClick += (s, e) =>
            {
                // Save the clicked node in the Tag property of the ContextMenuStrip
                AttachedTreeView.ContextMenuStrip.Tag = e.Node;
            };

            // Add an event handler for the menu item
            menuItem.Click += (s, e) =>
            {
                // Get the clicked node from the Tag property of the ContextMenuStrip
                TreeNode clickedNode = AttachedTreeView.ContextMenuStrip.Tag as TreeNode;

                if (clickedNode != null)
                {
                    //MessageBox.Show("You clicked Menu Item 1 for node: " + clickedNode.Text);
                    BasicActor actor = (BasicActor)clickedNode.Tag;
                    if(clickedNode.Tag != null)
                    {
                        CopyToClipboard(actor.ActorAddress.ToString("X"));
                    }
                }
            };
            menuItem2.Click += (s, e) =>
            {
                // Get the clicked node from the Tag property of the ContextMenuStrip
                TreeNode clickedNode = AttachedTreeView.ContextMenuStrip.Tag as TreeNode;

                if (clickedNode != null)
                {
                    CopyToClipboard(clickedNode.Text);
                }
            };
            menuItem3.Click += (s, e) =>
            {
                // Get the clicked node from the Tag property of the ContextMenuStrip
                TreeNode clickedNode = AttachedTreeView.ContextMenuStrip.Tag as TreeNode;

                if (clickedNode != null)
                {
                    clickedNode.Remove();
                }
            };
            menuItem6.Click += (s, e) =>
            {
                // Get the clicked node from the Tag property of the ContextMenuStrip
                TreeNode clickedNode = AttachedTreeView.ContextMenuStrip.Tag as TreeNode;

                if (clickedNode != null)
                {
                    clickedNode.Nodes.Clear();
                }
            };
            menuItem4.Click += (s, e) =>
            {
                // Get the clicked node from the Tag property of the ContextMenuStrip
                TreeNode clickedNode = AttachedTreeView.ContextMenuStrip.Tag as TreeNode;

                if (clickedNode != null)
                {
                    //MessageBox.Show("You clicked Menu Item 1 for node: " + clickedNode.Text);
                    BasicActor actor = (BasicActor)clickedNode.Tag;
                    if (clickedNode.Tag != null)
                    {
                        CopyToClipboard(actor.ActorAddress.ToString());
                    }
                }
            };
            menuItem5.Click += (s, e) =>
            {
                // Get the clicked node from the Tag property of the ContextMenuStrip
                TreeNode clickedNode = AttachedTreeView.ContextMenuStrip.Tag as TreeNode;

                if (clickedNode != null)
                {
                    //MessageBox.Show("You clicked Menu Item 1 for node: " + clickedNode.Text);
                    BasicActor actor = (BasicActor)clickedNode.Tag;
                    if (clickedNode.Tag != null)
                    {
                        // Show the InputBox
                        string userInput = Interaction.InputBox("Change Value", $"Current value: {clickedNode.Text.Split('=')[1]} Enter New Value", clickedNode.Text.Split('=')[1]);

                        // Validate that the input is a number
                        byte[] bytes = BitConverter.GetBytes(0);

                        if (clickedNode.Name == "float")
                        {
                            var testfloat = mem.ReadFloat(actor.ActorAddress);
                            float.TryParse(userInput, out float newValue);
                            bytes = BitConverter.GetBytes(newValue).ToArray();
                            mem.WriteBytes(actor.ActorAddress, bytes.ToArray());
                        }
                        else if (clickedNode.Text.Contains("*"))
                        {
                            var test = mem.ReadULong(actor.ActorAddress);
                            ulong.TryParse(userInput, out ulong newValue);
                            bytes = BitConverter.GetBytes(newValue).ToArray();
                            mem.WriteBytes(actor.ActorAddress, bytes.ToArray());
                        }
                        else if (clickedNode.Name.ToLower().Contains("int64"))
                        {
                            var test = mem.ReadULong(actor.ActorAddress);
                            ulong.TryParse(userInput, out var newValue);
                            bytes = BitConverter.GetBytes(newValue).ToArray();
                            mem.WriteBytes(actor.ActorAddress, bytes.ToArray());
                        }
                        else if (clickedNode.Name.ToLower().Contains("int32"))
                        {
                            var test = mem.ReadInt(actor.ActorAddress);
                            int.TryParse(userInput, out var newValue);
                            bytes = BitConverter.GetBytes(newValue).ToArray();
                            mem.WriteBytes(actor.ActorAddress, bytes.ToArray());
                        }
                        else if (clickedNode.Name.ToLower().Contains("bool"))
                        {
                            var test = mem.ReadBool(actor.ActorAddress);
                            bool.TryParse(userInput, out var newValue);
                            bytes = BitConverter.GetBytes(newValue).ToArray();
                            mem.WriteBytes(actor.ActorAddress, bytes.ToArray());
                        }
                        else
                        {
                            MessageBox.Show($"Invalid input. Please enter a {clickedNode.Name}.");
                        }
                        // TODO: Validate the new value and change the node value
                        // Ensure the newValue is suitable for your case
                        //clickedNode.Text = newValue.ToString();
                        ReadPropertyValue(actor, clickedNode);

                        //{
                        //    MessageBox.Show("Invalid input. Please enter a number.");
                        //}
                    }
                }
            };
        }

        private static void CopyToClipboard(string text)
        {
            int retries = 10;

            while (retries > 0)
            {
                try
                {
                    Clipboard.SetText(text);
                    break;  // Exit the loop if the operation succeeded
                }
                catch (Exception)
                {
                    if (--retries == 0) SoT_DataManager.InfoLog += $"\nFailed to copy {text} to clipboard";  // If this was the last retry, rethrow the exception

                    System.Threading.Thread.Sleep(100);  // Wait for a short time before retrying
                }
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedNode = e.Node;
        }

        private void AttachButton_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node != null)
            {
                var actoraddress = ulong.Parse(node.Name);
            }
            var actor = (BasicActor)node.Tag;
            var classObject = (SDK_Class)SDKClassComboBox.SelectedItem;
            if (classObject == null || string.IsNullOrWhiteSpace(classObject.Name))
            {
                MessageBox.Show("Error, class not found");
                return;
            }
            classObject.Update();

            var newNode = AttachedTreeView.Nodes.Add(node.Text);
            newNode.Tag = node.Tag;
            newNode.Name = node.Name;

            if (!string.IsNullOrWhiteSpace(classObject.ParentClassName))
            {
                var parent = SDKService.GetClassFromName(classObject.ParentClassName);

                if (parent != null)
                {
                    classObject.Parent = parent;
                    //var parentNode = node.Nodes.Add("Parent");
                    //parentNode.Name = "Parent";
                    var parentNodeClass = newNode.Nodes.Add("ClassParent " + parent.Name);
                    parentNodeClass.Name = parent.Name;
                    parentNodeClass.Tag = new BasicActor() { RawName = parent.Name, ActorAddress = actor.ActorAddress };
                    //ExpandSelectedNode(parentNodeClass);
                }
            }

            if (classObject.Properties.Any())
            {
                ReadProperties(actor, classObject, newNode);
            }
        }

        private static void ReadProperties(BasicActor actor, SDK_Class classObject, TreeNode newNode)
        {
            foreach (var p in classObject.Properties)
            {
                try
                {
                    TreeNode propertyNode = new TreeNode(p.Value.GetPropertyText());

                    if (p.Value.IsArray)
                    {
                        ulong arrayAddress = mem.ReadULong(actor.ActorAddress + (ulong)p.Value.Offset);
                        int arraySize = mem.ReadInt(actor.ActorAddress + (ulong)p.Value.Offset + 8);

                        if(arrayAddress == 0 && arraySize == 0)
                        {
                        }
                        else if (arrayAddress > SoT_Tool.maxmemaddress || arrayAddress < SoT_Tool.minmemaddress || arraySize > 20000 || arraySize < 0)
                            continue;
                        if (p.Value.IsPointer)
                        {
                            for (int i = 0; i < arraySize; i++)
                            {
                                ulong arrayObjectAddress = arrayAddress + ((ulong)i * 8);
                                var arrayNode = propertyNode.Nodes.Add("Class " + p.Value.TypeName + "* [" + i + "] = "+ 
                                    arrayObjectAddress.ToString("X") + " => "+ mem.ReadULong(arrayAddress + ((ulong)i * 8)).ToString("X"));
                                //ulong arrayObjectAddress = mem.ReadULong(arrayAddress + ((ulong)i * 8));
                                arrayNode.Tag = new BasicActor() { RawName = p.Value.TypeName, ActorAddress = arrayObjectAddress };
                                arrayNode.Name = p.Value.TypeName;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < arraySize; i++)
                            {
                                var arrayNode = propertyNode.Nodes.Add(p.Value.TypeName + " [" + i + "]");
                                arrayNode.Name = p.Value.TypeName;
                                ulong arrayObjectAddress = arrayAddress + ((ulong)i * (ulong)p.Value.Size);
                                if (p.Value.IsSimpleType || p.Value.TypeName == "Guid" || p.Value.TypeName == "Vector" || p.Value.TypeName == "Name"
                                    || p.Value.TypeName == "String" || p.Value.TypeName == "Text")
                                {
                                    ReadPropertyValue(actor, p.Value, arrayNode);
                                }
                                var tag = new BasicActor() { RawName = p.Value.TypeName, ActorAddress = arrayObjectAddress };
                                arrayNode.Tag = tag;
                            }
                        }
                        propertyNode.Text += " = " + arraySize;
                    }
                    else
                    if (p.Value.IsSimpleType || p.Value.TypeName == "Guid" || p.Value.TypeName == "Vector" || p.Value.TypeName == "Name" || p.Value.TypeName == "String"
                        || p.Value.TypeName == "Text")
                    {
                        ReadPropertyValue(actor, p.Value, propertyNode);
                    }
                    //propertyNode.Name = classObject.Name + "." + p.Value.Name;
                    //propertyNode.Name = classObject.Name + "." + p.Value.Name;
                    propertyNode.Name = p.Value.TypeName;
                    if (p.Value.IsPointer && !p.Value.IsArray)
                    {
                        //var propertyAddress = mem.ReadULong(actor.ActorAddress + (ulong)p.Value.Offset);
                        var propertyAddress = actor.ActorAddress + (ulong)p.Value.Offset;
                        propertyNode.Tag = new BasicActor() { RawName = p.Value.TypeName, ActorAddress = propertyAddress };
                        propertyNode.Name = p.Value.TypeName;
                        //propertyNode.Text += " = " + mem.ReadULong(actor.ActorAddress + (ulong)p.Value.Offset).ToString("X");
                        propertyNode.Text += " = " + propertyAddress.ToString("X") + "=>" + mem.ReadULong(propertyAddress).ToString("X");
                    }
                    else if (!p.Value.IsSimpleType && !p.Value.IsArray)
                    {
                        var propertyAddress = actor.ActorAddress + (ulong)p.Value.Offset;
                        propertyNode.Tag = new BasicActor() { RawName = p.Value.TypeName, ActorAddress = propertyAddress };
                        propertyNode.Name = p.Value.TypeName;
                        //propertyNode.Text = p.Value.Name;
                    }
                    else
                    {
                        propertyNode.Tag = new BasicActor() { RawName = p.Value.TypeName, ActorAddress = (ulong)p.Value.Offset + actor.ActorAddress };
                        propertyNode.Name = p.Value.TypeName;
                        //propertyNode.Tag = actor;
                    }
                    if (SoT_Tool.offsets.Keys.Contains(propertyNode.Name))
                    {
                        propertyNode.ForeColor = Color.Green;
                        propertyNode.Checked = true;
                    }
                    else
                    {
                        propertyNode.ForeColor = Color.Blue;
                        propertyNode.Checked = false;
                    }
                    bool found = false;
                    foreach (var node in newNode.Nodes)
                    {
                        if (node is TreeNode treeNode)
                        {
                            var n = (TreeNode)node;
                            if (n.Tag is BasicActor)
                            {
                                var bactor = (BasicActor)n.Tag;
                                var bactor2 = ((BasicActor)propertyNode.Tag);
                                if (bactor.ActorAddress == bactor2.ActorAddress && bactor.RawName == bactor2.RawName)
                                {
                                    found = true;
                                }
                            }
                        }
                    }

                    if (found)
                        continue;
                    else
                        newNode.Nodes.Add(propertyNode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void ReadPropertyValue(BasicActor actor, TreeNode propertyNode)
        {
            var oldText = propertyNode.Text;
            if (propertyNode.Text.Contains(" = "))
                propertyNode.Text = propertyNode.Text.Split(" = ")[0];
            if (actor.RawName.ToLower().Contains("int32"))
                propertyNode.Text += " = " + mem.ReadInt(actor.ActorAddress);
            else if (actor.RawName.ToLower().Contains("int64"))
                propertyNode.Text += " = " + mem.ReadULong(actor.ActorAddress);
            else if (actor.RawName.StartsWith("E"))
            {
                int enumValue = mem.ReadByte(actor.ActorAddress);
                SDKService.SDK_Enums.TryGetValue(actor.RawName, out var enumObject);
                enumObject.Values.TryGetValue(enumValue, out var enumName);
                propertyNode.Text += " = " + enumName;
            }
            else if (actor.RawName == "float")
                propertyNode.Text += " = " + mem.ReadFloat(actor.ActorAddress);
            else if (actor.RawName == "Name")
                propertyNode.Text += " = " + mem.ReadFName(actor.ActorAddress);
            else if (actor.RawName == "Text")
                propertyNode.Text += " = " + mem.ReadFText(actor.ActorAddress);
            else if (actor.RawName == "String")
                propertyNode.Text += " = " + mem.ReadFString(actor.ActorAddress);
            else if (actor.RawName.ToLower() == "bool") // || p.IsBitSize)
            {
                //if (p.IsBitSize)
                //{
                //    propertyNode.Text += " = " + mem.ReadBool(actor.ActorAddress + (ulong)p.Offset, p.BitNumber);
                //}
                //else
                propertyNode.Text += " = " + mem.ReadBool(actor.ActorAddress);
            }
            else if (actor.RawName == "Guid")
                propertyNode.Text += " = " + mem.ReadGuid(actor.ActorAddress);
            else if (actor.RawName == "Vector" || actor.RawName == "Rotator")
                propertyNode.Text += " = " + mem.ReadVector3(actor.ActorAddress);
            else if (actor.RawName == "char")
                propertyNode.Text += " = " + mem.ReadByte(actor.ActorAddress);
            else if (propertyNode.Text.Contains("*"))
            {
                propertyNode.Text += " = " + actor.ActorAddress.ToString("X") +"=>"+ mem.ReadULong(actor.ActorAddress).ToString("X");
            }
            else
            {
                propertyNode.Text += " => " + actor.ActorAddress.ToString("X");
            }
        }

        private static void ReadPropertyValue(BasicActor actor, SDK_Property p, TreeNode propertyNode)
        {
            if (p.TypeName.ToLower().Contains("int32"))
                propertyNode.Text += " = " + mem.ReadInt(actor.ActorAddress + (ulong)p.Offset);
            else if (p.SDK_Enum != null)
            {
                int enumValue = mem.ReadByte(actor.ActorAddress + (ulong)p.Offset);
                p.SDK_Enum.Values.TryGetValue(enumValue, out var enumName);
                if (enumName != null)
                    propertyNode.Text += " = " + enumName;
            }
            else if (p.TypeName == "int64")
                propertyNode.Text += " = " + mem.ReadULong(actor.ActorAddress + (ulong)p.Offset);
            else if (p.TypeName == "float")
                propertyNode.Text += " = " + mem.ReadFloat(actor.ActorAddress + (ulong)p.Offset);
            else if (p.TypeName == "Name")
                propertyNode.Text += " = " + mem.ReadFName(actor.ActorAddress + (ulong)p.Offset);
            else if (p.TypeName == "Text")
                propertyNode.Text += " = " + mem.ReadFText(actor.ActorAddress + (ulong)p.Offset);
            else if (p.TypeName == "String")
                propertyNode.Text += " = " + mem.ReadFString(actor.ActorAddress + (ulong)p.Offset);
            else if (p.TypeName.ToLower() == "bool" || p.IsBitSize)
            {
                if (p.IsBitSize)
                {
                    propertyNode.Text += " = " + mem.ReadBool(actor.ActorAddress + (ulong)p.Offset, p.BitNumber);
                }
                else
                    propertyNode.Text += " = " + mem.ReadBool(actor.ActorAddress + (ulong)p.Offset);
            }
            else if (p.TypeName == "Guid")
                propertyNode.Text += " = " + mem.ReadGuid(actor.ActorAddress + (ulong)p.Offset);
            else if (p.TypeName == "Vector" || p.TypeName == "Rotator")
                propertyNode.Text += " = " + mem.ReadVector3(actor.ActorAddress + (ulong)p.Offset);
            else if (p.TypeName == "char")
                propertyNode.Text += " = " + mem.ReadByte(actor.ActorAddress + (ulong)p.Offset);
            else if (propertyNode.Text.Contains("*"))
            {
                propertyNode.Text += " = " + actor.ActorAddress.ToString("X") + "=>" + mem.ReadULong(actor.ActorAddress).ToString("X");
            }
            else
            {
                propertyNode.Text += " => " + actor.ActorAddress.ToString("X");
            }
        }

        private void ExpandSelectedNode(TreeNode? node = null)
        {
            if (node == null && treeView1.SelectedNode == null)
            {
                MessageBox.Show("Error, no node selected");
                return;
            }
            if (node == null)
                node = treeView1.SelectedNode;

            if (node.Nodes.Count > 2)
                return;

            BasicActor actor;
            object tag = node.Tag;
            if (node.Tag == null && node.Parent != null)
                tag = node.Parent.Tag;
            if (tag == null && node.Tag == null && node.Parent != null && node.Parent.Parent != null)
                tag = node.Parent.Parent.Tag;

            if (tag == null)
            {
                MessageBox.Show("Error, no actoraddress found");
                return;
            }
            actor = (BasicActor)tag;

            Models.SDKHelper.SDK_Class classObject = new Models.SDKHelper.SDK_Class();
            var objectName = node.Name;

            if (node.Text.StartsWith("Script"))
            {
                if (SDKService.fullSDKDetailed.ContainsKey(objectName))
                {
                    var classNames = SDKService.fullSDKDetailed[objectName].Select(c => c.Value.Name).ToList();
                    classNames.Sort();
                    foreach (var c in classNames)
                    {
                        var newNode = node.Nodes.Add("Class " + c);
                        newNode.Name = c;
                    }
                    return;
                }
                else
                {
                    MessageBox.Show("Error, script not found");
                    return;
                }
            }
            else if(node.Text.StartsWith("ClassParent"))
            {
                classObject = SDKService.GetClassFromName(objectName);
            }
            else
            {
                ulong address = actor.ActorAddress;
                if (node.Text.Contains("*"))
                {
                    address = mem.ReadULong(actor.ActorAddress);
                    if (address == 0)
                    {
                        node.Text = node.Text.Split("=").First() + "= 0";
                        if (node.Nodes.Count > 0)
                            node.Nodes.Clear();
                        return;
                    }
                    node.Text = node.Text.Split("=").First() + "= "+ actor.ActorAddress.ToString("X")+"=>"+ address.ToString("X");
                }
                var classObjectOrig = SDKService.GetClassFromName(objectName);
                var rawname = mem.ReadRawname(address);
                if (!string.IsNullOrWhiteSpace(rawname))
                {
                    var classname = rawname.ToLower();
                    var classes = SDKClassComboBox.Items.Cast<SDK_Class>().ToList();
                    if (classes.Any(classes => classes.Name.ToLower() == classname))
                    {
                        classObject = classes.First(classes => classes.Name.ToLower() == classname);
                    }
                    else
                    {
                        classname = classname.Replace("_c", "");
                        if (classes.Any(classes => classes.Name.ToLower() == classname))
                        {
                            classObject = classes.First(classes => classes.Name.ToLower() == classname);
                        }
                        else
                        {
                            classname = classname.Replace("bp_", "");
                            if (classes.Any(classes => classes.Name.ToLower() == classname))
                            {
                                classObject = classes.First(classes => classes.Name.ToLower() == classname);
                            }
                            else
                                classObject = SDKService.GetClassFromName(objectName);
                        }
                    }
                    if (classObject != null && classObjectOrig != null)
                        if(classObject.Size < classObjectOrig.Size || classObject.Properties.Count < classObjectOrig.Properties.Count)
                            classObject = classObjectOrig;
                }
                else
                    classObject = SDKService.GetClassFromName(objectName);
            }
            //else if (node.Text.StartsWith("Class"))
            //{
            //    classObject = SDKService.GetClassFromName(objectName);
            //}
            //else
            //{
            //    if (objectName.Contains("."))
            //    {
            //        var split = objectName.Split(".");
            //        classObject = SDKService.GetPropertyClass(split[0], split[1]);
            //    }
            //    else
            //    {
            //        classObject = SDKService.GetPropertyClass(node.Parent.Text, objectName);
            //    }
            //}

            if (classObject == null || string.IsNullOrWhiteSpace(classObject.Name))
            {
                MessageBox.Show("Error, class not found");
                return;
            }
            classObject.Update();

            if (!string.IsNullOrWhiteSpace(classObject.ParentClassName))
            {
                var parent = SDKService.GetClassFromName(classObject.ParentClassName);

                if (parent != null)
                {
                    classObject.Parent = parent;
                    var parentNodeClass = node.Nodes.Add("ClassParent " + parent.Name);
                    parentNodeClass.Name = parent.Name;
                    parentNodeClass.Tag = actor;
                    //ExpandSelectedNode(parentNodeClass);
                }
            }

            if (node.Text.Contains("*"))
                actor = new BasicActor() { ActorAddress = mem.ReadULong(actor.ActorAddress), RawName = mem.ReadRawname(mem.ReadULong(actor.ActorAddress)) };

            if (classObject.Properties.Any())
            {
                ReadProperties(actor, classObject, node);
            }
            if (classObject.Functions.Any())
            {
                ReadProperties(actor, classObject, node);
            }
        }

        private void AttachedTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            selectedNode = e.Node;
        }

        private void AttachedTreeView_DoubleClick(object sender, EventArgs e)
        {
            var node = AttachedTreeView.SelectedNode;
            selectedNode = node;
            if (node != null)
            {
                ExpandSelectedNode(node);
            }
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            var searchstring = SearchText.Text;

            var actors = SoT_Tool.GetAllActors();
            actors.Add(new BasicActor() { ActorAddress = SoT_Tool.world_address, RawName = "World" });

            if(actors.Count() < 10)
            {
                actors.AddRange(SoT_DataManager.Actors.Values.Where(a => !actors.Any(a2 => a2.ActorAddress == a.ActorAddress)));
            }

            var newNodes = actors.Where(a => a.RawName.ToLower().Contains(searchstring.ToLower())).ToList();
            nodeList.Clear();
            treeView1.Nodes.Clear();
            foreach (var actor in actors.Where(a => a.RawName.ToLower().Contains(searchstring.ToLower())).ToList())
            {
                var node = new TreeNode(actor.ActorAddress.ToString("X") + " : " + actor.RawName);
                node.Tag = actor;
                treeView1.Nodes.Add(node);
                node.Name = actor.ActorAddress.ToString();
                nodeList.Add(node);
            }

            //var newNodes2 = nodeList.Where(n => ((BasicActor)n.Tag).RawName.ToLower().Contains(searchstring.ToLower())).ToList();
            //foreach (var n in newNodes)
            //{
            //    treeView1.Nodes.Add(n);
            //}
        }

        private void RangeFilterButton_Click(object sender, EventArgs e)
        {
            var searchstring = SearchText.Text;
            var newNodes = nodeList.Where(n => ((BasicActor)n.Tag).RawName.Contains(searchstring)).ToList();

            newNodes = newNodes.Where(n => SoT_Tool.GetDistanceFromActor(((BasicActor)n.Tag).ActorAddress) < 5).ToList();

            treeView1.Nodes.Clear();
            foreach (var n in newNodes)
            {
                treeView1.Nodes.Add(n);
            }
        }

        private void DetectTypeButton_Click(object sender, EventArgs e)
        {
            if(SDKClassComboBox.Items.Count < 2)
            {
                var list = SDKService.fullSDKDetailed.SelectMany(c => c.Value.Values.ToArray()).ToList();
                list.Sort((a, b) => a.Name.CompareTo(b.Name));
                foreach (var sdk in list)
                {
                    //SDKClassComboBox.SelectedIndex = 0;
                    SDKClassComboBox.Items.Add(sdk);
                }
                SDKClassComboBox.DisplayMember = "Name";
            }

            var node = treeView1.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("Error, no node selected");
                return;
            }
            var classes = SDKClassComboBox.Items.Cast<SDK_Class>().ToList();
            var classname = node.Text.Split(": ")[1].ToLower();
            if (classes.Any(classes => classes.Name.ToLower() == classname))
            {
                var index = SDKClassComboBox.Items.IndexOf(classes.First(classes => classes.Name.ToLower() == classname));
                SDKClassComboBox.SelectedIndex = index;
                return;
            }
            classname = classname.Replace("_c", "");
            if (classes.Any(classes => classes.Name.ToLower() == classname))
            {
                var index = SDKClassComboBox.Items.IndexOf(classes.First(classes => classes.Name.ToLower() == classname));
                SDKClassComboBox.SelectedIndex = index;
                return;
            }
            classname = classname.Replace("bp_", "");
            if (classes.Any(classes => classes.Name.ToLower() == classname))
            {
                var index = SDKClassComboBox.Items.IndexOf(classes.First(classes => classes.Name.ToLower() == classname));
                SDKClassComboBox.SelectedIndex = index;
                return;
            }
            //(List<SDK_Class>)
        }

        List<TreeNode> TrackedNodes = new List<TreeNode>();

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (TrackedNodes != null)
            {
                List<TreeNode> toRemove = new List<TreeNode>();
                foreach (var node in TrackedNodes)
                {
                    object tag = node.Tag;
                    if (tag == null)
                    {
                        continue;
                    }
                    BasicActor actor = (BasicActor)node.Tag;
                    try
                    {
                        ReadPropertyValue(actor, node);

                    }
                    catch (Exception ex)
                    {
                        toRemove.Add(node);
                        node.ForeColor = Color.Red;
                        MessageBox.Show("Exception caught. Stopping tracking " + actor.RawName);
                        //return;
                    }
                }
                foreach (var node in toRemove)
                {
                    TrackedNodes.Remove(node);
                }
            }
        }

        private void TrackButton_Click(object sender, EventArgs e)
        {
            var node = selectedNode;

            object tag = node.Tag;
            if (tag == null)
            {
                MessageBox.Show("Error, no actoraddress found");
                return;
            }
            BasicActor actor = (BasicActor)node.Tag;

            if (actor.RawName == "Vector")
            {
                var pos = mem.ReadVector3(actor.ActorAddress);
                if (pos == null)
                {
                    MessageBox.Show("Error, no position found");
                    return;
                }
                if (Math.Abs(pos.X) > 15000 || Math.Abs(pos.Y) > 15000 || Math.Abs(pos.Z) > 15000)
                {
                    pos = pos / 100;
                }
                var track = new Marker(mem, "Inspector Marker", actor.ActorAddress);
                SoT_DataManager.DisplayObjects.Add(track);
            }

            try
            {
                ReadPropertyValue(actor, selectedNode);
            }
            catch
            {
                if (TrackedNodes.Contains(selectedNode))
                    TrackedNodes.Remove(selectedNode);
                selectedNode.ForeColor = Color.Blue;
                return;
            }
            if (!TrackedNodes.Contains(selectedNode))
            {
                TrackedNodes.Add(selectedNode);
                selectedNode.ForeColor = Color.LightCoral;
            }
            else
            {
                TrackedNodes.Remove(selectedNode);
                selectedNode.ForeColor = Color.Blue;
            }
        }

        private void AttachPropButton_Click(object sender, EventArgs e)
        {
            var node = selectedNode;
            var actor = (BasicActor)node.Tag;
            var address = mem.ReadULong(actor.ActorAddress);
            if(address > 0)
            {
                actor = new BasicActor() { RawName = mem.ReadRawname(address), ActorAddress = address };
            }
            else
            {
                address = actor.ActorAddress;
            }

            var name = mem.ReadRawname(address);
            var classTest = SDKService.GetClassFromName(name);
            var classObject = (SDK_Class)SDKClassComboBox.SelectedItem;

            if (classTest != null)
            {
                classObject = classTest;
            }

            if (classObject == null || string.IsNullOrWhiteSpace(classObject.Name))
            {
                MessageBox.Show("Error, class not found");
                return;
            }
            classObject.Update();

            var newNode = AttachedTreeView.Nodes.Add(node.Text);
            newNode.Tag = actor;
            newNode.Name = node.Name;

            if (!string.IsNullOrWhiteSpace(classObject.ParentClassName))
            {
                var parent = SDKService.GetClassFromName(classObject.ParentClassName);

                if (parent != null)
                {
                    classObject.Parent = parent;
                    //var parentNode = node.Nodes.Add("Parent");
                    //parentNode.Name = "Parent";
                    var parentNodeClass = newNode.Nodes.Add("ClassParent " + parent.Name);
                    parentNodeClass.Name = parent.Name;
                    parentNodeClass.Tag = new BasicActor() { RawName = parent.Name, ActorAddress = actor.ActorAddress };
                    //ExpandSelectedNode(parentNodeClass);
                }
            }

            if (classObject.Properties.Any())
            {
                ReadProperties(actor, classObject, newNode);
            }
        }
    }
}
