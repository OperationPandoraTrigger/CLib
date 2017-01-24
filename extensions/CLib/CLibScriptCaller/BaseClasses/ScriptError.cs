using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLibScriptCaller
{
    class ScriptError: Exception
    {
        public ScriptError() { }
        public ScriptError(string message) : base(message) { }
        public ScriptError(string message, Exception ex) : base(message, ex) { }
    }
}
