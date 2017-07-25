package md52520702dc6f751a40a8e5f2fc3ba5c79;


public class MessageActivity
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("PrivateChat.MessageActivity, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", MessageActivity.class, __md_methods);
	}


	public MessageActivity () throws java.lang.Throwable
	{
		super ();
		if (getClass () == MessageActivity.class)
			mono.android.TypeManager.Activate ("PrivateChat.MessageActivity, PrivateChat, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);

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
