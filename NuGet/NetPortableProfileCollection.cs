namespace NuGet
{
    using System;
    using System.Collections.ObjectModel;

    internal class NetPortableProfileCollection : KeyedCollection<string, NetPortableProfile>
    {
        public NetPortableProfileCollection() : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        protected override string GetKeyForItem(NetPortableProfile item) => 
            item.Name;
    }
}

