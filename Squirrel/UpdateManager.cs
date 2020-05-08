namespace Squirrel
{
    using ICSharpCode.SharpZipLib.Zip;
    using Microsoft.Win32;
    using NuGet;
    using Splat;
    using Squirrel.Json;
    using Squirrel.Shell;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal sealed class UpdateManager : IUpdateManager, IDisposable, IEnableLogger
    {
        private readonly string rootAppDirectory;
        private readonly string applicationName;
        private readonly IFileDownloader urlDownloader;
        private readonly string updateUrlOrPath;
        private IDisposable updateLock;
        private static bool exiting;
        private const string gitHubUrl = "https://api.github.com";

        public UpdateManager(string urlOrPath, string applicationName = null, string rootDirectory = null, IFileDownloader urlDownloader = null)
        {
            this.updateUrlOrPath = urlOrPath;
            this.applicationName = applicationName ?? getApplicationName();
            this.urlDownloader = urlDownloader ?? new FileDownloader(null);
            if (rootDirectory != null)
            {
                this.rootAppDirectory = Path.Combine(rootDirectory, this.applicationName);
            }
            else
            {
                this.rootAppDirectory = Path.Combine(rootDirectory ?? GetLocalAppDataDirectory(null), this.applicationName);
            }
        }

        private Task<IDisposable> acquireUpdateLock() => 
            ((this.updateLock == null) ? Task.Run<IDisposable>(delegate {
                IDisposable theLock;
                string key = Utility.CalculateStreamSHA1(new MemoryStream(Encoding.UTF8.GetBytes(this.rootAppDirectory)));
                try
                {
                    theLock = ModeDetector.InUnitTestRunner() ? Disposable.Create(delegate {
                    }) : new SingleGlobalInstance(key, TimeSpan.FromMilliseconds(10000.0));
                }
                catch (TimeoutException)
                {
                    throw new TimeoutException("Couldn't acquire update lock, another instance may be running updates");
                }
                IDisposable disposable = Disposable.Create(delegate {
                    theLock.Dispose();
                    this.updateLock = null;
                });
                this.updateLock = disposable;
                return disposable;
            }) : Task.FromResult<IDisposable>(this.updateLock));

        [AsyncStateMachine(typeof(<ApplyReleases>d__9))]
        public Task<string> ApplyReleases(UpdateInfo updateInfo, Action<int> progress = null)
        {
            <ApplyReleases>d__9 d__;
            d__.<>4__this = this;
            d__.updateInfo = updateInfo;
            d__.progress = progress;
            d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<ApplyReleases>d__9>(ref d__);
            return d__.<>t__builder.Task;
        }

        [AsyncStateMachine(typeof(<CheckForUpdate>d__7))]
        public Task<UpdateInfo> CheckForUpdate(bool ignoreDeltaUpdates = false, Action<int> progress = null)
        {
            <CheckForUpdate>d__7 d__;
            d__.<>4__this = this;
            d__.ignoreDeltaUpdates = ignoreDeltaUpdates;
            d__.progress = progress;
            d__.<>t__builder = AsyncTaskMethodBuilder<UpdateInfo>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<CheckForUpdate>d__7>(ref d__);
            return d__.<>t__builder.Task;
        }

        public void CreateShortcutsForExecutable(string exeName, ShortcutLocation locations, bool updateOnly, string programArguments = null, string icon = null)
        {
            new ApplyReleasesImpl(this.rootAppDirectory).CreateShortcutsForExecutable(exeName, locations, updateOnly, programArguments, icon);
        }

        public Task<RegistryKey> CreateUninstallerRegistryEntry() => 
            new InstallHelperImpl(this.applicationName, this.rootAppDirectory).CreateUninstallerRegistryEntry();

        public Task<RegistryKey> CreateUninstallerRegistryEntry(string uninstallCmd, string quietSwitch) => 
            new InstallHelperImpl(this.applicationName, this.rootAppDirectory).CreateUninstallerRegistryEntry(uninstallCmd, quietSwitch);

        public SemanticVersion CurrentlyInstalledVersion(string executable = null)
        {
            executable = executable ?? Path.GetDirectoryName(typeof(UpdateManager).Assembly.Location);
            if (!executable.StartsWith(this.rootAppDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            char[] separator = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            string fileName = Enumerable.FirstOrDefault<string>(executable.Split(separator), x => x.StartsWith("app-", StringComparison.OrdinalIgnoreCase));
            return ((fileName != null) ? fileName.ToSemanticVersion() : null);
        }

        public void Dispose()
        {
            IDisposable disposable = Interlocked.Exchange<IDisposable>(ref this.updateLock, null);
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        [AsyncStateMachine(typeof(<DownloadReleases>d__8))]
        public Task DownloadReleases(IEnumerable<ReleaseEntry> releasesToDownload, Action<int> progress = null)
        {
            <DownloadReleases>d__8 d__;
            d__.<>4__this = this;
            d__.releasesToDownload = releasesToDownload;
            d__.progress = progress;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<DownloadReleases>d__8>(ref d__);
            return d__.<>t__builder.Task;
        }

        ~UpdateManager()
        {
            if ((this.updateLock != null) && !exiting)
            {
                throw new Exception("You must dispose UpdateManager!");
            }
        }

        [AsyncStateMachine(typeof(<FullInstall>d__10))]
        public Task FullInstall(bool silentInstall = false, Action<int> progress = null)
        {
            <FullInstall>d__10 d__;
            d__.<>4__this = this;
            d__.silentInstall = silentInstall;
            d__.progress = progress;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<FullInstall>d__10>(ref d__);
            return d__.<>t__builder.Task;
        }

        [AsyncStateMachine(typeof(<FullUninstall>d__11))]
        public Task FullUninstall()
        {
            <FullUninstall>d__11 d__;
            d__.<>4__this = this;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<FullUninstall>d__11>(ref d__);
            return d__.<>t__builder.Task;
        }

        private static string getApplicationName() => 
            new FileInfo(getUpdateExe()).Directory.Name;

        public static string GetLocalAppDataDirectory(string assemblyLocation = null)
        {
            string assembyLocation = Utility.GetAssembyLocation();
            if ((assemblyLocation == null) && (assembyLocation == null))
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            assemblyLocation = assemblyLocation ?? assembyLocation;
            return (!Path.GetFileName(assemblyLocation).Equals("update.exe", StringComparison.OrdinalIgnoreCase) ? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(assemblyLocation), @"..\..")) : Path.GetFullPath(Path.Combine(Path.GetDirectoryName(assemblyLocation), "..")));
        }

        public Dictionary<ShortcutLocation, ShellLink> GetShortcutsForExecutable(string exeName, ShortcutLocation locations, string programArguments = null) => 
            new ApplyReleasesImpl(this.rootAppDirectory).GetShortcutsForExecutable(exeName, locations, programArguments);

        private static string getUpdateExe()
        {
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if ((entryAssembly != null) && (Path.GetFileName(entryAssembly.Location).Equals("update.exe", StringComparison.OrdinalIgnoreCase) && ((entryAssembly.Location.IndexOf("app-", StringComparison.OrdinalIgnoreCase) == -1) && (entryAssembly.Location.IndexOf("SquirrelTemp", StringComparison.OrdinalIgnoreCase) == -1))))
            {
                return Path.GetFullPath(entryAssembly.Location);
            }
            FileInfo info1 = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"..\Update.exe"));
            if (!info1.Exists)
            {
                throw new Exception("Update.exe not found, not a Squirrel-installed app?");
            }
            return info1.FullName;
        }

        [AsyncStateMachine(typeof(<GitHubUpdateManager>d__35))]
        public static Task<UpdateManager> GitHubUpdateManager(string repoUrl, string applicationName = null, string rootDirectory = null, IFileDownloader urlDownloader = null, bool prerelease = false, string accessToken = null)
        {
            <GitHubUpdateManager>d__35 d__;
            d__.repoUrl = repoUrl;
            d__.applicationName = applicationName;
            d__.rootDirectory = rootDirectory;
            d__.urlDownloader = urlDownloader;
            d__.prerelease = prerelease;
            d__.accessToken = accessToken;
            d__.<>t__builder = AsyncTaskMethodBuilder<UpdateManager>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<GitHubUpdateManager>d__35>(ref d__);
            return d__.<>t__builder.Task;
        }

        public void RemoveShortcutsForExecutable(string exeName, ShortcutLocation locations)
        {
            new ApplyReleasesImpl(this.rootAppDirectory).RemoveShortcutsForExecutable(exeName, locations);
        }

        public void RemoveUninstallerRegistryEntry()
        {
            new InstallHelperImpl(this.applicationName, this.rootAppDirectory).RemoveUninstallerRegistryEntry();
        }

        public static void RestartApp(string exeToStart = null, string arguments = null)
        {
            exeToStart = exeToStart ?? Path.GetFileName(Assembly.GetEntryAssembly().Location);
            string str = (arguments != null) ? $"-a "{arguments}"" : "";
            exiting = true;
            Process.Start(getUpdateExe(), $"--processStartAndWait {exeToStart} {str}");
            Thread.Sleep(500);
            Environment.Exit(0);
        }

        public string ApplicationName =>
            this.applicationName;

        public string RootAppDirectory =>
            this.rootAppDirectory;

        public bool IsInstalledApp =>
            Assembly.GetExecutingAssembly().Location.StartsWith(this.RootAppDirectory, StringComparison.OrdinalIgnoreCase);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly UpdateManager.<>c <>9 = new UpdateManager.<>c();
            public static Func<string, bool> <>9__18_0;
            public static Action <>9__30_1;
            public static Func<UpdateManager.Release, DateTime> <>9__35_1;

            internal void <acquireUpdateLock>b__30_1()
            {
            }

            internal bool <CurrentlyInstalledVersion>b__18_0(string x) => 
                x.StartsWith("app-", StringComparison.OrdinalIgnoreCase);

            internal DateTime <GitHubUpdateManager>b__35_1(UpdateManager.Release x) => 
                x.PublishedAt;
        }

        [CompilerGenerated]
        private struct <ApplyReleases>d__9 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<string> <>t__builder;
            public UpdateManager <>4__this;
            private UpdateManager.ApplyReleasesImpl <applyReleases>5__1;
            public UpdateInfo updateInfo;
            public Action<int> progress;
            private TaskAwaiter<IDisposable> <>u__1;
            private TaskAwaiter<string> <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    TaskAwaiter<IDisposable> awaiter;
                    TaskAwaiter<string> awaiter2;
                    if (num == 0)
                    {
                        awaiter = this.<>u__1;
                        this.<>u__1 = new TaskAwaiter<IDisposable>();
                        this.<>1__state = num = -1;
                    }
                    else if (num == 1)
                    {
                        awaiter2 = this.<>u__2;
                        this.<>u__2 = new TaskAwaiter<string>();
                        this.<>1__state = num = -1;
                        goto TR_0005;
                    }
                    else
                    {
                        this.<applyReleases>5__1 = new UpdateManager.ApplyReleasesImpl(this.<>4__this.rootAppDirectory);
                        awaiter = this.<>4__this.acquireUpdateLock().GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<IDisposable>, UpdateManager.<ApplyReleases>d__9>(ref awaiter, ref this);
                            return;
                        }
                    }
                    awaiter.GetResult();
                    awaiter = new TaskAwaiter<IDisposable>();
                    awaiter2 = this.<applyReleases>5__1.ApplyReleases(this.updateInfo, false, false, this.progress).GetAwaiter();
                    if (awaiter2.IsCompleted)
                    {
                        goto TR_0005;
                    }
                    else
                    {
                        this.<>1__state = num = 1;
                        this.<>u__2 = awaiter2;
                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<string>, UpdateManager.<ApplyReleases>d__9>(ref awaiter2, ref this);
                    }
                    return;
                TR_0005:
                    awaiter2 = new TaskAwaiter<string>();
                    string result = awaiter2.GetResult();
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult(result);
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

        [CompilerGenerated]
        private struct <CheckForUpdate>d__7 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<UpdateInfo> <>t__builder;
            public UpdateManager <>4__this;
            private UpdateManager.CheckForUpdateImpl <checkForUpdate>5__1;
            public bool ignoreDeltaUpdates;
            public Action<int> progress;
            private TaskAwaiter<IDisposable> <>u__1;
            private TaskAwaiter<UpdateInfo> <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    TaskAwaiter<IDisposable> awaiter;
                    TaskAwaiter<UpdateInfo> awaiter2;
                    if (num == 0)
                    {
                        awaiter = this.<>u__1;
                        this.<>u__1 = new TaskAwaiter<IDisposable>();
                        this.<>1__state = num = -1;
                    }
                    else if (num == 1)
                    {
                        awaiter2 = this.<>u__2;
                        this.<>u__2 = new TaskAwaiter<UpdateInfo>();
                        this.<>1__state = num = -1;
                        goto TR_0005;
                    }
                    else
                    {
                        this.<checkForUpdate>5__1 = new UpdateManager.CheckForUpdateImpl(this.<>4__this.rootAppDirectory);
                        awaiter = this.<>4__this.acquireUpdateLock().GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<IDisposable>, UpdateManager.<CheckForUpdate>d__7>(ref awaiter, ref this);
                            return;
                        }
                    }
                    awaiter.GetResult();
                    awaiter = new TaskAwaiter<IDisposable>();
                    awaiter2 = this.<checkForUpdate>5__1.CheckForUpdate(Utility.LocalReleaseFileForAppDir(this.<>4__this.rootAppDirectory), this.<>4__this.updateUrlOrPath, this.ignoreDeltaUpdates, this.progress, this.<>4__this.urlDownloader).GetAwaiter();
                    if (awaiter2.IsCompleted)
                    {
                        goto TR_0005;
                    }
                    else
                    {
                        this.<>1__state = num = 1;
                        this.<>u__2 = awaiter2;
                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<UpdateInfo>, UpdateManager.<CheckForUpdate>d__7>(ref awaiter2, ref this);
                    }
                    return;
                TR_0005:
                    awaiter2 = new TaskAwaiter<UpdateInfo>();
                    UpdateInfo result = awaiter2.GetResult();
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult(result);
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

        [CompilerGenerated]
        private struct <DownloadReleases>d__8 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public UpdateManager <>4__this;
            private UpdateManager.DownloadReleasesImpl <downloadReleases>5__1;
            public IEnumerable<ReleaseEntry> releasesToDownload;
            public Action<int> progress;
            private TaskAwaiter<IDisposable> <>u__1;
            private TaskAwaiter <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    TaskAwaiter<IDisposable> awaiter;
                    TaskAwaiter awaiter2;
                    if (num == 0)
                    {
                        awaiter = this.<>u__1;
                        this.<>u__1 = new TaskAwaiter<IDisposable>();
                        this.<>1__state = num = -1;
                    }
                    else if (num == 1)
                    {
                        awaiter2 = this.<>u__2;
                        this.<>u__2 = new TaskAwaiter();
                        this.<>1__state = num = -1;
                        goto TR_0005;
                    }
                    else
                    {
                        this.<downloadReleases>5__1 = new UpdateManager.DownloadReleasesImpl(this.<>4__this.rootAppDirectory);
                        awaiter = this.<>4__this.acquireUpdateLock().GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<IDisposable>, UpdateManager.<DownloadReleases>d__8>(ref awaiter, ref this);
                            return;
                        }
                    }
                    awaiter.GetResult();
                    awaiter = new TaskAwaiter<IDisposable>();
                    awaiter2 = this.<downloadReleases>5__1.DownloadReleases(this.<>4__this.updateUrlOrPath, this.releasesToDownload, this.progress, this.<>4__this.urlDownloader).GetAwaiter();
                    if (awaiter2.IsCompleted)
                    {
                        goto TR_0005;
                    }
                    else
                    {
                        this.<>1__state = num = 1;
                        this.<>u__2 = awaiter2;
                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.<DownloadReleases>d__8>(ref awaiter2, ref this);
                    }
                    return;
                TR_0005:
                    awaiter2.GetResult();
                    awaiter2 = new TaskAwaiter();
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
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

        [CompilerGenerated]
        private struct <FullInstall>d__10 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public UpdateManager <>4__this;
            private UpdateManager.ApplyReleasesImpl <applyReleases>5__1;
            private UpdateInfo <updateInfo>5__2;
            public bool silentInstall;
            public Action<int> progress;
            private TaskAwaiter<UpdateInfo> <>u__1;
            private TaskAwaiter <>u__2;
            private TaskAwaiter<IDisposable> <>u__3;
            private TaskAwaiter<string> <>u__4;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    TaskAwaiter<UpdateInfo> awaiter;
                    TaskAwaiter awaiter2;
                    TaskAwaiter<IDisposable> awaiter3;
                    TaskAwaiter<string> awaiter4;
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
                            goto TR_000A;

                        case 2:
                            awaiter3 = this.<>u__3;
                            this.<>u__3 = new TaskAwaiter<IDisposable>();
                            this.<>1__state = num = -1;
                            goto TR_0008;

                        case 3:
                            awaiter4 = this.<>u__4;
                            this.<>u__4 = new TaskAwaiter<string>();
                            this.<>1__state = num = -1;
                            goto TR_0007;

                        default:
                            awaiter = this.<>4__this.CheckForUpdate(false, null).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                break;
                            }
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<UpdateInfo>, UpdateManager.<FullInstall>d__10>(ref awaiter, ref this);
                            return;
                    }
                    UpdateInfo result = new TaskAwaiter<UpdateInfo>().GetResult();
                    this.<updateInfo>5__2 = result;
                    awaiter2 = this.<>4__this.DownloadReleases(this.<updateInfo>5__2.ReleasesToApply, null).GetAwaiter();
                    if (!awaiter2.IsCompleted)
                    {
                        this.<>1__state = num = 1;
                        this.<>u__2 = awaiter2;
                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.<FullInstall>d__10>(ref awaiter2, ref this);
                        return;
                    }
                    goto TR_000A;
                TR_0007:
                    awaiter4.GetResult();
                    awaiter4 = new TaskAwaiter<string>();
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                    return;
                TR_0008:
                    awaiter3.GetResult();
                    awaiter3 = new TaskAwaiter<IDisposable>();
                    awaiter4 = this.<applyReleases>5__1.ApplyReleases(this.<updateInfo>5__2, this.silentInstall, true, this.progress).GetAwaiter();
                    if (awaiter4.IsCompleted)
                    {
                        goto TR_0007;
                    }
                    else
                    {
                        this.<>1__state = num = 3;
                        this.<>u__4 = awaiter4;
                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<string>, UpdateManager.<FullInstall>d__10>(ref awaiter4, ref this);
                    }
                    return;
                TR_000A:
                    awaiter2.GetResult();
                    awaiter2 = new TaskAwaiter();
                    this.<applyReleases>5__1 = new UpdateManager.ApplyReleasesImpl(this.<>4__this.rootAppDirectory);
                    awaiter3 = this.<>4__this.acquireUpdateLock().GetAwaiter();
                    if (!awaiter3.IsCompleted)
                    {
                        this.<>1__state = num = 2;
                        this.<>u__3 = awaiter3;
                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<IDisposable>, UpdateManager.<FullInstall>d__10>(ref awaiter3, ref this);
                    }
                    else
                    {
                        goto TR_0008;
                    }
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

        [CompilerGenerated]
        private struct <FullUninstall>d__11 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public UpdateManager <>4__this;
            private UpdateManager.ApplyReleasesImpl <applyReleases>5__1;
            private TaskAwaiter<IDisposable> <>u__1;
            private TaskAwaiter <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    TaskAwaiter<IDisposable> awaiter;
                    TaskAwaiter awaiter2;
                    if (num == 0)
                    {
                        awaiter = this.<>u__1;
                        this.<>u__1 = new TaskAwaiter<IDisposable>();
                        this.<>1__state = num = -1;
                    }
                    else if (num == 1)
                    {
                        awaiter2 = this.<>u__2;
                        this.<>u__2 = new TaskAwaiter();
                        this.<>1__state = num = -1;
                        goto TR_0005;
                    }
                    else
                    {
                        this.<applyReleases>5__1 = new UpdateManager.ApplyReleasesImpl(this.<>4__this.rootAppDirectory);
                        awaiter = this.<>4__this.acquireUpdateLock().GetAwaiter();
                        if (!awaiter.IsCompleted)
                        {
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<IDisposable>, UpdateManager.<FullUninstall>d__11>(ref awaiter, ref this);
                            return;
                        }
                    }
                    awaiter.GetResult();
                    awaiter = new TaskAwaiter<IDisposable>();
                    awaiter2 = this.<applyReleases>5__1.FullUninstall().GetAwaiter();
                    if (awaiter2.IsCompleted)
                    {
                        goto TR_0005;
                    }
                    else
                    {
                        this.<>1__state = num = 1;
                        this.<>u__2 = awaiter2;
                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.<FullUninstall>d__11>(ref awaiter2, ref this);
                    }
                    return;
                TR_0005:
                    awaiter2.GetResult();
                    awaiter2 = new TaskAwaiter();
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
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

        [CompilerGenerated]
        private struct <GitHubUpdateManager>d__35 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<UpdateManager> <>t__builder;
            public bool prerelease;
            public string repoUrl;
            public string accessToken;
            private UpdateManager.<>c__DisplayClass35_0 <>8__1;
            public string applicationName;
            public string rootDirectory;
            public IFileDownloader urlDownloader;
            private HttpClient <client>5__2;
            private TaskAwaiter<HttpResponseMessage> <>u__1;
            private TaskAwaiter<string> <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    ProductInfoHeaderValue value2;
                    StringBuilder builder;
                    if ((num != 0) && (num != 1))
                    {
                        this.<>8__1 = new UpdateManager.<>c__DisplayClass35_0();
                        this.<>8__1.prerelease = this.prerelease;
                        Uri uri = new Uri(this.repoUrl);
                        value2 = new ProductInfoHeaderValue("Squirrel", Assembly.GetExecutingAssembly().GetName().Version.ToString());
                        if (uri.Segments.Count<string>() != 3)
                        {
                            throw new Exception("Repo URL must be to the root URL of the repo e.g. https://github.com/myuser/myrepo");
                        }
                        builder = new StringBuilder("/repos").Append(uri.AbsolutePath).Append("/releases");
                        if (!string.IsNullOrWhiteSpace(this.accessToken))
                        {
                            builder.Append("?access_token=").Append(this.accessToken);
                        }
                        HttpClient client1 = new HttpClient();
                        client1.BaseAddress = new Uri("https://api.github.com");
                        this.<client>5__2 = client1;
                    }
                    try
                    {
                        TaskAwaiter<HttpResponseMessage> awaiter;
                        TaskAwaiter<string> awaiter2;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter<HttpResponseMessage>();
                            this.<>1__state = num = -1;
                        }
                        else if (num == 1)
                        {
                            awaiter2 = this.<>u__2;
                            this.<>u__2 = new TaskAwaiter<string>();
                            this.<>1__state = num = -1;
                            goto TR_0009;
                        }
                        else
                        {
                            this.<client>5__2.DefaultRequestHeaders.UserAgent.Add(value2);
                            awaiter = this.<client>5__2.GetAsync(builder.ToString()).GetAwaiter();
                            if (!awaiter.IsCompleted)
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<HttpResponseMessage>, UpdateManager.<GitHubUpdateManager>d__35>(ref awaiter, ref this);
                                return;
                            }
                        }
                        HttpResponseMessage result = new TaskAwaiter<HttpResponseMessage>().GetResult();
                        result.EnsureSuccessStatusCode();
                        awaiter2 = result.Content.ReadAsStringAsync().GetAwaiter();
                        if (awaiter2.IsCompleted)
                        {
                            goto TR_0009;
                        }
                        else
                        {
                            this.<>1__state = num = 1;
                            this.<>u__2 = awaiter2;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<string>, UpdateManager.<GitHubUpdateManager>d__35>(ref awaiter2, ref this);
                        }
                        return;
                    TR_0009:
                        awaiter2 = new TaskAwaiter<string>();
                        UpdateManager manager = new UpdateManager(Enumerable.OrderByDescending<UpdateManager.Release, DateTime>(Enumerable.Where<UpdateManager.Release>(SimpleJson.DeserializeObject<List<UpdateManager.Release>>(awaiter2.GetResult()), new Func<UpdateManager.Release, bool>(this.<>8__1.<GitHubUpdateManager>b__0)), UpdateManager.<>c.<>9__35_1 ?? (UpdateManager.<>c.<>9__35_1 = new Func<UpdateManager.Release, DateTime>(this.<GitHubUpdateManager>b__35_1))).First<UpdateManager.Release>().HtmlUrl.Replace("/tag/", "/download/"), this.applicationName, this.rootDirectory, this.urlDownloader);
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult(manager);
                    }
                    finally
                    {
                        if ((num < 0) && (this.<client>5__2 != null))
                        {
                            this.<client>5__2.Dispose();
                        }
                    }
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

        internal class ApplyReleasesImpl : IEnableLogger
        {
            private readonly string rootAppDirectory;

            public ApplyReleasesImpl(string rootAppDirectory)
            {
                this.rootAppDirectory = rootAppDirectory;
            }

            [AsyncStateMachine(typeof(<ApplyReleases>d__2))]
            public Task<string> ApplyReleases(UpdateInfo updateInfo, bool silentInstall, bool attemptingFullInstall, Action<int> progress = null)
            {
                <ApplyReleases>d__2 d__;
                d__.<>4__this = this;
                d__.updateInfo = updateInfo;
                d__.silentInstall = silentInstall;
                d__.attemptingFullInstall = attemptingFullInstall;
                d__.progress = progress;
                d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
                d__.<>1__state = -1;
                d__.<>t__builder.Start<<ApplyReleases>d__2>(ref d__);
                return d__.<>t__builder.Task;
            }

            [AsyncStateMachine(typeof(<cleanDeadVersions>d__14))]
            private Task cleanDeadVersions(SemanticVersion originalVersion, SemanticVersion currentVersion, bool forceUninstall = false)
            {
                <cleanDeadVersions>d__14 d__;
                d__.<>4__this = this;
                d__.originalVersion = originalVersion;
                d__.currentVersion = currentVersion;
                d__.forceUninstall = forceUninstall;
                d__.<>t__builder = AsyncTaskMethodBuilder.Create();
                d__.<>1__state = -1;
                d__.<>t__builder.Start<<cleanDeadVersions>d__14>(ref d__);
                return d__.<>t__builder.Task;
            }

            [AsyncStateMachine(typeof(<createFullPackagesFromDeltas>d__8))]
            private Task<ReleaseEntry> createFullPackagesFromDeltas(IEnumerable<ReleaseEntry> releasesToApply, ReleaseEntry currentVersion)
            {
                <createFullPackagesFromDeltas>d__8 d__;
                d__.<>4__this = this;
                d__.releasesToApply = releasesToApply;
                d__.currentVersion = currentVersion;
                d__.<>t__builder = AsyncTaskMethodBuilder<ReleaseEntry>.Create();
                d__.<>1__state = -1;
                d__.<>t__builder.Start<<createFullPackagesFromDeltas>d__8>(ref d__);
                return d__.<>t__builder.Task;
            }

            public void CreateShortcutsForExecutable(string exeName, ShortcutLocation locations, bool updateOnly, string programArguments, string icon)
            {
                this.Log<UpdateManager.ApplyReleasesImpl>().Info<string, string>("About to create shortcuts for {0}, rootAppDir {1}", exeName, this.rootAppDirectory);
                ReleaseEntry entry = Utility.FindCurrentVersion(Utility.LoadLocalReleases(Utility.LocalReleaseFileForAppDir(this.rootAppDirectory)));
                string updateExe = Path.Combine(this.rootAppDirectory, "update.exe");
                ZipPackage zf = new ZipPackage(Path.Combine(Utility.PackageDirectoryForAppDir(this.rootAppDirectory), entry.Filename));
                string exePath = Path.Combine(Utility.AppDirForRelease(this.rootAppDirectory, entry), exeName);
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(exePath);
                foreach (ShortcutLocation location in (ShortcutLocation[]) Enum.GetValues(typeof(ShortcutLocation)))
                {
                    if (updateOnly || locations.HasFlag(location))
                    {
                        string file = this.linkTargetForVersionInfo(location, zf, versionInfo);
                        if (!(!File.Exists(file) & updateOnly))
                        {
                            this.Log<UpdateManager.ApplyReleasesImpl>().Info<string, string>("Creating shortcut for {0} => {1}", exeName, file);
                            this.WarnIfThrows(delegate {
                                File.Delete(file);
                                ShellLink link1 = new ShellLink();
                                link1.Target = updateExe;
                                link1.IconPath = icon ?? exePath;
                                ShellLink local2 = link1;
                                local2.IconIndex = 0;
                                local2.WorkingDirectory = Path.GetDirectoryName(exePath);
                                local2.Description = zf.Description;
                                local2.Arguments = "--processStart " + exeName;
                                ShellLink sl = local2;
                                if (!string.IsNullOrWhiteSpace(programArguments))
                                {
                                    sl.Arguments = sl.Arguments + $" -a "{programArguments}"";
                                }
                                sl.SetAppUserModelId($"com.squirrel.{zf.Id}.{exeName.Replace(".exe", "")}");
                                object[] args = new object[] { file, sl.Target, sl.WorkingDirectory, sl.Arguments };
                                this.Log<UpdateManager.ApplyReleasesImpl>().Info("About to save shortcut: {0} (target {1}, workingDir {2}, args {3})", args);
                                if (!ModeDetector.InUnitTestRunner())
                                {
                                    sl.Save(file);
                                }
                            }, "Can't write shortcut: " + file);
                        }
                        else if (locations.HasFlag(location))
                        {
                            this.Log<UpdateManager.ApplyReleasesImpl>().Warn<string>("Wanted to update shortcut {0} but it appears user deleted it", file);
                        }
                    }
                }
                this.fixPinnedExecutables(zf.Version);
            }

            private void executeSelfUpdate(SemanticVersion currentVersion)
            {
                DirectoryInfo targetDir = this.getDirectoryForRelease(currentVersion);
                string newSquirrel = Path.Combine(targetDir.FullName, "Squirrel.exe");
                if (File.Exists(newSquirrel))
                {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();
                    if ((entryAssembly == null) || !Path.GetFileName(entryAssembly.Location).Equals("update.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        () => File.Copy(newSquirrel, Path.Combine(targetDir.Parent.FullName, "Update.exe"), true).Retry(2);
                    }
                    else
                    {
                        string name = targetDir.Parent.Name;
                        Process process = Process.Start(newSquirrel, "--updateSelf=" + entryAssembly.Location);
                        this.Log<UpdateManager.ApplyReleasesImpl>().Info<int>("Started updateSelf pid {0}", process.Id);
                    }
                }
            }

            private void fixPinnedExecutables(SemanticVersion newCurrentVersion)
            {
                if (Environment.OSVersion.Version < new Version(6, 1))
                {
                    this.Log<UpdateManager.ApplyReleasesImpl>().Warn<string>("fixPinnedExecutables: Found OS Version '{0}', exiting...", Environment.OSVersion.VersionString);
                }
                else
                {
                    string argument = "app-" + newCurrentVersion.ToString();
                    this.Log<UpdateManager.ApplyReleasesImpl>().Info<string>("fixPinnedExecutables: newCurrentFolder: {0}", argument);
                    string path = Path.Combine(this.rootAppDirectory, argument);
                    bool newVersionExists = Directory.Exists(path);
                    Func<FileInfo, ShellLink> func = delegate (FileInfo file) {
                        try
                        {
                            return new ShellLink(file.FullName);
                        }
                        catch (Exception exception1)
                        {
                            string message = $"File '{file.FullName}' could not be converted into a valid ShellLink";
                            this.Log<UpdateManager.ApplyReleasesImpl>().WarnException(message, exception1);
                            return null;
                        }
                    };
                    Tuple<string, SearchOption> tuple = new Tuple<string, SearchOption>(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar"), SearchOption.TopDirectoryOnly);
                    Tuple<string, SearchOption> tuple2 = new Tuple<string, SearchOption>(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), SearchOption.AllDirectories);
                    Tuple<string, SearchOption> tuple3 = new Tuple<string, SearchOption>(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), SearchOption.TopDirectoryOnly);
                    Tuple<string, SearchOption>[] tupleArray = new Tuple<string, SearchOption>[] { tuple, tuple2, tuple3 };
                    int index = 0;
                    while (true)
                    {
                        while (true)
                        {
                            if (index >= tupleArray.Length)
                            {
                                return;
                            }
                            Tuple<string, SearchOption> tuple4 = tupleArray[index];
                            if (!Directory.Exists(tuple4.Item1))
                            {
                                this.Log<UpdateManager.ApplyReleasesImpl>().Warn<string>("Skipping {0}, since it does not exist!", tuple4.Item1);
                            }
                            else
                            {
                                ShellLink[] linkArray;
                                try
                                {
                                    linkArray = (from x in Enumerable.Select<FileInfo, ShellLink>(new DirectoryInfo(tuple4.Item1).GetFiles("*.lnk", tuple4.Item2), func)
                                        where x != null
                                        select x).ToArray<ShellLink>();
                                }
                                catch (Exception exception1)
                                {
                                    string message = $"fixPinnedExecutables: enumerating path {tuple4.Item1} failed";
                                    this.Log<UpdateManager.ApplyReleasesImpl>().WarnException(message, exception1);
                                    break;
                                }
                                foreach (ShellLink link in linkArray)
                                {
                                    try
                                    {
                                        this.updateLink(link, path, newVersionExists);
                                    }
                                    catch (Exception exception3)
                                    {
                                        string message = $"fixPinnedExecutables: shortcut failed: {link.Target}";
                                        this.Log<UpdateManager.ApplyReleasesImpl>().ErrorException(message, exception3);
                                    }
                                }
                            }
                            break;
                        }
                        index++;
                    }
                }
            }

            [AsyncStateMachine(typeof(<FullUninstall>d__3))]
            public Task FullUninstall()
            {
                <FullUninstall>d__3 d__;
                d__.<>4__this = this;
                d__.<>t__builder = AsyncTaskMethodBuilder.Create();
                d__.<>1__state = -1;
                d__.<>t__builder.Start<<FullUninstall>d__3>(ref d__);
                return d__.<>t__builder.Task;
            }

            private DirectoryInfo getDirectoryForRelease(SemanticVersion releaseVersion) => 
                new DirectoryInfo(Path.Combine(this.rootAppDirectory, "app-" + releaseVersion));

            private string getLinkTarget(ShortcutLocation location, string title, string applicationName, bool createDirectoryIfNecessary = true)
            {
                string path = null;
                switch (location)
                {
                    case ShortcutLocation.StartMenu:
                        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), applicationName);
                        break;

                    case ShortcutLocation.Desktop:
                        path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                        break;

                    case (ShortcutLocation.Desktop | ShortcutLocation.StartMenu):
                        break;

                    case ShortcutLocation.Startup:
                        path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                        break;

                    default:
                        if (location == ShortcutLocation.AppRoot)
                        {
                            path = this.rootAppDirectory;
                        }
                        else if (location == ShortcutLocation.Taskbar)
                        {
                            string[] paths = new string[] { Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "Internet Explorer", "Quick Launch", "User Pinned", "TaskBar" };
                            path = Path.Combine(paths);
                        }
                        break;
                }
                if (createDirectoryIfNecessary && !Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return Path.Combine(path, title + ".lnk");
            }

            private IEnumerable<DirectoryInfo> getReleases()
            {
                DirectoryInfo info = new DirectoryInfo(this.rootAppDirectory);
                return (info.Exists ? (from x in info.GetDirectories()
                    where x.Name.StartsWith("app-", StringComparison.InvariantCultureIgnoreCase)
                    select x) : Enumerable.Empty<DirectoryInfo>());
            }

            public Dictionary<ShortcutLocation, ShellLink> GetShortcutsForExecutable(string exeName, ShortcutLocation locations, string programArguments)
            {
                this.Log<UpdateManager.ApplyReleasesImpl>().Info<string, string>("About to create shortcuts for {0}, rootAppDir {1}", exeName, this.rootAppDirectory);
                ReleaseEntry entry = Utility.FindCurrentVersion(Utility.LoadLocalReleases(Utility.LocalReleaseFileForAppDir(this.rootAppDirectory)));
                string str = Path.Combine(this.rootAppDirectory, "update.exe");
                ZipPackage package = new ZipPackage(Path.Combine(Utility.PackageDirectoryForAppDir(this.rootAppDirectory), entry.Filename));
                string fileName = Path.Combine(Utility.AppDirForRelease(this.rootAppDirectory, entry), exeName);
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileName);
                Dictionary<ShortcutLocation, ShellLink> dictionary = new Dictionary<ShortcutLocation, ShellLink>();
                foreach (ShortcutLocation location in (ShortcutLocation[]) Enum.GetValues(typeof(ShortcutLocation)))
                {
                    if (locations.HasFlag(location))
                    {
                        string str3 = this.linkTargetForVersionInfo(location, package, versionInfo);
                        this.Log<UpdateManager.ApplyReleasesImpl>().Info<string, string>("Creating shortcut for {0} => {1}", exeName, str3);
                        ShellLink link1 = new ShellLink();
                        link1.Target = str;
                        link1.IconPath = fileName;
                        link1.IconIndex = 0;
                        link1.WorkingDirectory = Path.GetDirectoryName(fileName);
                        link1.Description = package.Description;
                        link1.Arguments = "--processStart " + exeName;
                        ShellLink link = link1;
                        if (!string.IsNullOrWhiteSpace(programArguments))
                        {
                            link.Arguments = link.Arguments + $" -a "{programArguments}"";
                        }
                        link.SetAppUserModelId($"com.squirrel.{package.Id}.{exeName.Replace(".exe", "")}");
                        dictionary.Add(location, link);
                    }
                }
                return dictionary;
            }

            private Task<string> installPackageToAppDir(UpdateInfo updateInfo, ReleaseEntry release) => 
                Task.Run<string>(delegate {
                    <>c__DisplayClass7_0.<<installPackageToAppDir>b__0>d local;
                    local.<>4__this = class_1;
                    local.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
                    local.<>1__state = -1;
                    local.<>t__builder.Start<<>c__DisplayClass7_0.<<installPackageToAppDir>b__0>d>(ref local);
                    return local.<>t__builder.Task;
                });

            [AsyncStateMachine(typeof(<invokePostInstall>d__10))]
            private Task invokePostInstall(SemanticVersion currentVersion, bool isInitialInstall, bool firstRunOnly, bool silentInstall)
            {
                <invokePostInstall>d__10 d__;
                d__.<>4__this = this;
                d__.currentVersion = currentVersion;
                d__.isInitialInstall = isInitialInstall;
                d__.firstRunOnly = firstRunOnly;
                d__.silentInstall = silentInstall;
                d__.<>t__builder = AsyncTaskMethodBuilder.Create();
                d__.<>1__state = -1;
                d__.<>t__builder.Start<<invokePostInstall>d__10>(ref d__);
                return d__.<>t__builder.Task;
            }

            private static bool isAppFolderDead(string appFolderPath) => 
                File.Exists(Path.Combine(appFolderPath, ".dead"));

            private string linkTargetForVersionInfo(ShortcutLocation location, IPackage package, FileVersionInfo versionInfo)
            {
                string[] strArray = new string[] { versionInfo.ProductName, package.Title, versionInfo.FileDescription, Path.GetFileNameWithoutExtension(versionInfo.FileName) };
                string[] textArray2 = new string[] { versionInfo.CompanyName, package.Authors.FirstOrDefault<string>() ?? package.Id };
                string applicationName = Enumerable.First<string>(textArray2, x => !string.IsNullOrWhiteSpace(x));
                string title = Enumerable.First<string>(strArray, x => !string.IsNullOrWhiteSpace(x));
                return this.getLinkTarget(location, title, applicationName, true);
            }

            private static void markAppFolderAsDead(string appFolderPath)
            {
                File.WriteAllText(Path.Combine(appFolderPath, ".dead"), "");
            }

            public void RemoveShortcutsForExecutable(string exeName, ShortcutLocation locations)
            {
                ReleaseEntry entry = Utility.FindCurrentVersion(Utility.LoadLocalReleases(Utility.LocalReleaseFileForAppDir(this.rootAppDirectory)));
                ZipPackage package = new ZipPackage(Path.Combine(Utility.PackageDirectoryForAppDir(this.rootAppDirectory), entry.Filename));
                FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(Utility.AppDirForRelease(this.rootAppDirectory, entry), exeName));
                foreach (ShortcutLocation location in (ShortcutLocation[]) Enum.GetValues(typeof(ShortcutLocation)))
                {
                    if (locations.HasFlag(location))
                    {
                        string file = this.linkTargetForVersionInfo(location, package, versionInfo);
                        this.Log<UpdateManager.ApplyReleasesImpl>().Info<string, string>("Removing shortcut for {0} => {1}", exeName, file);
                        this.ErrorIfThrows(delegate {
                            if (File.Exists(file))
                            {
                                File.Delete(file);
                            }
                        }, "Couldn't delete shortcut: " + file);
                    }
                }
                this.fixPinnedExecutables(package.Version);
            }

            private void updateLink(ShellLink shortcut, string newAppPath, bool newVersionExists)
            {
                string str = this.rootAppDirectory + @"\app-";
                if (shortcut.WorkingDirectory.StartsWith(str, StringComparison.OrdinalIgnoreCase))
                {
                    if (!newVersionExists)
                    {
                        this.Log<UpdateManager.ApplyReleasesImpl>().Info<string>("Unpinning {0} from taskbar", shortcut.ShortCutFile);
                        TaskbarHelper.UnpinFromTaskbar(shortcut.Target);
                    }
                    else
                    {
                        shortcut.Target = this.updatePath(shortcut.Target, newAppPath);
                        shortcut.WorkingDirectory = this.updatePath(shortcut.WorkingDirectory, newAppPath);
                        shortcut.IconPath = this.updatePath(shortcut.IconPath, newAppPath);
                        this.Log<UpdateManager.ApplyReleasesImpl>().Info<string>("Updating shortcut {0}", shortcut.ShortCutFile);
                        shortcut.Save();
                    }
                }
            }

            [AsyncStateMachine(typeof(<updateLocalReleasesFile>d__17))]
            internal Task<List<ReleaseEntry>> updateLocalReleasesFile()
            {
                <updateLocalReleasesFile>d__17 d__;
                d__.<>4__this = this;
                d__.<>t__builder = AsyncTaskMethodBuilder<List<ReleaseEntry>>.Create();
                d__.<>1__state = -1;
                d__.<>t__builder.Start<<updateLocalReleasesFile>d__17>(ref d__);
                return d__.<>t__builder.Task;
            }

            private string updatePath(string pathToUpdate, string newAppPath)
            {
                if (pathToUpdate.StartsWith(this.rootAppDirectory))
                {
                    char[] separator = new char[] { Path.DirectorySeparatorChar };
                    string[] paths = pathToUpdate.Substring(this.rootAppDirectory.Length + 1).Split(separator);
                    if (paths[0].StartsWith("app-"))
                    {
                        paths[0] = newAppPath;
                        pathToUpdate = Path.Combine(paths);
                    }
                }
                return pathToUpdate;
            }

            [CompilerGenerated]
            private struct <<cleanDeadVersions>b__14_3>d : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder <>t__builder;
                public UpdateManager.ApplyReleasesImpl <>4__this;
                public DirectoryInfo x;
                private TaskAwaiter <>u__1;

                private void MoveNext()
                {
                    int num = this.<>1__state;
                    try
                    {
                        TaskAwaiter awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                        }
                        else
                        {
                            UpdateManager.ApplyReleasesImpl <>4__this = this.<>4__this;
                            List<string> allSquirrelAwareApps = SquirrelAwareExecutableDetector.GetAllSquirrelAwareApps(this.x.FullName, 1);
                            string args = $"--squirrel-obsolete {this.x.Name.Replace("app-", "")}";
                            if (allSquirrelAwareApps.Count <= 0)
                            {
                                goto TR_0003;
                            }
                            else
                            {
                                UpdateManager.ApplyReleasesImpl.<>c__DisplayClass14_1 class_;
                                awaiter = allSquirrelAwareApps.ForEachAsync<string>(new Func<string, Task>(class_.<cleanDeadVersions>b__4), 1).GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    goto TR_0004;
                                }
                                else
                                {
                                    this.<>1__state = num = 0;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<<cleanDeadVersions>b__14_3>d>(ref awaiter, ref this);
                                }
                            }
                            return;
                        }
                    TR_0004:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                    }
                    catch (Exception exception)
                    {
                        this.<>1__state = -2;
                        this.<>t__builder.SetException(exception);
                        return;
                    }
                TR_0003:
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    this.<>t__builder.SetStateMachine(stateMachine);
                }
            }

            [CompilerGenerated]
            private struct <<cleanDeadVersions>b__14_7>d : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder <>t__builder;
                public DirectoryInfo x;
                public UpdateManager.ApplyReleasesImpl <>4__this;
                private TaskAwaiter <>u__1;

                private void MoveNext()
                {
                    int num = this.<>1__state;
                    try
                    {
                        int num1 = num;
                        try
                        {
                            TaskAwaiter awaiter;
                            if (num == 0)
                            {
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_0006;
                            }
                            else
                            {
                                awaiter = Utility.DeleteDirectoryOrJustGiveUp(this.x.FullName).GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    goto TR_0006;
                                }
                                else
                                {
                                    this.<>1__state = num = 0;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<<cleanDeadVersions>b__14_7>d>(ref awaiter, ref this);
                                }
                            }
                            return;
                        TR_0006:
                            awaiter.GetResult();
                            awaiter = new TaskAwaiter();
                            if (Directory.Exists(this.x.FullName))
                            {
                                UpdateManager.ApplyReleasesImpl.markAppFolderAsDead(this.x.FullName);
                            }
                        }
                        catch (UnauthorizedAccessException exception)
                        {
                            this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().WarnException("Couldn't delete directory: " + this.x.FullName, exception);
                            UpdateManager.ApplyReleasesImpl.markAppFolderAsDead(this.x.FullName);
                        }
                    }
                    catch (Exception exception2)
                    {
                        this.<>1__state = -2;
                        this.<>t__builder.SetException(exception2);
                        return;
                    }
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    this.<>t__builder.SetStateMachine(stateMachine);
                }
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly UpdateManager.ApplyReleasesImpl.<>c <>9 = new UpdateManager.ApplyReleasesImpl.<>c();
                public static Action<int> <>9__2_0;
                public static Func<ReleaseEntry, SemanticVersion> <>9__2_3;
                public static Func<FileInfo, string> <>9__2_6;
                public static Func<DirectoryInfo, SemanticVersion> <>9__3_0;
                public static Func<FileInfo, bool> <>9__3_1;
                public static Func<FileInfo, bool> <>9__3_2;
                public static Func<FileInfo, IEnumerable<Process>> <>9__3_5;
                public static Action<Process> <>9__3_6;
                public static FastZip.ConfirmOverwriteDelegate <>9__7_1;
                public static Func<DirectoryInfo, bool> <>9__7_2;
                public static Func<DirectoryInfo, string> <>9__7_3;
                public static Func<ReleaseEntry, bool> <>9__8_0;
                public static Func<ReleaseEntry, SemanticVersion> <>9__8_1;
                public static Func<ReleaseEntry, bool> <>9__8_2;
                public static Func<FileInfo, bool> <>9__10_1;
                public static Func<FileInfo, bool> <>9__10_2;
                public static Func<FileInfo, string> <>9__10_3;
                public static Func<ShellLink, bool> <>9__11_1;
                public static Func<DirectoryInfo, bool> <>9__14_0;
                public static Func<DirectoryInfo, bool> <>9__14_2;
                public static Func<DirectoryInfo, bool> <>9__14_5;
                public static Func<DirectoryInfo, bool> <>9__18_0;
                public static Func<string, bool> <>9__20_0;
                public static Func<string, bool> <>9__20_1;

                internal void <ApplyReleases>b__2_0(int _)
                {
                }

                internal SemanticVersion <ApplyReleases>b__2_3(ReleaseEntry x) => 
                    x.Version;

                internal string <ApplyReleases>b__2_6(FileInfo x) => 
                    x.Name;

                internal bool <cleanDeadVersions>b__14_0(DirectoryInfo x) => 
                    x.Name.ToLowerInvariant().Contains("app-");

                internal bool <cleanDeadVersions>b__14_2(DirectoryInfo x) => 
                    !UpdateManager.ApplyReleasesImpl.isAppFolderDead(x.FullName);

                internal bool <cleanDeadVersions>b__14_5(DirectoryInfo x) => 
                    x.Name.ToLowerInvariant().Contains("app-");

                internal bool <createFullPackagesFromDeltas>b__8_0(ReleaseEntry x) => 
                    !x.IsDelta;

                internal SemanticVersion <createFullPackagesFromDeltas>b__8_1(ReleaseEntry x) => 
                    x.Version;

                internal bool <createFullPackagesFromDeltas>b__8_2(ReleaseEntry x) => 
                    x.IsDelta;

                internal bool <fixPinnedExecutables>b__11_1(ShellLink x) => 
                    (x != null);

                internal SemanticVersion <FullUninstall>b__3_0(DirectoryInfo x) => 
                    x.Name.ToSemanticVersion();

                internal bool <FullUninstall>b__3_1(FileInfo x) => 
                    x.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);

                internal bool <FullUninstall>b__3_2(FileInfo x) => 
                    (!x.Name.StartsWith("squirrel.", StringComparison.OrdinalIgnoreCase) && !x.Name.StartsWith("update.", StringComparison.OrdinalIgnoreCase));

                internal IEnumerable<Process> <FullUninstall>b__3_5(FileInfo x) => 
                    Process.GetProcessesByName(x.Name.Replace(".exe", ""));

                internal void <FullUninstall>b__3_6(Process x)
                {
                    x.Kill();
                }

                internal bool <getReleases>b__18_0(DirectoryInfo x) => 
                    x.Name.StartsWith("app-", StringComparison.InvariantCultureIgnoreCase);

                internal bool <installPackageToAppDir>b__7_1(string o) => 
                    true;

                internal bool <installPackageToAppDir>b__7_2(DirectoryInfo x) => 
                    x.Name.Equals("lib", StringComparison.OrdinalIgnoreCase);

                internal string <installPackageToAppDir>b__7_3(DirectoryInfo x) => 
                    x.Name;

                internal bool <invokePostInstall>b__10_1(FileInfo x) => 
                    x.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);

                internal bool <invokePostInstall>b__10_2(FileInfo x) => 
                    !x.Name.StartsWith("squirrel.", StringComparison.OrdinalIgnoreCase);

                internal string <invokePostInstall>b__10_3(FileInfo x) => 
                    x.FullName;

                internal bool <linkTargetForVersionInfo>b__20_0(string x) => 
                    !string.IsNullOrWhiteSpace(x);

                internal bool <linkTargetForVersionInfo>b__20_1(string x) => 
                    !string.IsNullOrWhiteSpace(x);
            }

            [CompilerGenerated]
            private struct <ApplyReleases>d__2 : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder<string> <>t__builder;
                public UpdateManager.ApplyReleasesImpl <>4__this;
                public UpdateInfo updateInfo;
                public bool attemptingFullInstall;
                public bool silentInstall;
                public Action<int> progress;
                private UpdateManager.ApplyReleasesImpl.<>c__DisplayClass2_0 <>8__1;
                private string <ret>5__2;
                private UpdateManager.ApplyReleasesImpl.<>c__DisplayClass2_0 <>7__wrap1;
                private TaskAwaiter<ReleaseEntry> <>u__1;
                private TaskAwaiter <>u__2;
                private TaskAwaiter<string> <>u__3;
                private TaskAwaiter<List<ReleaseEntry>> <>u__4;

                private void MoveNext()
                {
                    string fullName;
                    int num = this.<>1__state;
                    try
                    {
                        TaskAwaiter<ReleaseEntry> awaiter;
                        TaskAwaiter awaiter2;
                        TaskAwaiter<string> awaiter3;
                        TaskAwaiter<List<ReleaseEntry>> awaiter4;
                        switch (num)
                        {
                            case 0:
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter<ReleaseEntry>();
                                this.<>1__state = num = -1;
                                break;

                            case 1:
                                awaiter2 = this.<>u__2;
                                this.<>u__2 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_0006;

                            case 2:
                                awaiter3 = this.<>u__3;
                                this.<>u__3 = new TaskAwaiter<string>();
                                this.<>1__state = num = -1;
                                goto TR_0019;

                            case 3:
                                awaiter4 = this.<>u__4;
                                this.<>u__4 = new TaskAwaiter<List<ReleaseEntry>>();
                                this.<>1__state = num = -1;
                                goto TR_0017;

                            case 4:
                                awaiter2 = this.<>u__2;
                                this.<>u__2 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_0016;

                            case 5:
                                goto TR_0015;

                            default:
                            {
                                this.<>8__1 = new UpdateManager.ApplyReleasesImpl.<>c__DisplayClass2_0();
                                this.<>8__1.<>4__this = this.<>4__this;
                                this.<>8__1.updateInfo = this.updateInfo;
                                this.<>8__1.attemptingFullInstall = this.attemptingFullInstall;
                                this.<>8__1.silentInstall = this.silentInstall;
                                this.progress = this.progress ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__2_0 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__2_0 = new Action<int>(this.<ApplyReleases>b__2_0)));
                                this.<>7__wrap1 = this.<>8__1;
                                ReleaseEntry release = this.<>7__wrap1.release;
                                awaiter = this.<>4__this.createFullPackagesFromDeltas(this.<>8__1.updateInfo.ReleasesToApply, this.<>8__1.updateInfo.CurrentlyInstalledVersion).GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    break;
                                }
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<ReleaseEntry>, UpdateManager.ApplyReleasesImpl.<ApplyReleases>d__2>(ref awaiter, ref this);
                                return;
                            }
                        }
                        ReleaseEntry result = new TaskAwaiter<ReleaseEntry>().GetResult();
                        this.<>7__wrap1.release = result;
                        this.<>7__wrap1 = null;
                        this.progress(10);
                        if (this.<>8__1.release != null)
                        {
                            awaiter3 = this.<>4__this.ErrorIfThrows<string>(new Func<Task<string>>(this.<>8__1.<ApplyReleases>b__1), "Failed to install package to app dir").GetAwaiter();
                            if (!awaiter3.IsCompleted)
                            {
                                this.<>1__state = num = 2;
                                this.<>u__3 = awaiter3;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<string>, UpdateManager.ApplyReleasesImpl.<ApplyReleases>d__2>(ref awaiter3, ref this);
                                return;
                            }
                        }
                        else
                        {
                            if (!this.<>8__1.attemptingFullInstall)
                            {
                                goto TR_0005;
                            }
                            else
                            {
                                this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().Info("No release to install, running the app");
                                awaiter2 = this.<>4__this.invokePostInstall(this.<>8__1.updateInfo.CurrentlyInstalledVersion.Version, false, true, this.<>8__1.silentInstall).GetAwaiter();
                                if (awaiter2.IsCompleted)
                                {
                                    goto TR_0006;
                                }
                                else
                                {
                                    this.<>1__state = num = 1;
                                    this.<>u__2 = awaiter2;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<ApplyReleases>d__2>(ref awaiter2, ref this);
                                }
                            }
                            return;
                        }
                        goto TR_0019;
                    TR_0005:
                        this.progress(100);
                        fullName = this.<>4__this.getDirectoryForRelease(this.<>8__1.updateInfo.CurrentlyInstalledVersion.Version).FullName;
                        goto TR_0004;
                    TR_0006:
                        awaiter2.GetResult();
                        awaiter2 = new TaskAwaiter();
                        goto TR_0005;
                    TR_000D:
                        this.progress(100);
                        fullName = this.<ret>5__2;
                        goto TR_0004;
                    TR_0015:
                        try
                        {
                            if (num == 5)
                            {
                                awaiter2 = this.<>u__2;
                                this.<>u__2 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_0010;
                            }
                            else
                            {
                                SemanticVersion originalVersion = this.<>8__1.updateInfo.CurrentlyInstalledVersion?.Version;
                                awaiter2 = this.<>4__this.cleanDeadVersions(originalVersion, this.<>8__1.newVersion, false).GetAwaiter();
                                if (awaiter2.IsCompleted)
                                {
                                    goto TR_0010;
                                }
                                else
                                {
                                    this.<>1__state = num = 5;
                                    this.<>u__2 = awaiter2;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<ApplyReleases>d__2>(ref awaiter2, ref this);
                                }
                            }
                            return;
                        TR_0010:
                            awaiter2.GetResult();
                            awaiter2 = new TaskAwaiter();
                        }
                        catch (Exception exception)
                        {
                            this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().WarnException("Failed to clean dead versions, continuing anyways", exception);
                        }
                        goto TR_000D;
                    TR_0016:
                        awaiter2.GetResult();
                        awaiter2 = new TaskAwaiter();
                        this.progress(0x4b);
                        this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().Info("Starting fixPinnedExecutables");
                        this.<>4__this.ErrorIfThrows(new Action(this.<>8__1.<ApplyReleases>b__5), null);
                        this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().Info("Fixing up tray icons");
                        this.<>8__1.trayFixer = new TrayStateChanger();
                        DirectoryInfo info = new DirectoryInfo(Utility.AppDirForRelease(this.<>4__this.rootAppDirectory, this.<>8__1.updateInfo.FutureReleaseEntry));
                        this.<>8__1.allExes = Enumerable.Select<FileInfo, string>(info.GetFiles("*.exe"), UpdateManager.ApplyReleasesImpl.<>c.<>9__2_6 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__2_6 = new Func<FileInfo, string>(this.<ApplyReleases>b__2_6))).ToList<string>();
                        this.<>4__this.ErrorIfThrows(new Action(this.<>8__1.<ApplyReleases>b__7), null);
                        this.progress(80);
                        goto TR_0015;
                    TR_0017:
                        awaiter4 = new TaskAwaiter<List<ReleaseEntry>>();
                        List<ReleaseEntry> source = awaiter4.GetResult();
                        this.progress(50);
                        this.<>8__1.newVersion = source.MaxBy<ReleaseEntry, SemanticVersion>((UpdateManager.ApplyReleasesImpl.<>c.<>9__2_3 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__2_3 = new Func<ReleaseEntry, SemanticVersion>(this.<ApplyReleases>b__2_3)))).First<ReleaseEntry>().Version;
                        this.<>4__this.executeSelfUpdate(this.<>8__1.newVersion);
                        awaiter2 = this.<>4__this.ErrorIfThrows(new Func<Task>(this.<>8__1.<ApplyReleases>b__4), "Failed to invoke post-install").GetAwaiter();
                        if (awaiter2.IsCompleted)
                        {
                            goto TR_0016;
                        }
                        else
                        {
                            this.<>1__state = num = 4;
                            this.<>u__2 = awaiter2;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<ApplyReleases>d__2>(ref awaiter2, ref this);
                        }
                        return;
                    TR_0019:
                        awaiter3 = new TaskAwaiter<string>();
                        string str2 = awaiter3.GetResult();
                        this.<ret>5__2 = str2;
                        this.progress(30);
                        awaiter4 = this.<>4__this.ErrorIfThrows<List<ReleaseEntry>>(new Func<Task<List<ReleaseEntry>>>(this.<>4__this.<ApplyReleases>b__2_2), "Failed to update local releases file").GetAwaiter();
                        if (!awaiter4.IsCompleted)
                        {
                            this.<>1__state = num = 3;
                            this.<>u__4 = awaiter4;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<List<ReleaseEntry>>, UpdateManager.ApplyReleasesImpl.<ApplyReleases>d__2>(ref awaiter4, ref this);
                        }
                        else
                        {
                            goto TR_0017;
                        }
                    }
                    catch (Exception exception2)
                    {
                        this.<>1__state = -2;
                        this.<>t__builder.SetException(exception2);
                    }
                    return;
                TR_0004:
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult(fullName);
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    this.<>t__builder.SetStateMachine(stateMachine);
                }
            }

            [CompilerGenerated]
            private struct <cleanDeadVersions>d__14 : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder <>t__builder;
                public UpdateManager.ApplyReleasesImpl <>4__this;
                public SemanticVersion currentVersion;
                public SemanticVersion originalVersion;
                public bool forceUninstall;
                private DirectoryInfo <di>5__1;
                private UpdateManager.ApplyReleasesImpl.<>c__DisplayClass14_0 <>8__2;
                private TaskAwaiter <>u__1;

                private void MoveNext()
                {
                    int num = this.<>1__state;
                    try
                    {
                        TaskAwaiter awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                        }
                        else
                        {
                            if (num == 1)
                            {
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_0010;
                            }
                            else
                            {
                                this.<>8__2 = new UpdateManager.ApplyReleasesImpl.<>c__DisplayClass14_0();
                                this.<>8__2.<>4__this = this.<>4__this;
                                if (this.currentVersion != null)
                                {
                                    this.<di>5__1 = new DirectoryInfo(this.<>4__this.rootAppDirectory);
                                    if (this.<di>5__1.Exists)
                                    {
                                        this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().Info<SemanticVersion>("cleanDeadVersions: for version {0}", this.currentVersion);
                                        this.<>8__2.originalVersionFolder = null;
                                        if (this.originalVersion != null)
                                        {
                                            this.<>8__2.originalVersionFolder = this.<>4__this.getDirectoryForRelease(this.originalVersion).Name;
                                            this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().Info<string>("cleanDeadVersions: exclude original version folder {0}", this.<>8__2.originalVersionFolder);
                                        }
                                        this.<>8__2.currentVersionFolder = null;
                                        if (this.currentVersion != null)
                                        {
                                            this.<>8__2.currentVersionFolder = this.<>4__this.getDirectoryForRelease(this.currentVersion).Name;
                                            this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().Info<string>("cleanDeadVersions: exclude current version folder {0}", this.<>8__2.currentVersionFolder);
                                        }
                                        IEnumerable<DirectoryInfo> source = Enumerable.Where<DirectoryInfo>(Enumerable.Where<DirectoryInfo>(Enumerable.Where<DirectoryInfo>(this.<di>5__1.GetDirectories(), UpdateManager.ApplyReleasesImpl.<>c.<>9__14_0 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__14_0 = new Func<DirectoryInfo, bool>(this.<cleanDeadVersions>b__14_0))), new Func<DirectoryInfo, bool>(this.<>8__2.<cleanDeadVersions>b__1)), UpdateManager.ApplyReleasesImpl.<>c.<>9__14_2 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__14_2 = new Func<DirectoryInfo, bool>(this.<cleanDeadVersions>b__14_2)));
                                        if (this.forceUninstall)
                                        {
                                            goto TR_0011;
                                        }
                                        else
                                        {
                                            awaiter = source.ForEachAsync<DirectoryInfo>(new Func<DirectoryInfo, Task>(this.<>4__this.<cleanDeadVersions>b__14_3), 4).GetAwaiter();
                                            if (awaiter.IsCompleted)
                                            {
                                                goto TR_0013;
                                            }
                                            else
                                            {
                                                this.<>1__state = num = 0;
                                                this.<>u__1 = awaiter;
                                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<cleanDeadVersions>d__14>(ref awaiter, ref this);
                                            }
                                        }
                                        return;
                                    }
                                }
                                goto TR_0002;
                            }
                            return;
                        }
                        goto TR_0013;
                    TR_0010:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        string path = Utility.LocalReleaseFileForAppDir(this.<>4__this.rootAppDirectory);
                        string str2 = Utility.PackageDirectoryForAppDir(this.<>4__this.rootAppDirectory);
                        ReleaseEntry entry = null;
                        IEnumerator<ReleaseEntry> enumerator = ReleaseEntry.ParseReleaseFile(File.ReadAllText(path, Encoding.UTF8)).GetEnumerator();
                        try
                        {
                            while (enumerator.MoveNext())
                            {
                                ReleaseEntry current = enumerator.Current;
                                if (current.Version == this.currentVersion)
                                {
                                    entry = ReleaseEntry.GenerateFromFile(Path.Combine(str2, current.Filename), null);
                                    continue;
                                }
                                File.Delete(Path.Combine(str2, current.Filename));
                            }
                        }
                        finally
                        {
                            if ((num < 0) && (enumerator != null))
                            {
                                enumerator.Dispose();
                            }
                        }
                        ReleaseEntry[] releaseEntries = new ReleaseEntry[] { entry };
                        ReleaseEntry.WriteReleaseFile(releaseEntries, path);
                        goto TR_0002;
                    TR_0011:
                        awaiter = Enumerable.Where<DirectoryInfo>(Enumerable.Where<DirectoryInfo>(this.<di>5__1.GetDirectories(), UpdateManager.ApplyReleasesImpl.<>c.<>9__14_5 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__14_5 = new Func<DirectoryInfo, bool>(this.<cleanDeadVersions>b__14_5))), new Func<DirectoryInfo, bool>(this.<>8__2.<cleanDeadVersions>b__6)).ForEachAsync<DirectoryInfo>(new Func<DirectoryInfo, Task>(this.<>4__this.<cleanDeadVersions>b__14_7), 4).GetAwaiter();
                        if (awaiter.IsCompleted)
                        {
                            goto TR_0010;
                        }
                        else
                        {
                            this.<>1__state = num = 1;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<cleanDeadVersions>d__14>(ref awaiter, ref this);
                        }
                        return;
                    TR_0013:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        goto TR_0011;
                    }
                    catch (Exception exception)
                    {
                        this.<>1__state = -2;
                        this.<>t__builder.SetException(exception);
                        return;
                    }
                TR_0002:
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    this.<>t__builder.SetStateMachine(stateMachine);
                }
            }

            [CompilerGenerated]
            private struct <createFullPackagesFromDeltas>d__8 : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder<ReleaseEntry> <>t__builder;
                public UpdateManager.ApplyReleasesImpl <>4__this;
                public ReleaseEntry currentVersion;
                public IEnumerable<ReleaseEntry> releasesToApply;
                private UpdateManager.ApplyReleasesImpl.<>c__DisplayClass8_0 <>8__1;
                private TaskAwaiter<ReleasePackage> <>u__1;
                private TaskAwaiter<ReleaseEntry> <>u__2;

                private void MoveNext()
                {
                    ReleaseEntry entry;
                    int num = this.<>1__state;
                    try
                    {
                        TaskAwaiter<ReleasePackage> awaiter;
                        TaskAwaiter<ReleaseEntry> awaiter2;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter<ReleasePackage>();
                            this.<>1__state = num = -1;
                        }
                        else if (num == 1)
                        {
                            awaiter2 = this.<>u__2;
                            this.<>u__2 = new TaskAwaiter<ReleaseEntry>();
                            this.<>1__state = num = -1;
                            goto TR_0009;
                        }
                        else
                        {
                            this.<>8__1 = new UpdateManager.ApplyReleasesImpl.<>c__DisplayClass8_0();
                            this.<>8__1.<>4__this = this.<>4__this;
                            this.<>8__1.currentVersion = this.currentVersion;
                            this.<>8__1.releasesToApply = this.releasesToApply;
                            if (this.<>8__1.releasesToApply.Any<ReleaseEntry>())
                            {
                                if (!Enumerable.All<ReleaseEntry>(this.<>8__1.releasesToApply, UpdateManager.ApplyReleasesImpl.<>c.<>9__8_0 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__8_0 = new Func<ReleaseEntry, bool>(this.<createFullPackagesFromDeltas>b__8_0))))
                                {
                                    if (!Enumerable.All<ReleaseEntry>(this.<>8__1.releasesToApply, UpdateManager.ApplyReleasesImpl.<>c.<>9__8_2 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__8_2 = new Func<ReleaseEntry, bool>(this.<createFullPackagesFromDeltas>b__8_2))))
                                    {
                                        throw new Exception("Cannot apply combinations of delta and full packages");
                                    }
                                    awaiter = Task.Run<ReleasePackage>(new Func<ReleasePackage>(this.<>8__1.<createFullPackagesFromDeltas>b__3)).GetAwaiter();
                                    if (!awaiter.IsCompleted)
                                    {
                                        this.<>1__state = num = 0;
                                        this.<>u__1 = awaiter;
                                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<ReleasePackage>, UpdateManager.ApplyReleasesImpl.<createFullPackagesFromDeltas>d__8>(ref awaiter, ref this);
                                        return;
                                    }
                                }
                                else
                                {
                                    entry = this.<>8__1.releasesToApply.MaxBy<ReleaseEntry, SemanticVersion>((UpdateManager.ApplyReleasesImpl.<>c.<>9__8_1 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__8_1 = new Func<ReleaseEntry, SemanticVersion>(this.<createFullPackagesFromDeltas>b__8_1)))).FirstOrDefault<ReleaseEntry>();
                                    goto TR_0002;
                                }
                            }
                            else
                            {
                                entry = null;
                                goto TR_0002;
                            }
                        }
                        ReleasePackage result = new TaskAwaiter<ReleasePackage>().GetResult();
                        if (this.<>8__1.releasesToApply.Count<ReleaseEntry>() != 1)
                        {
                            FileInfo info = new FileInfo(result.InputPackageFile);
                            ReleaseEntry currentVersion = ReleaseEntry.GenerateFromFile(info.OpenRead(), info.Name, null);
                            awaiter2 = this.<>4__this.createFullPackagesFromDeltas(this.<>8__1.releasesToApply.Skip<ReleaseEntry>(1), currentVersion).GetAwaiter();
                            if (awaiter2.IsCompleted)
                            {
                                goto TR_0009;
                            }
                            else
                            {
                                this.<>1__state = num = 1;
                                this.<>u__2 = awaiter2;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<ReleaseEntry>, UpdateManager.ApplyReleasesImpl.<createFullPackagesFromDeltas>d__8>(ref awaiter2, ref this);
                            }
                            return;
                        }
                        else
                        {
                            entry = ReleaseEntry.GenerateFromFile(result.InputPackageFile, null);
                        }
                        goto TR_0002;
                    TR_0009:
                        awaiter2 = new TaskAwaiter<ReleaseEntry>();
                        entry = awaiter2.GetResult();
                    }
                    catch (Exception exception)
                    {
                        this.<>1__state = -2;
                        this.<>t__builder.SetException(exception);
                        return;
                    }
                TR_0002:
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult(entry);
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    this.<>t__builder.SetStateMachine(stateMachine);
                }
            }

            [CompilerGenerated]
            private struct <FullUninstall>d__3 : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder <>t__builder;
                public UpdateManager.ApplyReleasesImpl <>4__this;
                private List<FileInfo> <allApps>5__1;
                private int <i>5__2;
                private bool <didSucceedDeleting>5__3;
                private TaskAwaiter <>u__1;

                private void MoveNext()
                {
                    int num = this.<>1__state;
                    try
                    {
                        DirectoryInfo info;
                        TaskAwaiter awaiter;
                        int num2;
                        switch (num)
                        {
                            case 0:
                                break;

                            case 1:
                                goto TR_0011;

                            case 2:
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_0005;

                            default:
                                info = this.<>4__this.getReleases().MaxBy<DirectoryInfo, SemanticVersion>((UpdateManager.ApplyReleasesImpl.<>c.<>9__3_0 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__3_0 = new Func<DirectoryInfo, SemanticVersion>(this.<FullUninstall>b__3_0)))).FirstOrDefault<DirectoryInfo>();
                                this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().Info("Starting full uninstall");
                                if (!info.Exists)
                                {
                                    goto TR_0014;
                                }
                                else
                                {
                                    UpdateManager.ApplyReleasesImpl <>4__this = this.<>4__this;
                                    SemanticVersion version = info.Name.ToSemanticVersion();
                                }
                                break;
                        }
                        try
                        {
                            List<Process> list2;
                            if (num == 0)
                            {
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_001C;
                            }
                            else
                            {
                                List<string> allSquirrelAwareApps = SquirrelAwareExecutableDetector.GetAllSquirrelAwareApps(info.FullName, 1);
                                if (UpdateManager.ApplyReleasesImpl.isAppFolderDead(info.FullName))
                                {
                                    throw new Exception("App folder is dead, but we're trying to uninstall it?");
                                }
                                this.<allApps>5__1 = Enumerable.Where<FileInfo>(Enumerable.Where<FileInfo>(info.EnumerateFiles(), UpdateManager.ApplyReleasesImpl.<>c.<>9__3_1 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__3_1 = new Func<FileInfo, bool>(this.<FullUninstall>b__3_1))), UpdateManager.ApplyReleasesImpl.<>c.<>9__3_2 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__3_2 = new Func<FileInfo, bool>(this.<FullUninstall>b__3_2))).ToList<FileInfo>();
                                if (allSquirrelAwareApps.Count <= 0)
                                {
                                    this.<allApps>5__1.ForEach(new Action<FileInfo>(this.<>4__this.<FullUninstall>b__3_4));
                                    goto TR_001A;
                                }
                                else
                                {
                                    UpdateManager.ApplyReleasesImpl.<>c__DisplayClass3_0 class_;
                                    awaiter = allSquirrelAwareApps.ForEachAsync<string>(new Func<string, Task>(class_.<FullUninstall>b__3), 1).GetAwaiter();
                                    if (awaiter.IsCompleted)
                                    {
                                        goto TR_001C;
                                    }
                                    else
                                    {
                                        this.<>1__state = num = 0;
                                        this.<>u__1 = awaiter;
                                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<FullUninstall>d__3>(ref awaiter, ref this);
                                    }
                                }
                            }
                            return;
                        TR_001A:
                            list2 = Enumerable.SelectMany<FileInfo, Process>(this.<allApps>5__1, UpdateManager.ApplyReleasesImpl.<>c.<>9__3_5 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__3_5 = new Func<FileInfo, IEnumerable<Process>>(this.<FullUninstall>b__3_5))).ToList<Process>();
                            if (list2.Count > 0)
                            {
                                list2.ForEach(UpdateManager.ApplyReleasesImpl.<>c.<>9__3_6 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__3_6 = new Action<Process>(this.<FullUninstall>b__3_6)));
                                Thread.Sleep(750);
                            }
                            this.<allApps>5__1 = null;
                            goto TR_0014;
                        TR_001C:
                            awaiter.GetResult();
                            awaiter = new TaskAwaiter();
                            goto TR_001A;
                        }
                        catch (Exception exception)
                        {
                            this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().WarnException("Failed to run pre-uninstall hooks, uninstalling anyways", exception);
                            goto TR_0014;
                        }
                        return;
                    TR_0004:
                        File.WriteAllText(Path.Combine(this.<>4__this.rootAppDirectory, ".dead"), " ");
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult();
                        return;
                    TR_0005:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        goto TR_0004;
                    TR_0009:
                        num2 = this.<i>5__2 + 1;
                        this.<i>5__2 = num2;
                        goto TR_0013;
                    TR_0011:
                        try
                        {
                            if (num == 1)
                            {
                                awaiter = this.<>u__1;
                                this.<>u__1 = new TaskAwaiter();
                                this.<>1__state = num = -1;
                                goto TR_000C;
                            }
                            else
                            {
                                awaiter = Utility.DeleteDirectory(this.<>4__this.rootAppDirectory).GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    goto TR_000C;
                                }
                                else
                                {
                                    this.<>1__state = num = 1;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<FullUninstall>d__3>(ref awaiter, ref this);
                                }
                            }
                            return;
                        TR_000C:
                            awaiter.GetResult();
                            awaiter = new TaskAwaiter();
                            this.<didSucceedDeleting>5__3 = true;
                            goto TR_0009;
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(0x3e8);
                            goto TR_0009;
                        }
                        return;
                    TR_0013:
                        while (true)
                        {
                            if (this.<i>5__2 < 10)
                            {
                                break;
                            }
                            if (this.<didSucceedDeleting>5__3)
                            {
                                goto TR_0004;
                            }
                            else
                            {
                                awaiter = this.<>4__this.ErrorIfThrows(new Func<Task>(this.<>4__this.<FullUninstall>b__3_7), ("Failed to delete app directory: " + this.<>4__this.rootAppDirectory)).GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    goto TR_0005;
                                }
                                else
                                {
                                    this.<>1__state = num = 2;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<FullUninstall>d__3>(ref awaiter, ref this);
                                }
                            }
                            return;
                        }
                        goto TR_0011;
                    TR_0014:
                        this.<>4__this.fixPinnedExecutables(new SemanticVersion(0xff, 0xff, 0xff, 0xff));
                        this.<didSucceedDeleting>5__3 = false;
                        this.<i>5__2 = 0;
                        goto TR_0013;
                    }
                    catch (Exception exception2)
                    {
                        this.<>1__state = -2;
                        this.<>t__builder.SetException(exception2);
                    }
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    this.<>t__builder.SetStateMachine(stateMachine);
                }
            }

            [CompilerGenerated]
            private struct <invokePostInstall>d__10 : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder <>t__builder;
                public UpdateManager.ApplyReleasesImpl <>4__this;
                public bool isInitialInstall;
                public SemanticVersion currentVersion;
                public bool firstRunOnly;
                private List<string> <squirrelApps>5__1;
                private DirectoryInfo <targetDir>5__2;
                private UpdateManager.ApplyReleasesImpl.<>c__DisplayClass10_0 <>8__3;
                public bool silentInstall;
                private TaskAwaiter <>u__1;

                private void MoveNext()
                {
                    int num = this.<>1__state;
                    try
                    {
                        TaskAwaiter awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0008;
                        }
                        else
                        {
                            this.<>8__3 = new UpdateManager.ApplyReleasesImpl.<>c__DisplayClass10_0();
                            this.<>8__3.<>4__this = this.<>4__this;
                            this.<>8__3.isInitialInstall = this.isInitialInstall;
                            this.<targetDir>5__2 = this.<>4__this.getDirectoryForRelease(this.currentVersion);
                            this.<>8__3.args = this.<>8__3.isInitialInstall ? $"--squirrel-install {this.currentVersion}" : $"--squirrel-updated {this.currentVersion}";
                            this.<squirrelApps>5__1 = SquirrelAwareExecutableDetector.GetAllSquirrelAwareApps(this.<targetDir>5__2.FullName, 1);
                            this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().Info<string>("Squirrel Enabled Apps: [{0}]", string.Join(",", this.<squirrelApps>5__1));
                            if (this.firstRunOnly)
                            {
                                goto TR_0007;
                            }
                            else
                            {
                                awaiter = this.<squirrelApps>5__1.ForEachAsync<string>(new Func<string, Task>(this.<>8__3.<invokePostInstall>b__0), 1).GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    goto TR_0008;
                                }
                                else
                                {
                                    this.<>1__state = num = 0;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.ApplyReleasesImpl.<invokePostInstall>d__10>(ref awaiter, ref this);
                                }
                            }
                        }
                        return;
                    TR_0007:
                        if (this.<squirrelApps>5__1.Count == 0)
                        {
                            this.<>4__this.Log<UpdateManager.ApplyReleasesImpl>().Warn("No apps are marked as Squirrel-aware! Going to run them all");
                            this.<squirrelApps>5__1 = Enumerable.Select<FileInfo, string>(Enumerable.Where<FileInfo>(Enumerable.Where<FileInfo>(this.<targetDir>5__2.EnumerateFiles(), UpdateManager.ApplyReleasesImpl.<>c.<>9__10_1 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__10_1 = new Func<FileInfo, bool>(this.<invokePostInstall>b__10_1))), UpdateManager.ApplyReleasesImpl.<>c.<>9__10_2 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__10_2 = new Func<FileInfo, bool>(this.<invokePostInstall>b__10_2))), UpdateManager.ApplyReleasesImpl.<>c.<>9__10_3 ?? (UpdateManager.ApplyReleasesImpl.<>c.<>9__10_3 = new Func<FileInfo, string>(this.<invokePostInstall>b__10_3))).ToList<string>();
                            this.<squirrelApps>5__1.ForEach(new Action<string>(this.<>8__3.<invokePostInstall>b__4));
                        }
                        if (!(!this.<>8__3.isInitialInstall | this.silentInstall))
                        {
                            this.<>8__3.firstRunParam = this.<>8__3.isInitialInstall ? "--squirrel-firstrun" : "";
                            Enumerable.Select<string, ProcessStartInfo>(this.<squirrelApps>5__1, new Func<string, ProcessStartInfo>(this.<>8__3.<invokePostInstall>b__5)).ForEach<ProcessStartInfo>(new Action<ProcessStartInfo>(this.<>4__this.<invokePostInstall>b__10_6));
                        }
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult();
                        return;
                    TR_0008:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        goto TR_0007;
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

            [CompilerGenerated]
            private struct <updateLocalReleasesFile>d__17 : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder<List<ReleaseEntry>> <>t__builder;
                public UpdateManager.ApplyReleasesImpl <>4__this;
                private TaskAwaiter<List<ReleaseEntry>> <>u__1;

                private void MoveNext()
                {
                    int num = this.<>1__state;
                    try
                    {
                        TaskAwaiter<List<ReleaseEntry>> awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter<List<ReleaseEntry>>();
                            this.<>1__state = num = -1;
                            goto TR_0004;
                        }
                        else
                        {
                            awaiter = Task.Run<List<ReleaseEntry>>(new Func<List<ReleaseEntry>>(this.<>4__this.<updateLocalReleasesFile>b__17_0)).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto TR_0004;
                            }
                            else
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<List<ReleaseEntry>>, UpdateManager.ApplyReleasesImpl.<updateLocalReleasesFile>d__17>(ref awaiter, ref this);
                            }
                        }
                        return;
                    TR_0004:
                        awaiter = new TaskAwaiter<List<ReleaseEntry>>();
                        List<ReleaseEntry> result = awaiter.GetResult();
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult(result);
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

        internal class CheckForUpdateImpl : IEnableLogger
        {
            private readonly string rootAppDirectory;

            public CheckForUpdateImpl(string rootAppDirectory)
            {
                this.rootAppDirectory = rootAppDirectory;
            }

            [AsyncStateMachine(typeof(<CheckForUpdate>d__2))]
            public Task<UpdateInfo> CheckForUpdate(string localReleaseFile, string updateUrlOrPath, bool ignoreDeltaUpdates = false, Action<int> progress = null, IFileDownloader urlDownloader = null)
            {
                <CheckForUpdate>d__2 d__;
                d__.<>4__this = this;
                d__.localReleaseFile = localReleaseFile;
                d__.updateUrlOrPath = updateUrlOrPath;
                d__.ignoreDeltaUpdates = ignoreDeltaUpdates;
                d__.progress = progress;
                d__.urlDownloader = urlDownloader;
                d__.<>t__builder = AsyncTaskMethodBuilder<UpdateInfo>.Create();
                d__.<>1__state = -1;
                d__.<>t__builder.Start<<CheckForUpdate>d__2>(ref d__);
                return d__.<>t__builder.Task;
            }

            private UpdateInfo determineUpdateInfo(IEnumerable<ReleaseEntry> localReleases, IEnumerable<ReleaseEntry> remoteReleases, bool ignoreDeltaUpdates)
            {
                string packageDirectory = Utility.PackageDirectoryForAppDir(this.rootAppDirectory);
                localReleases = localReleases ?? Enumerable.Empty<ReleaseEntry>();
                if ((remoteReleases == null) || !remoteReleases.Any<ReleaseEntry>())
                {
                    this.Log<UpdateManager.CheckForUpdateImpl>().Warn("Release information couldn't be determined due to remote corrupt RELEASES file");
                    remoteReleases = localReleases;
                }
                ReleaseEntry entry = Utility.FindCurrentVersion(remoteReleases);
                ReleaseEntry currentVersion = Utility.FindCurrentVersion(localReleases);
                if ((currentVersion != null) && (entry.Version <= currentVersion.Version))
                {
                    this.Log<UpdateManager.CheckForUpdateImpl>().Info("No updates");
                    ReleaseEntry[] entryArray1 = new ReleaseEntry[] { currentVersion };
                    return UpdateInfo.Create(currentVersion, entryArray1, packageDirectory);
                }
                SemanticVersion version = currentVersion?.Version;
                SemanticVersion version2 = entry?.Version;
                this.Log<UpdateManager.CheckForUpdateImpl>().Info<SemanticVersion, SemanticVersion>("Remote version {0} differs from local {1}", version2, version);
                if (ignoreDeltaUpdates)
                {
                    remoteReleases = from x in remoteReleases
                        where !x.IsDelta
                        select x;
                }
                if (!localReleases.Any<ReleaseEntry>())
                {
                    this.Log<UpdateManager.CheckForUpdateImpl>().Warn("First run or local directory is corrupt, starting from scratch");
                    ReleaseEntry[] entryArray2 = new ReleaseEntry[] { entry };
                    return UpdateInfo.Create(currentVersion, entryArray2, packageDirectory);
                }
                if (Enumerable.Max<ReleaseEntry, SemanticVersion>(localReleases, x => x.Version) <= Enumerable.Max<ReleaseEntry, SemanticVersion>(remoteReleases, x => x.Version))
                {
                    return UpdateInfo.Create(currentVersion, remoteReleases, packageDirectory);
                }
                this.Log<UpdateManager.CheckForUpdateImpl>().Warn("hwhat, local version is greater than remote version");
                ReleaseEntry[] availableReleases = new ReleaseEntry[] { entry };
                return UpdateInfo.Create(currentVersion, availableReleases, packageDirectory);
            }

            [AsyncStateMachine(typeof(<initializeClientAppDirectory>d__3))]
            private Task initializeClientAppDirectory()
            {
                <initializeClientAppDirectory>d__3 d__;
                d__.<>4__this = this;
                d__.<>t__builder = AsyncTaskMethodBuilder.Create();
                d__.<>1__state = -1;
                d__.<>t__builder.Start<<initializeClientAppDirectory>d__3>(ref d__);
                return d__.<>t__builder.Task;
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly UpdateManager.CheckForUpdateImpl.<>c <>9 = new UpdateManager.CheckForUpdateImpl.<>c();
                public static Action<int> <>9__2_0;
                public static Func<ReleaseEntry, SemanticVersion> <>9__2_1;
                public static Func<FileInfo, ReleaseEntry> <>9__2_2;
                public static Func<ReleaseEntry, bool> <>9__4_0;
                public static Func<ReleaseEntry, SemanticVersion> <>9__4_1;
                public static Func<ReleaseEntry, SemanticVersion> <>9__4_2;

                internal void <CheckForUpdate>b__2_0(int _)
                {
                }

                internal SemanticVersion <CheckForUpdate>b__2_1(ReleaseEntry x) => 
                    x.Version;

                internal ReleaseEntry <CheckForUpdate>b__2_2(FileInfo x) => 
                    ReleaseEntry.GenerateFromFile(x.FullName, null);

                internal bool <determineUpdateInfo>b__4_0(ReleaseEntry x) => 
                    !x.IsDelta;

                internal SemanticVersion <determineUpdateInfo>b__4_1(ReleaseEntry x) => 
                    x.Version;

                internal SemanticVersion <determineUpdateInfo>b__4_2(ReleaseEntry x) => 
                    x.Version;
            }

            [CompilerGenerated]
            private struct <CheckForUpdate>d__2 : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder<UpdateInfo> <>t__builder;
                public Action<int> progress;
                public string localReleaseFile;
                public UpdateManager.CheckForUpdateImpl <>4__this;
                private IEnumerable<ReleaseEntry> <localReleases>5__1;
                public string updateUrlOrPath;
                private ReleaseEntry <latestLocalRelease>5__2;
                public IFileDownloader urlDownloader;
                private int <retries>5__3;
                private string <releaseFile>5__4;
                public bool ignoreDeltaUpdates;
                private TaskAwaiter <>u__1;
                private TaskAwaiter<byte[]> <>u__2;

                private void MoveNext()
                {
                    int num = this.<>1__state;
                    try
                    {
                        IEnumerable<ReleaseEntry> enumerable;
                        TaskAwaiter awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0024;
                        }
                        else
                        {
                            if (num == 1)
                            {
                                goto TR_0014;
                            }
                            else
                            {
                                this.progress = this.progress ?? (UpdateManager.CheckForUpdateImpl.<>c.<>9__2_0 ?? (UpdateManager.CheckForUpdateImpl.<>c.<>9__2_0 = new Action<int>(this.<CheckForUpdate>b__2_0)));
                                this.<localReleases>5__1 = Enumerable.Empty<ReleaseEntry>();
                                bool flag = false;
                                try
                                {
                                    if (File.Exists(this.localReleaseFile))
                                    {
                                        this.<localReleases>5__1 = Utility.LoadLocalReleases(this.localReleaseFile);
                                    }
                                    else
                                    {
                                        flag = true;
                                    }
                                }
                                catch (Exception exception)
                                {
                                    this.<>4__this.Log<UpdateManager.CheckForUpdateImpl>().WarnException("Failed to load local releases, starting from scratch", exception);
                                    flag = true;
                                }
                                if (!flag)
                                {
                                    goto TR_0022;
                                }
                                else
                                {
                                    awaiter = this.<>4__this.initializeClientAppDirectory().GetAwaiter();
                                    if (awaiter.IsCompleted)
                                    {
                                        goto TR_0024;
                                    }
                                    else
                                    {
                                        this.<>1__state = num = 0;
                                        this.<>u__1 = awaiter;
                                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.CheckForUpdateImpl.<CheckForUpdate>d__2>(ref awaiter, ref this);
                                    }
                                }
                            }
                            return;
                        }
                        goto TR_0022;
                    TR_0009:
                        enumerable = ReleaseEntry.ParseReleaseFile(this.<releaseFile>5__4);
                        this.progress(0x42);
                        this.progress(100);
                        UpdateInfo result = this.<>4__this.determineUpdateInfo(this.<localReleases>5__1, enumerable, this.ignoreDeltaUpdates);
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult(result);
                        return;
                    TR_000A:
                        this.progress(0x21);
                        goto TR_0009;
                    TR_0014:
                        while (true)
                        {
                            try
                            {
                                TaskAwaiter<byte[]> awaiter2;
                                if (num == 1)
                                {
                                    awaiter2 = this.<>u__2;
                                    this.<>u__2 = new TaskAwaiter<byte[]>();
                                    this.<>1__state = num = -1;
                                    goto TR_000B;
                                }
                                else
                                {
                                    Uri uri = Utility.AppendPathToUri(new Uri(this.updateUrlOrPath), "RELEASES");
                                    if (this.<latestLocalRelease>5__2 != null)
                                    {
                                        Dictionary<string, string> newQuery = new Dictionary<string, string>();
                                        newQuery.Add("id", this.<latestLocalRelease>5__2.PackageName);
                                        newQuery.Add("localVersion", this.<latestLocalRelease>5__2.Version.ToString());
                                        newQuery.Add("arch", Environment.Is64BitOperatingSystem ? "amd64" : "x86");
                                        uri = Utility.AddQueryParamsToUri(uri, newQuery);
                                    }
                                    awaiter2 = this.urlDownloader.DownloadUrl(uri.ToString()).GetAwaiter();
                                    if (awaiter2.IsCompleted)
                                    {
                                        goto TR_000B;
                                    }
                                    else
                                    {
                                        this.<>1__state = num = 1;
                                        this.<>u__2 = awaiter2;
                                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<byte[]>, UpdateManager.CheckForUpdateImpl.<CheckForUpdate>d__2>(ref awaiter2, ref this);
                                    }
                                }
                                break;
                            TR_000B:
                                awaiter2 = new TaskAwaiter<byte[]>();
                                byte[] bytes = awaiter2.GetResult();
                                this.<releaseFile>5__4 = Encoding.UTF8.GetString(bytes);
                                goto TR_000A;
                            }
                            catch (WebException exception2)
                            {
                                this.<>4__this.Log<UpdateManager.CheckForUpdateImpl>().InfoException("Download resulted in WebException (returning blank release list)", exception2);
                                if (this.<retries>5__3 <= 0)
                                {
                                    throw;
                                }
                                int num2 = this.<retries>5__3;
                                this.<retries>5__3 = num2 - 1;
                                continue;
                            }
                            break;
                        }
                        return;
                    TR_0022:
                        this.<latestLocalRelease>5__2 = (this.<localReleases>5__1.Count<ReleaseEntry>() > 0) ? this.<localReleases>5__1.MaxBy<ReleaseEntry, SemanticVersion>((UpdateManager.CheckForUpdateImpl.<>c.<>9__2_1 ?? (UpdateManager.CheckForUpdateImpl.<>c.<>9__2_1 = new Func<ReleaseEntry, SemanticVersion>(this.<CheckForUpdate>b__2_1)))).First<ReleaseEntry>() : null;
                        if (!Utility.IsHttpUrl(this.updateUrlOrPath))
                        {
                            this.<>4__this.Log<UpdateManager.CheckForUpdateImpl>().Info<string>("Reading RELEASES file from {0}", this.updateUrlOrPath);
                            if (!Directory.Exists(this.updateUrlOrPath))
                            {
                                throw new Exception($"The directory {this.updateUrlOrPath} does not exist, something is probably broken with your application");
                            }
                            FileInfo info2 = new FileInfo(Path.Combine(this.updateUrlOrPath, "RELEASES"));
                            if (!info2.Exists)
                            {
                                string message = $"The file {info2.FullName} does not exist, something is probably broken with your application";
                                this.<>4__this.Log<UpdateManager.CheckForUpdateImpl>().Warn(message);
                                FileInfo[] files = new DirectoryInfo(this.updateUrlOrPath).GetFiles("*.nupkg");
                                if (files.Length == 0)
                                {
                                    throw new Exception(message);
                                }
                                ReleaseEntry.WriteReleaseFile(Enumerable.Select<FileInfo, ReleaseEntry>(files, UpdateManager.CheckForUpdateImpl.<>c.<>9__2_2 ?? (UpdateManager.CheckForUpdateImpl.<>c.<>9__2_2 = new Func<FileInfo, ReleaseEntry>(this.<CheckForUpdate>b__2_2))), info2.FullName);
                            }
                            this.<releaseFile>5__4 = File.ReadAllText(info2.FullName, Encoding.UTF8);
                            this.progress(0x21);
                            goto TR_0009;
                        }
                        else
                        {
                            if (this.updateUrlOrPath.EndsWith("/"))
                            {
                                this.updateUrlOrPath = this.updateUrlOrPath.Substring(0, this.updateUrlOrPath.Length - 1);
                            }
                            this.<>4__this.Log<UpdateManager.CheckForUpdateImpl>().Info<string>("Downloading RELEASES file from {0}", this.updateUrlOrPath);
                            this.<retries>5__3 = 3;
                        }
                        goto TR_0014;
                    TR_0024:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        goto TR_0022;
                    }
                    catch (Exception exception3)
                    {
                        this.<>1__state = -2;
                        this.<>t__builder.SetException(exception3);
                    }
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    this.<>t__builder.SetStateMachine(stateMachine);
                }
            }

            [CompilerGenerated]
            private struct <initializeClientAppDirectory>d__3 : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder <>t__builder;
                public UpdateManager.CheckForUpdateImpl <>4__this;
                private string <pkgDir>5__1;
                private TaskAwaiter <>u__1;

                private void MoveNext()
                {
                    int num = this.<>1__state;
                    try
                    {
                        TaskAwaiter awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0005;
                        }
                        else
                        {
                            this.<pkgDir>5__1 = Path.Combine(this.<>4__this.rootAppDirectory, "packages");
                            if (!Directory.Exists(this.<pkgDir>5__1))
                            {
                                goto TR_0004;
                            }
                            else
                            {
                                awaiter = Utility.DeleteDirectory(this.<pkgDir>5__1).GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    goto TR_0005;
                                }
                                else
                                {
                                    this.<>1__state = num = 0;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.CheckForUpdateImpl.<initializeClientAppDirectory>d__3>(ref awaiter, ref this);
                                }
                            }
                        }
                        return;
                    TR_0004:
                        Directory.CreateDirectory(this.<pkgDir>5__1);
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult();
                        return;
                    TR_0005:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        goto TR_0004;
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

        internal class DownloadReleasesImpl : IEnableLogger
        {
            private readonly string rootAppDirectory;

            public DownloadReleasesImpl(string rootAppDirectory)
            {
                this.rootAppDirectory = rootAppDirectory;
            }

            private Task checksumAllPackages(IEnumerable<ReleaseEntry> releasesDownloaded) => 
                releasesDownloaded.ForEachAsync<ReleaseEntry>(delegate (ReleaseEntry x) {
                    this.checksumPackage(x);
                }, 4);

            private void checksumPackage(ReleaseEntry downloadedRelease)
            {
                FileInfo info = new FileInfo(Path.Combine(this.rootAppDirectory, "packages", downloadedRelease.Filename));
                if (!info.Exists)
                {
                    this.Log<UpdateManager.DownloadReleasesImpl>().Error<string>("File {0} should exist but doesn't", info.FullName);
                    throw new Exception("Checksummed file doesn't exist: " + info.FullName);
                }
                if (info.Length != downloadedRelease.Filesize)
                {
                    this.Log<UpdateManager.DownloadReleasesImpl>().Error<long, long>("File Length should be {0}, is {1}", downloadedRelease.Filesize, info.Length);
                    info.Delete();
                    throw new Exception("Checksummed file size doesn't match: " + info.FullName);
                }
                using (FileStream stream = info.OpenRead())
                {
                    string str = Utility.CalculateStreamSHA1(stream);
                    if (!str.Equals(downloadedRelease.SHA1, StringComparison.OrdinalIgnoreCase))
                    {
                        this.Log<UpdateManager.DownloadReleasesImpl>().Error<string, string>("File SHA1 should be {0}, is {1}", downloadedRelease.SHA1, str);
                        info.Delete();
                        throw new Exception("Checksum doesn't match: " + info.FullName);
                    }
                }
            }

            private Task downloadRelease(string updateBaseUrl, ReleaseEntry releaseEntry, IFileDownloader urlDownloader, string targetFile, Action<int> progress)
            {
                string relativeUri = releaseEntry.BaseUrl + releaseEntry.Filename;
                if (!string.IsNullOrEmpty(releaseEntry.Query))
                {
                    relativeUri = relativeUri + releaseEntry.Query;
                }
                string absoluteUri = new Uri(Utility.EnsureTrailingSlash(new Uri(updateBaseUrl)), relativeUri).AbsoluteUri;
                File.Delete(targetFile);
                return urlDownloader.DownloadFile(absoluteUri, targetFile, progress);
            }

            [AsyncStateMachine(typeof(<DownloadReleases>d__2))]
            public Task DownloadReleases(string updateUrlOrPath, IEnumerable<ReleaseEntry> releasesToDownload, Action<int> progress = null, IFileDownloader urlDownloader = null)
            {
                <DownloadReleases>d__2 d__;
                d__.<>4__this = this;
                d__.updateUrlOrPath = updateUrlOrPath;
                d__.releasesToDownload = releasesToDownload;
                d__.progress = progress;
                d__.urlDownloader = urlDownloader;
                d__.<>t__builder = AsyncTaskMethodBuilder.Create();
                d__.<>1__state = -1;
                d__.<>t__builder.Start<<DownloadReleases>d__2>(ref d__);
                return d__.<>t__builder.Task;
            }

            private bool isReleaseExplicitlyHttp(ReleaseEntry x) => 
                ((x.BaseUrl != null) && Uri.IsWellFormedUriString(x.BaseUrl, UriKind.Absolute));

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly UpdateManager.DownloadReleasesImpl.<>c <>9 = new UpdateManager.DownloadReleasesImpl.<>c();
                public static Action<int> <>9__2_0;

                internal void <DownloadReleases>b__2_0(int _)
                {
                }
            }

            [CompilerGenerated]
            private struct <DownloadReleases>d__2 : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder <>t__builder;
                public UpdateManager.DownloadReleasesImpl <>4__this;
                public string updateUrlOrPath;
                public IFileDownloader urlDownloader;
                public Action<int> progress;
                public IEnumerable<ReleaseEntry> releasesToDownload;
                private UpdateManager.DownloadReleasesImpl.<>c__DisplayClass2_0 <>8__1;
                private TaskAwaiter <>u__1;

                private void MoveNext()
                {
                    int num = this.<>1__state;
                    try
                    {
                        TaskAwaiter awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0004;
                        }
                        else if (num == 1)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                            goto TR_0008;
                        }
                        else
                        {
                            this.<>8__1 = new UpdateManager.DownloadReleasesImpl.<>c__DisplayClass2_0();
                            this.<>8__1.<>4__this = this.<>4__this;
                            this.<>8__1.updateUrlOrPath = this.updateUrlOrPath;
                            this.<>8__1.urlDownloader = this.urlDownloader;
                            this.<>8__1.progress = this.progress;
                            this.<>8__1.progress = this.<>8__1.progress ?? (UpdateManager.DownloadReleasesImpl.<>c.<>9__2_0 ?? (UpdateManager.DownloadReleasesImpl.<>c.<>9__2_0 = new Action<int>(this.<DownloadReleases>b__2_0)));
                            this.<>8__1.urlDownloader = this.<>8__1.urlDownloader ?? new FileDownloader(null);
                            this.<>8__1.packagesDirectory = Path.Combine(this.<>4__this.rootAppDirectory, "packages");
                            this.<>8__1.current = 0.0;
                            this.<>8__1.toIncrement = 100.0 / ((double) this.releasesToDownload.Count<ReleaseEntry>());
                            if (!Utility.IsHttpUrl(this.<>8__1.updateUrlOrPath))
                            {
                                awaiter = this.releasesToDownload.ForEachAsync<ReleaseEntry>(new Action<ReleaseEntry>(this.<>8__1.<DownloadReleases>b__3), 4).GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    goto TR_0008;
                                }
                                else
                                {
                                    this.<>1__state = num = 1;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.DownloadReleasesImpl.<DownloadReleases>d__2>(ref awaiter, ref this);
                                }
                            }
                            else
                            {
                                this.<>4__this.Log<UpdateManager.DownloadReleasesImpl>().Info<string>("Downloading update from {0}", this.<>8__1.updateUrlOrPath);
                                awaiter = this.releasesToDownload.ForEachAsync<ReleaseEntry>(new Func<ReleaseEntry, Task>(this.<>8__1.<DownloadReleases>b__1), 4).GetAwaiter();
                                if (awaiter.IsCompleted)
                                {
                                    goto TR_0004;
                                }
                                else
                                {
                                    this.<>1__state = num = 0;
                                    this.<>u__1 = awaiter;
                                    this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.DownloadReleasesImpl.<DownloadReleases>d__2>(ref awaiter, ref this);
                                }
                            }
                        }
                        return;
                    TR_0004:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        goto TR_0003;
                    TR_0008:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        goto TR_0003;
                    }
                    catch (Exception exception)
                    {
                        this.<>1__state = -2;
                        this.<>t__builder.SetException(exception);
                    }
                    return;
                TR_0003:
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult();
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    this.<>t__builder.SetStateMachine(stateMachine);
                }
            }
        }

        internal class InstallHelperImpl : IEnableLogger
        {
            private readonly string applicationName;
            private readonly string rootAppDirectory;
            private const string currentVersionRegSubKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";
            private const string uninstallRegSubKey = @"Software\Microsoft\Windows\CurrentVersion\Uninstall";

            public InstallHelperImpl(string applicationName, string rootAppDirectory)
            {
                this.applicationName = applicationName;
                this.rootAppDirectory = rootAppDirectory;
            }

            public Task<RegistryKey> CreateUninstallerRegistryEntry()
            {
                string str = Path.Combine(this.rootAppDirectory, "Update.exe");
                return this.CreateUninstallerRegistryEntry($"{str} --uninstall", "-s");
            }

            [AsyncStateMachine(typeof(<CreateUninstallerRegistryEntry>d__5))]
            public Task<RegistryKey> CreateUninstallerRegistryEntry(string uninstallCmd, string quietSwitch)
            {
                <CreateUninstallerRegistryEntry>d__5 d__;
                d__.<>4__this = this;
                d__.uninstallCmd = uninstallCmd;
                d__.quietSwitch = quietSwitch;
                d__.<>t__builder = AsyncTaskMethodBuilder<RegistryKey>.Create();
                d__.<>1__state = -1;
                d__.<>t__builder.Start<<CreateUninstallerRegistryEntry>d__5>(ref d__);
                return d__.<>t__builder.Task;
            }

            public void RemoveUninstallerRegistryEntry()
            {
                RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", true).DeleteSubKeyTree(this.applicationName);
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly UpdateManager.InstallHelperImpl.<>c <>9 = new UpdateManager.InstallHelperImpl.<>c();
                public static Func<ReleaseEntry, bool> <>9__5_0;
                public static Func<ReleaseEntry, SemanticVersion> <>9__5_1;

                internal bool <CreateUninstallerRegistryEntry>b__5_0(ReleaseEntry x) => 
                    !x.IsDelta;

                internal SemanticVersion <CreateUninstallerRegistryEntry>b__5_1(ReleaseEntry x) => 
                    x.Version;
            }

            [CompilerGenerated]
            private struct <CreateUninstallerRegistryEntry>d__5 : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder<RegistryKey> <>t__builder;
                public UpdateManager.InstallHelperImpl <>4__this;
                private string <targetIco>5__1;
                private ZipPackage <zp>5__2;
                private string <targetPng>5__3;
                private RegistryKey <key>5__4;
                private WebClient <wc>5__5;
                public string uninstallCmd;
                public string quietSwitch;
                private string <pkgPath>5__6;
                private TaskAwaiter <>u__1;

                private void MoveNext()
                {
                    int num = this.<>1__state;
                    try
                    {
                        RegistryKey key;
                        if (num != 0)
                        {
                            ReleaseEntry entry = Enumerable.OrderByDescending<ReleaseEntry, SemanticVersion>(Enumerable.Where<ReleaseEntry>(ReleaseEntry.ParseReleaseFile(File.ReadAllText(Path.Combine(this.<>4__this.rootAppDirectory, "packages", "RELEASES"), Encoding.UTF8)), UpdateManager.InstallHelperImpl.<>c.<>9__5_0 ?? (UpdateManager.InstallHelperImpl.<>c.<>9__5_0 = new Func<ReleaseEntry, bool>(this.<CreateUninstallerRegistryEntry>b__5_0))), UpdateManager.InstallHelperImpl.<>c.<>9__5_1 ?? (UpdateManager.InstallHelperImpl.<>c.<>9__5_1 = new Func<ReleaseEntry, SemanticVersion>(this.<CreateUninstallerRegistryEntry>b__5_1))).First<ReleaseEntry>();
                            this.<pkgPath>5__6 = Path.Combine(this.<>4__this.rootAppDirectory, "packages", entry.Filename);
                            this.<zp>5__2 = new ZipPackage(this.<pkgPath>5__6);
                            this.<targetPng>5__3 = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".png");
                            this.<targetIco>5__1 = Path.Combine(this.<>4__this.rootAppDirectory, "app.ico");
                            RegistryKey key2 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).CreateSubKey("Uninstall", RegistryKeyPermissionCheck.ReadWriteSubTree);
                            try
                            {
                            }
                            finally
                            {
                                if ((num < 0) && (key2 != null))
                                {
                                    key2.Dispose();
                                }
                            }
                            this.<key>5__4 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\" + this.<>4__this.applicationName, RegistryKeyPermissionCheck.ReadWriteSubTree);
                            if ((this.<zp>5__2.IconUrl == null) || File.Exists(this.<targetIco>5__1))
                            {
                                goto TR_0011;
                            }
                        }
                        try
                        {
                            if (num != 0)
                            {
                                this.<wc>5__5 = Utility.CreateWebClient();
                            }
                            try
                            {
                                TaskAwaiter awaiter;
                                if (num == 0)
                                {
                                    awaiter = this.<>u__1;
                                    this.<>u__1 = new TaskAwaiter();
                                    this.<>1__state = num = -1;
                                    goto TR_002A;
                                }
                                else
                                {
                                    this.<>4__this.Log<UpdateManager.InstallHelperImpl>().Info<Uri>("Fetching uninstall icon {0}", this.<zp>5__2.IconUrl);
                                    awaiter = this.<wc>5__5.DownloadFileTaskAsync(this.<zp>5__2.IconUrl, this.<targetPng>5__3).GetAwaiter();
                                    if (awaiter.IsCompleted)
                                    {
                                        goto TR_002A;
                                    }
                                    else
                                    {
                                        this.<>1__state = num = 0;
                                        this.<>u__1 = awaiter;
                                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, UpdateManager.InstallHelperImpl.<CreateUninstallerRegistryEntry>d__5>(ref awaiter, ref this);
                                    }
                                }
                                return;
                            TR_002A:
                                awaiter.GetResult();
                                awaiter = new TaskAwaiter();
                                FileStream outputStream = new FileStream(this.<targetIco>5__1, FileMode.Create);
                                try
                                {
                                    if (this.<zp>5__2.IconUrl.AbsolutePath.EndsWith("ico"))
                                    {
                                        byte[] buffer = File.ReadAllBytes(this.<targetPng>5__3);
                                        outputStream.Write(buffer, 0, buffer.Length);
                                    }
                                    else
                                    {
                                        Bitmap bitmap = (Bitmap) Image.FromFile(this.<targetPng>5__3);
                                        try
                                        {
                                            Icon icon = Icon.FromHandle(bitmap.GetHicon());
                                            try
                                            {
                                                icon.Save(outputStream);
                                            }
                                            finally
                                            {
                                                if ((num < 0) && (icon != null))
                                                {
                                                    icon.Dispose();
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            if ((num < 0) && (bitmap != null))
                                            {
                                                bitmap.Dispose();
                                            }
                                        }
                                    }
                                    this.<key>5__4.SetValue("DisplayIcon", this.<targetIco>5__1, RegistryValueKind.String);
                                    goto TR_001A;
                                }
                                finally
                                {
                                    if ((num < 0) && (outputStream != null))
                                    {
                                        outputStream.Dispose();
                                    }
                                }
                            }
                            finally
                            {
                                if ((num < 0) && (this.<wc>5__5 != null))
                                {
                                    this.<wc>5__5.Dispose();
                                }
                            }
                            return;
                        TR_001A:
                            this.<wc>5__5 = null;
                        }
                        catch (Exception exception)
                        {
                            this.<>4__this.Log<UpdateManager.InstallHelperImpl>().InfoException("Couldn't write uninstall icon, don't care", exception);
                        }
                        finally
                        {
                            if (num < 0)
                            {
                                File.Delete(this.<targetPng>5__3);
                            }
                        }
                    TR_0011:
                        <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>[] typeArray5 = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>[8];
                        <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>[] typeArray6 = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>[8];
                        typeArray6[0] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>("DisplayName", this.<zp>5__2.Title ?? (this.<zp>5__2.Description ?? this.<zp>5__2.Summary));
                        <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>[] local5 = typeArray6;
                        local5[1] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>("DisplayVersion", this.<zp>5__2.Version.ToString());
                        local5[2] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>("InstallDate", DateTime.Now.ToString("yyyymmdd"));
                        local5[3] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>("InstallLocation", this.<>4__this.rootAppDirectory);
                        local5[4] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>("Publisher", string.Join(",", this.<zp>5__2.Authors));
                        local5[5] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>("QuietUninstallString", $"{this.uninstallCmd} {this.quietSwitch}");
                        local5[6] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>("UninstallString", this.uninstallCmd);
                        local5[7] = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>("URLUpdateInfo", (this.<zp>5__2.ProjectUrl != null) ? this.<zp>5__2.ProjectUrl.ToString() : "");
                        <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, int>[] typeArray2 = new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, int>[] { new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, int>("EstimatedSize", (int) (new FileInfo(this.<pkgPath>5__6).Length / 0x400L)), new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, int>("NoModify", 1), new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, int>("NoRepair", 1), new <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, int>("Language", 0x409) };
                        <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string>[] typeArray3 = local5;
                        int index = 0;
                        while (true)
                        {
                            if (index >= typeArray3.Length)
                            {
                                <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, int>[] typeArray4 = typeArray2;
                                index = 0;
                                while (true)
                                {
                                    if (index >= typeArray4.Length)
                                    {
                                        key = this.<key>5__4;
                                        break;
                                    }
                                    <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, int> type2 = typeArray4[index];
                                    this.<key>5__4.SetValue(type2.Key, type2.Value, RegistryValueKind.DWord);
                                    index++;
                                }
                                break;
                            }
                            <25b7cc0c-80fe-4b81-9211-16beab382b69><>f__AnonymousType0<string, string> type = typeArray3[index];
                            this.<key>5__4.SetValue(type.Key, type.Value, RegistryValueKind.String);
                            index++;
                        }
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult(key);
                    }
                    catch (Exception exception2)
                    {
                        this.<>1__state = -2;
                        this.<>t__builder.SetException(exception2);
                    }
                }

                [DebuggerHidden]
                private void SetStateMachine(IAsyncStateMachine stateMachine)
                {
                    this.<>t__builder.SetStateMachine(stateMachine);
                }
            }
        }

        [DataContract]
        public class Release
        {
            [DataMember(Name="prerelease")]
            public bool Prerelease { get; set; }

            [DataMember(Name="published_at")]
            public DateTime PublishedAt { get; set; }

            [DataMember(Name="html_url")]
            public string HtmlUrl { get; set; }
        }
    }
}

