using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Xml;

namespace SharedLib
{
    public delegate void EventReceiveResponse(XmlDocument req, XmlDocument res);

    public class Listener
    {
        NetworkStream _stream;
        TcpClient client = null;
        public String server = "localhost";
        public int port = 6666;
        public Thread steamingThread;
        private readonly Dictionary<string, XmlDocument> _requestTable = new Dictionary<string, XmlDocument>();
        volatile public bool stop = false;
        public EventReceiveResponse onReceiveResponse;

        public Listener()
        {
        }

        public void ConnectListener()
        {
            try
            {
                if (client == null || !client.Connected)
                {
                    client = new TcpClient(server, port);
                }

                if (_stream == null)
                {
                    Console.WriteLine("Start Steaming");
                    _stream = client.GetStream();

                    if (steamingThread == null)
                    {
                        steamingThread = new Thread(UpdateListen);
                        steamingThread.Start();
                        Console.WriteLine("Started Steaming");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Not found server");
                return;
            }

        }

        public void UpdateListen()
        {
            try
            {
                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;
                String buffer = null;

                // Enter the listening loop.
                while (!stop)
                {
                    try
                    {
                        if (_stream == null)
                        {
                            ConnectListener();
                        }
                        data = null;
                        int i;
                        _stream = client.GetStream();
                        // Loop to receive all the data sent by the client.
                        _stream.ReadTimeout = 15000;
                        while ((i = _stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            if (stop)
                            {
                                client.Close();
                                return;
                            }

                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                            buffer += data;
                            if (buffer.EndsWith("</request>") || buffer.EndsWith("</response>"))
                            {
                                // Process the data sent by the client.

                                string response = OnReceive(buffer);
                                buffer = null;
                                if (response != string.Empty)
                                {
                                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(response);

                                    // Send back a response.
                                    _stream.Write(msg, 0, msg.Length);
                                }
                            }
                        }

                        // Shutdown and end connection
                        client.Close();
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine(e.Message + "@Line 997");
                        _stream = null;
                        Thread.Sleep(200);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + "@Line 1003");
                        _stream = null;
                        Thread.Sleep(200);
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message + "@Line 1011");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "@Line 1015");
            }

        }

        public void SendMessage(string message)
        {
            if (_stream == null) return;

            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                ConnectListener();
                //messagingWindow.txtGameIP.Enabled = false;
                //messagingWindow.txtGamePort.Enabled = false;

                string requestID = string.Empty;
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(message);

                    XmlNode nodeRequestID = doc.DocumentElement.SelectSingleNode("/request/id");
                    XmlNode nodeAction = doc.DocumentElement.SelectSingleNode("/request/action");

                    if (nodeRequestID == null)
                    {
                        Console.WriteLine("Error: request/id not specify");
                    }
                    else if (nodeAction == null)
                    {
                        Console.WriteLine("Error: request/action not specify");
                    }
                    else
                    {
                        _requestTable.Add(nodeRequestID.InnerText, doc);
                    }


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "@Line 842");
                    Console.WriteLine("Error: request/action not specify");
                }



                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();



                // Send the message to the connected TcpServer. 
                _stream.Write(data, 0, data.Length);

                Console.WriteLine(string.Format("Sent: {0}", message));

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                //Int32 bytes = stream.Read(data, 0, data.Length);
                //responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //Debug(string.Format("Received: {0}", responseData));

                // Close everything.
                //stream.Close();
                //client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(string.Format("ArgumentNullException: {0}", e));
                Console.Write("@Line 880");
            }
            catch (SocketException e)
            {
                Console.WriteLine(string.Format("SocketException: {0}", e));
                Console.Write("@Line 885");
            }
            catch (Exception e)
            {
                Console.WriteLine(string.Format("Exception: {0}", e));
                Console.Write("@Line 890");
            }

        }


        string OnReceive(string input)
        {
            string requestID = string.Empty;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(input);

                XmlNode nodeReponse = doc.DocumentElement.SelectSingleNode("/response");
                if (nodeReponse != null)
                {
                    XmlNode nodeResonseID = doc.DocumentElement.SelectSingleNode("/response/id");

                    if (_requestTable.ContainsKey(nodeResonseID.InnerText))
                    {
                        if (onReceiveResponse != null)
                            onReceiveResponse(_requestTable[nodeResonseID.InnerText], doc);
                        _requestTable.Remove(nodeResonseID.InnerText);
                    }
                    return String.Empty;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Listener OnReceive " + e.Message);
                return String.Empty;
            }
            return String.Empty;
        }
    }
}

