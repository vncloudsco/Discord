namespace Splat
{
    using System;

    internal interface IModeDetector
    {
        bool? InDesignMode();
        bool? InUnitTestRunner();
    }
}

