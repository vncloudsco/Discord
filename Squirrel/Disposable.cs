namespace Squirrel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal static class Disposable
    {
        public static IDisposable Create(Action action) => 
            new AnonDisposable(action);

        private class AnonDisposable : IDisposable
        {
            private static readonly Action dummyBlock = delegate {
            };
            private Action block;

            public AnonDisposable(Action b)
            {
                this.block = b;
            }

            public void Dispose()
            {
                Interlocked.Exchange<Action>(ref this.block, dummyBlock)();
            }

            [Serializable, CompilerGenerated]
            private sealed class <>c
            {
                public static readonly Disposable.AnonDisposable.<>c <>9 = new Disposable.AnonDisposable.<>c();

                internal void <.cctor>b__4_0()
                {
                }
            }
        }
    }
}

