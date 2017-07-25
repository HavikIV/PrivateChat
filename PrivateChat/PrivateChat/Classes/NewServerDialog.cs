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
        private server _server;
        private ServerAdapter _adapter;
        private string mode;
        private SQLiteAsyncConnection connection;

        // constructor
        public NewServerDialog(Context context, ref ListView list, ref ServerAdapter ad, SQLiteAsyncConnection conn)
        {
            _context = context;
            _list = list;
            _server = new server();
            _adapter = ad;
            mode = "Add";
            connection = conn;
        }

        public NewServerDialog(Context context, ref ListView list, ref ServerAdapter ad, string s, int p)
        {
            _context = context;
            _list = list;
            _adapter = ad;
            _server = _adapter.servers.ElementAt(p);
            mode = s;
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
                dialog.SetTitle("New Server");
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

                name.Text = _server.name;
                string[] octets = _server.ipAddress.Split('.');
                octet1.Text = octets[0];
                octet2.Text = octets[1];
                octet3.Text = octets[2];
                octet4.Text = octets[3];
                port.Text = _server.port.ToString();

                dialog.SetTitle("Edit Server");
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

                _server.name = name.Text;
                _server.ipAddress = octet1.Text + "." + octet2.Text + "." + octet3.Text + "." + octet4.Text;
                // Try to parse the port number provided, The highest port number that is 65535, and the highest value of unsigned Int16 is 65536,
                // TryParse will catch the thrown exception if the user enter a port number that's bigger than 65535.
                short p;
                if (Int16.TryParse(port.Text, out p))
                {
                    // TryParse was successful so set the port number
                    _server.port = p;
                }

                // Remove the old server from the List and replace it with the new instant of the server
                _adapter.servers.RemoveAt(_server.ID);
                _adapter.servers.Insert(_server.ID, _server);

                // Notify the adapter that there were changes made.
                _adapter.NotifyDataSetChanged();
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
            else
            {
                // All of the entries were entered by the User, i.e. none were left empty

                // Populate the _server object using the input from the user
                _server.ID = _adapter.servers.Count;    // The ID of the newly added server is the current count of the list of servers.
                _server.name = name.Text;
                _server.ipAddress = octet1.Text + "." + octet2.Text + "." + octet3.Text + "." + octet4.Text;
                // Try to parse the port number provided, The highest port number that is 65535, and the highest value of unsigned Int16 is 65536,
                // TryParse will catch the thrown exception if the user enter a port number that's bigger than 65535.
                short p;
                if (Int16.TryParse(port.Text, out p))
                {
                    // TryParse was successful so set the port number
                    _server.port = p;
                }

                // Add the server to the ListView
                _adapter.servers.Add(_server);
                // Notify the adapter that there were changes made.
                _adapter.NotifyDataSetChanged();

                // Adding the server to the database
                Server testServer = new Server();
                testServer.Name = name.Text;
                testServer.IPAddress = octet1.Text + "." + octet2.Text + "." + octet3.Text + "." + octet4.Text;
                testServer.Port = p;

                try
                {
                    connection.InsertAsync(testServer);
                    Toast.MakeText(_context, "Added a Server to the database", ToastLength.Long).Show();
                }
                catch (SQLiteException ex)
                {
                    Toast.MakeText(_context, "There was an exception " + ex, ToastLength.Long).Show();
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
    }
}