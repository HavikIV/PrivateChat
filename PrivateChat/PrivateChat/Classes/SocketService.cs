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
        private bool shouldWaitForAdd = false;
        private bool busyReading = false;
        private bool restarting = false;
        private Thread thread;

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
                //Toast.MakeText(this, "There was a problem connecting to the database. Exception: " + ex.Message, ToastLength.Long).Show();
                DisplayNotification(9005, "DatabaseException", "There was a problem connecting to the database. Exception: " + ex.Message, null);
            }

            // Before doing anything create a new List of Sockets
            connections = new List<SocketInfo>();

            read = new List<Socket>();
            error = new List<Socket>();

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

            // Lets make sure that there isn't already a worker thread started
            if (thread != null)
            {
                // Inform the worker thread that the service was restarted so it needs to close
                restarting = true;

                // Lets give the thread enough time to end
                Thread.Sleep(1000); // Sleep for 1 second

                // Close the previously started thread
                thread.Abort();

                restarting = false; // Reset the boolean variable
            }

            // Check if the database contains any servers that this service should connect to
            // Query the database for all Servers
            var query = connection.Table<Server>();
            // Create a List of the Servers and for each Server create a Socket for it and add it to the list of connections if the Socket can successfully connect
            query.ToListAsync().ContinueWith(t =>
            {
                try
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

                                shouldWaitForAdd = true;

                                // Lets make sure the HandleSocket's Thread isn't busy trying to read at the moment
                                while (busyReading)
                                {
                                    // Wait for a millisecond
                                    Thread.Sleep(100);
                                }

                                // Make sure it's wasn't added previously first
                                if (connections.Count > 0)
                                {
                                    foreach (var c in connections)
                                    {
                                        IPEndPoint ep = (IPEndPoint)c.socket.RemoteEndPoint;
                                        if (ep.Address.Equals(ipEndpoint.Address) && ep.Port == ipEndpoint.Port)
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

                                shouldWaitForAdd = false;
                            }
                            catch (SocketException ex)
                            {
                                // Create an Notification instead to display the error message
                                if (ex.ErrorCode == 11001)
                                {
                                    // The Host wasn't found
                                    //Toast.MakeText(this, "The host wasn't found in the DNS server.", ToastLength.Long).Show();
                                    DisplayNotification(9000, "ATTENTION", "The host wasn't found in the DNS server. Server: " + server.Name, null);
                                }
                                else
                                {
                                    //Toast.MakeText(this, "Error code: " + ex.ErrorCode, ToastLength.Long).Show();
                                    DisplayNotification(9001, "ATTENTION", "Error code: " + ex.ErrorCode, null);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Notification.Builder builder = new Notification.Builder(this)
                    //                    .SetSmallIcon(Resource.Drawable.Icon)
                    //                    .SetContentTitle("NO SERVER TABLE FOUND")
                    //                    .SetContentText("Need to create a Table first");
                    //NotificationManager nm = GetSystemService(NotificationService) as NotificationManager;
                    //nm.Notify(9000, builder.Build());
                }
            }).Wait();

            // Only start a new thread if it hasn't been started before
            if (thread == null)
            {
                // Start a separate thread handle reading of all incoming messages
                thread = new Thread(new ThreadStart(HandleSockets));

                // Start the thread
                thread.Start();
            }

            return StartCommandResult.Sticky;
        }

        private void registerUser(SocketInfo s)
        {
            //  This method is used for registering the User to the server through the provided socket.

            //  Send the server the user's phone number.
            ISharedPreferences prefs = this.GetSharedPreferences("PrivateChat.PrivateChat", FileCreationMode.Private);
            string phoneNumber = prefs.GetString("phone", "default");
            string fullname = prefs.GetString("name", "default");
            s.sendBuffer = Encoding.ASCII.GetBytes(phoneNumber + " " + fullname);

            try
            {
                // Attempt to register the User's phone number with the user
                IAsyncResult asyncSend = s.socket.BeginSend(s.sendBuffer, 0, s.sendBuffer.Length, SocketFlags.None, new AsyncCallback(Send), s);
            }
            catch (Exception ex)
            {
                // Something went wrong while registering the phone number, use a notification to inform the user of the error
                DisplayNotification(9002, "ATTENTION", "Error: " + ex.Message, null);
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
            // Make sure that the thread handling the sockets doesn't try to read from any of the sockets before we finish adding this one
            shouldWaitForAdd = true;

            //  Check to make sure that the socket reading thread isn't busy reading at the moment, if it is, need to wait for it to finish
            while (busyReading)
            {
                // wait for a millisecond before checking again, the boolean variable shouldWaitForAdd should prevent
                // the HandleSocket thread from hogging the CPU and allowing this function to finish before it can try reading again
                Thread.Sleep(100);
            }

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
                    IPEndPoint ep = (IPEndPoint)c.socket.RemoteEndPoint;
                    if (ep.Address.Equals(ipEndpoint.Address) && ep.Port == ipEndpoint.Port)
                    {
                        // Replace the old socket with the new socket
                        c.socket = si.socket;
                        return true;
                    }
                }

                // The connection to the list
                connections.Add(si);

                shouldWaitForAdd = false;
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

                // Check to see if all of the bytes were sent
                if (bytesSent == si.sendBuffer.Length)
                {
                    // Lets clear the buffer
                    si.sendBuffer = new byte[512];
                }

                // Need to signal that everything was sent out and to resume execution of code
                workDone.Set();
            }
            catch (Exception ex)
            {
                // Something went wrong, so let the user know, I think this work (the context part anyways)
                //Toast.MakeText((Context)result.AsyncState, "Couldn't send out the message, exception: " + ex, ToastLength.Long).Show();
            }
        }

        public static void Read(IAsyncResult result)
        {
            try
            {
                // Grab the SocketInfo from the IAsyncResult variable's AsyncState
                SocketInfo si = (SocketInfo)result.AsyncState;
                // Grab the socket from the IAsyncResult variable's AsyncState
                Socket s = (Socket)result.AsyncState;

                // Need to end the read request
                int bytes = s.EndReceive(result);

                // Check to make sure the message was completely received
                if (bytes > 0)
                {
                    // Lets try to receive more data and append it into the buffer offset by the number of bytes previously received
                    s.BeginReceive(si.sendBuffer, bytes, si.sendBuffer.Length, 0, new AsyncCallback(Read), si);
                }
                else
                {
                    // Need to signal that everything was read and to resume execution of code
                    workDone.Set();
                }
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

                    // Lets determine how many recipients to send the message to
                    string temp = contacts; // copy of the contacts string
                    temp.Replace(" ", "");  // remove all spaces from the string of contacts
                    int recipients = temp.Length / 10;  // number of recipients

                    ISharedPreferences prefs = this.GetSharedPreferences("PrivateChat.PrivateChat", FileCreationMode.Private);
                    string phoneNumber = prefs.GetString("phone", "default");   // The user's phone number

                    // Place the message in the buffer (<recipients> [phoneNumber(s)] message)
                    c.sendBuffer = Encoding.ASCII.GetBytes(recipients + " " + contacts + " " + phoneNumber + " " + message);

                    // Should let just the Select() handle sending out the message
                    IAsyncResult result = c.socket.BeginSend(c.sendBuffer, 0, c.sendBuffer.Length, SocketFlags.None, new AsyncCallback(Send), c);

                    // Wait for the message to finish being sent out
                    workDone.WaitOne();
                }
            }

        }

        // This function will use the Select() method to determine if activity on the sockets,
        // if so, it will do the appropriate work.
        private void HandleSockets()
        {
            ISharedPreferences prefs = this.GetSharedPreferences("PrivateChat.PrivateChat", FileCreationMode.Private);

            // This infinite loop will continue calling the Select() to handle the sockets
            while (!restarting)
            {
                // Lets grab the user's phone number
                string phoneNumber = prefs.GetString("phone", "default");   // The user's phone number

                if (connections.Count != 0 && !shouldWaitForAdd)
                {
                    busyReading = true;

                    // Add the sockets to the 3 ILists for the Select() method to use
                    foreach (var server in connections)
                    {
                        if (server.socket != null)
                        {
                            read.Add(server.socket);
                            //write.Add(server.socket);
                            error.Add(server.socket);
                        }
                    }

                    if ( read.Count != 0)
                    {
                        try
                        {
                            Socket.Select(read, null, error, 10000); // timeout is 10 milliseconds

                            // Now check the 3 lists to see which sockets are still in them
                            // only the ones that have some work that needs done will remain in the lists

                            foreach (var s in error)
                            {
                                // Close the connection
                                s.Shutdown(SocketShutdown.Both);
                                s.Close();
                            }

                            foreach (var s in read)
                            {
                                //byte[] buffer = new byte[512]; // empty buffer

                                // Lets grab the server info from the connections list so to pass it to the read callback method and need to save the message in the database
                                var server = FindServer(s);

                                // Read the incoming message, and pass the SocketInfo for the read callback method
                                IAsyncResult result = s.BeginReceive(server.sendBuffer, 0, server.sendBuffer.Length, 0, new AsyncCallback(Read), server);

                                // Wait the message to be finished being read
                                workDone.WaitOne();

                                int recipients = server.sendBuffer[0] - 48; // '0' - 48 should give the integer value

                                // Check to make sure we received something
                                if (recipients > 0)
                                {
                                    // We received a message so lets do some work

                                    int arraySize = 10 * (recipients + 1) + recipients; // Size is the number of recipients of the message, plus the spaces between them, and the sender's phone number
                                    byte[] numbers = new byte[arraySize];
                                    Buffer.BlockCopy(server.sendBuffer, 2, numbers, 0, numbers.Length);

                                    string groupName = System.Text.Encoding.Default.GetString(numbers); // Convert the array of bytes to a string of phone numbers

                                    groupName = RemovePhoneNumber(phoneNumber, groupName); // remove the user's phone number, it's possible that it may create a double space so will need to remove it next
                                    //groupName.Replace("  ", " "); // Replace any double space with a single space

                                    Buffer.BlockCopy(server.sendBuffer, numbers.Length + 2, server.sendBuffer, 0, server.sendBuffer.Length - (numbers.Length + 2)); // Remove everything but the message from the buffer

                                    int ConversationID = -1;
                                    var ts = DateTime.Now;

                                    // Lets try to find the Conversation that corresponds with the message's groupName
                                    var queryCount = connection.Table<Conversation>().Where(v => v.ServerID.Equals(server.ID) && v.GroupName.Equals(groupName)).CountAsync();
                                    if (queryCount.Result != 0)
                                    {
                                        var query = connection.Table<Conversation>().Where(v => v.ServerID.Equals(server.ID) && v.GroupName.Equals(groupName));
                                        query.FirstAsync().ContinueWith(t => {
                                            var conv = t.Result;
                                            conv.LastMessage = System.Text.Encoding.Default.GetString(server.sendBuffer);
                                            conv.LastTimeStamp = ts.ToString();
                                            conv.TotalMessages += 1;
                                            connection.UpdateAsync(conv);
                                            ConversationID = conv.ID;
                                        }).Wait();
                                    }
                                    else
                                    {
                                        // It's a new Conversation
                                        var conversation = new Conversation();
                                        conversation.ServerID = server.ID;
                                        conversation.GroupName = groupName;
                                        conversation.TotalMessages += 1;
                                        conversation.LastMessage = System.Text.Encoding.Default.GetString(server.sendBuffer);
                                        conversation.LastTimeStamp = ts.ToString();

                                        // Add Conversation to the database an update the ConversationID to the newly added Conversation
                                        connection.InsertAsync(conversation).ContinueWith(t => {
                                            var query = connection.Table<Conversation>().Where(v => v.ServerID.Equals(server.ID) && v.GroupName.Equals(conversation.GroupName));
                                            query.FirstAsync().ContinueWith(b => { ConversationID = b.Result.ID; }).Wait(); // wait for the COnversationID to be updated
                                        }).Wait(); // Need to for these operations to finish before moving on as the do need the ConversationID in the step
                                    }

                                    // Lets add the message the Messages Table
                                    var mes = new Messages();
                                    mes.Message = System.Text.Encoding.Default.GetString(server.sendBuffer);
                                    mes.Owner = groupName;
                                    mes.TimeStamp = ts.ToString();
                                    mes.ServerID = server.ID;
                                    mes.ConversationID = ConversationID; // Get this from the work above

                                    // Add the new Message to the database
                                    connection.InsertAsync(mes);

                                    // Let's send a broadcast to tell the MessageActivity (if it's open) to reload the adapter as a new message was added
                                    Intent intent = new Intent("PrivateChat.PrivateChat");
                                    intent.SetAction(MessageActivity.ReloadAdapter); // Set the action to reload the messages into the adapter
                                    SendBroadcast(intent);  // Send the broadcast


                                    // *** THE FOLLOWING CODE TO OPEN AN ACTIVITY FROM AN NOTIFICATION WAS PROVIDED BY LUISRODRIGUEZ92 FROM XAMARIN FORUMS ***

                                    // Intent to open the MessageActivity when the notification is clicked
                                    Intent sIntent = new Intent(this, typeof(MessageActivity));
                                    // Pass the necessary information for the activity load the appropriate messages
                                    sIntent.PutExtra("ServerID", mes.ServerID);
                                    sIntent.PutExtra("ConversationID", mes.ConversationID);
                                    sIntent.PutExtra("PhoneNumber", groupName);

                                    // Create a Task Stack builder to handle the back stack
                                    TaskStackBuilder stackBuilder = TaskStackBuilder.Create(this);

                                    // Add all parents of MessageActivity to the stack
                                    stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MessageActivity)));

                                    // Push the intent that starts the MessageActivity onto the stack
                                    stackBuilder.AddNextIntent(sIntent);

                                    // Obtain the PendingIntent for launching the task constructed by stack builder. The pending intent can be used only once.
                                    const int pendingIntentId = 0;
                                    PendingIntent pendingIntent = stackBuilder.GetPendingIntent(pendingIntentId, PendingIntentFlags.UpdateCurrent);

                                    // Send a notification that a new message was received
                                    DisplayNotification(9100, groupName, mes.Message, pendingIntent);

                                }
                            }
                        }
                        catch (ArgumentNullException ex)
                        {
                            if (read.Count == 0 && error.Count == 0)
                            {
                                // Then this exception makes sense as it means that Select was given three null or empty lists
                                // Otherwise this exception was thrown for a reason that I do not know.
                                DisplayNotification(9003, "ArgumentNullExpection", ex.Message, null);
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            DisplayNotification(9004, "InvalidOperationException", ex.Message, null);
                        }
                        catch (SocketException ex)
                        {
                            if (ex.ErrorCode == 10022)
                            {
                               // Let's ignore this for now, though it seems that one of the sockets that was added into the lists was deleted and resulted in an Invalid Argument exception
                            }
                        }
                    }

                    busyReading = false;
                }

                // If there are no connections in the list then put the thread to sleep for 10 seconds and then check again
                // in order to avoid wasting CPU cycles doing nothing
                while (connections.Count == 0)
                {
                    // In case a socket was left in either the read or error list when the server was deleted
                    // let's remove them from the lists before sleeping
                    if (read.Count != 0 || error.Count != 0)
                    {
                        // Make sure that the sockets are properly closed first
                        foreach (var s in error)
                        {
                            // Close the connection
                            s.Shutdown(SocketShutdown.Both);
                            s.Close();
                        }

                        foreach (var s in read)
                        {
                            // Close the connection
                            s.Shutdown(SocketShutdown.Both);
                            s.Close();
                        }

                        // Remove the sockets from the lists
                        read.Clear();
                        error.Clear();
                    }
                    Thread.Sleep(10000); // Sleep for 10 seconds
                }
            }

            restarting = false;
        }

        //  Close the socket that connects to the server with the provided server ID
        public void CloseSocket(int id)
        {
            foreach (var server in connections)
            {
                if (server.ID == id)
                {
                    //  Found the server, so lets close it after shutting down the stream first
                    server.socket.Shutdown(SocketShutdown.Both);
                    server.socket.Close();

                    // Now lets remove it from the list of connections so that the HandleSocket() doesn't try to do anything stupid with a closed socket
                    connections.Remove(server);
                    break;  // Lets end the loop here as don't need to search rest of the list
                }
            }
        }

        // Removes all instances of the given phone number in the old string
        // Returns a news string without the given phone number
        // Created this method as the String.Replace(string old, string new) method wasn't working
        public string RemovePhoneNumber(string phoneNumber, string old)
        {
            string newVal = "";
            int i = 0;
            while (i < old.Length)
            {
                string temp = "";
                while (i == 0 || temp.Length != 10)
                {
                    temp += old[i];
                    i++;
                }

                if (temp != phoneNumber)
                {
                    newVal += temp;
                }

                if (newVal.Length != 0 && i < old.Length)
                {
                    newVal += old[i];
                }

                i++;
            }

            return newVal;
        }

        public void UpdateServerID(Server server)
        {
            foreach (var s in connections)
            {
                IPEndPoint ep = (IPEndPoint)s.socket.RemoteEndPoint;
                IPAddress ipAddress = Dns.GetHostEntry(server.IPAddress).AddressList[0];
                IPEndPoint ip = new IPEndPoint(ipAddress, server.Port);
                if (ep.Address.Equals(ipAddress) && ep.Port == server.Port)
                {
                    //  Found the server, so lets close it
                    s.ID = server.ID;
                    break;  // Lets end the loop here as don't need to search rest of the list
                }
            }
        }

        //  Find the server object that has the provided socket
        SocketInfo FindServer(Socket s)
        {
            foreach (var server in connections)
            {
                if (server.socket == s)
                {
                    return server;
                }
            }

            // Didn't find a server associated with the socket, so lets return an empty SocketInfo object
            return new SocketInfo(null, -1);
        }

        // Display an error notification
        // This method requires the Notification ID, title of the error,
        // the full error message, and the action that should be done
        private void DisplayNotification(int id, string title, string message, PendingIntent pendingIntent)
        {
            // Building a notification
            Notification.Builder builder = new Notification.Builder(this)
                .SetSmallIcon(Resource.Drawable.Icon)
                .SetColor(0x010203)
                .SetContentIntent(pendingIntent)
                .SetContentTitle(title)
                .SetContentText(message)
                .SetAutoCancel(true);

            // Get an instance of the notification manager so that the notification can be display
            NotificationManager nm = GetSystemService(NotificationService) as NotificationManager;

            // Issue the notification
            nm.Notify(id, builder.Build());
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