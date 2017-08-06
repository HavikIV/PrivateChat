using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using Android.Views;
using Android.Util;
using Android.Database.Sqlite;

using com.refractored.fab;
using Newtonsoft.Json;
using SQLite;

using PrivateChat.Tables;
using PrivateChat.Adapters;

namespace PrivateChat
{
    [Activity(Label = "PrivateChat", Icon = "@drawable/icon")]
    public class MainActivity : Activity, ActionMode.ICallback
    {
        ServerAdapter adapter;
        SocketServiceConnection serviceConnection;
        ActionMode actionMode;
        int deletePosition = -1;
        SQLiteDatabase ServerDB;
        SQLiteAsyncConnection connection;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Main);

            // Create a path to the Database
            var docFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var pathToDatabase = System.IO.Path.Combine(docFolder, "ServerDatabase.db");

            // Check if the database exists, if does open it else create the database.
            try
            {
                ServerDB = SQLiteDatabase.OpenOrCreateDatabase(pathToDatabase, null);
            }
            catch (Android.Database.Sqlite.SQLiteException ex)
            {
                // Something went wrong trying to Open or Create a Database
                Toast.MakeText(this, "Wasn't able to open or create the database. Error " + ex, ToastLength.Long).Show();
            }

            connect(pathToDatabase);

            // Set up the custom adapter that's going to be used for the ListView
            adapter = new ServerAdapter(this, connection);

            // Set the ListView's adapter to the custom adapter
            ListView list = FindViewById<ListView>(Resource.Id.serverList);
            list.Adapter = adapter;

            // Add a click listener for the items in the ListView
            list.ItemClick += ListItemClick;

            // Add a long click listener to the ListView
            list.LongClickable = true;  // Making sure that items in the ListView can be long clicked
            list.ItemLongClick += ListItemLongClick; 

            // Load previously added servers
            //ISharedPreferences prefs = this.GetSharedPreferences("Privatechat.PrivateChat", FileCreationMode.Private);

            //string items = prefs.GetString("Servers", "default_value");

            //if (items != "default_value")
            //{
            //    // One or more server was added previously
            //    adapter.servers = JsonConvert.DeserializeObject<List<server>>(items);
            //    adapter.NotifyDataSetChanged();
            //}

            // Set what action to take when the FAB is clicked
            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += FabClick;

            //IPAddress ipAddress = Dns.Resolve("97.94.155.89").AddressList[0];
            //IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 1986);    //  Represents an network endpoint as an IP ad

            //Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //IAsyncResult asyncConnect = clientSocket.BeginConnect(ipEndPoint, new AsyncCallback(client.client.connectCallback), clientSocket);
            //Button btn = FindViewById<Button>(Resource.Id.BTN);

            //btn.Click += addIPAddress;

            //Button save = FindViewById<Button>(Resource.Id.savebtn);
            //save.Click += saveToSharedPreferences;

            //Button clear = FindViewById<Button>(Resource.Id.clearbtn);
            //clear.Click += clearSharedPreferences;

            //Button load = FindViewById<Button>(Resource.Id.loadbtn);
            //load.Click += loadFromSharedPreferences;
            //adapter = new serverAdapter(this);
        }

        public void connect(string path)
        {
            // Switched to Wait() from await to avoid an exception from occurring. Before it was possible
            // for the App to move on before a connection to the database was established
            // Check if the database exists, if does open it else create the database.
            try
            {
                connection = new SQLiteAsyncConnection(path);
                connection.CreateTableAsync<Server>().Wait(); // Making sure that connection to the database is established before moving on
                Toast.MakeText(this, "Server table created", ToastLength.Long).Show();
            }
            catch (SQLite.SQLiteException ex)
            {
                // Something went wrong trying to Open or Create a Database
                Toast.MakeText(this, "Wasn't able to connect to the database. Error " + ex, ToastLength.Long).Show();
            }
        }

        // This function is called whenever an item in the ListView is clicked.
        // It will setup and start the conversation activity
        private void ListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Create an intent for starting the conversation activity
            Intent intent = new Intent(this, typeof(ConversationActivity));

            // Pass which server's conversations need to be displayed
            intent.PutExtra("ServerID", adapter.servers.ElementAt(e.Position).ID);

            // Pass the name of the selected server
            intent.PutExtra("ServerName", adapter.servers.ElementAt(e.Position).Name);

            // Start the conversation activity
            StartActivity(intent);
        }

        private void ListItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            // Confirm that the user wants to delete the Server from the list
            //var confirmDialog = new AlertDialog.Builder(this);
            //confirmDialog.SetTitle("Confirm Delete");
            //confirmDialog.SetMessage("Are you sure you wish to delete this server? All data associated with this server will be lost.");
            //confirmDialog.SetNegativeButton("Cancel", (s, a) => { }); // Don't do anything as the user doesn't want to delete the server
            //// If the user is positive that they want to delete the server, then do it
            //confirmDialog.SetPositiveButton("Delete", (s, a) => {
            //    adapter.servers.RemoveAt(e.Position);
            //    adapter.NotifyDataSetChanged();
            //});

            //confirmDialog.Show();
            deletePosition = e.Position;

            // In order to give users more option when the item is long clicked, create a contextual action mode.
            // This mode will allow the user the options to either edit, share, or delete the selected item(s).

            actionMode = StartActionMode(this);
        }

        private void DeleteServer()
        {
            if (deletePosition != -1)
            {
                // Delete from the database
                var ser = adapter.servers.ElementAt(deletePosition);
                var query = connection.Table<Server>().Where(v => v.Name.Equals(ser.Name));

                // Since the Server is being deleted, all of its Conversations and their Messages need to be deleted too
                var conversationQuery = connection.Table<Conversation>().Where(v => v.ServerID.Equals(ser.ID));

                // Delete should happen in the background after it FirstAsync finishes in the background
                query.FirstAsync().ContinueWith(t => { connection.DeleteAsync(t.Result); });
                conversationQuery.ToListAsync().ContinueWith(t => {
                    foreach (var c in t.Result)
                    {
                        // For each of the Conversations, need to delete its Messages
                        // Grab its Messages for deletion
                        var messageQuery = connection.Table<Messages>().Where(v => v.ServerID.Equals(c.ServerID) && v.ConversationID.Equals(c.ID));
                        messageQuery.ToListAsync().ContinueWith(x => {
                            foreach (var m in x.Result)
                            {
                                // Delete the Message from the database
                                connection.DeleteAsync(m);
                            }
                        });

                        // Delete the Conversation from the database
                        connection.DeleteAsync(c);
                    }
                });

                // Delete the server from the adapter
                adapter.servers.RemoveAt(deletePosition);
                adapter.NotifyDataSetChanged();

                deletePosition = -1;
            }
        }

        private void EditServer()
        {
            // Grab the ListView
            ListView list = FindViewById<ListView>(Resource.Id.serverList);
            // Display the dialog
            var dialog = new NewServerDialog(this, ref list, ref adapter, "Edit", deletePosition);
            dialog.Show(FragmentManager, "server");
            deletePosition = -1;

            //
        }

        // When the fab is clicked, this function will display a dialog that will ask
        // the user to input the necessary information needed to connect to a server
        // such as the IP address and port number. It will also ask for the name that
        // the server should be saved under. Once the information is obtained, it should
        // tested and added to the ListView.
        private void FabClick(object sender, EventArgs e)
        {
            // Grab the ListView
            ListView list = FindViewById<ListView>(Resource.Id.serverList);

            // Display the dialog
            var dialog = new NewServerDialog(this, ref list, ref adapter, connection);
            dialog.Show(FragmentManager, "server");
        }

        protected override void OnResume()
        {
            base.OnResume();
        }

        protected override void OnStart()
        {
            base.OnStart();

            //if (serviceConnection == null)
            //{
            //    this.serviceConnection = new SocketServiceConnection(this);
            //}

            //Intent serviceToStart = new Intent(this, typeof(SocketService));
            //BindService(serviceToStart, this.serviceConnection, Bind.AutoCreate);
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.actionDelete:
                    {
                        DeleteServer();
                        mode.Finish();
                        return false;
                    }
                case Resource.Id.actionEdit:
                    {
                        EditServer();
                        mode.Finish();
                        return false;
                    }
            }

            return false;
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            MenuInflater inflater = mode.MenuInflater;
            inflater.Inflate(Resource.Menu.ServerActionBar, menu);
            return true;
        }

        public void OnDestroyActionMode(ActionMode mode)
        {
            base.OnDestroy();
        }

        public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
        {
            return false;
        }

        //private void addIPAddress( object sender, EventArgs e)
        //{
        //    //ListView list = FindViewById<ListView>(Resource.Id.listView1);
        //    //EditText tv = FindViewById<EditText>(Resource.Id.editText1);
        //    server s = new server(tv.Text, 1986, 0);

        //    adapter.serverList.Add(s);
        //    list.Adapter = adapter;
        //    //list.SetAdapter(adapter);

        //}

        //private void saveToSharedPreferences(object sender, EventArgs e)
        //{
        //    ISharedPreferences prefs = this.GetSharedPreferences("PrivateChat.PrivateChat", FileCreationMode.Private);
        //    ISharedPreferencesEditor editor = prefs.Edit();

        //    //  Make a string give everything in the listview
        //    string listString = "";
        //    //ListView list = FindViewById<ListView>(Resource.Id.listView1);

        //    //  Grab each item in the ArrayAdapter and add it to the listString with a space in between each item 
        //    for(int i = 0; i < adapter.Count; i++)
        //    {
        //        if (i < adapter.Count - 1)
        //        {
        //            listString += adapter.GetItem(i) + " ";
        //        }
        //        else
        //        {
        //            listString += adapter.GetItem(i);
        //        }
        //    }

        //    // Add and save to the SharedPrferences
        //    editor.PutString("IPAddresses", listString);
        //    editor.Apply();
        //}

        //private void clearSharedPreferences(object sender, EventArgs e)
        //{
        //    ISharedPreferences prefs = this.GetSharedPreferences("PrivateChat.PrivateChat", FileCreationMode.Private);
        //    ISharedPreferencesEditor editor = prefs.Edit();

        //    editor.Remove("IPAddresses");
        //    editor.Apply();

        //    //ListView list = FindViewById<ListView>(Resource.Id.listView2);
        //    list.Adapter = null;
        //    //list.SetAdapter(null);
        //}

        //private void loadFromSharedPreferences(object sender, EventArgs e)
        //{
        //    ISharedPreferences prefs = this.GetSharedPreferences("PrivateChat.PrivateChat", FileCreationMode.Private);

        //    //  load the items from the SharedPerferences
        //    string listString = prefs.GetString("IPAddresses", "default_value");
        //    var s = listString.Split(' ');

        //    serverAdapter ad = new serverAdapter(this);

        //    //ListView list = FindViewById<ListView>(Resource.Id.listView2);
        //    list.Adapter = ad;
        //    //list.SetAdapter(ad);
        //}
    }
}

