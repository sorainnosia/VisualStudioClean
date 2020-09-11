using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualStudioClean
{
    public class VSCleanSetting
    {
        public bool RemoveEmptyDirectory { get; set; }
        public bool DeleteCsVb { get; set; }
        public List<string> OnlyDeleteExtensions { get; set; }
        public List<string> ScanPaths { get; set; }
        public List<string> ExcludePaths { get; set; }
    }
}
