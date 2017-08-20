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
        private List<Socket> read, write, error;

        private List<SocketInfo> connections;
        private SQLiteAsyncConnection connection;

        // Wait handle for prevent execution from moving forward before the asynchronous functions finish executing
        private static ManualResetEvent workDone = new ManualResetEvent(false);

        //  Used to pass state information to delegate
        class SocketInfo
        {
            internal byte[] sendBuffer;
            internal Socket socket;
            internal int ID;
            internal SocketInfo(Socket sock, int id)
            {
                sendBuffer = new byte[512];
                socket = sock;
                ID = id;
            }
        }


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

            // Before doing anything create a new List of Sockets
            connections = new List<SocketInfo>();

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

                            // Is the connection new
                            bool isNew = true; // by default

                            // Make sure it's wasn't added previously first
                            if (connections.Count > 0)
                            {
                                foreach (var c in connections)
                                {
                                    if ((IPEndPoint)c.socket.RemoteEndPoint == ipEndpoint)
                                    {
                                        // Already connected to this server, so no work needs to be done on it
                                        isNew = false;
                                        break;
                                    }
                                }
                            }

                            // If the Server hasn't already been added to the list, add it now
                            if (isNew)
                            {
                                // Create a Socket and attempt to connect to the Server using it
                                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                                SocketInfo si = new SocketInfo(socket, server.ID);
                                IAsyncResult result = socket.BeginConnect(ipEndpoint, new AsyncCallback(Connect), si);

                                // Wait for the connection to be made
                                workDone.WaitOne();

                                // Since the connection was successful, need to register the User's phone number on it
                                registerUser(si);

                                // Add the socket to the list of connections
                                connections.Add(si);
                            }
                        }
                        catch (SocketException ex)
                        {
                            // Create an Notification instead to display the error message
                            if (ex.ErrorCode == 11001)
                            {
                                // The Host wasn't found
                                //Toast.MakeText(this, "The host wasn't found in the DNS server.", ToastLength.Long).Show();
                                Notification.Builder builder = new Notification.Builder(this)
                                    .SetSmallIcon(Resource.Drawable.Icon)
                                    .SetContentTitle("ATTENTION")
                                    .SetContentText("The host wasn't found in the DNS server. Server: " + server.Name);
                                NotificationManager nm = GetSystemService(NotificationService) as NotificationManager;
                                nm.Notify(9000, builder.Build());
                            }
                            else
                            {
                                //Toast.MakeText(this, "Error code: " + ex.ErrorCode, ToastLength.Long).Show();
                                Notification.Builder builder = new Notification.Builder(this)
                                    .SetSmallIcon(Resource.Drawable.Icon)
                                    .SetContentTitle("ATTENTION")
                                    .SetContentText("Error code: " + ex.ErrorCode);
                                NotificationManager nm = GetSystemService(NotificationService) as NotificationManager;
                                nm.Notify(9000, builder.Build());
                            }
                        }
                    }
                }
            }).Wait();

            return StartCommandResult.Sticky;
        }

        private void registerUser(SocketInfo s)
        {
            //  This method is used for registering the User to the server through the provided socket.

            //  Send the server the user's phone number.
            ISharedPreferences prefs = this.GetSharedPreferences("PrivateChat.PrivateChat", FileCreationMode.Private);
            string phoneNumber = prefs.GetString("phone", "default");
            s.sendBuffer = Encoding.ASCII.GetBytes(phoneNumber);

            try
            {
                // Attempt to register the User's phone number with the user
                IAsyncResult asyncSend = s.socket.BeginSend(s.sendBuffer, 0, s.sendBuffer.Length, SocketFlags.None, new AsyncCallback(Send), s);
            }
            catch (Exception ex)
            {
                // Something went wrong while registering the phone number, use a notification to inform the user of the error
                Notification.Builder builder = new Notification.Builder(this)
                    .SetSmallIcon(Resource.Drawable.Icon)
                    .SetContentTitle("ATTENTION")
                    .SetContentText("Error: " + ex);
                NotificationManager nm = GetSystemService(NotificationService) as NotificationManager;
                nm.Notify(9000, builder.Build());
            }

            // Wait for the BeginSend to be done
            workDone.WaitOne();

            //  Receive the confirmation from the server that the user was registered.

        }

        // This function will test to see if it can connect to the provided server
        // if it can, then it will be added to the list of connections and the user will
        // notified of its connection status (failed or connected)
        public bool ConnectAndAdd(Server server)
        {
            // Create an IPAddress variable
            IPAddress ipAddress = Dns.GetHostEntry(server.IPAddress).AddressList[0];
            // Create an IPEndpoint using the IPAddress and the port number
            IPEndPoint ipEndpoint = new IPEndPoint(ipAddress, server.Port);

            // Create an socket from the IPEndpoint
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketInfo si = new SocketInfo(socket, server.ID);
            IAsyncResult result = socket.BeginConnect(ipEndpoint, new AsyncCallback(Connect), si);

            // wait for the Connection to be established or fail
            workDone.WaitOne();

            // Check to see if the attempt was successful or not
            if (si.socket.Connected)
            {
                // Register the user's phone number with the server
                registerUser(si);

                // Make sure it isn't already in the list
                foreach (var c in connections)
                {
                    if ((IPEndPoint)c.socket.RemoteEndPoint == ipEndpoint)
                    {
                        // Replace the old socket with the new socket
                        c.socket = si.socket;
                        return true;
                    }
                }

                // The connection to the list
                connections.Add(si);

                // Since the connection was successful need to inform the caller
                return true;
            }

            return false;   // Returns false by default
        }

        // This function will be passed to a BeginConnect() as an AsyncCallback
        // It will be responsible for connecting to the given server and registering the User's phone number with it.
        // This function should be only be called whenever the service is started or whenever the testing a server.
        public static void Connect(IAsyncResult result)
        {
            // Grab the socket from the IAsyncResult variable's AsyncState
            SocketInfo si = (SocketInfo)result.AsyncState;
           // Socket socket = (Socket)result.AsyncState;

            // Need to end the request of Connecting as it was successful as it reach this point
            si.socket.EndConnect(result);

            // Need to reset the wait handle so the calling function can continue executing
            workDone.Set();
        }

        // This function will be passed to a BeginSend() as an AsyncCallback
        // It will be responsible for sending the provided message to the server
        public static void Send(IAsyncResult result)
        {
            try
            {
                // Grab the socket from the IAsyncResult variable's AsyncState
                SocketInfo si = (SocketInfo)result.AsyncState;
                //Socket socket = (Socket)result.AsyncState;

                // Need to end the send request
                int bytesSent = si.socket.EndSend(result);

                // Check to see if all of the byes were sent
                //if (bytesSent == si.sendBuffer.Length)
                //{
                //    //Toast.MakeText((Context)result.AsyncState, "The entire message was sent out to the server.", ToastLength.Long).Show();
                //}

                // Need to signal that everything was sent out and to resume execution of code
                workDone.Set();
            }
            catch (Exception ex)
            {
                // Something went wrong, so let the user know, I think this work (the context part anyways)
                //Toast.MakeText((Context)result.AsyncState, "Couldn't send out the message, exception: " + ex, ToastLength.Long).Show();
            }
        }

        // This function will be called whenever the user wants to send a message.
        // It will take the message as a string, the list of strings of the phone numbers,
        // and the ID of the server that the message needs to be sent to.
        public void SendMessage(string message, string contacts, int id)
        {
            // Find the needed Socket from the list using the ServerID
            foreach (var c in connections)
            {
                if (c.ID == id)
                {
                    // Found it, lets send out the message

                    // Place the message in the buffer
                    c.sendBuffer = Encoding.ASCII.GetBytes(contacts + message);

                    // Should let just the Select() handle sending out the message
                    IAsyncResult result = c.socket.BeginSend(c.sendBuffer, 0, c.sendBuffer.Length, SocketFlags.None, new AsyncCallback(Send), c);
                }
            }

        }

        // This function will use the Select() method to determine if activity on the sockets,
        // if so, it will do the appropriate work.
        private void HandleSockets()
        {
            // This infinite loop will continue calling the Select() to handle the sockets
            while (true)
            {
                // Add the sockets to the 3 ILists for the Select() method to use
                foreach (var server in connections)
                {
                    read.Add(server.socket);
                    write.Add(server.socket);
                    error.Add(server.socket);
                }

                Socket.Select(read, write, error, 1000);
                
                // Now check the 3 lists to see which sockets are still in them
                // only the ones that have some work that needs done will remain in the lists

                foreach (var s in read)
                {

                }

                foreach (var s in write)
                {

                }

                foreach (var s in error)
                {

                }
            }
        }

        public override IBinder OnBind(Intent intent)
        {
            Binder = new SocketBinder(this);
            return Binder;
        }

        public override bool OnUnbind(Intent intent)
        {
            return true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}