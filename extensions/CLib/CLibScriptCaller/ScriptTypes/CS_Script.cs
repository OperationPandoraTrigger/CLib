using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CLibScriptCaller.ScriptTypes
{
    class CS_Script : IBase_Script
    {
        private readonly Regex _referenceRegex = new Regex(@"^[\ \t]*(?:\/{2})?\#r[\ \t]+""([^""]+)""", RegexOptions.Multiline);

        private object _instance = null;
        private MethodInfo _methodInfo = null;
        private Dictionary<string, Assembly> _assemblyData = new Dictionary<string, Assembly>();

        public CS_Script()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
                {
                    Assembly result = null;
                    _assemblyData.TryGetValue(args.Name, out result);
                    return result;
                };
        }

        public void Load(string path, string compilerVersion = "v4.0")
        {
            // Load Source Code
            var source = File.ReadAllText(path);

            // build up references for script call
            List<string> references = new List<string>();
            Match match = _referenceRegex.Match(source);
            while (match.Success)
            {
                var dll = match.Groups[1].Value;
                if (!Path.IsPathRooted(dll))
                {
                    var dllpath = Path.Combine(DllEntry.DefaultPath, dll);
                    if (File.Exists(dllpath))
                    {
                        dll = dllpath;
                    }
                }
                references.Add(dll);
                source = source.Substring(0, match.Index) + source.Substring(match.Index + match.Length);
                match = _referenceRegex.Match(source);
            }

            // create and setup Script Compiler
            Dictionary<string, string> options = new Dictionary<string, string>();
            options.Add("CompilerVersion", compilerVersion);
            CSharpCodeProvider csc = new CSharpCodeProvider(options);
            CompilerParameters parameters = new CompilerParameters();
            parameters.CompilerOptions = "/platform:x86 /optimize";
            parameters.GenerateInMemory = true;
            parameters.ReferencedAssemblies.AddRange(references.ToArray());
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Core.dll");
            parameters.ReferencedAssemblies.Add("Microsoft.CSharp.dll");

            // check if Compiler have errors
            CompilerResults results = csc.CompileAssemblyFromSource(parameters, source);
            if (results.Errors.HasErrors)
            {
                throw new ScriptError(results.Errors[0].ToString());
            }
            var assembly = results.CompiledAssembly;

            // load References
            foreach (var reference in references)
            {
                var dll = reference;
                try
                {
                    var referencedAssembly = Assembly.LoadFrom(dll);
                    _assemblyData[referencedAssembly.FullName] = referencedAssembly;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error Loading " + dll + " - " + ex.Message);
                }
            }
            // set Type and get InvokeMethod Informations
            Type startupType = assembly.GetType("CLibScript", true, true);
            _instance = Activator.CreateInstance(startupType, false);
            MethodInfo invokeMethod = startupType.GetMethod("CLibEntryPoint", BindingFlags.Public | BindingFlags.Static);
            if (invokeMethod == null)
            {
                throw new ScriptError("Unable to access CLR method to wrap through reflection. Make sure it is a public instance method.");
            }
            _methodInfo = invokeMethod;
        }

        public string Execute(string input)
        {
            if (_methodInfo == null)
                throw new NullReferenceException("Script can no be loaded");
            return (string)_methodInfo.Invoke(_instance, new object[] { input });
        }
    }
}