namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal static class StreamExtensions
    {
        public static Stream AsStream(this string value) => 
            value.AsStream(Encoding.UTF8);

        public static Stream AsStream(this string value, Encoding encoding) => 
            new MemoryStream(encoding.GetBytes(value));

        private static bool CompareBinary(Stream stream, Stream otherStream)
        {
            if (stream.CanSeek && (otherStream.CanSeek && (stream.Length != otherStream.Length)))
            {
                return false;
            }
            byte[] buffer = new byte[0x1000];
            byte[] buffer2 = new byte[0x1000];
            int count = 0;
            while (true)
            {
                count = stream.Read(buffer, 0, buffer.Length);
                if (count > 0)
                {
                    int num2 = otherStream.Read(buffer2, 0, count);
                    if (count != num2)
                    {
                        return false;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        if (buffer[i] != buffer2[i])
                        {
                            return false;
                        }
                    }
                }
                if (count <= 0)
                {
                    return true;
                }
            }
        }

        private static bool CompareText(Stream stream, Stream otherStream) => 
            ReadStreamLines(stream).SequenceEqual<string>(ReadStreamLines(otherStream), StringComparer.Ordinal);

        public static bool ContentEquals(this Stream stream, Stream otherStream)
        {
            otherStream.Seek(0L, SeekOrigin.Begin);
            return (IsBinary(otherStream) ? CompareBinary(stream, otherStream) : CompareText(stream, otherStream));
        }

        public static bool IsBinary(Stream stream)
        {
            byte[] array = new byte[30];
            return (Array.FindIndex<byte>(array, 0, stream.Read(array, 0, 30), d => d == 0) >= 0);
        }

        public static byte[] ReadAllBytes(this Stream stream)
        {
            MemoryStream destination = stream as MemoryStream;
            if (destination != null)
            {
                return destination.ToArray();
            }
            using (destination = new MemoryStream())
            {
                stream.CopyTo(destination);
                return destination.ToArray();
            }
        }

        [IteratorStateMachine(typeof(<ReadStreamLines>d__8))]
        private static IEnumerable<string> ReadStreamLines(Stream stream)
        {
            <ReadStreamLines>d__8 d__1 = new <ReadStreamLines>d__8(-2);
            d__1.<>3__stream = stream;
            return d__1;
        }

        public static string ReadToEnd(this Stream stream)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static Func<Stream> ToStreamFactory(this Stream stream)
        {
            byte[] buffer;
            using (MemoryStream stream2 = new MemoryStream())
            {
                try
                {
                    stream.CopyTo(stream2);
                    buffer = stream2.ToArray();
                }
                finally
                {
                    stream.Close();
                }
            }
            return () => new MemoryStream(buffer);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly StreamExtensions.<>c <>9 = new StreamExtensions.<>c();
            public static Predicate<byte> <>9__6_0;

            internal bool <IsBinary>b__6_0(byte d) => 
                (d == 0);
        }

        [CompilerGenerated]
        private sealed class <ReadStreamLines>d__8 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private string <>2__current;
            private int <>l__initialThreadId;
            private Stream stream;
            public Stream <>3__stream;
            private StreamReader <reader>5__1;
            private bool <hasSeenBeginLine>5__2;

            [DebuggerHidden]
            public <ReadStreamLines>d__8(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<reader>5__1 != null)
                {
                    this.<reader>5__1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<reader>5__1 = new StreamReader(this.stream);
                        this.<>1__state = -3;
                        this.<hasSeenBeginLine>5__2 = false;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    while (true)
                    {
                        if (this.<reader>5__1.Peek() == -1)
                        {
                            this.<>m__Finally1();
                            this.<reader>5__1 = null;
                            flag = false;
                        }
                        else
                        {
                            string str = this.<reader>5__1.ReadLine();
                            if (str.IndexOf(Constants.EndIgnoreMarker, StringComparison.OrdinalIgnoreCase) > -1)
                            {
                                this.<hasSeenBeginLine>5__2 = false;
                                continue;
                            }
                            if (str.IndexOf(Constants.BeginIgnoreMarker, StringComparison.OrdinalIgnoreCase) > -1)
                            {
                                this.<hasSeenBeginLine>5__2 = true;
                                continue;
                            }
                            if (this.<hasSeenBeginLine>5__2)
                            {
                                continue;
                            }
                            this.<>2__current = str;
                            this.<>1__state = 1;
                            flag = true;
                        }
                        break;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                StreamExtensions.<ReadStreamLines>d__8 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new StreamExtensions.<ReadStreamLines>d__8(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.stream = this.<>3__stream;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.String>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            string IEnumerator<string>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

