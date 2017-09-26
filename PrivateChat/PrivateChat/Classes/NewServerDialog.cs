using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;

using Newtonsoft.Json;
using SQLite;

using PrivateChat.Tables;
using PrivateChat.Adapters;

namespace PrivateChat
{
    class NewServerDialog : DialogFragment
    {
        private readonly Context _context;
        private ListView _list;
        private Server _server;
        private ServerAdapter _adapter;
        private string mode;
        private SQLiteAsyncConnection connection;
        private SocketServiceConnection serviceConnection;

        // constructor
        public NewServerDialog(Context context, ref ListView list, ref ServerAdapter ad, SQLiteAsyncConnection conn)
        {
            _context = context;
            _list = list;
            _server = new Server();
            _adapter = ad;
            mode = "Add";
            connection = conn;

            // Bind to SocketService
            serviceConnection = new SocketServiceConnection((MainActivity)_context);
            Intent service = new Intent(_context, typeof(SocketService));
            _context.BindService(service, serviceConnection, Bind.AutoCreate);
        }

        public NewServerDialog(Context context, ref ListView list, ref ServerAdapter ad, string s, int p)
        {
            _context = context;
            _list = list;
            _adapter = ad;
            _server = _adapter.servers.ElementAt(p);
            mode = s;

            // Bind to SocketService
            serviceConnection = new SocketServiceConnection((MainActivity)_context);
            Intent service = new Intent(_context, typeof(SocketService));
            _context.BindService(service, serviceConnection, Bind.AutoCreate);
        }

        public override Dialog OnCreateDialog(Bundle savedState)
        {
            // Inflate the dialog with a predefined layout in an xml file
            var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);

            // The predefined view for the dialog
            var view = inflater.Inflate(Resource.Layout.NewServerDialog, null);

            // Grab the EditText from the view so I can add a TextWatcher Event.


            // Create an AlertDialog that will contain the UI to display to the user
            var dialog = new AlertDialog.Builder(_context);
            if (mode == "Add")
            {
                dialog.SetTitle("Enter New Server's Information");
                dialog.SetView(view);
                dialog.SetNegativeButton("Cancel", (s, a) => { });
                dialog.SetPositiveButton("Connect", (s, a) => Connect(view));
            }
            else
            {
                EditText name = (EditText)view.FindViewById(Resource.Id.name);
                EditText octet1 = (EditText)view.FindViewById(Resource.Id.ipAddress1);
                EditText octet2 = (EditText)view.FindViewById(Resource.Id.ipAddress2);
                EditText octet3 = (EditText)view.FindViewById(Resource.Id.ipAddress3);
                EditText octet4 = (EditText)view.FindViewById(Resource.Id.ipAddress4);
                EditText port = (EditText)view.FindViewById(Resource.Id.portNumber);

                name.Text = _server.Name;
                string[] octets = _server.IPAddress.Split('.');
                octet1.Text = octets[0];
                octet2.Text = octets[1];
                octet3.Text = octets[2];
                octet4.Text = octets[3];
                port.Text = _server.Port.ToString();

                dialog.SetTitle("Edit The Server's Information");
                dialog.SetView(view);
                dialog.SetNegativeButton("Cancel", (s, a) => { });
                dialog.SetPositiveButton("Update", (s, a) => UpdateServer(view));
            }

            return dialog.Create();
        }

        private void UpdateServer(View view)
        {
            EditText name = (EditText)view.FindViewById(Resource.Id.name);
            EditText octet1 = (EditText)view.FindViewById(Resource.Id.ipAddress1);
            EditText octet2 = (EditText)view.FindViewById(Resource.Id.ipAddress2);
            EditText octet3 = (EditText)view.FindViewById(Resource.Id.ipAddress3);
            EditText octet4 = (EditText)view.FindViewById(Resource.Id.ipAddress4);
            EditText port = (EditText)view.FindViewById(Resource.Id.portNumber);
            //  Check to see if any of the EditTexts were left empty
            if (name.Text == "" || octet1.Text == "" || octet2.Text == "" || octet3.Text == "" || octet4.Text == "" || port.Text == "")
            {
                // Cancel the PositiveButton click; Would need to override the OnClick method for the PositiveButton
                // Set the focus to the first empty EditText; 
                // Let the user know that they left some fo the EditTexts empty and they should try again
                Toast.MakeText(_context, "You left one or more entries empty. Please check again and try again.", ToastLength.Long).Show();
            }
            else if (!validIP(view))
            {
                // Not a valid IP address
                Toast.MakeText(_context, "The IP address entered isn't a valid IP address. Please check again and try again.", ToastLength.Long).Show();
            }
            else
            {
                // All of the entries were entered by the User, i.e. none were left empty

                // Updating the server object

                _server.Name = name.Text;
                _server.IPAddress = octet1.Text + "." + octet2.Text + "." + octet3.Text + "." + octet4.Text;
                // Try to parse the port number provided, The highest port number that is 65535, and the highest value of unsigned Int16 is 65536,
                // TryParse will catch the thrown exception if the user enter a port number that's bigger than 65535.
                short p;
                if (Int16.TryParse(port.Text, out p))
                {
                    // TryParse was successful so set the port number
                    _server.Port = p;
                }

                // Before the server information is updated, need to make sure that we're able to connect to the server with this new information
                if (serviceConnection.Binder.Service.ConnectAndAdd(_server))
                {
                    // The attempt was successful
                    // Remove the old server from the List and replace it with the new instant of the server
                    _adapter.servers.RemoveAt(_server.ID);
                    _adapter.servers.Insert(_server.ID, _server);

                    // Notify the adapter that there were changes made.
                    _adapter.NotifyDataSetChanged();

                    // Update the entry in the database too
                    var qeury = connection.Table<Server>().Where(v => v.Name.Equals(_server.Name));
                    qeury.FirstAsync().ContinueWith(t => { connection.UpdateAsync(t.Result); });
                }
                else
                {
                    // The attempt failed, so inform the user of the failure
                    Toast.MakeText(_context, "Failed to connect to the server with the new information. Please check and try again later.", ToastLength.Long).Show();
                }
            }
        }

        private void Connect(View view)
        {
            EditText name = (EditText)view.FindViewById(Resource.Id.name);
            EditText octet1 = (EditText)view.FindViewById(Resource.Id.ipAddress1);
            EditText octet2 = (EditText)view.FindViewById(Resource.Id.ipAddress2);
            EditText octet3 = (EditText)view.FindViewById(Resource.Id.ipAddress3);
            EditText octet4 = (EditText)view.FindViewById(Resource.Id.ipAddress4);
            EditText port = (EditText)view.FindViewById(Resource.Id.portNumber);

            //  Check to see if any of the EditTexts were left empty
            if (name.Text == "" || octet1.Text == "" || octet2.Text == "" || octet3.Text == "" || octet4.Text == "" || port.Text == "")
            {
                // Cancel the PositiveButton click; Would need to override the OnClick method for the PositiveButton
                // Set the focus to the first empty EditText; 
                // Let the user know that they left some fo the EditTexts empty and they should try again
                Toast.MakeText(_context, "You left one or more entries empty. Please check again and try again.", ToastLength.Long).Show();
            }
            else if (!validIP(view))
            {
                // Not a valid IP address
                Toast.MakeText(_context, "The IP address entered isn't a valid IP address. Please check again and try again.", ToastLength.Long).Show();
            }
            else if (!UniqueServerName(name.Text))
            {
                // Not a unique name for the server
                Toast.MakeText(_context, "The Name entered is already in use. Please try again.", ToastLength.Long).Show();
            }
            else
            {
                // All of the entries were entered by the User, i.e. none were left empty

                // Populate the _server object using the input from the user
                _server.ID = _adapter.servers.Count;    // The ID of the newly added server is the current count of the list of servers.
                _server.Name = name.Text;
                _server.IPAddress = octet1.Text + "." + octet2.Text + "." + octet3.Text + "." + octet4.Text;

                // Remove moving on, going to make sure that the server ID doesn't match any of the already added servers
                foreach (var server in _adapter.servers)
                {
                    if (_server.ID == server.ID)
                    {
                        // Let's increment the ID by one
                        _server.ID += 1;
                    }
                }
                // Try to parse the port number provided, The highest port number that is 65535, and the highest value of unsigned Int16 is 65536,
                // TryParse will catch the thrown exception if the user enter a port number that's bigger than 65535.
                short p;
                if (Int16.TryParse(port.Text, out p))
                {
                    // TryParse was successful so set the port number
                    _server.Port = p;
                }

                // Check to see if we can connect to the server with the provided information
                if (serviceConnection.Binder.Service.ConnectAndAdd(_server))
                {
                    // Was able to connect to the server, so continue doing the necessary work

                    // Add the server to the ListView
                    _adapter.servers.Add(_server);
                    // Notify the adapter that there were changes made.
                    _adapter.NotifyDataSetChanged();

                    // UPDATE: don't need the following code anymore as I switch to using Server class completely, instead of the server struct 
                    //Server testServer = new Server();
                    //testServer.Name = name.Text;
                    //testServer.IPAddress = octet1.Text + "." + octet2.Text + "." + octet3.Text + "." + octet4.Text;
                    //testServer.Port = p;

                    try
                    {
                        // Adding the server to the database
                        connection.InsertAsync(_server);
                        Toast.MakeText(_context, "Added a Server to the database", ToastLength.Long).Show();

                        // Need to update the Server ID of the server in the connections list so that it matches with it's ID within the database
                        // Lets grab the server from the database so that we can extract it's ID
                        var query = connection.Table<Server>().Where(v => v.Name.Equals(_server.Name) && v.IPAddress.Equals(_server.IPAddress)); // Should return only 1 server if found as each name is unique
                        query.FirstAsync().ContinueWith(t => { serviceConnection.Binder.Service.UpdateServerID(t.Result); }).Wait();
                    }
                    catch (SQLiteException ex)
                    {
                        Toast.MakeText(_context, "There was an exception " + ex, ToastLength.Long).Show();
                    }
                }
                else
                {
                    // Wasn't able to connect to the server, meaning that the provided information isn't correct or the server is down
                    // Inform the User that attempt was a failure, and they should check and try again later
                    Toast.MakeText(_context, "Wasn't able to connect to the server with the provided information. Please check information and try again later.", ToastLength.Long).Show();
                }
            }
        }

        // This function will take view of the dialog, and from there it will check if the provided
        // IP Address is valid or not.
        public bool validIP(View view)
        {
            EditText octet1 = (EditText)view.FindViewById(Resource.Id.ipAddress1);
            EditText octet2 = (EditText)view.FindViewById(Resource.Id.ipAddress2);
            EditText octet3 = (EditText)view.FindViewById(Resource.Id.ipAddress3);
            EditText octet4 = (EditText)view.FindViewById(Resource.Id.ipAddress4);

            if (ValidIPOctet(octet1.Text) && ValidIPOctet(octet2.Text) && ValidIPOctet(octet3.Text) && ValidIPOctet(octet4.Text))
            {
                // All 4 Octets are valid
                return true;
            }
            // Not all are valid octets
            return false;
        }

        // This function will check if the IP Octet is within valid range, 0-255
        private bool ValidIPOctet(string s)
        {
            short octet = Int16.Parse(s);
            if (octet >= 0 && octet <= 255)
            {
                // valid octet
                return true;
            }
            return false;
        }

        // Check to see if the provided server name is unique
        private bool UniqueServerName(string s)
        {
            foreach (var ser in _adapter.servers)
            {
                if (ser.Name == s)
                {
                    return false;
                }
            }
            return true;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Unbind from the service
            _context.UnbindService(serviceConnection);
        }
    }
}