using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace HTTPServer
{
    class Server
    {
        Socket serverSocket;
        StatusCode statusCode;
        bool notBadRequest;
     
        public Server(int portNumber, string redirectionMatrixPath)
        { 
            LoadRedirectionRules(redirectionMatrixPath);

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint serverEndpoint = new IPEndPoint(IPAddress.Any, portNumber);  

            serverSocket.Bind(serverEndpoint);  
        }

        public void StartServer()
        {
            int maxConnectionsNotAccepted = 5;  //tcp accepted them, but application didn't yet
            serverSocket.Listen(maxConnectionsNotAccepted);

            while (true)
            {
                
                Socket clientSocket = this.serverSocket.Accept();

                Console.WriteLine("New client accepted: {0}", clientSocket.RemoteEndPoint);

                Thread newthread = new Thread(new ParameterizedThreadStart(HandleConnection));
                
                newthread.Start(clientSocket);
            }

        }

        public void HandleConnection(object obj)
        {
            Socket clientSock = (Socket)obj;
            clientSock.ReceiveTimeout = 1000;   //1000 ms, if no data received,
                                                //clientSock.Receive() throws exception,
                                                //and we close the connection
                                                //set to 0 if needs to be infinite
            byte[] data;
            int receivedLength;

            while (true)
            {
                try
                {
                    data = new byte[8192];
                    receivedLength = clientSock.Receive(data);


                    if (receivedLength == 0)
                    {
                        Console.WriteLine("Client: {0} ended the connection", clientSock.RemoteEndPoint);
                        break;
                    }

                    string convertedData = Encoding.ASCII.GetString(data);
                    Console.WriteLine(convertedData);
                    string[] fullHeader = convertedData.Split('\n');

                    Request request = new Request(fullHeader);

                    Response response = HandleRequest(request);
                    
                    clientSock.Send(Encoding.ASCII.GetBytes(response.ResponseString));
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    break;   //break so I dont keep on receiving from client
                }
            }

             clientSock.Close();
        }

        Response HandleRequest(Request request)
        {
       
            string content;
            try
            {
     //throw new Exception();
                //check for bad request
                notBadRequest = request.ParseRequest();// &&  //need to make sure that I parsed req successfully
                    //request.relativeURI.Contains(".html"); //and that html extension is present
                if (!notBadRequest)
                {
                    request.relativeURI = Configuration.BadRequestDefaultPageName;
                    statusCode = StatusCode.BadRequest;
                    content = LoadDefaultPage(request.relativeURI);
                    return new Response(statusCode, "text/html", content,
                        headHttpStatus: request.method == RequestMethod.HEAD);
                }
                //-------------------------------------------------------------------------

                //check for post request
                if (request.method == RequestMethod.POST)
                {
                    return HandlePostRequest(request);
                }
                

                //check for redirect request
                String redirectedPhysicalPath = GetRedirectionPagePathIFExist(request.relativeURI);
                if (redirectedPhysicalPath != "")
                {
                    statusCode = StatusCode.Redirect;
                    content = LoadDefaultPage(redirectedPhysicalPath);
                    return new Response(statusCode, "text/html", content,
                        redirectedPhysicalPath, headHttpStatus: request.method == RequestMethod.HEAD);
                }
                //-------------------------------------------------------------------------


                //check for not found request
                String physicalPath = Configuration.RootPath + "\\" + request.relativeURI;
                if (!File.Exists(physicalPath))
                {
                    request.relativeURI = Configuration.NotFoundDefaultPageName;
                    statusCode = StatusCode.NotFound;
                    content = LoadDefaultPage(request.relativeURI);
                    return new Response(statusCode, "text/html", content,
                        headHttpStatus: request.method == RequestMethod.HEAD);
                }
                //-------------------------------------------------------------------------



                //a normal status 200 request where the requested resource is found
                statusCode = StatusCode.OK;
                content = LoadDefaultPage(request.relativeURI);
                return new Response(statusCode, "text/html", content,
                    headHttpStatus: request.method == RequestMethod.HEAD);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);

                //Internal server error request
                request.relativeURI = Configuration.InternalErrorDefaultPageName;
                statusCode = StatusCode.InternalServerError;
                content = LoadDefaultPage(request.relativeURI);
                return new Response( statusCode, "text/html", content,
                    headHttpStatus: request.method == RequestMethod.HEAD);
            }
        }

        private string GetRedirectionPagePathIFExist(string relativePath)
        {
            try 
            {
                return Configuration.RedirectionRules[relativePath];
            }
            catch(Exception ex)
            {
                Logger.LogException(ex);
                return "";
            }
            
        }

        private string LoadDefaultPage(string defaultPageName)
        {
            string filePath = Path.Combine(Configuration.RootPath, defaultPageName);

            try
            {
                String fileContent = File.ReadAllText(filePath);
                return fileContent;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                return "";
            }
        }

        private void LoadRedirectionRules(string filePath)
        {
            //read redirectionrules from file into Configuration.RedirectionRules
            try
            {
                String[] fileData = File.ReadAllLines(filePath);
                foreach (String elem in fileData)
                {
                    //when request to aboustus.html ==> it redirects me to aboutus2.html
                    String[] redrectString = elem.Split('-');
                    Configuration.RedirectionRules = new Dictionary<string, string>
                    {
                        { redrectString[0] , redrectString[1] }
                    };
                }

            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                Environment.Exit(1);
            }
        }

        private Response HandlePostRequest(Request request)
        {
            //normal headers
            //Accept-Encoding: gzip, deflate, br
            //Accept - Language: en - GB,en; q = 0.9
            //
            //name=mohamed&otherName=agina

            String content;
            Dictionary<String, String> receivedPostData = request.extractPostData();

            if (receivedPostData.Count == 0)
            {
                request.relativeURI = Configuration.BadRequestDefaultPageName;
                statusCode = StatusCode.BadRequest;
                content = request.convertPostDataToJsonString(new Dictionary<string, string>() { { "Error", "Invalid data format used" } });
                return new Response(statusCode, "application/json", content);
            }

            content = request.convertPostDataToJsonString(receivedPostData);
            statusCode = StatusCode.OK;
            return new Response(statusCode, "application/json", content);
        }
    }
}
