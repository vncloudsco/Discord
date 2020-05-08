namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal class MemberDefinitionCollection<T> : Collection<T> where T: IMemberDefinition
    {
        private TypeDefinition container;

        internal MemberDefinitionCollection(TypeDefinition container)
        {
            this.container = container;
        }

        internal MemberDefinitionCollection(TypeDefinition container, int capacity) : base(capacity)
        {
            this.container = container;
        }

        private void Attach(T element)
        {
            if (!ReferenceEquals(element.DeclaringType, this.container))
            {
                if (element.DeclaringType != null)
                {
                    throw new ArgumentException("Member already attached");
                }
                element.DeclaringType = this.container;
            }
        }

        private static void Detach(T element)
        {
            element.DeclaringType = null;
        }

        protected override void OnAdd(T item, int index)
        {
            this.Attach(item);
        }

        protected sealed override void OnClear()
        {
            foreach (T local in this)
            {
                MemberDefinitionCollection<T>.Detach(local);
            }
        }

        protected sealed override void OnInsert(T item, int index)
        {
            this.Attach(item);
        }

        protected sealed override void OnRemove(T item, int index)
        {
            MemberDefinitionCollection<T>.Detach(item);
        }

        protected sealed override void OnSet(T item, int index)
        {
            this.Attach(item);
        }
    }
}

