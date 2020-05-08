namespace Squirrel.Shell
{
    using Microsoft.CSharp.RuntimeBinder;
    using System;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class TaskbarHelper
    {
        public static bool IsPinnedToTaskbar(string executablePath) => 
            Enumerable.Any<ShellLink>(from pinnedShortcut in Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar"), "*.lnk") select new ShellLink(pinnedShortcut), shortcut => string.Equals(shortcut.Target, executablePath, StringComparison.OrdinalIgnoreCase));

        public static void PinToTaskbar(string executablePath)
        {
            pinUnpin(executablePath, "pin to taskbar");
            if (!IsPinnedToTaskbar(executablePath))
            {
                throw new Exception("Pinning executable to taskbar failed.");
            }
        }

        private static void pinUnpin(string executablePath, string verbToExecute)
        {
            object obj2 = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));
            try
            {
                string directoryName = Path.GetDirectoryName(executablePath);
                string fileName = Path.GetFileName(executablePath);
                if (<>o__3.<>p__0 == null)
                {
                    CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) };
                    <>o__3.<>p__0 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "NameSpace", null, typeof(TaskbarHelper), argumentInfo));
                }
                object obj3 = <>o__3.<>p__0.Target(<>o__3.<>p__0, obj2, directoryName);
                if (<>o__3.<>p__1 == null)
                {
                    CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) };
                    <>o__3.<>p__1 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ParseName", null, typeof(TaskbarHelper), argumentInfo));
                }
                object obj4 = <>o__3.<>p__1.Target(<>o__3.<>p__1, obj3, fileName);
                if (<>o__3.<>p__2 == null)
                {
                    CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                    <>o__3.<>p__2 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Verbs", null, typeof(TaskbarHelper), argumentInfo));
                }
                object obj5 = <>o__3.<>p__2.Target(<>o__3.<>p__2, obj4);
                int num = 0;
                while (true)
                {
                    if (<>o__3.<>p__5 == null)
                    {
                        CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                        <>o__3.<>p__5 = CallSite<Func<CallSite, object, bool>>.Create(Binder.UnaryOperation(CSharpBinderFlags.None, (ExpressionType) 0x53, typeof(TaskbarHelper), argumentInfo));
                    }
                    if (<>o__3.<>p__4 == null)
                    {
                        CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                        <>o__3.<>p__4 = CallSite<Func<CallSite, int, object, object>>.Create(Binder.BinaryOperation(CSharpBinderFlags.None, ExpressionType.LessThan, typeof(TaskbarHelper), argumentInfo));
                    }
                    if (<>o__3.<>p__3 == null)
                    {
                        CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                        <>o__3.<>p__3 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Count", null, typeof(TaskbarHelper), argumentInfo));
                    }
                    if (!<>o__3.<>p__5.Target(<>o__3.<>p__5, <>o__3.<>p__4.Target(<>o__3.<>p__4, num, <>o__3.<>p__3.Target(<>o__3.<>p__3, obj5))))
                    {
                        break;
                    }
                    if (<>o__3.<>p__6 == null)
                    {
                        CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) };
                        <>o__3.<>p__6 = CallSite<Func<CallSite, object, int, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Item", null, typeof(TaskbarHelper), argumentInfo));
                    }
                    object obj6 = <>o__3.<>p__6.Target(<>o__3.<>p__6, obj5, num);
                    if (<>o__3.<>p__10 == null)
                    {
                        <>o__3.<>p__10 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(TaskbarHelper)));
                    }
                    if (<>o__3.<>p__9 == null)
                    {
                        CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                        <>o__3.<>p__9 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "ToLower", null, typeof(TaskbarHelper), argumentInfo));
                    }
                    if (<>o__3.<>p__8 == null)
                    {
                        CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) };
                        <>o__3.<>p__8 = CallSite<Func<CallSite, object, string, string, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Replace", null, typeof(TaskbarHelper), argumentInfo));
                    }
                    if (<>o__3.<>p__7 == null)
                    {
                        CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                        <>o__3.<>p__7 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(TaskbarHelper), argumentInfo));
                    }
                    if (<>o__3.<>p__10.Target(<>o__3.<>p__10, <>o__3.<>p__9.Target(<>o__3.<>p__9, <>o__3.<>p__8.Target(<>o__3.<>p__8, <>o__3.<>p__7.Target(<>o__3.<>p__7, obj6), "&", string.Empty))).Equals(verbToExecute))
                    {
                        if (<>o__3.<>p__11 == null)
                        {
                            CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                            <>o__3.<>p__11 = CallSite<Action<CallSite, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "DoIt", null, typeof(TaskbarHelper), argumentInfo));
                        }
                        <>o__3.<>p__11.Target(<>o__3.<>p__11, obj6);
                    }
                    num++;
                }
            }
            finally
            {
                if (<>o__3.<>p__12 == null)
                {
                    CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.IsStaticType | CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                    <>o__3.<>p__12 = CallSite<Action<CallSite, Type, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "ReleaseComObject", null, typeof(TaskbarHelper), argumentInfo));
                }
                <>o__3.<>p__12.Target(<>o__3.<>p__12, typeof(Marshal), obj2);
            }
        }

        public static void UnpinFromTaskbar(string executablePath)
        {
            pinUnpin(executablePath, "unpin from taskbar");
            if (IsPinnedToTaskbar(executablePath))
            {
                throw new Exception("Executable is still pinned to taskbar.");
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly TaskbarHelper.<>c <>9 = new TaskbarHelper.<>c();
            public static Func<string, ShellLink> <>9__0_0;

            internal ShellLink <IsPinnedToTaskbar>b__0_0(string pinnedShortcut) => 
                new ShellLink(pinnedShortcut);
        }

        [CompilerGenerated]
        private static class <>o__3
        {
            public static CallSite<Func<CallSite, object, string, object>> <>p__0;
            public static CallSite<Func<CallSite, object, string, object>> <>p__1;
            public static CallSite<Func<CallSite, object, object>> <>p__2;
            public static CallSite<Func<CallSite, object, object>> <>p__3;
            public static CallSite<Func<CallSite, int, object, object>> <>p__4;
            public static CallSite<Func<CallSite, object, bool>> <>p__5;
            public static CallSite<Func<CallSite, object, int, object>> <>p__6;
            public static CallSite<Func<CallSite, object, object>> <>p__7;
            public static CallSite<Func<CallSite, object, string, string, object>> <>p__8;
            public static CallSite<Func<CallSite, object, object>> <>p__9;
            public static CallSite<Func<CallSite, object, string>> <>p__10;
            public static CallSite<Action<CallSite, object>> <>p__11;
            public static CallSite<Action<CallSite, Type, object>> <>p__12;
        }
    }
}

