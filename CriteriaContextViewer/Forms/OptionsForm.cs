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
            checkBoxVerboseCriteriaTree.Checked = Options.VerboseCriteriaTree;
            checkBoxUseCreatureNames.Checked = Options.UseCreatureNames;
            checkBoxUseGameobjectNames.Checked = Options.UseGameobjectNames;
            checkBoxUseSpells.Checked = Options.UseSpells;
        }

        private void checkBoxVerboseCriteriaTree_CheckedChanged(object sender, EventArgs e)
        {
            Options.VerboseCriteriaTree = checkBoxVerboseCriteriaTree.Checked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DBSettingsModel dbSettingsModel = new DBSettingsModel();
            DatabaseDetailsForm databaseDetailsForm = new DatabaseDetailsForm(ref dbSettingsModel);
            databaseDetailsForm.ShowDialog();

            Options.DBSettingsModel = dbSettingsModel;
        }

        private void checkBoxUseCreatureNames_CheckedChanged(object sender, EventArgs e)
        {
            Options.UseCreatureNames = checkBoxUseCreatureNames.Checked;
        }

        private void checkBoxGameobjectNames_CheckedChanged(object sender, EventArgs e)
        {
            Options.UseGameobjectNames = checkBoxUseGameobjectNames.Checked;
        }

        private void checkBoxUseSpells_CheckedChanged(object sender, EventArgs e)
        {
            Options.UseSpells = checkBoxUseSpells.Checked;
        }
    }
}
