namespace Squirrel
{
    using NuGet;
    using Splat;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;

    internal static class Utility
    {
        private static Lazy<string> directoryChars;
        private static IFullLogger logger;

        static Utility()
        {
            directoryChars = new Lazy<string>(() => "abcdefghijklmnopqrstuvwxyz" + Enumerable.Aggregate<int, StringBuilder>(Enumerable.Range(0x3b0, 0x4f).Concat<int>(Enumerable.Range(0x400, 0xff)), new StringBuilder(), delegate (StringBuilder acc, int x) {
                acc.Append(char.ConvertFromUtf32(x));
                return acc;
            }).ToString());
        }

        public static Uri AddQueryParamsToUri(Uri uri, IEnumerable<KeyValuePair<string, string>> newQuery)
        {
            NameValueCollection values = HttpUtility.ParseQueryString(uri.Query);
            foreach (KeyValuePair<string, string> pair in newQuery)
            {
                values[pair.Key] = pair.Value;
            }
            UriBuilder builder1 = new UriBuilder(uri);
            builder1.Query = values.ToString();
            return builder1.Uri;
        }

        public static string AppDirForRelease(string rootAppDirectory, ReleaseEntry entry) => 
            Path.Combine(rootAppDirectory, "app-" + entry.Version.ToString());

        public static string AppDirForVersion(string rootAppDirectory, SemanticVersion version) => 
            Path.Combine(rootAppDirectory, "app-" + version.ToString());

        public static Uri AppendPathToUri(Uri uri, string path)
        {
            UriBuilder builder = new UriBuilder(uri);
            if (!builder.Path.EndsWith("/"))
            {
                builder.Path = builder.Path + "/";
            }
            builder.Path = builder.Path + path;
            return builder.Uri;
        }

        public static string CalculateFileSHA1(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                return CalculateStreamSHA1(stream);
            }
        }

        public static string CalculateStreamSHA1(Stream file)
        {
            using (SHA1 sha = SHA1.Create())
            {
                return BitConverter.ToString(sha.ComputeHash(file)).Replace("-", string.Empty);
            }
        }

        [AsyncStateMachine(typeof(<CopyToAsync>d__10))]
        public static Task CopyToAsync(string from, string to)
        {
            <CopyToAsync>d__10 d__;
            d__.from = from;
            d__.to = to;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<CopyToAsync>d__10>(ref d__);
            return d__.<>t__builder.Task;
        }

        public static WebClient CreateWebClient()
        {
            WebClient client = new WebClient();
            IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
            if (defaultWebProxy != null)
            {
                defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
                client.Proxy = defaultWebProxy;
            }
            return client;
        }

        [AsyncStateMachine(typeof(<DeleteDirectory>d__22))]
        public static Task DeleteDirectory(string directoryPath)
        {
            <DeleteDirectory>d__22 d__;
            d__.directoryPath = directoryPath;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<DeleteDirectory>d__22>(ref d__);
            return d__.<>t__builder.Task;
        }

        [AsyncStateMachine(typeof(<DeleteDirectoryOrJustGiveUp>d__35))]
        public static Task DeleteDirectoryOrJustGiveUp(string dir)
        {
            <DeleteDirectoryOrJustGiveUp>d__35 d__;
            d__.dir = dir;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<DeleteDirectoryOrJustGiveUp>d__35>(ref d__);
            return d__.<>t__builder.Task;
        }

        public static void DeleteFileHarder(string path, bool ignoreIfFails = false)
        {
            try
            {
                () => File.Delete(path).Retry(2);
            }
            catch (Exception exception)
            {
                if (!ignoreIfFails)
                {
                    LogHost.Default.ErrorException("Really couldn't delete file: " + path, exception);
                    throw;
                }
            }
        }

        public static Uri EnsureTrailingSlash(Uri uri) => 
            AppendPathToUri(uri, "");

        public static void ErrorIfThrows(this IEnableLogger This, Action block, string message = null)
        {
            This.Log<IEnableLogger>().LogIfThrows(LogLevel.Error, message, block);
        }

        public static Task ErrorIfThrows(this IEnableLogger This, Func<Task> block, string message = null) => 
            This.Log<IEnableLogger>().LogIfThrows(LogLevel.Error, message, block);

        public static Task<T> ErrorIfThrows<T>(this IEnableLogger This, Func<Task<T>> block, string message = null) => 
            This.Log<IEnableLogger>().LogIfThrows<T>(LogLevel.Error, message, block);

        public static ReleaseEntry FindCurrentVersion(IEnumerable<ReleaseEntry> localReleases) => 
            (localReleases.Any<ReleaseEntry>() ? (from x in localReleases
                where !x.IsDelta
                select x).MaxBy<ReleaseEntry, SemanticVersion>(x => x.Version).FirstOrDefault<ReleaseEntry>() : null);

        public static Task ForEachAsync<T>(this IEnumerable<T> source, Action<T> body, int degreeOfParallelism = 4) => 
            source.ForEachAsync<T>(((Func<T, Task>) (x => Task.Run(delegate {
                body(x);
            }))), degreeOfParallelism);

        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body, int degreeOfParallelism = 4) => 
            Task.WhenAll((IEnumerable<Task>) (from partition in Partitioner.Create<T>(source).GetPartitions(degreeOfParallelism) select Task.Run(delegate {
                <>c__DisplayClass16_0<T>.<<ForEachAsync>b__1>d local;
                local.<>4__this = class_1;
                local.<>t__builder = AsyncTaskMethodBuilder.Create();
                local.<>1__state = -1;
                local.<>t__builder.Start<<>c__DisplayClass16_0<T>.<<ForEachAsync>b__1>d>(ref local);
                return local.<>t__builder.Task;
            })));

        public static IEnumerable<string> GetAllFilePathsRecursively(string rootPath) => 
            Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories);

        public static IEnumerable<FileInfo> GetAllFilesRecursively(this DirectoryInfo rootPath) => 
            rootPath.EnumerateFiles("*", SearchOption.AllDirectories);

        public static string GetAssembyLocation()
        {
            if (Debugger.IsAttached)
            {
                List<string> list = PathSplit(Environment.CurrentDirectory);
                for (int i = 0; i < list.Count; i++)
                {
                    string path = Path.Combine(Path.Combine(list.ToArray()), "update.exe");
                    if (File.Exists(path))
                    {
                        return path;
                    }
                    list.RemoveAt(list.Count - 1);
                }
            }
            return Assembly.GetEntryAssembly().Location;
        }

        public static DirectoryInfo GetTempDirectory(string localAppDirectory)
        {
            DirectoryInfo info = new DirectoryInfo(Environment.GetEnvironmentVariable("SQUIRREL_TEMP") ?? Path.Combine(localAppDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SquirrelTemp"));
            if (!info.Exists)
            {
                info.Create();
            }
            return info;
        }

        [AsyncStateMachine(typeof(<InvokeProcessAsync>d__14))]
        public static Task<Tuple<int, string>> InvokeProcessAsync(ProcessStartInfo psi, CancellationToken ct)
        {
            <InvokeProcessAsync>d__14 d__;
            d__.psi = psi;
            d__.ct = ct;
            d__.<>t__builder = AsyncTaskMethodBuilder<Tuple<int, string>>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<InvokeProcessAsync>d__14>(ref d__);
            return d__.<>t__builder.Task;
        }

        public static Task<Tuple<int, string>> InvokeProcessAsync(string fileName, string arguments, CancellationToken ct)
        {
            ProcessStartInfo psi = new ProcessStartInfo(fileName, arguments);
            psi.UseShellExecute = false;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.ErrorDialog = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            return InvokeProcessAsync(psi, ct);
        }

        public static bool IsHttpUrl(string urlOrPath)
        {
            Uri result = null;
            return (Uri.TryCreate(urlOrPath, UriKind.Absolute, out result) ? ((result.Scheme == Uri.UriSchemeHttp) || (result.Scheme == Uri.UriSchemeHttps)) : false);
        }

        public static IEnumerable<ReleaseEntry> LoadLocalReleases(string localReleaseFile)
        {
            using (StreamReader reader = new StreamReader(File.OpenRead(localReleaseFile), Encoding.UTF8))
            {
                return ReleaseEntry.ParseReleaseFile(reader.ReadToEnd());
            }
        }

        public static string LocalReleaseFileForAppDir(string rootAppDirectory) => 
            Path.Combine(PackageDirectoryForAppDir(rootAppDirectory), "RELEASES");

        private static IFullLogger Log() => 
            (logger ?? (logger = Locator.CurrentMutable.GetService<ILogManager>(null).GetLogger(typeof(Utility))));

        public static void LogIfThrows(this IFullLogger This, LogLevel level, string message, Action block)
        {
            try
            {
                block();
            }
            catch (Exception exception)
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        This.DebugException(message ?? "", exception);
                        break;

                    case LogLevel.Info:
                        This.InfoException(message ?? "", exception);
                        break;

                    case LogLevel.Warn:
                        This.WarnException(message ?? "", exception);
                        break;

                    case LogLevel.Error:
                        This.ErrorException(message ?? "", exception);
                        break;

                    default:
                        break;
                }
                throw;
            }
        }

        [AsyncStateMachine(typeof(<LogIfThrows>d__37))]
        public static Task LogIfThrows(this IFullLogger This, LogLevel level, string message, Func<Task> block)
        {
            <LogIfThrows>d__37 d__;
            d__.This = This;
            d__.level = level;
            d__.message = message;
            d__.block = block;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<LogIfThrows>d__37>(ref d__);
            return d__.<>t__builder.Task;
        }

        [AsyncStateMachine(typeof(<LogIfThrows>d__38))]
        public static Task<T> LogIfThrows<T>(this IFullLogger This, LogLevel level, string message, Func<Task<T>> block)
        {
            <LogIfThrows>d__38<T> d__;
            d__.This = This;
            d__.level = level;
            d__.message = message;
            d__.block = block;
            d__.<>t__builder = AsyncTaskMethodBuilder<T>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<LogIfThrows>d__38<T>>(ref d__);
            return d__.<>t__builder.Task;
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);
        public static string PackageDirectoryForAppDir(string rootAppDirectory) => 
            Path.Combine(rootAppDirectory, "packages");

        private static List<string> PathSplit(string path)
        {
            char[] separator = new char[] { Path.DirectorySeparatorChar };
            List<string> list = new List<string>(path.Split(separator));
            if (Path.IsPathRooted(path))
            {
                list[0] = list[0] + Path.DirectorySeparatorChar.ToString();
            }
            return list;
        }

        public static string RemoveByteOrderMarkerIfPresent(string content) => 
            (string.IsNullOrEmpty(content) ? string.Empty : RemoveByteOrderMarkerIfPresent(Encoding.UTF8.GetBytes(content)));

        public static string RemoveByteOrderMarkerIfPresent(byte[] content)
        {
            byte[] dst = new byte[0];
            if (content != null)
            {
                Func<byte[], byte[], bool> func = (bom, src) => (src.Length >= bom.Length) ? !Enumerable.Where<byte>(bom, (chr, index) => src[index] != chr).Any<byte>() : false;
                byte[] buffer1 = new byte[4];
                buffer1[2] = 0xfe;
                buffer1[3] = 0xff;
                byte[] buffer2 = buffer1;
                byte[] buffer7 = new byte[4];
                buffer7[0] = 0xff;
                buffer7[1] = 0xfe;
                byte[] buffer3 = buffer7;
                byte[] buffer4 = new byte[] { 0xfe, 0xff };
                byte[] buffer5 = new byte[] { 0xff, 0xfe };
                byte[] buffer6 = new byte[] { 0xef, 0xbb, 0xbf };
                dst = !func(buffer2, content) ? (!func(buffer3, content) ? (!func(buffer4, content) ? (!func(buffer5, content) ? (!func(buffer6, content) ? content : new byte[content.Length - buffer6.Length]) : new byte[content.Length - buffer5.Length]) : new byte[content.Length - buffer4.Length]) : new byte[content.Length - buffer3.Length]) : new byte[content.Length - buffer2.Length];
            }
            if (dst.Length != 0)
            {
                Buffer.BlockCopy(content, content.Length - dst.Length, dst, 0, dst.Length);
            }
            return Encoding.UTF8.GetString(dst);
        }

        public static void Retry(this Action block, int retries = 2)
        {
            delegate {
                block();
                return null;
            }.Retry<object>(retries);
        }

        public static T Retry<T>(this Func<T> block, int retries = 2)
        {
            T local;
            while (true)
            {
                try
                {
                    local = block();
                    break;
                }
                catch (Exception)
                {
                    if (retries == 0)
                    {
                        throw;
                    }
                    retries--;
                    Thread.Sleep(250);
                }
            }
            return local;
        }

        private static TAcc scan<T, TAcc>(this IEnumerable<T> This, TAcc initialValue, Func<TAcc, T, TAcc> accFunc)
        {
            TAcc local = initialValue;
            foreach (T local2 in This)
            {
                local = accFunc(local, local2);
            }
            return local;
        }

        internal static string tempNameForIndex(int index, string prefix) => 
            ((index >= directoryChars.Value.Length) ? (prefix + directoryChars.Value[index % directoryChars.Value.Length].ToString() + tempNameForIndex(index / directoryChars.Value.Length, "")) : (prefix + directoryChars.Value[index].ToString()));

        public static void WaitForDebugger(bool andBreak)
        {
            while (!Debugger.IsAttached)
            {
                Thread.Sleep(0x3e8);
            }
            if (andBreak)
            {
                Debugger.Break();
            }
        }

        public static void WarnIfThrows(this IEnableLogger This, Action block, string message = null)
        {
            This.Log<IEnableLogger>().LogIfThrows(LogLevel.Warn, message, block);
        }

        public static Task WarnIfThrows(this IEnableLogger This, Func<Task> block, string message = null) => 
            This.Log<IEnableLogger>().LogIfThrows(LogLevel.Warn, message, block);

        public static Task<T> WarnIfThrows<T>(this IEnableLogger This, Func<Task<T>> block, string message = null) => 
            This.Log<IEnableLogger>().LogIfThrows<T>(LogLevel.Warn, message, block);

        public static IDisposable WithTempDirectory(out string path, string localAppDirectory = null)
        {
            <>c__DisplayClass20_0 class_;
            DirectoryInfo tempDirectory = GetTempDirectory(localAppDirectory);
            DirectoryInfo tempDir = null;
            foreach (string str in from x in Enumerable.Range(0, 0x100000) select tempNameForIndex(x, "temp"))
            {
                string str2 = Path.Combine(tempDirectory.FullName, str);
                if (!File.Exists(str2) && !Directory.Exists(str2))
                {
                    Directory.CreateDirectory(str2);
                    tempDir = new DirectoryInfo(str2);
                    break;
                }
            }
            path = tempDir.FullName;
            return Disposable.Create(delegate {
                Func<Task> <>9__2;
                Func<Task> function = <>9__2;
                if (<>9__2 == null)
                {
                    Func<Task> local1 = <>9__2;
                    function = <>9__2 = delegate {
                        <>c__DisplayClass20_0.<<WithTempDirectory>b__2>d local;
                        local.<>4__this = class_;
                        local.<>t__builder = AsyncTaskMethodBuilder.Create();
                        local.<>1__state = -1;
                        local.<>t__builder.Start<<>c__DisplayClass20_0.<<WithTempDirectory>b__2>d>(ref local);
                        return local.<>t__builder.Task;
                    };
                }
                Task.Run(function).Wait();
            });
        }

        public static IDisposable WithTempFile(out string path, string localAppDirectory = null)
        {
            DirectoryInfo tempDirectory = GetTempDirectory(localAppDirectory);
            path = "";
            foreach (string str in from x in Enumerable.Range(0, 0x100000) select tempNameForIndex(x, "temp"))
            {
                path = Path.Combine(tempDirectory.FullName, str);
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    break;
                }
            }
            string thePath = path;
            return Disposable.Create(delegate {
                File.Delete(thePath);
            });
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Utility.<>c <>9 = new Utility.<>c();
            public static Func<byte[], byte[], bool> <>9__4_0;
            public static Func<int, string> <>9__20_0;
            public static Func<int, string> <>9__21_0;
            public static Action<string> <>9__22_0;
            public static Func<string, Task> <>9__22_1;
            public static Func<ReleaseEntry, bool> <>9__28_0;
            public static Func<ReleaseEntry, SemanticVersion> <>9__28_1;
            public static Func<StringBuilder, int, StringBuilder> <>9__49_1;

            internal string <.cctor>b__49_0() => 
                ("abcdefghijklmnopqrstuvwxyz" + Enumerable.Aggregate<int, StringBuilder>(Enumerable.Range(0x3b0, 0x4f).Concat<int>(Enumerable.Range(0x400, 0xff)), new StringBuilder(), delegate (StringBuilder acc, int x) {
                    acc.Append(char.ConvertFromUtf32(x));
                    return acc;
                }).ToString());

            internal StringBuilder <.cctor>b__49_1(StringBuilder acc, int x)
            {
                acc.Append(char.ConvertFromUtf32(x));
                return acc;
            }

            internal void <DeleteDirectory>b__22_0(string file)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            [AsyncStateMachine(typeof(<<DeleteDirectory>b__22_1>d))]
            internal Task <DeleteDirectory>b__22_1(string dir)
            {
                <<DeleteDirectory>b__22_1>d local;
                local.dir = dir;
                local.<>t__builder = AsyncTaskMethodBuilder.Create();
                local.<>1__state = -1;
                local.<>t__builder.Start<<<DeleteDirectory>b__22_1>d>(ref local);
                return local.<>t__builder.Task;
            }

            internal bool <FindCurrentVersion>b__28_0(ReleaseEntry x) => 
                !x.IsDelta;

            internal SemanticVersion <FindCurrentVersion>b__28_1(ReleaseEntry x) => 
                x.Version;

            internal bool <RemoveByteOrderMarkerIfPresent>b__4_0(byte[] bom, byte[] src) => 
                ((src.Length >= bom.Length) ? !Enumerable.Where<byte>(bom, (chr, index) => src[index] != chr).Any<byte>() : false);

            internal string <WithTempDirectory>b__20_0(int x) => 
                Utility.tempNameForIndex(x, "temp");

            internal string <WithTempFile>b__21_0(int x) => 
                Utility.tempNameForIndex(x, "temp");

            private struct <<DeleteDirectory>b__22_1>d : IAsyncStateMachine
            {
                public int <>1__state;
                public AsyncTaskMethodBuilder <>t__builder;
                public string dir;
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
                        else
                        {
                            awaiter = Utility.DeleteDirectory(this.dir).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto TR_0004;
                            }
                            else
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Utility.<>c.<<DeleteDirectory>b__22_1>d>(ref awaiter, ref this);
                            }
                        }
                        return;
                    TR_0004:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
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
        }

        [CompilerGenerated]
        private struct <CopyToAsync>d__10 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public string from;
            public string to;
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
                        string from = this.from;
                        string to = this.to;
                        if (File.Exists(from))
                        {
                            Utility.<>c__DisplayClass10_0 class_;
                            awaiter = Task.Run(new Action(class_.<CopyToAsync>b__0)).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto TR_0005;
                            }
                            else
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Utility.<CopyToAsync>d__10>(ref awaiter, ref this);
                            }
                            return;
                        }
                        else
                        {
                            Utility.Log().Warn<string>("The file {0} does not exist", from);
                        }
                    }
                    goto TR_0002;
                TR_0005:
                    awaiter.GetResult();
                    awaiter = new TaskAwaiter();
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
        private struct <DeleteDirectory>d__22 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public string directoryPath;
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
                        Utility.Log().Debug<string>("Starting to delete folder: {0}", this.directoryPath);
                        if (Directory.Exists(this.directoryPath))
                        {
                            string[] source = new string[0];
                            try
                            {
                                source = Directory.GetFiles(this.directoryPath);
                            }
                            catch (UnauthorizedAccessException exception1)
                            {
                                string message = $"The files inside {this.directoryPath} could not be read";
                                Utility.Log().Warn<UnauthorizedAccessException>(message, exception1);
                            }
                            string[] directories = new string[0];
                            try
                            {
                                directories = Directory.GetDirectories(this.directoryPath);
                            }
                            catch (UnauthorizedAccessException exception5)
                            {
                                string message = $"The directories inside {this.directoryPath} could not be read";
                                Utility.Log().Warn<UnauthorizedAccessException>(message, exception5);
                            }
                            Task task = source.ForEachAsync<string>(Utility.<>c.<>9__22_0 ?? (Utility.<>c.<>9__22_0 = new Action<string>(this.<DeleteDirectory>b__22_0)), 4);
                            Task task2 = directories.ForEachAsync<string>(Utility.<>c.<>9__22_1 ?? (Utility.<>c.<>9__22_1 = new Func<string, Task>(this.<DeleteDirectory>b__22_1)), 4);
                            Task[] tasks = new Task[] { task, task2 };
                            awaiter = Task.WhenAll(tasks).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto TR_0008;
                            }
                            else
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Utility.<DeleteDirectory>d__22>(ref awaiter, ref this);
                            }
                            return;
                        }
                        else
                        {
                            Utility.Log().Warn<string>("DeleteDirectory: does not exist - {0}", this.directoryPath);
                        }
                    }
                    goto TR_0002;
                TR_0008:
                    awaiter.GetResult();
                    awaiter = new TaskAwaiter();
                    Utility.Log().Debug<string>("Now deleting folder: {0}", this.directoryPath);
                    File.SetAttributes(this.directoryPath, FileAttributes.Normal);
                    try
                    {
                        Directory.Delete(this.directoryPath, false);
                    }
                    catch (Exception exception6)
                    {
                        string message = $"DeleteDirectory: could not delete - {this.directoryPath}";
                        Utility.Log().ErrorException(message, exception6);
                    }
                }
                catch (Exception exception4)
                {
                    this.<>1__state = -2;
                    this.<>t__builder.SetException(exception4);
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
        private struct <DeleteDirectoryOrJustGiveUp>d__35 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public string dir;
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
                            goto TR_0005;
                        }
                        else
                        {
                            awaiter = Utility.DeleteDirectory(this.dir).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto TR_0005;
                            }
                            else
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Utility.<DeleteDirectoryOrJustGiveUp>d__35>(ref awaiter, ref this);
                            }
                        }
                        return;
                    TR_0005:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                    }
                    catch (Exception exception)
                    {
                        $"Uninstall failed to delete dir '{this.dir}': {exception}";
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

        [CompilerGenerated]
        private struct <InvokeProcessAsync>d__14 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<Tuple<int, string>> <>t__builder;
            public CancellationToken ct;
            public ProcessStartInfo psi;
            private Utility.<>c__DisplayClass14_0 <>8__1;
            private TaskAwaiter <>u__1;
            private TaskAwaiter<string> <>u__2;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    string str;
                    TaskAwaiter awaiter;
                    TaskAwaiter<string> awaiter2;
                    switch (num)
                    {
                        case 0:
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter();
                            this.<>1__state = num = -1;
                            break;

                        case 1:
                            awaiter2 = this.<>u__2;
                            this.<>u__2 = new TaskAwaiter<string>();
                            this.<>1__state = num = -1;
                            goto TR_000C;

                        case 2:
                            awaiter2 = this.<>u__2;
                            this.<>u__2 = new TaskAwaiter<string>();
                            this.<>1__state = num = -1;
                            goto TR_0008;

                        default:
                            this.<>8__1 = new Utility.<>c__DisplayClass14_0();
                            this.<>8__1.ct = this.ct;
                            this.<>8__1.pi = Process.Start(this.psi);
                            Utility.Log().Info<string, string, int>("Process Started: {0} {1}, pid {2}", this.psi.FileName, this.psi.Arguments, this.<>8__1.pi.Id);
                            awaiter = Task.Run(new Action(this.<>8__1.<InvokeProcessAsync>b__0)).GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                break;
                            }
                            this.<>1__state = num = 0;
                            this.<>u__1 = awaiter;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Utility.<InvokeProcessAsync>d__14>(ref awaiter, ref this);
                            return;
                    }
                    awaiter.GetResult();
                    awaiter = new TaskAwaiter();
                    awaiter2 = this.<>8__1.pi.StandardOutput.ReadToEndAsync().GetAwaiter();
                    if (!awaiter2.IsCompleted)
                    {
                        this.<>1__state = num = 1;
                        this.<>u__2 = awaiter2;
                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<string>, Utility.<InvokeProcessAsync>d__14>(ref awaiter2, ref this);
                        return;
                    }
                    goto TR_000C;
                TR_0006:
                    Utility.Log().Info<int, string>("Received exitcode {0} from process {1}", this.<>8__1.pi.ExitCode, this.psi.FileName);
                    Tuple<int, string> result = Tuple.Create<int, string>(this.<>8__1.pi.ExitCode, str.Trim());
                    this.<>1__state = -2;
                    this.<>t__builder.SetResult(result);
                    return;
                TR_0008:
                    awaiter2 = new TaskAwaiter<string>();
                    str = awaiter2.GetResult();
                    if (string.IsNullOrWhiteSpace(str))
                    {
                        str = string.Empty;
                    }
                    goto TR_0006;
                TR_000C:
                    awaiter2 = new TaskAwaiter<string>();
                    str = awaiter2.GetResult();
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        goto TR_0006;
                    }
                    else
                    {
                        awaiter2 = this.<>8__1.pi.StandardError.ReadToEndAsync().GetAwaiter();
                        if (awaiter2.IsCompleted)
                        {
                            goto TR_0008;
                        }
                        else
                        {
                            this.<>1__state = num = 2;
                            this.<>u__2 = awaiter2;
                            this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<string>, Utility.<InvokeProcessAsync>d__14>(ref awaiter2, ref this);
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

        [CompilerGenerated]
        private struct <LogIfThrows>d__37 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public Func<Task> block;
            public LogLevel level;
            public IFullLogger This;
            public string message;
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
                            goto TR_000B;
                        }
                        else
                        {
                            awaiter = this.block().GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto TR_000B;
                            }
                            else
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, Utility.<LogIfThrows>d__37>(ref awaiter, ref this);
                            }
                        }
                        return;
                    TR_000B:
                        awaiter.GetResult();
                        awaiter = new TaskAwaiter();
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult();
                    }
                    catch (Exception exception)
                    {
                        switch (this.level)
                        {
                            case LogLevel.Debug:
                                this.This.DebugException(this.message ?? "", exception);
                                break;

                            case LogLevel.Info:
                                this.This.InfoException(this.message ?? "", exception);
                                break;

                            case LogLevel.Warn:
                                this.This.WarnException(this.message ?? "", exception);
                                break;

                            case LogLevel.Error:
                                this.This.ErrorException(this.message ?? "", exception);
                                break;

                            default:
                                break;
                        }
                        throw;
                    }
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
        private struct <LogIfThrows>d__38<T> : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<T> <>t__builder;
            public Func<Task<T>> block;
            public LogLevel level;
            public IFullLogger This;
            public string message;
            private TaskAwaiter<T> <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    int num1 = num;
                    try
                    {
                        TaskAwaiter<T> awaiter;
                        if (num == 0)
                        {
                            awaiter = this.<>u__1;
                            this.<>u__1 = new TaskAwaiter<T>();
                            this.<>1__state = num = -1;
                            goto TR_000B;
                        }
                        else
                        {
                            awaiter = this.block().GetAwaiter();
                            if (awaiter.IsCompleted)
                            {
                                goto TR_000B;
                            }
                            else
                            {
                                this.<>1__state = num = 0;
                                this.<>u__1 = awaiter;
                                this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<T>, Utility.<LogIfThrows>d__38<T>>(ref awaiter, ref (Utility.<LogIfThrows>d__38<T>) ref this);
                            }
                        }
                        return;
                    TR_000B:
                        awaiter = new TaskAwaiter<T>();
                        T result = awaiter.GetResult();
                        this.<>1__state = -2;
                        this.<>t__builder.SetResult(result);
                    }
                    catch (Exception exception)
                    {
                        switch (this.level)
                        {
                            case LogLevel.Debug:
                                this.This.DebugException(this.message ?? "", exception);
                                break;

                            case LogLevel.Info:
                                this.This.InfoException(this.message ?? "", exception);
                                break;

                            case LogLevel.Warn:
                                this.This.WarnException(this.message ?? "", exception);
                                break;

                            case LogLevel.Error:
                                this.This.ErrorException(this.message ?? "", exception);
                                break;

                            default:
                                break;
                        }
                        throw;
                    }
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

        [Flags]
        private enum MoveFileFlags
        {
            MOVEFILE_REPLACE_EXISTING = 1,
            MOVEFILE_COPY_ALLOWED = 2,
            MOVEFILE_DELAY_UNTIL_REBOOT = 4,
            MOVEFILE_WRITE_THROUGH = 8,
            MOVEFILE_CREATE_HARDLINK = 0x10,
            MOVEFILE_FAIL_IF_NOT_TRACKABLE = 0x20
        }
    }
}

