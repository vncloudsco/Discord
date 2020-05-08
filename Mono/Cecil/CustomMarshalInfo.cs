namespace Mono.Cecil
{
    using System;

    internal sealed class CustomMarshalInfo : MarshalInfo
    {
        internal System.Guid guid;
        internal string unmanaged_type;
        internal TypeReference managed_type;
        internal string cookie;

        public CustomMarshalInfo() : base(NativeType.CustomMarshaler)
        {
        }

        public System.Guid Guid
        {
            get => 
                this.guid;
            set => 
                (this.guid = value);
        }

        public string UnmanagedType
        {
            get => 
                this.unmanaged_type;
            set => 
                (this.unmanaged_type = value);
        }

        public TypeReference ManagedType
        {
            get => 
                this.managed_type;
            set => 
                (this.managed_type = value);
        }

        public string Cookie
        {
            get => 
                this.cookie;
            set => 
                (this.cookie = value);
        }
    }
}

