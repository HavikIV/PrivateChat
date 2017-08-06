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

using SQLite;

using PrivateChat.Tables;

namespace PrivateChat.Adapters
{
    class MessagesAdapter : BaseAdapter
    {

        Activity activity;
        public List<Messages> messages;

        public MessagesAdapter(Activity a, SQLiteAsyncConnection c, int si, int ci)
        {
            activity = a;
            messages = new List<Messages>();
            if (ci != -1)
            {
                // Since the Conversation was previously started, load all of the messages
                LoadMessages(c, si, ci);
            }
        }

        // All of the previously added messages
        private void LoadMessages(SQLiteAsyncConnection connection, int ServerID, int ConversationID)
        {
            // Query the Database for all of the messages for the corresponding ServerID and ConversationID
            var query = connection.Table<Messages>().Where(v => v.ServerID.Equals(ServerID) && v.ConversationID.Equals(ConversationID));

            // Add each of the Messages found to the List of Messages
            query.ToListAsync().ContinueWith(t => {
                foreach (var m in t.Result)
                {
                    messages.Add(m);
                }
            }).Wait();
            NotifyDataSetChanged();

        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return messages[position].ID;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? activity.LayoutInflater.Inflate(Resource.Layout.MessagesListItem, parent, false);
            var messageElement = messages[position];

            // Fill in the items of the view
            var message = view.FindViewById<TextView>(Resource.Id.Message);
            var timeStamp = view.FindViewById<TextView>(Resource.Id.TimeStamp);
            message.Text = messageElement.Message;
            timeStamp.Text = messageElement.TimeStamp;
            if (messageElement.Owner != "ME")
            {
                // Since the creator of the message isn't the user, need to set the items to align to the left side of the screen
                var param = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                param.AddRule(LayoutRules.AlignParentLeft);
                message.LayoutParameters = param;

                var param2 = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                param.AddRule(LayoutRules.AlignParentLeft);
                param2.AddRule(LayoutRules.Below, message.Id);
                timeStamp.LayoutParameters = param2;
            }

            return view;
        }

        // Fill in count here, currently 0
        public override int Count
        {
            get
            {
                return messages.Count;
            }
        }

    }
}