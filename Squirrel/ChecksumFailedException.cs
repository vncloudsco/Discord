namespace Squirrel
{
    using System;
    using System.Runtime.CompilerServices;

    internal class ChecksumFailedException : Exception
    {
        public string Filename { get; set; }
    }
}

