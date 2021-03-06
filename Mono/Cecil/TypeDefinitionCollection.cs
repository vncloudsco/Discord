﻿namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using System;
    using System.Collections.Generic;

    internal sealed class TypeDefinitionCollection : Collection<TypeDefinition>
    {
        private readonly ModuleDefinition container;
        private readonly Dictionary<Row<string, string>, TypeDefinition> name_cache;

        internal TypeDefinitionCollection(ModuleDefinition container)
        {
            this.container = container;
            this.name_cache = new Dictionary<Row<string, string>, TypeDefinition>(new RowEqualityComparer());
        }

        internal TypeDefinitionCollection(ModuleDefinition container, int capacity) : base(capacity)
        {
            this.container = container;
            this.name_cache = new Dictionary<Row<string, string>, TypeDefinition>(capacity, new RowEqualityComparer());
        }

        private void Attach(TypeDefinition type)
        {
            if ((type.Module != null) && !ReferenceEquals(type.Module, this.container))
            {
                throw new ArgumentException("Type already attached");
            }
            type.module = this.container;
            type.scope = this.container;
            this.name_cache[new Row<string, string>(type.Namespace, type.Name)] = type;
        }

        private void Detach(TypeDefinition type)
        {
            type.module = null;
            type.scope = null;
            this.name_cache.Remove(new Row<string, string>(type.Namespace, type.Name));
        }

        public TypeDefinition GetType(string fullname)
        {
            string str;
            string str2;
            TypeParser.SplitFullName(fullname, out str, out str2);
            return this.GetType(str, str2);
        }

        public TypeDefinition GetType(string @namespace, string name)
        {
            TypeDefinition definition;
            return (!this.name_cache.TryGetValue(new Row<string, string>(@namespace, name), out definition) ? null : definition);
        }

        protected override void OnAdd(TypeDefinition item, int index)
        {
            this.Attach(item);
        }

        protected override void OnClear()
        {
            foreach (TypeDefinition definition in this)
            {
                this.Detach(definition);
            }
        }

        protected override void OnInsert(TypeDefinition item, int index)
        {
            this.Attach(item);
        }

        protected override void OnRemove(TypeDefinition item, int index)
        {
            this.Detach(item);
        }

        protected override void OnSet(TypeDefinition item, int index)
        {
            this.Attach(item);
        }
    }
}

