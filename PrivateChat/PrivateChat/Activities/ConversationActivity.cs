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

using com.refractored.fab;
using SQLite;

using PrivateChat.Tables;
using PrivateChat.Adapters;

namespace PrivateChat
{
    [Activity()]
    public class ConversationActivity : Activity, ActionMode.ICallback
    {
        private int ServerID;
        private SQLiteAsyncConnection connection;
        private ConversationAdapter adapter;
        private ActionMode actionmode;
        private int deletePosition = -1;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the conversation layout resource
            SetContentView(Resource.Layout.Conversation);

            // Get the ServerID that was padded to the Intent when this activity was started
            // and store it in the class's ServerId property for later use.
            ServerID = Intent.GetIntExtra("ServerID", 0);

            // Create a path to the Database
            var docFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var pathToDatabase = System.IO.Path.Combine(docFolder, "ServerDatabase.db");

            // Create a connection to the database that this activity will use
            ConnectToDatabase(pathToDatabase);

            // Load any previously started conversations into a ConversationAdapter
            adapter = new ConversationAdapter(this, connection, ServerID);
            var list = FindViewById<ListView>(Resource.Id.conversationList);
            list.Adapter = adapter;

            // Add a LongClickListener for the items in the ListView
            list.LongClickable = true;
            list.ItemLongClick += ListItemLongClick;

            // Create an event for when the FAB is clicked
            var fab = FindViewById<FloatingActionButton>(Resource.Id.cfab);
            fab.Click += FabClick;
        }

        private void ListItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            deletePosition = e.Position;

            actionmode = StartActionMode(this);
        }

        public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.actionDeleteConversation:
                    {
                        DeleteConversation();
                        mode.Finish();
                        return false;
                    }
            }

            return false;
        }

        private void DeleteConversation()
        {
            if (deletePosition != -1)
            {
                // Delete from the adapter
                adapter.conversations.RemoveAt(deletePosition);
                adapter.NotifyDataSetChanged();

                // Delete from the database
                deletePosition += 1;
                var query = connection.Table<Conversation>().Where(v => v.ID.Equals(deletePosition));
                // Don't need to wait for this call to finish, let it happen in the background
                query.FirstAsync().ContinueWith(t => { connection.DeleteAsync(t.Result); });
                //connection.DeleteAsync(conv);

                // Reset the deletePosition variable
                deletePosition = -1;
            }
        }

        public bool OnCreateActionMode(ActionMode mode, IMenu menu)
        {
            MenuInflater inflater = mode.MenuInflater;
            inflater.Inflate(Resource.Menu.ConversationActionBar, menu);
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

        // Whenever this FAB is clicked it should start a new conversation and add it to the Conversation Table
        // Starting a new conversation consists of starting a new MessagesActivity
        private void FabClick(object sender, EventArgs e)
        {
            var intent = new Intent(this, typeof(MessageActivity));
            intent.PutExtra("ServerID", ServerID);

            // Start the MessageActivity
            StartActivity(intent);
        }

        // This method is called to try to establish a connection to the database
        // and adding a Conversation Table if needed.
        private void ConnectToDatabase(string path)
        {
            try
            {
                // Make a connection to the database
                connection = new SQLiteAsyncConnection(path);

                // Add a Conversations Table to the database if it doesn't exist
                connection.CreateTableAsync<Conversation>().Wait();
            }
            catch (SQLiteException ex)
            {
                // Something went wrong trying to connect to the database and need to let the user know
                Toast.MakeText(this, "Something went wrong will trying to connect to the database. The exception is " + ex, ToastLength.Long).Show();
            }
        }
    }
}