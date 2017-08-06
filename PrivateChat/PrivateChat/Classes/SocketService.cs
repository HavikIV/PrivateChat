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

using SQLite;
using PrivateChat.Tables;

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
        private SQLiteAsyncConnection connection;


        public IBinder Binder { get; private set; }

        public override void OnCreate()
        {
            base.OnCreate();

            // Create a string containing the path to the location of the database
            // The folder location of the database
            var docFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var databasePath = System.IO.Path.Combine(docFolder, "ServerDatabase.db");

            // Create a connection to the database using the pathway
            try
            {
                connection = new SQLiteAsyncConnection(databasePath);
            }
            catch (SQLiteException ex)
            {
                // There was a problem connecting to the database, so should let the User know that using a toast
                Toast.MakeText(this, "There was a problem connecting to the database. Exception: " + ex.Message, ToastLength.Long).Show();
            }

            
            /* Code to connect to a server using a socket
             * IPAddress ipa = Dns.GetHostEntry("68.189.4.56").AddressList[0];
             * IPEndPoint ipep = new IPEndPoint(ipa, 1986);
             *
             * Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
             * IAsyncResult asyncConnect = s.BeginConnect(ipep, new AsyncCallback(client.client.connectCallback), s);
            */
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

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            // Check if the database contains any servers that this service should connect to

            // Query the database for all Servers
            var query = connection.Table<Server>();
            // Create a List of the Servers and for each Server create a Socket for it and add it to the list of connections if the Socket can successfully connect
            query.ToListAsync().ContinueWith(t =>
            {
                // Make sure that the query wasn't empty
                if (t.Result.Count != 0)
                {
                    foreach (var server in t.Result)
                    {
                        try
                        {
                            // Create an IPAddress variable for the Server's IPAddress info.
                            // Resolve the IP Address into an IPHostEntry container and grab the first IP Address in its AddressList
                            IPAddress ipAddress = Dns.GetHostEntry(server.IPAddress).AddressList[0];
                            // Create an IPEndPoint from the Server's information
                            IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, server.Port);

                            // Create a Socket and attempt to connect to the Server using it
                            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            IAsyncResult asyncConnect = socket.BeginConnect(ipEndpoint, new AsyncCallback(client.client.connectCallback), socket);

                            // Register the user with the Server
                            registerUser(socket);

                            // Add the socket to the list of connections
                            connections.Add(socket);
                        }
                        catch (SocketException ex)
                        {
                            if (ex.ErrorCode == 11001)
                            {
                                // The Host wasn't found
                                Toast.MakeText(this, "The host wasn't found in the DNS server.", ToastLength.Long).Show();
                            }
                            else
                            {
                                Toast.MakeText(this, "Error code: " + ex.ErrorCode, ToastLength.Long).Show();
                            }
                        }
                    }
                }
            }).Wait();

            return base.OnStartCommand(intent, flags, startId);
        }

        public void registerUser(Socket s)
        {
            //  This method is used for registering the User to the server through the provided socket.

            //  Send the server the user's phone number.
            ISharedPreferences prefs = this.GetSharedPreferences("PrivateChat.PrivateChat", FileCreationMode.Private);
            string phoneNumber = prefs.GetString("phone", "default");
            byte[] sendBuffer = Encoding.ASCII.GetBytes(phoneNumber);

            IAsyncResult asyncSend = s.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, new AsyncCallback(client.client.sendCallback), s);

            //  Receive the confirmation from the server that the user was registered.

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