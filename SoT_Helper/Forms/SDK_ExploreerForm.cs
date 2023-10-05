using Newtonsoft.Json;
using SoT_Helper.Extensions;
using SoT_Helper.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SoT_Helper
{
    public partial class SDK_ExploreerForm : Form
    {
        public SDK_ExploreerForm()
        {
            InitializeComponent();
            ListOffsetJson();

            SearchComboBox.Items.Add("Search by class name");
            SearchComboBox.Items.Add("Search by property name");
            SearchComboBox.Items.Add("Search by property type");
            SearchComboBox.Items.Add("Search by function name");
            SearchComboBox.Items.Add("Search everything");
            SearchComboBox.SelectedIndex = 0;
        }

        private void ListOffsetJson()
        {
            // Get all distinct class names from offsets.json, sort them and add them to the treeview
            var classNames = SoT_Tool.offsets.Keys.Select(o => o.Split(".")[0]).Distinct().ToList();
            classNames.Sort();
            treeView1.Nodes.Clear();
            foreach (var c in classNames)
            {
                var node = treeView1.Nodes.Add("Class " + c);
                node.Name = c;
            }
        }

        private void ExpandSelectedNode(TreeNode? node = null)
        {
            if (node == null && treeView1.SelectedNode == null && treeViewSearchResults == null && treeViewSearchResults.SelectedNode == null)
            {
                MessageBox.Show("Error, no node selected");
                return;
            }
            if (node == null)
                node = treeView1.SelectedNode;
            if (node == null && treeViewSearchResults != null)
                node = treeViewSearchResults.SelectedNode;

            if (node.Nodes.Count > 0)
            {
                node.ExpandAll();
                return;
            }

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
            else if (node.Text.StartsWith("Class"))
            {
                classObject = SDKService.GetClassFromName(objectName);
            }
            else
            {
                if (objectName.Contains("."))
                {
                    var split = objectName.Split(".");
                    classObject = SDKService.GetPropertyClass(split[0], split[1]);
                }
                else
                {
                    //var test = node.Parent;
                    //var test3 = node.Parent.Name;
                    ////var test4 = node.Parent.Parent;
                    ////var test5 = node.Parent.Parent.Name;
                    //var 
                    //if(node.Parent.Name.Contains("."))

                    classObject = SDKService.GetPropertyClass(node.Parent.Text, objectName);
                    //classObject = SDKService.GetClassFromName(objectName);
                }
            }

            if (classObject == null || string.IsNullOrWhiteSpace(classObject.Name))
            {
                MessageBox.Show("Error, class not found");
                return;
            }
            classObject.Update();

            if (showParentToolStripMenuItem.Checked && !string.IsNullOrWhiteSpace(classObject.ParentClassName))
            {
                var parent = SDKService.GetClassFromName(classObject.ParentClassName);

                if (parent != null)
                {
                    classObject.Parent = parent;
                    //var parentNode = node.Nodes.Add("Parent");
                    //parentNode.Name = "Parent";
                    var parentNodeClass = node.Nodes.Add("ClassParent " + parent.Name);
                    parentNodeClass.Name = parent.Name;
                    ExpandSelectedNode(parentNodeClass);
                }
            }

            if (classObject.Properties.Any() && showPropertiesToolStripMenuItem.Checked)
            {
                foreach (var p in classObject.Properties)
                {
                    var newNode = node.Nodes.Add(p.Value.GetPropertyText());
                    newNode.Name = classObject.Name + "." + p.Value.Name;
                    if (SoT_Tool.offsets.Keys.Contains(newNode.Name))
                    {
                        newNode.ForeColor = Color.Green;
                        newNode.Checked = true;
                    }
                    else
                    {
                        newNode.ForeColor = Color.Blue;
                        newNode.Checked = false;
                    }
                    if (p.Value.SDK_Enum != null)
                    {
                        newNode.Name = classObject.Name + ".Enum." + p.Value.SDK_Enum.Name + "." + p.Value.Name;

                        foreach (var e in p.Value.SDK_Enum.Values)
                        {
                            var enumNode = newNode.Nodes.Add(e.Key + " " + e.Value);
                        }
                    }
                }
            }
            if (classObject.Functions.Any() && showFunctionsToolStripMenuItem.Checked)
            {
                foreach (var f in classObject.Functions)
                {
                    var newNode = node.Nodes.Add("Function " + f.Returntype + " : " + f.Name + "();");
                    newNode.Name = classObject.Name + "." + f.Name;
                }
            }
        }

        private string GetLetters(string text)
        {
            return new string(text.Where(c => Char.IsLetterOrDigit(c)).ToArray());
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeViewSearchResults.SelectedNode = null;
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            ExpandSelectedNode();
        }

        private void ExpandNodeButton_Click(object sender, EventArgs e)
        {
            ExpandSelectedNode();
        }

        private void listOffsetsjsonItemsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListOffsetJson();
        }

        private void listAllSDKScriptFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
            foreach (var f in SDKService.fullSDKDetailed.Keys)
            {
                var node = treeView1.Nodes.Add("Script " + f);
                node.Name = f;
            }
        }

        private void focusNodeButton_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("No node selected");
                return;
            }
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(node);
        }

        private void onlyExpandedButton_Click(object sender, EventArgs e)
        {
            List<TreeNode> nodes = new List<TreeNode>();

            foreach (var node in treeView1.Nodes)
            {
                if (node is TreeNode)
                {
                    var t = node as TreeNode;
                    if (t.Nodes.Count > 0)
                    {
                        nodes.Add(t);
                    }
                }
            }
            treeView1.Nodes.Clear();
            foreach (var n in nodes)
            {
                treeView1.Nodes.Add(n);
            }
        }

        private void SearchComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            treeView1.Nodes.Clear();
        }

        private void AddOffsetButton_Click(object sender, EventArgs e)
        {
            var node = treeView1.SelectedNode;
            if (node == null && treeViewSearchResults != null)
                node = treeViewSearchResults.SelectedNode;
            if (node == null)
            {
                MessageBox.Show("No node selected");
                return;
            }
            if (!node.Text.Contains(";"))
            {
                MessageBox.Show("Error, node is not a property");
                return;
            }
            var offsetKey = node.Name;
            if (SoT_Tool.offsets.Keys.Contains(offsetKey))
            {
                MessageBox.Show("Error, offset already exists");
                return;
            }
            var offset = SDKService.FindSDKOffset(offsetKey);
            var newOffsets = new Dictionary<string, int>();
            foreach (var o in SoT_Tool.offsets)
            {
                newOffsets.Add(o.Key, o.Value);
            }
            newOffsets.Add(offsetKey, offset);
            SoT_Tool.offsets = newOffsets.ToImmutableDictionary<string, int>();
        }

        private void saveOffsetsjsonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string number = "";
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
            // Convert the updated list to a JSON string and write it back to the file
            string updatedJson = JsonConvert.SerializeObject(SoT_Tool.offsets, Formatting.Indented);
            List<string> updatedOffsets = OffsetFinder.OrderJsonLines(updatedJson);

            File.WriteAllLines("offsets.json", updatedOffsets);
            //SoT_Tool.offsets = SoT_Tool.offsets.ToImmutableDictionary<string, int>();
            SoT_DataManager.InfoLog += "\nOffsets.json updated.";
        }

        private void SearchSDKButton_Click(object sender, EventArgs e)
        {
            var search = SearchTextBox.Text;
            var searchType = SearchComboBox.SelectedIndex;
            if (string.IsNullOrWhiteSpace(search))
            {
                MessageBox.Show("Error, search is empty");
                return;
            }
            treeViewSearchResults.Nodes.Clear();
            /*
             SearchComboBox.Items.Add("Search by class name");
            SearchComboBox.Items.Add("Search by property name");
            SearchComboBox.Items.Add("Search by property type");
            SearchComboBox.Items.Add("Search by function name");
            SearchComboBox.Items.Add("Search everything");
             */
            if (search.Contains("\""))
            {
                search = search.Replace("\"", "");
            }
            else if (search.Contains("*"))
            {
                search = search.Replace("*", "%");
            }
            else if (!search.Contains("%"))
            {
                search = "%" + search + "%";
            }
            if (searchType == 2) //property type
            {
                //SDKService.fullSDKDetailed.Values.SelectMany(c => c).ToList().ForEach(x => x.Update());
                SDKService.fullSDKDetailed.Values.SelectMany(c => c.Values).Where(c => c.Properties.Values
                    .Any(
                    //p => search.Contains("\"") ? p.TypeName.ToLower() == search.ToLower() : p.TypeName.ToLower().Contains(search.ToLower()))
                    p => p.TypeName.ToLower().IsMatch(search.ToLower()))
                    ).ToList()
                    .ForEach(c =>
                {
                    var node = treeViewSearchResults.Nodes.Add("Class " + c.Name);
                    node.Name = c.Name;
                });
            }
            else if (searchType == 3)//(SearchComboBox.SelectedText.Contains("function name"))
            {
                //SDKService.fullSDKDetailed.Values.SelectMany(c => c).ToList().ForEach(x => x.Update());
                SDKService.fullSDKDetailed.Values.SelectMany(c => c.Values).Where(c => c.Functions.Any(p => p.Name.ToLower().IsMatch(search.ToLower()))).ToList().ForEach(c =>
                {
                    var node = treeViewSearchResults.Nodes.Add("Class " + c.Name);
                    node.Name = c.Name;
                });
            }
            else if (searchType == 4)//(SearchComboBox.SelectedText.Contains("everything"))
            {
                SDKService.fullSDKDetailed.Values.SelectMany(c => c).Where(c => c.Key.ToLower().IsMatch(search.ToLower())).ToList().ForEach(c =>
                {
                    var node = treeViewSearchResults.Nodes.Add("Class " + c.Value.Name);
                    node.Name = c.Value.Name;
                });
                SDKService.fullSDKDetailed.Values.SelectMany(c => c.Values).Where(c => c.Properties.Values.Any(p => p.TypeName.ToLower().IsMatch(search.ToLower()))).ToList().ForEach(c =>
                {
                    if (!treeViewSearchResults.Nodes.ContainsKey(c.Name))
                    {
                        var node = treeViewSearchResults.Nodes.Add("Class " + c.Name);
                        node.Name = c.Name;
                    }
                });
                SDKService.fullSDKDetailed.Values.SelectMany(c => c.Values).Where(c => c.Properties.Values.Any(p => p.Name.ToLower().IsMatch(search.ToLower()))).ToList().ForEach(c =>
                {
                    if (!treeViewSearchResults.Nodes.ContainsKey(c.Name))
                    {
                        var node = treeViewSearchResults.Nodes.Add("Class " + c.Name);
                        node.Name = c.Name;
                    }
                });
            }
            else if (searchType == 0)//(SearchComboBox.SelectedText.Contains("class name"))
            {
                SDKService.fullSDKDetailed.Values.SelectMany(c => c.Values).Where(c => c.Name.ToLower().IsMatch(search.ToLower())).ToList().ForEach(c =>
                {
                    var node = treeViewSearchResults.Nodes.Add("Class " + c.Name);
                    node.Name = c.Name;
                });
            }
            else if (searchType == 1)//(SearchComboBox.SelectedText.Contains("property name"))
            {
                SDKService.fullSDKDetailed.Values.SelectMany(c => c.Values).Where(c => c.Properties.Values.Any(p => p.Name.ToLower().IsMatch(search.ToLower()))).ToList().ForEach(c =>
                {
                    var node = treeViewSearchResults.Nodes.Add("Class " + c.Name);
                    node.Name = c.Name;
                });
            }
            else
            {
                MessageBox.Show("Error, search type not selected");
                return;
            }

        }



        private void treeViewSearchResults_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView1.SelectedNode = null;
        }

        private void attachClassToMemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var myForm = new InspectorForm();
            myForm.Show();
        }

        private void showFunctionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //showFunctionsToolStripMenuItem.Checked = !showFunctionsToolStripMenuItem.Checked;
        }

        private void showParentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //showParentToolStripMenuItem.Checked = !showParentToolStripMenuItem.Checked;
        }

        private void showPropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //showPropertiesToolStripMenuItem.Checked = !showPropertiesToolStripMenuItem.Checked;
        }
    }
}
