namespace Squirrel
{
    using System;
    using System.Runtime.InteropServices;

    [ComImport, Guid("FB852B2C-6BAD-4605-9551-F15F87830935"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface ITrayNotifyWin7
    {
        void RegisterCallback([MarshalAs(UnmanagedType.Interface)] INotificationCb callback);
        void SetPreference([In] ref NOTIFYITEM_Writable notifyItem);
        void EnableAutoTray([In] bool enabled);
    }
}

