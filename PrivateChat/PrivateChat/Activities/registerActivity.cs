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
    [Activity(Label = "Register", NoHistory = true)]
    public class registerActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            
            // Display the registerPhone layout
            SetContentView(Resource.Layout.registerPhone);

            // Set what action should be taken when the save button is clicked
            Button save = FindViewById<Button>(Resource.Id.saveBtn);
            save.Click += savePhone;
        }

        private void savePhone(object sender, EventArgs e)
        {
            // Get the shared preferences
            ISharedPreferences prefs = this.GetSharedPreferences("com.PrivateChat.PrivateChat", FileCreationMode.Private);
            ISharedPreferencesEditor editor = prefs.Edit();

            // Grab the phone number that was entered in the EditText
            EditText et = FindViewById<EditText>(Resource.Id.phoneEt);
            string phone = et.Text;

            // Grab the user's full name that was entered in the EditText
            EditText nameET = FindViewById<EditText>(Resource.Id.nameEt);
            string name = nameET.Text;

            // make sure the length of the string is 10 characters long
            if (phone.Length == 10)
            {
                // Save the phone number and name
                editor.PutString("phone", phone);
                editor.PutString("name", name);
                editor.Apply();


                // Since the phone number has been saved, need to move the user on to the main activity
                var intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
            }
            else
            {
                // Not a valid phone number so clear the EditText for the user to try again.
                et.Text = "";
            }
        }
    }
}