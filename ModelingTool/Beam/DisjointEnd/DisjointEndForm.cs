using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelingTool.Beam.DisjointEnd
{
    public partial class DisjointEndForm : Form
    {
        public DisjointEndForm()
        {
            InitializeComponent();
        }

        public bool MultiBeamDisjoint { get; private set; }
        public bool OneBeamDisjoint { get; private set; }
        public bool OnePointDisjoint { get; private set; }

        private void button1_Click(object sender, EventArgs e)
        {
            OnePointDisjoint = true;
            OneBeamDisjoint = false;
            MultiBeamDisjoint = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OnePointDisjoint = false;
            OneBeamDisjoint = true;
            MultiBeamDisjoint = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OnePointDisjoint = false;
            OneBeamDisjoint = false;
            MultiBeamDisjoint = true;
        }
    }
}
