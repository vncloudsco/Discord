namespace Squirrel
{
    using Splat;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    internal sealed class SingleGlobalInstance : IDisposable, IEnableLogger
    {
        private IDisposable handle;

        public SingleGlobalInstance(string key, TimeSpan timeOut)
        {
            if (!ModeDetector.InUnitTestRunner())
            {
                string path = Path.Combine(Path.GetTempPath(), ".squirrel-lock-" + key);
                Stopwatch stopwatch = new Stopwatch();
                this.Log<SingleGlobalInstance>().Info("Grabbing lockfile with timeout of " + timeOut);
                stopwatch.Start();
                FileStream fh = null;
                while (stopwatch.Elapsed < timeOut)
                {
                    try
                    {
                        fh = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Delete);
                        byte[] buffer = new byte[] { 0xba, 0xad, 240, 13 };
                        fh.Write(buffer, 0, 4);
                        break;
                    }
                    catch (Exception exception)
                    {
                        this.Log<SingleGlobalInstance>().WarnException("Failed to grab lockfile, will retry: " + path, exception);
                        Thread.Sleep(250);
                    }
                }
                stopwatch.Stop();
                if (fh == null)
                {
                    throw new Exception("Couldn't acquire lock, is another instance running");
                }
                this.handle = Disposable.Create(delegate {
                    fh.Dispose();
                    File.Delete(path);
                });
            }
        }

        public void Dispose()
        {
            if (!ModeDetector.InUnitTestRunner())
            {
                IDisposable disposable = Interlocked.Exchange<IDisposable>(ref this.handle, null);
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }

        ~SingleGlobalInstance()
        {
            if (this.handle != null)
            {
                throw new AbandonedMutexException("Leaked a Mutex!");
            }
        }
    }
}

