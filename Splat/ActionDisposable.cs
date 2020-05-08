namespace Splat
{
    using System;
    using System.Threading;

    internal sealed class ActionDisposable : IDisposable
    {
        private Action block;

        public ActionDisposable(Action block)
        {
            this.block = block;
        }

        public void Dispose()
        {
            Interlocked.Exchange<Action>(ref this.block, delegate {
            })();
        }

        public static IDisposable Empty =>
            new ActionDisposable(delegate {
            });
    }
}

