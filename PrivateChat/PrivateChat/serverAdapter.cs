using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Provider;
using Android.Views;
using Android.Widget;

namespace PrivateChat
{
    [Serializable]
    public struct server
    {
        public string ipAddress { get; set; }
        public int port { get; set; }
        public int Id { get; set; }
        public server(string ip, int p, int id) { ipAddress = ip; port = p; Id = id; }
    }

    public class serverAdapter : BaseAdapter
    {
        public List<server> serverList = new List<server>();
        Activity _activity;

        public serverAdapter(Activity activity)
        {
            _activity = activity;
        }

        public override int Count
        {
            get { return serverList.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            // could wrap a Contact in a Java.Lang.Object
            // to return it here if needed
            return null;
        }

        public override long GetItemId(int position)
        {
            return serverList[position].Id;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? _activity.LayoutInflater.Inflate(
                Resource.Layout.serverView, parent, false);
            var ip = view.FindViewById<TextView>(Resource.Id.ipAddress);
            var port = view.FindViewById<TextView>(Resource.Id.port);
            ip.Text = serverList[position].ipAddress;
            port.Text = serverList[position].port.ToString();
            
            return view;
        }
    }
}