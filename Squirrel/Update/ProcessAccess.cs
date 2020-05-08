﻿namespace Squirrel.Update
{
    using System;

    [Flags]
    public enum ProcessAccess
    {
        CreateThread = 2,
        SetSessionId = 4,
        VmOperation = 8,
        VmRead = 0x10,
        VmWrite = 0x20,
        DupHandle = 0x40,
        CreateProcess = 0x80,
        SetQuota = 0x100,
        SetInformation = 0x200,
        QueryInformation = 0x400,
        SuspendResume = 0x800,
        QueryLimitedInformation = 0x1000,
        Synchronize = 0x100000,
        Delete = 0x10000,
        ReadControl = 0x20000,
        WriteDac = 0x40000,
        WriteOwner = 0x80000,
        StandardRightsRequired = 0xf0000,
        AllAccess = 0x1fffff
    }
}

