using Autodesk.Revit.DB;
using ModelingTool.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelingTool.Beam.QuickCreation
{
    public partial class QuickCreationForm : System.Windows.Forms.Form
    {
        private Document doc;
        private FamilySymbol familySymbolResult;
        private Level levelResult;
        private IList<Element> levelList;
        private DuplicateForm duplicateForm;

        public FamilySymbol SelectedFamilySymbol { get { return familySymbolResult; } }
        public Level SelectedLevel { get { return levelResult; } }
        public bool ByGrid { get; private set; }
        public bool ByPoint { get; private set; }

        public QuickCreationForm(Document doc)
        {
            InitializeComponent();
            this.doc = doc;
        }

        public void InitFamilySymbolMap(Dictionary<string, FamilySymbol[]> map)
        {
            foreach (var pair in map)
            {
                AddFamilySymbolMap(pair);
            }
        }

        public void AddFamilySymbolMap(KeyValuePair<string, FamilySymbol[]> pair)
        {
            TreeNode[] familySymbolNodeSet = pair.Value.Select(symbol =>
            {
                TreeNode node = new TreeNode(symbol.Name);
                node.Tag = symbol;
                return node;
            }).ToArray();
            treeView1.Nodes.Add(new TreeNode(pair.Key, familySymbolNodeSet));
        }

        public void InitLevelList(IList<Element> levelList)
        {
            this.levelList = levelList;
            comboBox1.Items.AddRange(levelList.Select(lv => lv.Name).ToArray());
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes == null || e.Node.Nodes.Count == 0)
            {
                familySymbolResult = (FamilySymbol)e.Node.Tag;
            }
            else
            {
                familySymbolResult = null;
            }
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex >= 0)
            {
                levelResult = (Level)levelList[comboBox1.SelectedIndex];
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (IsReadyToClose())
            {
                ByPoint = true;
                ByGrid = false;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (IsReadyToClose())
            {
                ByPoint = false;
                ByGrid = true;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool IsReadyToClose()
        {
            bool readyToClose = true;
            errorProvider1.Clear();
            if (familySymbolResult == null)
            {
                errorProvider1.SetError(treeView1, "未选择梁类型！");
                errorProvider1.SetIconAlignment(treeView1, ErrorIconAlignment.MiddleRight);
                readyToClose = false;
            }
            if (levelResult == null)
            {
                errorProvider1.SetError(comboBox1, "未选择布置标高！");
                errorProvider1.SetIconAlignment(comboBox1, ErrorIconAlignment.MiddleRight);
                readyToClose = false;
            }
            return readyToClose;
        }

        private void treeView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                System.Drawing.Point click = new System.Drawing.Point(e.X, e.Y);
                TreeNode node = treeView1.GetNodeAt(click);
                if (node != null)
                {
                    treeView1.SelectedNode = node;
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (familySymbolResult != null)
            {
                ToolStripMenuItem2.Enabled = true;
                ToolStripMenuItem3.Enabled = true;
            }
            else
            {
                ToolStripMenuItem2.Enabled = false;
                ToolStripMenuItem3.Enabled = false;
            }
        }

        private void ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog() { Multiselect = false, CheckFileExists = true, CheckPathExists = true })
            {
                dialog.InitialDirectory = @"C:\ProgramData\Autodesk\RVT 2016\Libraries\China\结构\框架";
                dialog.Title = "载入本地族……";
                dialog.Filter = @"rfa文件 (*.rfa)|*.rfa";
                switch (dialog.ShowDialog(this))
                {
                    case DialogResult.OK:
                        using (Transaction trans = new Transaction(doc, "载入本地族"))
                        {
                            trans.Start();
                            Family family;
                            if (doc.LoadFamily(dialog.FileName, out family))
                            {
                                AddFamilySymbolMap(family.GetFamilySymbolPair(doc));
                            }
                            trans.Commit();
                        }
                        break;
                }
            }
        }

        private void ToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if (duplicateForm == null)
            {
                duplicateForm = new DuplicateForm();
            }
            duplicateForm.SymbolName = familySymbolResult.Name;
            duplicateForm.SetSymbolGeometryMap(FamilyUtil.GetSymbolGeometry(familySymbolResult));
            switch (duplicateForm.ShowDialog())
            {
                case DialogResult.OK:
                    using (Transaction trans = new Transaction(doc, "添加族类型"))
                    {
                        trans.Start();
                        FamilySymbol newSymbol = familySymbolResult.Duplicate(duplicateForm.SymbolName, duplicateForm.GeometryMap);
                        TreeNode newNode = new TreeNode(newSymbol.Name);
                        newNode.Tag = newSymbol;
                        TreeNode current = treeView1.SelectedNode;
                        current.Parent.Nodes.Insert(current.Index + 1, newNode);
                        treeView1.SelectedNode = newNode;
                        trans.Commit();
                    }
                    break;
            }
        }

        private void ToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            using (Transaction trans = new Transaction(doc, "删除族类型"))
            {
                trans.Start();
                if (familySymbolResult.Delete(doc, this))
                {
                    TreeNode current = treeView1.SelectedNode;
                    TreeNode parent = current.Parent;
                    current.Remove();
                    if (parent.Nodes.Count == 0)
                        parent.Remove();
                }
                trans.Commit();
            }
        }
    }
}
