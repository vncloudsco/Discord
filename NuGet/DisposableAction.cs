namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal sealed class DisposableAction : IDisposable
    {
        public static readonly DisposableAction NoOp = new DisposableAction(delegate {
        });
        private Action _action;

        public DisposableAction(Action action)
        {
            this._action = action;
        }

        public static IDisposable All(IEnumerable<IDisposable> tokens) => 
            All(tokens.ToArray<IDisposable>());

        public static IDisposable All(params IDisposable[] tokens) => 
            new DisposableAction(delegate {
                IDisposable[] disposableArray = tokens;
                for (int i = 0; i < disposableArray.Length; i++)
                {
                    disposableArray[i].Dispose();
                }
            });

        public void Dispose()
        {
            this._action();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DisposableAction.<>c <>9 = new DisposableAction.<>c();

            internal void <.cctor>b__6_0()
            {
            }
        }
    }
}

