using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

namespace PrivateChat.client
{
    public class client
    {
        //  Used to pass state information to delegate
        class StateObject
        {
            internal byte[] sendBuffer;
            internal Socket socket;
            internal StateObject(int size, Socket sock)
            {
                sendBuffer = new byte[size];
                socket = sock;
            }
        }

        public static void connectCallback(IAsyncResult asyncConnect)
        {
            Socket clientSocket = (Socket)asyncConnect.AsyncState;
            clientSocket.EndConnect(asyncConnect);

            if (clientSocket.Connected == false)
            {
                //  Connection failed; print it to the screen
            }
            else
            {
                //  Connection was successful; print it to the screen
            }

            byte[] sendBuffer = Encoding.ASCII.GetBytes("000-000-0000 Hello Bitch");
            IAsyncResult asyncSend = clientSocket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, new AsyncCallback(sendCallback), clientSocket);

            //  Print to the screen that we're sending a message to the server.

            writeDot(asyncSend);
        }

        public static void sendCallback(IAsyncResult asyncSend)
        {
            Socket clientSocket = (Socket)asyncSend.AsyncState;
            int byteSent = clientSocket.EndSend(asyncSend);
            //  Print to the screen the number of bytes sent to the server

            StateObject stateObject = new StateObject(16, clientSocket);

            IAsyncResult asyncReceive = clientSocket.BeginReceive(stateObject.sendBuffer, 0, stateObject.sendBuffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), stateObject);

            //  Print to the screen that we're receiving a message now.

            writeDot(asyncReceive);
        }

        public static void receiveCallback(IAsyncResult asyncReceive)
        {
            StateObject stateObject = (StateObject)asyncReceive.AsyncState;
            int bytesReceived = stateObject.socket.EndReceive(asyncReceive);
            //  Print the number of bytes we've received from the server
            string TAG = typeof(client).FullName;
            Log.Debug(TAG, "The number of bytes received from the server " + bytesReceived);
            Log.Debug(TAG, "The message received is: " + Encoding.Default.GetString(stateObject.sendBuffer).TrimEnd('\0'));

            //  Shutting down both the receiving and send portions of the data streams
            //stateObject.socket.Shutdown(SocketShutdown.Both);
            //  Closing the socket
            //stateObject.socket.Close();
        }

        public static bool writeDot(IAsyncResult ar)
        {
            int i = 0;
            while (ar.IsCompleted == false)
            {
                if (i++ > 20)
                {
                    //  Timed out after 2 seconds; print it to the screen

                    return false;
                }
                //  Write a dot to the screen; maybe
                System.Threading.Thread.Sleep(100);
            }
            return true;
        }
    }
}