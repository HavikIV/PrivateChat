using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Newtonsoft.Json;
using SQLite;

using PrivateChat.Tables;

namespace PrivateChat.Adapters
{
    [Serializable]
    public struct server
    {
        public int ID { get; set; }
        public string name { get; set; }
        public string ipAddress { get; set; }
        public int port { get; set; }
        public server(int id, string n, string ip, int p) { ID = id;  name = n; ipAddress = ip; port = p; }
    }

    public class ServerAdapter : BaseAdapter
    {
        public List<Server> servers;
        Activity activity;
        SQLiteAsyncConnection conn;

        public ServerAdapter (Activity a, SQLiteAsyncConnection c)
        {
            activity = a;
            servers = new List<Server>();
            conn = c;
            LoadServers();
        }

        // This function will load any previously added servers into the List of servers
        void LoadServers()
        {
            // Load previously added servers from the database
            var query = conn.Table<Server>();
            query.ToListAsync().ContinueWith(t => {
                foreach (var ser in t.Result)
                {
                    Server s = new Server();
                    s.Name = ser.Name;
                    s.IPAddress = ser.IPAddress;
                    s.Port = ser.Port;
                    s.ID = ser.ID;
                    servers.Add(s);
                    //NotifyDataSetChanged();
                }
            }).Wait(); // Wait for all of the servers from the database to be loaded into servers list.
            NotifyDataSetChanged();
            //ISharedPreferences prefs = activity.GetSharedPreferences("Privatechat.PrivateChat", FileCreationMode.Private);

            //string items = prefs.GetString("Servers", "default_value");

            //if (items != "default_value")
            //{
            //    // One or more server was added previously
            //    servers = JsonConvert.DeserializeObject<List<server>>(items);
            //    this.NotifyDataSetChanged();
            //}
        }

        // This function is called whenever we need to save the servers in the ListView to the application's SharedPreferences
        // using the key "Servers"
        private void SaveServers()
        {
            ISharedPreferences prefs = activity.GetSharedPreferences("com.Privatechat.PrivateChat", FileCreationMode.Private);
            ISharedPreferencesEditor editor = prefs.Edit();

            // Serialize the items in the list of servers into a string
            string items = JsonConvert.SerializeObject(servers);

            // Save the items to SharedPerferences
            editor.PutString("Servers", items);
            editor.Apply();
        }

        public override int Count
        {
            get { return servers.Count; }
        }

        // Overriding the NotifyDataSetChanged function so that everytime the data set is changed,
        // the App will save the changes into the SharedPreferences to make them permanent
        public override void NotifyDataSetChanged()
        {
            base.NotifyDataSetChanged();
            //SaveServers();
        }

        public override Java.Lang.Object GetItem(int position)
        {
            // could wrap a Contact in a Java.Lang.Object
            // to return it here if needed
            return null;
        }

        public override long GetItemId(int position)
        {
            return servers[position].ID;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? activity.LayoutInflater.Inflate(
                Resource.Layout.ServerListItem, parent, false);
            var serverName = view.FindViewById<TextView>(Resource.Id.itemServerName);
            var serverIP = view.FindViewById<TextView>(Resource.Id.itemIPAddress);
            var serverPort = view.FindViewById<TextView>(Resource.Id.itemPortNumber);
            serverName.Text = servers[position].Name;
            serverIP.Text = servers[position].IPAddress;
            serverPort.Text = servers[position].Port.ToString();
            
            return view;
        }
    }
}