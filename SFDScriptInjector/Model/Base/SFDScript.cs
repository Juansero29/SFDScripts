using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFDScriptInjector.Model.Base
{
    public class SFDScript
    {
        public string SurroundingRegionName { get; set; }
        public string FilePath { get; set; }
        public string Text { get; set; }
    }
}
