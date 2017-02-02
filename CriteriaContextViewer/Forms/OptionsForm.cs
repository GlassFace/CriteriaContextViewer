using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CriteriaContextViewer.Forms
{
    public partial class OptionsForm : Form
    {
        public OptionsModel Options { get; set; }
        
        public OptionsForm(ref OptionsModel options)
        {
            Options = options;
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Options.UseDungeonEncounter = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Options.UseItems = checkBox2.Checked;
        }

        private void OptionsForm_Load(object sender, EventArgs e)
        {
            checkBox1.Checked = Options.UseDungeonEncounter;
            checkBox2.Checked = Options.UseItems;
        }
    }
}
