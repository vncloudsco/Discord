namespace Mono.Cecil
{
    using System;

    internal abstract class ParameterReference : IMetadataTokenProvider
    {
        private string name;
        internal int index = -1;
        protected TypeReference parameter_type;
        internal Mono.Cecil.MetadataToken token;

        internal ParameterReference(string name, TypeReference parameterType)
        {
            if (parameterType == null)
            {
                throw new ArgumentNullException("parameterType");
            }
            this.name = name ?? string.Empty;
            this.parameter_type = parameterType;
        }

        public abstract ParameterDefinition Resolve();
        public override string ToString() => 
            this.name;

        public string Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }

        public int Index =>
            this.index;

        public TypeReference ParameterType
        {
            get => 
                this.parameter_type;
            set => 
                (this.parameter_type = value);
        }

        public Mono.Cecil.MetadataToken MetadataToken
        {
            get => 
                this.token;
            set => 
                (this.token = value);
        }
    }
}

