using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelingTool.Beam.JoinEnd
{
    public partial class JoinEndForm : Form
    {
        public JoinEndForm()
        {
            InitializeComponent();
        }

        public bool MultiBeamJoin { get; private set; }
        public bool OneBeamJoin { get; private set; }
        public bool OnePointJoin { get; private set; }

        private void button1_Click(object sender, EventArgs e)
        {
            OnePointJoin = true;
            OneBeamJoin = false;
            MultiBeamJoin = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OnePointJoin = false;
            OneBeamJoin = true;
            MultiBeamJoin = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OnePointJoin = false;
            OneBeamJoin = false;
            MultiBeamJoin = true;
        }
    }
}
