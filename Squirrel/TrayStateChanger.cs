namespace Squirrel
{
    using Microsoft.Win32;
    using Splat;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal class TrayStateChanger : IEnableLogger
    {
        private static List<NOTIFYITEM> getTrayItems(TrayNotify instance)
        {
            NotificationCb callback = new NotificationCb();
            ulong handle = 0UL;
            ITrayNotify notify1 = (ITrayNotify) instance;
            notify1.RegisterCallback(callback, out handle);
            notify1.UnregisterCallback(handle);
            return callback.items;
        }

        public List<NOTIFYITEM> GetTrayItems()
        {
            List<NOTIFYITEM> list;
            TrayNotify instance = new TrayNotify();
            try
            {
                list = !useLegacyInterface() ? getTrayItems(instance) : getTrayItemsWin7(instance);
            }
            finally
            {
                Marshal.ReleaseComObject(instance);
            }
            return list;
        }

        private static List<NOTIFYITEM> getTrayItemsWin7(TrayNotify instance)
        {
            NotificationCb callback = new NotificationCb();
            ITrayNotifyWin7 win1 = (ITrayNotifyWin7) instance;
            win1.RegisterCallback(callback);
            win1.RegisterCallback(null);
            return callback.items;
        }

        public void PromoteTrayItem(string exeToPromote)
        {
            TrayNotify instance = new TrayNotify();
            try
            {
                List<NOTIFYITEM> list = null;
                bool flag = useLegacyInterface();
                list = !flag ? getTrayItems(instance) : getTrayItemsWin7(instance);
                exeToPromote = exeToPromote.ToLowerInvariant();
                for (int i = 0; i < list.Count; i++)
                {
                    NOTIFYITEM item = list[i];
                    if (item.exe_name.ToLowerInvariant().Contains(exeToPromote) && (item.preference == NOTIFYITEM_PREFERENCE.PREFERENCE_SHOW_WHEN_ACTIVE))
                    {
                        item.preference = NOTIFYITEM_PREFERENCE.PREFERENCE_SHOW_ALWAYS;
                        if (flag)
                        {
                            ((ITrayNotifyWin7) instance).SetPreference(ref NOTIFYITEM_Writable.fromNotifyItem(item));
                        }
                        else
                        {
                            ((ITrayNotify) instance).SetPreference(ref NOTIFYITEM_Writable.fromNotifyItem(item));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Failed to promote Tray icon: " + exception.ToString());
            }
            finally
            {
                Marshal.ReleaseComObject(instance);
            }
        }

        public unsafe void RemoveDeadEntries(List<string> executablesInPackage, string rootAppDirectory, string currentAppVersion)
        {
            byte[] sourceArray = null;
            try
            {
                sourceArray = (byte[]) Registry.GetValue(@"HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\TrayNotify", "IconStreams", new byte[1]);
                if (sourceArray.Length >= 20)
                {
                    byte[] buffer2;
                    List<byte[]> list = new List<byte[]>();
                    IconStreamsHeader structure = new IconStreamsHeader();
                    fixed (byte* numRef = ((((buffer2 = sourceArray) == null) || (buffer2.Length == 0)) ? null : ((byte*) buffer2[0])))
                    {
                        structure = (IconStreamsHeader) Marshal.PtrToStructure((IntPtr) numRef, typeof(IconStreamsHeader));
                        if (structure.count > 1)
                        {
                            int num = 0;
                            while (true)
                            {
                                if (num >= structure.count)
                                {
                                    if (structure.count != list.Count)
                                    {
                                        structure.count = (uint) list.Count;
                                        Marshal.StructureToPtr(structure, (IntPtr) numRef, false);
                                        byte* numPtr = numRef + Marshal.SizeOf(typeof(IconStreamsHeader));
                                        for (int i = 0; i < list.Count; i++)
                                        {
                                            Marshal.Copy(list[i], 0, (IntPtr) numPtr, list[i].Length);
                                            numPtr += list[i].Length;
                                        }
                                        numRef = ref null;
                                        try
                                        {
                                            int length = Marshal.SizeOf(typeof(IconStreamsHeader)) + (list.Count * Marshal.SizeOf(typeof(IconStreamsItem)));
                                            byte[] destinationArray = new byte[length];
                                            Array.Copy(sourceArray, destinationArray, length);
                                            Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\TrayNotify", "IconStreams", destinationArray);
                                        }
                                        catch (Exception exception3)
                                        {
                                            Console.WriteLine("Failed to write new IconStreams regkey: " + exception3.ToString());
                                        }
                                    }
                                    return;
                                }
                                int sourceIndex = Marshal.SizeOf(typeof(IconStreamsHeader)) + (num * Marshal.SizeOf(typeof(IconStreamsItem)));
                                if (sourceIndex > sourceArray.Length)
                                {
                                    this.Log<TrayStateChanger>().Error("Corrupted IconStreams regkey, bailing");
                                    return;
                                }
                                IconStreamsItem item = (IconStreamsItem) Marshal.PtrToStructure((IntPtr) (numRef + sourceIndex), typeof(IconStreamsItem));
                                try
                                {
                                    string path = item.ExePath.ToLowerInvariant();
                                    if (!Enumerable.Any<string>(executablesInPackage, exe => path.Contains(exe)) || (!path.StartsWith(rootAppDirectory, StringComparison.Ordinal) || path.Contains("app-" + currentAppVersion)))
                                    {
                                        byte[] destinationArray = new byte[Marshal.SizeOf(typeof(IconStreamsItem))];
                                        Array.Copy(sourceArray, sourceIndex, destinationArray, 0, destinationArray.Length);
                                        list.Add(destinationArray);
                                    }
                                    num++;
                                    continue;
                                }
                                catch (Exception exception2)
                                {
                                    this.Log<TrayStateChanger>().ErrorException("Failed to parse IconStreams regkey", exception2);
                                }
                                break;
                            }
                        }
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Couldn't load IconStreams key, bailing: " + exception.ToString());
            }
        }

        private static bool useLegacyInterface()
        {
            Version version = Environment.OSVersion.Version;
            return ((version.Major >= 6) ? ((version.Major <= 6) ? (version.Minor <= 1) : false) : true);
        }

        private class NotificationCb : INotificationCb
        {
            public readonly List<NOTIFYITEM> items = new List<NOTIFYITEM>();

            public void Notify([In] uint nEvent, [In] ref NOTIFYITEM notifyItem)
            {
                this.items.Add(notifyItem);
            }
        }
    }
}

