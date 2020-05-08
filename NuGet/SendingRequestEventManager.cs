namespace NuGet
{
    using System;
    using System.Windows;

    internal class SendingRequestEventManager : WeakEventManager
    {
        private static readonly object _managerLock = new object();

        public static void AddListener(IHttpClientEvents source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedAddListener(source, listener);
        }

        private void OnSendingRequest(object sender, WebRequestEventArgs e)
        {
            base.DeliverEvent(sender, e);
        }

        public static void RemoveListener(IHttpClientEvents source, IWeakEventListener listener)
        {
            CurrentManager.ProtectedRemoveListener(source, listener);
        }

        protected override void StartListening(object source)
        {
            ((IHttpClientEvents) source).SendingRequest += new EventHandler<WebRequestEventArgs>(this.OnSendingRequest);
        }

        protected override void StopListening(object source)
        {
            ((IHttpClientEvents) source).SendingRequest -= new EventHandler<WebRequestEventArgs>(this.OnSendingRequest);
        }

        private static SendingRequestEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(SendingRequestEventManager);
                object obj2 = _managerLock;
                lock (obj2)
                {
                    SendingRequestEventManager currentManager = (SendingRequestEventManager) GetCurrentManager(managerType);
                    if (currentManager == null)
                    {
                        currentManager = new SendingRequestEventManager();
                        SetCurrentManager(managerType, currentManager);
                    }
                    return currentManager;
                }
            }
        }
    }
}

