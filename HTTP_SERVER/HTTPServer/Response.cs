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

        
        public Response(StatusCode code, string contentType, string content, string redirectoinPath = "", bool headHttpStatus = false)
        { 
            String statusLine = GetStatusLine(code);

            responseString = statusLine + "\r\n" +
                            "Content_Type:" + contentType + "\r\n" +
                            "Content_Length:" + content.Length + "\r\n" +
                            "Date:" + DateTime.Now + "\r\n";

            if (!headHttpStatus)  //head doesnt contain content, but everything else is just like get request
                responseString += "\r\n" + content;
            

            if (redirectoinPath != "")
                responseString = responseString + "Location: "+ redirectoinPath; 

            Console.WriteLine("This is the response string: \n" + responseString);
        }

        private string GetStatusLine(StatusCode code)
        {
            //ex -> "HTTP/1.1 301 Redirect"
            int codeNumber = 0;
            if (code == StatusCode.OK)
                codeNumber = 200;
            else if (code == StatusCode.InternalServerError)
                codeNumber = 500; 
            else if(code == StatusCode.NotFound)
                codeNumber = 404; 
            else if(code == StatusCode.BadRequest)
                codeNumber = 400; 
            else if(code == StatusCode.Redirect)
                codeNumber = 301;


            return Configuration.ServerHTTPVersion + " " + codeNumber.ToString() + " " + code;
        }
    }
}
