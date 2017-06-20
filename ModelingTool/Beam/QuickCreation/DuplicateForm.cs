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
    public partial class DuplicateForm : Form
    {
        public string SymbolName
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public Dictionary<string, double> GeometryMap
        {
            get
            {
                Dictionary<string, double> result = new Dictionary<string, double>();
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    result.Add(row.Cells["pName"].Value.ToString(), double.Parse(row.Cells["pValue"].Value.ToString()));
                }
                return result;
            }
        }

        public DuplicateForm()
        {
            InitializeComponent();
        }

        public void SetSymbolGeometryMap(Dictionary<string, double> map)
        {
            dataGridView1.Rows.Clear();
            foreach (var pair in map)
            {
                dataGridView1.Rows.Add(pair.Key, pair.Value);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                dataGridView1.BeginEdit(true);
            }
        }
    }
}
