namespace Mono.Cecil
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class ResolutionException : Exception
    {
        private readonly MemberReference member;

        public ResolutionException(MemberReference member) : base("Failed to resolve " + member.FullName)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }
            this.member = member;
        }

        protected ResolutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MemberReference Member =>
            this.member;

        public IMetadataScope Scope
        {
            get
            {
                TypeReference member = this.member as TypeReference;
                if (member != null)
                {
                    return member.Scope;
                }
                TypeReference declaringType = this.member.DeclaringType;
                if (declaringType == null)
                {
                    throw new NotSupportedException();
                }
                return declaringType.Scope;
            }
        }
    }
}

