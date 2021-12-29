using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HTTPServer
{

    public enum StatusCode
    {
        OK = 200,
        InternalServerError = 500,
        NotFound = 404,
        BadRequest = 400,
        Redirect = 301
    }

    class Response
    {
        string responseString;
        Request request;

        public string ResponseString
        {
            get
            {
                return responseString;
            }
        }

        

        List<string> headerLines = new List<string>();

        public Response( StatusCode code, string contentType, string content, string redirectoinPath)
        {
           

            String statusLine = GetStatusLine(code);
           
            responseString = Configuration.ServerHTTPVersion + " " + statusLine + " " + code + "\r\nContent_Type:" + contentType + "\r\nContent_Length:" + content.Length + "\r\nDate:" + DateTime.Now + "\r\n" + "\r\n" + content; 
            if (redirectoinPath != "") { responseString = responseString + "Location: "+ redirectoinPath;  }

            Console.WriteLine(responseString);
        }

        private string GetStatusLine(StatusCode code)
        {

            int codeNumber = 0;
            if (code == StatusCode.OK) { codeNumber = 200; }
            else if (code == StatusCode.InternalServerError) { codeNumber = 500; }
            else if(code == StatusCode.NotFound) { codeNumber = 404; }
            else if(code == StatusCode.BadRequest) { codeNumber = 400; }
            else if(code == StatusCode.Redirect) { codeNumber = 301; }

            
            
            return codeNumber.ToString();
        }
    }
}
