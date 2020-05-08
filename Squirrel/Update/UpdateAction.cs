namespace Squirrel.Update
{
    using System;

    internal enum UpdateAction
    {
        Unset,
        Install,
        Uninstall,
        Download,
        Update,
        Releasify,
        Shortcut,
        Deshortcut,
        ProcessStart,
        UpdateSelf,
        CheckForUpdates
    }
}

