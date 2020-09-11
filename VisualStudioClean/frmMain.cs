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
    public partial class frmMain : Form
    {
        public VSCleanLib Lib = null;
        string CurrentDirectory = "<current directory>";
        Thread MainThread = null;
        Thread MainThread2 = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void LoadSetting()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(LoadSetting), null);
                return;
            }
            listView1.Items.Clear();
            VSCleanSetting obj = VSCleanLib.GetCurrentSetting();
            if (obj == null || obj.ExcludePaths == null) return;
            foreach(string str in obj.ExcludePaths)
            {
                string f = str;
                if (string.IsNullOrEmpty(f)) f = CurrentDirectory;
                listView1.Items.Add(f);
            }

            listView2.Items.Clear();
            if (obj == null || obj.ScanPaths == null) return;
            foreach (string str in obj.ScanPaths)
            {
                string f = str;
                if (string.IsNullOrEmpty(f)) f = CurrentDirectory;
                listView2.Items.Add(f);
            }
        }

        private void btnClean_Click(object sender, EventArgs e)
        {
            if (GetText(btnClean) == "Start")
            {
                VSCleanSetting obj = VSCleanLib.GetCurrentSetting();
                if (obj == null || obj.ExcludePaths == null) return;

                if (obj.ScanPaths == null || obj.ScanPaths.Count == 0)
                {
                    MessageBoxShow("No path to start, add <project>\\bin or <project>\\obj to start");
                    return;
                }
                Stop();
                SetText(btnClean, "Stop");
                Lib = new VSCleanLib();
                Lib.Report = new Action<List<string>, List<string>>(Report);
                ParameterizedThreadStart ts = new ParameterizedThreadStart(Lib.Clean);
                MainThread = new Thread(ts);
                MainThread.Start(obj);
                Lib.IsRunning = true;
            }
            else if (GetText(btnClean) == "Stop")
            {
                SetText(btnClean, "Start");
                Stop();
            }
        }

        private void Stop()
        {
            if (Lib == null) return;
            Lib.IsRunning = false;
            if (MainThread != null)
            {
                try
                {
                    MainThread.Abort();
                }
                catch { };
                MainThread = null;
            }
            SetText(btnClean, "Start");
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (Lib != null && Lib.IsRunningScan)
            {
                MessageBoxShow("Scanning is in progress");
                return;
            }
            if (Lib == null) Lib = new VSCleanLib();

            Lib.IsRunningScan = true;
            Lib.ScanCompleted = delegate
            {
                MainThread2 = null;
                LoadSetting();
                MessageBoxShow("Scan VS successfully");
            };

            ThreadStart ts = new ThreadStart(Lib.ScanVS);
            MainThread2 = new Thread(ts);
            MainThread2.Start();
        }

        private void Stop2()
        {
            if (Lib == null) return;
            Lib.IsRunningScan = false;
            if (MainThread != null)
            {
                try
                {
                    MainThread.Abort();
                }
                catch { };
                MainThread = null;
            }
            SetText(btnClean, "Start");
        }

        public void Report(List<string> successList, List<string> failList)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<List<string>, List<string>>(Report), new object[] { successList, failList });
                return;
            }

            SetText(btnClean, "Start");
            if (failList == null || failList.Count == 0)
            {
                int success = 0;
                if (successList != null && successList.Count > 0) success = successList.Count;
                MessageBoxShow("All " + success.ToString() + " File(s)/Path(s) deleted successfully");
            }
            else
            {
                frmReport frm = new frmReport();
                frm.FailList = failList;
                frm.SuccessList = successList;
                frm.ShowDialog();
            }
        }

        #region Button Events
        private void btnAdd_Click(object sender, EventArgs e)
        {
            frmAdd frm = new frmAdd();
            frm.Exclude = true;
            frm.ShowDialog();
            LoadSetting();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            ListViewDelete(listView1, true);
        }

        private void btnAddScan_Click(object sender, EventArgs e)
        {
            frmAdd frm = new frmAdd();
            frm.Exclude = false;
            frm.ShowDialog();
            LoadSetting();
        }

        private void btnAddCurrentScan_Click(object sender, EventArgs e)
        {
            if (VSCleanLib.AddCurrentScan()) LoadSetting();
        }

        private void btnDeleteScan_Click(object sender, EventArgs e)
        {
            ListViewDelete(listView2, false);
        }
        #endregion

        #region Form/ListView Event
        private void frmMain_Load(object sender, EventArgs e)
        {
            LoadSetting();
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            listView1.Columns[0].Width = listView1.Width - 49;
            listView2.Columns[0].Width = listView2.Width - 49;
            if (panel1.Height <= 70 || panel2.Height <= 25)
            {
                panel2.Height = (int)(0.46f * (float)Height);
            }
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Stop();
            Stop2();
        }

        private void listView2_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewMouseDown(listView2, e);
        }

        private void listView1_MouseDown(object sender, MouseEventArgs e)
        {
            ListViewMouseDown(listView1, e);
        }

        private void listView2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                ListViewDelete(listView2, false);
            }
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                ListViewDelete(listView1, true);
            }
        }

        public void ListViewDelete(ListView listView, bool isExclude)
        {
            if (listView.SelectedItems == null || listView.SelectedItems.Count == 0) return;
            VSCleanSetting obj = VSCleanLib.GetCurrentSetting();
            for (int i = listView.SelectedItems.Count - 1; i >= 0; i--)
            {
                string strx = listView.SelectedItems[i].Text;
                if (strx == CurrentDirectory) strx = "";

                string str = VSCleanLib.GetFilenameAbsolute(strx);
                if (isExclude)
                {
                    if (VSCleanLib.ContainsExclude(obj, str))
                        VSCleanLib.RemoveExclude(obj, str);
                }
                else
                {
                    if (VSCleanLib.ContainsScan(obj, str))
                        VSCleanLib.RemoveScan(obj, str);
                }
            }
            if (VSCleanLib.SaveCurrentSetting(obj))
            {
                LoadSetting();
                return;
            }
            MessageBoxShow("Fail removing Path(s)");
            return;
        }

        public void ListViewMouseDown(ListView listView, MouseEventArgs e)
        {
            bool match = false;

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                MenuItem[] mi = null;
                MenuItem addNext = null;
                List<string> selected = new List<string>();
                if (listView.SelectedItems != null)
                {
                    foreach (ListViewItem item in listView.SelectedItems)
                    {
                        selected.Add(item.Text);
                        if (addNext != null) continue;

                        if (listView == listView2)
                        {
                            addNext = new MenuItem("Exclude");
                        }
                        else
                        {
                            string s = item.Text;
                            if (s == CurrentDirectory) s = "";
                            string path = VSCleanLib.GetFilenameAbsolute(s);
                            if (Directory.Exists(path))
                                addNext = new MenuItem("Include");
                        }
                    }
                    if (addNext != null) mi = new MenuItem[] { new MenuItem("Copy"), addNext };
                    else mi = new MenuItem[] { new MenuItem("Copy") };

                    mi[0].Click += Menu_Click;
                    mi[0].Tag = new object[] { listView, selected };
                    if (mi.Length >= 2)
                    {
                        mi[1].Click += Menu_Click;
                        mi[1].Tag = new object[] { listView, selected };
                    }
                    listView.ContextMenu = new ContextMenu(mi);
                    match = true;

                    if (match)
                    {
                        listView.ContextMenu.Show(listView, new Point(e.X, e.Y));
                    }
                }
            }
        }

        private void Exclude(ListView listView, string s)
        {
            if (s == CurrentDirectory) s = "";
            VSCleanSetting obj = VSCleanLib.GetCurrentSetting();
            if (VSCleanLib.AddExclude(obj, VSCleanLib.GetFilenameAbsolute(s)))
            {
                VSCleanLib.RemoveScan(obj, VSCleanLib.GetFilenameAbsolute(s));
                VSCleanLib.SaveCurrentSetting(obj);
            }
        }

        private void Include(ListView listView, string s)
        {
            if (s == CurrentDirectory) s = "";
            VSCleanSetting obj = VSCleanLib.GetCurrentSetting();
            if (VSCleanLib.AddScan(obj, VSCleanLib.GetFilenameAbsolute(s)))
            {
                VSCleanLib.RemoveExclude(obj, VSCleanLib.GetFilenameAbsolute(s));
                VSCleanLib.SaveCurrentSetting(obj);
            }
        }

        private void Menu_Click(object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;

            object[] objs = (object[])mi.Tag;
            ListView listView = (ListView)objs[0];
            List<string> selected = (List<string>) objs[1];
            
            if (mi.Text == "Copy")
            {
                List<string> transform = new List<string>();
                foreach (string s in selected)
                {
                    string s2 = s;
                    if (s2 == CurrentDirectory) s2 = "";
                    transform.Add(VSCleanLib.GetFilenameAbsolute(s2));
                }
                Clipboard.SetText(string.Join("\r\n", transform));
            }
            else if (mi.Text == "Exclude")
            {
                foreach (string s in selected) Exclude(listView, s);
                LoadSetting();
            }
            else if (mi.Text == "Include")
            {
                foreach (string s in selected) Include(listView, s);
                LoadSetting();
            }
        }
        #endregion

        #region Invoke
        private void ListViewRemove(ListView listView, string key)
        {
            try
            {
                for (int i = listView.Items.Count - 1; i >= 0; i--)
                {
                    if (listView.Items[i].Text == key) listView.Items.RemoveAt(i);
                }
            }
            catch { }
        }

        private void SetText(Control ctrl, string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<Control, string>(SetText), new object[] { ctrl, text });
                return;
            }
            ctrl.Text = text;
        }

        private string GetText(Control ctrl)
        {
            if (InvokeRequired)
            {
                return (string)Invoke(new Func<Control, string>(GetText), new object[] { ctrl });
            }
            return ctrl.Text;
        }

        private void MessageBoxShow(string str)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(MessageBoxShow), new object[] { str });
                return;
            }
            MessageBox.Show(str);
        }
        #endregion

    }
}

//if (listView1.SelectedItems == null || listView1.SelectedItems.Count == 0) return;
//VSCleanSetting obj = VSCleanLib.GetCurrentSetting();
//for (int i = listView1.SelectedItems.Count - 1; i >= 0; i--)
//{
//    string strx = listView1.SelectedItems[i].Text;
//    if (strx == CurrentDirectory) strx = "";

//    string str = VSCleanLib.GetFilenameAbsolute(strx);
//    if (VSCleanLib.ContainsExclude(obj, str))
//    {
//        VSCleanLib.RemoveExclude(obj, str);
//    }
//}
//if (VSCleanLib.SaveCurrentSetting(obj))
//{
//    LoadSetting();
//    return;
//}
//MessageBoxShow("Fail removing Path(s)");
//return;