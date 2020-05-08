namespace NuGet.Resolver
{
    using NuGet;
    using System;
    using System.Runtime.CompilerServices;

    internal class VirtualProjectManager
    {
        public VirtualProjectManager(IProjectManager projectManager)
        {
            this.ProjectManager = projectManager;
            this.LocalRepository = new VirtualRepository(projectManager.LocalRepository);
        }

        public IProjectManager ProjectManager { get; private set; }

        public VirtualRepository LocalRepository { get; private set; }
    }
}

