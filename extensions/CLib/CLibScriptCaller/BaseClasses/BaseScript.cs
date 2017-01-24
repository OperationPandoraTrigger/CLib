using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLibScriptCaller
{
    interface BaseScript
    {
        void Load(string script, string specialParamter);
        string Execute(string input);
    }
}
