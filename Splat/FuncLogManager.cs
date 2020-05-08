namespace Splat
{
    using System;

    internal class FuncLogManager : ILogManager
    {
        private readonly Func<Type, IFullLogger> _inner;

        public FuncLogManager(Func<Type, IFullLogger> getLogger)
        {
            this._inner = getLogger;
        }

        public IFullLogger GetLogger(Type type) => 
            this._inner(type);
    }
}

