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
    public class SocketServiceConnection : Java.Lang.Object, IServiceConnection
    {
        static readonly string TAF = typeof(SocketServiceConnection).FullName;

        Context mainActivity;
        
        public SocketServiceConnection(Context activity)
        {
            IsConnected = false;
            Binder = null;
            mainActivity = activity;
        }

        public bool IsConnected { get; private set; }
        public SocketBinder Binder { get; private set; }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            Binder = service as SocketBinder;
            IsConnected = this.Binder != null;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            IsConnected = false;
            Binder = null;
        }
    }
}