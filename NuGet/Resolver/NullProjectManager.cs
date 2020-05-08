namespace NuGet.Resolver
{
    using NuGet;
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class NullProjectManager : IProjectManager
    {
        private IPackageRepository _localRepository = new VirtualRepository(null);
        private IProjectSystem _project = new NullProjectSystem();
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageReferenceAdded;
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageReferenceAdding;
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageReferenceRemoved;
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageReferenceRemoving;

        public event EventHandler<PackageOperationEventArgs> PackageReferenceAdded
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageReferenceAdded = this.PackageReferenceAdded;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageReferenceAdded;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageReferenceAdded = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceAdded, handler3, a);
                    if (ReferenceEquals(packageReferenceAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageReferenceAdded = this.PackageReferenceAdded;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageReferenceAdded;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageReferenceAdded = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceAdded, handler3, source);
                    if (ReferenceEquals(packageReferenceAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<PackageOperationEventArgs> PackageReferenceAdding
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageReferenceAdding = this.PackageReferenceAdding;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageReferenceAdding;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageReferenceAdding = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceAdding, handler3, a);
                    if (ReferenceEquals(packageReferenceAdding, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageReferenceAdding = this.PackageReferenceAdding;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageReferenceAdding;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageReferenceAdding = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceAdding, handler3, source);
                    if (ReferenceEquals(packageReferenceAdding, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<PackageOperationEventArgs> PackageReferenceRemoved
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageReferenceRemoved = this.PackageReferenceRemoved;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageReferenceRemoved;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageReferenceRemoved = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceRemoved, handler3, a);
                    if (ReferenceEquals(packageReferenceRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageReferenceRemoved = this.PackageReferenceRemoved;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageReferenceRemoved;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageReferenceRemoved = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceRemoved, handler3, source);
                    if (ReferenceEquals(packageReferenceRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<PackageOperationEventArgs> PackageReferenceRemoving
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageReferenceRemoving = this.PackageReferenceRemoving;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageReferenceRemoving;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageReferenceRemoving = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceRemoving, handler3, a);
                    if (ReferenceEquals(packageReferenceRemoving, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageReferenceRemoving = this.PackageReferenceRemoving;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageReferenceRemoving;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageReferenceRemoving = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceRemoving, handler3, source);
                    if (ReferenceEquals(packageReferenceRemoving, source))
                    {
                        return;
                    }
                }
            }
        }

        public NullProjectManager(IPackageManager packageManager)
        {
            this.PackageManager = packageManager;
        }

        public void Execute(PackageOperation operation)
        {
        }

        public IPackageRepository LocalRepository =>
            this._localRepository;

        public IPackageManager PackageManager { get; private set; }

        public ILogger Logger { get; set; }

        public IProjectSystem Project =>
            this._project;

        public IPackageConstraintProvider ConstraintProvider
        {
            get => 
                NullConstraintProvider.Instance;
            set
            {
            }
        }
    }
}

