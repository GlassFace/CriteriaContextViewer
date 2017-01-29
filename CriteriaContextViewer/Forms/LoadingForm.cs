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
    public partial class LoadingForm : Form
    {
        public LoadingForm(string context, BackgroundWorker worker)
        {
            InitializeComponent();

            label1.Text = context;
            worker.ProgressChanged += (sender, args) => progressBar1.Value = args.ProgressPercentage;
            worker.RunWorkerCompleted += (s, e) => Close();
        }
        
        public void OnFormClosed(object sender, FormClosedEventArgs formClosedEventArgs)
        {

        }
    }
}
