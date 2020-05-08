namespace Mono.Options
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    public abstract class ArgumentSource
    {
        protected ArgumentSource()
        {
        }

        public static IEnumerable<string> GetArguments(TextReader reader) => 
            GetArguments(reader, false);

        [IteratorStateMachine(typeof(<GetArguments>d__7))]
        private static IEnumerable<string> GetArguments(TextReader reader, bool close)
        {
            <GetArguments>d__7 d__1 = new <GetArguments>d__7(-2);
            d__1.<>3__reader = reader;
            d__1.<>3__close = close;
            return d__1;
        }

        public abstract bool GetArguments(string value, out IEnumerable<string> replacement);
        public static IEnumerable<string> GetArgumentsFromFile(string file) => 
            GetArguments(File.OpenText(file), true);

        public abstract string[] GetNames();

        public abstract string Description { get; }

        [CompilerGenerated]
        private sealed class <GetArguments>d__7 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private string <>2__current;
            private int <>l__initialThreadId;
            private string <line>5__1;
            private StringBuilder <arg>5__2;
            private int <t>5__3;
            private char <c>5__4;
            private int <i>5__5;
            private TextReader reader;
            public TextReader <>3__reader;
            private bool close;
            public bool <>3__close;

            [DebuggerHidden]
            public <GetArguments>d__7(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.close)
                {
                    this.reader.Close();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num2;
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>1__state = -3;
                            this.<arg>5__2 = new StringBuilder();
                            break;

                        case 1:
                            this.<>1__state = -3;
                            this.<arg>5__2.Length = 0;
                            goto TR_0007;

                        case 2:
                            this.<>1__state = -3;
                            this.<arg>5__2.Length = 0;
                            break;

                        default:
                            return false;
                    }
                    goto TR_0018;
                TR_0007:
                    num2 = this.<i>5__5;
                    this.<i>5__5 = num2 + 1;
                TR_0014:
                    while (true)
                    {
                        if (this.<i>5__5 < this.<t>5__3)
                        {
                            this.<c>5__4 = this.<line>5__1[this.<i>5__5];
                            if ((this.<c>5__4 != '"') && (this.<c>5__4 != '\''))
                            {
                                if (this.<c>5__4 != ' ')
                                {
                                    this.<arg>5__2.Append(this.<c>5__4);
                                }
                                else if (this.<arg>5__2.Length > 0)
                                {
                                    this.<>2__current = this.<arg>5__2.ToString();
                                    this.<>1__state = 1;
                                    flag = true;
                                    break;
                                }
                            }
                            else
                            {
                                char ch = this.<c>5__4;
                                num2 = this.<i>5__5;
                                this.<i>5__5 = num2 + 1;
                                while (this.<i>5__5 < this.<t>5__3)
                                {
                                    this.<c>5__4 = this.<line>5__1[this.<i>5__5];
                                    if (this.<c>5__4 == ch)
                                    {
                                        break;
                                    }
                                    this.<arg>5__2.Append(this.<c>5__4);
                                    num2 = this.<i>5__5;
                                    this.<i>5__5 = num2 + 1;
                                }
                            }
                        }
                        else
                        {
                            if (this.<arg>5__2.Length <= 0)
                            {
                                goto TR_0018;
                            }
                            else
                            {
                                this.<>2__current = this.<arg>5__2.ToString();
                                this.<>1__state = 2;
                                flag = true;
                            }
                            break;
                        }
                        goto TR_0007;
                    }
                    return flag;
                TR_0018:
                    while (true)
                    {
                        this.<line>5__1 = this.reader.ReadLine();
                        if (this.<line>5__1 != null)
                        {
                            this.<t>5__3 = this.<line>5__1.Length;
                            this.<i>5__5 = 0;
                        }
                        else
                        {
                            this.<arg>5__2 = null;
                            this.<line>5__1 = null;
                            this.<>m__Finally1();
                            return false;
                        }
                        break;
                    }
                    goto TR_0014;
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
                ArgumentSource.<GetArguments>d__7 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new ArgumentSource.<GetArguments>d__7(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.reader = this.<>3__reader;
                d__.close = this.<>3__close;
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
                if (((num == -3) || (num == 1)) || (num == 2))
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

