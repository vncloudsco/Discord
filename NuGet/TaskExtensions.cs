namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class TaskExtensions
    {
        public static TResult WhenAny<TResult>(this Task<TResult>[] tasks, Predicate<TResult> predicate)
        {
            int numTasksRemaining = tasks.Length;
            TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
            Task<TResult>[] taskArray = tasks;
            for (int i = 0; i < taskArray.Length; i++)
            {
                Action<Task<TResult>> <>9__0;
                Action<Task<TResult>> continuationAction = <>9__0;
                if (<>9__0 == null)
                {
                    Action<Task<TResult>> local1 = <>9__0;
                    continuationAction = <>9__0 = delegate (Task<TResult> innerTask) {
                        if ((innerTask.Status == TaskStatus.RanToCompletion) && predicate(innerTask.Result))
                        {
                            tcs.TrySetResult(innerTask.Result);
                        }
                        if (Interlocked.Decrement(ref numTasksRemaining) == 0)
                        {
                            TResult result = default(TResult);
                            tcs.TrySetResult(result);
                        }
                    };
                }
                taskArray[i].ContinueWith(continuationAction, TaskContinuationOptions.ExecuteSynchronously);
            }
            return tcs.Task.Result;
        }
    }
}

