using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTTPServer
{
    public enum RequestMethod
    {
        GET,
        POST,
        HEAD
    }

    public enum HTTPVersion
    {
        HTTP10,
        HTTP11,
        HTTP09
    }

    class Request
    {
        string requestString;
        string[] requestLines;
        string[] requestStringArr;
        RequestMethod method;
        public string relativeURI;
        public HTTPVersion httpVersion;
        public bool BadRequest = true;

        Dictionary<string, string> headerLines=new Dictionary<string, string>
        {
        
        };

        

        public Dictionary<string, string> HeaderLines
        {
            get { return headerLines; }
        }

        public Request(string[] requestStringArr)
        {
            this.requestStringArr = requestStringArr;
            this.requestString = requestStringArr[0];
            
        }


        public bool ParseRequest()
        {
            requestString = requestString.Replace("\r\n", "\n");

            requestLines = requestString.Split(' ');
            
            bool isBlanckLine = ValidateBlankLine();
           
            if (requestLines.Length >= 3 && isBlanckLine == true)
            {

                bool isParseRequestLine = ParseRequestLine();
                bool isLoadHeaderLines =LoadHeaderLines();
                
                if (isParseRequestLine &&isLoadHeaderLines)
                {
                    Console.WriteLine("ggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg");
                    foreach(var em  in headerLines)
                    {
                        Console.WriteLine(em.Key + " " + em.Value);

                    }
                    Console.WriteLine("rrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr");
                    return true;
                }

            }
                       
            return false;

        }

        private bool ParseRequestLine()
        {
            try
            {

                String[] parts = requestLines;
                if (parts[0].Trim() == "GET") { method = RequestMethod.GET; BadRequest = false; }
                else if (parts[0].Trim() == "POST") { method = RequestMethod.POST; BadRequest = false; }
                else if (parts[0].Trim() == "HEAD") { method = RequestMethod.HEAD; BadRequest = false; }


                relativeURI = parts[1].Trim();
                string[] URi = relativeURI.Split('/');
                relativeURI = URi[1];

                
                if (parts[2].Trim() == "HTTP/1.1") { httpVersion = HTTPVersion.HTTP11; Configuration.ServerHTTPVersion = "HTTP/1.1"; }
                else if (parts[2].Trim() == "HTTP/1.0") { httpVersion = HTTPVersion.HTTP10; Configuration.ServerHTTPVersion = "HTTP/1.0"; }
                else if (parts[2].Trim() == "HTTP/0.9") { httpVersion = HTTPVersion.HTTP09; Configuration.ServerHTTPVersion = "HTTP/0.9"; }

                return true;
            }
            catch (Exception ex) { return false; }
        }

        private bool ValidateIsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);
        }
        private bool LoadHeaderLines()
        { try
            {

                String[] header = requestStringArr[1].Split(':');
                String firstHeader = header[1]+":"+ header[2];
                headerLines.Add(header[0].Trim(), firstHeader.Trim());

                foreach ( String elem in requestStringArr.Skip(2))
                {
                    if (elem.Trim() == "") { break; }
                    header = elem.Split(':');
                    headerLines.Add(header[0].Trim(), header[1].Trim());
                }
           
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private bool ValidateBlankLine()
        {
            for (int i = 0; i < requestLines.Length; i++)
            {
                
                if (requestLines[i] == " ") { return false; }
            }
            return true;
        }

    }
}
