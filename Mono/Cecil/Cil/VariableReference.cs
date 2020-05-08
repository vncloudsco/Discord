namespace Mono.Cecil.Cil
{
    using Mono.Cecil;
    using System;

    internal abstract class VariableReference
    {
        private string name;
        internal int index;
        protected TypeReference variable_type;

        internal VariableReference(TypeReference variable_type) : this(string.Empty, variable_type)
        {
        }

        internal VariableReference(string name, TypeReference variable_type)
        {
            this.index = -1;
            this.name = name;
            this.variable_type = variable_type;
        }

        public abstract VariableDefinition Resolve();
        public override string ToString() => 
            (string.IsNullOrEmpty(this.name) ? ((this.index < 0) ? string.Empty : ("V_" + this.index)) : this.name);

        public string Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }

        public TypeReference VariableType
        {
            get => 
                this.variable_type;
            set => 
                (this.variable_type = value);
        }

        public int Index =>
            this.index;
    }
}

