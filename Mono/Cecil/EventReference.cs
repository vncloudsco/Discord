namespace Mono.Cecil
{
    using System;

    internal abstract class EventReference : MemberReference
    {
        private TypeReference event_type;

        protected EventReference(string name, TypeReference eventType) : base(name)
        {
            if (eventType == null)
            {
                throw new ArgumentNullException("eventType");
            }
            this.event_type = eventType;
        }

        public abstract EventDefinition Resolve();

        public TypeReference EventType
        {
            get => 
                this.event_type;
            set => 
                (this.event_type = value);
        }

        public override string FullName =>
            (this.event_type.FullName + " " + base.MemberFullName());
    }
}

