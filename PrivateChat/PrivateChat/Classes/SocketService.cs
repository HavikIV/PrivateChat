using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace PrivateChat
{
    [Service]
    public class SocketService : Service
    {
        //   This service will handle connecting to the server(s).
        //   It will connect to the server and supply the server the phone
        //   number of the user. It will also handle sending any outgoing
        //   messages in the buffer. It will also handle receiving all incoming
        //   messages to the user.

        //  IList of sockets for checking with Select() for any type of activity on the list of sockets.
        private IList<Socket> read, write, error;

        private List<Socket> connections;


        public IBinder Binder { get; private set; }

        public override void OnCreate()
        {
            base.OnCreate();
            IPAddress ipa = Dns.GetHostEntry("68.189.4.56").AddressList[0];
            IPEndPoint ipep = new IPEndPoint(ipa, 1986);

            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IAsyncResult asyncConnect = s.BeginConnect(ipep, new AsyncCallback(client.client.connectCallback), s);

            ////  Check if the list of Servers is empty or not
            //if (true)
            //{
            //    //  The list of servers isn't empty so try to connect to the servers specified by the user
            //    foreach (server in Servers)
            //    {
            //        IPAddress ipa = Dns.Resolve(server.ip).AddressList[0];
            //        IPEndPoint ipep = new IPEndPoint(ipa, server.port);

            //        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //        //  Attempt to connect to the server
            //        IAsyncResult asyncConnect = s.BeginConnect(ipep, new AsyncCallback(client.client.connectCallback), s);

            //        //  Check to see if the attempt was successful or not
            //        if ((s.Poll(1000, SelectMode.SelectRead) && s.Available == 0) || !s.Connected)
            //        {
            //            //  The socket isn't connected meaning that the server information provided isn't correct so notify the user

            //        }
            //        else
            //        {
            //            //  The socket was able to connect to the server so pass the socket to a new thread that will send the user's phone number to the server
            //            //  and once the user is registered to the server, add the socket to the list of connections.

            //            //  Declare a thread
            //            Thread myThread;
            //            //  Create an instance of the thread
            //            myThread = new Thread(() => registerUser(s));
            //            //  Start the thread
            //            myThread.Start();
            //        }
            //    }

            //  Start the infinite loop for checking activity on the connections.
            //}
        }

        public void registerUser(Socket s)
        {
            //  This method will registering the User to the provided server.

            //  Send the server the user's phone number.

            //  Receive the confirmation from the server that the user was registered.

            //  Add the socket to the list of the connections.
            connections.Add(s);
        }

        public override IBinder OnBind(Intent intent)
        {
            return this.Binder;
        }

        public override bool OnUnbind(Intent intent)
        {
            return true;
        }

        public override void OnDestroy()
        {

        }
    }
}