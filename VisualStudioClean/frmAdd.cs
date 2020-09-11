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
using System.IO;

namespace VisualStudioClean
{
    public partial class frmAdd : Form
    {
        public bool Exclude = false;
        public frmAdd()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            VSCleanSetting obj = VSCleanLib.GetCurrentSetting();
            if (string.IsNullOrEmpty(txtSubPath.Text)) return;
            if (obj == null)
            {
                MessageBox.Show("Error reading " + VSCleanLib.SettingFilename);
                return;
            }
            string path = txtSubPath.Text;
            string checkPath = path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (checkPath.StartsWith(VSCleanLib.GetWorkingDirectory(), StringComparison.CurrentCultureIgnoreCase) == false)
            {
                MessageBox.Show("Path must be Sub Path from VSClean.exe");
                return;
            }
            if (File.Exists(VSCleanLib.GetFilenameAbsolute(path)) == false && Directory.Exists(VSCleanLib.GetFilenameAbsolute(path))) path = path.TrimEnd(new char[] { '\\' }) + "\\";
            if (Exclude == false && Directory.Exists(VSCleanLib.GetFilenameAbsolute(path)) == false)
            {
                MessageBox.Show("Path must be Directory");
                return;
            }

            bool contains = false;
            if (Exclude) contains = VSCleanLib.ContainsExclude(obj, path);
            else contains = VSCleanLib.ContainsScan(obj, path);
            if (contains)
            {
                MessageBox.Show("Path/File already exist in list");
                return;
            }
            if (Exclude)
                obj.ExcludePaths.Add(VSCleanLib.GetFilenameRelative(path));
            else
                obj.ScanPaths.Add(VSCleanLib.GetFilenameRelative(path));

            if (VSCleanLib.SaveCurrentSetting(obj))
            {
                Close();
                return;
            }
            MessageBox.Show("Fail adding Path to " + VSCleanLib.SettingFilename);
            return;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            CommonDialog dlgx = null;
            try
            {
                dlgx = new FolderBrowserDialogEx();
                var dlg2 = (FolderBrowserDialogEx)dlgx;
            }
            catch
            {
                dlgx = new FolderBrowserDialog();
            }

            var dlg1 = (FolderBrowserDialogEx)dlgx;
            dlg1.Description = "Select a folder or file";
            dlg1.ShowNewFolderButton = true;
            dlg1.ShowEditBox = true;
            dlg1.NewStyle = true;
            dlg1.ShowFullPathInEditBox = false;
            dlg1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            dlg1.ShowBothFilesAndFolders = true;

            // Show the FolderBrowserDialog.
            DialogResult result = dlg1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtSubPath.Text = dlg1.SelectedPath;
            }
        }
    }
}
