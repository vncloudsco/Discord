namespace NuGet.Runtime
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class BindingRedirectResolver
    {
        [IteratorStateMachine(typeof(<GetAssemblies>d__6))]
        private static IEnumerable<IAssembly> GetAssemblies(IEnumerable<string> paths, AppDomain domain)
        {
            <GetAssemblies>d__6 d__1 = new <GetAssemblies>d__6(-2);
            d__1.<>3__paths = paths;
            d__1.<>3__domain = domain;
            return d__1;
        }

        private static IEnumerable<IAssembly> GetAssemblies(string path, AppDomain domain) => 
            (Directory.Exists(path) ? GetAssemblies(Directory.GetFiles(path, "*.dll"), domain).Concat<IAssembly>(GetAssemblies(Directory.GetFiles(path, "*.exe"), domain)) : Enumerable.Empty<IAssembly>());

        public static IEnumerable<AssemblyBinding> GetBindingRedirects(IEnumerable<IAssembly> assemblies)
        {
            if (assemblies == null)
            {
                throw new ArgumentNullException("assemblies");
            }
            List<IAssembly> list1 = assemblies.ToList<IAssembly>();
            Dictionary<Tuple<string, string>, IAssembly> dictionary = Enumerable.ToDictionary<IAssembly, Tuple<string, string>>(list1, new Func<IAssembly, Tuple<string, string>>(BindingRedirectResolver.GetUniqueKey));
            HashSet<IAssembly> set = new HashSet<IAssembly>();
            using (List<IAssembly>.Enumerator enumerator = list1.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    foreach (IAssembly assembly in enumerator.Current.ReferencedAssemblies)
                    {
                        IAssembly assembly2;
                        Tuple<string, string> uniqueKey = GetUniqueKey(assembly);
                        if (dictionary.TryGetValue(uniqueKey, out assembly2) && ((assembly2.Version != assembly.Version) && !string.IsNullOrEmpty(assembly2.PublicKeyToken)))
                        {
                            set.Add(assembly2);
                        }
                    }
                }
            }
            return (from a in set select new AssemblyBinding(a));
        }

        public static IEnumerable<AssemblyBinding> GetBindingRedirects(string path) => 
            GetBindingRedirects(path, AppDomain.CurrentDomain);

        public static IEnumerable<AssemblyBinding> GetBindingRedirects(IEnumerable<string> assemblyPaths, AppDomain domain)
        {
            if (assemblyPaths == null)
            {
                throw new ArgumentNullException("assemblyPaths");
            }
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            return GetBindingRedirects(GetAssemblies(assemblyPaths, domain));
        }

        public static IEnumerable<AssemblyBinding> GetBindingRedirects(string path, AppDomain domain)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (domain == null)
            {
                throw new ArgumentNullException("domain");
            }
            return GetBindingRedirects(GetAssemblies(path, domain));
        }

        private static Tuple<string, string> GetUniqueKey(IAssembly assembly) => 
            Tuple.Create<string, string>(assembly.Name, assembly.PublicKeyToken);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly BindingRedirectResolver.<>c <>9 = new BindingRedirectResolver.<>c();
            public static Func<IAssembly, AssemblyBinding> <>9__3_0;

            internal AssemblyBinding <GetBindingRedirects>b__3_0(IAssembly a) => 
                new AssemblyBinding(a);
        }

        [CompilerGenerated]
        private sealed class <GetAssemblies>d__6 : IEnumerable<IAssembly>, IEnumerable, IEnumerator<IAssembly>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private IAssembly <>2__current;
            private int <>l__initialThreadId;
            private IEnumerable<string> paths;
            public IEnumerable<string> <>3__paths;
            private AppDomain domain;
            public AppDomain <>3__domain;
            private IEnumerator<string> <>7__wrap1;

            [DebuggerHidden]
            public <GetAssemblies>d__6(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
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
                        this.<>7__wrap1 = this.paths.GetEnumerator();
                        this.<>1__state = -3;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    if (!this.<>7__wrap1.MoveNext())
                    {
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        flag = false;
                    }
                    else
                    {
                        string current = this.<>7__wrap1.Current;
                        this.<>2__current = RemoteAssembly.LoadAssembly(current, this.domain);
                        this.<>1__state = 1;
                        flag = true;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<IAssembly> IEnumerable<IAssembly>.GetEnumerator()
            {
                BindingRedirectResolver.<GetAssemblies>d__6 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new BindingRedirectResolver.<GetAssemblies>d__6(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.paths = this.<>3__paths;
                d__.domain = this.<>3__domain;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<NuGet.Runtime.IAssembly>.GetEnumerator();

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

            IAssembly IEnumerator<IAssembly>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

