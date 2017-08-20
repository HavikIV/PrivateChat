package md52520702dc6f751a40a8e5f2fc3ba5c79;


public class NewServerDialog
	extends android.app.DialogFragment
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreateDialog:(Landroid/os/Bundle;)Landroid/app/Dialog;:GetOnCreateDialog_Landroid_os_Bundle_Handler\n" +
			"n_onDestroy:()V:GetOnDestroyHandler\n" +
			"";
		mono.android.Runtime.register ("PrivateChat.NewServerDialog, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", NewServerDialog.class, __md_methods);
	}


	public NewServerDialog () throws java.lang.Throwable
	{
		super ();
		if (getClass () == NewServerDialog.class)
			mono.android.TypeManager.Activate ("PrivateChat.NewServerDialog, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public NewServerDialog (android.content.Context p0, android.widget.ListView p1, md5223c696fa0ea78c72c00d0ddee27184f.ServerAdapter p2, java.lang.String p3, int p4) throws java.lang.Throwable
	{
		super ();
		if (getClass () == NewServerDialog.class)
			mono.android.TypeManager.Activate ("PrivateChat.NewServerDialog, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "Android.Content.Context, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:Android.Widget.ListView&, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=84e04ff9cfb79065:PrivateChat.Adapters.ServerAdapter&, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null:System.String, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e:System.Int32, mscorlib, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", this, new java.lang.Object[] { p0, p1, p2, p3, p4 });
	}


	public android.app.Dialog onCreateDialog (android.os.Bundle p0)
	{
		return n_onCreateDialog (p0);
	}

	private native android.app.Dialog n_onCreateDialog (android.os.Bundle p0);


	public void onDestroy ()
	{
		n_onDestroy ();
	}

	private native void n_onDestroy ();

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
