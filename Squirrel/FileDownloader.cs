namespace Squirrel
{
    using Splat;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    internal class FileDownloader : IFileDownloader, IEnableLogger
    {
        private readonly WebClient _providedClient;

        public FileDownloader(WebClient providedClient = null)
        {
            this._providedClient = providedClient;
        }

        [AsyncStateMachine(typeof(<DownloadFile>d__2))]
        public Task DownloadFile(string url, string targetFile, Action<int> progress)
        {
            <DownloadFile>d__2 d__;
            d__.<>4__this = this;
            d__.url = url;
            d__.targetFile = targetFile;
            d__.progress = progress;
            d__.<>t__builder = AsyncTaskMethodBuilder.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<DownloadFile>d__2>(ref d__);
            return d__.<>t__builder.Task;
        }

        [AsyncStateMachine(typeof(<DownloadUrl>d__3))]
        public Task<byte[]> DownloadUrl(string url)
        {
            <DownloadUrl>d__3 d__;
            d__.<>4__this = this;
            d__.url = url;
            d__.<>t__builder = AsyncTaskMethodBuilder<byte[]>.Create();
            d__.<>1__state = -1;
            d__.<>t__builder.Start<<DownloadUrl>d__3>(ref d__);
            return d__.<>t__builder.Task;
        }

        [CompilerGenerated]
        private struct <DownloadFile>d__2 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder <>t__builder;
            public Action<int> progress;
            public string url;
            public string targetFile;
            public FileDownloader <>4__this;
            private FileDownloader.<>c__DisplayClass2_0 <>8__1;
            private FileDownloader.<>c__DisplayClass2_2 <>8__2;
            private TaskAwaiter <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    if (num != 0)
                    {
                        FileDownloader.<>c__DisplayClass2_1 class_;
                        Action<int> progress = this.progress;
                        string url = this.url;
                        string targetFile = this.targetFile;
                        this.<>8__2 = new FileDownloader.<>c__DisplayClass2_2();
                        this.<>8__2.CS$<>8__locals1 = class_;
                        this.<>8__2.wc = this.<>4__this._providedClient ?? Utility.CreateWebClient();
                    }
                    try
                    {
                        if (num != 0)
                        {
                            this.<>8__1 = new FileDownloader.<>c__DisplayClass2_0();
                            this.<>8__1.CS$<>8__locals2 = this.<>8__2;
                            this.<>8__1.failedUrl = null;
                            this.<>8__1.lastSignalled = DateTime.MinValue;
                            this.<>8__1.CS$<>8__locals2.wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(this.<>8__1.<DownloadFile>b__0);
                        }
                        goto TR_0014;
                    TR_000D:
                        this.<>8__1 = null;
                        goto TR_000C;
                    TR_0014:
                        while (true)
                        {
                            try
                            {
                                TaskAwaiter awaiter;
                                if (num == 0)
                                {
                                    awaiter = this.<>u__1;
                                    this.<>u__1 = new TaskAwaiter();
                                    this.<>1__state = num = -1;
                                    goto TR_000E;
                                }
                                else
                                {
                                    this.<>4__this.Log<FileDownloader>().Info("Downloading file: " + (this.<>8__1.failedUrl ?? this.<>8__1.CS$<>8__locals2.CS$<>8__locals1.url));
                                    awaiter = this.<>4__this.WarnIfThrows(new Func<Task>(this.<>8__1.<DownloadFile>b__1), ("Failed downloading URL: " + (this.<>8__1.failedUrl ?? this.<>8__1.CS$<>8__locals2.CS$<>8__locals1.url))).GetAwaiter();
                                    if (awaiter.IsCompleted)
                                    {
                                        goto TR_000E;
                                    }
                                    else
                                    {
                                        this.<>1__state = num = 0;
                                        this.<>u__1 = awaiter;
                                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter, FileDownloader.<DownloadFile>d__2>(ref awaiter, ref this);
                                    }
                                }
                                return;
                            TR_000E:
                                awaiter.GetResult();
                                awaiter = new TaskAwaiter();
                                goto TR_000D;
                            }
                            catch (Exception)
                            {
                                try
                                {
                                    File.Delete(this.<>8__1.CS$<>8__locals2.CS$<>8__locals1.targetFile);
                                }
                                catch (IOException)
                                {
                                }
                                if (this.<>8__1.failedUrl != null)
                                {
                                    throw;
                                }
                                this.<>8__1.failedUrl = this.<>8__1.CS$<>8__locals2.CS$<>8__locals1.url.ToLower();
                                this.<>8__1.CS$<>8__locals2.CS$<>8__locals1.progress(0);
                                continue;
                            }
                            break;
                        }
                    }
                    finally
                    {
                        if ((num < 0) && (this.<>8__2.wc != null))
                        {
                            this.<>8__2.wc.Dispose();
                        }
                    }
                    return;
                TR_000C:
                    this.<>8__2 = null;
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
        private struct <DownloadUrl>d__3 : IAsyncStateMachine
        {
            public int <>1__state;
            public AsyncTaskMethodBuilder<byte[]> <>t__builder;
            public string url;
            public FileDownloader <>4__this;
            private FileDownloader.<>c__DisplayClass3_1 <>8__1;
            private FileDownloader.<>c__DisplayClass3_0 <>8__2;
            private TaskAwaiter<byte[]> <>u__1;

            private void MoveNext()
            {
                int num = this.<>1__state;
                try
                {
                    if (num != 0)
                    {
                        FileDownloader.<>c__DisplayClass3_2 class_;
                        string url = this.url;
                        this.<>8__2 = new FileDownloader.<>c__DisplayClass3_0();
                        this.<>8__2.CS$<>8__locals1 = class_;
                        this.<>8__2.wc = this.<>4__this._providedClient ?? Utility.CreateWebClient();
                    }
                    try
                    {
                        if (num != 0)
                        {
                            this.<>8__1 = new FileDownloader.<>c__DisplayClass3_1();
                            this.<>8__1.CS$<>8__locals2 = this.<>8__2;
                            this.<>8__1.failedUrl = null;
                        }
                        while (true)
                        {
                            try
                            {
                                TaskAwaiter<byte[]> awaiter;
                                if (num == 0)
                                {
                                    awaiter = this.<>u__1;
                                    this.<>u__1 = new TaskAwaiter<byte[]>();
                                    this.<>1__state = num = -1;
                                    goto TR_000A;
                                }
                                else
                                {
                                    this.<>4__this.Log<FileDownloader>().Info("Downloading url: " + (this.<>8__1.failedUrl ?? this.<>8__1.CS$<>8__locals2.CS$<>8__locals1.url));
                                    awaiter = this.<>4__this.WarnIfThrows<byte[]>(new Func<Task<byte[]>>(this.<>8__1.<DownloadUrl>b__0), ("Failed to download url: " + (this.<>8__1.failedUrl ?? this.<>8__1.CS$<>8__locals2.CS$<>8__locals1.url))).GetAwaiter();
                                    if (awaiter.IsCompleted)
                                    {
                                        goto TR_000A;
                                    }
                                    else
                                    {
                                        this.<>1__state = num = 0;
                                        this.<>u__1 = awaiter;
                                        this.<>t__builder.AwaitUnsafeOnCompleted<TaskAwaiter<byte[]>, FileDownloader.<DownloadUrl>d__3>(ref awaiter, ref this);
                                    }
                                }
                                return;
                            TR_000A:
                                awaiter = new TaskAwaiter<byte[]>();
                                byte[] result = awaiter.GetResult();
                                this.<>1__state = -2;
                                this.<>t__builder.SetResult(result);
                                return;
                            }
                            catch (Exception)
                            {
                                if (this.<>8__1.failedUrl != null)
                                {
                                    throw;
                                }
                                this.<>8__1.failedUrl = this.<>8__1.CS$<>8__locals2.CS$<>8__locals1.url.ToLower();
                                continue;
                            }
                            break;
                        }
                    }
                    finally
                    {
                        if ((num < 0) && (this.<>8__2.wc != null))
                        {
                            this.<>8__2.wc.Dispose();
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
    }
}

