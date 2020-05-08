namespace Splat
{
    using System;

    internal static class BitmapLoader
    {
        private static IBitmapLoader _Current = AssemblyFinder.AttemptToLoadType<IBitmapLoader>("Splat.PlatformBitmapLoader");

        public static IBitmapLoader Current
        {
            get
            {
                IBitmapLoader loader = _Current;
                if (loader == null)
                {
                    throw new Exception("Could not find a default bitmap loader. This should never happen, your dependency resolver is broken");
                }
                return loader;
            }
            set => 
                (_Current = value);
        }
    }
}

