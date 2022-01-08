using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Logger
    {
        public static string obj = "";
        public static void LogException(Exception ex)
        {
            lock (obj)
            {
                using (StreamWriter sr = new StreamWriter("log.txt", true))
                {
                    sr.WriteLine("Datetime: " + DateTime.Now + "\nMessage: " +
                        ex.ToString() + "\n\n");
                }
            }
        }
    }
}
