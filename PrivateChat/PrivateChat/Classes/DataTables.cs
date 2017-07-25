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
using Android.Database.Sqlite;
using Android.Provider;

using SQLite;

namespace PrivateChat.Tables
{
    public class Messages
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        // Column Names for the Table
        // The TimeStamp column will contain the time stamp when the message was sent or received
        // The Owner column will contain name of the creator of the message (It will determine which side the is displayed on)
        // The Message column will contain the message
        // The ConversationID will contain the ID of the conversation that the message is part of
        public string TimeStamp { get; set; } // Set to the current time stamp using GetTimeStamp(DataTime.Now)
        public string Owner { get; set; }
        public string Message { get; set; }
        public int ConversationID { get; set; }
    }

    public class Conversation
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        // Column Names for the Table
        // The GroupName column will contain the name(s) of who the user is messaging 
        // The TotalMessages column will contain the total number messages that are within the conversation
        // The Messages column will contain a link to a Messages Table
        // The ServerID column will contain the ID of the Server that the Conversation belongs to
        // The LastMessage column will contain the last message in the conversation
        // The LastTimeStamp column will contain the TimeStamp of the last message
        public int ServerID { get; set; }
        public string GroupName { get; set; }
        public int TotalMessages { get; set; }
        public string LastMessage { get; set; }
        public string LastTimeStamp { get; set; }
        //public Messages Messages { get; set; }
    }

    public class Server
    {

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        // Column Names for the Table
        // The Name column will contain the given name for the Server
        // The IPAddress column will contain the IP Address for the Server
        // The Port column will contain the port number that's needed to connection to the Server
        // The Conversation column will contain a link to a Conversation Table
        public string Name { get; set; }
        public string IPAddress { get; set; }
        public int Port { get; set; }
        //public Conversation Conversations { get; set; }
    }
}