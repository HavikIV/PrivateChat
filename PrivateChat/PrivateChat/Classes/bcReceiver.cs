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

namespace PrivateChat
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "notify_action"})]
    class bcReceiver : BroadcastReceiver
    {
        private MessageActivity activity;

        public bcReceiver()
        {
        }

        public bcReceiver(MessageActivity a)
        {
            activity = a;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action.Equals(MessageActivity.ReloadAdapter))
            {
                activity.ReloadMessages(); // Reload the messages
            }
        }
    }
}