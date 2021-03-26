using SFDScriptInjector.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFDScriptInjector
{
    public class ScriptAggregator
    {
        public static async Task<Script> MergeIntoOneScript(params Script[] scripts)
        {
            var s = new Script();
            await Task.Run(() =>
               {
                   foreach (var script in scripts)
                   {
                       s.FilePath += script.FilePath;
                       s.SurroundingRegionName += script.SurroundingRegionName;
                       s.ScriptText += '\n' + script.ScriptText;
                   }
               });
            return s;
        }
    }
}
