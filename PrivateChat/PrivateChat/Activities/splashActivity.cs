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
    [Activity(Theme = "@style/SplashTheme", MainLauncher = true, NoHistory = true)]
    public class splashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here

            // Check to see if the phone number has been saved or not
            ISharedPreferences prefs = this.GetSharedPreferences("PrivateChat.PrivateChat", FileCreationMode.Private);
            string phone = prefs.GetString("phone", "default");
            // if phone number has been saved open the MainActivity else open the registerActvitiy
            Intent intent;
            if (phone != "default")
            {
                intent = new Intent(this, typeof(MainActivity));
            }
            else
            {
                intent = new Intent(this, typeof(registerActivity));
            }

            // Start the SocketService
            Intent serviceToStart = new Intent(this, typeof(SocketService));
            StartService(serviceToStart);

            // Start the activity
            StartActivity(intent);
        }
    }
}