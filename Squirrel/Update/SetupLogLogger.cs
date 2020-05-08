namespace Squirrel.Update
{
    using Splat;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal class SetupLogLogger : ILogger, IDisposable
    {
        private TextWriter inner;
        private readonly object gate = 0x2a;

        public SetupLogLogger(bool saveInTemp)
        {
            int num = 0;
            while (true)
            {
                while (true)
                {
                    if (num < 10)
                    {
                        try
                        {
                            FileStream stream = File.Open(Path.Combine(saveInTemp ? Path.GetTempPath() : Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), $"SquirrelSetup.{num}.log".Replace(".0.log", ".log")), FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                            this.inner = new StreamWriter(stream, Encoding.UTF8, 0x1000, false);
                        }
                        catch (Exception exception)
                        {
                            Console.Error.WriteLine("Couldn't open log file, trying new file: " + exception.ToString());
                            break;
                        }
                        return;
                    }
                    else
                    {
                        this.inner = Console.Error;
                        return;
                    }
                    break;
                }
                num++;
            }
        }

        public void Dispose()
        {
            object gate = this.gate;
            lock (gate)
            {
                this.inner.Flush();
                this.inner.Dispose();
            }
        }

        public void Write(string message, LogLevel logLevel)
        {
            if (logLevel >= this.Level)
            {
                string str = $"{Process.GetCurrentProcess().Id}> {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}> {message}";
                object gate = this.gate;
                lock (gate)
                {
                    Console.WriteLine(str);
                    this.inner.WriteLine(str);
                }
            }
        }

        public LogLevel Level { get; set; }
    }
}

