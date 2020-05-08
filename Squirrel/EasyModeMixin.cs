namespace Squirrel
{
    using NuGet;
    using Splat;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    internal static class EasyModeMixin
    {
        public static void CreateShortcutForThisExe(this IUpdateManager This)
        {
            This.CreateShortcutsForExecutable(Path.GetFileName(Assembly.GetEntryAssembly().Location), ShortcutLocations.Defaults, !Environment.CommandLine.Contains("squirrel-install"), null, null);
        }

        public static void RemoveShortcutForThisExe(this IUpdateManager This)
        {
            This.RemoveShortcutsForExecutable(Path.GetFileName(Assembly.GetEntryAssembly().Location), ShortcutLocations.Defaults);
        }

        [AsyncStateMachine(typeof(<UpdateApp>d__0))]
        public static Task<ReleaseEntry> UpdateApp(this IUpdateManager This, Action<int> progress = null)
        {
            <UpdateApp>d__0 d__;
            d__.This = This;
            d__.progress = progress;
            d__.<>t__builder = AsyncTaskMethodBuilder<ReleaseEntry>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<UpdateApp>d__0>(ref d__);
            return d__.<>t__builder.Task;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly EasyModeMixin.<>c <>9 = new EasyModeMixin.<>c();
            public static Action<int> <>9__0_0;
            public static Func<ReleaseEntry, SemanticVersion> <>9__0_8;

            internal void <UpdateApp>b__0_0(int _)
            {
            }

            internal SemanticVersion <UpdateApp>b__0_8(ReleaseEntry x) => 
                x.Version;
        }

        [CompilerGenerated]
        private struct <UpdateApp>d__0 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<ReleaseEntry> <>t__builder;
            public IUpdateManager This;
            public Action<int> progress;
            private EasyModeMixin.<>c__DisplayClass0_0 <>8__1;
            private EasyModeMixin.<>c__DisplayClass0_0 <>7__wrap1;
            private TaskAwaiter<UpdateInfo> <>u__1;
            private TaskAwaiter <>u__2;
            private TaskAwaiter<string> <>u__3;
            private TaskAwaiter<RegistryKey> <>u__4;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    ReleaseEntry entry;
                    switch (num)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            goto TR_0019;

                        default:
                            this.<>8__1 = new EasyModeMixin.<>c__DisplayClass0_0();
                            this.<>8__1.This = this.This;
                            this.<>8__1.progress = this.progress;
                            this.<>8__1.progress = this.<>8__1.progress ?? (EasyModeMixin.<>c.<>9__0_0 ?? (EasyModeMixin.<>c.<>9__0_0 = new Action<int>(this.<UpdateApp>b__0_0)));
                            this.<>8__1.This.Log<IUpdateManager>().Info("Starting automatic update");
                            this.<>8__1.ignoreDeltaUpdates = false;
                            break;
                    }
                    goto TR_001B;
                TR_000A:
                    entry = this.<>8__1.updateInfo.ReleasesToApply.Any<ReleaseEntry>() ? this.<>8__1.updateInfo.ReleasesToApply.MaxBy<ReleaseEntry, SemanticVersion>((EasyModeMixin.<>c.<>9__0_8 ?? (EasyModeMixin.<>c.<>9__0_8 = new Func<ReleaseEntry, SemanticVersion>(this.<UpdateApp>b__0_8)))).Last<ReleaseEntry>() : null;
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult(entry);
                    return;
                TR_0019:
                    try
                    {
                        TaskAwaiter<UpdateInfo> awaiter;
                        TaskAwaiter awaiter2;
                        TaskAwaiter<string> awaiter3;
                        TaskAwaiter<RegistryKey> awaiter4;
                        switch (num)
                        {
                            case 0:
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter<UpdateInfo>();
                                this.<>1__state = num = -1;
                                break;

                            case 1:
                                awaiter2 = this.<>u__2;
                                this.<>u__2 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_000E;

                            case 2:
                                awaiter3 = this.<>u__3;
                                this.<>u__3 = new TaskAwaiter<string>();
                                this.<>1__state = num = -1;
                                goto TR_000C;

                            case 3:
                                awaiter4 = this.<>u__4;
                                this.<>u__4 = new TaskAwaiter<RegistryKey>();
                                this.<>1__state = num = -1;
                                goto TR_000B;

                            default:
                            {
                                this.<>7__wrap1 = this.<>8__1;
                                UpdateInfo updateInfo = this.<>7__wrap1.updateInfo;
                                awaiter = this.<>8__1.This.ErrorIfThrows<UpdateInfo>(new Func<Task<UpdateInfo>>(this.<>8__1.<UpdateApp>b__1), "Failed to check for updates").GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    break;
                                }
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<UpdateInfo>, EasyModeMixin.<UpdateApp>d__0>(ref awaiter, ref this);
                                return;
                            }
                        }
                        UpdateInfo result = new TaskAwaiter<UpdateInfo>().GetResult();
                        this.<>7__wrap1.updateInfo = result;
                        this.<>7__wrap1 = null;
                        awaiter2 = this.<>8__1.This.ErrorIfThrows(new Func<Task>(this.<>8__1.<UpdateApp>b__3), "Failed to download updates").GetAwaiter();
                        if (!awaiter2.IsCompleted)
                        {
                            this.<>1__state = num = 1;
                            this.<>u__2 = awaiter2;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, EasyModeMixin.<UpdateApp>d__0>(ref awaiter2, ref this);
                            return;
                        }
                        goto TR_000E;
                    TR_000B:
                        awaiter4.GetResult();
                        awaiter4 = new TaskAwaiter<RegistryKey>();
                        goto TR_000A;
                    TR_000C:
                        awaiter3.GetResult();
                        awaiter3 = new TaskAwaiter<string>();
                        awaiter4 = this.<>8__1.This.ErrorIfThrows<RegistryKey>(new Func<Task<RegistryKey>>(this.<>8__1.<UpdateApp>b__7), "Failed to set up uninstaller").GetAwaiter();
                        if (awaiter4.IsCompleted)
                        {
                            goto TR_000B;
                        }
                        else
                        {
                            this.<>1__state = num = 3;
                            this.<>u__4 = awaiter4;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<RegistryKey>, EasyModeMixin.<UpdateApp>d__0>(ref awaiter4, ref this);
                        }
                        return;
                    TR_000E:
                        awaiter2.GetResult();
                        awaiter2 = new TaskAwaiter();
                        awaiter3 = this.<>8__1.This.ErrorIfThrows<string>(new Func<Task<string>>(this.<>8__1.<UpdateApp>b__5), "Failed to apply updates").GetAwaiter();
                        if (!awaiter3.IsCompleted)
                        {
                            this.<>1__state = num = 2;
                            this.<>u__3 = awaiter3;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<string>, EasyModeMixin.<UpdateApp>d__0>(ref awaiter3, ref this);
                        }
                        else
                        {
                            goto TR_000C;
                        }
                    }
                    catch (Exception)
                    {
                        if (this.<>8__1.ignoreDeltaUpdates)
                        {
                            throw;
                        }
                        this.<>8__1.ignoreDeltaUpdates = true;
                        goto TR_001B;
                    }
                    return;
                TR_001B:
                    while (true)
                    {
                        this.<>8__1.updateInfo = null;
                        break;
                    }
                    goto TR_0019;
                }
                catch (Exception exception)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception);
                }
            }

            [DebuggerHidden]
            private void SetStateMachine(IAsyncStateMachine stateMachine)
            {
                this.<>t__builder.SetStateMachine(stateMachine);
            }
        }
    }
}

