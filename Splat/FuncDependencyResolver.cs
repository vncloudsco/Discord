namespace Splat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class FuncDependencyResolver : IMutableDependencyResolver, IDependencyResolver, IDisposable
    {
        private readonly Func<Type, string, IEnumerable<object>> innerGetServices;
        private readonly Action<Func<object>, Type, string> innerRegister;
        private readonly Dictionary<Tuple<Type, string>, List<Action<IDisposable>>> _callbackRegistry = new Dictionary<Tuple<Type, string>, List<Action<IDisposable>>>();
        private IDisposable inner;

        public FuncDependencyResolver(Func<Type, string, IEnumerable<object>> getAllServices, Action<Func<object>, Type, string> register = null, IDisposable toDispose = null)
        {
            this.innerGetServices = getAllServices;
            this.innerRegister = register;
            this.inner = toDispose ?? ActionDisposable.Empty;
        }

        public void Dispose()
        {
            Interlocked.Exchange<IDisposable>(ref this.inner, ActionDisposable.Empty).Dispose();
        }

        public object GetService(Type serviceType, string contract = null) => 
            (this.GetServices(serviceType, contract) ?? Enumerable.Empty<object>()).LastOrDefault<object>();

        public IEnumerable<object> GetServices(Type serviceType, string contract = null) => 
            this.innerGetServices(serviceType, contract);

        public void Register(Func<object> factory, Type serviceType, string contract = null)
        {
            if (this.innerRegister == null)
            {
                throw new NotImplementedException();
            }
            this.innerRegister(factory, serviceType, contract);
            Tuple<Type, string> key = Tuple.Create<Type, string>(serviceType, contract ?? string.Empty);
            if (this._callbackRegistry.ContainsKey(key))
            {
                List<Action<IDisposable>> list = null;
                foreach (Action<IDisposable> action in this._callbackRegistry[key])
                {
                    bool remove = false;
                    ActionDisposable disposable = new ActionDisposable(() => remove = true);
                    action(disposable);
                    if (remove)
                    {
                        if (list == null)
                        {
                            list = new List<Action<IDisposable>>();
                        }
                        list.Add(action);
                    }
                }
                if (list != null)
                {
                    foreach (Action<IDisposable> action2 in list)
                    {
                        this._callbackRegistry[key].Remove(action2);
                    }
                }
            }
        }

        public IDisposable ServiceRegistrationCallback(Type serviceType, string contract, Action<IDisposable> callback)
        {
            Tuple<Type, string> pair = Tuple.Create<Type, string>(serviceType, contract ?? string.Empty);
            if (!this._callbackRegistry.ContainsKey(pair))
            {
                this._callbackRegistry[pair] = new List<Action<IDisposable>>();
            }
            this._callbackRegistry[pair].Add(callback);
            return new ActionDisposable(delegate {
                this._callbackRegistry[pair].Remove(callback);
            });
        }
    }
}

