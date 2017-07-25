using Android.OS;

namespace PrivateChat
{
    public class SocketBinder : Binder
    {
        public SocketBinder(SocketService service)
        {
            this.Service = service;
        }

        public SocketService Service { get; private set; }
    }
}