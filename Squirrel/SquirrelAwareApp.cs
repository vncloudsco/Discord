namespace Squirrel
{
    using Splat;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class SquirrelAwareApp
    {
        public static void HandleEvents(Action<Version> onInitialInstall = null, Action<Version> onAppUpdate = null, Action<Version> onAppObsoleted = null, Action<Version> onAppUninstall = null, Action onFirstRun = null, string[] arguments = null)
        {
            Action<Version> action = delegate (Version v) {
            };
            string[] strArray = arguments ?? Environment.GetCommandLineArgs().Skip<string>(1).ToArray<string>();
            if (strArray.Length != 0)
            {
                <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>[] typeArray1 = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>[4];
                <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>[] typeArray2 = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>[4];
                typeArray2[0] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>("--squirrel-install", onInitialInstall ?? action);
                <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>[] local15 = typeArray2;
                <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>[] local16 = typeArray2;
                local16[1] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>("--squirrel-updated", onAppUpdate ?? action);
                <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>[] local13 = local16;
                <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>[] local14 = local16;
                local14[2] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>("--squirrel-obsolete", onAppObsoleted ?? action);
                <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>[] local11 = local14;
                <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>[] local12 = local14;
                local12[3] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>("--squirrel-uninstall", onAppUninstall ?? action);
                Dictionary<string, Action<Version>> dictionary = Enumerable.ToDictionary<<25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>, string, Action<Version>>(local12, k => k.Key, v => v.Value);
                if (strArray[0] == "--squirrel-firstrun")
                {
                    (onFirstRun ?? delegate {
                    })();
                }
                else if ((strArray.Length == 2) && dictionary.ContainsKey(strArray[0]))
                {
                    Version version = new Version(strArray[1]);
                    try
                    {
                        dictionary[strArray[0]](version);
                        if (!ModeDetector.InUnitTestRunner())
                        {
                            Environment.Exit(0);
                        }
                    }
                    catch (Exception exception)
                    {
                        LogHost.Default.ErrorException("Failed to handle Squirrel events", exception);
                        if (!ModeDetector.InUnitTestRunner())
                        {
                            Environment.Exit(-1);
                        }
                    }
                }
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly SquirrelAwareApp.<>c <>9 = new SquirrelAwareApp.<>c();
            public static Action<Version> <>9__0_0;
            public static Func<<25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>, string> <>9__0_1;
            public static Func<<25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>>, Action<Version>> <>9__0_2;
            public static Action <>9__0_3;

            internal void <HandleEvents>b__0_0(Version v)
            {
            }

            internal string <HandleEvents>b__0_1(<25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>> k) => 
                k.Key;

            internal Action<Version> <HandleEvents>b__0_2(<25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, Action<Version>> v) => 
                v.Value;

            internal void <HandleEvents>b__0_3()
            {
            }
        }
    }
}

