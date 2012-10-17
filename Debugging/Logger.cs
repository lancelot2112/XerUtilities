using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;

namespace XerUtilities.Debugging
{
    public static class Logger
    {
        static Dictionary<string, StreamWriter> writers = new Dictionary<string, StreamWriter>();
        public static void Write(string fileName, string msgName, string msg)
        {
            Check(fileName);
            try
            {
                writers[fileName].WriteLine(msgName + " >> " + msg);
            }
            catch { }          

        }

        public static void Check(string fileName)
        {
            if (!writers.ContainsKey(fileName))
            {
                FileStream fs = new FileStream(fileName + ".txt", FileMode.Create);
                StreamWriter write = new StreamWriter(fs);
                writers.Add(fileName, write);
            }
        }

        public static void Initialize(Game game)
        {
            game.Exiting += (Object sender, EventArgs args) => Close();
        }

        public static void Close()
        {
            foreach (StreamWriter write in writers.Values)
            {
                try
                {
                    write.Flush();
                    write.Close();
                }
                catch { }
            }
        }
    }
}
