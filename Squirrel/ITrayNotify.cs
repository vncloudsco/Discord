namespace Squirrel
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("D133CE13-3537-48BA-93A7-AFCD5D2053B4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITrayNotify
    {
        void RegisterCallback([MarshalAs(UnmanagedType.Interface)] INotificationCb callback, out ulong handle);
        void UnregisterCallback([In] ulong handle);
        void SetPreference([In] ref NOTIFYITEM_Writable notifyItem);
        void EnableAutoTray([In] bool enabled);
        void DoAction([In] bool enabled);
    }
}

