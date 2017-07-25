package md52520702dc6f751a40a8e5f2fc3ba5c79;


public class SocketBinder
	extends android.os.Binder
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("PrivateChat.SocketBinder, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", SocketBinder.class, __md_methods);
	}


	public SocketBinder () throws java.lang.Throwable
	{
		super ();
		if (getClass () == SocketBinder.class)
			mono.android.TypeManager.Activate ("PrivateChat.SocketBinder, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}

	public SocketBinder (md52520702dc6f751a40a8e5f2fc3ba5c79.SocketService p0) throws java.lang.Throwable
	{
		super ();
		if (getClass () == SocketBinder.class)
			mono.android.TypeManager.Activate ("PrivateChat.SocketBinder, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "PrivateChat.SocketService, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", this, new java.lang.Object[] { p0 });
	}

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
