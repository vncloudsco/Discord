namespace Splat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal class ModernDependencyResolver : IMutableDependencyResolver, IDependencyResolver, IDisposable
    {
        private Dictionary<Tuple<Type, string>, List<Func<object>>> _registry;
        private Dictionary<Tuple<Type, string>, List<Action<IDisposable>>> _callbackRegistry;

        public ModernDependencyResolver() : this(null)
        {
        }

        protected ModernDependencyResolver(Dictionary<Tuple<Type, string>, List<Func<object>>> registry)
        {
            Dictionary<Tuple<Type, string>, List<Func<object>>> dictionary1;
            if (registry == null)
            {
                dictionary1 = new Dictionary<Tuple<Type, string>, List<Func<object>>>();
            }
            else
            {
                dictionary1 = Enumerable.ToDictionary<KeyValuePair<Tuple<Type, string>, List<Func<object>>>, Tuple<Type, string>, List<Func<object>>>(registry, k => k.Key, v => v.Value.ToList<Func<object>>());
            }
            this._registry = dictionary1;
            this._callbackRegistry = new Dictionary<Tuple<Type, string>, List<Action<IDisposable>>>();
        }

        public void Dispose()
        {
            this._registry = null;
        }

        public ModernDependencyResolver Duplicate() => 
            new ModernDependencyResolver(this._registry);

        public object GetService(Type serviceType, string contract = null)
        {
            Tuple<Type, string> key = Tuple.Create<Type, string>(serviceType, contract ?? string.Empty);
            return (this._registry.ContainsKey(key) ? this._registry[key].Last<Func<object>>()() : null);
        }

        public IEnumerable<object> GetServices(Type serviceType, string contract = null)
        {
            Tuple<Type, string> key = Tuple.Create<Type, string>(serviceType, contract ?? string.Empty);
            if (!this._registry.ContainsKey(key))
            {
                return Enumerable.Empty<object>();
            }
            return (from x in this._registry[key] select x()).ToList<object>();
        }

        public void Register(Func<object> factory, Type serviceType, string contract = null)
        {
            Tuple<Type, string> key = Tuple.Create<Type, string>(serviceType, contract ?? string.Empty);
            if (!this._registry.ContainsKey(key))
            {
                this._registry[key] = new List<Func<object>>();
            }
            this._registry[key].Add(factory);
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
            ActionDisposable disposable = new ActionDisposable(delegate {
                this._callbackRegistry[pair].Remove(callback);
            });
            if (this._registry.ContainsKey(pair))
            {
                foreach (Func<object> local2 in this._registry[pair])
                {
                    callback(disposable);
                }
            }
            return disposable;
        }
    }
}

