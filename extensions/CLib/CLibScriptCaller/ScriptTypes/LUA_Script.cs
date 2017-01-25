using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NLua;

namespace CLibScriptCaller.ScriptTypes
{
    class LUA_Script : IBase_Script
    {

        private LuaFunction function = null;
        private Lua luaEngine = null;
        static LUA_Script()
        {

        }

        public void Load(string path, string specialParamter)
        {
            luaEngine = new Lua();
            path = Path.Combine(DllEntry.defaultPath, path);
            string source = File.ReadAllText(path);
            luaEngine.DoString(source);
            
            function = luaEngine.GetFunction("CLibEntryPoint");
        }

        public string Execute(string args)
        {
            if (luaEngine == null || function == null)
                return "ERROR: FUNCTION DONT EXIST";

            return function.Call(new object[] { args }).First().ToString();
        }
    }
}
