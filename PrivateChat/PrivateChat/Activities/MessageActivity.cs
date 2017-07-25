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

namespace PrivateChat
{
    [Activity(Label = "MessageActivity")]
    public class MessageActivity : Activity
    {
        private int ServerID;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Messages);

            // Grab the ServerID from the Intent
            ServerID = Intent.GetIntExtra("ServerID", -1);

            // Set up a FocusChange for the EditText for clearing out the default text
            // when the user clicks on it to enter their message. If the user leaves the EditText
            // without inputting a message, display the default again.
            var message = FindViewById<EditText>(Resource.Id.messageEt);
            message.FocusChange += (sender, args) => {
                if (args.HasFocus)
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

            var send = FindViewById<Button>(Resource.Id.sendBtn);
            send.Click += SendClick;
        }

        private void SendClick(object sender, EventArgs e)
        {
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
                if (sendTo.Text == "")
                {
                    // Let the user know that they didn't supply a phone to send the message to.
                    Toast.MakeText(this, "Please provide the phone number of the person you wish to send a message to.", ToastLength.Long).Show();

                    // Pass the focus to the EditText for the user to input a valid phone number.
                    sendTo.RequestFocus();
                }
                else if (sendTo.Text.Length < 10 || sendTo.Text.Length > 10)
                {
                    // The phone number provided is either too short or too big to be a valid number, so let the user know
                    Toast.MakeText(this, "Provided phone number is either too short or too big, please check and try again.", ToastLength.Long).Show();

                    // Pass the focus to the EditText for the user to input a valid phone number
                    sendTo.RequestFocus();
                }

                // The User has provided the APP with a valid phone number and a message, so send it out,
                // store the Conversation in the ConversationTable and the Message in the MessageTable along with the ServerID and ConversationID.

                // Get the path to the database so that a connection can be established
                var docFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var pathToDatabase = System.IO.Path.Combine(docFolder, "ServerDatabase.db");
                // Connect to the database
                SQLiteAsyncConnection connection = new SQLiteAsyncConnection(pathToDatabase);

                // Maybe I should add constructors to the Table classes for faster initializing 
                var conversation = new Conversation();
                conversation.ServerID = ServerID;
                conversation.GroupName = sendTo.Text;
                conversation.TotalMessages += 1;
                conversation.LastMessage = message.Text;
                conversation.LastTimeStamp = DateTime.Now.ToString();

                // Add Conversation to the database
                connection.InsertAsync(conversation);

                // Create a Message Table if it doesn't exist and add this message to it
            }
        }
    }
}