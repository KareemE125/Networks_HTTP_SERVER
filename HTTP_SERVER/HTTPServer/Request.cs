using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
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
        string requestString; //-> "GET /main.html HTTP/1.1"

        string[] requestLines; //-> ["GET", "/main.html", "HTTP/1.1"]
       
        string[] requestStringArr; //-> ["GET /main.html HTTP/1.1"
                                   //   "Host: 127.0.0.1:1000",
                                   //   "Connection: keep-alive",
                                   //   "sec-ch-ua: " Not A;"]

        public RequestMethod method;
        public string relativeURI;   //-> aboutus.html
        public HTTPVersion httpVersion;
        public bool BadRequest = true;

        Dictionary<string, string> headerLines = new Dictionary<string, string> { };
        

        public string[] RequestStringArr
        {
            get { return requestStringArr; }
        }

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

            requestLines = requestString.Split(' '); //"GET /aboutus.html HTTP/1.1\r" -------> 3 separate values
            bool isBlanckLine = ValidateRequest();
           
            if (requestLines.Length >= 3 && isBlanckLine)
            {
                bool validUri = ValidateIsURI(requestLines[1]); // -> /aboutus.html
                bool isParseRequestLine = ParseRequestLine();
                bool isLoadHeaderLines = LoadHeaderLines();
                
                if (validUri && isParseRequestLine && isLoadHeaderLines)    
                    return true;
            }
            return false;
        }

        private bool ParseRequestLine()
        {
            //parse "GET /aboutus.html HTTP/1.1\r"
            try
            {

                if (requestLines[0].Trim() == "GET")
                    method = RequestMethod.GET;
                else if (requestLines[0].Trim() == "POST")
                    method = RequestMethod.POST;
                else if (requestLines[0].Trim() == "HEAD")
                    method = RequestMethod.HEAD;
                else return false;

                // --> /aboutus.html  --> aboutus.html 
                //remove slash from aboutus.html
                relativeURI = requestLines[1].Trim().Remove(0, 1);


                if (requestLines[2].Trim() == "HTTP/1.1")
                    httpVersion = HTTPVersion.HTTP11;
                else if (requestLines[2].Trim() == "HTTP/1.0")
                    httpVersion = HTTPVersion.HTTP10;
                else if (requestLines[2].Trim() == "HTTP/0.9")
                    httpVersion = HTTPVersion.HTTP09;
                else return false;

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }
        }

        private bool ValidateIsURI(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute);
        }
        private bool LoadHeaderLines()
        {
            try
            {

                String[] header = requestStringArr[1].Split(':'); //Host: 127.0.0.1:1000 -> ["Host", " 127.0.0.1", "1000"]
                String ipAndPort = header[1]+":"+ header[2];
                headerLines.Add(header[0].Trim(), ipAndPort.Trim()); //add host as key and ip/port as value to dictionary


                foreach ( String elem in requestStringArr.Skip(2)) //skip "GET ... ..." and "Host ... ... .."
                {
                    //Convert headers to map
                    //
                    //   Connection: keep-alive
                    //   sec-ch-ua: " Not A;Brand";v="99", "Chromium";v="96", "Google Chrome";v="96"
                    //   sec-ch-ua-mobile: ?0
                    //   sec-ch-ua-platform: "macOS"
                    if (elem.Trim() == "")
                        break; 
                    header = elem.Split(':');
                    headerLines.Add(header[0].Trim(), header[1].Trim());
                }
           
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return false;
            }
        }
        private bool ValidateRequest()
        {
            //validate all 3 parts of the request "GET /aboutus.html HTTP/1.1\r" are not empty
            for (int i = 0; i < requestLines.Length; i++)
            {

                if (requestLines[i] == " " || String.IsNullOrEmpty(requestLines[i]))
                    return false; 
            }

            //validate presence of blank line to differentiate between headers and body
            for (int i = 0; i < requestStringArr.Length; i++)
            {
                if (requestStringArr[i] == "\r")   //not \n since I split on \r
                    return true;
            }

            return false;
        }

        public Dictionary<String, String> extractPostData()
        {
            //normal headers
            //Accept-Encoding: gzip, deflate, br
            //Accept - Language: en - GB,en; q = 0.9
            //
            //name=mohamed&otherName=agina

            //need to convert
            //name=mohamed&otherName=agina
            //->
            //{
            //name: mohamed,
            //otherName: agina
            //}
            try
            {
                int lastIdx = requestStringArr.Length - 1;   //data is present in last sentence of request
                string postMethodData = requestStringArr[lastIdx];

                //postMethodData may be	name=mohamed&otherName=agina\0\0\0\0\0\


                Dictionary<String, String> receivedPostData = new Dictionary<string, string>();
                String[] pairs = postMethodData.Split('&');  //split key-value pairs on the &

                foreach (string keyValue in pairs)
                {
                    String[] keyValueSeparated = keyValue.Split('=');  //split key from value using equal

                    keyValueSeparated[1] = keyValueSeparated[1].Split('\0')[0];   //agina\0\0\0\0 -> agina

                    receivedPostData[keyValueSeparated[0]] = keyValueSeparated[1];  //add key and value to dictionary
                }

                return receivedPostData;
            }catch(Exception e)
            {
                Logger.LogException(e);
                return new Dictionary<string, string> { };
            }
            
        }

        public String convertPostDataToJsonString(Dictionary<String, String> postData)
        {
            //convert {
            //name: mohamed,
            //otherName: agina
            //}
            //to a json string

            postData["relativeURI"] = relativeURI;

            string data = JsonConvert.SerializeObject(postData, Formatting.Indented);    //convert object to json string
            Console.WriteLine("this is the data: " + data);
            return data;
        }

    }
}
