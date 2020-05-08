namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class TypeParser
    {
        private readonly string fullname;
        private readonly int length;
        private int position;

        private TypeParser(string fullname)
        {
            this.fullname = fullname;
            this.length = fullname.Length;
        }

        private static void Add<T>(ref T[] array, T item)
        {
            if (array == null)
            {
                T[] localArray = new T[] { item };
                array = localArray;
            }
            else
            {
                array = array.Resize<T>(array.Length + 1);
                array[array.Length - 1] = item;
            }
        }

        private static void AdjustGenericParameters(TypeReference type)
        {
            int num;
            if (TryGetArity(type.Name, out num))
            {
                for (int i = 0; i < num; i++)
                {
                    type.GenericParameters.Add(new GenericParameter(type));
                }
            }
        }

        private static void AppendNamePart(string part, StringBuilder name)
        {
            foreach (char ch in part)
            {
                if (IsDelimiter(ch))
                {
                    name.Append('\\');
                }
                name.Append(ch);
            }
        }

        private static void AppendType(TypeReference type, StringBuilder name, bool fq_name, bool top_level)
        {
            TypeReference declaringType = type.DeclaringType;
            if (declaringType != null)
            {
                AppendType(declaringType, name, false, top_level);
                name.Append('+');
            }
            string str = type.Namespace;
            if (!string.IsNullOrEmpty(str))
            {
                AppendNamePart(str, name);
                name.Append('.');
            }
            AppendNamePart(type.GetElementType().Name, name);
            if (fq_name)
            {
                if (type.IsTypeSpecification())
                {
                    AppendTypeSpecification((TypeSpecification) type, name);
                }
                if (RequiresFullyQualifiedName(type, top_level))
                {
                    name.Append(", ");
                    name.Append(GetScopeFullName(type));
                }
            }
        }

        private static void AppendTypeSpecification(TypeSpecification type, StringBuilder name)
        {
            ArrayType type2;
            if (type.ElementType.IsTypeSpecification())
            {
                AppendTypeSpecification((TypeSpecification) type.ElementType, name);
            }
            ElementType etype = type.etype;
            switch (etype)
            {
                case ElementType.Ptr:
                    name.Append('*');
                    return;

                case ElementType.ByRef:
                    name.Append('&');
                    return;

                case ElementType.ValueType:
                case ElementType.Class:
                case ElementType.Var:
                    break;

                case ElementType.Array:
                    goto TR_0007;

                case ElementType.GenericInst:
                {
                    Collection<TypeReference> genericArguments = ((GenericInstanceType) type).GenericArguments;
                    name.Append('[');
                    int num2 = 0;
                    while (true)
                    {
                        if (num2 >= genericArguments.Count)
                        {
                            name.Append(']');
                            break;
                        }
                        if (num2 > 0)
                        {
                            name.Append(',');
                        }
                        TypeReference reference = genericArguments[num2];
                        bool flag = !ReferenceEquals(reference.Scope, reference.Module);
                        if (flag)
                        {
                            name.Append('[');
                        }
                        AppendType(reference, name, true, false);
                        if (flag)
                        {
                            name.Append(']');
                        }
                        num2++;
                    }
                    break;
                }
                default:
                    if (etype != ElementType.SzArray)
                    {
                        return;
                    }
                    goto TR_0007;
            }
            return;
        TR_0007:
            type2 = (ArrayType) type;
            if (type2.IsVector)
            {
                name.Append("[]");
            }
            else
            {
                name.Append('[');
                for (int i = 1; i < type2.Rank; i++)
                {
                    name.Append(',');
                }
                name.Append(']');
            }
        }

        private static TypeReference CreateReference(Type type_info, ModuleDefinition module, IMetadataScope scope)
        {
            string str;
            string str2;
            SplitFullName(type_info.type_fullname, out str, out str2);
            TypeReference type = new TypeReference(str, str2, module, scope);
            MetadataSystem.TryProcessPrimitiveTypeReference(type);
            AdjustGenericParameters(type);
            string[] self = type_info.nested_names;
            if (!self.IsNullOrEmpty<string>())
            {
                for (int i = 0; i < self.Length; i++)
                {
                    type = new TypeReference(string.Empty, self[i], module, null) {
                        DeclaringType = type
                    };
                    AdjustGenericParameters(type);
                }
            }
            return type;
        }

        private static TypeReference CreateSpecs(TypeReference type, Type type_info)
        {
            type = TryCreateGenericInstanceType(type, type_info);
            int[] specs = type_info.specs;
            if (!specs.IsNullOrEmpty<int>())
            {
                for (int i = 0; i < specs.Length; i++)
                {
                    int num3 = specs[i];
                    switch (num3)
                    {
                        case -3:
                            type = new ArrayType(type);
                            break;

                        case -2:
                            type = new ByReferenceType(type);
                            break;

                        case -1:
                            type = new PointerType(type);
                            break;

                        default:
                        {
                            ArrayType type2 = new ArrayType(type);
                            type2.Dimensions.Clear();
                            int num2 = 0;
                            while (true)
                            {
                                if (num2 >= specs[i])
                                {
                                    type = type2;
                                    break;
                                }
                                ArrayDimension item = new ArrayDimension();
                                type2.Dimensions.Add(item);
                                num2++;
                            }
                            break;
                        }
                    }
                }
            }
            return type;
        }

        private static IMetadataScope GetMetadataScope(ModuleDefinition module, Type type_info) => 
            (!string.IsNullOrEmpty(type_info.assembly) ? MatchReference(module, AssemblyNameReference.Parse(type_info.assembly)) : module.TypeSystem.Corlib);

        private static string GetScopeFullName(TypeReference type)
        {
            IMetadataScope scope = type.Scope;
            switch (scope.MetadataScopeType)
            {
                case MetadataScopeType.AssemblyNameReference:
                    return ((AssemblyNameReference) scope).FullName;

                case MetadataScopeType.ModuleDefinition:
                    return ((ModuleDefinition) scope).Assembly.Name.FullName;
            }
            throw new ArgumentException();
        }

        private static TypeReference GetTypeReference(ModuleDefinition module, Type type_info)
        {
            TypeReference reference;
            if (!TryGetDefinition(module, type_info, out reference))
            {
                reference = CreateReference(type_info, module, GetMetadataScope(module, type_info));
            }
            return CreateSpecs(reference, type_info);
        }

        private static bool IsDelimiter(char chr) => 
            ("+,[]*&".IndexOf(chr) != -1);

        private static AssemblyNameReference MatchReference(ModuleDefinition module, AssemblyNameReference pattern)
        {
            Collection<AssemblyNameReference> assemblyReferences = module.AssemblyReferences;
            for (int i = 0; i < assemblyReferences.Count; i++)
            {
                AssemblyNameReference reference = assemblyReferences[i];
                if (reference.FullName == pattern.FullName)
                {
                    return reference;
                }
            }
            return pattern;
        }

        private string ParseAssemblyName()
        {
            if (!this.TryParse(','))
            {
                return string.Empty;
            }
            this.TryParseWhiteSpace();
            int position = this.position;
            while (true)
            {
                if (this.position < this.length)
                {
                    char ch = this.fullname[this.position];
                    if ((ch != '[') && (ch != ']'))
                    {
                        this.position++;
                        continue;
                    }
                }
                return this.fullname.Substring(position, this.position - position);
            }
        }

        private Type[] ParseGenericArguments(int arity)
        {
            Type[] array = null;
            if ((this.position != this.length) && (this.fullname[this.position] == '['))
            {
                this.TryParse('[');
                for (int i = 0; i < arity; i++)
                {
                    bool flag = this.TryParse('[');
                    Add<Type>(ref array, this.ParseType(flag));
                    if (flag)
                    {
                        this.TryParse(']');
                    }
                    this.TryParse(',');
                    this.TryParseWhiteSpace();
                }
                this.TryParse(']');
            }
            return array;
        }

        private static bool ParseInt32(string value, out int result) => 
            int.TryParse(value, out result);

        private string[] ParseNestedNames()
        {
            string[] array = null;
            while (this.TryParse('+'))
            {
                Add<string>(ref array, this.ParsePart());
            }
            return array;
        }

        private string ParsePart()
        {
            StringBuilder builder = new StringBuilder();
            while ((this.position < this.length) && !IsDelimiter(this.fullname[this.position]))
            {
                int num;
                if (this.fullname[this.position] == '\\')
                {
                    this.position++;
                }
                this.position = (num = this.position) + 1;
                builder.Append(this.fullname[num]);
            }
            return builder.ToString();
        }

        private int[] ParseSpecs()
        {
            int[] array = null;
            while (this.position < this.length)
            {
                char ch = this.fullname[this.position];
                if (ch == '&')
                {
                    this.position++;
                    Add<int>(ref array, -2);
                    continue;
                }
                if (ch == '*')
                {
                    this.position++;
                    Add<int>(ref array, -1);
                    continue;
                }
                if (ch != '[')
                {
                    return array;
                }
                this.position++;
                char ch2 = this.fullname[this.position];
                if (ch2 == '*')
                {
                    this.position++;
                    Add<int>(ref array, 1);
                    continue;
                }
                if (ch2 == ']')
                {
                    this.position++;
                    Add<int>(ref array, -3);
                    continue;
                }
                int item = 1;
                while (true)
                {
                    if (!this.TryParse(','))
                    {
                        Add<int>(ref array, item);
                        this.TryParse(']');
                        break;
                    }
                    item++;
                }
            }
            return array;
        }

        private Type ParseType(bool fq_name)
        {
            Type type = new Type {
                type_fullname = this.ParsePart(),
                nested_names = this.ParseNestedNames()
            };
            if (TryGetArity(type))
            {
                type.generic_arguments = this.ParseGenericArguments(type.arity);
            }
            type.specs = this.ParseSpecs();
            if (fq_name)
            {
                type.assembly = this.ParseAssemblyName();
            }
            return type;
        }

        public static TypeReference ParseType(ModuleDefinition module, string fullname)
        {
            if (string.IsNullOrEmpty(fullname))
            {
                return null;
            }
            TypeParser parser = new TypeParser(fullname);
            return GetTypeReference(module, parser.ParseType(true));
        }

        private static bool RequiresFullyQualifiedName(TypeReference type, bool top_level) => 
            (!ReferenceEquals(type.Scope, type.Module) ? ((type.Scope.Name != "mscorlib") || !top_level) : false);

        public static void SplitFullName(string fullname, out string @namespace, out string name)
        {
            int length = fullname.LastIndexOf('.');
            if (length == -1)
            {
                @namespace = string.Empty;
                name = fullname;
            }
            else
            {
                @namespace = fullname.Substring(0, length);
                name = fullname.Substring(length + 1);
            }
        }

        public static string ToParseable(TypeReference type)
        {
            if (type == null)
            {
                return null;
            }
            StringBuilder name = new StringBuilder();
            AppendType(type, name, true, true);
            return name.ToString();
        }

        private static void TryAddArity(string name, ref int arity)
        {
            int num;
            if (TryGetArity(name, out num))
            {
                arity += num;
            }
        }

        private static TypeReference TryCreateGenericInstanceType(TypeReference type, Type type_info)
        {
            Type[] self = type_info.generic_arguments;
            if (self.IsNullOrEmpty<Type>())
            {
                return type;
            }
            GenericInstanceType type2 = new GenericInstanceType(type);
            Collection<TypeReference> genericArguments = type2.GenericArguments;
            for (int i = 0; i < self.Length; i++)
            {
                genericArguments.Add(GetTypeReference(type.Module, self[i]));
            }
            return type2;
        }

        private static bool TryCurrentModule(ModuleDefinition module, Type type_info) => 
            (!string.IsNullOrEmpty(type_info.assembly) ? ((module.assembly != null) && (module.assembly.Name.FullName == type_info.assembly)) : true);

        private static bool TryGetArity(Type type)
        {
            int arity = 0;
            TryAddArity(type.type_fullname, ref arity);
            string[] self = type.nested_names;
            if (!self.IsNullOrEmpty<string>())
            {
                for (int i = 0; i < self.Length; i++)
                {
                    TryAddArity(self[i], ref arity);
                }
            }
            type.arity = arity;
            return (arity > 0);
        }

        private static bool TryGetArity(string name, out int arity)
        {
            arity = 0;
            int num = name.LastIndexOf('`');
            return ((num != -1) ? ParseInt32(name.Substring(num + 1), out arity) : false);
        }

        private static bool TryGetDefinition(ModuleDefinition module, Type type_info, out TypeReference type)
        {
            type = null;
            if (!TryCurrentModule(module, type_info))
            {
                return false;
            }
            TypeDefinition self = module.GetType(type_info.type_fullname);
            if (self == null)
            {
                return false;
            }
            string[] strArray = type_info.nested_names;
            if (!strArray.IsNullOrEmpty<string>())
            {
                for (int i = 0; i < strArray.Length; i++)
                {
                    self = self.GetNestedType(strArray[i]);
                }
            }
            type = self;
            return true;
        }

        private bool TryParse(char chr)
        {
            if ((this.position >= this.length) || (this.fullname[this.position] != chr))
            {
                return false;
            }
            this.position++;
            return true;
        }

        private void TryParseWhiteSpace()
        {
            while ((this.position < this.length) && char.IsWhiteSpace(this.fullname[this.position]))
            {
                this.position++;
            }
        }

        private class Type
        {
            public const int Ptr = -1;
            public const int ByRef = -2;
            public const int SzArray = -3;
            public string type_fullname;
            public string[] nested_names;
            public int arity;
            public int[] specs;
            public TypeParser.Type[] generic_arguments;
            public string assembly;
        }
    }
}

