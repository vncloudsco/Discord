namespace Splat
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Threading;

    internal static class DispatcherMixin
    {
        public static Task<T> InvokeAsync<T>(this Dispatcher This, Func<T> block)
        {
            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            This.BeginInvoke(delegate {
                try
                {
                    tcs.SetResult(block());
                }
                catch (Exception exception)
                {
                    tcs.SetException(exception);
                }
            }, new object[0]);
            return tcs.Task;
        }
    }
}

