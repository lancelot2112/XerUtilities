using System.IO;
using System.Text;
using System.Collections.Generic;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

using IronPython.Hosting;

using XerUtilities.Debugging;


namespace XerUtilities.Scripting
{
    public class IronPythonHost
    {
        public static IConsoleHost Console;
        public static ScriptRuntime pyRuntime;
        public static ScriptEngine pyEngine;
        static ScriptScope pyScope;

        ScriptSource pySource;
        MemoryStream output;

        public IronPythonHost(IConsoleHost host)
        {
            Console = host;

            output = new MemoryStream();
            output.SetLength(0);

            pyRuntime = Python.CreateRuntime();
            pyRuntime.IO.SetOutput(output, Encoding.ASCII);
            pyEngine = Python.GetEngine(pyRuntime);
            pyScope = pyEngine.CreateScope();
            ExecutePythonCode("import clr");
            ExecutePythonCode("import Microsoft");
            ExecutePythonCode("clr.AddReference(\"Microsoft.Xna.Framework\")");
            ExecutePythonCode("import Microsoft.Xna.Framework");            
            ExecutePythonCode("import Microsoft.Xna.Framework.Graphics");
            ExecutePythonCode("from Microsoft.Xna.Framework import *");
            ExecutePythonCode("from Microsoft.Xna.Framework.Graphics import *");
            SetVariable("_runtime", this);
            Console.Echo("[HOST] Using IRONPYTHON v2.7.1");
        }

        public static void SetVariable(string name, object obj)
        {
            pyScope.SetVariable(name, obj);
        }

        public void ExecutePythonCode(string code)
        {
            pySource = pyEngine.CreateScriptSourceFromString(code,SourceCodeKind.InteractiveCode);
            object output = pySource.Execute(pyScope);
            if (output != null) Console.Echo(pyEngine.Operations.Format(output));
            ShowResults();
        }

        public void ShowResults()
        {
            if (output.Length > 0)
            {
                byte[] buff = new byte[output.Length];
                output.Position = 0;
                output.Read(buff, 0, buff.Length);
                string result = Encoding.ASCII.GetString(buff);
                Console.Echo(result);
                output.SetLength(0);
            }
        }

        public void Directory()
        {
            foreach (KeyValuePair<string, dynamic> dyn in pyScope.GetItems())
            {
                if(dyn.Value is object)
                Console.Echo(dyn.Key);
            }
        }
    }
}
