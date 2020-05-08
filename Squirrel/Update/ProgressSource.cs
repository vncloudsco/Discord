namespace Squirrel.Update
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class ProgressSource
    {
        [CompilerGenerated]
        private EventHandler<int> Progress;

        public event EventHandler<int> Progress
        {
            [CompilerGenerated] add
            {
                EventHandler<int> progress = this.Progress;
                while (true)
                {
                    EventHandler<int> a = progress;
                    EventHandler<int> handler3 = (EventHandler<int>) Delegate.Combine(a, value);
                    progress = Interlocked.CompareExchange<EventHandler<int>>(ref this.Progress, handler3, a);
                    if (ReferenceEquals(progress, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<int> progress = this.Progress;
                while (true)
                {
                    EventHandler<int> source = progress;
                    EventHandler<int> handler3 = (EventHandler<int>) Delegate.Remove(source, value);
                    progress = Interlocked.CompareExchange<EventHandler<int>>(ref this.Progress, handler3, source);
                    if (ReferenceEquals(progress, source))
                    {
                        return;
                    }
                }
            }
        }

        public void Raise(int i)
        {
            if (this.Progress != null)
            {
                this.Progress(this, i);
            }
        }
    }
}

