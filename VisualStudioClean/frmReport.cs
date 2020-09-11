using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Threading;
using System.IO;

namespace VisualStudioClean
{
    public partial class frmReport : Form
    {
        List<string> _FailList;
        List<string> _SuccessList;
        public List<string> FailList
        {
            set
            {
                listView1.Items.Clear();
                if (value == null || value.Count == 0) return;
                _FailList = value;
                LoadSetting();
            }
        }

        public List<string> SuccessList
        {
            set
            {
                if (value == null || value.Count == 0) return;
                _SuccessList = value;
                LoadSetting();
            }
        }

        public frmReport()
        {
            InitializeComponent();
        }

        private void LoadSetting()
        {
            listView1.Items.Clear();
            if (_FailList == null || _FailList.Count == 0) return;
            foreach(string str in _FailList)
            {
                listView1.Items.Add(str);
            }

            if (_SuccessList != null)
            {
                lblSuccess.Text = _SuccessList.Count.ToString();
            }
            lblFail.Text = _FailList.Count.ToString();
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
