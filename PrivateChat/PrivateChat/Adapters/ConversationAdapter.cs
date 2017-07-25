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
using Java.Lang;

using SQLite;

using PrivateChat.Tables;

namespace PrivateChat.Adapters
{
    // This custom adapter is used to display the conversations in the ListView of the ConversationActivity
    class ConversationAdapter : BaseAdapter
    {
        public List<Conversation> conversations;
        private Activity activity;
        private SQLiteAsyncConnection conn;
        private int ServerID;


        public ConversationAdapter(Activity a, SQLiteAsyncConnection c, int s)
        {
            activity = a;
            conn = c;
            ServerID = s;
            conversations = new List<Conversation>();
            LoadConversations();
        }

        // This method is called to load previously started conversations.
        // It will query the database to see if there are any Conversations saved
        // in the Conversation Table that correspond with the given ServerID
        private void LoadConversations()
        {
            // Query the database for conversation with ServerID
            var query = conn.Table<Conversation>().Where(v => v.ServerID.Equals(ServerID));

            // Add the conversations to a List by using ToListAsync() and added them to the adapter to display the conversation in the ListView
            query.ToListAsync().ContinueWith(t => {
                foreach (var convo in t.Result)
                {
                    // Add each conversation to the List
                    conversations.Add(convo);
                }
            }).Wait();
            NotifyDataSetChanged();
        }

        public override int Count
        {
            get
            {
                return conversations.Count();
            }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return conversations[position].ID;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? activity.LayoutInflater.Inflate(Resource.Layout.ConversationListItem, parent, false);
            var groupName = view.FindViewById<TextView>(Resource.Id.GroupName);
            var lastMessage = view.FindViewById<TextView>(Resource.Id.LastMessage);
            var totalMessages = view.FindViewById<TextView>(Resource.Id.TotalMessages);
            var lastTimeStamp = view.FindViewById<TextView>(Resource.Id.LastTimeStamp);
            groupName.Text = conversations[position].GroupName;
            lastMessage.Text = conversations[position].LastMessage;
            totalMessages.Text = conversations[position].TotalMessages.ToString();
            lastTimeStamp.Text = conversations[position].LastTimeStamp;

            return view;
        }
    }
}