namespace Squirrel
{
    using NuGet;
    using Splat;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    internal interface IUpdateManager : IDisposable, IEnableLogger
    {
        Task<string> ApplyReleases(UpdateInfo updateInfo, Action<int> progress = null);
        Task<UpdateInfo> CheckForUpdate(bool ignoreDeltaUpdates = false, Action<int> progress = null);
        void CreateShortcutsForExecutable(string exeName, ShortcutLocation locations, bool updateOnly, string programArguments, string icon);
        Task<RegistryKey> CreateUninstallerRegistryEntry();
        Task<RegistryKey> CreateUninstallerRegistryEntry(string uninstallCmd, string quietSwitch);
        SemanticVersion CurrentlyInstalledVersion(string executable = null);
        Task DownloadReleases(IEnumerable<ReleaseEntry> releasesToDownload, Action<int> progress = null);
        Task FullInstall(bool silentInstall, Action<int> progress = null);
        Task FullUninstall();
        void RemoveShortcutsForExecutable(string exeName, ShortcutLocation locations);
        void RemoveUninstallerRegistryEntry();
    }
}

