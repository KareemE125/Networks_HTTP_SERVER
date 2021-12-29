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
        int portNumber;
        string redirectionMatrixPath;
        StatusCode code;
        bool notbadrequest;
     
        public Server(int portNumber, string redirectionMatrixPath)
        {
            this.redirectionMatrixPath = redirectionMatrixPath;

            this.portNumber = portNumber;
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint hostEndPoint = new IPEndPoint(IPAddress.Any, portNumber);

            serverSocket.Bind(hostEndPoint);  
        }

        public void StartServer()
        {

            serverSocket.Listen(100);
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
            clientSock.ReceiveTimeout = 0;
            byte[] data;
            int receivedLength;

            while (true)
            {
                try
                {
                    data = new byte[1024];
                    receivedLength = clientSock.Receive(data);


                    if (receivedLength == 0)
                    {
                        Console.WriteLine("Client: {0} ended the connection", clientSock.RemoteEndPoint);
                        break;
                    }

                    string ConvertedData = Encoding.ASCII.GetString(data);
                    Console.WriteLine(ConvertedData);
                    string[] header = ConvertedData.Split('\n');
               


                    Request request = new Request(header);

                    Response response = HandleRequest(request);
                    
                    clientSock.Send(Encoding.ASCII.GetBytes(response.ResponseString), 0, response.ResponseString.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex); 
                }
                break;
            }

             clientSock.Close();
        }

        Response HandleRequest(Request request)
        {
       
            string content;
            try
            {
                //throw new Exception();
                LoadRedirectionRules(@"redirectionRules.txt");

                try
                {
                    request.ParseRequest();
                   
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    request.relativeURI = Configuration.InternalErrorDefaultPageName;

               
                }

                
                //TODO: map the relativeURI in request to get the physical path of the resource.
                String physicalPath = Configuration.RootPath + "\\" + request.relativeURI;

                //TODO: check for redirect
                String redirectedPhysicalPath = GetRedirectionPagePathIFExist(request.relativeURI);

                notbadrequest = request.relativeURI.Contains(".html");//....



                if (!notbadrequest||request.BadRequest)
                {
                    request.relativeURI = Configuration.BadRequestDefaultPageName;

                    code = StatusCode.BadRequest;
                   
                     content = LoadDefaultPage(request.relativeURI);

                    return new Response(code, "text/html", content, "");
                }


                if (!File.Exists(physicalPath))
                {
                    request.relativeURI = Configuration.NotFoundDefaultPageName;

                    code = StatusCode.NotFound;

                    content = LoadDefaultPage(request.relativeURI);


                    return new Response(code, "text/html", content, "");
                }
                //TODO: read the physical file
                //byte[] fileData = new byte[1000];

                if (redirectedPhysicalPath != "")
                {
                    

                    code = StatusCode.Redirect;
                 
                    //fileData = File.ReadAllBytes(Configuration.RootPath + "\\"+redirectedPhysicalPath);
                    content = LoadDefaultPage(redirectedPhysicalPath);
                    return new Response(code, "text/html", content, redirectedPhysicalPath);
                }
                else
                {

                    code = StatusCode.OK;

                    //fileData = File.ReadAllBytes(physicalPath);
                    content = LoadDefaultPage(request.relativeURI);
                    return new Response(code, "text/html", content, redirectedPhysicalPath);
                }
                //content = Encoding.ASCII.GetString(fileData).Trim();
                //content = LoadDefaultPage(request.relativeURI);
                
                // Create OK response
                
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                //TODO: check for bad request and not found
                
                //ToDo: Internal Server Error. 
                
                request.relativeURI = Configuration.InternalErrorDefaultPageName;

                code = StatusCode.InternalServerError;

                content = LoadDefaultPage(request.relativeURI);


                return new Response( code, "text/html", content, "");
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
                return string.Empty;
            }
        }

        private void LoadRedirectionRules(string filePath)
        {
            try
            {
                String[] fileData = File.ReadAllLines(filePath);
                foreach (String elem in fileData)
                {
                    //when request to aboustus.html ==> it redirects me to aboutus2.html
                    String[] redrectString = elem.Split('-');
                    Configuration.RedirectionRules= new Dictionary<string, string>
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
    }
}
