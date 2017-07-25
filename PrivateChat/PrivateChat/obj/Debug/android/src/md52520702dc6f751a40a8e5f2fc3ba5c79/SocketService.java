package md52520702dc6f751a40a8e5f2fc3ba5c79;


public class SocketService
	extends android.app.Service
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:()V:GetOnCreateHandler\n" +
			"n_onBind:(Landroid/content/Intent;)Landroid/os/IBinder;:GetOnBind_Landroid_content_Intent_Handler\n" +
			"n_onUnbind:(Landroid/content/Intent;)Z:GetOnUnbind_Landroid_content_Intent_Handler\n" +
			"n_onDestroy:()V:GetOnDestroyHandler\n" +
			"";
		mono.android.Runtime.register ("PrivateChat.SocketService, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", SocketService.class, __md_methods);
	}


	public SocketService () throws java.lang.Throwable
	{
		super ();
		if (getClass () == SocketService.class)
			mono.android.TypeManager.Activate ("PrivateChat.SocketService, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate ()
	{
		n_onCreate ();
	}

	private native void n_onCreate ();


	public android.os.IBinder onBind (android.content.Intent p0)
	{
		return n_onBind (p0);
	}

	private native android.os.IBinder n_onBind (android.content.Intent p0);


	public boolean onUnbind (android.content.Intent p0)
	{
		return n_onUnbind (p0);
	}

	private native boolean n_onUnbind (android.content.Intent p0);


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
