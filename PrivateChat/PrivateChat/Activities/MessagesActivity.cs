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
using PrivateChat.Adapters;

namespace PrivateChat
{
    [Activity(Label = "MessageActivity")]
    public class MessageActivity : Activity
    {
        private int ServerID;
        private int ConversationID;
        private string PhoneNumber;
        private MessagesAdapter adapter;
        private SQLiteAsyncConnection connection;
        private SocketServiceConnection serviceConnection;
        private bcReceiver bReceiver;

        public static string ReloadAdapter = "reload_action";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Messages);

            // Grab the ServerID from the Intent
            ServerID = Intent.GetIntExtra("ServerID", -1);
            // Grab the ServerID from the Intent (if there is one)
            ConversationID = Intent.GetIntExtra("ConversationID", -1);
            // Grab the phone number of the person the conversation is with (if it's a previously started conversation)
            PhoneNumber = Intent.GetStringExtra("PhoneNumber");

            var sendTo = FindViewById<EditText>(Resource.Id.sendToEt);
            // Check to see if a phone number was passed within the Intent;
            if (PhoneNumber != null)
            {
                // Since a PhoneNumber was passed, need to disable the sendTo EditText and place the phone number in it to be displayed;
                sendTo.Enabled = false;
                sendTo.Text = PhoneNumber;
            }

            // Add a FocusChange for the EditText for clearing out the default text
            // When the user clicks on it to add a contact to send their message.
            sendTo.FocusChange += (sender, e) =>
            {
                if (e.HasFocus)
                {
                    // The user has clicked on the EditText so check to see if the text needs to be cleared out or not
                    if (sendTo.Text == "To")
                    {
                        sendTo.Text = "";
                    }
                }
                else
                {
                    // Since the User has changed the focus from the sendTo EdtText, need to make sure that it wasn't left empty. If it was, display default text
                    if (sendTo.Text == "")
                    {
                        sendTo.Text = "To";
                    }
                }
            };

            // Set up a FocusChange for the EditText for clearing out the default text
            // when the user clicks on it to enter their message. If the user leaves the EditText
            // without inputting a message, display the default again.
            var message = FindViewById<EditText>(Resource.Id.messageEt);
            message.FocusChange += (sender, e) => {
                if (e.HasFocus)
                {
                    // Check if the text is "Type Message" then clear the text
                    if (message.Text == "Type Message")
                    {
                        message.Text = "";
                    }
                }
                else
                {
                    // Check to see if the user inputted any message, then don't touch it otherwise display the default text
                    if (message.Text == "")
                    {
                        message.Text = "Type Message";
                    }
                }
            };

            // In case this is an already started conversation, lets move the focus straight the message EditText
            if (!sendTo.Enabled)
            {
                message.RequestFocus(); // Try to grab the focus
            }

            // Get the path to the database so that a connection can be established
            var docFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            var pathToDatabase = System.IO.Path.Combine(docFolder, "ServerDatabase.db");
            // Connect to the database
            connection = new SQLiteAsyncConnection(pathToDatabase);
            // Create a Message Table if it doesn't exist
            connection.CreateTableAsync<Messages>();

            adapter = new MessagesAdapter(this, connection, ServerID, ConversationID);

            var list = FindViewById<ListView>(Resource.Id.messagesList);
            list.Adapter = adapter;

            var send = FindViewById<Button>(Resource.Id.sendBtn);
            send.Click += SendClick;

            // Bind to the SocketService
            serviceConnection = new SocketServiceConnection(this);
            Intent service = new Intent(this, typeof(SocketService));
            BindService(service, serviceConnection, Bind.AutoCreate);

            // Set up a broadcast receiver that will reload the messages from the database once it's notified by the Socket Service
            bReceiver = new bcReceiver(this);

            IntentFilter filter = new IntentFilter(ReloadAdapter);
            RegisterReceiver(bReceiver, filter);
        }

        // This method is called whenever a new message is received and it needs to be displayed in the activity
        public void ReloadMessages()
        {
            // Lets empty out the adapter first
            adapter.messages.Clear();

            // Grab all of the old messages from the database
            var query = connection.Table<Messages>().Where(v => v.ServerID.Equals(ServerID) && v.ConversationID.Equals(ConversationID));
            query.ToListAsync().ContinueWith(b => {
                foreach (var m in b.Result)
                {
                    // Add each of the messages to the adapter to be displayed
                    adapter.messages.Add(m);
                }
            }).Wait();
        }

        private void SendClick(object sender, EventArgs e)
        {
            bool LoadOLd = false; // This will be set to true if the Conversation already existed

            if (!serviceConnection.Binder.Service.ServerIsConnected(ServerID))
            {
                // Since the server isn't connected, it isn't possible to send any messages so lets end the function call here,
                // but first lets inform the user why we're ending the call here.
                Toast.MakeText(this, "Please connect to the server first before trying to send any messages.", ToastLength.Long).Show();

                return;
            }
            // If the message is an empty string, don't do anything.
            var message = FindViewById<EditText>(Resource.Id.messageEt);
            if (string.IsNullOrEmpty(message.Text))
            {
                // Don't do anything, return from the function call
                return;
            }
            else
            {
                // Since the message string isn't empty, need to make sure that the user inputted a proper phone number
                var sendTo = FindViewById<EditText>(Resource.Id.sendToEt);
                string temp = sendTo.Text.Replace(" ", ""); // temp copy of the sendTo without any spaces
                if (sendTo.Text == "")
                {
                    // Let the user know that they didn't supply a phone to send the message to.
                    Toast.MakeText(this, "Please provide the phone number of the person you wish to send a message to.", ToastLength.Long).Show();

                    // Pass the focus to the EditText for the user to input a valid phone number.
                    sendTo.RequestFocus();
                    // Need to end the function call here
                    return;
                }
                else if (sendTo.Text.Length < 10 || (temp.Length % 10 != 0))
                {
                    // The phone number provided is either too short or too big to be a valid number, so let the user know
                    Toast.MakeText(this, "Provided phone number(s) is either too short or too big, please check and try again.", ToastLength.Long).Show();

                    // Pass the focus to the EditText for the user to input a valid phone number
                    sendTo.RequestFocus();
                    // Need to end the function call here
                    return;
                }

                // The User has provided the APP with a valid phone number and a message, so send it out,
                // store the Conversation in the ConversationTable and the Message in the MessageTable along with the ServerID and ConversationID.

                // Lets disable the sendTo EditText has the user should not be able to change who to send the message out to anymore.
                sendTo.Enabled = false;

                // Get the TimeStamp right now
                var ts = DateTime.Now;

                // Check to see if the Conversation already exists

                //var q = connection.Table<Conversation>().Where(v => v.ServerID.Equals(ServerID) && v.GroupName.Equals(sendTo.Text));
                //int x = 0;
                //q.FirstAsync().ContinueWith(t => { if (t.Result != null) ConversationID = t.Result.ID; }).Wait();
                var q = connection.Table<Conversation>().Where(v => v.ServerID.Equals(ServerID) && v.GroupName.Equals(sendTo.Text));
                q.ToListAsync().ContinueWith(g =>
                {
                    if (g.Result.Count != 0)
                    {
                        LoadOLd = true;
                        var c = g.Result;
                        ConversationID = c[0].ID; // Found a matching Conversation so will update the ConversationID
                    }
                }).Wait();

                // Let's send out the message before moving on
                serviceConnection.Binder.Service.SendMessage(message.Text, sendTo.Text, ServerID);

                // Since if a Conversation already exists then it's ID would have been passed within its Intent
                if (ConversationID != -1)
                {
                    // This conversation was already added to the database so just need to update it instead
                    // Grab the Conversation from the database
                    var query = connection.Table<Conversation>().Where(v => v.ServerID.Equals(ServerID) && v.GroupName.Equals(sendTo.Text));
                    query.FirstAsync().ContinueWith(t => {
                        var conv = t.Result;
                        conv.LastMessage = message.Text;
                        conv.LastTimeStamp = ts.ToString();
                        conv.TotalMessages += 1;
                        connection.UpdateAsync(conv);
                    });
                }
                else
                {
                    // Since the ConversationID at this point is -1, this is a new Conversation, so add it to the database
                    // Maybe I should add constructors to the Table classes for faster initializing 
                    var conversation = new Conversation();
                    conversation.ServerID = ServerID;
                    conversation.GroupName = sendTo.Text;
                    conversation.TotalMessages += 1;
                    conversation.LastMessage = message.Text;
                    conversation.LastTimeStamp = ts.ToString();

                    // Add Conversation to the database an update the ConversationID to the newly added Conversation
                    connection.InsertAsync(conversation).ContinueWith(t => {
                        var query = connection.Table<Conversation>().Where(v => v.ServerID.Equals(ServerID) && v.GroupName.Equals(conversation.GroupName));
                        query.FirstAsync().ContinueWith(b => { ConversationID = b.Result.ID; }).Wait(); // wait for the COnversationID to be updated
                    }).Wait(); // Need to for these operations to finish before moving on as the do need the ConversationID in the step
                }

                var mes = new Messages();
                mes.Message = message.Text;
                mes.Owner = "ME";
                mes.TimeStamp = ts.ToString();
                mes.ServerID = ServerID;
                mes.ConversationID = ConversationID; // Get this from the work above


                // Load the previous message if need to
                if (LoadOLd && adapter.messages.Count == 0)
                {
                    // Grab all of the old messages from the database
                    var query = connection.Table<Messages>().Where(v => v.ServerID.Equals(ServerID) && v.ConversationID.Equals(ConversationID));
                    query.ToListAsync().ContinueWith(b => {
                        foreach (var m in b.Result)
                        {
                            // Add each of the messages to the adapter to be displayed
                            adapter.messages.Add(m);
                        }
                    }).Wait();
                }
                // Add the new Message to the adapter
                adapter.messages.Add(mes);
                // Add the new Message to the database
                connection.InsertAsync(mes);

                // Clear out the TextView for the next Message from the user.
                message.Text = "";
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unbind to the Service
            UnbindService(serviceConnection);

            // Unregister the broadcast receiver
            UnregisterReceiver(bReceiver);
        }
    }
}