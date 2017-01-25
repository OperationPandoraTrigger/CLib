using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using EdgeJs;
namespace CLibScriptCaller.ScriptTypes
{
    class JS_Script : IBase_Script
    {

        private Func<object, Task<object>> script;
        static JS_Script()
        {
            string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();
            foreach (var name in names)
            {
                var parts = new List<string>(name.Split('.'));
                var ext = parts[parts.Count - 1];
                parts.RemoveAt(0);
                parts.RemoveAt(parts.Count - 1);
                if (parts[0] != "edge")
                    continue;
                var path = string.Join("\\", parts) + "." + ext;
                path = Path.Combine(DllEntry.defaultPath, path);
                try
                {
                    if (File.Exists(path))
                        File.Delete(path);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    Stream resFilestream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
                    if (resFilestream != null)
                    {
                        BinaryReader br = new BinaryReader(resFilestream);
                        FileStream fs = new FileStream(path, FileMode.Create);
                        BinaryWriter bw = new BinaryWriter(fs);
                        byte[] ba = new byte[resFilestream.Length];
                        resFilestream.Read(ba, 0, ba.Length);
                        bw.Write(ba);
                        br.Close();
                        bw.Close();
                        resFilestream.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error extracting embedded resources - {0}", ex.Message);
                }
            }
        }

        public void Load(string path, string specialParamter)
        {
            script = Edge.Func(path);
        }

        public string Execute(string args)
        {
            var task = script.Invoke(args);
            task.Wait();
            return (string)task.Result;
        }
    }
}
