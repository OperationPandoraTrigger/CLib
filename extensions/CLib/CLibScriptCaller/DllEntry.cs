using System;
using System.Runtime.InteropServices;
using System.Text;
using RGiesecke.DllExport;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using CLibScriptCaller.ScriptTypes;

namespace CLibScriptCaller
{
    public class DllEntry
    {
#if WIN64
        [DllExport("RVExtensionVersion")]
#else
		[DllExport("_RVExtensionVersion@6", CallingConvention.StdCall)]
#endif
        public static void RVExtensionVersion(StringBuilder output, int outputSize)
        {
            outputSize--;
            var executingAssembly = Assembly.GetExecutingAssembly();
            try
            {
                string location = executingAssembly.Location;
                if (location == null)
                    throw new Exception("Assembly location not found");
                output.Append(FileVersionInfo.GetVersionInfo(location).FileVersion);
            }
            catch (Exception e)
            {
                output.Append(e.Message);
            }
        }
        // arma Default Entry Point
#if WIN64
        [DllExport("RVExtension")]
#else
		[DllExport("_RVExtension@12", CallingConvention.StdCall)]
#endif
        public static void RVExtension(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string input)
        {
            if (input != "version")
                return;

            var executingAssembly = Assembly.GetExecutingAssembly();
            try
            {
                var location = executingAssembly.Location;
                if (location == null)
                    throw new Exception("Assembly location not found");
                output.Append(FileVersionInfo.GetVersionInfo(location).FileVersion);
            }
            catch (Exception e)
            {
                output.Append(e);
            }
        }


        public static string DefaultPath
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }
        private static double currentScriptPointer = 0;
        internal static Dictionary<string, IBase_Script> scriptDic = new Dictionary<string, IBase_Script>();
        internal static Dictionary<string, string> scriptNamesDic = new Dictionary<string, string>();

        // CLib Entry Point
        [DllExport("CLibCallScript", CallingConvention = CallingConvention.Winapi)]
        public static string CallScript(string input)
        {
            string[] inputs = input.Split(new char[] { ';' }, 5);

            switch (inputs[0].ToLower())
            {
                // rename function pointer for better and easier sqf work
                case "rename":
                    return RenameScriptPointer(inputs[1], inputs[2]);
                // reload Function(maybe later only in dev mode activ)
                case "reload":
                    return LoadScript(inputs[1], inputs[2], inputs[3], true);
                // get current Pointer for script Function
                case "getpointer":
                    return GetScriptPointer(inputs[0]);
                case "compile":
                case "load":
                    // Load Script and Compile it, Return ScriptPointer to SQF so that we can call later the Script
                    return LoadScript(inputs[1], inputs[2], inputs[3], false);

                // Run Loaded Script and Return Data to SQF
                case "call":
                case "run":
                    return RunScript(inputs[1], inputs[2]);
                // Run Script without Pre-Loading of the script
                case "instrun":
                    return LoadAndRunScript(inputs[1], inputs[2], inputs[3], inputs[4]);
                // remove/delte Script from Dic
                case "delete":
                case "remove":
                    return RemoveScript(inputs[1]);
                default:
                    return "ERROR: UNKNOWN COMMAND";
            }
        }

        internal static string LoadScript(string scriptType, string path, string specialParamter, bool reload)
        {
            // check if script is allready loaded and dont reload script, return old/known scriptPointer
            if (reload && scriptDic.ContainsKey(path.ToLower()))
                return scriptNamesDic[path.ToLower()];

            // select right script Type(maybe add Later more types of compatible Scripts)
            IBase_Script script = null;
            switch (scriptType.ToLower())
            {
                case "cs":
                case "csharp":
                    script = new CS_Script();
                    break;
                case "lua":
                    script = new LUA_Script();
                    break;
                case "js":
                case "javascript":
                case "nodejs":
                    script = new JS_Script();
                    break;
                case "rb":
                case "ruby":
                    script = new RB_Script();
                    break;
                case "py":
                case "python":
                    script = new PY_Script();
                    break;
                default:
                    return "ERROR: SCRIPT TYPE DONT EXIST";
            }
            // Load Script
            try
            {
                script.Load(path.ToLower(), specialParamter);
            }
            catch
            {
                return "ERROR: WHILE LOADING SCRIPT";
            }


            // register Script in Script Dic
            string pointer = currentScriptPointer.ToString();
            scriptDic.Add(pointer, script);
            scriptNamesDic.Add(path.ToLower(), pointer);

            currentScriptPointer++;
            // return Current Script pointer to SQF so that it can get called later
            return pointer;

        }

        internal static string RunScript(string pointer, string args)
        {
            if (scriptDic.ContainsKey(pointer))
                return "ERROR: SCRIPT POINTER NOT FOUND";

            try
            {
                return scriptDic[pointer].Execute(args);
            }
            catch
            {
                return "ERROR: WHILE EXECUTING SCRIPT";
            }
        }

        // useless function but still her...
        internal static string RemoveScript(string pointer)
        {
            if (!scriptDic.ContainsKey(pointer))
                return "ERROR SCRIPT NOT FOUND";
            scriptDic.Remove(pointer);
            return "SUCCESS SCRIPT REMOVED";
        }

        // another useless function...
        internal static string RenameScriptPointer(string oldPointer, string newPointer)
        {
            if (!scriptDic.ContainsKey(oldPointer))
                return "ERROR: SCRIPT POINTER NOT FOUND";
            if (scriptDic.ContainsKey(newPointer))
                return "ERROR: NEW SCRIPT POINTER ALLREADY USED";
            IBase_Script script = scriptDic[oldPointer];
            RemoveScript(oldPointer);
            scriptDic.Add(newPointer, script);
            return "DONE";
        }
        // Simple Wraper for faster call results
        internal static string LoadAndRunScript(string scriptType, string path, string specialParamter, string args)
        {
            string pointer;
            // if script allready exist in dic than only call it
            if (scriptNamesDic.ContainsKey(path))
            {
                pointer = scriptNamesDic[path];
                return pointer + ";" + RunScript(pointer, args);
            }
            // load Script and save Pointer
            pointer = LoadScript(scriptType, path, specialParamter, false);

            string output = RunScript(pointer, args);

            return pointer + ";" + output;
        }

        internal static string GetScriptPointer(string path)
        {
            if (scriptNamesDic.ContainsKey(path.ToLower()))
                return "";
            return scriptNamesDic[path];
        }
    }
}
