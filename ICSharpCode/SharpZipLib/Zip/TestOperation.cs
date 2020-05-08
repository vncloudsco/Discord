namespace ICSharpCode.SharpZipLib.Zip
{
    using System;

    internal enum TestOperation
    {
        Initialising,
        EntryHeader,
        EntryData,
        EntryComplete,
        MiscellaneousTests,
        Complete
    }
}

