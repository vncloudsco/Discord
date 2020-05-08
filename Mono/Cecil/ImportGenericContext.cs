namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ImportGenericContext
    {
        private Collection<IGenericParameterProvider> stack;
        public bool IsEmpty =>
            ReferenceEquals(this.stack, null);
        public ImportGenericContext(IGenericParameterProvider provider)
        {
            this.stack = null;
            this.Push(provider);
        }

        public void Push(IGenericParameterProvider provider)
        {
            if (this.stack != null)
            {
                this.stack.Add(provider);
            }
            else
            {
                Collection<IGenericParameterProvider> collection = new Collection<IGenericParameterProvider>(1) {
                    provider
                };
                this.stack = collection;
            }
        }

        public void Pop()
        {
            this.stack.RemoveAt(this.stack.Count - 1);
        }

        public TypeReference MethodParameter(string method, int position)
        {
            for (int i = this.stack.Count - 1; i >= 0; i--)
            {
                MethodReference reference = this.stack[i] as MethodReference;
                if ((reference != null) && (method == reference.Name))
                {
                    return reference.GenericParameters[position];
                }
            }
            throw new InvalidOperationException();
        }

        public TypeReference TypeParameter(string type, int position)
        {
            for (int i = this.stack.Count - 1; i >= 0; i--)
            {
                TypeReference reference = GenericTypeFor(this.stack[i]);
                if (reference.FullName == type)
                {
                    return reference.GenericParameters[position];
                }
            }
            throw new InvalidOperationException();
        }

        private static TypeReference GenericTypeFor(IGenericParameterProvider context)
        {
            TypeReference reference = context as TypeReference;
            if (reference != null)
            {
                return reference.GetElementType();
            }
            MethodReference reference2 = context as MethodReference;
            if (reference2 == null)
            {
                throw new InvalidOperationException();
            }
            return reference2.DeclaringType.GetElementType();
        }
    }
}

