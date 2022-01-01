using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{
    class Logger
    {
        public static void LogException(Exception ex)
        {
            using (StreamWriter sr = new StreamWriter("log.txt", true))
            {
                sr.WriteLine("Datetime: " + DateTime.Now + "\nMessage: " +
                    ex.ToString() + "\n\n");
            }
        }
    }
}
