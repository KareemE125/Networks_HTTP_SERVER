using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{

    class Program
    {
        static void Main(string[] args)
        { 

            CreateRedirectionRulesFile();
            Server s1 = new Server(1000, "redirectionRules.txt");
            Console.WriteLine("Booting.....");
            s1.StartServer();
        }

        static void CreateRedirectionRulesFile()
        {
            using (StreamWriter streamWriter = new StreamWriter(File.Open("redirectionRules.txt", FileMode.Create)))
            {
                streamWriter.Write("aboutus.html-aboutus2.html");
            }
        }


    }
}
