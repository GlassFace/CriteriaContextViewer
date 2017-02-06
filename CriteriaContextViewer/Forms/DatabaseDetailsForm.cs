using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CriteriaContextViewer.Forms
{
    public partial class DatabaseDetailsForm : Form
    {
        public DBSettingsModel DBSettingsModel { get; set; }

        public DatabaseDetailsForm(ref DBSettingsModel dbSettingsModel)
        {
            InitializeComponent();
            DBSettingsModel = dbSettingsModel;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DBSettingsModel.Username = textBox1.Text;
            DBSettingsModel.Password = textBox2.Text;
            DBSettingsModel.Database = textBox3.Text;
            DBSettingsModel.Server = textBox4.Text;
            DBSettingsModel.Port = textBox5.Text;
        }

        private void DatabaseDetailsForm_Load(object sender, EventArgs e)
        {
            textBox1.Text = DBSettingsModel.Username;
            textBox2.Text = DBSettingsModel.Password;
            textBox3.Text = DBSettingsModel.Database;
            textBox4.Text = DBSettingsModel.Server;
            textBox5.Text = DBSettingsModel.Port;
        }
    }
}
