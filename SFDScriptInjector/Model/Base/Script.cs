using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFDScriptInjector.Model.Base
{
    public class Script
    {
        public string SurroundingRegionName { get; set; }
        public string FilePath { get; set; }
        public string ScriptText { get; set; }
    }
}
