namespace Splat
{
    using System;

    internal interface ILogManager
    {
        IFullLogger GetLogger(Type type);
    }
}

