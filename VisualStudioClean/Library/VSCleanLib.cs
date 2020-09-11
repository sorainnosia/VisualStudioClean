using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace VisualStudioClean
{
    public class VSCleanLib
    {
        public static string SettingFilename = "settings.json";
        public VSCleanSetting ScanObj = null;
        public List<string> FailList = null;
        public List<string> SuccessList = null;
        public bool IsRunning = false;
        public bool IsRunningScan = false;
        public Action ScanCompleted = null;
        public Action<List<string>, List<string>> Report = null;
        public List<string> DefaultScanEndsWithPath = new List<string>();
        public List<string> DefaultExclusions = new List<string>();
        public static List<string> DefaultOnlyDeleteExtensions = new List<string>();

        static VSCleanLib()
        {
            DefaultOnlyDeleteExtensions.Add(".dll");
            DefaultOnlyDeleteExtensions.Add(".exe");
            DefaultOnlyDeleteExtensions.Add(".config");
            DefaultOnlyDeleteExtensions.Add(".pdb");
            DefaultOnlyDeleteExtensions.Add(".xml");
            DefaultOnlyDeleteExtensions.Add(".manifest");
            DefaultOnlyDeleteExtensions.Add(".nupkg");
            DefaultOnlyDeleteExtensions.Add(".ps1");
            DefaultOnlyDeleteExtensions.Add(".psm1");
            DefaultOnlyDeleteExtensions.Add(".xdt");
            DefaultOnlyDeleteExtensions.Add(".targets");
            DefaultOnlyDeleteExtensions.Add(".css");
            DefaultOnlyDeleteExtensions.Add(".js");
            DefaultOnlyDeleteExtensions.Add(".eot");
            DefaultOnlyDeleteExtensions.Add(".svg");
            DefaultOnlyDeleteExtensions.Add(".ttf");
            DefaultOnlyDeleteExtensions.Add(".woff");
            DefaultOnlyDeleteExtensions.Add(".map");
            DefaultOnlyDeleteExtensions.Add(".rtf");
            DefaultOnlyDeleteExtensions.Add(".doc");
            DefaultOnlyDeleteExtensions.Add(".docx");
            DefaultOnlyDeleteExtensions.Add(".xls");
            DefaultOnlyDeleteExtensions.Add(".xlsx");
            DefaultOnlyDeleteExtensions.Add(".ini");
            DefaultOnlyDeleteExtensions.Add("._");
            DefaultOnlyDeleteExtensions.Add(".props");
            DefaultOnlyDeleteExtensions.Add(".txt");
            DefaultOnlyDeleteExtensions.Add(".cache");
            DefaultOnlyDeleteExtensions.Add(".resources");
            DefaultOnlyDeleteExtensions.Add(".zip");
            DefaultOnlyDeleteExtensions.Add(".json");
            DefaultOnlyDeleteExtensions.Add(".png");
            DefaultOnlyDeleteExtensions.Add(".jpg");
            DefaultOnlyDeleteExtensions.Add(".gif");
            DefaultOnlyDeleteExtensions.Add(".transform");
            DefaultOnlyDeleteExtensions.Add(".nuspec");
        }

        public VSCleanLib()
        {
            DefaultScanEndsWithPath.Add(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "debug");
            DefaultScanEndsWithPath.Add(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "release");
            DefaultScanEndsWithPath.Add(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar + "debug");
            DefaultScanEndsWithPath.Add(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar + "release");

            DefaultExclusions.Add("settings.json");
            DefaultExclusions.Add("VSClean.exe");
            DefaultExclusions.Add("VSClean.exe.config");
        }

        private bool DefaultExclusion(VSCleanSetting obj, string file)
        {
            string file2 = file.TrimEnd(Path.DirectorySeparatorChar);
            foreach (string str in DefaultExclusions)
            {
                string str2 = GetFilenameAbsolute(str).TrimEnd(Path.DirectorySeparatorChar);
                if (str2.ToLower() == file2.ToLower()) return true;
            }
            return false;
        }

        #region Helper
        public static bool AddScan(VSCleanSetting obj, string str)
        {
            if (obj == null) obj = GetCurrentSetting();
            string path = str;
            if (path == "") path = GetWorkingDirectory();
            if (Directory.Exists(path) == false) return false;
            bool contains = ContainsScan(obj, path);
            if (contains) return false;
            contains = ContainsExclude(obj, path);
            if (contains) return false;

            obj.ScanPaths.Add(GetFilenameRelative(path));
            return SaveCurrentSetting(obj);
        }

        public static bool AddCurrentScan()
        {
            VSCleanSetting obj = GetCurrentSetting();
            return AddScan(obj, "");
        }

        public static bool ContainsScan(VSCleanSetting obj, string file)
        {
            if (obj == null || obj.ScanPaths == null) return false;

            string file2 = file.TrimEnd(Path.DirectorySeparatorChar);
            if (file2 == "") file2 = GetFilenameAbsolute("").TrimEnd(Path.DirectorySeparatorChar);
            foreach (string str in obj.ScanPaths)
            {
                string str2 = GetFilenameAbsolute(str).TrimEnd(Path.DirectorySeparatorChar);
                if (str2.ToLower() == file2.ToLower()) return true;
            }
            return false;
        }

        public static bool AddExclude(VSCleanSetting obj, string str)
        {
            if (obj == null) obj = GetCurrentSetting();
            string path = str;
            bool contains = ContainsExclude(obj, path);
            if (contains) return false;

            obj.ExcludePaths.Add(GetFilenameRelative(path));
            return SaveCurrentSetting(obj);
        }

        public static bool ContainsExclude(VSCleanSetting obj, string file)
        {
            if (obj == null || obj.ExcludePaths == null) return false;

            string file2 = file.TrimEnd(Path.DirectorySeparatorChar);
            if (file2 == Path.DirectorySeparatorChar.ToString()) file2 = GetFilenameAbsolute("").TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(file2)) { file2 = file2 + Path.DirectorySeparatorChar; }

            foreach (string str in obj.ExcludePaths)
            {
                string str2 = GetFilenameAbsolute(str).TrimEnd(Path.DirectorySeparatorChar);
                if (Directory.Exists(str2)) str2 = str2 + Path.DirectorySeparatorChar;
                if (str2.ToLower() == file2.ToLower()) return true;
            }
            return false;
        }

        public static bool IsExcluded(VSCleanSetting obj, string file)
        {
            if (obj == null || obj.ExcludePaths == null) return false;

            bool isDirectory = false;
            string file2 = file.TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(file2)) { file2 = file2 + Path.DirectorySeparatorChar; isDirectory = true; }
            if (file2 == Path.DirectorySeparatorChar.ToString()) file2 = GetFilenameAbsolute("").TrimEnd(Path.DirectorySeparatorChar);

            foreach (string str in obj.ExcludePaths)
            {
                string str2 = GetFilenameAbsolute(str).TrimEnd(Path.DirectorySeparatorChar);
                if (Directory.Exists(str2)) str2 = str2 + Path.DirectorySeparatorChar;
                if (str2.ToLower() == file2.ToLower()) return true;
            }

            if (isDirectory == false && obj.OnlyDeleteExtensions != null && obj.OnlyDeleteExtensions.Count > 0)
            {
                //if ((file.ToLower().EndsWith(".cs") || file.ToLower().EndsWith(".vb")) && Path.GetFileName(file).ToLower().StartsWith("temporarygeneratedfile_"))
                //    return false;

                List<string> exts = obj.OnlyDeleteExtensions;
                bool mustDelete = false;
                for (int i = 0; i < exts.Count; i++)
                {
                    string ext = exts[i].ToLower();
                    if (file.ToLower().EndsWith(ext))
                    {
                        mustDelete = true;
                        break;
                    }
                }
                if (mustDelete) return false;
                return true;
            }
            return false;
        }

        public static bool RemoveExclude(VSCleanSetting obj, string file)
        {
            if (obj == null || obj.ExcludePaths == null) return false;
            for (int i = obj.ExcludePaths.Count - 1; i >= 0; i--)
            {
                string str = obj.ExcludePaths[i];
                str = GetFilenameAbsolute(str);
                if (str.ToLower() == file.ToLower())
                {
                    obj.ExcludePaths.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public static bool RemoveScan(VSCleanSetting obj, string file)
        {
            if (obj == null || obj.ScanPaths == null) return false;
            for (int i = obj.ScanPaths.Count - 1; i >= 0; i--)
            {
                string str = obj.ScanPaths[i];
                str = GetFilenameAbsolute(str);
                if (str.ToLower() == file.ToLower())
                {
                    obj.ScanPaths.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public static VSCleanSetting GetCurrentSetting()
        {
            VSCleanSetting obj = null;
            try
            {
                string file = GetFilenameAbsolute(SettingFilename);
                if (File.Exists(file) == false)
                {
                    VSCleanSetting ep = new VSCleanSetting();
                    ep.RemoveEmptyDirectory = true;
                    ep.DeleteCsVb = true;
                    ep.ExcludePaths = new List<string>();
                    ep.ScanPaths = new List<string>();
                    ep.OnlyDeleteExtensions = DefaultOnlyDeleteExtensions;
                    return ep;
                }
                string str = File.ReadAllText(file);
                obj = JsonConvert.DeserializeObject<VSCleanSetting>(str);

                if (obj.DeleteCsVb)
                {
                    if (obj.OnlyDeleteExtensions.Contains(".cs", StringComparer.OrdinalIgnoreCase) == false) obj.OnlyDeleteExtensions.Add(".cs");
                    if (obj.OnlyDeleteExtensions.Contains(".vb", StringComparer.OrdinalIgnoreCase) == false) obj.OnlyDeleteExtensions.Add(".vb");
                }
            }
            catch
            {
            }
            return obj;
        }

        public static bool SaveCurrentSetting(VSCleanSetting obj)
        {
            if (obj == null || obj.ScanPaths == null) return false;
            try
            {
                if (obj.DeleteCsVb)
                {
                    if (obj.OnlyDeleteExtensions.Contains(".cs", StringComparer.OrdinalIgnoreCase)) obj.OnlyDeleteExtensions.RemoveAll(n => n.Equals(".cs", StringComparison.OrdinalIgnoreCase));
                    if (obj.OnlyDeleteExtensions.Contains(".vb", StringComparer.OrdinalIgnoreCase)) obj.OnlyDeleteExtensions.RemoveAll(n => n.Equals(".vb", StringComparison.OrdinalIgnoreCase));
                }
                string str = JsonConvert.SerializeObject(obj);
                string file = GetFilenameAbsolute(SettingFilename);
                File.WriteAllText(file, str);
                return true;
            }
            catch { }
            return false;
        }

        public static string GetWorkingDirectory()
        {
            string currpath = AppDomain.CurrentDomain.BaseDirectory;
            currpath = currpath.TrimEnd(Path.DirectorySeparatorChar);
            currpath = currpath + Path.DirectorySeparatorChar;
            return currpath;
        }

        public static string GetFilenameRelative(string pathorfile)
        {
            string currpath = GetWorkingDirectory();
            pathorfile = pathorfile.TrimEnd(Path.DirectorySeparatorChar);
            if (Directory.Exists(pathorfile)) pathorfile = pathorfile + Path.DirectorySeparatorChar;
            if (pathorfile.StartsWith(currpath, StringComparison.CurrentCultureIgnoreCase))
                pathorfile = pathorfile.Substring(currpath.Length);

            if (pathorfile == Path.DirectorySeparatorChar.ToString()) pathorfile = "";
            return pathorfile;
        }

        public static string GetFilenameAbsolute(string pathorfile)
        {
            string currpath = GetWorkingDirectory();
            if (pathorfile == Path.DirectorySeparatorChar.ToString()) return currpath;

            return Path.Combine(currpath, pathorfile);
        }
        #endregion

        #region Clean
        public void Clean(object objx)
        {
            VSCleanSetting obj = (VSCleanSetting)objx;
            FailList = new List<string>();
            SuccessList = new List<string>();

            OrderedExclusions(obj);

            foreach (string path in obj.ScanPaths)
            {
                string f = GetFilenameAbsolute(path);
                DoClean(obj, f);
            }
            if (obj.RemoveEmptyDirectory) RemoveEmptyDirectory(GetFilenameAbsolute(""));

            IsRunning = false;
            if (Report != null) Report(SuccessList, FailList);
        }

        private void DoClean(VSCleanSetting obj, string str)
        {
            str = GetFilenameAbsolute(str);
            if (DefaultExclusion(obj, str)) return;
            bool exist = false;
            try
            {
                exist = Directory.Exists(str);
            }
            catch { }
            if (exist)
            {
                if (IsExcluded(obj, str)) return;
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(str);
                }
                catch { }
                if (files != null && files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        if (IsExcluded(obj, file)) continue;
                        if (DefaultExclusion(obj, file)) continue;

                        string ss = GetFilenameRelative(file);
                        try
                        {
                            File.Delete(file);
                            if (string.IsNullOrEmpty(ss) == false && SuccessList.Contains(ss) == false) SuccessList.Add(ss);
                        }
                        catch
                        {
                            if (string.IsNullOrEmpty(ss) == false && FailList.Contains(ss) == false) FailList.Add(ss);
                        }
                    }
                }
                string[] dirs = null;
                try
                {
                    dirs = Directory.GetDirectories(str);
                }
                catch { }
                if (dirs != null && dirs.Length > 0)
                {
                    foreach (string dir in dirs)
                    {
                        DoClean(obj, dir);
                    }
                    try
                    {
                        dirs = Directory.GetDirectories(str);
                    }
                    catch { }
                }
                if (dirs == null || dirs.Length == 0)
                {
                    try
                    {
                        Directory.Delete(str);
                    }
                    catch
                    {
                    }
                }
            }
            exist = false;
            try
            {
                exist = File.Exists(str);
            }
            catch { }
            if (exist)
            {
                string ss = GetFilenameRelative(str);
                if (IsExcluded(obj, str)) return;
                try
                {
                    File.Delete(str);
                    if (string.IsNullOrEmpty(ss) == false && SuccessList.Contains(ss) == false) SuccessList.Add(ss);
                }
                catch
                {
                    if (string.IsNullOrEmpty(ss) == false && FailList.Contains(ss) == false) FailList.Add(ss);
                }
            }
        }

        /// <summary>
        /// Ordering ScanPaths/ExcludePaths by length without making the result ordered list having upper case
        /// </summary>
        /// <param name="obj"></param>
        private void OrderedExclusions(VSCleanSetting obj)
        {
            List<string> scanPaths = obj.ScanPaths.Select(m => m.ToUpper()).ToList().OrderByDescending(m => m.Length).ToList();
            List<string> excludePaths = obj.ExcludePaths.Select(m => m.ToUpper()).ToList().OrderByDescending(m => m.Length).ToList();

            // Maintain Long
            List<string> resultScan = new List<string>();
            List<string> resultExclude = new List<string>();

            foreach (string s in scanPaths)
            {
                foreach (string r in obj.ScanPaths)
                {
                    if (s.ToUpper() == r.ToUpper()) resultScan.Add(r);
                }
            }

            foreach (string s in excludePaths)
            {
                foreach (string r in obj.ExcludePaths)
                {
                    if (s.ToUpper() == r.ToUpper()) resultExclude.Add(r);
                }
            }

            obj.ScanPaths = resultScan;
            obj.ExcludePaths = resultExclude;
        }
        #endregion

        #region Scanning
        public void ScanVS()
        {
            string abs = GetFilenameAbsolute("");
            ScanObj = GetCurrentSetting();
            ScanVSInner(abs);
            IsRunningScan = false;
            if (ScanCompleted != null) ScanCompleted();
        }

        private void ScanVSInner(string path)
        {
            foreach (string str in DefaultScanEndsWithPath)
            {
                if (path.ToLower().EndsWith(str.ToLower()))
                    AddScan(ScanObj, path);
            }
            string nameonly = Path.GetFileName(path);
            if (nameonly.ToLower() == "packages")
            {
                if (IsNuget(path))
                    AddScan(ScanObj, path);
            }
            if (nameonly.ToLower() == "bin" || nameonly.ToLower() == "obj")
            {
                string[] dirs2 = Directory.GetDirectories(path);
                int debugrelease = 0;
                foreach(string dir in dirs2)
                {
                    string nameonly2 = Path.GetFileName(dir);
                    if (nameonly2.ToLower() == "debug" || nameonly2.ToLower() == "release") debugrelease++;
                }
                if (debugrelease == 0) //web application
                {
                    AddScan(ScanObj, path);
                }
            }
            string[] dirs = Directory.GetDirectories(path);
            foreach (string dir in dirs)
            {
                ScanVSInner(dir);
            }
        }

        private bool IsNuget(string path)
        {
            string[] files = Directory.GetFiles(path);
            if (files != null && files.Length > 0)
            {
                if (files.Length > 1 || Path.GetFileName(files[0]).ToLower() != "repositories.config")
                    return false;
            }
            string[] dirs = Directory.GetDirectories(path);

            int MinDotCount = 2;
            foreach (string str in dirs)
            {
                int dotcount = str.Count(m => m == '.');
                int last = -1;
                int test = 0;
                if (dotcount < MinDotCount) return false;
                for (int i = 0; i < MinDotCount; i++)
                {
                    if (last == -1)
                    {
                        last = str.LastIndexOf(".");
                        if (last + 1 >= str.Length) return false;
                        string temp = str.Substring(last + 1);
                        if (int.TryParse(temp, out test) == false) return false;
                    }
                    else
                    {
                        string str2 = str.Substring(0, last);
                        last = str2.LastIndexOf(".");
                        if (last + 1 >= str2.Length) return false;
                        string temp = str2.Substring(last + 1);
                        if (int.TryParse(temp, out test) == false) return false;
                    }
                }
            }
            return true;
        }
        #endregion

        private void RemoveEmptyDirectory(string path)
        {
            if (Directory.Exists(path) == false) return;

            string[] innerDirs = Directory.GetDirectories(path);
            foreach(string inner in innerDirs)
            {
                RemoveEmptyDirectory(inner);
            }

            string[] files = Directory.GetFiles(path);
            if (files != null && files.Length > 0) return;

            string[] dirs = Directory.GetDirectories(path);
            if (dirs != null && dirs.Length > 0) return;

            try
            {
                Directory.Delete(path);
            }
            catch { }
        }
    }
}
