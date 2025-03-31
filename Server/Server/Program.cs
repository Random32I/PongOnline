using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.ComponentModel.Design;

//Pong Server

namespace asyncServer
{
    internal class Program
    {
        static float[] ballPos = new float[2];
        static float[] ballDir = new float[2];
        static float paddle1Dir = 0;
        static float paddle2Dir = 0;

        static IPAddress ip = IPAddress.Parse("127.0.0.1");

        private static Socket serverTCP = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        private static Socket Player1 = default;
        private static Socket Player2 = default;
        private static byte[] TCPBuffer = new byte[1024];
        private static byte[] TCPOutBuffer = new byte[1024];
        private static string text = "";

        static void Main(string[] args)
        {
            byte[] outBuffer = new byte[2048];
            byte[] inBuffer = new byte[2048];

            IPEndPoint localEP = new IPEndPoint(ip, 1111);
            Socket serverUDP = new Socket(ip.AddressFamily, SocketType.Dgram, ProtocolType.Udp);

            // Create an EP to capture the info from the sending client
            IPEndPoint client = new IPEndPoint(IPAddress.Any, 0); // 0 means any available port
            EndPoint remoteClient = (EndPoint)client;

            serverTCP.Bind(localEP);

            serverTCP.Listen(1);
            serverTCP.BeginAccept(new AsyncCallback(AcceptCallback), null);
            serverTCP.Listen(1);
            serverTCP.BeginAccept(new AsyncCallback(AcceptCallback), null);

            // Bind, send/receive data
            try
            {
                serverUDP.Bind(localEP);
                Console.WriteLine("Waiting for data...");

                bool paddle1Labeled = false;
                bool paddle2Labeled = false;
                int paddleIndex = 0;
                float paddle1Pos = 0;
                float paddle2Pos = 0;

                while (true)
                {
                    int recv = serverUDP.ReceiveFrom(inBuffer, ref remoteClient);
                    float newPos = 0;
                    bool failed = false;
                    try
                    {
                        newPos = StringToData(Encoding.ASCII.GetString(inBuffer, 0, recv), out paddleIndex, out ballPos[0], out ballPos[1], out ballDir[0], out ballDir[1]);
                        Console.WriteLine("Recieved positions x:" + newPos + " from " + paddleIndex);
                    }
                    catch (Exception e)
                    {
                        //Client Updated
                        failed = true;
                        if (Encoding.ASCII.GetString(inBuffer, 0, recv) == "Game")
                        {
                            outBuffer = Encoding.ASCII.GetBytes($"Game");
                            serverUDP.SendTo(outBuffer, 0, outBuffer.Length, SocketFlags.None, remoteClient);
                        }
                        else
                        {
                            if (!paddle1Labeled || !paddle2Labeled)
                            {
                                if (paddle1Labeled)
                                {
                                    paddleIndex = 2;
                                    paddle2Labeled = true;
                                }
                                else
                                {
                                    paddleIndex = 1;
                                    paddle1Labeled = true;
                                }
                                outBuffer = Encoding.ASCII.GetBytes($"{paddleIndex},{paddle2Labeled}");
                            }
                            serverUDP.SendTo(outBuffer, 0, outBuffer.Length, SocketFlags.None, remoteClient);
                        }
                    }

                    if (!failed)
                    {
                        if (paddleIndex == 1)
                        {
                            paddle1Pos = newPos;

                            outBuffer = Encoding.ASCII.GetBytes($"{paddle2Pos},{2},{ballPos[0]},{ballPos[1]},{ballDir[0]},{ballDir[1]},{paddle2Dir}");
                            serverUDP.SendTo(outBuffer, 0, outBuffer.Length, SocketFlags.None, remoteClient);
                            Console.WriteLine("Sent to 1");
                        }
                        else if (paddleIndex == 2)
                        {
                            paddle2Pos = newPos;

                            outBuffer = Encoding.ASCII.GetBytes($"{paddle1Pos},{1},{ballPos[0]},{ballPos[1]},{ballDir[0]},{ballDir[1]},{paddle1Dir}");
                            serverUDP.SendTo(outBuffer, 0, outBuffer.Length, SocketFlags.None, remoteClient);
                            Console.WriteLine("Sent to 2");
                        }
                        else if (paddleIndex == 0)
                        {
                            if (paddle1Labeled)
                            {
                                paddleIndex = 2;
                                paddle2Labeled = true;
                                paddle2Pos = newPos;

                                outBuffer = Encoding.ASCII.GetBytes($"{paddle1Pos},{1},{ballPos[0]},{ballPos[1]},{ballDir[0]},{ballDir[1]},{paddle1Dir}");
                                serverUDP.SendTo(outBuffer, 0, outBuffer.Length, SocketFlags.None, remoteClient);
                                Console.WriteLine("Sent to 2");
                            }
                            else
                            {
                                paddleIndex = 1;
                                paddle1Labeled = true;
                                paddle1Pos = newPos;

                                outBuffer = Encoding.ASCII.GetBytes($"{paddle2Pos},{2},{ballPos[0]},{ballPos[1]},{ballDir[0]},{ballDir[1]},{paddle2Dir}");
                                serverUDP.SendTo(outBuffer, 0, outBuffer.Length, SocketFlags.None, remoteClient);
                                Console.WriteLine("Sent to 1");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // Update to include ballPos and ballDir
        static float StringToData(string str, out int paddleIndex, out float ballPosX, out float ballPosY, out float ballDirX, out float ballDirY)
        {
            float pos;

            string[] strings = str.Split(',');

            pos = float.Parse(strings[0]);
            paddleIndex = int.Parse(strings[1]);
            if (paddleIndex != 2)
            {
                ballPosX = float.Parse(strings[2]);
                ballPosY = float.Parse(strings[3]);
                ballDirX = float.Parse(strings[4]);
                ballDirY = float.Parse(strings[5]);

                paddle2Dir = int.Parse(strings[6]);
            }
            else
            {
                ballPosX = ballPos[0];
                ballPosY = ballPos[1];
                ballDirX = ballDir[0];
                ballDirY = ballDir[1];

                paddle1Dir = int.Parse(strings[6]);
            }


            return pos;
        }

        private static void AcceptCallback(IAsyncResult result)
        {
            Socket socket = serverTCP.EndAccept(result);
            if (Player1 == default)
            {
                Player1 = socket;
                Console.WriteLine("Cube 1 connected!!");
            }
            else
            {
                Player2 = socket;
                Console.WriteLine("Cube 2 connected!!");
            }
            socket.BeginReceive(TCPBuffer, 0, TCPBuffer.Length, 0, new AsyncCallback(ReceiveCallback), socket);
        }
        private static void ReceiveCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            int rec = socket.EndReceive(result);
            char[] outputText = new char[rec / 2];

            Buffer.BlockCopy(TCPBuffer, 0, outputText, 0, rec);

            //if (outputText[0] != 0)
            //{
            text = new string(outputText);
            Console.WriteLine("Received: " + text);
            //}
            //else
            //{
            //    outputText = text.ToCharArray();
            //    Console.WriteLine("Not Received");
            //}

            if (socket == Player1)
            {
                Player1.BeginReceive(TCPBuffer, 0, TCPBuffer.Length, 0, new AsyncCallback(ReceiveCallback), Player1);
                Buffer.BlockCopy(TCPBuffer, 0, TCPOutBuffer, 0, rec);
                Console.WriteLine("Sending: \"" + text + "\" to Cube 2");
                Player2.BeginSend(TCPOutBuffer, 0, TCPOutBuffer.Length, 0, new AsyncCallback(SendCallback), Player2);
            }
            else if (socket == Player2)
            {
                Player2.BeginReceive(TCPBuffer, 0, TCPBuffer.Length, 0, new AsyncCallback(ReceiveCallback), Player2);
                Buffer.BlockCopy(TCPBuffer, 0, TCPOutBuffer, 0, rec);
                Console.WriteLine("Sending: \"" + text + "\" to Cube 1");
                Player1.BeginSend(TCPOutBuffer, 0, TCPOutBuffer.Length, 0, new AsyncCallback(SendCallback), Player1);
            }
        }
        private static void SendCallback(IAsyncResult result)
        {
            Socket socket = (Socket)result.AsyncState;
            socket.EndSend(result);
        }
    }
}