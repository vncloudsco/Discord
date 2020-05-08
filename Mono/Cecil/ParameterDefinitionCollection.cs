﻿namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal sealed class ParameterDefinitionCollection : Collection<ParameterDefinition>
    {
        private readonly IMethodSignature method;

        internal ParameterDefinitionCollection(IMethodSignature method)
        {
            this.method = method;
        }

        internal ParameterDefinitionCollection(IMethodSignature method, int capacity) : base(capacity)
        {
            this.method = method;
        }

        protected override void OnAdd(ParameterDefinition item, int index)
        {
            item.method = this.method;
            item.index = index;
        }

        protected override void OnInsert(ParameterDefinition item, int index)
        {
            item.method = this.method;
            item.index = index;
            for (int i = index; i < base.size; i++)
            {
                base.items[i].index = i + 1;
            }
        }

        protected override void OnRemove(ParameterDefinition item, int index)
        {
            item.method = null;
            item.index = -1;
            for (int i = index + 1; i < base.size; i++)
            {
                base.items[i].index = i - 1;
            }
        }

        protected override void OnSet(ParameterDefinition item, int index)
        {
            item.method = this.method;
            item.index = index;
        }
    }
}

